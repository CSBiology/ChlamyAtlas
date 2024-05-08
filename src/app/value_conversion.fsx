#r "nuget: ProteomIQon, 0.0.8"
#r "nuget: Deedle"
#r "nuget: FSharp.Stats, 0.4.0"
open Deedle
open FSharp.Stats
open ProteomIQon

//Workflow of the script:
// 1. Read in the predictions of each model on the test dataset
// // Using the functions frameToValues and valuesToDecoys to convert the dataframe into an Array of type BaseDecoys
// 2. Convert the predictions of the model on the test values to a QValues/predicted score function 
// // Allows the get a qValue for a new prediction
// // using the functions getBandwith and calculateQValueLogReg from the FDRControl' module (ProteomIQon)
// 3. Take the prediction of the model and use the QValue functions to get the QValue for the predictions
// // using the function addQValues
// // using the function transformMultiToSingle to convert the multiple predictions to a list of single predictions if needed
// 4. Alternativly, take the prediction of the model and use the QValue functions to get the QValue for the predictions and the final prediction
// // using the function addQValuesAndPrediction

//types for the dataframes to QValue function conversion
type BaseValues = 
    {
        Score:float
        Label:float
    }

type BaseDecoys = 
    {
        Score:float
        Decoy:bool
    }

// Input type (this should come from the python script)
// Single prediction
type PredOutput =
    {
        Header     : string
        Prediction : float list
    }

//Output type (in case we want to do the cutoff computation for the user
// would require that the cutoff is stored in another type (the original input type probably))
// // new output type
type CompleteOutput =
    {
        Header     : string
        Chloropred : float
        Qchloro    : float
        Mitopred   : float
        Qmito      : float
        Secrpred   : float
        Qsecr      : float
        FinalPred  : string
    }
//functions to read the dataframes and covert them to the QValue functions
let frameToValues dataframe = 
    dataframe
        |>Frame.mapRows (fun k s ->
                {
                    Score = s.GetAs<float>("pred")
                    Label = s.GetAs<float>("target")
                }
        )
        |>Series.values
        |>Array.ofSeq

let valuesToDecoys (values: BaseValues array) =
    values
    |> Array.map (fun v -> 
        {   
            Score = v.Score; 
            Decoy = if v.Label = 0. then true else false
        }
    )
// function to determine the optimal bandwith for the QValue function
let getBandwith frameOfIntereset = 
    frameOfIntereset
    |>frameToValues
    |>Array.map (fun x -> x.Score)
    |>Distributions.Bandwidth.nrd0

//function to add the QValues to the Input type to create the Output type
let addQValuesAndPrediction (prediction: PredOutput) (funcChloroQ: float->float) (funcMitoQ2: float->float) (funcSecrQ: float->float) (cutoff)=
    let qChloro = funcChloroQ prediction.Prediction.[0]
    let qMito = funcMitoQ2 prediction.Prediction.[1]
    let qSecr = funcSecrQ prediction.Prediction.[2]
    let finalOutput = 
        if   qChloro < cutoff && qMito > cutoff   && qSecr > cutoff then "Chloroplast"
        elif qMito < cutoff   && qChloro > cutoff && qSecr > cutoff then "Mitochondria"
        elif qSecr < cutoff   && qMito > cutoff   && qChloro > cutoff then "Secretory"
        elif qChloro < cutoff && qMito < cutoff   && qSecr > cutoff then "Chloroplast, Mitochondria"
        elif qChloro < cutoff && qSecr < cutoff   && qMito > cutoff then "Chloroplast, Secretory"
        elif qMito < cutoff   && qSecr < cutoff   && qChloro > cutoff then "Mitochondria, Secretory"
        elif qChloro < cutoff && qMito < cutoff   && qSecr < cutoff then "Chloroplast, Mitochondria, Secretory"
        else "Cytoplasmic"
    {
        Header = prediction.Header
        Chloropred = prediction.Prediction.[0]
        Mitopred = prediction.Prediction.[1]
        Secrpred = prediction.Prediction.[2]
        Qchloro = qChloro
        Qmito = qMito
        Qsecr = qSecr
        FinalPred = finalOutput
    }

//read the dataframes
let frameChloro  = Frame.ReadCsv(@"src\test_predictions\prediction_chloro_epoch13.csv",hasHeaders=true,separators=",")
let frameMito  = Frame.ReadCsv(@"src\test_predictions\prediction_mito_epoch6.csv",hasHeaders=true,separators=",")
let frameSecr  = Frame.ReadCsv(@"src\test_predictions\prediction_sp_epoch55.csv",hasHeaders=true,separators=",")

//convert the dataframes to the QValue functions
let decoyArrayChloro = 
    frameChloro
    |>frameToValues
    |>valuesToDecoys

let decoyArrayMito = 
    frameMito
    |>frameToValues
    |>valuesToDecoys

let decoyArraySecr = 
    frameSecr
    |>frameToValues
    |>valuesToDecoys

let chloroQ = FDRControl'.calculateQValueLogReg 1.0(getBandwith frameChloro)(decoyArrayChloro) (fun (x:BaseDecoys)-> x.Decoy) (fun x->x.Score) (fun x -> x.Score)
let mitoQ = FDRControl'.calculateQValueLogReg 1.0(getBandwith frameMito)(decoyArrayMito) (fun (x:BaseDecoys)-> x.Decoy) (fun x->x.Score) (fun x -> x.Score)
let secrQ = FDRControl'.calculateQValueLogReg 1.0(getBandwith frameSecr)(decoyArraySecr) (fun (x:BaseDecoys)-> x.Decoy) (fun x->x.Score) (fun x -> x.Score)

//example inputs:
let example1: PredOutput = 
    {
        Header = "Cre03.g207377"
        Prediction = [0.8505728244781494;0.20688468217849731;0.14377914369106293]
    }

let example2: PredOutput =
    {
        Header = "Cre05.g242950"
        Prediction = [0.1338839828968048;0.6741871237754822;0.13130035996437073]
    }

let example3: PredOutput =
    {
        Header = "Cre05.g242950"
        Prediction = [0.2514885365962982; 0.09152786433696747; 0.2829943597316742]
    }

//example cutoff
let cutoff = 0.05

//calculation of the outputs from the examples
let example1output_ = addQValuesAndPrediction example1 chloroQ mitoQ secrQ cutoff
let examp2e1output_ = addQValuesAndPrediction example2 chloroQ mitoQ secrQ cutoff
let example4output_ = addQValuesAndPrediction example3 chloroQ mitoQ secrQ cutoff

//run multiple examples
let arrayOfInputs = [|example1;example2;example3|]

let arrayOfOutputs = 
    arrayOfInputs
    |> Array.map (fun x ->
        addQValuesAndPrediction x chloroQ mitoQ secrQ cutoff
    )
    