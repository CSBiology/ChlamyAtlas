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
# this is what would need to be send 
# currently just a single sequence is supported, but I can change the code to support multiple sequences
# not sure what we did for deepstabp in the end, but changing it to support multiple sequences is easy
class FastaRecord(BaseModel):
    header      : str
    sequence    : str


# path to the source directory for the models
src_dic = str(Path(__file__).parent.parent.parent)


## Examples
#should be chloroplast
example_sequence1  = "MQSLRSAASSRSCPYMPRTGRQLVPTPFRVSVSAGRSRAPVATQPARDVHLAGSMATSSGPAQDWRGEVPQSPLPSVAPVSPAKAVKWFFPSAAQPLASVEEAFSEVVAITTDATRLLPATLVSIFAAAWPVIAKAWTQPVGDTVRILTAGTCALVSRLSELAAAAATILAAGAAAAAATTTTTTTSRLAATSATAQTATATSTPPSAVYDTAAANKYLEGTAFARHAVLATSCQQVLELTRVCTCAGKPFAIALMPDYTYFDKSAQLDALAEMASASGSLLIACSIAPGFYKRAAAAADMMGTPEGWDASAADVERQFGLDVALASRIPGARNRGILEPVTGMPAFMFGGWFVAHNGPLSNYACTSGCELPLSSPFKLLDAMGIFKRLLEWRQKQAAVARLAVGNH*"
fasta1 = FastaRecord(header="Cre03.g207377", sequence=example_sequence1)
#should be mitochondria
example_sequence2 = "MSGAETHRLALLASSRALIARVQLGDAPAALSQALSTLSSVIYASTSYGRSECSCSSSSGREVQPSATPASPATAQTSRVAPFLALQRPSAAFSRLGLSTWAPHASVSGWHGARQLHAGSAAQQAADVPGGSASGSGGKESEGAAKPQEQTVTNPLAAALASAASGPVASGSASAAGGLAQAAQAAAGRGRRQPRNWMWYDADDEREERRKERQRLAWAMGPGGGEEVGAAHLMDVPQSMKKMQRIVKLVRGLPYPDAVAQCSLVPHKAARYMLQALEAAHADATEVKGLDAERLVVGTVFVTRGAYEPGISYHSKGRPGSKTFHRSHIRVVLNQAAERPAPFARIVAPLMSRRGLLGGSGGAGGAPRPRFAYRTEV*"
fasta2 = FastaRecord(header="Cre05.g242950", sequence=example_sequence2)
#should be secretory proteins
example_sequence3 = "MSTAPVKKPVNLPFLPKDVEDLVLWRKPKEAGAIFGGATAAYLAYVYNPFNGFTIVSYLLSIISLALFLWSHLGHFVSRSGPPVPEFLVKGVTQEQARQVADAALPVVNKALGYVGVLASGKDLKTSSLVVVGSYTAGRIFALASPFTLAYVVVVLAFVLPKAYEAKQDEVDKVLAVVKAKVDEAVTAFNNNVLSKIPKAQPPAPKKVD*"
fasta3 = FastaRecord(header="Cre06.g308950", sequence=example_sequence3)
# should be nothing
example_sequence4 = "MGCAQSTPADQGAKPPANTNGHSTARAAPAASAEAPPAANGNGATTPSPLYAAPPSSAQTQQQPAPAPAAPPVVHPPGSQAAVNQSLKSLSANEANAVQATLLKVSMLIQKFAEGRSERTTPVQAVRRALNLVTADCRAKYASVSVLSETQEHALLVTAVGVPDTVHEGNRLLKVPGSNASVERILQRGTDFFYWQPSASEGPAPSDWATLASAAGLTYLAAVPIKVSDKVIGMLTVGFADAAADEADYIMFPTYLQLVAASLSSMVKDNSIPKYMTLVKDLHETQDLDSLMHKVVQHLRTVLGHSNNHHIWYRIGLTAPNNTASTIFDDLTQVPPTLMQRTLSNPTGSSSFKLLKEVQAAGGVMRTVVAMKNTVMKIAVHNRQQVMIPDVQKVINQSGNVSADIFNTRLIKPPTSVLVFPLKVKQHIFGVIFCMSSVQSDFSDVSPKLREVCEVMSPHLLFMLTQPLANDYKTMQTASLTQTAGGSVISEGGSISGMVTGAGGSIGRSDSLGVSLSGDSFMYTQSRSSTGALVTGLTEKLNQKRIRSSMDFHNNTTMTDLQITGLLGEGGFAKVFRGLWRGLVVGVKVVCDDGKNEKMVMKNAHEIAILSALSHPNIVQAYNCLTDVLVRDLLNTTVHRFNNPTVLNSPAYKYLLSMEDKTCHLEVIEYCDLGNLSNALKNNIFMIPNPVIAAAAGAGDGAAAAELAERARQQPMKVNMRTLLLTLIEIASACGYLHRMGVVHCDIKPANVLLKSSNIDFRGFTAKVSDFGLSRVEDDDSCASFPFNSCGTAAYVAPEALICNKKVNSSVDVYAFGILMWEMYTGQRPYGNMKQQQLVEEVVMRGLRPKFPSTAPAGYVVLAQSAWSGSPQARPSFDEILTHLNAMLQQVDDREMDSMVNGSFGSMGEKFEYMQLQQQQAAAAAQAQQGGVPAGMDRRPSQISRRGQPQMPSPMGPGASPRNGVAPGQGGSQVGQGPVPMVAPQQQVQPGMQAAGRPAQAAAPAAPAQGSAHMYSSA*"
fasta4 = FastaRecord(header="Cre06.g310100", sequence=example_sequence4)



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

# run prediction
cpred1,mpred1,secrpred1 = prediction(fasta1.sequence, tokenizer, model, predchloro, predmito, predsp)
cpred2,mpred2,secrpred2 = prediction(fasta2.sequence, tokenizer, model, predchloro, predmito, predsp)
cpred3,mpred3,secrpred3 = prediction(fasta3.sequence, tokenizer, model, predchloro, predmito, predsp)
cpred4,mpred4,secrpred4 = prediction(fasta4.sequence, tokenizer, model, predchloro, predmito, predsp)

#current return is
# float triple
# triple order is chloroplast, mitochondria, secreted
# example: (1.,0.,0.) -> chloroplast: positive, mitochondria: negative, secreted: negative
# however, not clean 1./0. but floats between 1 and 0
print(cpred1,mpred1,secrpred1)
print(cpred2,mpred2,secrpred2)
print(cpred3,mpred3,secrpred3)
print(cpred4,mpred4,secrpred4)



