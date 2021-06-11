using Microsoft.AspNetCore.Http;
using OPS.Entity.Schemas;
using OPS.ViewModels.Admin.Master.Payment;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
    public interface IMstPaymentRepository<T> : IGenericRepository<T> where T: class
    {
        PaymentViewModel Load(PaymentViewModel filtering);
        AjaxResponseModel Create(MstPayment payment, string userID);
        AjaxResponseModel Update(MstPayment payment, string cd, string userID);
        AjaxResponseModel Delete(string cd, string userID);
        bool IsDuplicated(string Code,string Category, string cd);
        AjaxResponseModel ImportCSV(IFormFile file, string userLoginId);

    }
}
