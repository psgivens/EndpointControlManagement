module EndpointControlManagement.Domain.PrivilegeChange

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes

type PrivilegeActiveState = 
    | Active
    | Archived 

type PrivilegeEndpoint = { Id: Guid; Name: string }

type PrivilegeChangeState = { 
    State: PrivilegeActiveState; 
    Name:string; 
    Endpoints: PrivilegeEndpoint list }
    

type PrivilegeChangeCommand =
    | Create of string
    | Update
    | AddEndpoint of Guid * string
    | RemoveEndpoint of Guid
    | Archive of string

type PrivilegeChangeEvent = 
    | Created of string
    | Updated of string
    | EndpointAdded of Guid * string
    | EndpointRemoved of Guid
    | Archived

let private (|IsArchived|_|) state =
    match state with 
    | Some(value) when value.State = Archived -> Some value
    | _ -> None 

let remove id values =
    values |> List.filter     

let handle (command:CommandHandlers<PrivilegeChangeEvent, Version>) (state:PrivilegeChangeState option) (cmdenv:Envelope<PrivilegeChangeCommand>) =
    let event =
        match state, cmdenv.Item with 
        | None, Create name -> Created name
        | None, Update _ -> failwith "Cannot update a Privilege which does not exist"
        | None, Archive -> failwith "Cannot archive a Privilege which does not exist"
        | Some _, Create _ -> failwith "Cannot create a Privilege which already exists"
        | IsArchived _, cmd -> failwith <| sprintf "Cannot perform action %A on archived Privilege" cmd
        | Some _, Update name -> Updated name
        | Some _, AddEndpoint endpoint -> EndpointAdded endpoint
        | Some _, RemoveEndpoint id -> EndpointRemoved id
        | Some _, Archive -> Archived

    event |> command.event

let remove id list =
    let notMatch (item:PrivilegeEndpoint) = id <> item.Id
    list |> List.filter notMatch

let evolve (state:PrivilegeChangeState option) (event:PrivilegeChangeEvent) =
    match state, event with 
    | None, Created name -> { State=Active; Name=name; Endpoints=[] }
    | None, Updated _ -> failwith "Cannot update a Privilege which does not exist"
    | None, Archived -> failwith "Cannot archive a Privilege which does not exist"
    | Some _, Created _ -> failwith "Cannot create a Privilege which already exists"
    | IsArchived _, _ -> failwith <| sprintf "Cannot perform action %A on archived Privilege" event
    | Some _, Updated name -> { state with Name=name }
    | Some _, EndpointAdded endpoint -> { state with Endpoints = endpoint::state.Endpoints }
    | Some _, EndpointRemoved id -> { state with Endpoints = state.Endpoints |> remove id } 
    | Some _, Archive -> { state with State=Archived }

