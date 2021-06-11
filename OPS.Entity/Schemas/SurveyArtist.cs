using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("SurveyArtist")]
    public class SurveyArtist
    {
        public int SurveyCd { get; set; }

        public int ArtistCd { get; set; }

        public virtual Survey Survey { get; set; }

        public virtual MstArtist Artist { get; set; }
    }
}
