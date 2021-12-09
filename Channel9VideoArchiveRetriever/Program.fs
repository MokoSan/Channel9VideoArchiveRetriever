// Learn more about F# at http://fsharp.org

open FSharp.Data

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

type Channel9VideoDetail = { Name : string; VideoUrl: string; Description: string }

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

let printFirst10ResultsFromWayBackMachine =
    getUrlsFromWayBackMachine
    |> Seq.take 10
    |> Seq.iter(fun r -> printfn "%A" r)

let getAllVideoPageLinksFromUrl (url : string) : string seq =
    let htmlPage = HtmlDocument.Load(url)
    htmlPage.Descendants("article")
    |> Seq.filter(fun x -> x.Elements("a").Length > 0)
    |> Seq.map (fun x -> x.Elements("a").Head.AttributeValue("href"))
    |> Seq.map(fun x -> webArchiveBase + x )

[<EntryPoint>]
let main argv =
    let first = 
        getUrlsFromWayBackMachine
        |> Seq.take 1 
        |> Seq.head

    printfn "First: %A" first

    getUrlsFromWayBackMachine
    |> Seq.take 1
    |> Seq.iter(fun y -> (getAllVideoPageLinksFromUrl y )|> Seq.iter(fun x -> printfn "%A" x ))

    0 // return an integer exit code