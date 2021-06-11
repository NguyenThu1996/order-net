using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace OPS.Entity.Schemas
{
    public class ApplicationRole : IdentityRole
    {
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
