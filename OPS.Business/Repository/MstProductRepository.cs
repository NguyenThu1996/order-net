using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master.Product;
using OPS.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OPS.Business.Repository
{
    public class MstProductRepository : GenericRepository<MstProduct>, IMstProductRepository<MstProduct>
    {
        public MstProductRepository(OpsContext context) : base(context)
        {

        }

        public AjaxResponseModel Create(ProductModel model, string userId)
        {
            var resultModel = new AjaxResponseModel();
            try
            {
                var entity = new MstProduct
                {
                    Code            = model.Code.Trim(),
                    Name            = model.Name.Trim(),
                    NameKana        = model.NameKana.Trim(),
                    ArtistCd        = model.ArtistCd,
                    Price           = model.Price,
                    OriginalName    = model.OriginalName.Trim(),
                    JappaneseName   = model.JapaneseName?.Trim(),
                    ItemName        = model.ItemName?.Trim(),
                    CategoryName    = model.CategoryName?.Trim(),
                    ItemCategory    = model.ItemCategory.Trim(),
                    InsertDate      = DateTime.Now,
                    IsDeleted       = false,
                    InsertUserId    = userId
                };

                // add technique for product
                List<ProductTechnique> listProductTechniques = AddNewProductTechniques(model, 0);

                entity.ProductTechniques = listProductTechniques;

                _context.MstProducts.Add(entity);
                _context.SaveChanges();

                resultModel.Status = true;
                resultModel.Message = "新規作成に成功しました。";
                return resultModel;
            }
            catch (Exception)
            {
                resultModel.Status = false;
                resultModel.Message = "新規作品作成に失敗しました。";
                return resultModel;
            }
        }

        public AjaxResponseModel Remove(int cd, string userId)
        {
            try
            {
                var product = _context.MstProducts
                              .Where(p => !p.Product1Contracts.Any(c=>!c.IsDeleted && c.IsCompleted)
                               && !p.Product2Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !p.Product3Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !p.Product4Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !p.Product5Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !p.Product6Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !p.Rankings.Any()
                               && p.Cd == cd)
                              .FirstOrDefault();

                if (product == null)
                {
                    return new AjaxResponseModel
                    {
                        Message = "この作品が使われている為、削除できません。",
                        Status = false
                    };
                }
                product.IsDeleted    = true;
                product.UpdateDate   = DateTime.Now;
                product.UpdateUserId = userId;

                Update(product);
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
                    Message = "この媒体が削除できません。",
                    Status  = false
                };
            }
        }

        public AjaxResponseModel Update(ProductModel model, string userId)
        {
            var resultModel = new AjaxResponseModel();
            try
            {
                var product = _context.MstProducts.Include(x => x.ProductTechniques)
                                                  .FirstOrDefault(x => x.Cd == model.Cd);

                if (product != null)
                {
                    product.ItemCategory   = model.ItemCategory.Trim();
                    product.ItemName       = model.ItemName?.Trim();
                    product.JappaneseName  = model.JapaneseName?.Trim();
                    product.Code           = model.Code.Trim();
                    product.OriginalName   = model.OriginalName.Trim();
                    product.Name           = model.Name.Trim();
                    product.CategoryName = model.CategoryName.Trim();
                    product.Price          = model.Price;
                    product.NameKana       = model.NameKana.Trim();
                    product.ArtistCd       = model.ArtistCd;
                    product.UpdateDate     = DateTime.Now;
                    product.UpdateUserId   = userId;

                    Update(product);

                    if (product.ProductTechniques != null)
                    {
                        // remove technique of product
                        _context.ProductTechniques.RemoveRange(product.ProductTechniques);

                        // add technique for product
                        List<ProductTechnique> listProductTechniques = AddNewProductTechniques(model, model.Cd);

                        _context.ProductTechniques.AddRange(listProductTechniques);
                    }

                    _context.SaveChanges();

                    resultModel.Status = true;
                    resultModel.Message = "編集に成功しました。";
                    return resultModel;
                }

                resultModel.Status = false;
                resultModel.Message = "作品が見つかりません。";
                return resultModel;
            }
            catch (Exception)
            {
                resultModel.Status = false;
                resultModel.Message = "作品編集に失敗しました。";
                return resultModel;
            }
        }

        private static List<ProductTechnique> AddNewProductTechniques(ProductModel model, int productCd)
        {
            return model.ListProductTechniques
                              .Where(p => !p.IsDelete && p.TechniqueCd > 0).GroupBy(p => p.TechniqueCd)
                .Select(x => new ProductTechnique
                {
                    ProductCd = productCd,
                    TechniqueCd = x.Key,
                })
                .ToList();
        }

        private IQueryable<MstProduct> Filtering(IQueryable<MstProduct> products, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "code":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "japaneseName":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.JappaneseName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.JappaneseName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "itemName":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.ItemName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.ItemName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "originalName":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.OriginalName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.OriginalName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "categoryName":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.CategoryName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.CategoryName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "price":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.Price).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.Price).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "name":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "artistCode":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.Artist.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.Artist.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "" +
                "itemCategory":
                    if (filtering.SortDirection == "asc")
                    {
                        products = products.OrderBy(x => x.ItemCategory).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        products = products.OrderByDescending(x => x.ItemCategory).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                        products = products.OrderBy(x => x.Artist.Code).ThenBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    break;
            }

            return products;
        }

        public ProductViewModel Load(ProductViewModel product)
        {
            product.Keyword = product.Keyword?.Trim().ToLower();
            product.keywordKana = product.keywordKana?.Trim().ToLower();

            var result = new ProductViewModel();
            var products = _context.MstProducts
                .Include(x => x.Artist)
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(product.Keyword))
            {
                products = products.Where(x => x.Name.ToLower().Contains(product.Keyword)
                                               || x.Code.ToLower().Contains(product.Keyword)
                                               || x.ItemCategory.ToLower().Contains(product.Keyword)
                                               || x.OriginalName.ToLower().Contains(product.Keyword)
                                               || x.JappaneseName.ToLower().Contains(product.Keyword)
                                               || x.ItemName.ToLower().Contains(product.Keyword)
                                               || x.NameKana.ToLower().Contains(product.Keyword)
                                               || x.NameKana.ToLower().Contains(product.keywordKana)
                                               || x.CategoryName.ToLower().Contains(product.Keyword)
                                               || x.Artist.Code.ToLower().Contains(product.Keyword)
                                               || x.ArtistCd.ToString().Contains(product.Keyword));
            }    

            if (product.ArtistCd != 0)
            {
                products = products.Where(x => (x.ArtistCd == product.ArtistCd));
            }

            result.TotalRowsAfterFiltering = products.Count();
            //Sort And Paging
            products = Filtering(products, product);

            result.Products = products
                .Select(x => new
                {
                    x.Cd,
                    x.Code,
                    x.Name,
                    x.NameKana,
                    x.ItemCategory,
                    x.ItemName,
                    x.JappaneseName,
                    x.OriginalName,
                    x.CategoryName,
                    x.Price,
                    ArtistName    = x.Artist.Name,
                    ArtisCode     = x.Artist.Code,
                    TechniqueName = x.ProductTechniques.Select(t => new
                    {
                        t.Technique.Code,
                        t.Technique.Name
                    }),
                })
                .AsEnumerable()
                .Select(x => new ProductVModel()
                {
                    Cd = x.Cd,
                    Name = x.Name,
                    ItemName = x.ItemName,
                    JapaneseName = x.JappaneseName,
                    OriginalName = x.OriginalName,
                    CategoryName = x.CategoryName,
                    Price = x.Price.HasValue ? x.Price.Value.ToString("N0") : "",
                    Code = x.Code,
                    ItemCategory = x.ItemCategory,
                    NameKana = x.NameKana,
                    ArtistCode = x.ArtisCode,
                    ArtistName = x.ArtistName,
                    TechniqueName = x.TechniqueName.OrderBy(x => x.Code).Select(y => y.Name).ToList(),
                }).ToList();
                
            return result;
        }

        public bool IsDuplicated(ProductModel model)
        {
            return _context.MstProducts.Any(c => c.Code.ToLower().Equals(model.Code.ToLower().Trim())
                                                 && (c.Cd != model.Cd)
                                                 && c.ArtistCd == model.ArtistCd
                                                 && c.ItemCategory.ToLower().Equals(model.ItemCategory.ToLower().Trim()));
        }

        public ProductModel GetProductDetails(int cd)
        {
            var entity = _context.MstProducts.Where(x => x.Cd == cd).Select(x => new
            {
                x.Cd,
                x.ArtistCd,
                x.Name,
                x.Code,
                x.ItemName,
                x.ItemCategory,
                x.OriginalName,
                x.JappaneseName,
                x.NameKana,
                x.CategoryName,
                x.Price,
                lstTechniques = x.ProductTechniques.OrderBy(t => t.Technique.Code).Select(t => new
                {
                    t.ProductCd,
                    t.TechniqueCd,
                    IsExist = (t.Technique.Technique1Contracts.Any(e => e.ProductCd1 == cd && e.IsCompleted && !e.IsDeleted)
                               || t.Technique.Technique2Contracts.Any(e => e.ProductCd2 == cd && e.IsCompleted && !e.IsDeleted)
                               || t.Technique.Technique3Contracts.Any(e => e.ProductCd3 == cd && e.IsCompleted && !e.IsDeleted)
                               || t.Technique.Technique4Contracts.Any(e => e.ProductCd4 == cd && e.IsCompleted && !e.IsDeleted)
                               || t.Technique.Technique5Contracts.Any(e => e.ProductCd5 == cd && e.IsCompleted && !e.IsDeleted)
                               || t.Technique.Technique6Contracts.Any(e => e.ProductCd6 == cd && e.IsCompleted && !e.IsDeleted))
                }),
            })
            .AsEnumerable()
            .Select(y => new ProductModel()
            {
                Cd = y.Cd,
                ArtistCd = y.ArtistCd,
                Code = y.Code,
                Name = y.Name,
                ItemName = y.ItemName,
                ItemCategory = y.ItemCategory,
                NameKana = y.NameKana,
                JapaneseName = y.JappaneseName,
                OriginalName = y.OriginalName,
                CategoryName = y.CategoryName,
                Price = y.Price,
                ListProductTechniques = y.lstTechniques.Any() ? y.lstTechniques.Select(z => new ProductTechniqueModel
                {
                    ProductCd = z.ProductCd,
                    TechniqueCd = z.TechniqueCd,
                    IsExist = z.IsExist,
                }).ToList() : new List<ProductTechniqueModel> { new ProductTechniqueModel() },
            }).FirstOrDefault();

            return entity;
        }

        public List<SelectListItem> GetSelectListTechnique(bool isNullItemFirst = false)
        {
            var techniques = _context.MstTechniques.Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.Cd,
                    x.Code,
                    x.Name,
                })
                 .OrderBy(x => x.Code)
                .Select(x => new SelectListItem()
                {
                    Value = x.Cd.ToString(),
                    Text = string.Format("{0} - {1}", x.Code, x.Name),
                }).ToList();

            if (isNullItemFirst)
            {
                techniques.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "",
                });
            }

            return techniques;
        }

        public AjaxResponseModel ImportCSV(IFormFile file, string userId)
        {
            string[] headers = new string[] { "作家コード", "作品コード", "アイテムド", "作品名", "作品名（略称）", "カナ名称", "事業分類" };
            var result = new AjaxResponseModel(false, "");
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);

                if (fileExtension.ToUpper() != ".CSV")
                {
                    result.Message = "アプロードファイルはCSVのみ指定ください。";
                    return result;
                }

                var products = new List<ImportedProductModel>();
                var errorRecords = new CustomLookup();
                var isBadRecord = false;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding(932)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<ImportedProductModelMap>();
                    csv.Configuration.TrimOptions = TrimOptions.Trim | TrimOptions.InsideQuotes;
                    csv.Configuration.BadDataFound = context =>
                    {
                        if (context.Row > 1)
                        {
                            isBadRecord = true;
                            var field = context.RawRecord.Replace("\r\n", "");
                            var message = string.Format("「{0}」はCSVフォーマットが正しくありません。", field.Length > 25 ? $"{field.Substring(0, 25)}..." : field);
                            errorRecords.Add(context.Row, message);
                        }
                    };

                    csv.Read();
                    csv.ReadHeader();
                    string[] headerRow = csv.Context.HeaderRecord;

                    if (headerRow.Length != headers.Length)
                    {
                        var message = $"以下の{headers.Length}列のCSVファイルを選択してください。\n";
                        message += string.Join("\n", headers.Select(h => $"「{h}」"));

                        result.Message = message;
                        return result;
                    }

                    while (csv.Read())
                    {
                        if (!isBadRecord)
                        {
                            if (csv.Context.Record.Length != headers.Length)
                            {
                                errorRecords.Add(csv.Context.Row, "カラム数があっていません。");
                            }

                            var validationResults = new List<ValidationResult>();
                            var product = csv.GetRecord<ImportedProductModel>();
                            var context = new ValidationContext(product, null, null);
                            var isValid = Validator.TryValidateObject(product, context, validationResults, true);

                            if (isValid)
                            {
                                products.Add(product);
                            }
                            else
                            {
                                foreach (var valResult in validationResults)
                                {
                                    errorRecords.Add(csv.Context.Row, valResult.ErrorMessage);
                                }
                            }
                        }

                        isBadRecord = false;
                    }
                }

                if (errorRecords.Count > 0)
                {
                    var message = "データが無効です。\n";
                    message += string.Join("\n", errorRecords.Select(a => $"{a.Key}行目・{a.Value}"));
                    message += "\n再度確認してください。";

                    result.Message = message;
                    return result;
                }

                if (products.Count > 0)
                {
                    //check duplicates
                    var duplicateDic = products.GroupBy(p => new { p.ArtistCode, p.Code, p.ItemCategory })
                                                .Where(p => p.Count() > 1)
                                                .ToDictionary(p => $"{p.Key.ArtistCode}・{p.Key.Code}・{p.Key.ItemCategory}", p => p.Count());

                    if (duplicateDic.Count > 0)
                    {
                        var message = "作家コード・作品コード・アイテムドは以下の値が重複しています。\n";
                        message += string.Join("\n", duplicateDic.Select(p => $"「{p.Key}」（{p.Value}回）"));
                        message += "\n再度確認してください。";

                        result.Message = message;
                        return result;
                    }

                    var artistCodeCdDic = new Dictionary<string, int>();
                    var artistCodeList = products.Select(p => p.ArtistCode).Distinct().ToList();

                    foreach(var artistCode in artistCodeList)
                    {
                        var artistCd = _context.MstArtists.Where(a => a.Code == artistCode).Select(a => a.Cd).FirstOrDefault();
                        artistCodeCdDic.Add(artistCode, artistCd);
                    }

                    if(artistCodeCdDic.Any(cc => cc.Value == 0))
                    {
                        var message = "以下の作家コードが存在していません。\n";
                        message += string.Join("\n", artistCodeCdDic.Where(cc => cc.Value == 0).Select(cc => $"「{cc.Key}」"));
                        message += "\n再度確認してください。";

                        result.Message = message;
                        return result;
                    }

                    var now = DateTime.Now;
                    var newProductList = new List<MstProduct>();
                    foreach (var csvProduct in products)
                    {
                        var productInDb = _context.MstProducts.FirstOrDefault(p => p.ArtistCd == artistCodeCdDic[csvProduct.ArtistCode]
                                                                                    && p.Code == csvProduct.Code
                                                                                    && p.ItemCategory == csvProduct.ItemCategory);
                        if (productInDb != null)
                        {
                            productInDb.Name = csvProduct.Name;
                            productInDb.OriginalName = csvProduct.OriginalName;
                            productInDb.NameKana = csvProduct.NameKana;
                            productInDb.CategoryName = csvProduct.CategoryName;
                            productInDb.UpdateUserId = userId;
                            productInDb.UpdateDate = now;
                        }
                        else
                        {
                            var newProduct = new MstProduct
                            {
                                ArtistCd = artistCodeCdDic[csvProduct.ArtistCode],
                                Code = csvProduct.Code,
                                ItemCategory = csvProduct.ItemCategory,
                                Name = csvProduct.Name,
                                OriginalName = csvProduct.OriginalName,
                                NameKana = csvProduct.NameKana,
                                CategoryName = csvProduct.CategoryName,
                                InsertUserId = userId,
                                InsertDate = now,
                                UpdateUserId = userId,
                                UpdateDate = now,
                            };

                            newProductList.Add(newProduct);
                        }
                    }

                    if (newProductList.Count > 0)
                    {
                        _context.MstProducts.AddRange(newProductList);
                    }

                    _context.SaveChanges();

                    result.Status = true;
                    return result;
                }
                else
                {
                    result.Message = "CSVファイルにデータがありません。再度確認してください。";
                    return result;
                }
            }
            catch
            {
                result.Status = false;
                result.Message = "エラーが発生しました。もう一度お試しください。";
                return result;
            }
        }
    }
}
