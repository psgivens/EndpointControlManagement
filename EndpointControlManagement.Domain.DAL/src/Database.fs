module EndpointControlManagement.Domain.DAL.Database

open EndpointControlManagement.Data.Models


let initializeDatabase () =
    use context = new EndpointControlManagementDbContext ()
    context.Database.EnsureDeleted () |> ignore
    context.Database.EnsureCreated () |> ignore
