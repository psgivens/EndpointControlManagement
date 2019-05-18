module EndpointControlManagement.Api.ProcessingSystem

open System
open Akka.Actor
open Akka.FSharp

open EndpointControlManagement.Domain
open Common.FSharp.Envelopes
open EndpointControlManagement.Domain.DomainTypes
open EndpointControlManagement.Domain.UserManagement
open EndpointControlManagement.Domain
open Common.FSharp.Actors

open EndpointControlManagement.Domain.DAL.EndpointControlManagementEventStore
open Common.FSharp.Actors.Infrastructure

open EndpointControlManagement.Domain.DAL.Database
open Akka.Dispatch.SysMsg
open Common.FSharp

open Suave
open Common.FSharp.Suave

type ActorGroups = {
    UserManagementActors:ActorIO<UserManagementCommand>
    }

let composeActors system =
    // Create member management actors
    let userManagementActors = 
        EventSourcingActors.spawn 
            (system,
             "userManagement", 
             UserManagementEventStore (),
             buildState UserManagement.evolve,
             UserManagement.handle,
             DAL.UserManagement.persist)    
             
    { UserManagementActors=userManagementActors }


let initialize () = 
    printfn "Resolve newtonsoft..."

    // System set up
    NewtonsoftHack.resolveNewtonsoft ()  

    printfn "Creating a new database..."
    initializeDatabase ()
    
    let system = Configuration.defaultConfig () |> System.create "sample-system"
            
    printfn "Composing the actors..."
    let actorGroups = composeActors system

    let userCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<UserManagementCommand, UserManagementEvent> 
        system "user_management_command" actorGroups.UserManagementActors

    let runWaitAndIgnore = 
      Async.AwaitTask
      >> Async.Ignore
      >> Async.RunSynchronously

    let userId = UserId.create ()
    let envelop streamId = envelopWithDefaults userId (TransId.create ()) streamId

    printfn "Creating user..."
    { 
        FirstName="Phillip"
        LastName="Givens"
        Email="one@three.com"
    }
    |> UserManagementCommand.Create
    |> envelop (StreamId.create ())
    |> userCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    let user = EndpointControlManagement.Domain.DAL.UserManagement.findUserByEmail "one@three.com"
    printfn "Created User %s with userId %A" user.Email user.Id         

    actorGroups

let actorGroups = initialize ()


type DomainContext = {
  UserId: UserId
  TransId: TransId
}

let inline private addContext (item:DomainContext) (ctx:HttpContext) = 
  { ctx with userState = ctx.userState |> Map.add "domain_context" (box item) }

let inline private getDomainContext (ctx:HttpContext) :DomainContext =
  ctx.userState |> Map.find "domain_context" :?> DomainContext

let authenticationHeaders (p:HttpRequest) = 
  let h = 
    ["user_id"; "transaction_id"]
    |> List.map (p.header >> Option.ofChoice)

  match h with
  | [Some userId; Some transId] -> 
    let (us, uid) = userId |> Guid.TryParse
    let (ut, tid) = transId |> Guid.TryParse
    if us && ut then 
      addContext { 
          UserId = UserId.box uid; 
          TransId = TransId.box tid 
      } 
      >> Some 
      >> async.Return
    else noMatch
  | _ -> noMatch

let envelopWithDefaults (ctx:HttpContext) = 
  let domainContext = getDomainContext ctx
  Common.FSharp.Envelopes.Envelope.envelopWithDefaults
    domainContext.UserId
    domainContext.TransId

let sendEnvelope<'a> (tell:Tell<'a>) (streamId:StreamId) (cmd:'a) (ctx:HttpContext) = 
  cmd
  |> envelopWithDefaults ctx streamId
  |> tell
  
  ctx |> Some |> async.Return 
