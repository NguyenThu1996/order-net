using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("mst_tel")]
    public class MstTelephone
    {
        [Column("number")]
        [MaxLength(6)]
        public string Number { get; set; }

        [Column("area_code")]
        [MaxLength(6)]
        public string AreaCode { get; set; }

        [Column("city_code")]
        [MaxLength(4)]
        public string CityCode { get; set; }
    }
}
