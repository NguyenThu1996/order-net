using Microsoft.AspNetCore.Mvc.Rendering;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OPS.Business.Repository
{
    public class SalemanRegisterRepository : GenericRepository<MstSalesman>, ISalemanRegisterRepository<MstSalesman>
    {
        public SalemanRegisterRepository(OpsContext context) : base(context)
        {
        }

        public List<SelectListItem> GetSelectListSalesman(string userId)
        {
            var result = _context.MstSalesmen
                .Where(s => !s.IsDeleted && !s.EventSalesAssigments.Where(e => e.Event.ApplicationUserId == userId && s.Cd == e.SalesmanCd).Any())
                .Select(s => new
                {
                    s.Cd,
                    s.Code,
                    s.Name,
                    s.NameKana
                })
                .OrderBy(s => s.Code)
                .AsEnumerable()
                .Select(s => new SelectListItem
                {
                    Value = s.Cd.ToString(),
                    Text = string.Format("{0} - {1}", s.Code, s.Name),
                }).ToList();

            return result;
        }

        public bool AddSalemanEvent(string userId, int salemanCd)
        {
            var eventCd = _context.MstEvents.FirstOrDefault(e => e.ApplicationUserId == userId);
            
            if (eventCd != null)
            {
                var newSaleman = new EventSalesAssigment
                {
                    EventCd = eventCd.Cd,
                    SalesmanCd = salemanCd,
                    InsertDate = DateTime.Now,
                    InsertUserId = userId,
                    UpdateDate = DateTime.Now,
                    UpdateUserId = userId,
                };

                _context.EventSalesAssigments.Add(newSaleman);
                _context.SaveChanges();

                return true;
            }

            return false;
        }
    }
}
