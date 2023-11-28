# ChlamyAtlas
A web UI for optimised versions of the models published in Wang et al. 2023.

Requirements for the python side:
Python 3.10.10 (used for the ml api)
Required Conda packages (moved to docker later):
Pytorch v2.1 (pip install torch=2.1)
ankh (pip install ankh)
fastapi (pip install fastapi)

Function:
The main function taks as input a fasta file. The sequence is used for the prediction.
The sequence is first transformed (probably best to move this to the F# backend further down the line) so it matches the required shape for the tokenizer model.
The transformed sequence is then given to the ankh tokenizer and model which results in an embedding for each amino acid.
This embedding is the input for three different models. They predict if the original protein localises to the chloroplast, mitochondira, secretory protein, or if it is does neither of the three.
Currently the return type is currently unclear (probably something like Protein_ID (faster header), Chloroplast (bool), Chloroplast (qValue), Mitochondria (bool), Mitochondria (qValue), Secretory Protein(bool), Secretory Protein (qValue)).

To DO:
Use the correct model files for the prediction (The current ones are just placeholders, as I need to double check the training data...)
Implement the qValue calculation (probably in the F#, I will make the backend file for this calculation; probably needs a function call in the backend in the end on the fields of the types that you get from the python files)
Comments for the code
  
