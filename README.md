# ChlamyAtlas
A web UI for optimised versions of the models published in Wang et al. 2023.

<b>General Description </b>
The goal of this web service should be to allow a researcher to input one or multiple fasta sequence of chlamydomonas and one qValue cutoff. The webservice should return the predicted scores for the localication to the chloroplast, mitochodria, or to the secretory pathway. Furthermore, the return should also include the qValue for each prediction and the final prediction according to the qValue cutoff. 

<b>Requirements for the python side:</b>

Python 3.10.10 (used for the ml api)

Pytorch v2.1 (pip install torch=2.1)

ankh (pip install ankh)

fastapi (pip install fastapi)

<b>Requirements for the F# side: </b>

.NET SDK 7 (only tested 7, but I assume it should work with >5 since both the referenced Fsharp.Stats version, and ProteomIQon only need that)

<b>Example:</b>
The predict function gets called in /src/app/rounter/main.py<br>
4 Examples are given and the output is printed at the end<br>
The QValue conversion is given in /src/app/value_conversion.fsx<br>
Two eampples are given at the end on what should happen with the Input type to get the final Output type for one sequence.

<b>Function:</b>

The main python function taks as input a sequence that is used for the prediction.<br>
The sequence is first transformed (probably best to move this to the F# backend further down the line) so it matches the required shape for the tokenizer model.<br>
The transformed sequence is then given to the ankh tokenizer and model which results in an embedding for each amino acid.<br>
This embedding is the input for three different models. They predict if the original protein localises to the chloroplast, mitochondira, secretory protein.<br>
The return of the prediction is a triple with the three predictions (chloro_prediction, mito_prediction, secr_prediction).<br>
The prediction should then be given to an F# script that calculates the corresponding qValues for each Prediction.<br>
Afterwards a cutoff given from the user should be used to determine the final prediction.
The final type returned by the F# scipt has the fields Header, Chloropred, Qchloro, Mitopred, Qmito, Secrpred, Qsecr, finalPrediction. <br>

<b>TO DO:</b><br>
- Determine how to deal with multiple sequences (either send them all to the python srcipt as list, or all the python script multiple times)
