using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("OrderReportCash")]
    public class OrderReportCash : DbTable
    {
        public int OrderReportCd { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        [MaxLength(120)]
        public string CustomerName { get; set; }

        public decimal AmountOfMoney { get; set; }

        [Required]
        [MaxLength(30)]
        public string ReceiptNo { get; set; }

        [ForeignKey("OrderReportCd")]
        public virtual OrderReport OrderReport { get; set; }
    }
}
