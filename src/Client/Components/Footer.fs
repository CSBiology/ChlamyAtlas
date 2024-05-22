namespace Components

open Feliz
open Feliz.DaisyUI
open Feliz.Router

type Footer =

    static member private InPageLink(page: Routing.Pages) =
        Daisy.link [
            link.hover
            prop.href (page.ToRoute() |> Router.format)
            prop.text (page.ToStringRdb())
        ]

    static member Main(model: App.Model) =
        Daisy.footer [
            prop.className "px-10 py-5 bg-neutral/50 text-neutral-content"
            prop.children [
                Html.nav [
                    Daisy.footerTitle "Versions"
                    Html.span [
                        Html.text "UI-Version: "
                        match model.Version.UIVersion with
                        | Some version -> Html.b version
                        | None -> Html.span "connecting .."
                    ]
                    Html.span [
                        Html.text "ML-Version: "
                        match model.Version.MLVersion with
                        | Some version -> Html.b version
                        | None -> Html.span "connecting .."
                    ]
                ]
                Html.nav [
                    Daisy.footerTitle "Services"
                    Footer.InPageLink(Routing.Main)
                    Footer.InPageLink(Routing.DataAccess)
                ]
                Html.nav [
                    Daisy.footerTitle "Info"
                    Html.div [
                        prop.className "grid grid-cols-2 gap-2"
                        prop.children [
                            Footer.InPageLink(Routing.About)
                            Footer.InPageLink(Routing.PrivacyPolicy)
                            Footer.InPageLink(Routing.Contact)
                        ]
                    ]
                ]
            ]
        ]

