using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Business;
using OPS.Utility;
using OPS.ViewModels.Shared;
using OPS.ViewModels.User.Survey;
using System;
using System.Linq;
using System.Security.Claims;
using Vereyon.Web;
using static OPS.Utility.Constants;

namespace OPS.Controllers
{
    [Authorize(Roles = "User,Survey")]
    public class SurveyController : Controller
    {
        private IUnitOfWork _unitOfWork { get; set; }

        private IFlashMessage _flashMessage { get; set; }

        public SurveyController(IUnitOfWork unitOfWork, IFlashMessage flashMessage)
        {
            _unitOfWork   = unitOfWork;
            _flashMessage = flashMessage;
        }

        /// <summary>
        /// Method to get list of all surveys
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load list Survey and put it into data table at view Survey/Index.cshtml
        /// </summary>
        /// <param name="filtering"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadSurveys(SurveyListUserModel surveyList)
        {
            try
            {
                surveyList.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                surveyList.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();
                //Get user code
                surveyList.EventCode      = User.FindFirstValue(ClaimTypes.Name);
                surveyList.EventCode      = surveyList.EventCode.Replace(Constants.SurveyUserName, "");
                //Get list survey
                var data            = _unitOfWork.SurveyRepository.LoadListSurveyUser(surveyList);
                //Put list survey to json result
                var result  = Json(new
                {
                    data            = data.ListSurvey,
                    draw            = surveyList.Draw,
                    recordsFiltered = data.TotalRowsAfterFiltering
                });

                return result;
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        /// <summary>
        /// Init do survey page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Do()
        {
            try
            {
                SurveyViewModel survey = new SurveyViewModel();

                //Get user code
                var eventCode = User.FindFirstValue(ClaimTypes.Name);
                eventCode     = eventCode.Replace(Constants.SurveyUserName, "");
                survey.LstMedia     = _unitOfWork.MasterDataRepository.GetSelectListEventMedia(eventCode);
                survey.LstAgeRange  = _unitOfWork.MasterDataRepository.GetSelectListAgeRange();
                survey.LstCareer    = _unitOfWork.MasterDataRepository.GetSelectListCareer();

                survey.ListFavoriteArtists = _unitOfWork.SurveyRepository.GetListFavoriteArtist();

                //Set values
                survey.VisitTime            = (int) VisitTime.first_time;
                survey.Gender               = (int) GenderOfSurvey.Male;
                survey.IsMarried            = (int) IsMarried.Yes;
                survey.LivingStatus         = (int) LivingStatus.Alone;
                survey.IsComeToBuy          = (int) IsComeToBuy.Yes;

                return View(survey);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Submit survey into database
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Do(SurveyViewModel survey)
        {
            //Get current user login
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //Get user code
            var eventCode = User.FindFirstValue(ClaimTypes.Name);
            eventCode     = eventCode.Replace(Constants.SurveyUserName, "");
            try
            {
                if (ModelState.IsValid)
                {
                    //Add survey in to database
                    bool isCreatedSurvey = _unitOfWork.SurveyRepository.CreateSurvey(survey, userId, eventCode);

                    if (isCreatedSurvey)
                    {
                        _flashMessage.Confirmation("アンケートの提出に成功しました。");
                        return RedirectToAction("Do", "Survey");
                    }
                    else
                    {
                        //Get lists dropdown
                        survey.LstMedia    = _unitOfWork.MasterDataRepository.GetSelectListEventMedia(eventCode);
                        survey.LstAgeRange = _unitOfWork.MasterDataRepository.GetSelectListAgeRange();
                        survey.LstCareer   = _unitOfWork.MasterDataRepository.GetSelectListCareer();
                        survey.ListFavoriteArtists = _unitOfWork.SurveyRepository.GetListFavoriteArtist();

                        _flashMessage.Danger("アンケートの提出に失敗しました。もう一度お試しください。");
                        return View(survey);
                    }
                }
                else
                {
                    //Get lists dropdown
                    survey.LstMedia    = _unitOfWork.MasterDataRepository.GetSelectListEventMedia(eventCode);
                    survey.LstAgeRange = _unitOfWork.MasterDataRepository.GetSelectListAgeRange();
                    survey.LstCareer   = _unitOfWork.MasterDataRepository.GetSelectListCareer();
                    survey.ListFavoriteArtists = _unitOfWork.SurveyRepository.GetListFavoriteArtist();

                    _flashMessage.Danger("アンケートの提出に失敗しました。もう一度お試しください。");
                    return View(survey);
                }
            }
            catch (Exception)
            {
                //Get lists dropdown
                survey.LstMedia    = _unitOfWork.MasterDataRepository.GetSelectListEventMedia(eventCode);
                survey.LstAgeRange = _unitOfWork.MasterDataRepository.GetSelectListAgeRange();
                survey.LstCareer   = _unitOfWork.MasterDataRepository.GetSelectListCareer();
                survey.ListFavoriteArtists = _unitOfWork.SurveyRepository.GetListFavoriteArtist();

                _flashMessage.Danger("アンケートの提出に失敗しました。もう一度お試しください。");
                return View(survey);
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            return View();
        }
    }
}
