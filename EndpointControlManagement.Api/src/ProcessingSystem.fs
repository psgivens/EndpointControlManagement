module EndpointControlManagement.Api.ProcessingSystem

open System
open Akka.Actor
open Akka.FSharp

open EndpointControlManagement.Domain
open Common.FSharp.Envelopes
open EndpointControlManagement.Domain.DomainTypes
open EndpointControlManagement.Domain.EndpointChange
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
    EndpointChangeActors:ActorIO<EndpointChangeCommand>
    }

let composeActors system =
    // Create member management actors
    let endpointChangeActors = 
        EventSourcingActors.spawn 
            (system,
             "EndpointChange", 
             EndpointChangeEventStore (),
             buildState EndpointChange.evolve,
             EndpointChange.handle,
             DAL.EndpointChange.persist)    
             
    { EndpointChangeActors=endpointChangeActors }


let initialize () = 
    printfn "Resolve newtonsoft..."

    // System set up
    NewtonsoftHack.resolveNewtonsoft ()  

    printfn "Creating a new database..."
    initializeDatabase ()
    
    let system = Configuration.defaultConfig () |> System.create "sample-system"
            
    printfn "Composing the actors..."
    let actorGroups = composeActors system

    let endpointCommandRequestReplyCanceled = 
      RequestReplyActor.spawnRequestReplyActor<EndpointChangeCommand, EndpointChangeEvent> 
        system "endpoint_management_command" actorGroups.EndpointChangeActors

    let runWaitAndIgnore = 
      Async.AwaitTask
      >> Async.Ignore
      >> Async.RunSynchronously

    let userId = UserId.create ()
    let envelop streamId = envelopWithDefaults userId (TransId.create ()) streamId

    printfn "Creating endpoint..."
    { 
      EndpointDetails.Url = "/sample/url"
      EndpointDetails.Method = EndpointMethod.GET
      EndpointDetails.Name = "sample url"
    }
    |> EndpointChangeCommand.Create
    |> envelop (StreamId.create ())
    |> endpointCommandRequestReplyCanceled.Ask
    |> runWaitAndIgnore

    let endpoint = EndpointControlManagement.Domain.DAL.EndpointChange.findEndpointByName "sample url"
    printfn "Created Endpoint %s with endpointId %A" endpoint.Name endpoint.Id         

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
