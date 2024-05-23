module Versions

open Fake.IO
open System
open System.Text.RegularExpressions
open BlackFox.Fake
open Fake.Core

let mlRegexPattern = """^predictor_version = "(?<version>\d+\.\d+\.\d+)"$"""
let uiRegexPattern = """^let AppVersion = "(?<version>\d+\.\d+\.\d+)"$"""

let private getVersion(pattern, path) =
    let path = Path.getFullName(path)
    let file = File.read(path)
    let regex = new Regex(pattern)
    file
    |> Seq.find (fun line -> regex.IsMatch(line))
    |> regex.Match
    |> fun m -> m.Groups.["version"].Value

let getUIVersion() =
    getVersion(uiRegexPattern, ProjectInfo.uiVersionPath)

let getMLVersion() =
    getVersion(mlRegexPattern, ProjectInfo.mlVersionPath)


let versions = BuildTask.createFn "versions" [] (fun config ->
    Trace.logfn "UI Version: %s" (getUIVersion())
    Trace.logfn "ML Version: %s" (getMLVersion())
)