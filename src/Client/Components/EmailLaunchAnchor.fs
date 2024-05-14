namespace Components

open Fable.Core
open Feliz

type EmailLaunchAnchor =
    static member Main (name: string, email: string) =
        Html.a [
            prop.onClick (fun e ->
                e.preventDefault()
                Browser.Dom.window.location.href <- email;
            )
            prop.href ""
            prop.text name
        ]

