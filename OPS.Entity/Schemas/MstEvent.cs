using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("MstEvent")]
    public class MstEvent : DbTable
    {
        [Required]
        [MaxLength(5)]
        public string Code { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        //[Required]
        [MaxLength(240)]
        public string NameKana { get; set; }

        [MaxLength(50)]
        public string NameAbbr { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        //[Required]
        [MaxLength(150)]
        public string Place { get; set; }

        //[Required]
        [MaxLength(150)]
        public string Address { get; set; }

        [MaxLength(150)]
        public string Decorate { get; set; }

        [MaxLength(200)]
        public string Remark { get; set; }

        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }

        public int? SalesmanCd { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("SalesmanCd")]
        public virtual MstSalesman MainSalesman { get; set; }

        public virtual ICollection<Survey> Surveys { get; set; }

        public virtual ICollection<EventSalesAssigment> EventSalesAssigments { get; set; }

        [InverseProperty("Event")]
        public virtual ICollection<Contract> Contracts { get; set; }

        [InverseProperty("FutureEvent")]
        public virtual ICollection<Contract> ContractsFromOtherEvent { get; set; }

        public virtual ICollection<OrderReport> OrderReports { get; set; }

        public virtual ICollection<Map> Maps { get; set; }

        public virtual ICollection<SalesmanTarget> SalesmanTargets { get; set; }

        public virtual ICollection<PlanMedia> PlanMedias { get; set; }

        public virtual ICollection<EventRequest> EventRequests { get; set; }

        public virtual ICollection<BusinessHope> BusinessHopes { get; set; }

        public virtual ICollection<CustomerInfo> CustomerInfos { get; set; }

        public virtual ICollection<Ranking> Rankings { get; set; }

        public virtual ICollection<PurchaseStatistics> PurchaseStatistics { get; set; }

        public virtual ICollection<EventMedia> EventMedias { get; set; }
    }
}
