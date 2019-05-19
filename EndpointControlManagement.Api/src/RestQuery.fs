module EndpointControlManagement.Api.RestQuery

open EndpointControlManagement.Api.Dtos

open Suave
open Suave.Successful

open Common.FSharp.Suave
open EndpointControlManagement.Domain

let getEndpoint endpointName =
  DAL.EndpointChange.findEndpointByName endpointName
  |> convertToDto
  |> toJson 
  |> OK

let getEndpoints (ctx:HttpContext) =
  DAL.EndpointChange.getAllEndpoints ()
  |> List.map convertToDto
  |> toJson
  |> OK

