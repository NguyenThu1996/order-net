using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("EventRequest")]
    public class EventRequest : DbTable
    {
        public int MapCd { get; set; }

        public int EventCd { get; set; }

        [MaxLength(40)]
        public string Name { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; }

        [ForeignKey("MapCd")]
        public Map Map { get; set; }

        [ForeignKey("EventCd")]
        public MstEvent Event { get; set; }
    }
}
