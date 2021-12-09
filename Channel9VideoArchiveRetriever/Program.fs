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
    let httpClient = new System.Net.Http.HttpClient()

    let getAsync (url:string) = 
        async {
            let! response = httpClient.GetAsync(url) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content
        }

    let handleSingleResult (url : string) : string =
        try
            printfn "Invoking the Wayback Machine API for: %A" url
            let retrievedResult = Async.RunSynchronously ( getAsync url )
            let parsedResult = WaybackMachineJsonProvider.Parse(retrievedResult)
            parsedResult.ArchivedSnapshots.Closest.Url
        with
            | :? System.Exception ->
                printfn "Url Invoke Failed for %A" url
                ""

    wayBackMachineUrls
    |> Seq.map(handleSingleResult)

let getAllVideoPageLinksFromUrl (url : string) : string seq =
    let htmlPage = HtmlDocument.Load(url)
    htmlPage.Descendants("article")
    |> Seq.filter(fun x -> x.Elements("a").Length > 0)
    |> Seq.map (fun x -> x.Elements("a").Head.AttributeValue("href"))
    |> Seq.map(fun x -> webArchiveBase + x )

let getChannel9VideoInfoFromUrl (url : string) : Channel9VideoInfo option = 
    try
        let htmlPage = HtmlDocument.Load(url)
        printfn "Getting Channel9 Video Info for: %A" url

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
            printfn "System.ArgumentException for %A" url; 
            None
        | :? System.Net.WebException -> 
            printfn "System.Net.WebException for %A" url; 
            None

let persistChannel9VideoInfosAsJson (videos: Channel9VideoInfo option list) (path : string) : unit =

    let serializeChannel9VideoInfos (videos : Channel9VideoInfo option list) : string = 
        JsonSerializer.Serialize videos

    File.WriteAllText(path, (serializeChannel9VideoInfos videos))

let getVideosAndPersist (path : string) : unit =
    let channel9Infos : Channel9VideoInfo option list =  
        getUrlsFromWayBackMachine
        |> Seq.map(fun x -> (getAllVideoPageLinksFromUrl x ) |> Seq.map(fun x -> getChannel9VideoInfoFromUrl x))
        |> Seq.concat
        |> Seq.toList
    persistChannel9VideoInfosAsJson channel9Infos path

let convertJsonToReadMe (pathOfJson : string) (outputPath : string): unit =
    // Read in file 
    let text = File.ReadAllText(pathOfJson)
    let allChannelInfos : Channel9VideoInfo option list = JsonSerializer.Deserialize text

    printfn "%A" allChannelInfos

    let header1 = "| Name | Url |"
    let header2 = "| :---: | :---: |" 

    let allChannelInfosAsStrings = 
        allChannelInfos
        |> List.filter(fun x -> x.IsSome)
        |> List.map(fun x -> $"| {x.Value.Name} | {x.Value.VideoUrl} |") 

    let listToPersist : string list =
        [ header1; header2 ]
        |> List.append allChannelInfosAsStrings

    File.WriteAllLines(outputPath, listToPersist)

[<EntryPoint>]
let main argv =
    let listOfUrls : string = 
        let allUrls = 
            getUrlsFromWayBackMachine
            |> Seq.toList
        JsonSerializer.Serialize allUrls
    File.WriteAllText(Path.Combine(__SOURCE_DIRECTORY__, "output", "Urls_2001ThruHighestPageNumber.json"), listOfUrls)

    (*
    let testJson =  Path.Combine(__SOURCE_DIRECTORY__, "output", "Test.json")
    let output = Path.Combine(__SOURCE_DIRECTORY__, "output", "Test.md")
    convertJsonToReadMe testJson output
    *)
    
    0 // return an integer exit code