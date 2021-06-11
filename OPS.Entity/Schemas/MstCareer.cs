using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstCareer")]
    public class MstCareer : DbTable
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [MaxLength(240)]
        public string NameKana { get; set; }

        [Required]
        [MaxLength(4)]
        public string Code { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        public virtual ICollection<Survey> Surveys { get; set; }
    }
}
