using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("Map")]
    public class Map : DbTable
    {
        public int EventCd { get; set; }

        public DateTime? Period { get; set; }

        public int? Week { get; set; }

        [MaxLength(50)]
        public string Plan { get; set; }

        [MaxLength(50)]
        public string Decoration { get; set; }

        [MaxLength(50)]
        public string Precaution { get; set; }

        [MaxLength(100)]
        public string TransportExpense { get; set; }

        public int? EventTargetAmount { get; set; }

        public decimal? EventTargetRevenue { get; set; }

        public int? NewTargetAmount { get; set; }

        public decimal? NewTargetRevenue { get; set; }

        public int? EffortTargetAmount { get; set; }

        public decimal? EffortTargetRevenue { get; set; }

        public decimal? BreakEvenPoint { get; set; }

        public decimal? SaleResult { get; set; }

        public decimal? NewResult { get; set; }

        public decimal? EffortResult { get; set; }

        public decimal? Profit { get; set; }

        public int? ClubMemberTarget { get; set; }

        public int? ClubMemberJoin { get; set; }

        public bool? IsLayoutChecked { get; set; }

        public bool? IsLayoutSubmitted { get; set; }

        public DateTime? ProductExDeadline { get; set; }

        [MaxLength(50)]
        public string ProductExResponsible { get; set; }

        public int? ProductExAmount { get; set; }

        [MaxLength(255)]
        public string CallArea { get; set; }

        [MaxLength(255)]
        public string Progress { get; set; }

        [MaxLength(512)]
        public string ProductIdea { get; set; }

        [MaxLength(512)]
        public string StrategiesAndTactics { get; set; }

        [MaxLength(512)]
        public string VenueAnalysis { get; set; }

        public int? MarginalProfitRatio { get; set; }

        [MaxLength(30)]
        public string MonthWeek { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [ForeignKey("EventCd")]
        public MstEvent Event { get; set; }

        public virtual ICollection<SalesmanTarget> SalesmanTargets { get; set; }

        public virtual ICollection<PlanMedia> PlanMedias { get; set; }

        public virtual ICollection<EventRequest> EventRequests { get; set; }

        public virtual ICollection<BusinessHope> BusinessHopes { get; set; }

        public virtual ICollection<CustomerInfo> CustomerInfos { get; set; }

        public virtual ICollection<Ranking> Rankings { get; set; }
    }
}
