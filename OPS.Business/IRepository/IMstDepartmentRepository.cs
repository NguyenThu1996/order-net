using OPS.ViewModels.Admin.Master.Department;
using OPS.ViewModels.Shared;

namespace OPS.Business.IRepository
{
    public interface IMstDepartmentRepository<T> : IGenericRepository<T> where T : class
    {
        DepartmentViewModel Load(DepartmentViewModel model);
        bool Create(DepartmentModel model, string userId);
        bool Update(DepartmentModel model, string userId);
        AjaxResponseModel Remove(int cd, string userId);
        bool IsDuplicated(DepartmentModel model);
    }
}
