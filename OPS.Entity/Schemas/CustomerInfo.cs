using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("CustomerInfo")]
    public class CustomerInfo : DbTable
    {
        public int MapCd { get; set; }

        public int EventCd { get; set; }

        public int? Credit { get; set; }

        public int? Cash { get; set; }

        public int? Card { get; set; }

        public int? FisrtTime { get; set; }

        public int? SecondTime { get; set; }

        public int? ManyTime { get; set; }

        public decimal? AverageIncome { get; set; }

        public int? U20Male { get; set; }

        public int? U20Female { get; set; }

        public int? U30Male { get; set; }

        public int? U30Female { get; set; }

        public int? U40Male { get; set; }

        public int? U40Female { get; set; }

        public int? U50Male { get; set; }

        public int? U50Female { get; set; }

        public int? U60Male { get; set; }

        public int? U60Female { get; set; }

        public int? U70Male { get; set; }

        public int? U70Female { get; set; }

        public int? U80Male { get; set; }

        public int? U80Female { get; set; }

        [ForeignKey("MapCd")]
        public Map Map { get; set; }

        [ForeignKey("EventCd")]
        public MstEvent Event { get; set; }
    }
}
