using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("Survey")]
    public class Survey : DbTable
    {
        public int EventCd { get; set; }

        public int MediaCd { get; set; }

        [DefaultValue(1)]
        public int VisitTime { get; set; }

        [DefaultValue(1)]
        public int AgeRange { get; set; }

        [DefaultValue(false)]
        public bool Gender { get; set; }

        public int CareerCd { get; set; }

        [DefaultValue(false)]
        public bool IsMarried { get; set; }

        [DefaultValue(false)]
        public bool LivingStatus { get; set; }

        [DefaultValue(false)]
        public bool IsComeToBuy { get; set; }

        [DefaultValue(false)]
        public bool IsVisitedReception { get; set; }

        [MaxLength(150)]
        public string FavoriteArtist { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [ForeignKey("EventCd")]
        public virtual MstEvent Event { get; set; }

        [ForeignKey("MediaCd")]
        public virtual MstMedia Media { get; set; }

        [ForeignKey("CareerCd")]
        public virtual MstCareer Career { get; set; }

        public virtual ICollection<SurveyArtist> SurveyArtists { get; set; }
    }
}
