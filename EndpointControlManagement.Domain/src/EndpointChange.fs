module EndpointControlManagement.Domain.EndpointChange

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes

type EndpointActiveState = 
    | Active
    | Archived 

type EndpointMethod = 
    | GET
    | POST
    | DELETE
    | PUT

type EndpointDetails = {
    Name: string
    Url: string
    Method: EndpointMethod
}

type EndpointChangeState = { State: EndpointActiveState; Endpoint: EndpointDetails }

type EndpointChangeCommand =
    | Create of EndpointDetails
    | Update of EndpointDetails
    | Archive

type EndpointChangeEvent = 
    | Created of EndpointDetails
    | Updated of EndpointDetails
    | Archived

let private (|IsArchived|_|) state =
    match state with 
    | Some(value) when value.State = EndpointActiveState.Archived -> Some value
    | _ -> None 

let handle (command:CommandHandlers<EndpointChangeEvent, Version>) (state:EndpointChangeState option) (cmdenv:Envelope<EndpointChangeCommand>) =    
    match state, cmdenv.Item with 
    | None, Create details -> Created details |> command.event
    | None, Update _ -> failwith "Cannot update a Endpoint which does not exist"
    | None, Archive -> failwith "Cannot archive a Endpoint which does not exist"
    | Some _, Create _ -> failwith "Cannot create a Endpoint which already exists"
    | IsArchived _, cmd -> failwith <| sprintf "Cannot perform action %A on archived Endpoint" cmd
    | Some _, Update details -> Updated details |> command.event
    | Some _, Archive -> Archived |> command.event


let evolve (state:EndpointChangeState option) (event:EndpointChangeEvent) =
    match state, event with 
    | None, Created details -> { State=Active; Endpoint=details}
    | None, Updated _ -> failwith "Cannot update a Endpoint which does not exist"
    | None, Archived -> failwith "Cannot archive a Endpoint which does not exist"
    | Some _, Created _ -> failwith "Cannot create a Endpoint which already exists"
    | IsArchived _, _ -> failwith <| sprintf "Cannot perform action %A on archived Endpoint" event
    | Some state', Updated details -> { state' with Endpoint=details }
    | Some state', Archived -> { state' with State=EndpointActiveState.Archived }

