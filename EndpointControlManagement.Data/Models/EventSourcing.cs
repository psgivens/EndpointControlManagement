using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndpointControlManagement.Data.Models
{
    public class EventStream
    {
        [Column(Order = 0), Key]
        public virtual long Id { get; set; }
        public virtual System.DateTimeOffset Created { get; set; }
    }
    public abstract class EnvelopeEntityBase
    {
        public virtual System.Guid StreamId { get; set; }
        public virtual EventStream Stream { get; set; }
        public virtual System.Guid UserId { get; set; }
        public virtual System.Guid Id { get; set; }

        public virtual System.Guid TransactionId { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual short Version { get; set; }
        public virtual System.DateTimeOffset TimeStamp { get; set; }
        public virtual string Event { get; set; }
    }
    public class RoleEventEnvelopeEntity : EnvelopeEntityBase { }
    public class PrivilegeEventEnvelopeEntity : EnvelopeEntityBase { }
    public class EndpointEventEnvelopeEntity : EnvelopeEntityBase { }
    public class DataConstraintEventEnvelopeEntity : EnvelopeEntityBase { }
    public class RolePrivMapEventEnvelopeEntity : EnvelopeEntityBase { }
    public class PrivEPMapEventEnvelopeEntity : EnvelopeEntityBase { }
}
