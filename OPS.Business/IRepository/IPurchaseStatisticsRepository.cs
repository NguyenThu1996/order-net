using OPS.Entity.Schemas;
using OPS.ViewModels.Admin.PurchaseStatistics;

namespace OPS.Business.IRepository
{
    public interface IPurchaseStatisticsRepository
    {
        IndexViewModel Load(IndexViewModel filtering);

        PurchaseStatisticsDetailModel GetListOfContract(SearchFormModel model);

        PurchaseStatistics CreatePurchaseStatistics(PurchaseStatisticsDetailModel model, string userId);

        PurchaseStatisticsDetailModel GetListOfContractByPurchaseStatisticsCd(SearchFormModel model);

        PurchaseStatistics UpdatePurchaseStatistics(PurchaseStatisticsDetailModel model, string userId);
    }
}
