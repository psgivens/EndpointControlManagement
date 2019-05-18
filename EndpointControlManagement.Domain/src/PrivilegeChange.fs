module EndpointControlManagement.Domain.PrivilegeChange

open System
open Common.FSharp.CommandHandlers
open Common.FSharp.Envelopes

type PrivilegeActiveState = 
    | Active
    | Archived 

type PrivilegeChangeState = { State: PrivilegeActiveState; Name:string }

type PrivilegeChangeCommand =
    | Create of string
    | Update
    | Archive of string

type PrivilegeChangeEvent = 
    | Created of string
    | Updated of string
    | Archived

let private (|IsArchived|_|) state =
    match state with 
    | Some(value) when value.State = Archived -> Some value
    | _ -> None 

let handle (command:CommandHandlers<PrivilegeChangeEvent, Version>) (state:PrivilegeChangeState option) (cmdenv:Envelope<PrivilegeChangeCommand>) =
    let event =
        match state, cmdenv.Item with 
        | None, Create name -> Created name
        | None, Update _ -> failwith "Cannot update a Privilege which does not exist"
        | None, Archive -> failwith "Cannot archive a Privilege which does not exist"
        | Some _, Create _ -> failwith "Cannot create a Privilege which already exists"
        | IsArchived _, cmd -> failwith <| sprintf "Cannot perform action %A on archived Privilege" cmd
        | Some _, Update name -> Updated name
        | Some _, Archive -> Archived

    event |> command.event

let evolve (state:PrivilegeChangeState option) (event:PrivilegeChangeEvent) =
    match state, event with 
    | None, Created name -> { State=Active; Name=name}
    | None, Updated _ -> failwith "Cannot update a Privilege which does not exist"
    | None, Archived -> failwith "Cannot archive a Privilege which does not exist"
    | Some _, Created _ -> failwith "Cannot create a Privilege which already exists"
    | IsArchived _, _ -> failwith <| sprintf "Cannot perform action %A on archived Privilege" event
    | Some _, Updated name -> { state with Name=name }
    | Some _, Archive -> { state with State=Archived }

