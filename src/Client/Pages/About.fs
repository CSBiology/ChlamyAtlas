namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core
open Fable.Core.JsInterop
open Shared
open Feliz.Router

type About =
    static member Main() =
        Components.MainCard.Main [
            Html.div [
                prop.className "prose"
                prop.children [
                    Html.div "About"
                ]
            ]
        ]