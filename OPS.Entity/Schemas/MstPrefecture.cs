using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("mst_pref")]
    public class MstPrefecture
    {
        [Column("code")]
        public int Code { get; set; }

        [Column("pref_name")]
        [MaxLength(20)]
        public string Name { get; set; }
    }
}
