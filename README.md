# ChlamyAtlas
A web UI for optimised versions of the models published in Wang et al. 2023.

<b>Requirements for the python side:</b>

Python 3.10.10 (used for the ml api)

Pytorch v2.1 (pip install torch=2.1)

ankh (pip install ankh)

fastapi (pip install fastapi)

<b>Example:</b>
The predict function gets called in /src/app/rounter/main.py<br>
4 Examples are given and the output is printed at the end<br>
The QValue conversion is given in /src/app/value_conversion.fsx<br>
Two eampples are given at the end on what should happen with the Input type to get the final Output type for one sequence.

<b>Function:</b>

The main function taks as input a fasta file. The sequence is used for the prediction.<br>
The sequence is first transformed (probably best to move this to the F# backend further down the line) so it matches the required shape for the tokenizer model.<br>
The transformed sequence is then given to the ankh tokenizer and model which results in an embedding for each amino acid.<br>
This embedding is the input for three different models. They predict if the original protein localises to the chloroplast, mitochondira, secretory protein, or if it is does neither of the three.<br>
The return of the prediction is a triple with the three predictions (chloro_prediction, mito_prediction, secr_prediction).<br>
The prediction should then be given to an F# script that calculates the corresponding qValues for each Prediction.<br>
The final type returned by the F# scipt has the fields Header, Chloropred, Qchloro, Mitopred, Qmito, Secrpred, Qsecr. <br>

<b>TO DO:</b><br>
- Determine how to deal with multiple sequences (either send them all to the python srcipt as list, or all the python script multiple times)
