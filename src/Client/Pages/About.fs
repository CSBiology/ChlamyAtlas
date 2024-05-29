namespace Pages

open Feliz
open Feliz.DaisyUI
open Fable.Core
open Fable.Core.JsInterop
open Shared
open Feliz.Router

open Shared.Urls

type About =

    [<ReactComponent>]
    static member private PaperReF(citationContainer: Browser.Types.HTMLElement option, name: string, href, number: int) =
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
                    match citationContainer with
                    | Some citationContainer ->
                        citationContainer.scrollIntoView()
                    | None -> ()
                )
                prop.className "underline"
                prop.text number
            ]
            if citationContainer.IsSome then
                ReactDOM.createPortal(listElement,citationContainer.Value)
        ]
        

    [<ReactComponent>]
    static member Main() =
        let ref0 = React.useElementRef()
        let (ref: Browser.Types.HTMLElement option), setRef = React.useState(ref0.current)
        React.useEffect(fun () ->
            // Force a rerender, so it can be passed to the child.
            // If this causes an unwanted flicker, use useLayoutEffect instead
            setRef ref0.current
        )
        Components.MainCard.Main [
            Html.article [
                prop.className "prose 2xl:text-lg"
                prop.children [
                    Html.h1 "About"
                    Html.p [
                        Html.text "Chlamy Atlas is a AI-powered web application which predicts the localizations of proteins from the Green Algae "
                        Html.i "Chlamydomonas reinhardtii"
                        Html.text "."
                    ]
                    Html.p [
                        Html.text "The localization and function of proteins are intricately linked, with an understanding of one often facilitating insights into the other. Yet, our knowledge in this area remains notably sparse, particularly within the realm of plant biology. In their research, Wang et al."
                        About.PaperReF(ref, "A chloroplast protein atlas reveals punctate structures and spatial organization of biosynthetic pathways", REF.REFPaperAtlas, 1)
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
                        About.PaperReF(ref, "ProteomIQon open source GitHub repository", REF.ProteomIQon.GitHub, 2)
                        Html.sup ","
                        About.PaperReF(ref, "ProteomIQon Zenodo publication", REF.ProteomIQon.Zenodo, 3)
                        Html.text ", ensuring high confidence levels in the model's outputs, thereby enhancing the reliability and applicability of the predictive model."
                    ]
                    Html.h2 "Handling Long Protein Sequences"
                    Html.p [
                        Html.text "Chlamy Atlas is built on a protein language model composed of transformer blocks. The computation of sequences by these blocks scales quadratically with the sequence's length, making it impractical to process sequences of arbitrary length in the usual manner. Therefore, for longer sequences, we opted to slice out the middle portion, as targeting signals are typically located at the N or C terminus."
                        About.PaperReF(ref, "Detecting sequence signals in targeting peptides using deep learning", REF.REFSequenceSignalsDeepLearning, 4)
                        Html.sup ","
                        About.PaperReF(ref, "DeepLoc 2.0: multi-label subcellular localization prediction using protein language models", REF.REFDeepLoc, 5)
                        Html.textf " This approach minimizes the impact on the final prediction while ensuring quick computation. However, since this process can still affect prediction accuracy, we apply it only to protein sequences exceeding a maximum length of %i amino acids." Shared.Constants.MaxSequenceLength
                    ]
                    Html.h2 "References"
                    Html.ol [
                        prop.id "citation-container"
                        prop.ref ref0
                    ]
                ]
            ]
        ]