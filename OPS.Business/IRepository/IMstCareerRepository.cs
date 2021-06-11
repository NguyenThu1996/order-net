using Microsoft.AspNetCore.Http;
using OPS.ViewModels.Admin.Master.Career;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
    public interface IMstCareerRepository<T> : IGenericRepository<T> where T : class
    {
        CareerViewModel Load(CareerViewModel filtering);

        bool Create(CareerModel model, string userId);

        bool Update(CareerModel model, string userId);

        AjaxResponseModel Remove(int cd, string userId);

        bool IsDuplicated(CareerModel model);

        AjaxResponseModel ImportCSV(IFormFile file, string userLoginId);
    }
}
