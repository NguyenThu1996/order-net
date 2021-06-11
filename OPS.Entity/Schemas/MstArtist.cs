using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstArtist")]
    public class MstArtist : DbTable
    {
        [Required]
        [MaxLength(4)]
        public string Code { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [MaxLength(240)]
        public string NameKana { get; set; }

        [Required]
        [MaxLength(120)]
        public string CategoryName { get; set; }

        [Required]
        [MaxLength(120)]
        public string ItemName { get; set; }

        public virtual ICollection<MstProduct> Products { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [DefaultValue(false)]
        public bool IsFavorited { get; set; }

        [InverseProperty("Artist1")]
        public virtual ICollection<Contract> Artist1Contracts { get; set; }

        [InverseProperty("Artist2")]
        public virtual ICollection<Contract> Artist2Contracts { get; set; }

        [InverseProperty("Artist3")]
        public virtual ICollection<Contract> Artist3Contracts { get; set; }

        [InverseProperty("Artist4")]
        public virtual ICollection<Contract> Artist4Contracts { get; set; }

        [InverseProperty("Artist5")]
        public virtual ICollection<Contract> Artist5Contracts { get; set; }

        [InverseProperty("Artist6")]
        public virtual ICollection<Contract> Artist6Contracts { get; set; }

        public virtual ICollection<BusinessHope> BusinessHopes { get; set; }

        public virtual ICollection<Ranking> Rankings { get; set; }

        public virtual ICollection<PurchaseStatisticsDetail> PurchaseStatisticsDetails { get; set; }

        public virtual ICollection<ArtistDepartment> ArtistDepartments { get; set; }

        public virtual ICollection<SurveyArtist> SurveyArtists { get; set; }
    }
}
