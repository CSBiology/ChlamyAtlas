namespace Shared

open System

module Urls =

    [<LiteralAttribute>]
    let WebsiteUrl = "TODO"

    [<Literal>]
    let ContactEmailObfuscated = // obfuscated by https://www.email-obfuscator.com
         "mailto:\u0074\u0069\u006d\u006f\u002e\u006d\u0075\u0065\u0068\u006c\u0068\u0061\u0075\u0073\u0040\u0072\u0070\u0074\u0075\u002e\u0064\u0065"

    [<Literal>]
    let GitHubRepo = "https://github.com/CSBiology/ChlamyAtlas"

    [<Literal>]
    let REFPaperAtlas = @"https://www.sciencedirect.com/science/article/pii/S0092867423006761?via%3Dihub"

    [<Literal>]
    let BioRPTU = @"https://bio.rptu.de"

    [<Literal>]
    let CSBiology = "https://csbiology.github.io"

    module ProteomIQon =
        
        [<Literal>]
        let Zenodo = @"https://zenodo.org/records/6482417"

        [<Literal>]
        let GitHub = @"https://github.com/CSBiology/ProteomIQon"

module EndPoints =
    [<Literal>]
    let endpoint = "/bridge"

    [<Literal>]
    let siteDomain = "localhost:5000"

    [<Literal>]
    let siteUrl = "http://" + siteDomain

    [<Literal>]
    let clientEndpoint = "ws://" + siteDomain + endpoint

    [<Literal>]
    let fastApiBrideEndpoint = "ws://localhost:8000/dataml"
    let fastApiBrideEndpointURI = Uri(fastApiBrideEndpoint)

module Route =
    let clientBuilder typeName methodName =
        sprintf "%s/api/%s/%s" EndPoints.siteUrl typeName methodName

    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type HelloWorld = {
    message: string
}

type IAppApiv1 = {
    GetVersion: unit -> Async<string>
    Log: unit -> Async<unit>
}

type IPredictionApiv1 = {
    StartEvaluation: DataInput -> Async<Guid>
    PutEmail: Guid * string -> Async<unit>
    GetStatus: Guid -> Async<DataResponseStatus>
    GetData: Guid -> Async<DataResponseDTO>
    PutConfig: DataInputConfig -> Async<Guid>
    ValidateData: string -> Async<Result<DataInputItem [], exn>>
}

type ILargeFileApiv1 = {
    UploadLargeFile: byte [] -> Async<unit>
}