module PageLogic

open Domain 
open FSharp.Data
open System.IO
open System.Text.Json

let getChannel9VideoInfoFromPageUrl (url : string) : Channel9VideoInfo option = 
    try
        let htmlPage = HtmlDocument.Load(url)
        printfn "Getting Channel9 Video Info for: %A" url

        let title : string = 
            htmlPage.Body().AttributeValue("data-episodetitle")

        let url : string = 
            htmlPage.Descendants("main")
            |> Seq.filter(fun x -> x.HasClass "playerContainer")
            |> Seq.filter(fun x -> x.Elements("a").Length > 0)
            |> Seq.map(fun x -> x.Elements("a").Head.AttributeValue("href"))
            |> Seq.head

        let dateOfRelease = 
            htmlPage.Descendants("div")
            |> Seq.filter(fun x -> x.HasClass("releaseDate"))
            |> Seq.map(fun x -> x.InnerText())
            |> Seq.head

        let author =
            htmlPage.Descendants("div")
            |> Seq.filter(fun x -> x.HasClass("authors"))
            |> Seq.filter(fun x -> x.Elements("a").Length > 0)
            |> Seq.map(fun x -> x.Elements("a").Head.InnerText())
            |> Seq.head
            
        Some { Name          = title; 
               VideoUrl      = url; 
               DateOfRelease = dateOfRelease; 
               Author        = author; 
               OriginalUrl   = url; }
    with 
        | :? System.ArgumentException as e -> 
            printfn "System.ArgumentException for %A - %A" url e; 
            None
        | :? System.Net.WebException as e -> 
            printfn "System.Net.WebException for %A - %A" url e; 
            None
        | :? System.Exception as e ->
            printfn "System.Exception for %A - %A" url e; 
            None

let getChannel9VideoInfoFromUrls (urls : string seq) (parentUrl: string): Channel9VideoInfo option seq  =
    urls
    |> Seq.map(getChannel9VideoInfoFromPageUrl)