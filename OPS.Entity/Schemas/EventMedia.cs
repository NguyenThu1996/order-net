using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("EventMedia")]
    public class EventMedia
    {
        public int EventCd { get; set; }

        public int MediaCd { get; set; }

        public virtual MstEvent Event { get; set; }

        public virtual MstMedia Media { get; set; }

        public DateTime? InsertDate { get; set; }

        [MaxLength(255)]
        public string InsertUserId { get; set; }

        public DateTime? UpdateDate { get; set; }

        [MaxLength(255)]
        public string UpdateUserId { get; set; }
    }
}
