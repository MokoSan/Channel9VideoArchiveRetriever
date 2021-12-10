module UrlLogic

open FSharp.Data
open System.IO
open System.Text.Json

[<Literal>]
let webArchiveBase = "http://web.archive.org/"

[<Literal>]
let WaybackMachineJsonExample = """{"url": "https://channel9.msdn.com/Browse/AllContent?page=221", "archived_snapshots": {"closest": {"status": "200", "available": true, "url": "http://web.archive.org/web/20200905130037/https://channel9.msdn.com/Browse/AllContent?page=221", "timestamp": "20200905130037"}}}"""
type WaybackMachineJsonProvider = JsonProvider<WaybackMachineJsonExample>

[<Literal>]
let baseUrl = "https://channel9.msdn.com/Browse/AllContent"
[<Literal>]
let apiBase = "https://archive.org/wayback/available?url="
[<Literal>]
let highestPageNumber = 2690 

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
    printfn "    Getting Video Pages for URL: %A" url 
    let htmlPage = HtmlDocument.Load(url)
    htmlPage.Descendants("article")
    |> Seq.filter(fun x -> x.Elements("a").Length > 0)
    |> Seq.map (fun x -> x.Elements("a").Head.AttributeValue("href"))
    |> Seq.map(fun x -> webArchiveBase + x )

let getAllVideoPageLinksFromAllUrls (urls : string seq) : string seq =
    urls
    |> Seq.take 200 // TODO: Remove this.
    |> Seq.map(getAllVideoPageLinksFromUrl)
    |> Seq.concat

let getAllUrlsFromFile (path : string) : string seq =
    let allLinks = File.ReadAllText path
    let deserializedJson : string seq = JsonSerializer.Deserialize allLinks
    deserializedJson