using Microsoft.AspNetCore.Http;
using OPS.ViewModels.Admin.Master.Technique;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
    public interface IMstTechniqueRepository<T> : IGenericRepository<T> where T : class
    {
        TechniqueViewModel Load(TechniqueViewModel filtering);
        bool Create(TechniqueModel model, string userId);
        bool Update(TechniqueModel model, string userId);
        AjaxResponseModel Remove(int cd, string userId);
        bool IsDuplicate(TechniqueModel model);
        AjaxResponseModel ImportCSV(IFormFile file, string userLoginId);
    }
}
