#r "nuget: ProteomIQon, 0.0.8"
#r "nuget: Deedle.Interactive, 3.0.0"
#r "nuget: FSharp.Stats, 0.5.0"
open Deedle
open ProteomIQon
open FSharp.Stats

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
type PredOutput =
    {
        Header     : string
        Chloropred : float
        Mitopred   : float
        Secrpred   : float
    }

//Output type (this should be what the user gets as output (probably in table from in the end))
type CompleteOutput =
    {
        Header     : string
        Chloropred : float
        Qchloro    : float
        Mitopred   : float
        Qmito     : float
        Secrpred   : float
        Qsecr      : float
    }
//functions to read the dataframes and covert them to the QValue functions
let frameToValues dataframe = 
    dataframe
        |>Deedle.Frame.mapRows (fun k s ->
                {
                    Score = s.GetAs<float>("pred")
                    Label = s.GetAs<float>("target")
                }
            )
        |>Deedle.Series.values
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
let addQValues (prediction: PredOutput) (funcChloroQ: float->float) (funcMitoQ2: float->float) (funcSecrQ: float->float) =
    {
        Header = prediction.Header
        Chloropred = prediction.Chloropred
        Mitopred = prediction.Mitopred
        Secrpred = prediction.Secrpred
        Qchloro = funcChloroQ prediction.Chloropred
        Qmito = funcMitoQ2 prediction.Mitopred
        Qsecr = funcSecrQ prediction.Secrpred
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

let chloroQ = ProteomIQon.FDRControl'.calculateQValueLogReg 1.0(getBandwith frameChloro)(decoyArrayChloro) (fun (x:BaseDecoys)-> x.Decoy) (fun x->x.Score) (fun x -> x.Score)
let mitoQ = ProteomIQon.FDRControl'.calculateQValueLogReg 1.0(getBandwith frameMito)(decoyArrayMito) (fun (x:BaseDecoys)-> x.Decoy) (fun x->x.Score) (fun x -> x.Score)
let secrQ = ProteomIQon.FDRControl'.calculateQValueLogReg 1.0(getBandwith frameSecr)(decoyArraySecr) (fun (x:BaseDecoys)-> x.Decoy) (fun x->x.Score) (fun x -> x.Score)

//example inputs:
let example1 = 
    {
        Header = "Cre03.g207377"
        Chloropred = 0.8505728244781494
        Mitopred = 0.20688468217849731
        Secrpred = 0.14377914369106293
    }

let example2 =
    {
        Header = "Cre05.g242950"
        Chloropred = 0.1338839828968048
        Mitopred = 0.6741871237754822
        Secrpred = 0.13130035996437073
    }

//run the example inputs through the addQValues function
let example1output = addQValues example1 chloroQ mitoQ secrQ
let example2output = addQValues example2 chloroQ mitoQ secrQ

