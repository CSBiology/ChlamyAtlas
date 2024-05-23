from fastapi import APIRouter
from pydantic import BaseModel
from ..life_cycle import loc_classifiers, ankh_base_model
from ..request_types import DataInputItem, AnkhModels, MyClassifiers
from ..prediction import prediction

def run_ml(data_input: DataInputItem):
    return prediction(
        data_input,
        ankh_base_model[AnkhModels.tokenizer.value],
        ankh_base_model[AnkhModels.model.value],
        loc_classifiers[MyClassifiers.predchloro.value],
        loc_classifiers[MyClassifiers.predmito.value],
        loc_classifiers[MyClassifiers.predsp.value]
    )

router = APIRouter(
    prefix="/api/v1",
    tags=["v1"]
)

@router.post("/predict", tags=["latest"])
async def predict(info: DataInputItem):
    print("[Predict]", info.Header)
    prediction = run_ml(info)
    print("[Return]", prediction)
    return {"Prediction": prediction}

