using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master.Career;
using OPS.ViewModels.Shared;

namespace OPS.Business.Repository
{
    public class MstCareerRepository : GenericRepository<MstCareer>, IMstCareerRepository<MstCareer>
    {
        public MstCareerRepository(OpsContext context) : base(context)
        {

        }

        public CareerViewModel Load(CareerViewModel filtering)
        {
            filtering.Keyword = filtering.Keyword?.Trim().ToLower();
            filtering.keywordKana = filtering.keywordKana?.Trim().ToLower();
            var result = new CareerViewModel();
            var careers = _context.MstCareers.AsQueryable();

            result.TotalRows = careers.Count();
            careers = careers.Where(x => !x.IsDeleted);
            if (!string.IsNullOrEmpty(filtering.Keyword))
            {
                careers = careers.Where(x => x.Name.ToLower().Contains(filtering.Keyword)
                                             || x.Code.ToLower().Contains(filtering.Keyword)
                                             || x.NameKana.ToLower().Contains(filtering.Keyword)
                                             || x.NameKana.ToLower().Contains(filtering.keywordKana));

            }
            result.TotalRowsAfterFiltering = careers.Count();

            // sorting and paging
            careers = Filtering(careers, filtering);
            result.Careers = careers
                .Select(x => new
                {
                    x.Cd,
                    x.Name,
                    x.Code,
                    x.NameKana
                })
                .AsEnumerable()
                .Select(x => new CareerModel()
                {
                    Cd       = x.Cd,
                    Name     = x.Name,
                    Code     = x.Code,
                    NameKana = x.NameKana
                }).ToList();
            return result;
        }

        private IQueryable<MstCareer> Filtering(IQueryable<MstCareer> careers, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "code":
                    if (filtering.SortDirection == "asc")
                    {
                        careers = careers.OrderBy(x => x.Code)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        careers = careers.OrderByDescending(x => x.Code)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "name":
                    if (filtering.SortDirection == "asc")
                    {
                        careers = careers.OrderBy(x => x.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        careers = careers.OrderByDescending(x => x.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        careers = careers.OrderBy(x => x.NameKana.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        careers = careers.OrderByDescending(x => x.NameKana.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return careers;
        }

        public bool Create(CareerModel model, string userId)
        {
            var entity = new MstCareer
            {
                Name         = model.Name.Trim(),
                Code         = model.Code.Trim(),
                NameKana     = model.NameKana.Trim(),
                InsertDate   = DateTime.Now,
                IsDeleted    = false,
                InsertUserId = userId
            };
            Add(entity);
            return true;
        }

        public bool Update(CareerModel model, string userId)
        {
            var entity = _context.MstCareers.Find(model.Cd);
            entity.Name = model.Name.Trim();
            entity.Code = model.Code.Trim();
            entity.NameKana = model.NameKana.Trim();
            entity.UpdateDate = DateTime.Now;
            entity.UpdateUserId = userId;
             
            Update(entity);
            return true;
        }

        public AjaxResponseModel Remove(int cd, string userId)
        {
            try
            {
                var entity = _context.MstCareers
                        .Include(x => x.Surveys)
                        .FirstOrDefault(x => x.Cd == cd);
                if (entity.Surveys.Any(s => !s.IsDeleted))
                {
                    return new AjaxResponseModel
                    {
                        Message = "この職業が使われているため、削除できません。",
                        Status = false
                    };
                }

                entity.IsDeleted = true;
                entity.UpdateDate = DateTime.Now;
                entity.UpdateUserId = userId;

                Update(entity);
                _context.SaveChanges();

                return new AjaxResponseModel
                {
                    Message = "削除に成功しました。",
                    Status = true
                };
            }
            catch(Exception)
            {
                return new AjaxResponseModel
                {
                    Message = "削除に失敗しました。",
                    Status = false
                };
            }
        }

        /// <summary>
        /// Check if career is existed
        /// </summary>
        /// <param name="model"></param>
        /// <returns>boolean</returns>
        public bool IsDuplicated(CareerModel model)
        {
            return _context.MstCareers.Any(c => (c.Name.ToLower().Equals(model.Name.ToLower().Trim()) 
                                                && (model.Cd == 0 || c.Cd != model.Cd) && !c.IsDeleted)
                                                || (c.Code.ToLower().Equals(model.Code.ToLower().Trim()) && (model.Cd == 0 || c.Cd != model.Cd)));
        }

        public AjaxResponseModel ImportCSV(IFormFile file, string userId)
        {
            string[] headers = new string[] { "業種コード", "業種名称" };
            var result = new AjaxResponseModel(false, "");
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);

                if (fileExtension.ToUpper() != ".CSV")
                {
                    result.Message = "アプロードファイルはCSVのみ指定ください。";
                    return result;
                }

                var careers = new List<ImportedCareerModel>();
                var errorRecords = new CustomLookup();
                var isBadRecord = false;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding(932)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<ImportedCareerModelMap>();
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
                            var career = csv.GetRecord<ImportedCareerModel>();
                            var context = new ValidationContext(career, null, null);
                            var isValid = Validator.TryValidateObject(career, context, validationResults, true);

                            if (isValid)
                            {
                                careers.Add(career);
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

                if (careers.Count > 0)
                {
                    //check duplicates
                    var duplicateDic = careers.GroupBy(c => c.Code)
                                                .Where(c => c.Count() > 1)
                                                .ToDictionary(c => c.Key, c => c.Count());

                    if (duplicateDic.Count > 0)
                    {
                        var message = "業種コードは以下の値が重複しています。\n";
                        message += string.Join("\n", duplicateDic.Select(t => $"「{t.Key}」（{t.Value}回）"));
                        message += "\n再度確認してください。";

                        result.Message = message;
                        return result;
                    }

                    var now = DateTime.Now;
                    var newCareerList = new List<MstCareer>();
                    foreach (var csvCareer in careers)
                    {
                        var careerInDb = _context.MstCareers.FirstOrDefault(t => t.Code == csvCareer.Code);
                        if (careerInDb != null)
                        {
                            careerInDb.Name = csvCareer.Name;
                            careerInDb.UpdateUserId = userId;
                            careerInDb.UpdateDate = now;
                        }
                        else
                        {
                            var newCareer = new MstCareer
                            {
                                Code = csvCareer.Code,
                                Name = csvCareer.Name,
                                InsertUserId = userId,
                                InsertDate = now,
                                UpdateUserId = userId,
                                UpdateDate = now,
                            };

                            newCareerList.Add(newCareer);
                        }
                    }

                    if (newCareerList.Count > 0)
                    {
                        _context.MstCareers.AddRange(newCareerList);
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
