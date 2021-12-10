module JsonToMdConversion 

open Domain
open System.IO
open Newtonsoft.Json

[<Literal>]
let header1 = "| Name | Date Of Release | Author |"
[<Literal>]
let header2 = "| ---- | --- | --- |" 

let convertChannel9InfosToReadMe (infos : Channel9VideoInfo option seq) (path : string) : unit =

    let allChannelInfosAsStrings : string seq = 
        infos 
        |> Seq.filter(fun x -> x.IsSome)
        |> Seq.map(fun x -> $"| [{x.Value.Name.Replace('|', ' ') }]({x.Value.VideoUrl}) | {x.Value.DateOfRelease} | {x.Value.Author} |") 

    let seqToPersist : string seq =
        allChannelInfosAsStrings
        |> Seq.append (seq { header1; header2 })

    File.WriteAllLines(path, seqToPersist)


let convertJsonToReadMe (pathOfJson : string) (outputPath : string): unit =
    // Read in file 
    let text = File.ReadAllText(pathOfJson)
    let allChannelInfos = JsonConvert.DeserializeObject<Channel9VideoInfo option seq> text

    convertChannel9InfosToReadMe allChannelInfos outputPath  