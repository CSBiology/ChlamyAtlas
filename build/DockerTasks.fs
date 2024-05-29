module DockerTasks

open BlackFox.Fake
open ProjectInfo
open Helpers
open Fake.Core
open Fake.IO

[<LiteralAttribute>]
let dockerfile_api_path = "build/Dockerfile.GunicornApi"
[<LiteralAttribute>]
let dockerfile_ui_path = "build/Dockerfile.UI"

[<LiteralAttribute>]
let ImageName_api = "chlamyatlas-api"
[<LiteralAttribute>]
let ImageName_ui = "chlamyatlas-ui"

[<LiteralAttribute>]
let ImageName_api_remote = "csbdocker/" + ImageName_api
[<LiteralAttribute>]
let ImageName_ui_remote = "csbdocker/" + ImageName_ui

let uiOnly (config: TargetParameter)=
    let args = config.Context.Arguments
    args
    |> (List.map String.toLower >> List.contains "--uionly")

let dockerBundle = BuildTask.createFn "DockerBundle" [] (fun config ->
    let uiOnly = uiOnly config
    //let release = release
    if not uiOnly then
        Trace.traceImportant $"Start building {ImageName_api} image."
        run
            docker
            [
                "build"; "-t";
                $"{ImageName_api}:new";
                "-f"; dockerfile_api_path; "."
            ]
            ""
    Trace.traceImportant $"Start building {ImageName_ui} image."
    // docker build -t deepstabp-ui -f build/Dockerfile.UI .
    run docker ["build"; "-t"; $"{ImageName_ui}:new"; "-f"; dockerfile_ui_path; "."] ""
)

let dockerTest = BuildTask.createFn "DockerTest" [] (fun config ->
    let dockerComposeNewPath = Path.getFullName @"build\chlamyAtlas.local.yml"
    run dockerCompose ["-f"; dockerComposeNewPath; "up"] __SOURCE_DIRECTORY__
)

/// Must login into "csbdocker" docker account
let dockerPublish = BuildTask.createFn "DockerPublish" [] (fun config ->
    let uiVersion = Versions.getUIVersion()
    let mlVersion = Versions.getMLVersion()
    Trace.traceImportant $"Versions found: UI: {uiVersion}, ML: {mlVersion}"
    let check = Fake.Core.UserInput.getUserInput($"Do you want to publish to docker-hub? (y/yes/true)" )
    match check.ToLower() with
    | "y" | "yes" | "true" ->
        let dockerTagImage() =
            // tag api
            run docker ["tag"; $"{ImageName_api}:new"; $"{ImageName_api_remote}:{mlVersion}"] ""
            run docker ["tag"; $"{ImageName_api}:new"; $"{ImageName_api_remote}:latest"] ""
            // tag ui
            run docker ["tag"; $"{ImageName_ui}:new"; $"{ImageName_ui_remote}:{uiVersion}"] ""
            run docker ["tag"; $"{ImageName_ui}:new"; $"{ImageName_ui_remote}:latest"] ""
        let dockerPushImage() =
            // push api
            run docker ["push"; $"{ImageName_api_remote}:{mlVersion}"] ""
            run docker ["push"; $"{ImageName_api_remote}:latest"] ""
            // push ui
            run docker ["push"; $"{ImageName_ui_remote}:{uiVersion}"] ""
            run docker ["push"; $"{ImageName_ui_remote}:latest"] ""
        Trace.traceImportant $"Tagging image with :latest and ui:{uiVersion}/api:{mlVersion}"
        dockerTagImage()
        Trace.traceImportant $"Pushing image to dockerhub with :latest and ui:{uiVersion}/api:{mlVersion}"
        dockerPushImage()
        Trace.trace "Done!"
    | _ ->
        Trace.trace "Exit DockerPublish task."
)

[<Literal>]
let private dockerCompose_production = "build/chlamyAtlas.yml"

let dockerTestProduction = BuildTask.createFn "DockerTestProduction" [] (fun config ->
    run dockerCompose ["-f"; dockerCompose_production; "up"; "--pull"; "always"] ""
)