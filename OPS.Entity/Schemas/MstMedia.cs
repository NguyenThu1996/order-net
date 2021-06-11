using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstMedia")]
    public class MstMedia : DbTable
    {
        [Required]
        [MaxLength(2)]
        public string Code { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [MaxLength(240)]
        public string NameKana { get; set; }

        public int? SortNo { get; set; }

        [MaxLength(2)]
        public string BranchCode { get; set; }

        [MaxLength(30)]
        public string Spec { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        public int Flag { get; set; }

        public virtual ICollection<Survey> Surveys { get; set; }

        public virtual ICollection<Contract> Contracts { get; set; }

        public virtual ICollection<OrderReportDetail> OrderReportDetails { get; set; }

        public virtual ICollection<PlanMedia> PlanMedias { get; set; }

        public virtual ICollection<PurchaseStatisticsDetail> PurchaseStatisticsDetails { get; set; }

        public virtual ICollection<EventMedia> EventMedias { get; set; }
    }
}
