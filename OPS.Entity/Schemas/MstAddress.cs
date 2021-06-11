using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("mst_address")]
    public class MstAddress
    {
        [Column("zip1")]
        [MaxLength(3)]
        public string Zip1 { get; set; }

        [MaxLength(4)]
        [Column("zip2")]
        public string Zip2 { get; set; }

        [Column("pref")]
        [MaxLength(12)]
        public string Prefecture { get; set; }

        [Column("city")]
        [MaxLength(36)]
        public string City { get; set; }

        [Column("addr")]
        [MaxLength(72)]
        public string Address { get; set; }

        [Column("pref_kana")]
        [MaxLength(24)]
        public string PrefKana { get; set; }

        [Column("city_kana")]
        [MaxLength(96)]
        public string CityKana { get; set; }

        [Column("addr_kana")]
        [MaxLength(90)]
        public string AddressKana { get; set; }

        [Column("insert_date")]
        public DateTime? InsertDate { get; set; }
    }
}
