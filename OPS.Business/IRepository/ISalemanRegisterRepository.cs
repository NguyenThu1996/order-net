using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace OPS.Business.IRepository
{
    public interface ISalemanRegisterRepository<T> : IGenericRepository<T> where T : class
    {
        List<SelectListItem> GetSelectListSalesman(string eventCd);

        bool AddSalemanEvent(string userId, int salemanCd);
    }
}
