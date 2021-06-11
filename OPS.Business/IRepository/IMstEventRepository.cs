using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OPS.ViewModels.Admin.Master.Event;
using OPS.ViewModels.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPS.Business.IRepository
{
    public interface IMstEventRepository<T> : IGenericRepository<T> where T : class
    {
        IndexViewModel Load(SearchEventModel model);

        Task<ErrorModel> Create(AddOrUpdateEventModel model, string userId);

        Task<ErrorModel> UpdateEvent(AddOrUpdateEventModel model, string userLoginId);

        AddOrUpdateEventModel GetEventForUpdate(int eventCd);

        AjaxResponseModel LoadEventInfoByCode(string eventId);

        bool DeleteEvent(int eventCd, string userId);

        List<SelectListItem> GetLstEventLogin();

        Task<AjaxResponseModel> ImportCSVAsync(IFormFile file, string userLoginId);
    }
}
