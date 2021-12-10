open System.IO
open System.Text.Json

open UrlLogic
open PageLogic
open Persistence
open JsonToMdConversion
open Domain 
open Mine

let getChannel9VideoInfoFromPath (path : string) : Channel9VideoInfo option seq =
    getAllUrlsFromFile path 
    |> Seq.map(getAllVideoPageLinksFromUrl)
    |> Seq.concat
    |> Seq.map(getChannel9VideoInfoFromPageUrl) 

[<EntryPoint>]
let main argv =
    let allFiles = 
        seq { 
            Path.Combine(__SOURCE_DIRECTORY__, "output", "Pages", "Urls_1001Thru2000.json") 
            Path.Combine(__SOURCE_DIRECTORY__, "output", "Pages", "Urls_1Thru300.json") 
        }

    let keyword = "Async"
    let outputFile = Path.Combine(__SOURCE_DIRECTORY__, "output", "Mined", $"{keyword}.md")
    mineForKeyword keyword allFiles outputFile

    0 // return an integer exit code