open FSharp.Data
open System.IO
open System.Text.Json

[<Literal>]
let WaybackMachineJsonExample = """{"url": "https://channel9.msdn.com/Browse/AllContent?page=221", "archived_snapshots": {"closest": {"status": "200", "available": true, "url": "http://web.archive.org/web/20200905130037/https://channel9.msdn.com/Browse/AllContent?page=221", "timestamp": "20200905130037"}}}"""
type WaybackMachineJsonProvider = JsonProvider<WaybackMachineJsonExample>

[<Literal>]
let baseUrl = "https://channel9.msdn.com/Browse/AllContent"
[<Literal>]
let apiBase = "https://archive.org/wayback/available?url="
[<Literal>]
let highestPageNumber = 2811
[<Literal>]
let webArchiveBase = "http://web.archive.org/"

type Channel9VideoInfo = { Name : string; VideoUrl: string; }

let oldPagesToLookUp : string seq = 
    let seqOfPages = 
        seq { for i in 2 .. highestPageNumber -> ( baseUrl + "?page=" + i.ToString() )}
    seqOfPages
    |> Seq.append (seq { baseUrl })
    
let wayBackMachineUrls : string seq =
    oldPagesToLookUp
    |> Seq.map( fun u -> apiBase + u )

let getUrlsFromWayBackMachine : string seq =
    let getAsync (url:string) = 
        async {
            let httpClient = new System.Net.Http.HttpClient()
            let! response = httpClient.GetAsync(url) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content
        }

    let handleSingleResult (url : string) =
        let retrievedResult = Async.RunSynchronously ( getAsync url )
        let parsedResult = WaybackMachineJsonProvider.Parse(retrievedResult)
        parsedResult.ArchivedSnapshots.Closest.Url

    wayBackMachineUrls
    |> Seq.map(handleSingleResult)

(*
let printFirst10ResultsFromWayBackMachine =
    getUrlsFromWayBackMachine
    |> Seq.take 10
    |> Seq.iter(fun r -> printfn "%A" r)
*)

let getAllVideoPageLinksFromUrl (url : string) : string seq =
    let htmlPage = HtmlDocument.Load(url)
    htmlPage.Descendants("article")
    |> Seq.filter(fun x -> x.Elements("a").Length > 0)
    |> Seq.map (fun x -> x.Elements("a").Head.AttributeValue("href"))
    |> Seq.map(fun x -> webArchiveBase + x )

let getChannel9VideoInfoFromUrl (url : string) : Channel9VideoInfo option = 
    try
        printfn "Getting Channel9VideoInfo for %A" url

        let htmlPage = HtmlDocument.Load(url)

        let title : string = 
            htmlPage.Body().AttributeValue("data-episodetitle")


        let url : string = 
            htmlPage.Descendants("main")
            |> Seq.filter(fun x -> x.HasClass "playerContainer")
            |> Seq.filter(fun x -> x.Elements("a").Length > 0)
            |> Seq.map(fun x -> x.Elements("a").Head.AttributeValue("href"))
            |> Seq.head

        Some { Name = title; VideoUrl = url }
    with 
        | :? System.ArgumentException -> 
            printfn "System Argument Exception for %A" url; 
            None

let persistChannel9VideoInfosAsJson (videos: Channel9VideoInfo option list) (path : string) : unit =

    let serializeChannel9VideoInfos (videos : Channel9VideoInfo option list) : string = 
        JsonSerializer.Serialize videos

    File.WriteAllText(path, (serializeChannel9VideoInfos videos))

let getVideosAndPersist (path : string ): unit =
    let channel9Infos : Channel9VideoInfo option list =  
        getUrlsFromWayBackMachine
        |> Seq.map(fun x -> (getAllVideoPageLinksFromUrl x ) |> Seq.map(fun x -> getChannel9VideoInfoFromUrl x))
        |> Seq.concat
        |> Seq.toList
    persistChannel9VideoInfosAsJson channel9Infos path

[<EntryPoint>]
let main argv =
    getVideosAndPersist (Path.Combine(__SOURCE_DIRECTORY__, "output", "Result.json"))
    0 // return an integer exit code