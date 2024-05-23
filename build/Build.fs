open Fake.Core
open Fake.IO

open Helpers

initializeContext ()

let _ = DockerTasks.dockerBundle
let _ = DockerTasks.dockerTest
let _ = DockerTasks.dockerPublish
let _ = DockerTasks.dockerTestProduction
let _ = Versions.versions

open ProjectInfo

Target.create "Clean" (fun _ ->
    Shell.cleanDir deployPath
    run dotnet [ "fable"; "clean"; "--yes" ] clientPath // Delete *.fs.js files created by Fable
)

Target.create "InstallClient" (fun _ -> run npm [ "install" ] ".")

Target.create "InstallApi" (fun _ -> run pip ["install"; "--no-cache-dir"; "--upgrade"; "-r"; "requirements.txt"] fastApiPath)

Target.create "Install" (fun _ -> ())

Target.create "Bundle" (fun _ ->
    [
        "server", dotnet [ "publish"; "-c"; "Release"; "-o"; deployPath ] serverPath
        "client", dotnet [ "fable"; "-o"; "output"; "-s"; "--run"; "npx"; "vite"; "build" ] clientPath
    ]
    |> runParallel)

Target.create "Run" (fun e -> 
    run dotnet [ "build" ] sharedPath
    [
        "server", dotnet [ "watch"; "run" ] serverPath
        "client", dotnet [ "fable"; "watch"; "-o"; "output"; "-s"; "--run"; "npx"; "vite"; ] clientPath
    ]
    |> runParallel)

Target.create "Host" (fun e ->
    run dotnet [ "build" ] sharedPath
    [
        "server", dotnet [ "watch"; "run" ] serverPath
        "client", dotnet [ "fable"; "watch"; "-o"; "output"; "-s"; "--run"; "npx"; "vite"; "--host" ] clientPath
    ]
    |> runParallel)

Target.create "fastapi" (fun _ ->
    run uvicorn ["app.main:app"; "--reload"] fastApiPath
)

Target.create "RunTests" (fun _ ->
    run dotnet [ "build" ] sharedTestsPath

    [
        "server", dotnet [ "watch"; "run" ] serverTestsPath
        "client", dotnet [ "fable"; "watch"; "-o"; "output"; "-s"; "--run"; "npx"; "vite" ] clientTestsPath
    ]
    |> runParallel)

Target.create "Format" (fun _ -> run dotnet [ "fantomas"; "." ] ".")

open Fake.Core.TargetOperators

let dependencies = [

    "Clean" ==> "InstallClient" ==> "InstallApi" ==> "Install"

    "Clean" ==> "Run"
    "Clean" ==> "Host"

    "InstallClient" ==> "RunTests"

    // Without Bundle before DockerBundle it will not work
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"
        ==> "DockerBundle"
]

[<EntryPoint>]
let main args = runOrDefault args