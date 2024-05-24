module PythonService

open System.Net.Http
open Newtonsoft.Json
open Shared


open System.Net.WebSockets;
open System.Text;
open System.Threading;
open System.Threading.Tasks;
open System

module Helper =

    let python_service_api_v1 = Environment.python_service_url() + "/api/v1"
    let v1PredictApiUrl = python_service_api_v1 + "/predict"
    let pythonServiceClient() =
        let httpClient = new HttpClient()
        httpClient.BaseAddress <- System.Uri(Environment.python_service_url())
        httpClient.Timeout <- Environment.python_service_timeout()
        httpClient

open Helper

//// Used to serialize Enum Unioncases as strings.
//let settings = JsonSerializerSettings()
//settings.Converters.Add(Converters.StringEnumConverter())

//let helloWorldHandler () =
//    task {
//        let! response = pythonServiceHandler().GetAsync("/")
//        let! content = response.Content.ReadAsStringAsync()
//        let r = JsonConvert.DeserializeObject<HelloWorld>(content)
//        return r
//    }
//    |> Async.AwaitTask

let inline logws (id: Guid) format =
    Printf.kprintf (fun msg -> printfn "[%A] %s" id msg) format

let getVersion() =
    task {
        let! response = pythonServiceClient().GetAsync("/predictor_version")
        let! content = response.Content.ReadAsStringAsync()
        let version = JsonConvert.DeserializeObject<{|Version: string|} >(content)
        return version.Version
    }
    |> Async.AwaitTask

let mlPrediction(dro: DataResponse) =
    let id = dro.Id
    Storage.Storage.Set(id, {dro with Status = DataResponseStatus.Starting})
    let mkContent (di:DataInputItem) = new StringContent(JsonConvert.SerializeObject(di), Encoding.UTF8, "application/json")
    logws id "Starting client .."
    let client = pythonServiceClient()
    let mkRequest(di: DataInputItem) = client.PostAsync(v1PredictApiUrl, mkContent di)
    let mkResponse(responseJson: string) = JsonConvert.DeserializeObject<{|Prediction: DataResponseItem|} >(responseJson)
    let mutable latestItem = 0
    let itemCount = dro.ItemCount
    task {
        try 
            logws id "Starting ml prediction loop .."
            for i in 0 .. (dro.ItemCount-1) do
                let di = dro.InitData.Items.[i]
                let! request = mkRequest di
                let! content = request.Content.ReadAsStringAsync()
                let responseItem = mkResponse content
                latestItem <- (i+1)
                Storage.Storage.Update(id,fun current -> {
                    current with
                        Status = DataResponseStatus.MLRunning(latestItem, current.ItemCount)
                        PredictionData = responseItem.Prediction::current.PredictionData
                    }
                )
                logws id "DataResponse received .. -- %i/%i --" latestItem itemCount
            logws id "Starting analysis .."
            Storage.Storage.Update(id, fun current -> { current with Status = DataResponseStatus.AnalysisRunning})
            let current = Storage.Storage.Get id
            let! analysedData = Analysis.runAnalysis current
            // email
            match (Storage.EmailStorage.TryGet id) with
            | Some email -> 
                logws id "Sending notification email .."
                Email.sendNotification email
                |> Async.RunSynchronously
            | None ->
                logws id "Not subscribed to notification service .."
            Storage.Storage.Update(id, fun current -> {
                current with
                    Status = DataResponseStatus.Finished
                    ResultData = analysedData
                }
            )
            logws id "Analysis done. Task completed successfully .."
        with
            | e ->
                match (Storage.EmailStorage.TryGet id) with
                | Some email -> 
                    logws id "Sending error notification email .."
                    Email.sendErrorNotification (email, e.Message)
                    |> Async.RunSynchronously
                | None ->
                    logws id "Not subscribed to notification service .."
                logws id "Error: %A" e.Message
                Storage.Storage.Update(id,fun current -> { current with Status = DataResponseStatus.Error e.Message})
    }
    |> Async.AwaitTask
        
        

//type Message = {
//    API: string
//    Data: obj
//}

//type RequestType = {
//    Fasta: DataInputItem []
//} with
//    static member init(items: DataInputItem []) = { Fasta = items }

//type ResponseType = {
//    Results: DataResponseItem list
//    Batch: int
//}

//open System.Threading.Tasks

// Useful links:
// https://www.newtonsoft.com/json
// https://stackoverflow.com/questions/42000362/creating-a-proxy-to-another-web-api-with-asp-net-core

//open Websocket.Client

//let subscribeWebsocket (dro: DataResponse) =
//    let id = dro.Id
//    Storage.Storage.Set(id, {dro with Status = DataResponseStatus.Starting})
//    let exitEvent = new ManualResetEvent(false)
//    async {
//        logws id "Subscribing to websocket .."
//        let uri = Environment.python_service_websocket + "/" + id.ToString() |> Uri
//        let setKeepAliveIntervaL = fun () ->
//            let cws = new ClientWebSocket()
//            cws.Options.KeepAliveInterval <- TimeSpan.FromSeconds(600)
//            cws
//        use client = new WebsocketClient(uri, setKeepAliveIntervaL)
//        client.IsReconnectionEnabled <- true
//        let closeAll(msg) =
//            logws id "%s" msg
//            client.Dispose()
//            exitEvent.Set() |> ignore
//        client.ReconnectionHappened.Subscribe(fun info ->
//            match info.Type with
//            | ReconnectionType.Lost ->
//                Storage.Storage.Update(id,fun current -> { current with Status = DataResponseStatus.Error "Connection lost." })
//                closeAll("Connection lost.")
//            | ReconnectionType.NoMessageReceived ->
//                client.Reconnect() |> Async.AwaitTask |> Async.StartImmediate
//                let data = Storage.Storage.Get id
//                let missingData = data.GetMissingPredictionItems0()
//                let dto = createDataInputDTO missingData
//                client.Send(dto) |> ignore
//            | _ ->
//                logws id "Reconnection happened, type: %A" info.Type
//        )
//        |> ignore
//        client.DisconnectionHappened.Subscribe(fun info ->
//            match info.Type with
//            | DisconnectionType.Error ->
//                Storage.Storage.Update(id,fun current -> { current with Status = DataResponseStatus.Error "Disconnection.Error happened." })
//                closeAll("Disconnection.Error happened.")
//            | _ ->
//                logws id "Disconnection happened, type: %A" info.Type
//        )
//        |> ignore
//        client.MessageReceived.Subscribe(fun response ->
//            try
//                let msg = Json.JsonSerializer.Deserialize<Message>(response.Text)
//                logws id "Message received: %A" msg.API
//                match msg.API with
//                | "Ping" -> () // This is just keep alive message
//                | "Exit" ->
//                    closeAll("Closing client ..")
//                    logws id "Starting analysis .."
//                    Storage.Storage.Update(id, fun current -> { current with Status = DataResponseStatus.AnalysisRunning } )
//                    Storage.Storage.Update(id, fun current ->
//                        if current.AllItemsProcessed then
//                            logws id "Running analysis .."
//                            let analysisResult =
//                                try
//                                    let res = Analysis.runAnalysis current |> Async.RunSynchronously
//                                    logws id "Analysis done. Task completed successfully .."
//                                    res
//                                with
//                                    | e -> {current with Status = DataResponseStatus.Error e.Message}
//                            analysisResult
//                        else
//                            { current with Status = DataResponseStatus.Error "Unhandled Error: Not all items processed."}
//                    )
//                    //email
//                    match (Storage.EmailStorage.TryGet id) with
//                    | Some email -> 
//                        logws id "Sending notification email .."
//                        Email.sendNotification email
//                        |> Async.RunSynchronously
//                    | None ->
//                        logws id "Not subscribed to notification service .."
//                | "Error" ->
//                    logws id "Python Error: %A" msg.Data
//                    closeAll("Python error")
//                | "DataResponse" ->
//                    let responseData: ResponseType = Json.JsonSerializer.Deserialize<ResponseType>(msg.Data :?> Json.JsonElement)
//                    Storage.Storage.Update(id,fun current ->
//                        logws id "DataResponse received .. -- %i/%i --" responseData.Batch current.ItemCount
//                        { current with
//                            Status = DataResponseStatus.MLRunning(responseData.Batch, current.ItemCount)
//                            PredictionData =
//                                let newData = responseData.Results
//                                newData@current.PredictionData
//                        }
//                    )
//                | msg ->
//                    logws id "Unhandled Message received: %A" msg
//            with
//                | e ->
//                    logws id "Error: Unable to parse received message: %A -  %A" e.Message response.Text
//                    closeAll(".NET error")
//        )
//        |> ignore
//        logws id "Starting client .."
//        //if client.IsRunning then
//        logws id "transforming data to json .."
//        let bytes = createDataInputDTO dro.InitData.Items
//        client.Start() |> ignore
//        logws id "Sending data .."
//        Storage.Storage.Update(id,fun current -> { current with Status = DataResponseStatus.MLRunning(0,current.ItemCount)})
//        client.Send(bytes) |> ignore
            
//        exitEvent.WaitOne() |> ignore
//    }
//    |> Async.Start
