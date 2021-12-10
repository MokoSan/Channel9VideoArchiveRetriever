open System.IO
open System.Text.Json

open UrlLogic
open PageLogic
open Persistence
open JsonToMdConversion
open Domain 

let getChannel9VideoInfoFromPath (path : string) : Channel9VideoInfo option seq =
    getAllUrlsFromFile path 
    |> Seq.map(getAllVideoPageLinksFromUrl)
    |> Seq.concat
    |> Seq.map(getChannel9VideoInfoFromPageUrl) 

[<EntryPoint>]
let main argv =
    let testUrlPath = Path.Combine(__SOURCE_DIRECTORY__, "output", "Urls", "Urls_301Thru1000.json")
    let outputPath = Path.Combine(__SOURCE_DIRECTORY__, "output", "Pages", "Urls_301Thru1000.json")
    let mdPath = Path.Combine(__SOURCE_DIRECTORY__, "output", "Pages", "Urls_301Thru1000.md")
    let channelInfoSeq = getChannel9VideoInfoFromPath testUrlPath
    persistChannel9VideoInfosAsJson channelInfoSeq outputPath |> ignore
    convertJsonToReadMe outputPath mdPath  |> ignore

    0 // return an integer exit code