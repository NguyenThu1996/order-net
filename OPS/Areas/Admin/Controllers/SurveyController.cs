using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Business;
using OPS.ViewModels.Admin.Survey;
using OPS.ViewModels.Shared;
using System;
using System.Linq;

namespace OPS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SurveyController : Controller
    {
        private IUnitOfWork _unitOfWork { get; set; }

        public SurveyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Method return view Survey/Index.cshtml of Admin 
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                SurveyListAdminModel listSurvey = new SurveyListAdminModel();
                //Get list Event and put it into search dropdown at Survey/Index.cshtml of Admin
                listSurvey.ListEvent            = _unitOfWork.MasterDataRepository.GetSelectListEvent();
                return View(listSurvey);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Load list Survey and put it into data table at view Survey/Index.cshtml of Admin
        /// </summary>
        /// <param name="filtering"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadListSurveys(SurveyListAdminModel surveyList)
        {
            try
            {
                surveyList.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                surveyList.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();
                //Get list survey
                var data                  = _unitOfWork.SurveyRepository.LoadListSurveyAdmin(surveyList);

                //Put list survey to json result
                var result = Json(new
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
    }
}
