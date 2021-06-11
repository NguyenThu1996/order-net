using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstPayment")]
    public class MstPayment : DbTable
    {
        [Required]
        [MaxLength(6)]
        public string Code { get; set; }

        [Required]
        [MaxLength(2)]
        public string Category { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [Required]
        [MaxLength(240)]
        public string NameKn { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
        public virtual ICollection<Contract> Contracts { get; set; }
    }
}
