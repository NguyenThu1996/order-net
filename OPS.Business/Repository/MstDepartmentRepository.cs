using Microsoft.EntityFrameworkCore.Internal;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.ViewModels.Admin.Master.Department;
using OPS.ViewModels.Shared;
using System;
using System.Linq;

namespace OPS.Business.Repository
{
    public class MstDepartmentRepository : GenericRepository<MstDepartment>, IMstDepartmentRepository<MstDepartment>
    {
        public MstDepartmentRepository(OpsContext context) : base(context)
        {

        }

        public bool Create(DepartmentModel model, string userId)
        {
            var department = new MstDepartment
            {
                Code         = model.Code.Trim(),
                Name         = model.Name.Trim(),
                NameKana     = model.NameKana.Trim(),
                IsDeleted    = false,
                InsertDate   = DateTime.Now,
                InsertUserId = userId
            };

            Add(department);

            return true;
   
        }

        public bool IsDuplicated(DepartmentModel model)
        {
            return _context.MstDepartments.Any(c => c.Code.ToLower().Equals(model.Code.ToLower().Trim())
                                                    && (model.Cd == 0 || c.Cd != model.Cd));
        }

        private IQueryable<MstDepartment> Filtering(IQueryable<MstDepartment> departments, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "code":
                    if (filtering.SortDirection == "asc")
                    {
                        departments = departments.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        departments = departments.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "name":
                    if (filtering.SortDirection == "asc")
                    {
                        departments = departments.OrderBy(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        departments = departments.OrderByDescending(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        departments = departments.OrderBy(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        departments = departments.OrderByDescending(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return departments;
        }

        public DepartmentViewModel Load(DepartmentViewModel filtering)
        {
            filtering.Keyword     = filtering.Keyword?.Trim().ToLower();
            filtering.KeywordKana = filtering.KeywordKana?.Trim().ToLower();
            var result            = new DepartmentViewModel();
            var departments       = _context.MstDepartments.Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(filtering.Keyword))
            {
                departments = departments.Where(x => x.Name.ToLower().Contains(filtering.Keyword)
                                                    || x.NameKana.ToLower().Contains(filtering.KeywordKana)
                                                    || x.NameKana.ToLower().Contains(filtering.Keyword)
                                                    || x.Code.ToLower().Contains(filtering.Keyword));
            }

            result.TotalRowsAfterFiltering = departments.Count();
            //sort And paging
            departments        = Filtering(departments, filtering);
            result.Departments = departments
                        .Select(x => new {
                            x.Cd,
                            x.Code,
                            x.Name,
                            x.NameKana
                        })
                        .AsEnumerable()
                        .Select(x => new DepartmentModel
                        {
                            Code     = x.Code,
                            Cd       = x.Cd,
                            Name     = x.Name,
                            NameKana = x.NameKana
                        }).ToList();

            return result;
        }

        public AjaxResponseModel Remove(int cd, string userId)
        {
            try
            {
                var department = _context.MstDepartments
                                   .Where(d => !d.Department1Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                                   && !d.Department2Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                                   && !d.Department3Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                                   && !d.Department4Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                                   && !d.Department5Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                                   && !d.Department6Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                                   && !d.ArtistDepartments.Any()
                                   && d.Cd == cd)
                                   .FirstOrDefault();

                if (department == null)
                {
                    return new AjaxResponseModel
                    {
                        Message = "この部署が使われている為、削除できません。",
                        Status  = false
                    };
                }

                department.IsDeleted    = true;
                department.UpdateDate   = DateTime.Now;
                department.UpdateUserId = userId;

                Update(department);

                _context.SaveChanges();

                return new AjaxResponseModel
                {
                    Message = "削除に成功しました。",
                    Status  = true
                };

            }
            catch (Exception)
            {
                return new AjaxResponseModel
                {
                    Message = "削除に失敗しました。",
                    Status  = false
                };
            }
        }

        public bool Update(DepartmentModel model, string userId)
        {
            var department = _context.MstDepartments.Find(model.Cd);

            department.Code         = model.Code.Trim();
            department.Name         = model.Name.Trim();
            department.NameKana     = model.NameKana.Trim();
            department.IsDeleted    = false;
            department.UpdateUserId = userId;
            department.UpdateDate   = DateTime.Now;

            Update(department);
            return true;
        }
    }
}
