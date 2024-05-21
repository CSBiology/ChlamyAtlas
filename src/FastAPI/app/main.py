from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from contextlib import asynccontextmanager
import json
from enum import Enum
from pydantic import BaseModel
import uvicorn
import ankh
import torch
from .prediction import Loc_classifier
import os
import asyncio
from .router import predictionAPI
from .request_types import MyClassifiers, AnkhModels
from .life_cycle import loc_classifiers, ankh_base_model

# class Message(BaseModel):
#     API: str
#     Data: object

# class APIMessages(Enum):
#     Error = 'Error'
#     Exit = 'Exit'
#     DataResponse = 'DataResponse'
#     Ping = 'Ping'

# exitMsg = Message(API=APIMessages.Exit.value, Data=None)

# def errorMsg(error: Exception) -> Message:
#     return Message(API=APIMessages.Error.value, Data=repr(error))

# this would be the input if we go for multiple sequences at a time
# class RequestType(BaseModel):
#     Fasta : list[DataInputItem]

# class MyResponseType(BaseModel):
#     Results: list[DataResponseItem]
#     Batch: int

predictor_version = "1.0.0"

@asynccontextmanager
async def lifespan(app: FastAPI):
    # load models for prediction
    # Get the directory of the current file (FastAPI application file)
    DIRPATH = os.path.dirname(os.path.realpath(__file__))
    predchloro = Loc_classifier (0.2,2,768, 2,32,64)
    predchloro.load_state_dict(torch.load(os.path.join(DIRPATH, "models/chloro_model_epoch_13.pt"), map_location=torch.device('cpu')))
    predchloro.eval()
    loc_classifiers[MyClassifiers.predchloro.value] = predchloro
    predmito = Loc_classifier (0.2,2,768, 2,32,64)
    predmito.load_state_dict(torch.load(os.path.join(DIRPATH, "models/mito_model_epoch_6.pt"), map_location=torch.device('cpu')))
    predmito.eval()
    loc_classifiers[MyClassifiers.predmito.value] = predmito
    predsp = Loc_classifier (0.2,2,768, 2,32,64)
    predsp.load_state_dict(torch.load(os.path.join(DIRPATH, "models/sp_model_epoch_55.pt"), map_location=torch.device('cpu')))
    predsp.eval()
    loc_classifiers[MyClassifiers.predsp.value] = predsp
    # load embedding model
    model, tokenizer = ankh.load_base_model()
    model.eval()
    ankh_base_model[AnkhModels.model.value] = model
    ankh_base_model[AnkhModels.tokenizer.value] = tokenizer
    yield
    # Clean up the ML models and release the resources
    loc_classifiers.clear()
    ankh_base_model.clear()

app = FastAPI(debug=True, lifespan=lifespan)

app.include_router(predictionAPI.router)

@app.get("/")
async def root():
    return {"message": "Hello World"}

@app.get("/predictor_version")
def read_root():
    return {"Version": predictor_version}

# ----------------- Websocket code -----------------

# async def run_ml(websocket: WebSocket, data_input: list[DataInputItem]):
#     data_length = len(data_input)
#     for i in range(data_length):
#         item_i = i + 1
#         input = data_input[i]
#         results = await prediction(
#             [input],
#             ankh_base_model[AnkhModels.tokenizer.value],
#             ankh_base_model[AnkhModels.model.value],
#             loc_classifiers[MyClassifiers.predchloro.value],
#             loc_classifiers[MyClassifiers.predmito.value],
#             loc_classifiers[MyClassifiers.predsp.value]
#         )
#         # results = [DataResponseItem(Header=e.Header, Predictions=[12.9,1923.,1283.2]) for e in batch]
#         # await asyncio.sleep(60)
#         response_data = dict(Results=[result.model_dump() for result in results], Batch=item_i)
#         response = MyResponseType(**response_data)
#         msg = Message(API=APIMessages.DataResponse.value, Data=response)
#         print("[WS] Sending batch --",item_i,"/",data_length,"-- ..")
#         await websocket.send_json(msg.model_dump())
#         # https://stackoverflow.com/a/72865252/12858021
#         await asyncio.sleep(2)
#     print("[WS] Done. Closing..")
#     await websocket.send_json(exitMsg.model_dump())
#     print("[WS] Closing message sent")

# @app.websocket("/dataml/{id}")
# async def websocket_endpoint(websocket: WebSocket, id):
#     print("[WS] Connected")
#     await websocket.accept()
#     # ping_task = asyncio.create_task(send_ping(websocket))
#     try:
#         # To allow multiprocessing, it might be better to use: https://stackoverflow.com/questions/72703446/how-to-do-multiprocessing-with-non-blocking-fastapi-websockets
#         # while True:
#         print ("[WS] Receiving data ..")
#         data_bytes = await websocket.receive_bytes()
#         print ("[WS] bytes received:", len(data_bytes))
#         data_json = data_bytes.decode('utf-8')
#         print ("[WS] json received ..", data_json[:100], "..")
#         data_map = json.loads(data_json)
#         data_input = RequestType(**data_map)
#         print ("[WS] Starting ml ..")
#         await run_ml(websocket, data_input.Fasta)
#     except WebSocketDisconnect:
#         #on_disconnect
#         print("[WS] Disconnected")
#         pass
#     except Exception as e:
#         msg = errorMsg(e)
#         print ("[WS] Error:", e)
#         await websocket.send_json(msg.model_dump())
#         await websocket.close(code=1011)

# if __name__ == "__main__":
#     uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info", ws_max_size=None)