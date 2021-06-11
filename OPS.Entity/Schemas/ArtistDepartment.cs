using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("ArtistDepartment")]
    public class ArtistDepartment
    {
        public int DepartmentCd { get; set; }

        public int ArtistCd { get; set; }

        public virtual MstDepartment Department { get; set; }

        public virtual MstArtist Artist { get; set; }
    }
}
