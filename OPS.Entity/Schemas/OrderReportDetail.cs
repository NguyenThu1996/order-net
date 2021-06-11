using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("OrderReportDetail")]
    public class OrderReportDetail : DbTable
    {
        public int? OrderReportCd { get; set; }

        public int? MediaCd { get; set; }

        [MaxLength(60)]
        public string VerifyNumber { get; set; }

        public int? LeftPaymentMethod { get; set; }

        public int? SalesmanSCd { get; set; }

        public int? SalesmanCCd { get; set; }

        public int? SalesmanACd { get; set; }

        [MaxLength(120)]
        public string Name { get; set; }

        [MaxLength(120)]
        public string NameFuri { get; set; }

        [MaxLength(8)]
        public string Zipcode { get; set; }

        [MaxLength(16)]
        public string HomePhone { get; set; }

        [MaxLength(120)]
        public string AuthorName { get; set; }

        [MaxLength(150)]
        public string ProductName { get; set; }

        [MaxLength(200)]
        public string Item { get; set; }

        public decimal? Price { get; set; }

        public decimal? Discount { get; set; }

        public decimal? TaxPrice { get; set; }

        public decimal? ReceivedCash { get; set; }

        public decimal? DownPayment { get; set; }

        public int? DownPaymentMethod { get; set; }

        [MaxLength(120)]
        public string CashKeeper { get; set; }

        public int? AvClub { get; set; }

        public int? NumberOfVisit { get; set; }

        public DateTime? DeliverDate { get; set; }

        public int? JxClub { get; set; }

        [MaxLength(120)]
        public string Area { get; set; }

        [MaxLength(130)]
        public string Remarks { get; set; }

        [MaxLength(20)]
        public string CashVoucherValue { get; set; }

        public int? ContractCd { get; set; }

        public int? ProductNo { get; set; }

        [ForeignKey("OrderReportCd")]
        public virtual OrderReport OrderReport { get; set; }

        [ForeignKey("MediaCd")]
        public virtual MstMedia Media { get; set; }

        [ForeignKey("SalesmanSCd")]
        public virtual MstSalesman SalesmanS { get; set; }

        [ForeignKey("SalesmanCCd")]
        public virtual MstSalesman SalesmanC { get; set; }

        [ForeignKey("SalesmanACd")]
        public virtual MstSalesman SalesmanA { get; set; }
    }
}
