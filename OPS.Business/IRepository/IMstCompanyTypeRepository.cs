using OPS.ViewModels.Admin.Master.CompanyType;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
    public interface IMstCompanyTypeRepository<T> : IGenericRepository<T> where T:class
    {
        IndexViewModel Load(OpsFilteringDataTableModel filtering);
        AjaxResponseModel Create(MasterCompanyTypeModel companyType, string userId);
        AjaxResponseModel Update(MasterCompanyTypeModel companyType, string userId);
        AjaxResponseModel Delete(int cd, string userId);
    }
}
