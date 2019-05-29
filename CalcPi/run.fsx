#load "CalcPi.fsx"

#if !COMPILED

// #I @"C:/Program Files/nodejs/node_modules/azure-functions-core-tools/bin/"
#I @"C:/ProgramData/chocolatey/lib/azure-functions-core-tools/tools/"

#r "Microsoft.Azure.Webjobs.Host.dll"
open Microsoft.Azure.WebJobs.Host

#r "System.Net.Http.Formatting.dll"
#r "System.Web.Http.dll"
#r "System.Net.Http.dll"
#r "Newtonsoft.Json.dll"

#else

#r "System.Net.Http"
#r "Newtonsoft.Json"

#endif

open System.Net
open System.Net.Http
open Newtonsoft.Json
open CalcPi

type Inputs = {
    Points: int
    Iterations: int
}

type Result = {
    Pi: string
}

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        log.Info("Webhook was triggered!")
        let! jsonContent = req.Content.ReadAsStringAsync() |> Async.AwaitTask
        let jsonFormatter = System.Net.Http.Formatting.JsonMediaTypeFormatter()
        jsonFormatter.SerializerSettings.ContractResolver 
            <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        try
            let inputs = JsonConvert.DeserializeObject<Inputs>(jsonContent)
            let pi = averagePi inputs.Points inputs.Iterations
            return req.CreateResponse(
                    HttpStatusCode.OK, 
                    { Pi = sprintf "Pi for %i points and %i iterations: %f" inputs.Points inputs.Iterations pi },
                    jsonFormatter
                )
        with ex ->
            log.Error(ex.Message)
            return req.CreateResponse(HttpStatusCode.BadRequest)
    } |> Async.StartAsTask
