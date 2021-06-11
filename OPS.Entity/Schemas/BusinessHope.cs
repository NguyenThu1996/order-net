using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("BusinessHope")]
    public class BusinessHope : DbTable
    {
        public int MapCd { get; set; }

        public int EventCd { get; set; }

        public int? ArtistCd { get; set; }

        [MaxLength(20)]
        public string Type { get; set; }

        [MaxLength(30)]
        public string Desgin { get; set; }

        public int? DesiredNumber { get; set; }

        [MaxLength(50)]
        public string Remark { get; set; }

        [ForeignKey("MapCd")]
        public Map Map { get; set; }

        [ForeignKey("EventCd")]
        public MstEvent Event { get; set; }

        [ForeignKey("ArtistCd")]
        public MstArtist Artist { get; set; }
    }
}
