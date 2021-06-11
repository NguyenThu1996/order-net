using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstProduct")]
    public class MstProduct : DbTable
    {
        [Required]
        [MaxLength(4)]
        public string Code { get; set; }

        [Required]
        [MaxLength(2)]
        public string ItemCategory { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [MaxLength(120)]
        public string ItemName { get; set; }

        [Required]
        [MaxLength(120)]
        public string OriginalName { get; set; }

        [MaxLength(120)]
        public string JappaneseName { get; set; }

        [Required]
        [MaxLength(240)]
        public string NameKana { get; set; }

        [Required]
        [MaxLength(120)]
        public string CategoryName { get; set; }

        public decimal? Price { get; set; }

        public int ArtistCd { get; set; }

        public int? SortNo { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [ForeignKey("ArtistCd")]
        public virtual MstArtist Artist { get; set; }

        [InverseProperty("Product1")]
        public virtual ICollection<Contract> Product1Contracts { get; set; }

        [InverseProperty("Product2")]
        public virtual ICollection<Contract> Product2Contracts { get; set; }

        [InverseProperty("Product3")]
        public virtual ICollection<Contract> Product3Contracts { get; set; }

        [InverseProperty("Product4")]
        public virtual ICollection<Contract> Product4Contracts { get; set; }

        [InverseProperty("Product5")]
        public virtual ICollection<Contract> Product5Contracts { get; set; }

        [InverseProperty("Product6")]
        public virtual ICollection<Contract> Product6Contracts { get; set; }

        public virtual ICollection<Ranking> Rankings { get; set; }

        public virtual ICollection<ProductTechnique> ProductTechniques { get; set; }
    }
}
