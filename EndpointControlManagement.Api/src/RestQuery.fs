module EndpointControlManagement.Api.RestQuery

open EndpointControlManagement.Api.Dtos

open Suave
open Suave.Successful

open Common.FSharp.Suave
open EndpointControlManagement.Domain

let getUser userIdString =
  DAL.UserManagement.findUserByEmail userIdString
  |> convertToDto
  |> toJson 
  |> OK

let getUsers (ctx:HttpContext) =
  DAL.UserManagement.getAllUsers ()
  |> List.map convertToDto
  |> toJson
  |> OK

