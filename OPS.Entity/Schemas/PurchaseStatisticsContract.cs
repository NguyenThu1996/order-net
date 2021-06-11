using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("PurchaseStatisticsContract")]
    public class PurchaseStatisticsContract : DbTable
    {
        public int PurchaseStatisticsDateCd { get; set; }

        public decimal TotalOfMoney { get; set; }

        [ForeignKey("PurchaseStatisticsDateCd")]
        public virtual PurchaseStatisticsDate PurchaseStatisticsDate { get; set; }
    }
}
