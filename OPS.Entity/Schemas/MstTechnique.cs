using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstTechnique")]
    public class MstTechnique : DbTable
    {
        [Required]
        [MaxLength(4)]
        public string Code { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [MaxLength(240)]
        public string NameKana { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [InverseProperty("Technique1")]
        public virtual ICollection<Contract> Technique1Contracts { get; set; }

        [InverseProperty("Technique2")]
        public virtual ICollection<Contract> Technique2Contracts { get; set; }

        [InverseProperty("Technique3")]
        public virtual ICollection<Contract> Technique3Contracts { get; set; }

        [InverseProperty("Technique4")]
        public virtual ICollection<Contract> Technique4Contracts { get; set; }

        [InverseProperty("Technique5")]
        public virtual ICollection<Contract> Technique5Contracts { get; set; }

        [InverseProperty("Technique6")]
        public virtual ICollection<Contract> Technique6Contracts { get; set; }

        public virtual ICollection<ProductTechnique> ProductTechniques { get; set; }
    }
}
