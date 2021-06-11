using OPS.ViewModels.Admin.Master.ProductAgreementType;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
   public interface IMstProductAgreementTypeRepository<T> : IGenericRepository<T> where T : class
    {
        ProductAgreementTypeViewModel Load(ProductAgreementTypeViewModel filtering);
        bool Create(ProductAgreementTypeModel model, string userId);
        bool Update(ProductAgreementTypeModel model, string userId);
        AjaxResponseModel Remove(int cd, string userId);
        bool IsDuplicated(ProductAgreementTypeModel model);
    }
}
