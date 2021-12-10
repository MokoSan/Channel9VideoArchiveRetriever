module JsonToMdConversion 

open DSL
open System.IO
open Newtonsoft.Json

let convertJsonToReadMe (pathOfJson : string) (outputPath : string): unit =
    // Read in file 
    let text = File.ReadAllText(pathOfJson)
    let allChannelInfos = JsonConvert.DeserializeObject<Channel9VideoInfo option seq> text

    let header1 = "| Name | Date Of Release | Author |"
    let header2 = "| ---- | --- | --- |" 

    let allChannelInfosAsStrings : string seq = 
        allChannelInfos
        |> Seq.filter(fun x -> x.IsSome)
        |> Seq.map(fun x -> $"| [{x.Value.Name.Replace('|', ' ') }]({x.Value.VideoUrl}) | {x.Value.DateOfRelease} | {x.Value.Author} |") 

    let listToPersist : string seq =
        allChannelInfosAsStrings
        |> Seq.append (seq { header1; header2 })

    File.WriteAllLines(outputPath, listToPersist)