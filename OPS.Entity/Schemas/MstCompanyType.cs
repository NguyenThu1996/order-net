using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstCompanyType")]
    public class MstCompanyType : DbTable
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [Required]
        [MaxLength(10)]
        public string Code { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        public virtual ICollection<Contract> Contracts { get; set; }
    }
}
