using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("PurchaseStatistics")]
    public class PurchaseStatistics : DbTable
    {
        public int EventCd { get; set; }

        [Required]
        [MaxLength(50)]
        public string PeriodTime { get; set; }

        public int Type { get; set; }

        public int? EventManagerCd { get; set; }

        public int? InputPersonCd { get; set; }

        [ForeignKey("EventCd")]
        public virtual MstEvent Event { get; set; }

        [ForeignKey("InsertUserId")]
        public virtual ApplicationUser InsertUser { get; set; }

        public virtual ICollection<PurchaseStatisticsDetail> PurchaseStatisticsDetails { get; set; }
    }
}
