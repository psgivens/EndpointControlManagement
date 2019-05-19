module EndpointControlManagement.Api.Dtos

open EndpointControlManagement.Data.Models

type EndpointDto = { 
    id : string
    name : string 
    url : string
    method : string }

let convertToDto (endpoint:Endpoint) = { 
  EndpointDto.name = endpoint.Name
  url = endpoint.Url
  method = endpoint.Method
  id = endpoint.Id.ToString () }
