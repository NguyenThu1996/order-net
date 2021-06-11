using OPS.ViewModels.Admin.Survey;
using OPS.ViewModels.User.Survey;
using System.Collections.Generic;

namespace OPS.Business.IRepository
{
    public interface ISurveyRepository
    {
        public bool CreateSurvey(SurveyViewModel survey, string userId, string eventCode);
        public SurveyListUserModel LoadListSurveyUser(SurveyListUserModel surveyList);
        public SurveyListAdminModel LoadListSurveyAdmin(SurveyListAdminModel surveyList);
        public int GetEventCd(string userId);
        List<SurveyArtistModel> GetListFavoriteArtist();
    }
}