// Learn more about F# at http://fsharp.org

open System
open FSharp.Data

[<Literal>]
let baseUrl = "https://channel9.msdn.com/Browse/AllContent"
[<Literal>]
let apiBase = "https://archive.org/wayback/available?url="
[<Literal>]
let highestPageNumber = 2811

let oldPagesToLookUp : string seq = 
    seq { for i in 0 .. highestPageNumber -> ( baseUrl + "?page=" + i.ToString() )}

let wayBackMachineUrls : string seq =
    oldPagesToLookUp
    |> Seq.map( fun u -> apiBase + u )

let getAsync (url:string) = 
    async {
        let httpClient = new System.Net.Http.HttpClient()
        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

let getResultFromWayBackMachine : string seq=
    wayBackMachineUrls
    |> Seq.map(fun s -> Async.RunSynchronously ( getAsync s ))

let printFirst10 =
    getResultFromWayBackMachine
    |> Seq.take 10
    |> Seq.iter(fun r -> printfn "%A" r)

[<EntryPoint>]
let main argv =
    printFirst10
    0 // return an integer exit code