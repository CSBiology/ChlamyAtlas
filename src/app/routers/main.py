import fastapi
from pydantic import BaseModel
import ankh
from regex import F
import torch
import sys
from pathlib import Path
# path to import from prediction.py
current_dir = Path(__file__).parent
parent_dir = current_dir.parent
sys.path.append(str(parent_dir))
from prediction import Loc_classifier, prediction

# Input data class
# this is what would need to be send if we go for one sequence at a time
class FastaRecord(BaseModel):
    Header      : str
    Sequence    : str

# this would be the input if we go for multiple sequences at a time
class PredictorInfo (BaseModel):
    Fasta : list[FastaRecord]


# path to the source directory for the models
src_dic = str(Path(__file__).parent.parent.parent)


## Examples
#should be chloroplast
example_sequence1  = "MQSLRSAASSRSCPYMPRTGRQLVPTPFRVSVSAGRSRAPVATQPARDVHLAGSMATSSGPAQDWRGEVPQSPLPSVAPVSPAKAVKWFFPSAAQPLASVEEAFSEVVAITTDATRLLPATLVSIFAAAWPVIAKAWTQPVGDTVRILTAGTCALVSRLSELAAAAATILAAGAAAAAATTTTTTTSRLAATSATAQTATATSTPPSAVYDTAAANKYLEGTAFARHAVLATSCQQVLELTRVCTCAGKPFAIALMPDYTYFDKSAQLDALAEMASASGSLLIACSIAPGFYKRAAAAADMMGTPEGWDASAADVERQFGLDVALASRIPGARNRGILEPVTGMPAFMFGGWFVAHNGPLSNYACTSGCELPLSSPFKLLDAMGIFKRLLEWRQKQAAVARLAVGNH*"
fasta1 = FastaRecord(Header="Cre03.g207377", Sequence=example_sequence1)
#should be mitochondria
example_sequence2 = "MSGAETHRLALLASSRALIARVQLGDAPAALSQALSTLSSVIYASTSYGRSECSCSSSSGREVQPSATPASPATAQTSRVAPFLALQRPSAAFSRLGLSTWAPHASVSGWHGARQLHAGSAAQQAADVPGGSASGSGGKESEGAAKPQEQTVTNPLAAALASAASGPVASGSASAAGGLAQAAQAAAGRGRRQPRNWMWYDADDEREERRKERQRLAWAMGPGGGEEVGAAHLMDVPQSMKKMQRIVKLVRGLPYPDAVAQCSLVPHKAARYMLQALEAAHADATEVKGLDAERLVVGTVFVTRGAYEPGISYHSKGRPGSKTFHRSHIRVVLNQAAERPAPFARIVAPLMSRRGLLGGSGGAGGAPRPRFAYRTEV*"
fasta2 = FastaRecord(Header="Cre05.g242950", Sequence=example_sequence2)
#should be secretory proteins
example_sequence3 = "MSTAPVKKPVNLPFLPKDVEDLVLWRKPKEAGAIFGGATAAYLAYVYNPFNGFTIVSYLLSIISLALFLWSHLGHFVSRSGPPVPEFLVKGVTQEQARQVADAALPVVNKALGYVGVLASGKDLKTSSLVVVGSYTAGRIFALASPFTLAYVVVVLAFVLPKAYEAKQDEVDKVLAVVKAKVDEAVTAFNNNVLSKIPKAQPPAPKKVD*"
fasta3 = FastaRecord(Header="Cre06.g308950", Sequence=example_sequence3)
# should be nothing
example_sequence4 = "MGCAQSTPADQGAKPPANTNGHSTARAAPAASAEAPPAANGNGATTPSPLYAAPPSSAQTQQQPAPAPAAPPVVHPPGSQAAVNQSLKSLSANEANAVQATLLKVSMLIQKFAEGRSERTTPVQAVRRALNLVTADCRAKYASVSVLSETQEHALLVTAVGVPDTVHEGNRLLKVPGSNASVERILQRGTDFFYWQPSASEGPAPSDWATLASAAGLTYLAAVPIKVSDKVIGMLTVGFADAAADEADYIMFPTYLQLVAASLSSMVKDNSIPKYMTLVKDLHETQDLDSLMHKVVQHLRTVLGHSNNHHIWYRIGLTAPNNTASTIFDDLTQVPPTLMQRTLSNPTGSSSFKLLKEVQAAGGVMRTVVAMKNTVMKIAVHNRQQVMIPDVQKVINQSGNVSADIFNTRLIKPPTSVLVFPLKVKQHIFGVIFCMSSVQSDFSDVSPKLREVCEVMSPHLLFMLTQPLANDYKTMQTASLTQTAGGSVISEGGSISGMVTGAGGSIGRSDSLGVSLSGDSFMYTQSRSSTGALVTGLTEKLNQKRIRSSMDFHNNTTMTDLQITGLLGEGGFAKVFRGLWRGLVVGVKVVCDDGKNEKMVMKNAHEIAILSALSHPNIVQAYNCLTDVLVRDLLNTTVHRFNNPTVLNSPAYKYLLSMEDKTCHLEVIEYCDLGNLSNALKNNIFMIPNPVIAAAAGAGDGAAAAELAERARQQPMKVNMRTLLLTLIEIASACGYLHRMGVVHCDIKPANVLLKSSNIDFRGFTAKVSDFGLSRVEDDDSCASFPFNSCGTAAYVAPEALICNKKVNSSVDVYAFGILMWEMYTGQRPYGNMKQQQLVEEVVMRGLRPKFPSTAPAGYVVLAQSAWSGSPQARPSFDEILTHLNAMLQQVDDREMDSMVNGSFGSMGEKFEYMQLQQQQAAAAAQAQQGGVPAGMDRRPSQISRRGQPQMPSPMGPGASPRNGVAPGQGGSQVGQGPVPMVAPQQQVQPGMQAAGRPAQAAAPAAPAQGSAHMYSSA*"
fasta4 = FastaRecord(Header="Cre06.g310100", Sequence=example_sequence4)

pred = PredictorInfo(Fasta=[fasta1,fasta2,fasta3,fasta4])


# load embedding model
model, tokenizer = ankh.load_base_model()
model.eval()

# load models for prediction
predchloro = Loc_classifier (0.2,2,768, 2,32,64)
predchloro.load_state_dict(torch.load(src_dic+"/models/chloro_model_epoch_13.pt", map_location=torch.device('cpu')))
predchloro.eval()
predmito = Loc_classifier (0.2,2,768, 2,32,64)
predmito.load_state_dict(torch.load(src_dic+"/models/mito_model_epoch_6.pt", map_location=torch.device('cpu')))
predmito.eval()
predsp = Loc_classifier (0.2,2,768, 2,32,64)
predsp.load_state_dict(torch.load(src_dic+"/models/sp_model_epoch_55.pt", map_location=torch.device('cpu')))
predsp.eval()

# # run prediction single sequence
out1 = prediction([fasta1], tokenizer, model, predchloro, predmito, predsp)
out2 = prediction([fasta2], tokenizer, model, predchloro, predmito, predsp)
out3 = prediction([fasta3], tokenizer, model, predchloro, predmito, predsp)
out4 = prediction([fasta4], tokenizer, model, predchloro, predmito, predsp)
# run prediction multiple sequences (can also be used for single sequence)
out5 = prediction(pred.Fasta, tokenizer, model, predchloro, predmito, predsp)


#current return is either
# class SinglePrediction(BaseModel):
#     name : str
#     prediction : list[float]
# or
# class MultiPrediction(BaseModel):
#     names : list[str]
#     predictions : list[list[float]]
#
# List element order is chloroplast, mitochondria, secreted
# example: [1.,0.,0.] -> chloroplast: positive, mitochondria: negative, secreted: negative
# however, not clean 1./0. but floats between 1 and 0

print (out1)
print (out2)
print (out3)
print (out4)
print (out5)



