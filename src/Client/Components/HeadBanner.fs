namespace Components


open Feliz
open Feliz.DaisyUI

type HeadBanner =

    static member Main() =
        let rptu_logo = StaticFile.import("../img/rptu_web_logo_schwarz.svg")
        Html.nav [
            prop.className "bg-white px-6 py-3"
            prop.children [
                Html.a [
                    prop.href Shared.Urls.RPTU
                    prop.target.blank
                    prop.children [
                        Html.img [
                            prop.className "cursor-pointer";
                            prop.src rptu_logo;
                            prop.style [style.width 192; style.height 74]
                        ]
                    ]
                ]
                Daisy.button.a [
                    button.link 
                    prop.className "p-0 no-underline hover:underline"
                    prop.href Shared.Urls.BioRPTU
                    prop.target.blank
                    prop.children [
                        Html.h2 [
                            prop.className "text-lg font-bold text-black"
                            prop.text "Department of Biology"
                        ]
                    ]
                ]
            ]
        ]