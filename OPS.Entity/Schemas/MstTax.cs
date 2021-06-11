using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OPS.Entity.Schemas
{
    [Table("MstTax")]
    public class MstTax : DbTable
    {
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Value { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
    }
}
