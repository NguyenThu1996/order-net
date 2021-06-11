using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using OPS.ViewModels.Admin.Master.Tax;
using OPS.ViewModels.Shared;
using System;
using System.Linq;

namespace OPS.Business.Repository
{
    public class MstTaxRepository :GenericRepository<MstTax>, IMstTaxRepository<MstTax>
    {
        public MstTaxRepository(OpsContext context) : base(context)
        {

        }

        public AjaxResponseModel Create(MstTax tax, string userID)
        {
            if(DuplicateDate(tax.StartDate, tax.EndDate, 0))
            {
                Add(tax);
                return new AjaxResponseModel(true, null);
            }
            return new AjaxResponseModel(false, "重複した日付");// Duplicate dates
        }

        public AjaxResponseModel Delete(string cd, string userID)
        {
            var taxe = _context.MstTaxes.FirstOrDefault(q => q.Cd.ToString().Equals(cd));
            if(taxe == null)
            {
                return new AjaxResponseModel(false, "税は存在しません。");
            }
            taxe.IsDeleted = true;
            taxe.UpdateDate = DateTime.Now;
            taxe.UpdateUserId = userID;
            Update(taxe);
            return new AjaxResponseModel(true, null);
        }

        public bool DuplicateDate(DateTime startDate, DateTime? endDate, int cd)
        {
            var context = _context.MstTaxes.Where(q => !q.IsDeleted && q.Cd!=cd);
            foreach(var i in context)
            {
                if (i.StartDate == startDate || i.EndDate == endDate || i.StartDate == endDate || i.EndDate == startDate)
                    return false;
            }
            return true;
        }

        public IndexViewModel Load(OpsFilteringDataTableModel filtering)
        {
            filtering.Keyword = filtering.Keyword?.Trim().ToString();
            var result = new IndexViewModel();
           
            var taxes = _context.MstTaxes.Where(q => !q.IsDeleted);

            if (!string.IsNullOrEmpty(filtering.Keyword))
            {
                taxes = taxes.Where(q => q.StartDate.ToString().Contains(filtering.Keyword)
                        || q.EndDate.ToString().Contains(filtering.Keyword)
                        || q.Value.ToString().Equals(filtering.Keyword));
            }

            result.TotalRowsAfterFiltering = taxes.Count();
            taxes = Filtering(taxes, filtering);

            result.Taxes = taxes
                .Select(q => new
                {
                    q.Cd,
                    q.StartDate,
                    q.EndDate,
                    q.Value
                })
                .AsEnumerable()
                .Select(q => new MasterTaxItemModel()
                {
                    Cd = q.Cd,
                    StartDate = q.StartDate.ToString(Constants.ExactDateFormat),
                    EndDate = q.EndDate?.ToString(Constants.ExactDateFormat),
                    Value = q.Value
                }).ToList();
            return result;
        }

        private IQueryable<MstTax> Filtering(IQueryable<MstTax> taxes, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "StartDate":
                    if (filtering.SortDirection == "asc")
                    {
                        taxes = taxes.OrderBy(x => x.StartDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        taxes = taxes.OrderByDescending(x => x.StartDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "EndDate":
                    if (filtering.SortDirection == "asc")
                    {
                        taxes = taxes.OrderBy(x => !x.EndDate.HasValue).ThenBy(x => x.EndDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        taxes = taxes.OrderByDescending(x => !x.EndDate.HasValue).ThenByDescending(x => x.EndDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "Value":
                    if (filtering.SortDirection == "asc")
                    {
                        taxes = taxes.OrderBy(x => x.Value).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        taxes = taxes.OrderByDescending(x => x.Value).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                    if (filtering.SortDirection == "asc")
                    {
                        taxes = taxes.OrderBy(x => x.EndDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        taxes = taxes.OrderByDescending(x => x.EndDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return taxes;
        }

        public AjaxResponseModel Update(MstTax tax, string userID)
        {
            var query = _context.MstTaxes.AsNoTracking().First(q => q.Cd == tax.Cd);
            if (query == null)
            {
                return new AjaxResponseModel(false, "税は存在しません。");
            }
            if( DuplicateDate(tax.StartDate, tax.EndDate, tax.Cd))
            {
                query.UpdateDate = DateTime.Now;
                query.UpdateUserId = userID;
                query.StartDate = tax.StartDate;
                query.EndDate = tax.EndDate;
                query.Value = tax.Value;
                Update(tax);
                return new AjaxResponseModel(true, null);
            }
            else
            {
                return new AjaxResponseModel(false, "重複した日付");// Duplicate dates
            }
        }
    }
}
