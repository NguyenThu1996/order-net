using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OPS.ViewModels.Admin.Master.Product;
using OPS.ViewModels.Shared;
using System.Collections.Generic;

namespace OPS.Business.IRepository
{
    public interface IMstProductRepository<T> : IGenericRepository<T> where T : class
    {
        ProductViewModel Load(ProductViewModel product);
        AjaxResponseModel Create(ProductModel model, string userId);
        AjaxResponseModel Update(ProductModel model, string userId);
        AjaxResponseModel Remove(int cd, string userId);
        bool IsDuplicated(ProductModel model);
        ProductModel GetProductDetails(int cd);
        List<SelectListItem> GetSelectListTechnique(bool isNullItemFirst = false);
        AjaxResponseModel ImportCSV(IFormFile file, string userLoginId);
    }
}
