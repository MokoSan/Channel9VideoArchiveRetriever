open System.IO
open System.Text.Json

open UrlLogic
open PageLogic
open Persistence
open JsonToMdConversion
open DSL 

let getChannel9VideoInfoFromPath (path : string) : Channel9VideoInfo option seq =
    getAllUrlsFromFile path 
    |> Seq.map(getAllVideoPageLinksFromUrl)
    |> Seq.concat
    |> Seq.map(getChannel9VideoInfoFromPageUrl) 

[<EntryPoint>]
let main argv =
    let testUrlPath = Path.Combine(__SOURCE_DIRECTORY__, "output", "Urls", "TestUrls.json")
    let outputPath = Path.Combine(__SOURCE_DIRECTORY__, "output", "Pages", "TestPages.json")
    let mdPath = Path.Combine(__SOURCE_DIRECTORY__, "output", "Pages", "TestPages.md")
    //let channelInfoSeq = getChannel9VideoInfoFromPath testUrlPath
    //persistChannel9VideoInfosAsJson channelInfoSeq outputPath |> ignore
    convertJsonToReadMe outputPath mdPath  |> ignore

    0 // return an integer exit code