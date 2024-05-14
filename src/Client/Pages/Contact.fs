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
            Html.article [
                prop.className "prose"
                prop.children [
                    Html.h1 "Contact"

                    Html.p "This service is developed and maintained by the Computational Systems Biology, RPTU University of Kaiserslautern, 67663 Kaiserslautern, Germany."

                    Html.p [
                        Html.text "Contact us via "
                        Components.EmailLaunchAnchor.Main("email", Shared.Urls.ContactEmailObfuscated)
                        Html.text " or visit the open source "
                        Html.a [prop.href Shared.Urls.GitHubRepo; prop.text "GitHub repository"]
                        Html.text " of this service!"
                    ]
                ]
            ]
        ]