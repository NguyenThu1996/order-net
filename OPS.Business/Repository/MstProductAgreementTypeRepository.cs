using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.ViewModels.Admin.Master.ProductAgreementType;
using OPS.ViewModels.Shared;
using System;
using System.Linq;

namespace OPS.Business.Repository
{
    public class MstProductAgreementTypeRepository : GenericRepository<MstProductAgreementType>,
                                                     IMstProductAgreementTypeRepository<MstProductAgreementType>
    {
        public MstProductAgreementTypeRepository(OpsContext context) : base(context)
        {

        }

        public bool Create(ProductAgreementTypeModel model, string userId)
        {
            var ProductAgreeType = new MstProductAgreementType
            {
                Code         = model.Code.Trim(),
                Name         = model.Name.Trim(),
                NameKana     = model.NameKana.Trim(),
                InsertDate   = DateTime.Now,
                IsDeleted    = false,
                InsertUserId = userId
            };

            Add(ProductAgreeType);

            return true;
        }

        public bool IsDuplicated(ProductAgreementTypeModel model)
        {
            return _context.MstProductAgreementTypes.Any(c => c.Code.ToLower().Equals(model.Code.ToLower().Trim())
                                                             && (model.Cd == 0 || c.Cd != model.Cd));
        }

        private IQueryable<MstProductAgreementType> Filtering(IQueryable<MstProductAgreementType> productAgreementTypes, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "code":
                    if (filtering.SortDirection == "asc")
                    {
                        productAgreementTypes = productAgreementTypes.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        productAgreementTypes = productAgreementTypes.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "name":
                    if (filtering.SortDirection == "asc")
                    {
                        productAgreementTypes = productAgreementTypes.OrderBy(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        productAgreementTypes = productAgreementTypes.OrderByDescending(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        productAgreementTypes = productAgreementTypes.OrderBy(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        productAgreementTypes = productAgreementTypes.OrderByDescending(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return productAgreementTypes;
        }

        public ProductAgreementTypeViewModel Load(ProductAgreementTypeViewModel filtering)
        {
            filtering.Keyword = filtering.Keyword?.Trim().ToLower();
            filtering.KeywordKana = filtering.KeywordKana?.Trim().ToLower();
            var result = new ProductAgreementTypeViewModel();
            var productAgreementTypes = _context.MstProductAgreementTypes.Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(filtering.Keyword))
            {
                productAgreementTypes = productAgreementTypes.Where(x => x.Name.ToLower().Contains(filtering.Keyword)
                                                        || x.NameKana.ToLower().Contains(filtering.KeywordKana)
                                                        || x.NameKana.ToLower().Contains(filtering.Keyword)
                                                        || x.Code.ToLower().Contains(filtering.Keyword));
                
            }

            result.TotalRowsAfterFiltering = productAgreementTypes.Count();
            //sort And paging
            productAgreementTypes = Filtering(productAgreementTypes, filtering);
            result.ProductAgreementTypes = productAgreementTypes
                        .Select(x => new {
                            x.Cd,
                            x.Code,
                            x.Name,
                            x.NameKana
                        })
                        .AsEnumerable()
                        .Select(x => new ProductAgreementTypeModel
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
                var ProductAgreeType = _context.MstProductAgreementTypes
                                       .Include(x => x.Contracts)
                                       .FirstOrDefault(x => x.Cd == cd);
                if (ProductAgreeType.Contracts.Any(x =>!x.IsDeleted && x.IsCompleted))
                {
                    return new AjaxResponseModel
                    {
                        Message = "この同意書が使われている為、削除できません。",
                        Status  = false
                    };
                }

                ProductAgreeType.IsDeleted    = true;
                ProductAgreeType.UpdateDate   = DateTime.Now;
                ProductAgreeType.UpdateUserId = userId;

                Update(ProductAgreeType);

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

        public bool Update(ProductAgreementTypeModel model, string userId)
        {
            var ProductAgreeType          = _context.MstProductAgreementTypes.Find(model.Cd);
            ProductAgreeType.Code         = model.Code.Trim();
            ProductAgreeType.Name         = model.Name.Trim();
            ProductAgreeType.NameKana     = model.NameKana.Trim();
            ProductAgreeType.UpdateDate   = DateTime.Now;
            ProductAgreeType.UpdateUserId = userId;

            Update(ProductAgreeType);

            return true;
        }
    }
}
