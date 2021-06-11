using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Business;
using OPS.ViewModels.User.SalemanRegister;
using Vereyon.Web;

namespace OPS.Controllers
{
    [Authorize(Roles = "User")]
    public class SalesmanController : Controller
    {
        private IUnitOfWork _unitOfWork { get; set; }

        private IFlashMessage _flashMessage { get; set; }

        public SalesmanController(IUnitOfWork unitOfWork, IFlashMessage flashMessage)
        {
            _unitOfWork = unitOfWork;
            _flashMessage = flashMessage;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            SalemanRegisterModel model = new SalemanRegisterModel();

            model.ListSalesman = _unitOfWork.SalemanRegisterRepository.GetSelectListSalesman(userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(SalemanRegisterModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            try
            {
                if (model.SalemanCd > 0)
                {
                    var result = _unitOfWork.SalemanRegisterRepository.AddSalemanEvent(userId, model.SalemanCd);

                    if (result)
                    {
                        model.ListSalesman = _unitOfWork.SalemanRegisterRepository.GetSelectListSalesman(userId);
                        _flashMessage.Confirmation("営業担当の追加に成功しました。");
                        return RedirectToAction("Register", "Salesman");
                    }
                }

                model.ListSalesman = _unitOfWork.SalemanRegisterRepository.GetSelectListSalesman(userId);
                _flashMessage.Danger("営業担当の追加に失敗しました。もう一度お試しください。");
                return View(model);
            }
            catch (Exception e)
            {
                model.ListSalesman = _unitOfWork.SalemanRegisterRepository.GetSelectListSalesman(userId);
                _flashMessage.Danger(e.Message);
                return View(model);
            }
        }
    }
}
