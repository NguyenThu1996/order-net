using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Business;
using OPS.ViewModels.Admin.Master.Event;
using OPS.ViewModels.Shared;
using OPS.Entity.Schemas;
using System;
using System.Linq;
using System.Security.Claims;
using OPS.ViewModels.Admin.Master.Payment;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Vereyon.Web;
using OPS.ViewModels.Admin.Master.Artist;
using OPS.ViewModels.Admin.Master.Tax;
using OPS.ViewModels.Admin.Master.Media;
using OPS.ViewModels.Admin.Master.CompanyType;
using OPS.ViewModels.Admin.Master.Organization;
using OPS.ViewModels.Admin.Master.Career;
using OPS.ViewModels.Admin.Master.Product;
using OPS.ViewModels.Admin.Master.Salesman;
using OPS.ViewModels.Admin.Master.ProductAgreementType;
using OPS.ViewModels.Admin.Master.Department;
using OPS.ViewModels.Admin.Master.Technique;
using Microsoft.AspNetCore.Http;

namespace OPS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : Controller
    {
        private IUnitOfWork _unitOfWork { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }

        private IFlashMessage _flashMessage { get; set; }

        public MasterDataController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IFlashMessage flashMessage)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _flashMessage = flashMessage;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Master Career

        [HttpGet]
        public IActionResult Career()
        {
            return View();
        }

        [HttpPost]
        public JsonResult LoadCareersByFiltering(CareerViewModel filtering)
        {
            filtering.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filtering.SortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstCareerRepository.Load(filtering);
            var result = Json(new
            {
                data = data.Careers,
                draw = filtering.Draw,
                recordsTotal = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
            return result;
        }

        [HttpGet]
        public JsonResult CareerDetail(int cd)
        {
            var result = _unitOfWork.MstCareerRepository.Get(cd);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CareerCreateOrUpdate(CareerModel model)
        {
           
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (ModelState.IsValid)
                {
                    model.Name = model.Name.Trim();
                    if (_unitOfWork.MstCareerRepository.IsDuplicated(model))
                    {
                        return Json(new AjaxResponseModel(false, "この職業が既に存在しています。"));
                    }
                    bool result;
                    if (model.Cd <= 0)
                    {
                        // add new
                        result = _unitOfWork.MstCareerRepository.Create(model, userId);
                    }
                    else
                    {
                        // update
                        var career = _unitOfWork.MstCareerRepository.Get(model.Cd); // check exist
                        if (career == null)
                        {
                            return Json(new AjaxResponseModel(false, "職業が見つかりません。"));
                        }

                        result = _unitOfWork.MstCareerRepository.Update(model, userId);
                    }

                    if (result)
                    {
                        _unitOfWork.SaveChanges();
                    }
                    return Json(new AjaxResponseModel(result, null));
                }
                else
                {
                    return Json(new AjaxResponseModel(false, "無効なデータ！"));
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        public JsonResult CareerDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstCareerRepository.Remove(cd, userId);

                if (result.Status)
                {
                    return Json(result);
                }
                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadCSVMstCareer(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstCareerRepository.ImportCSV(file, userId);

                return new JsonResult(result);
            }

            return new JsonResult(new AjaxResponseModel { Status = false, Message = "エラーが発生しました。もう一度お試しください。" });
        }

        #endregion

        #region Master Artist
        [HttpGet]
        public IActionResult Artist()
        {
            var viewModel = new ArtistModel
            {
                ListDepartments = _unitOfWork.MasterDataRepository.GetSelectListDepartments()
            };
            return View(viewModel);
        }

        [HttpPost]
        public JsonResult LoadArtistsByFiltering(ArtistViewModel filtering)
        {
            filtering.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filtering.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstArtistRepository.Load(filtering);
            var result = Json(new
            {
                data            = data.Artists,
                draw            = filtering.Draw,
                recordsTotal    = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ArtistCreateOrUpdate(ArtistModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                if (ModelState.IsValid)
                {
                    if (_unitOfWork.MstArtistRepository.IsDuplicated(model))
                    {
                        return Json(new AjaxResponseModel(false, "この作家コードが既に存在しています。"));
                    }

                    if (model.Cd <= 0)
                    {
                        //add new
                        var result = _unitOfWork.MstArtistRepository.Create(model, userId);
                        return Json(result);
                    }
                    else
                    {
                        // update 
                        var result = _unitOfWork.MstArtistRepository.Update(model, userId);
                        return Json(result);
                    }
                }
                else
                {
                    return Json(new AjaxResponseModel(false, "無効なデータ！"));
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ArtistDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstArtistRepository.Remove(cd, userId);

                if (result.Status)
                {
                    return Json(result);
                }
                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpGet]
        public IActionResult ArtistDetail(int cd)
        {
            ArtistModel result;
            if (cd > 0)
            {
                result = _unitOfWork.MstArtistRepository.GetArtistDetails(cd);
            }
            else
            {
                result = new ArtistModel();
            }

            result.ListDepartments = _unitOfWork.MasterDataRepository.GetSelectListDepartments();

            return PartialView("Partial/_ArtistDetails", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadCSVMstArtist(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstArtistRepository.ImportCSV(file, userId);

                return new JsonResult(result);
            }

            return new JsonResult(new AjaxResponseModel { Status = false, Message = "エラーが発生しました。もう一度お試しください。" });
        }
        #endregion

        #region Master Payment
        [HttpGet]
        public IActionResult Payment()
        {
            return View();
        }

        public JsonResult LoadPaymentByFiltering(PaymentViewModel filtering)
        {
            filtering.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filtering.SortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var data = _unitOfWork.MstPaymentRepository.Load(filtering);
            var result = Json(new
            {
                data = data.Payments,
                draw = filtering.Draw,
                recordsTotal = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreatePayment(PaymentModel data)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (ModelState.IsValid)
                {
                    if(!_unitOfWork.MstPaymentRepository.IsDuplicated(data.Code,data.Category.ToString().Trim().ToLower(), "0"))
                    {
                        MstPayment mst = new MstPayment()
                        {
                            Code = data.Code,
                            Category = data.Category,
                            Name = data.Name,
                            NameKn = data.NameKn,
                            IsDeleted = false
                        };
                        var result = _unitOfWork.MstPaymentRepository.Create(mst, userId);
                       if(result.Status)
                            _unitOfWork.SaveChanges();
                        return Json(result);
                    }
                    else
                    {
                        return Json(new AjaxResponseModel(false, "IsDuplicated"));
                    }
                }
                else
                {
                    return Json(new AjaxResponseModel(false, "無効なデータ！"));
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdatePayment(PaymentModel data, string cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (ModelState.IsValid)
                {
                    if (!_unitOfWork.MstPaymentRepository.IsDuplicated(data.Code, data.Category.ToString().Trim().ToLower(), cd))
                    {
                        MstPayment mst = new MstPayment()
                        {
                            Code = data.Code,
                            Category = data.Category,
                            Name = data.Name,
                            NameKn = data.NameKn
                        };
                        var result = _unitOfWork.MstPaymentRepository.Update(mst, cd, userId);
                        if(result.Status)
                            _unitOfWork.SaveChanges();
                        return Json(result);
                    }
                    else
                    {
                        return Json(new AjaxResponseModel(false, "IsDuplicated"));
                    }                   
                }
                else
                {
                    return Json(new AjaxResponseModel(false, "無効なデータ！"));
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeletePayment(string cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstPaymentRepository.Delete(cd, userId);
                if(result.Status)
                    _unitOfWork.SaveChanges();
                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadCSVMstPayment(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstPaymentRepository.ImportCSV(file, userId);

                return new JsonResult(result);
            }

            return new JsonResult(new AjaxResponseModel { Status = false, Message = "エラーが発生しました。もう一度お試しください。" });
        }
        #endregion

        #region Master Event
        public IActionResult Event()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoadEventByFiltering(SearchEventModel filter)
        {
            filter.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filter.SortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstEventRepository.Load(filter);
            var result = Json(new
            {
                data = data.ListEvent,
                draw = filter.Draw,
                recordsTotal = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
            return result;
        }

        [HttpGet]
        public IActionResult EventAdd()
        {
            try
            {
                AddOrUpdateEventModel addEvent = new AddOrUpdateEventModel()
                {
                    ListSalesman = _unitOfWork.MasterDataRepository.GetSelectsListSalesman(),
                    ListMedia = _unitOfWork.MasterDataRepository.GetSelectListEventMedias(),
                };
                return View(addEvent);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventAdd(AddOrUpdateEventModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            try
            {
                if (ModelState.IsValid && model.ListSaleManEvent.Count > 0)
                {
                     var result = await _unitOfWork.MstEventRepository.Create(model, userId);

                    if (result.Status)
                    {
                        _flashMessage.Confirmation(result.Message);
                        return RedirectToAction("Event", "MasterData");
                    } 
                    else
                    {
                        _flashMessage.Danger(result.Message);

                        model.ListSaleManEvent = model.ListSaleManEvent.Where(s => !s.IsDelete).ToList();
                        model.ListEventMedia = model.ListEventMedia.Where(m => !m.IsDelete).ToList();

                        model.ListSalesman = _unitOfWork.MasterDataRepository.GetSelectsListSalesman();
                        model.ListMedia = _unitOfWork.MasterDataRepository.GetSelectListEventMedias();
                        return View(model);
                    }
                } 

                _flashMessage.Danger("催事の新規作成に失敗しました。もう一度お試しください。");

                model.ListSaleManEvent = model.ListSaleManEvent.Where(s => !s.IsDelete).ToList();
                model.ListEventMedia = model.ListEventMedia.Where(m => !m.IsDelete).ToList();

                model.ListSalesman = _unitOfWork.MasterDataRepository.GetSelectsListSalesman();
                model.ListMedia = _unitOfWork.MasterDataRepository.GetSelectListEventMedias();
                return View(model);
            }
            catch (Exception e)
            {
                _flashMessage.Danger(e.Message);

                model.ListSaleManEvent = model.ListSaleManEvent.Where(s => !s.IsDelete).ToList();
                model.ListEventMedia = model.ListEventMedia.Where(m => !m.IsDelete).ToList();

                model.ListSalesman = _unitOfWork.MasterDataRepository.GetSelectsListSalesman();
                model.ListMedia = _unitOfWork.MasterDataRepository.GetSelectListEventMedias();
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult EventEdit(int cd)
        {
            try
            {
                var getEvent = _unitOfWork.MstEventRepository.GetEventForUpdate(cd);

                if (getEvent == null)
                {
                    return NotFound();
                }

                return View(getEvent);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventEdit(AddOrUpdateEventModel model)
        {
            try
            {
                var userLoginId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (ModelState.IsValid)
                {
                    var result = await _unitOfWork.MstEventRepository.UpdateEvent(model, userLoginId);

                    if (result.Status)
                    {
                        _flashMessage.Confirmation(result.Message);

                        return RedirectToAction("Event", "MasterData");
                    }

                    model.ListSalesman = _unitOfWork.MasterDataRepository.GetSelectsListSalesman();
                    model.ListMedia = _unitOfWork.MasterDataRepository.GetSelectListEventMedias();

                    model.ListSaleManEvent = model.ListSaleManEvent.Where(s => !s.IsDelete).ToList();
                    model.ListEventMedia = model.ListEventMedia.Where(m => !m.IsDelete).ToList();

                    _flashMessage.Danger(result.Message);
                    return View(model);
                }

                model.ListSalesman = _unitOfWork.MasterDataRepository.GetSelectsListSalesman();
                model.ListMedia = _unitOfWork.MasterDataRepository.GetSelectListEventMedias();

                model.ListSaleManEvent = model.ListSaleManEvent.Where(s => !s.IsDelete).ToList();
                model.ListEventMedia = model.ListEventMedia.Where(m => !m.IsDelete).ToList();

                _flashMessage.Danger("催事の編集に失敗しました。もう一度お試しください。");
                return View(model);
            }
            catch (Exception e)
            {
                _flashMessage.Danger(e.Message);
                model.ListSalesman = _unitOfWork.MasterDataRepository.GetSelectsListSalesman();
                model.ListMedia = _unitOfWork.MasterDataRepository.GetSelectListEventMedias();

                model.ListSaleManEvent = model.ListSaleManEvent.Where(s => !s.IsDelete).ToList();
                model.ListEventMedia = model.ListEventMedia.Where(m => !m.IsDelete).ToList();

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EventDelete(int cd)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var eventFindId = _unitOfWork.MstEventRepository.Get(cd); // check exist
                if (eventFindId == null)
                {
                    return Json(new AjaxResponseModel(false, "この催事が見つかりません。"));
                }

                var result = _unitOfWork.MstEventRepository.DeleteEvent(cd, userId);
                if (result == false)
                {
                    return Json(new AjaxResponseModel(false, "この催事が使われている為、削除できません。"));
                }
                if (result)
                {
                    _unitOfWork.SaveChanges();
                }

                return Json(new AjaxResponseModel(result, null));
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        public JsonResult GetDateTimeEvent(int cd)
        {
            try
            {
                var result = _unitOfWork.MasterDataRepository.GetDateTimeEvent(cd);
                
                return Json(result);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UploadCSVMstEvent(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _unitOfWork.MstEventRepository.ImportCSVAsync(file, userId);

                return new JsonResult(result);
            }

            return new JsonResult(new AjaxResponseModel { Status = false, Message = "エラーが発生しました。もう一度お試しください。" });
        }
        #endregion

        #region Master Salesman
        public IActionResult Salesman()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoadSalesmanByFiltering(SearchSalemanModel filter)
        {
            filter.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filter.SortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstSalesmanRepository.Load(filter);
            var result = Json(new
            {
                data = data.ListSalesman,
                draw = filter.Draw,
                recordsTotal = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
            return result;
        }

        [HttpGet]
        public JsonResult GetSalesmanForUpdate(int cd)
        {
            var result = _unitOfWork.MstSalesmanRepository.GetSalemanForUpdate(cd);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SalemanCreateOrUpdate(SalesmanModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                if (ModelState.IsValid)
                {
                    if (_unitOfWork.MstSalesmanRepository.IsDuplicated(model))
                    {
                        return Json(new AjaxResponseModel(false, "この担当者が既に存在しています。"));
                    }

                    if (model.Cd <= 0)
                    {
                        var result = _unitOfWork.MstSalesmanRepository.Create(model, userId);
                        if (result.Status)
                        {
                            _unitOfWork.SaveChanges();
                        }
                        return Json(result);
                    }
                    else
                    {
                       var result = _unitOfWork.MstSalesmanRepository.Update(model, userId);
                        if (result.Status)
                        {
                            _unitOfWork.SaveChanges();
                        }
                        return Json(result);
                    }
                }
                return Json(new AjaxResponseModel(false, "このフィールドは必須です。"));
            }
            catch(Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SalemanDelete(int cd)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var result = _unitOfWork.MstSalesmanRepository.DeleteSaleman(cd, userId);

                if (result.Status)
                {
                    _unitOfWork.SaveChanges();
                }

                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadCSVMstSalesman(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstSalesmanRepository.ImportCSV(file, userId);

                return new JsonResult(result);
            }

            return new JsonResult(new AjaxResponseModel { Status = false, Message = "エラーが発生しました。もう一度お試しください。" });
        }
        #endregion

        #region Master Media
        public IActionResult Media()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MediaCreateOrUpdate(MediaModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (ModelState.IsValid)
                {
                    if (_unitOfWork.mstMediaRepository.IsDuplicated(model))
                    {
                        return Json(new AjaxResponseModel(false, "この媒体が既に存在しています。"));
                    }
                    bool result;
                    if (model.Cd <= 0)
                    {
                        //add new
                        result = _unitOfWork.mstMediaRepository.Create(model, userId);
                    }
                    else
                    {
                        // update 
                        var media = _unitOfWork.mstMediaRepository.Get(model.Cd); // check exist
                        if (media == null)
                        {
                            return Json(new AjaxResponseModel(false, "この媒体が存在しません。"));
                        }
                        
                        result = _unitOfWork.mstMediaRepository.Update(model, userId);
                    }
                    if (result)
                    {
                        _unitOfWork.SaveChanges();
                    }
                    return Json(new AjaxResponseModel(result, null));
                }
                else
                {
                    return Json(new AjaxResponseModel(false, "無効なデータ！"));
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MediaDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.mstMediaRepository.Remove(cd, userId);
                if(result.Status)
                {
                    return Json(result);
                }

                return Json(result);
            }
            catch (Exception)
            {
                return Json(new AjaxResponseModel(false, "この媒体が削除できません。"));
            }
        }

        [HttpPost]
        public JsonResult LoadMediaByFiltering(MediaViewModel filtering)
        {
            filtering.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filtering.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();

            var data   = _unitOfWork.mstMediaRepository.Load(filtering);
            var result = Json(new
            {
                data            = data.Medias,
                draw            = filtering.Draw,
                recordsTotal    = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
            return result;
        }

        public JsonResult MediaDetail(int cd)
        {
            var result = _unitOfWork.mstMediaRepository.Get(cd);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadCSVMstMedia(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.mstMediaRepository.ImportCSV(file, userId);

                return new JsonResult(result);
            }

            return new JsonResult(new AjaxResponseModel { Status = false, Message = "エラーが発生しました。もう一度お試しください。" });
        }
        #endregion

        #region Master Technique
        public IActionResult Technique()
        {
            return View();
        }

        //LoadDatatable
        public JsonResult LoadTechniqueByFiltering(TechniqueViewModel technique)
        {
            technique.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            technique.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstTechniqueRepository.Load(technique);

            var result = Json(new
            {
                data            = data.Techniques,
                draw            = technique.Draw,
                recordsTotal    = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult TechniqueCreateOrUpdate(TechniqueModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    bool result;

                    if (_unitOfWork.MstTechniqueRepository.IsDuplicate(model))
                    {
                        return Json(new AjaxResponseModel
                        { 
                          Message = "この技法コードが既に存在しています。",
                          Status  = false
                        });
                    }
                    if(model.Cd == 0)
                    {
                        //add new Techinque
                        result = _unitOfWork.MstTechniqueRepository.Create(model, userId);
                    }
                    else
                    {
                        //update Technique
                        var technique = _unitOfWork.MstTechniqueRepository.Get(model.Cd);
                        if(technique == null)
                        {
                            return Json(new AjaxResponseModel
                            {
                                Message = "この技法が見つかりません。",
                                Status  = false
                            });
                        }

                        result = _unitOfWork.MstTechniqueRepository.Update(model, userId);
                    }
                    if(result)
                    {
                        _unitOfWork.SaveChanges();
                    }

                    return Json(new AjaxResponseModel(result, null));
                }
                else
                {
                    return Json(new AjaxResponseModel
                    {
                        Message = "無効なデータ！",
                        Status  = false
                    });
                }
            }
            catch(Exception e)
            {
                return Json(new AjaxResponseModel
                {
                    Message = e.Message,
                    Status  = false
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult TechniqueDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstTechniqueRepository.Remove(cd, userId);
                if (result.Status)
                {
                    return Json(result);
                }
                else
                {
                    return Json(result);
                }
            }
            catch (Exception)
            {
                return Json(new AjaxResponseModel(false, "削除に失敗しました。"));
            }
        }

        public JsonResult TechniqueDetail(int cd)
        {
            var result = _unitOfWork.MstTechniqueRepository.Get(cd);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadCSVMstTechnique(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstTechniqueRepository.ImportCSV(file, userId);

                return new JsonResult(result);
            }

            return new JsonResult(new AjaxResponseModel { Status = false, Message = "エラーが発生しました。もう一度お試しください。" });
        }
        #endregion

        #region Master Organization
        public IActionResult Organization()
        {
            return View();
        }
        public JsonResult LoadOrganizationsByFiltering(OrganizationViewModel organization)
        {
            organization.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            organization.SortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstOrganizationRepository.Load(organization);
            var result = Json(new
            {
                data = data.Organizations,
                draw = organization.Draw,
                recordsTotal = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult OrganizationCreateOrUpdate(OrganizationModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (_unitOfWork.MstOrganizationRepository.IsDuplicated(model))
                    {
                        return Json(new AjaxResponseModel
                        {
                            Message = "この組織が既に存在しています。",
                            Status  = false
                        });
                    }
                    bool result;
                    if(model.Cd == 0)
                    {   
                        //add new
                        result = _unitOfWork.MstOrganizationRepository.Create(model, userId);

                    }
                    else
                    {
                        //Update
                        var media = _unitOfWork.MstOrganizationRepository.Get(model.Cd); // check exist
                        if (media == null)
                        {
                            return Json(new AjaxResponseModel
                            {
                                Message = "この組織が存在していません。",
                                Status = false
                            });
                        }

                        result = _unitOfWork.MstOrganizationRepository.Update(model, userId);
                    }
                    if (result)
                    {
                        _unitOfWork.SaveChanges();
                    }

                    return Json(new AjaxResponseModel(result, null));
                }
                else
                {
                    return Json(new AjaxResponseModel
                    {
                        Message = "無効なデータ！",
                        Status  = false
                    });
                }
            }
            catch(Exception e)
            {
                return Json(new AjaxResponseModel
                {
                    Message = e.Message,
                    Status = false
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult OrganizationDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstOrganizationRepository.Remove(cd, userId);
                if (result.Status)
                {
                    return Json(result);
                }
                else
                {
                    return Json(result);
                }
            }
            catch (Exception)
            {
                return Json(new AjaxResponseModel(false, "削除に失敗しました。"));
            }
        }

        public JsonResult OrganizationDetail(int cd)
        {
            var result = _unitOfWork.MstOrganizationRepository.Get(cd);

            return Json(result);
        }
        #endregion

        #region Master CompanyType
        public IActionResult CompanyType()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CompanyTypeCreateOrUpdate(MasterCompanyTypeModel companyType)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (ModelState.IsValid)
                {
                    if (companyType.Cd == 0)
                    {
                        var result = _unitOfWork.MstCompanyTypeRepository.Create(companyType, userId);
                        if (result.Status)
                            _unitOfWork.SaveChanges();
                        return Json(result);
                    }
                    else
                    {
                        var result = _unitOfWork.MstCompanyTypeRepository.Update(companyType, userId);
                        if (result.Status)
                            _unitOfWork.SaveChanges();
                        return Json(result);
                    }
                }
                else
                {
                    return Json(new AjaxResponseModel(false, "無効なデータ！"));
                }
            }
            catch ( Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }
        public JsonResult LoadCompanyTypeByFiltering(OpsFilteringDataTableModel filtering)
        {
            filtering.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filtering.SortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var data = _unitOfWork.MstCompanyTypeRepository.Load(filtering);
            var result = Json(new
            {
                data = data.CompanyTypes,
                draw = filtering.Draw,
                recordsTotal = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CompanyTypeDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstCompanyTypeRepository.Delete(cd, userId);
                if (result.Status)
                    _unitOfWork.SaveChanges();
                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }
        #endregion

        #region Master Product
        public IActionResult Product()
        {
            try
            {
                var productModel = new ProductModel
                {
                    ListArtist = _unitOfWork.MstArtistRepository.GetSelectListArtist(),
                    ListTechniques = _unitOfWork.MstProductRepository.GetSelectListTechnique(),
                };

                return View(productModel);
            }
            catch(Exception)
            {
                throw;
            }   
        }

        public JsonResult LoadProductsByFiltering(ProductViewModel product )
        {
            product.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            product.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstProductRepository.Load(product);
            var result = Json(new
            {
                data            = data.Products,
                draw            = product.Draw,
                recordsTotal    = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ProductCreateOrUpdate(ProductModel model)
        {
            
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (_unitOfWork.MstProductRepository.IsDuplicated(model))
                    {
                        return Json(new AjaxResponseModel(false, "この作品コードが既に存在しています。"));
                    }

                    if (model.Cd <= 0)
                    {
                        var result = _unitOfWork.MstProductRepository.Create(model, userId);
                        return Json(result);
                    }
                    else
                    {
                        var result = _unitOfWork.MstProductRepository.Update(model, userId);
                        return Json(result);
                    }
                }
                else
                {
                    return Json(new AjaxResponseModel(false, "無効なデータ！"));
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpGet]
        public IActionResult ProductDetail(int cd)
        {
            ProductModel result;
            if (cd > 0)
            {
                result = _unitOfWork.MstProductRepository.GetProductDetails(cd);
            }
            else
            {
                result = new ProductModel();
            }

            result.ListArtist = _unitOfWork.MstArtistRepository.GetSelectListArtist();
            result.ListTechniques = _unitOfWork.MstProductRepository.GetSelectListTechnique();

            return PartialView("Partial/_ProductDetails", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ProductDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstProductRepository.Remove(cd, userId);
                if (result.Status)
                {
                    return Json(result);
                }
                
                return Json(result);
            }
            catch (Exception)
            {
                return Json(new AjaxResponseModel(false, "この媒体が削除できません。"));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadCSVMstProduct(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstProductRepository.ImportCSV(file, userId);

                return new JsonResult(result);
            }

            return new JsonResult(new AjaxResponseModel { Status = false, Message = "エラーが発生しました。もう一度お試しください。" });
        }

        #endregion

        #region Master Tax
        public IActionResult Tax()
        {
            return View();
        }

        public JsonResult LoadTaxesByFiltering(OpsFilteringDataTableModel filtering)
        {
            filtering.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filtering.SortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstTaxRepository.Load(filtering);
            var result = Json(new
            {
                data = data.Taxes,
                draw = filtering.Draw,
                recordsTotal = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult TaxCreateOrUpdate(MasterTaxModel tax)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                if (ModelState.IsValid)
                {
                    if (tax.Cd == 0)//Add
                    {
                        MstTax _tax = new MstTax
                        {
                            StartDate = tax.StartDate,
                            EndDate = tax.EndDate,
                            Value = tax.Value,
                            InsertUserId = userId,
                            InsertDate = DateTime.Now
                        };
                        var result = _unitOfWork.MstTaxRepository.Create(_tax, userId);
                        if (result.Status)
                            _unitOfWork.SaveChanges();
                        return Json(result);
                    }
                    else//Update
                    {
                        MstTax _tax = new MstTax
                        {
                            Cd = tax.Cd,
                            StartDate = tax.StartDate,
                            EndDate = tax.EndDate,
                            Value = tax.Value
                        };
                        var result = _unitOfWork.MstTaxRepository.Update(_tax, userId);
                        if (result.Status)
                            _unitOfWork.SaveChanges();
                        return Json(result);
                    }
                }
                else
                    return Json(new AjaxResponseModel(false, "無効なデータ！"));

            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult TaxDelete(string cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = _unitOfWork.MstTaxRepository.Delete(cd, userId);
                if(result.Status)
                    _unitOfWork.SaveChanges();
                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }
        #endregion

        #region Master Department
        public IActionResult Department()
        {
            return View();
        }

        public JsonResult LoadDepartmentsByFiltering(DepartmentViewModel department)
        {
            department.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            department.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();

            var data   = _unitOfWork.MstDepartmentRepository.Load(department);
            var result = Json(new
            {
                data            = data.Departments,
                draw            = department.Draw,
                recordsTotal    = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DepartmentCreateOrUpdate(DepartmentModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (_unitOfWork.MstDepartmentRepository.IsDuplicated(model))
                    {
                        return Json(new AjaxResponseModel
                        {
                            Message = "この部署がすでに存在しています。",
                            Status  = false
                        });
                    }
                    bool result;
                    if (model.Cd == 0)
                    {
                        //add new
                        result = _unitOfWork.MstDepartmentRepository.Create(model, userId);
                    }
                    else
                    {
                        //Update
                        var media = _unitOfWork.MstDepartmentRepository.Get(model.Cd); // check exist
                        if (media == null)
                        {
                            return Json(new AjaxResponseModel
                            {
                                Message = "この部署が見つかりません。",
                                Status  = false
                            });
                        }

                        result = _unitOfWork.MstDepartmentRepository.Update(model, userId);
                    }
                    if (result)
                    {
                        _unitOfWork.SaveChanges();
                    }

                    return Json(new AjaxResponseModel(result, null));
                }
                else
                {
                    return Json(new AjaxResponseModel
                    {
                        Message = "無効なデータ！",
                        Status  = false
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel
                {
                    Message = e.Message,
                    Status  = false
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DepartmentDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstDepartmentRepository.Remove(cd, userId);
                if (result.Status)
                {
                    return Json(result);
                }
                else
                {
                    return Json(result);
                }
            }
            catch (Exception)
            {
                return Json(new AjaxResponseModel(false, "削除に失敗しました。"));
            }
        }

        public JsonResult DepartmentDetail(int cd)
        {
            var result = _unitOfWork.MstDepartmentRepository.Get(cd);

            return Json(result);
        }

        #endregion

        #region Master ProductAgreementType
        public IActionResult ProductAgreementType()
        {
            return View();
        }

        public JsonResult LoadProductAgreementTypeByFiltering(ProductAgreementTypeViewModel ProductAgreement)
        {
            ProductAgreement.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            ProductAgreement.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.MstProductAgreementTypeRepository.Load(ProductAgreement);
            var result = Json(new
            {
                data            = data.ProductAgreementTypes,
                draw            = ProductAgreement.Draw,
                recordsTotal    = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ProductAgreementTypeCreateOrUpdate(ProductAgreementTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (_unitOfWork.MstProductAgreementTypeRepository.IsDuplicated(model))
                    {
                        return Json(new AjaxResponseModel
                        {
                            Message = "この同意書種類コードがすでに存在しています。",
                            Status  = false
                        });
                    }
                    bool result;
                    if (model.Cd == 0)
                    {
                        //add new
                        result = _unitOfWork.MstProductAgreementTypeRepository.Create(model, userId);
                    }
                    else
                    {
                        //Update
                        var media = _unitOfWork.MstProductAgreementTypeRepository.Get(model.Cd); // check exist
                        if (media == null)
                        {
                            return Json(new AjaxResponseModel
                            {
                                Message = "この同意書種類が見つかりません。",
                                Status  = false
                            });
                        }

                        result = _unitOfWork.MstProductAgreementTypeRepository.Update(model, userId);
                    }
                    if (result)
                    {
                        _unitOfWork.SaveChanges();
                    }

                    return Json(new AjaxResponseModel(result, null));
                }
                else
                {
                    return Json(new AjaxResponseModel
                    {
                        Message = "無効なデータ！",
                        Status  = false
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel
                {
                    Message = e.Message,
                    Status  = false
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ProductAgreementTypeDelete(int cd)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _unitOfWork.MstProductAgreementTypeRepository.Remove(cd, userId);
                if (result.Status)
                {
                    return Json(result);
                }
                else
                {
                    return Json(result);
                }
            }
            catch (Exception)
            {
                return Json(new AjaxResponseModel(false, "削除に失敗しました。"));
            }
        }

        public JsonResult ProductAgreementTypeDetail(int cd)
        {
            var result = _unitOfWork.MstProductAgreementTypeRepository.Get(cd);

            return Json(result);
        }
    }
    #endregion
}
