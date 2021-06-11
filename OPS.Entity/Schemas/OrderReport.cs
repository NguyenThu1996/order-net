using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("OrderReport")]
    public class OrderReport : DbTable
    {
        public DateTime HoldTime { get; set; }

        public int PersonInChargeCd { get; set; }

        public int LastConfirmSalesmanCd { get; set; }

        public int EventCd { get; set; }

        public decimal PreviousDayOrderCash { get; set; }

        public decimal SameDayOrderCash { get; set; }

        public int FinalNumberOfCustomers { get; set; }

        public decimal? TotalReceivedCash { get; set; }

        public decimal? TotalDownPayment { get; set; }

        [ForeignKey("PersonInChargeCd")]
        public virtual MstSalesman PersonInCharge { get; set; }

        [ForeignKey("LastConfirmSalesmanCd")]
        public virtual MstSalesman LastConfirmSalesman { get; set; }

        [ForeignKey("EventCd")]
        public virtual MstEvent Event { get; set; }

        [ForeignKey("InsertUserId")]
        public virtual ApplicationUser InsertUser { get; set; }

        public virtual ICollection<OrderReportCash> OrderReportCashes { get; set; }

        public virtual ICollection<OrderReportDetail> OrderReportDetails { get; set; }
    }
}
