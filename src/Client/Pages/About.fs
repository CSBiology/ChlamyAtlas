namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core
open Fable.Core.JsInterop
open Shared
open Feliz.Router

type About =

    [<ReactComponent>]
    static member private PaperReF(citationContainer: IRefValue<Browser.Types.HTMLElement option>, name: string, href, number: int) =
        let listElement =
            Html.li [
                prop.value number
                prop.children [
                    Html.span (name+ " ")
                    Html.a [
                        prop.href href
                        prop.text "Ref"
                    ]
                ]
            ]
        Html.sup [
            Html.a [
                prop.href ""
                prop.onClick(fun e ->
                    e.preventDefault()
                    match citationContainer.current with
                    | Some citationContainer ->
                        citationContainer.scrollIntoView()
                    | None -> ()
                )
                prop.className "underline"
                prop.text number
            ]
            if citationContainer.current.IsSome then
                ReactDOM.createPortal(listElement,citationContainer.current.Value)
        ]
        

    [<ReactComponent>]
    static member Main() =
        let ref = React.useElementRef()
        let chlamyAtlasRef = About.PaperReF(ref, "A chloroplast protein atlas reveals punctate structures and spatial organization of biosynthetic pathways.", Shared.Urls.REFPaperAtlas, 1)
        let proteomiqonGithubRef = About.PaperReF(ref, "ProteomIQon open source GitHub repository.", Shared.Urls.ProteomIQon.GitHub, 2)
        let proteomiqonZenodoRef = About.PaperReF(ref, "ProteomIQon Zenodo publication.", Shared.Urls.ProteomIQon.Zenodo, 3)
        Components.MainCard.Main [
            Html.article [
                prop.className "prose"
                prop.children [
                    Html.h1 "About"
                    Html.p [
                        Html.text "The localization and function of proteins are intricately linked, with an understanding of one often facilitating insights into the other. Yet, our knowledge in this area remains notably sparse, particularly within the realm of plant biology. In their research, Wang et al."
                        chlamyAtlasRef
                        Html.text " identified the localization of 1,064 proteins in "
                        Html.i "Chlamydomonas reinhardtii"
                        Html.text ", leveraging this data along with previously known protein locations to train a deep learning model capable of predicting three specific localizations: "
                        Html.strong "chloroplast"
                        Html.text ", "
                        Html.strong "mitochondria"
                        Html.text ", or "
                        Html.strong "secretory proteins"
                        Html.text "."
                    ]
                    Html.p [
                        Html.text "Building upon their foundation, we enhanced the model by incorporating a more advanced protein language model and adopting a more efficient pooling method. Additionally, we introduced q-values for all predictions using ProteomIQon"
                        proteomiqonGithubRef
                        Html.sup ","
                        proteomiqonZenodoRef
                        Html.text ", ensuring high confidence levels in the model's outputs, thereby enhancing the reliability and applicability of the predictive model."
                    ]
                    Html.h2 "References"
                    Html.ol [
                        prop.id "citation-container"
                        prop.ref ref
                    ]
                ]
            ]
        ]