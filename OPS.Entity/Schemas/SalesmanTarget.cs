using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("SalesmanTarget")]
    public class SalesmanTarget : DbTable
    {
        public int MapCd { get; set; }

        public int EventCd { get; set; }

        public int SalesmanCd { get; set; }

        public int? TargetAmount { get; set; }

        public decimal? TargetEvenue { get; set; }

        public int? ResultAmount { get; set; }

        public decimal? ResultEvenue { get; set; }

        public int? EffortTargetAmount { get; set; }

        public decimal? EffortTargetEvenue { get; set; }

        public int? ResultEffortAmount { get; set; }

        public decimal? ResultEffortEvenue { get; set; }

        public int? CertainTargetAmount { get; set; }

        public int? ResultCertainAmount { get; set; }

        [ForeignKey("EventCd")]
        public MstEvent Event { get; set; }

        [ForeignKey("MapCd")]
        public Map Map { get; set; }

        [ForeignKey("SalesmanCd")]
        public MstSalesman Salesman { get; set; }
    }
}
