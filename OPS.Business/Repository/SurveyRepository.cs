using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Survey;
using OPS.ViewModels.Shared;
using OPS.ViewModels.User.Survey;
using System;
using System.Collections.Generic;
using System.Linq;
using static OPS.Utility.Constants;

namespace OPS.Business.Repository
{
    public class SurveyRepository : ISurveyRepository
    {
        protected readonly OpsContext _context;
        public SurveyRepository(OpsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Function to Submit Survey in to database
        /// </summary>
        /// <param name="survey"></param>
        /// <param name="userId"></param>
        /// <param name="eventCode"></param>
        /// <returns></returns>
        public bool CreateSurvey(SurveyViewModel survey, string userId, string eventCode)
        {
            try
            {
                //Get EventCd
                var eventCd = _context.MstEvents.Where(x => x.Code == eventCode && !x.IsDeleted).Select(x => x.Cd).FirstOrDefault();

                //Create new Survey object
                var entity  = new Survey
                {
                    EventCd            = eventCd,
                    MediaCd            = survey.MediaCd,
                    VisitTime          = survey.VisitTime,
                    AgeRange           = survey.AgeRange,
                    Gender             = survey.Gender       == (int) Constants.Gender.Male,
                    CareerCd           = survey.CareerCd,
                    IsMarried          = survey.IsMarried    == (int) Constants.IsMarried.Yes,
                    LivingStatus       = survey.LivingStatus == (int) Constants.LivingStatus.Alone,
                    IsComeToBuy        = survey.IsComeToBuy  == (int) Constants.IsComeToBuy.Yes,
                    FavoriteArtist     = survey.FavoriteArtist,
                    //IsDeleted          = Convert.ToBoolean(Constants.IsDeleted.No),
                    //UpdateDate         = DateTime.Now,
                    //UpdateUserId       = userId,
                    InsertDate         = DateTime.Now,
                    InsertUserId       = userId,
                    
                    SurveyArtists      = AddListFavoriteArtists(survey),
                };

                //Add new survey into database
                _context.Surveys.Add(entity);
                _context.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<SurveyArtist> AddListFavoriteArtists(SurveyViewModel model)
        {
            return model.ListFavoriteArtists.Where(x => x.IsFavorited && x.ArtistCd > 0)
                        .Select(y => new SurveyArtist
                        {
                            ArtistCd = y.ArtistCd,
                        }).ToList();
        }

        /// <summary>
        /// This method to get list Survey from DB and put it into Admin/List Survey
        /// </summary>
        /// <param name="surveyList"></param>
        /// <returns></returns>
        public SurveyListAdminModel LoadListSurveyAdmin(SurveyListAdminModel surveyList)
        {
            try
            {
                var listSurveyResult = new SurveyListAdminModel();
                var surveysIQuery    = _context.Surveys.Where(x => !x.IsDeleted);

                //Search survey with eventCd if eventCd is not null
                if (surveyList.EventCd != 0)
                {
                    surveysIQuery = surveysIQuery.Where(x => x.EventCd == surveyList.EventCd);
                }

                listSurveyResult.TotalRowsAfterFiltering = surveysIQuery.Count();

                //Sort And Paging
                surveysIQuery = Filtering(surveysIQuery, surveyList);

                listSurveyResult.ListSurvey = surveysIQuery
                    .Select(s => new {
                        s.Cd,
                        s.InsertDate,
                        s.MediaCd,
                        MediaName = s.Media.Name,
                        s.VisitTime,
                        s.AgeRange,
                        s.Gender,
                        s.CareerCd,
                        CareerName = s.Career.Name,
                        s.IsMarried,
                        s.LivingStatus,
                        s.IsComeToBuy,
                        s.IsVisitedReception,
                        s.FavoriteArtist,
                        SurveyArtistName = s.SurveyArtists.Select(a => new
                        {
                            a.Artist.Name,
                            a.Artist.Code,
                        })
                    })
                    .AsEnumerable()
                    .Select(m => new SurveyItemAdminModel()
                    {
                        Cd                      = m.Cd,
                        InsertDate              = m.InsertDate?.ToString(ExactDateTimeFormat),
                        MediaName               = m.MediaName,
                        VisitTimeDescription    = ((VisitTime)m.VisitTime).GetEnumDescription(),
                        AgeRangeDescription     = ((AgeRange)m.AgeRange).GetEnumDescription(),
                        GenderName              = ((GenderOfSurvey)Convert.ToInt32(m.Gender)).GetEnumDescription(),
                        CareerName              = m.CareerName,
                        IsMarriedDescription    = ((IsMarried)Convert.ToInt32(m.IsMarried)).GetEnumDescription(),
                        LivingStatusDescription = ((LivingStatus)Convert.ToInt32(m.LivingStatus)).GetEnumDescription(),
                        IsComeToBuyDescription  = ((IsComeToBuy)Convert.ToInt32(m.IsComeToBuy)).GetEnumDescription(),
                        FavoriteArtist          = m.FavoriteArtist,
                        SurveyArtistName        = m.SurveyArtistName.OrderBy(m => m.Code).Select(y => y.Name).ToList(),
                    }).ToList();

                return listSurveyResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// This method to get list Survey from DB and put it into users/List Survey
        /// </summary>
        /// <param name="surveyListUser"></param>
        /// <returns></returns>
        public SurveyListUserModel LoadListSurveyUser(SurveyListUserModel surveyListUser)
        {
            try
            {
                var listSurveyReturn = new SurveyListUserModel();
                var surveys          = _context.Surveys.Where(x => !x.IsDeleted && x.Event.Code == surveyListUser.EventCode);
                listSurveyReturn.TotalRowsAfterFiltering = surveys.Count();

                //Sort And Paging
                surveys = Filtering(surveys, surveyListUser);

                listSurveyReturn.ListSurvey = surveys
                    .Select(s => new {
                        s.Cd,
                        s.InsertDate,
                        s.MediaCd,
                        MediaName = s.Media.Name,
                        s.VisitTime,
                        s.AgeRange,
                        s.Gender,
                        s.CareerCd,
                        CareerName = s.Career.Name,
                        s.IsMarried,
                        s.LivingStatus,
                        s.IsComeToBuy,
                        s.IsVisitedReception,
                        s.FavoriteArtist,
                        SurveyArtistName = s.SurveyArtists.Select(a => new
                        {
                            a.Artist.Name,
                            a.Artist.Code,
                        })
                    })
                    .AsEnumerable()
                    .Select(m => new SurveyItemUserModel()
                    {
                        Cd                      = m.Cd,
                        InsertDate              = m.InsertDate?.ToString(ExactDateTimeFormat),
                        MediaName               = m.MediaName,
                        VisitTimeDescription    = ((VisitTime)m.VisitTime).GetEnumDescription(),
                        AgeRangeDescription     = ((AgeRange)m.AgeRange).GetEnumDescription(),
                        GenderName              = ((GenderOfSurvey)Convert.ToInt32(m.Gender)).GetEnumDescription(),
                        CareerName              = m.CareerName,
                        IsMarriedDescription    = ((IsMarried)Convert.ToInt32(m.IsMarried)).GetEnumDescription(),
                        LivingStatusDescription = ((LivingStatus)Convert.ToInt32(m.LivingStatus)).GetEnumDescription(),
                        IsComeToBuyDescription  = ((IsComeToBuy)Convert.ToInt32(m.IsComeToBuy)).GetEnumDescription(),
                        FavoriteArtist          = m.FavoriteArtist,
                        SurveyArtistName        = m.SurveyArtistName.OrderBy(m => m.Code).Select(y => y.Name).ToList(),
                    }).ToList();

                return listSurveyReturn;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Method to get eventCd from applicationUserId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetEventCd(string userId)
        {
            try
            {
                var eventCd = _context.MstEvents.Where(x => x.ApplicationUserId == userId && !x.IsDeleted).Select(x => x.Cd).FirstOrDefault();
                return eventCd;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Method to implement sort for User/list Survey
        /// </summary>
        /// <param name="artists"></param>
        /// <param name="filtering"></param>
        /// <returns></returns>
        private IQueryable<Survey> Filtering(IQueryable<Survey> surveys, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "InsertDate":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.InsertDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.InsertDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "MediaName":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.Media.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.Media.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "VisitTimeDescription":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.VisitTime).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.VisitTime).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "AgeRangeDescription":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.AgeRange).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.AgeRange).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "GenderName":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.Gender).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.Gender).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "CareerName":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.Career.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.Career.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "IsMarriedDescription":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.IsMarried).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.IsMarried).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "LivingStatusDescription":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.LivingStatus).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.LivingStatus).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "IsComeToBuyDescription":
                    if (filtering.SortDirection == "asc")
                    {
                        surveys = surveys.OrderBy(x => x.IsComeToBuy).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        surveys = surveys.OrderByDescending(x => x.IsComeToBuy).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                default:
                    surveys = surveys.OrderByDescending(x => x.InsertDate).Skip(filtering.Start).Take(filtering.Length);
                    break;
            }

            return surveys;
        }

        public List<SurveyArtistModel> GetListFavoriteArtist()
        {
            var result = _context.MstArtists.Where(a => !a.IsDeleted && a.IsFavorited)
                        .Select(a => new
                        {
                            a.Cd,
                            a.ItemName,
                            DepartmentCd = a.ArtistDepartments.Select(ad => ad.DepartmentCd).OrderBy(cd => cd).FirstOrDefault(),
                        })
                        .AsEnumerable()
                        .OrderBy(a => a.DepartmentCd != 0 ? 0 : 1)
                        .ThenBy(a => a.DepartmentCd)
                        .Select(a => new SurveyArtistModel
                        {
                            ArtistCd = a.Cd,
                            ArtistName = a.ItemName,
                            IsFavorited = false
                        })
                        .ToList();

            return result;
        }
    }
}
