using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master;
using OPS.ViewModels.Admin.Master.Salesman;
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
    public class MstSalesmanRepository : GenericRepository<MstSalesman>, IMstSalesmanRepository<MstSalesman>
    {
        public MstSalesmanRepository(OpsContext context) : base(context)
        {

        }

        public IndexViewModel Load(SearchSalemanModel filter)
        {
            var result = new IndexViewModel();

            filter.Keyword = filter.Keyword?.Trim().ToLower();
            filter.KeywordKana = filter.KeywordKana?.Trim().ToLower();

            var mstSalesman = _context.MstSalesmen.Where(s => !s.IsDeleted);

            if (!string.IsNullOrEmpty(filter.Keyword) || !string.IsNullOrEmpty(filter.KeywordKana))
            {
                mstSalesman = mstSalesman.Where(x => x.Code.ToLower().Contains(filter.Keyword) 
                                                     || x.Name.ToLower().Contains(filter.Keyword)
                                                     || x.NameKana.ToLower().Contains(filter.Keyword)
                                                     || x.NameKana.ToLower().Contains(filter.KeywordKana));
            }
            

            result.TotalRowsAfterFiltering = mstSalesman.Count();

            mstSalesman = Filtering(mstSalesman, filter);

            result.ListSalesman = mstSalesman.Select(x => new
            {
                x.Cd,
                x.Code,
                x.Name,
                x.NameKana
            })
            .AsEnumerable()
            .Select(y => new SalesmanModel()
            {
                Cd = y.Cd,
                Name = y.Name,
                SalemanNameKana = y.NameKana,
                Code = y.Code
            })
            .ToList();

            return result;
        }

        private IQueryable<MstSalesman> Filtering(IQueryable<MstSalesman> salesman, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "Code":
                    if (filtering.SortDirection == "asc")
                    {
                        salesman = salesman.OrderBy(x => x.Code)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        salesman = salesman.OrderByDescending(x => x.Code)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "Name":
                    if (filtering.SortDirection == "asc")
                    {
                        salesman = salesman.OrderBy(x => x.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        salesman = salesman.OrderByDescending(x => x.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "SalemanNameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        salesman = salesman.OrderBy(x => x.NameKana.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        salesman = salesman.OrderByDescending(x => x.NameKana.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                    salesman = salesman.OrderBy(x => x.Code)
                        .Skip(filtering.Start).Take(filtering.Length);
                    break;
            }
            return salesman;
        }

        public AjaxResponseModel Create(SalesmanModel model, string userId)
        {
            try
            {
                var saleman = new MstSalesman()
                {
                    Code = model.Code.Trim(),
                    Name = model.Name.Trim(),
                    NameKana = model.SalemanNameKana.Trim(),
                    InsertDate = DateTime.Now,
                    InsertUserId = userId,
                    UpdateDate = DateTime.Now,
                    UpdateUserId = userId
                };

                Add(saleman);
                return new AjaxResponseModel(true, null);
            } 
            catch (Exception e)
            {
                return new AjaxResponseModel(false, e.Message);
            }
        }

        public AjaxResponseModel Update(SalesmanModel model, string userId)
        {
            try
            {
                var saleman = _context.MstSalesmen.FirstOrDefault(x => x.Cd == model.Cd);

                if (saleman != null)
                {
                    saleman.Code = model.Code.Trim();
                    saleman.Name = model.Name.Trim();
                    saleman.NameKana = model.SalemanNameKana.Trim();
                    saleman.UpdateDate = DateTime.Now;
                    saleman.UpdateUserId = userId;

                    Update(saleman);

                    return new AjaxResponseModel(true, null);
                }

                return new AjaxResponseModel(false, "この担当者が見つかりません。");
            }
            catch (Exception e)
            {
                return new AjaxResponseModel(false, e.Message);
            }
        }

        public bool IsDuplicated(SalesmanModel model)
        {
            var isDuplicated = _context.MstSalesmen.AsQueryable();

            if (model.Cd == 0)
            {
                isDuplicated = isDuplicated.Where(x => x.Code.ToLower().Equals(model.Code.ToLower().Trim()));
            }
            else
            {
                isDuplicated = isDuplicated.Where(x => x.Code.ToLower().Equals(model.Code.ToLower().Trim())
                                                    && x.Cd != model.Cd);
            }

            return isDuplicated.Any();
        }

        public SalesmanModel GetSalemanForUpdate(int cd)
        {
            var saleman = _context.MstSalesmen.Where(x => x.Cd == cd && !x.IsDeleted).Select(x => new
            {
                x.Cd,
                x.Code,
                x.Name,
                x.NameKana
            }).AsEnumerable().Select(y => new SalesmanModel()
            {
                Cd = y.Cd,
                Code = y.Code,
                Name = y.Name,
                SalemanNameKana = y.NameKana,
            }).FirstOrDefault();

            return saleman;
        }

        public AjaxResponseModel DeleteSaleman(int cd, string userId)
        {
            var saleman = _context.MstSalesmen
                                  .Where(x => !x.Events.Any(e => !e.IsDeleted)
                                      && !x.ARespContracts.Any(a => !a.IsDeleted && a.IsCompleted)
                                      && !x.CRespContracts.Any(c => !c.IsDeleted && c.IsCompleted)
                                      && !x.SRespContracts.Any(s => !s.IsDeleted && s.IsCompleted)
                                      && !x.RespOrderReports.Any()
                                      && x.Cd == cd).FirstOrDefault();
            if (saleman == null)
            {
                return new AjaxResponseModel(false, "この担当者が使われているため、削除できません。");
            }

            saleman.IsDeleted = true;
            saleman.UpdateDate = DateTime.Now;
            saleman.UpdateUserId = userId;

            Update(saleman);

            return new AjaxResponseModel(true, null);
        }

        public AjaxResponseModel ImportCSV(IFormFile file, string userId)
        {
            string[] headers = new string[] { "担当者コード", "担当者名" };
            var result = new AjaxResponseModel(false, "");
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);

                if (fileExtension.ToUpper() != ".CSV")
                {
                    result.Message = "アプロードファイルはCSVのみ指定ください。";
                    return result;
                }

                var salesmen = new List<ImportedSalesmanModel>();
                var errorRecords = new CustomLookup();
                var isBadRecord = false;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding(932)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<ImportedSalesmanModelMap>();
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
                        message += string.Join("、", headers.Select(h => $"「{h}」"));

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
                            var salesman = csv.GetRecord<ImportedSalesmanModel>();
                            var context = new ValidationContext(salesman, null, null);
                            var isValid = Validator.TryValidateObject(salesman, context, validationResults, true);

                            if (isValid)
                            {
                                salesmen.Add(salesman);
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

                if (salesmen.Count > 0)
                {
                    //check duplicates
                    var duplicateDic = salesmen.GroupBy(s => s.Code)
                                                .Where(s => s.Count() > 1)
                                                .ToDictionary(s => s.Key, s => s.Count());

                    if (duplicateDic.Count > 0)
                    {
                        var message = "担当者コードは以下の値が重複しています。\n";
                        message += string.Join("\n", duplicateDic.Select(t => $"「{t.Key}」（{t.Value}回）"));
                        message += "\n再度確認してください。";

                        result.Message = message;
                        return result;
                    }

                    var now = DateTime.Now;
                    var newSalesmanList = new List<MstSalesman>();
                    foreach (var csvSalesman in salesmen)
                    {
                        var salesmanInDb = _context.MstSalesmen.FirstOrDefault(t => t.Code == csvSalesman.Code);
                        if (salesmanInDb != null)
                        {
                            salesmanInDb.Name = csvSalesman.Name;
                            salesmanInDb.UpdateUserId = userId;
                            salesmanInDb.UpdateDate = now;
                        }
                        else
                        {
                            var newSalesman = new MstSalesman
                            {
                                Code = csvSalesman.Code,
                                Name = csvSalesman.Name,
                                InsertUserId = userId,
                                InsertDate = now,
                                UpdateUserId = userId,
                                UpdateDate = now,
                            };

                            newSalesmanList.Add(newSalesman);
                        }
                    }

                    if (newSalesmanList.Count > 0)
                    {
                        _context.MstSalesmen.AddRange(newSalesmanList);
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
