using Microsoft.AspNetCore.Mvc.Rendering;
using OPS.ViewModels.Admin.Map;
using OPS.ViewModels.Admin.Map.MapForPrint;
using OPS.ViewModels.Shared;
using System.Collections.Generic;

namespace OPS.Business.IRepository
{
    public interface IMapRepository
    {
        IndexViewModel GetListMapItem(IndexViewModel model);

        bool CheckExistMap(int EventCd);

        MapModel GetMapInfo(int EventCd, int? MapCd);
        //List<SelectListItem> GetListMediaNew();
        //List<SelectListItem> GetListMediaEffort();

        List<SelectListItem> GetLayout();
        AjaxResponseResultModel<int> AddMap(MapModel model, string userId);
        AjaxResponseResultModel<int> UpdateMap(MapModel model, string userId);
        MapPrint GetInfoPrintMap(int MapCd);
    }
}
