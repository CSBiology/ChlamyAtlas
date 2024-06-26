namespace Components

open Fable.Core
open Feliz
open Feliz.DaisyUI

type MainCard =
    static member Main(children: ReactElement list) =
        Html.div [
            prop.id "main-predictor"
            prop.className "flex flex-grow p-[0.5] sm:p-10 justify-center flex-col items-center"
            prop.children [
                Daisy.card [
                    prop.className [
                        // responsive-w
                        "md:w-[70%]"
                        "lg:w-[50%]"
                        "xl:w-max"
                        "xl:max-w-[50%]"
                        "xl:min-w-[600px]"
                        // responsive-h
                        "max-sm:min-h-[75vh]"
                        "min-h-[100%]"
                        //styling
                        "bg-white/[.95]";
                        "text-black"
                        "h-fit"
                        "w-[100%]"
                        "max-sm:rounded-none"
                    ]
                    card.bordered
                    prop.children [
                        Daisy.cardBody [
                            prop.className "h-full w-full flex flex-col"
                            prop.children children
                        ]
                    ]
                ]
            ]
        ]

