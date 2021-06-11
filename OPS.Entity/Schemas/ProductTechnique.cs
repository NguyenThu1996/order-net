using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("ProductTechnique")]
    public class ProductTechnique
    {
        public int ProductCd { get; set; }

        public int TechniqueCd { get; set; }

        public virtual MstProduct Product { get; set; }

        public virtual MstTechnique Technique { get; set; }
    }
}
