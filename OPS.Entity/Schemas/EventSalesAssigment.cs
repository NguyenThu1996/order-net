using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("EventSalesAssigment")]
    public class EventSalesAssigment
    {
        public int EventCd { get; set; }

        public int SalesmanCd { get; set; }

        public int? NewTargetAmount { get; set; }

        public decimal? NewTargetRevenue { get; set; }

        public int? EffortTargetAmount { get; set; }

        public decimal? EffortTargetRevenue { get; set; }

        public virtual MstEvent Event { get; set; }

        public virtual MstSalesman Salesman { get; set; }

        public DateTime? InsertDate { get; set; }

        [MaxLength(255)]
        public string InsertUserId { get; set; }

        public DateTime? UpdateDate { get; set; }

        [MaxLength(255)]
        public string UpdateUserId { get; set; }
    }
}
