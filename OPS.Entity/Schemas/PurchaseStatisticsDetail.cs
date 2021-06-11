using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("PurchaseStatisticsDetail")]
    public class PurchaseStatisticsDetail : DbTable
    {
        public int PurchaseStatisticsCd { get; set; }

        public int? MediaCd { get; set; }

        public int? ArtistCd { get; set; }

        [MaxLength(120)]
        public string ArtistName { get; set; }

        [ForeignKey("PurchaseStatisticsCd")]
        public virtual PurchaseStatistics PurchaseStatistics { get; set; }

        [ForeignKey("MediaCd")]
        public virtual MstMedia Media { get; set; }

        [ForeignKey("ArtistCd")]
        public virtual MstArtist Artist { get; set; }

        public virtual ICollection<PurchaseStatisticsDate> PurchaseStatisticsDates { get; set; }
    }
}
