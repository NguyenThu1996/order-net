using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("Ranking")]
    public class Ranking : DbTable
    {
        public int MapCd { get; set; }

        public int EventCd { get; set; }

        public int? Rank { get; set; }

        public int? ArtistCd { get; set; }

        public int? ProductCd { get; set; }

        public int? Amount { get; set; }

        [MaxLength(120)]
        public string ArtistName { get; set; }

        [MaxLength(150)]
        public string ProductName { get; set; }

        [ForeignKey("MapCd")]
        public Map Map { get; set; }

        [ForeignKey("EventCd")]
        public MstEvent Event { get; set; }

        [ForeignKey("ArtistCd")]
        public MstArtist Artist { get; set; }

        [ForeignKey("ProductCd")]
        public MstProduct Product { get; set; }
    }
}
