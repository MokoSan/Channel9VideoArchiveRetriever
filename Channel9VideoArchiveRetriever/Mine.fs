module Mine

open Newtonsoft.Json
open Domain 
open System.IO
open JsonToMdConversion

let mineForKeyword (keyword: string) (inputPaths: string seq) (outputPath: string) : unit = 
    let readAndDeserializeFromPath (path : string) : Channel9VideoInfo option seq =
        let serializedJson = File.ReadAllText path
        let deserializedJson : Channel9VideoInfo option seq = JsonConvert.DeserializeObject<Channel9VideoInfo option seq> serializedJson
        deserializedJson

    let channel9VideoInfos : Channel9VideoInfo option seq = 
        inputPaths
        |> Seq.map(readAndDeserializeFromPath)
        |> Seq.concat
        |> Seq.filter(fun x -> x.IsSome && x.Value.Name.Contains(keyword))

    convertChannel9InfosToReadMe channel9VideoInfos outputPath