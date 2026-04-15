using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Domain.Common
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; protected set; }

        public string? CreatedBy { get; protected set; }

        public DateTime? ModifiedAt { get; protected set; }

        public string? ModifiedBy { get; protected set; }

        protected void SetCreated(string user)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = user;
        }

        protected void SetModified(string user)
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = user;
        }
    }
}
