module EndpointControlManagement.Domain.DAL.EndpointControlManagementEventStore
open EndpointControlManagement.Data.Models
open Common.FSharp.Envelopes
open Newtonsoft.Json
open Microsoft.EntityFrameworkCore


type EndpointControlManagementDbContext with 
    member this.GetAggregateEvents<'a,'b when 'b :> EnvelopeEntityBase and 'b: not struct>
        (dbset:EndpointControlManagementDbContext->DbSet<'b>)
        (StreamId.Id (aggregateId):StreamId)
        :seq<Envelope<'a>>= 
        query {
            for event in this |> dbset do
            where (event.StreamId = aggregateId)
            select event
        } |> Seq.map (fun event ->
            {
                Id = event.Id
                UserId = UserId.box event.UserId
                StreamId = StreamId.box aggregateId
                TransactionId = TransId.box event.TransactionId
                Version = Version.box (event.Version)
                Created = event.TimeStamp
                Item = (JsonConvert.DeserializeObject<'a> event.Event)
            })

open EndpointControlManagement.Domain.UserManagement
type UserManagementEventStore () =
    interface IEventStore<UserManagementEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  EndpointControlManagementDbContext ()
            streamId
            |> context.GetAggregateEvents (fun i -> i.UserEvents) 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<UserManagementEvent>) =
            try
                use context = new EndpointControlManagementDbContext ()
                context.UserEvents.Add (
                    UserEventEnvelopeEntity (  Id = envelope.Id,
                                            StreamId = StreamId.unbox envelope.StreamId,
                                            UserId = UserId.unbox envelope.UserId,
                                            TransactionId = TransId.unbox envelope.TransactionId,
                                            Version = Version.unbox envelope.Version,
                                            TimeStamp = envelope.Created,
                                            Event = JsonConvert.SerializeObject(envelope.Item)
                                            )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 


