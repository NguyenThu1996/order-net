using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OPS.Entity.Schemas
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(256)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(256)]
        public string LastName { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        public virtual MstEvent Event { get; set; }

        public virtual ICollection<OrderReport> OrderReports { get; set; }
    }
}
