namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core
open Fable.Core.JsInterop
open Shared
open Feliz.Router

type Contact =
    static member Main() =
        Components.MainCard.Main [
            Html.div "Contact Page"
        ]