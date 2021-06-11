using OPS.ViewModels.Shared;
using System.Collections.Generic;
using OPS.ViewModels.Admin.Master.Artist;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace OPS.Business.IRepository
{
    public interface IMstArtistRepository<T> : IGenericRepository<T> where T : class
    {
        ArtistViewModel Load(ArtistViewModel model);
        AjaxResponseModel Create(ArtistModel model, string userId);
        AjaxResponseModel Update(ArtistModel model, string userId);
        AjaxResponseModel Remove(int cd, string userId);
        bool IsDuplicated(ArtistModel model);
        List<SelectListItem> GetSelectListArtist();
        ArtistModel GetArtistDetails(int cd);
        AjaxResponseModel ImportCSV(IFormFile file, string userLoginId);
    }
}
