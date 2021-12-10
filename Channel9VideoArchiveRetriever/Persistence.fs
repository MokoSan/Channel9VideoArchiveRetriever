module Persistence

open System.IO
open System.Text.Json
open Newtonsoft.Json
open Domain 

let BasePath = Path.Combine(__SOURCE_DIRECTORY__, "output", "Pages")

let persistChannel9VideoInfosAsJson (videos: Channel9VideoInfo option seq) (path : string) : unit =
    let serializeChannel9VideoInfos (videos : Channel9VideoInfo option seq) : string = 
        JsonConvert.SerializeObject videos
    File.WriteAllText(path, (serializeChannel9VideoInfos videos))

let persistUrls (urls : string seq) (path: string) : unit =
    let serializedJson : string = JsonConvert.SerializeObject urls
    File.WriteAllText(path, serializedJson)