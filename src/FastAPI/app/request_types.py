from pydantic import BaseModel
from enum import Enum

# Input data class
class DataInputItem(BaseModel):
    Header      : str
    Sequence    : str


# response data class
class DataResponseItem(BaseModel):
    Header      : str
    Predictions : list[float]

class MyClassifiers(Enum):
    predchloro = 'predchloro'
    predmito = 'predmito'
    predsp = 'predsp'

class AnkhModels(Enum):
    model = 'model'
    tokenizer='tokenizer'