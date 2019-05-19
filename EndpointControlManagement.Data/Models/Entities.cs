
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EndpointControlManagement.Data.Models
{
    public class User
    {
        [Key]
        public System.Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class Role
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class Privilege
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class DataConstraint
    {
        // TODO: Figure out what data constraints look like.
        // I may have some documentation somewhere about this. 
        public string Pattern { get; set; }
    }
    
    public class Endpoint
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public IList<DataConstraint> Constraints { get; set; } = new List<DataConstraint>();
    }

    public class RolePrivilegeMapping
    {
        public System.Guid RoleId { get; set; }
        public System.Guid PrivilegeId { get; set; }
    }

    public class PrivilegeEndpointMapping
    {
        public System.Guid PrivilegeId { get; set; }
        public System.Guid Endpoint { get; set; }
    }
}