using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstSalesman")]
    public class MstSalesman : DbTable
    {
        [Required]
        [MaxLength(6)]
        public string Code { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [MaxLength(240)]
        public string NameKana { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        public virtual ICollection<EventSalesAssigment> EventSalesAssigments { get; set; }

        public virtual ICollection<MstEvent> Events { get; set; }

        [InverseProperty("SalesmanS")]
        public virtual ICollection<Contract> SRespContracts { get; set; }

        [InverseProperty("SalesmanC")]
        public virtual ICollection<Contract> CRespContracts { get; set; }

        [InverseProperty("SalesmanA")]
        public virtual ICollection<Contract> ARespContracts { get; set; }

        [InverseProperty("PersonInCharge")]
        public virtual ICollection<OrderReport> RespOrderReports { get; set; }

        [InverseProperty("LastConfirmSalesman")]
        public virtual ICollection<OrderReport> ConfirmOrderReports { get; set; }

        [InverseProperty("SalesmanS")]
        public virtual ICollection<OrderReportDetail> SRespReportDetails { get; set; }

        [InverseProperty("SalesmanC")]
        public virtual ICollection<OrderReportDetail> CRespReportDetails { get; set; }

        [InverseProperty("SalesmanA")]
        public virtual ICollection<OrderReportDetail> ARespReportDetails { get; set; }

        public virtual ICollection<SalesmanTarget> SalesmanTargets { get; set; }
    }
}
