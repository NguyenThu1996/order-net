using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.ViewModels.Admin.Master.Organization;
using OPS.ViewModels.Shared;
using System;
using System.Linq;

namespace OPS.Business.Repository
{
    public class MstOrganizationRepository : GenericRepository<MstOrganization>, IMstOrganizationRepository<MstOrganization>
    {
        public MstOrganizationRepository(OpsContext context) :base(context)
        {

        }

        public bool Create(OrganizationModel model, string userId)
        {
            var organizarion = new MstOrganization
            {
                Code         = model.Code.Trim(),
                Name         = model.Name.Trim(),
                NameKana     = model.NameKana.Trim(),
                InsertDate   = DateTime.Now,
                IsDeleted    = false,
                InsertUserId = userId
            };

            Add(organizarion);

            return true;
        }

        public OrganizationViewModel Load(OrganizationViewModel filtering)
        {
            filtering.Keyword     = filtering.Keyword?.Trim().ToLower();
            filtering.KeywordKana = filtering.KeywordKana?.Trim().ToLower();
            var result            = new OrganizationViewModel();
            var organizations     = _context.MstOrganizations.Where(x => !x.IsDeleted);

            if(!string.IsNullOrEmpty(filtering.Keyword))
            {
                organizations = organizations.Where(x => x.Name.ToLower().Contains(filtering.Keyword)
                                                        || x.NameKana.ToLower().Contains(filtering.KeywordKana)
                                                        || x.NameKana.ToLower().Contains(filtering.Keyword)
                                                        || x.Code.ToLower().Contains(filtering.Keyword));
            }

            result.TotalRowsAfterFiltering = organizations.Count();
            //sort And paging
            organizations = Filtering(organizations, filtering);
            result.Organizations = organizations
                        .Select(x => new {
                            x.Cd,
                            x.Code,
                            x.Name,
                            x.NameKana
                        })
                        .AsEnumerable()
                        .Select(x => new OrganizationModel
                        {   
                            Code     = x.Code,
                            Cd       = x.Cd,
                            Name     = x.Name,
                            NameKana = x.NameKana
                        }).ToList();

            return result;
        }

        private IQueryable<MstOrganization> Filtering(IQueryable<MstOrganization> organizations, OpsFilteringDataTableModel filtering)
        {
            switch(filtering.SortColumnName)
            {
                case "code":
                    if(filtering.SortDirection == "asc")
                    {
                        organizations = organizations.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        organizations = organizations.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "name":
                    if (filtering.SortDirection == "asc")
                    {
                        organizations = organizations.OrderBy(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        organizations = organizations.OrderByDescending(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        organizations = organizations.OrderBy(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        organizations = organizations.OrderByDescending(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return organizations;
        }

        public AjaxResponseModel Remove(int cd, string userId)
        {
            try 
            {
                var organization = _context.MstOrganizations
                                   .Include(x => x.Contracts)
                                   .FirstOrDefault(x => x.Cd == cd);
                if(organization.Contracts.Any(x => !x.IsDeleted && x.IsCompleted))
                {
                    return new AjaxResponseModel
                    {
                        Message = "この組織が使われている為、削除できません。",
                        Status  = false
                    };
                }

                organization.IsDeleted    = true;
                organization.UpdateDate   = DateTime.Now;
                organization.UpdateUserId = userId;

                Update(organization);

                _context.SaveChanges();

                return new AjaxResponseModel
                {
                    Message = "削除に成功しました。",
                    Status  = true
                };

            }
            catch(Exception)
            {
                return new AjaxResponseModel
                {
                    Message = "削除に失敗しました。",
                    Status  = false
                };
            }
        }

        public bool Update(OrganizationModel model, string userId)
        {
            var organization          = _context.MstOrganizations.Find(model.Cd);
            organization.Code         = model.Code.Trim();
            organization.Name         = model.Name.Trim();
            organization.NameKana = model.NameKana.Trim();
            organization.UpdateDate   = DateTime.Now;
            organization.UpdateUserId = userId;

            Update(organization);

            return true;
        }

        public bool IsDuplicated(OrganizationModel model)
        {
            return _context.MstOrganizations.Any(c => c.Code.ToLower().Equals(model.Code.ToLower().Trim()) 
                                                      && (model.Cd == 0 || c.Cd != model.Cd));
        }
    }
}
