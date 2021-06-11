using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master.Technique;
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
    public class MstTechniqueRepository : GenericRepository<MstTechnique>, IMstTechniqueRepository<MstTechnique>
    {
        public MstTechniqueRepository(OpsContext context) : base(context)
        {

        }
        public bool Create(TechniqueModel model, string userId)
        {
            var Technique = new MstTechnique
            {
                Code         = model.Code.Trim(),
                Name         = model.Name.Trim(),
                NameKana     = model.NameKana.Trim(),
                InsertUserId = userId,
                IsDeleted    = false,
                InsertDate   = DateTime.Now
            };

            Add(Technique);
            return true;
        }

        public bool IsDuplicate(TechniqueModel model)
        {
            return _context.MstTechniques.Any(t => t.Code.ToLower().Equals(model.Code.ToLower().Trim())
                                                     && (model.Cd == 0 || t.Cd != model.Cd));
        }

        public TechniqueViewModel Load(TechniqueViewModel filtering)
        {
            filtering.Keyword     = filtering.Keyword?.Trim().ToLower();
            filtering.KeywordKana = filtering.KeywordKana?.Trim().ToLower();
            var result            = new TechniqueViewModel();
            var technique         = _context.MstTechniques.Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(filtering.Keyword))
            {
                technique = technique.Where(x => x.Name.ToLower().Contains(filtering.Keyword)
                                                        || x.NameKana.ToLower().Contains(filtering.KeywordKana)
                                                        || x.NameKana.ToLower().Contains(filtering.Keyword)
                                                        || x.Code.ToLower().Contains(filtering.Keyword));

            }

            result.TotalRowsAfterFiltering = technique.Count();
            //sort And paging
            technique = Filtering(technique, filtering);
            result.Techniques = technique
                        .Select(x => new {
                            x.Cd,
                            x.Code,
                            x.Name,
                            x.NameKana
                        })
                        .AsEnumerable()
                        .Select(x => new TechniqueModel
                        {
                            Code     = x.Code,
                            Cd       = x.Cd,
                            Name     = x.Name,
                            NameKana = x.NameKana
                        }).ToList();

            return result;
        }

        private IQueryable<MstTechnique> Filtering (IQueryable<MstTechnique> techniques, OpsFilteringDataTableModel filtering)
        {
            switch(filtering.SortColumnName)
            {
                case "code":
                    if (filtering.SortDirection == "asc")
                    {
                        techniques = techniques.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        techniques = techniques.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "name":
                    if (filtering.SortDirection == "asc")
                    {
                        techniques = techniques.OrderBy(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        techniques = techniques.OrderByDescending(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        techniques = techniques.OrderBy(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        techniques = techniques.OrderByDescending(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return techniques;
        }

        public AjaxResponseModel Remove(int cd, string userId)
        {
            try
            {
                var Technique = _context.MstTechniques
                               .Where(t => !t.Technique1Contracts.Any(c => !c.IsDeleted && c.IsCompleted )
                               && !t.Technique2Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !t.Technique3Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !t.Technique4Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !t.Technique5Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && !t.Technique6Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                               && t.Cd == cd)
                               .FirstOrDefault();
                if(Technique == null)
                {
                    return new AjaxResponseModel
                    {
                        Message = "この技法が使われている為、削除できません。",
                        Status  = false
                    };
                }

                Technique.IsDeleted    = true;
                Technique.UpdateDate   = DateTime.Now;
                Technique.InsertUserId = userId;

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

        public bool Update(TechniqueModel model, string userId)
        {
            var technique = _context.MstTechniques.Find(model.Cd);

            if (!string.IsNullOrEmpty(technique.Code)){
                technique.Code = model.Code.Trim();
            }
            
            technique.Name         = model.Name.Trim();
            technique.NameKana     = model.NameKana.Trim();
            technique.UpdateDate   = DateTime.Now;
            technique.UpdateUserId = userId;

            Update(technique);
            return true;
        }

        public AjaxResponseModel ImportCSV(IFormFile file, string userId)
        {
            string[] headers = new string[] { "技法コード", "技法名称" };
            var result = new AjaxResponseModel(false, "");
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);

                if (fileExtension.ToUpper() != ".CSV")
                {
                    result.Message = "アプロードファイルはCSVのみ指定ください。";
                    return result;
                }

                var techniques = new List<ImportedTechniqueModel>();
                var errorRecords = new CustomLookup();
                var isBadRecord = false;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding(932)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<ImportedTechniqueModelMap>();
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
                            var technique = csv.GetRecord<ImportedTechniqueModel>();
                            var context = new ValidationContext(technique, null, null);
                            var isValid = Validator.TryValidateObject(technique, context, validationResults, true);

                            if (isValid)
                            {
                                techniques.Add(technique);
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

                if (techniques.Count > 0)
                {
                    //check duplicates
                    var duplicateDic = techniques.GroupBy(t => t.Code)
                                                .Where(t => t.Count() > 1)
                                                .ToDictionary(t => t.Key, t => t.Count());

                    if (duplicateDic.Count > 0)
                    {
                        var message = "技法コードは以下の値が重複しています。\n";
                        message += string.Join("\n", duplicateDic.Select(t => $"「{t.Key}」（{t.Value}回）"));
                        message += "\n再度確認してください。";

                        result.Message = message;
                        return result;
                    }

                    var now = DateTime.Now;
                    var newTechniqueList = new List<MstTechnique>();
                    foreach (var csvTech in techniques)
                    {
                        var techInDb = _context.MstTechniques.FirstOrDefault(t => t.Code == csvTech.Code);
                        if (techInDb != null)
                        {
                            techInDb.Name = csvTech.Name;
                            techInDb.UpdateUserId = userId;
                            techInDb.UpdateDate = now;
                        }
                        else
                        {
                            var newTech = new MstTechnique
                            {
                                Code = csvTech.Code,
                                Name = csvTech.Name,
                                InsertUserId = userId,
                                InsertDate = now,
                                UpdateUserId = userId,
                                UpdateDate = now,
                            };

                            newTechniqueList.Add(newTech);
                        }
                    }

                    if (newTechniqueList.Count > 0)
                    {
                        _context.MstTechniques.AddRange(newTechniqueList);
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
