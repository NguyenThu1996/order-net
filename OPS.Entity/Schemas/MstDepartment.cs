using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstDepartment")]
    public class MstDepartment : DbTable
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [Required]
        [MaxLength(240)]
        public string NameKana { get; set; }

        [Required]
        [MaxLength(1)]
        public string Code { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        public virtual ICollection<ArtistDepartment> ArtistDepartments { get; set; }

        [InverseProperty("Department1")]
        public virtual ICollection<Contract> Department1Contracts { get; set; }

        [InverseProperty("Department2")]
        public virtual ICollection<Contract> Department2Contracts { get; set; }

        [InverseProperty("Department3")]
        public virtual ICollection<Contract> Department3Contracts { get; set; }

        [InverseProperty("Department4")]
        public virtual ICollection<Contract> Department4Contracts { get; set; }

        [InverseProperty("Department5")]
        public virtual ICollection<Contract> Department5Contracts { get; set; }

        [InverseProperty("Department6")]
        public virtual ICollection<Contract> Department6Contracts { get; set; }
    }
}
