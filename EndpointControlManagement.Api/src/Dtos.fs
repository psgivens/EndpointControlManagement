module EndpointControlManagement.Api.Dtos

open EndpointControlManagement.Data.Models

type UserDto = { 
    id : string
    first_name : string 
    last_name : string
    email : string }

let convertToDto (user:User) = { 
  UserDto.email = user.Email
  first_name = user.FirstName
  last_name = user.LastName
  id = user.Id.ToString () }
