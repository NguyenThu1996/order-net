using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("PlanMedia")]
    public class PlanMedia : DbTable
    {
        public int MapCd { get; set; }

        public int EventCd { get; set; }

        public int MediaCd { get; set; }

        [MaxLength(50)]
        public string Spec { get; set; }

        public int? OrderAmount { get; set; }

        public decimal? Cost { get; set; }

        public int? NumberOfCustomers { get; set; }

        public int? AttractCustomers { get; set; }

        public decimal? EstimatedRevenue { get; set; }

        public int? ResultAmount { get; set; }

        public decimal? ResultRevenue { get; set; }

        [MaxLength(16)]
        public string Unit { get; set; }

        public bool IsPrinted { get; set; }

        [ForeignKey("MapCd")]
        public Map Map { get; set; }

        [ForeignKey("EventCd")]
        public MstEvent Event { get; set; }

        [ForeignKey("MediaCd")]
        public MstMedia Media { get; set; }
    }
}
