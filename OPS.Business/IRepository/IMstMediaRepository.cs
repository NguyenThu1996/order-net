using Microsoft.AspNetCore.Http;
using OPS.ViewModels.Admin.Master.Media;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
    public interface IMstMediaRepository<T> : IGenericRepository<T> where T : class
    {
        MediaViewModel Load(MediaViewModel filtering);

        bool Create(MediaModel model, string userId);

        bool Update(MediaModel model, string userId);

        AjaxResponseModel Remove(int cd, string userId);

        bool IsDuplicated(MediaModel model);

        AjaxResponseModel ImportCSV(IFormFile file, string userLoginId);
    }
}
