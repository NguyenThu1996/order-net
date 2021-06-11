using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.ViewModels.Admin.Master.CompanyType;
using OPS.ViewModels.Shared;
using System;
using System.Linq;

namespace OPS.Business.Repository
{
    public class MstCompanyTypeRepository : GenericRepository<MstCompanyType>, IMstCompanyTypeRepository<MstCompanyType>
    {
        public MstCompanyTypeRepository(OpsContext context): base(context)
        {
        }
        public AjaxResponseModel Create(MasterCompanyTypeModel companyType, string userId)
        {
            var entity = new MstCompanyType
            {
                Code = companyType.Code,
                Name = companyType.Name,
                InsertDate = DateTime.Now,
                InsertUserId = userId,
                IsDeleted = false
            };
            if (IsDuplicated(entity.Code, companyType.Cd))
                return new AjaxResponseModel(false, "IsDuplicated");
            Add(entity);
            return new AjaxResponseModel(true, null);
        }

        public AjaxResponseModel Delete(int cd, string userId)
        {
            var query = _context.MstCompanyTypes
                       .Include(q => q.Contracts)
                       .FirstOrDefault(q => q.Cd == cd);
            if (query.Contracts.Any())
                return new AjaxResponseModel(false, "この会社種類が使われている為、削除できません。");
            query.IsDeleted = true;
            query.UpdateDate = DateTime.Now;
            query.UpdateUserId = userId;
            Update(query);
            return new AjaxResponseModel(true, null);
        }

        public IndexViewModel Load(OpsFilteringDataTableModel filtering)
        {
            filtering.Keyword = filtering.Keyword?.Trim().ToString();
            var result = new IndexViewModel();
            var companyType = _context.MstCompanyTypes.Where(q => !q.IsDeleted);
            if(!string.IsNullOrEmpty(filtering.Keyword))
            {
                companyType = companyType.Where(q => q.Code.ToLower().Contains(filtering.Keyword.ToLower())
                    || q.Name.ToLower().Contains(filtering.Keyword.ToLower()));
            }
            result.TotalRowsAfterFiltering = companyType.Count();
            companyType = Filtering(companyType, filtering);
            result.CompanyTypes = companyType
                .Select(q => new
                {
                    q.Cd,
                    q.Code,
                    q.Name
                })
                .AsEnumerable()
                .Select(q => new MasterCompanyTypeModel()
                {
                    Cd = q.Cd,
                    Code = q.Code,
                    Name = q.Name
                }).ToList();
            return result;
        }

        public AjaxResponseModel Update(MasterCompanyTypeModel companyType, string userId)
        {
            var query = _context.MstCompanyTypes.FirstOrDefault(q => q.Cd == companyType.Cd);
            if(query == null)
                return new AjaxResponseModel(false, "この会社種類が存在していません。");

            if (IsDuplicated(companyType.Code, companyType.Cd))
                return new AjaxResponseModel(false, "IsDuplicated");

            query.Code = companyType.Code;
            query.Name = companyType.Name;
            query.UpdateDate = DateTime.Now;
            query.UpdateUserId = userId;
            Update(query);
            return new AjaxResponseModel(true, null);
        }
        private IQueryable<MstCompanyType> Filtering(IQueryable<MstCompanyType> companyTypes, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "Cd":
                    if (filtering.SortDirection == "asc")
                    {
                        companyTypes = companyTypes.OrderBy(x => x.Cd).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        companyTypes = companyTypes.OrderByDescending(x => x.Cd).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "Code":
                    if (filtering.SortDirection == "asc")
                    {
                        companyTypes = companyTypes.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        companyTypes = companyTypes.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "Name":
                    if (filtering.SortDirection == "asc")
                    {
                        companyTypes = companyTypes.OrderBy(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        companyTypes = companyTypes.OrderByDescending(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                    if (filtering.SortDirection == "asc")
                    {
                        companyTypes = companyTypes.OrderBy(x => x.Cd).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        companyTypes = companyTypes.OrderByDescending(x => x.Cd).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return companyTypes;
        }
        private bool IsDuplicated(string code, int cd)
        {
            var query = _context.MstCompanyTypes.FirstOrDefault(q => q.Code.ToLower().Equals(code.Trim().ToLower()) && !q.IsDeleted && q.Cd != cd);
            return query != null;
        }
    }
}
