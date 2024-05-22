module App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Feliz
open App


let init () =
    let model = Model.init()
    let cmd = Cmd.ofMsg GetVersions
    model, cmd

let update (msg: Msg) (model: Model) =
    match msg with
    | GetVersions ->
        let cmd =
            Cmd.OfAsync.perform
                Api.appApi.GetVersions
                ()
                GetVersionsResponse
        model, cmd
    | GetVersionsResponse versions -> {model with Version = versions}, Cmd.none

[<ReactComponent>]
let private Main (model) dispatch =
    let modalContainer = React.useElementRef()
    React.strictMode [
        React.contextProvider(ReactContext.modalContext, modalContainer, React.fragment [
            Html.div [
                prop.id "modal-container"
                prop.ref modalContainer
            ]
            App.View.Main model dispatch
        ])
    ]

importSideEffects "./index.css"

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update Main
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run