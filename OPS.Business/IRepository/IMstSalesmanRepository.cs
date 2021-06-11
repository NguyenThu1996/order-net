using Microsoft.AspNetCore.Http;
using OPS.ViewModels.Admin.Master.Salesman;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
    public interface IMstSalesmanRepository<T> : IGenericRepository<T> where T : class
    {
        IndexViewModel Load(SearchSalemanModel filter);

        AjaxResponseModel Create(SalesmanModel model, string userId);

        AjaxResponseModel Update(SalesmanModel model, string userId);

        bool IsDuplicated(SalesmanModel model);

        SalesmanModel GetSalemanForUpdate(int cd);

        AjaxResponseModel DeleteSaleman(int cd, string userId);

        AjaxResponseModel ImportCSV(IFormFile file, string userLoginId);
    }
}
