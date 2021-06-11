using OPS.ViewModels.Admin.OrderReport;
using OPS.ViewModels.Shared;
using System.Collections.Generic;

namespace OPS.Business.IRepository
{
    public interface IOrderReportRepository
    {
        IndexViewModel Load(IndexViewModel filtering);

        bool IsOrderReportExisted(ModalSearchModel model);

        OrderReportDetailModel GetOrderReportForm(ModalSearchModel model);

        int SaveOrderReport(OrderReportDetailModel model, string userId);

        OrderReportDetailModel GetOrderReportDetail(ModalSearchModel model);

        List<OrderReportDetailModel> GetAllOrderReportsForPreview(int eventCd);

        OrderReportDetailModel GetOrderReportDetailForEdit(ModalSearchModel model);

        AjaxResponseResultModel<int> UpdateOrderReport(OrderReportDetailModel model, string userId);
    }
}
