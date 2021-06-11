using OPS.ViewModels.Shared;
using System;
using OPS.ViewModels.Admin.Master.Tax;
using OPS.Entity.Schemas;

namespace OPS.Business.IRepository
{
    public interface IMstTaxRepository<T> : IGenericRepository<T> where T : class
    {
        IndexViewModel Load(OpsFilteringDataTableModel filtering);
        AjaxResponseModel Create(MstTax tax, string userID);
        AjaxResponseModel Update(MstTax tax, string userID);
        AjaxResponseModel Delete(string cd, string userID);
        bool DuplicateDate(DateTime startDay, DateTime? endDate, int cd);
    }
}
