using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("PurchaseStatisticsDate")]
    public class PurchaseStatisticsDate : DbTable
    {
        public int PurchaseStatisticsDetailCd { get; set; }

        public int AttractingCustomers { get; set; }

        public DateTime HoldDate { get; set; }

        public int NumberOfContracts { get; set; }

        public decimal TotalPrice { get; set; }

        [ForeignKey("PurchaseStatisticsDetailCd")]
        public virtual PurchaseStatisticsDetail PurchaseStatisticsDetail { get; set; }

        public virtual ICollection<PurchaseStatisticsContract> PurchaseStatisticsContracts { get; set; }
    }
}
