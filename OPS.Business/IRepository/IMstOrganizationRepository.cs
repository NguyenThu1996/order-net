using OPS.ViewModels.Admin.Master.Organization;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
    public interface IMstOrganizationRepository<T> : IGenericRepository<T> where T : class 
    {
        OrganizationViewModel Load(OrganizationViewModel filtering);
        bool Create(OrganizationModel model, string userId);
        bool Update(OrganizationModel model, string userId);
        AjaxResponseModel Remove(int cd, string userId);
        bool IsDuplicated(OrganizationModel model);
    }
}
