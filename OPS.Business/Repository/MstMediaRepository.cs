using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master.Media;
using OPS.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static OPS.Utility.Constants;

namespace OPS.Business.Repository
{
    public class MstMediaRepository : GenericRepository<MstMedia>, IMstMediaRepository<MstMedia>
    {
        public MstMediaRepository(OpsContext context) : base(context)
        {

        }

        public bool Create(MediaModel model, string userId)
        {
            var entity = new MstMedia
            {   
                Code         = model.Code.Trim(),
                Name         = model.Name.Trim(),
                BranchCode   = model.BranhCode?.Trim(),
                NameKana     = model.NameKana.Trim(),
                Spec         = model.Spec?.Trim(),
                Flag         = model.Flag,
                InsertDate   = DateTime.Now,
                IsDeleted    = false,
                InsertUserId = userId
            };
            Add(entity);

            return true;
        }

        public bool IsDuplicated(MediaModel model)
        {
            var query = _context.MstMedias.AsQueryable();
                //Where(x => !x.IsDeleted);
            if (model.Cd == 0)
            {
                // add
                if (string.IsNullOrEmpty(model.BranhCode))
                {
                    query = query.Where(c => c.Code.ToLower().Equals(model.Code.ToLower().Trim())
                                   && c.BranchCode.Equals(model.BranhCode));
                }
                else
                {
                    query = query.Where(c => c.Code.ToLower().Equals(model.Code.ToLower().Trim())
                                   && c.BranchCode.ToLower().Equals(model.BranhCode.ToLower().Trim()));
                }
            } 
            else
            {
                // update
                if (string.IsNullOrEmpty(model.BranhCode))
                {
                    query = query.Where(c => c.Code.ToLower().Equals(model.Code.ToLower().Trim()) 
                                    && c.BranchCode.Equals(model.BranhCode)
                                    && c.Cd != model.Cd);
                }
                else
                {
                    query = query.Where(c => (c.Code.ToLower().Equals(model.Code.ToLower()) 
                                   && c.BranchCode.ToLower().Equals(model.BranhCode.ToLower().Trim()))
                                   && (c.Cd != model.Cd));
                }
            }

            return query.Any();
        }

        public MediaViewModel Load(MediaViewModel filtering)
        {
            filtering.Keyword = filtering.Keyword?.Trim().ToLower();
            filtering.keywordKana = filtering.keywordKana?.Trim().ToLower();
            var result = new MediaViewModel();
            var medias = _context.MstMedias.Where(x => !x.IsDeleted);
            if (!string.IsNullOrEmpty(filtering.Keyword))
            {
                medias = medias.Where(x => x.Name.ToLower().Contains(filtering.Keyword)
                                        || x.Code.ToLower().Contains(filtering.Keyword)
                                        || x.NameKana.ToLower().Contains(filtering.Keyword)
                                        || x.NameKana.ToLower().Contains(filtering.keywordKana)
                                        || x.BranchCode.ToLower().Contains(filtering.Keyword));

            }

            result.TotalRowsAfterFiltering = medias.Count();
            // sorting and paging
            medias = Filtering(medias, filtering);
            result.Medias = medias
                .Select(x => new
                {
                    x.Cd,
                    x.Code,
                    x.BranchCode,
                    x.Name,
                    x.NameKana,
                    x.Flag,
                    x.Spec
                })
                .AsEnumerable()
                .Select(x => new MediaVModel()
                {
                     Cd        = x.Cd,
                     Code      = x.Code,
                     BranhCode = x.BranchCode,
                     Name      = x.Name,
                     NameKana  = x.NameKana,
                     FlagName  = ((MstMediaFlag)Convert.ToInt32(x.Flag)).GetEnumDescription(),
                     Spec      = x.Spec
                }).ToList();

            return result;
        }

        private IQueryable<MstMedia> Filtering(IQueryable<MstMedia> medias, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "name":
                    if (filtering.SortDirection == "asc")
                    {
                        medias = medias.OrderBy(x => x.Name.ToLower())
                                 .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        medias = medias.OrderByDescending(x => x.Name.ToLower())
                                 .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "code":
                    if (filtering.SortDirection == "asc")
                    {
                        medias = medias.OrderBy(x => x.Code.ToLower()).ThenBy(x => x.BranchCode.ToLower())
                                 .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        medias = medias.OrderByDescending(x => x.Code.ToLower()).ThenByDescending(x => x.BranchCode.ToLower())
                                 .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "branhCode":
                    if (filtering.SortDirection == "asc")
                    {
                        medias = medias.OrderBy(x => x.BranchCode.ToLower())
                                 .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        medias = medias.OrderByDescending(x => x.BranchCode.ToLower())
                                 .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        medias = medias.OrderBy(x => x.NameKana.ToLower())
                                    .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        medias = medias.OrderByDescending(x => x.NameKana.ToLower())
                                    .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "flag":
                    if (filtering.SortDirection == "asc")
                    {
                        medias = medias.OrderBy(x => x.Flag)
                                    .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        medias = medias.OrderByDescending(x => x.Flag)
                                    .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "spec":
                    if (filtering.SortDirection == "asc")
                    {
                        medias = medias.OrderBy(x => x.Spec)
                                    .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        medias = medias.OrderByDescending(x => x.Spec)
                                    .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }

            return medias;
        }

        public AjaxResponseModel Remove(int cd, string userId)
        {
            try
            {
                var entity = _context.MstMedias
                             .Where(m => !m.Surveys.Any()
                             && !m.Contracts.Any( c => !c.IsDeleted && c.IsCompleted)
                             && !m.PurchaseStatisticsDetails.Any()
                             && !m.PlanMedias.Any()
                             && !m.OrderReportDetails.Any()
                             && m.Cd == cd)
                             .FirstOrDefault();
                
                if (entity == null)
                {
                    return new AjaxResponseModel
                    {
                        Message = "この媒体が使われている為、削除できません。",
                        Status  = false
                    };
                }

                entity.IsDeleted    = true;
                entity.UpdateDate   = DateTime.Now;
                entity.UpdateUserId = userId;

                Update(entity);
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

        public bool Update(MediaModel model, string userId)
        {
            var entity          = _context.MstMedias.Find(model.Cd);

            entity.Code         = model.Code.Trim();
            entity.Name         = model.Name.Trim();
            entity.BranchCode   = model.BranhCode?.Trim();
            entity.NameKana     = model.NameKana.Trim();
            entity.Flag         = model.Flag;
            entity.Spec         = model.Spec?.Trim();
            entity.UpdateDate   = DateTime.Now;
            entity.UpdateUserId = userId;

            Update(entity);
            return true;
        }

        public AjaxResponseModel ImportCSV(IFormFile file, string userId)
        {
            string[] headers = new string[] { "媒体コード", "媒体名" };
            var result = new AjaxResponseModel(false, "");
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);

                if (fileExtension.ToUpper() != ".CSV")
                {
                    result.Message = "アプロードファイルはCSVのみ指定ください。";
                    return result;
                }

                var medias = new List<ImportedMediaModel>();
                var errorRecords = new CustomLookup();
                var isBadRecord = false;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding(932)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<ImportedMediaModelMap>();
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
                            var media = csv.GetRecord<ImportedMediaModel>();
                            var context = new ValidationContext(media, null, null);
                            var isValid = Validator.TryValidateObject(media, context, validationResults, true);

                            if (isValid)
                            {
                                medias.Add(media);
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
                    message += string.Join("\n", errorRecords.Select(r => $"{r.Key}行目・{r.Value}"));
                    message += "\n再度確認してください。";

                    result.Message = message;
                    return result;
                }

                if (medias.Count > 0)
                {
                    //check duplicates
                    var duplicateDic = medias.GroupBy(m => new { m.Code, m.BranchCode })
                                                .Where(m => m.Count() > 1)
                                                .ToDictionary(m => $"{m.Key.Code}{m.Key.BranchCode}", m => m.Count());

                    if (duplicateDic.Count > 0)
                    {
                        var message = "媒体コード・枝番は以下の値が重複しています。\n";
                        message += string.Join("\n", duplicateDic.Select(m => $"「{m.Key}」（{m.Value}回）"));
                        message += "\n再度確認してください。";

                        result.Message = message;
                        return result;
                    }

                    var now = DateTime.Now;
                    var newMediaList = new List<MstMedia>();
                    foreach (var csvMedia in medias)
                    {
                        var mediaInDb = _context.MstMedias.FirstOrDefault(m => m.Code == csvMedia.Code && m.BranchCode == csvMedia.BranchCode);
                        if (mediaInDb != null)
                        {
                            mediaInDb.Name = csvMedia.Name;
                            mediaInDb.Flag = csvMedia.Flag;
                            mediaInDb.UpdateUserId = userId;
                            mediaInDb.UpdateDate = now;
                        }
                        else
                        {
                            var newMedia = new MstMedia
                            {
                                Code = csvMedia.Code,
                                BranchCode = csvMedia.BranchCode,
                                Name = csvMedia.Name,
                                Flag = csvMedia.Flag,
                                InsertUserId = userId,
                                InsertDate = now,
                                UpdateUserId = userId,
                                UpdateDate = now,
                            };

                            newMediaList.Add(newMedia);
                        }
                    }

                    if (newMediaList.Count > 0)
                    {
                        _context.MstMedias.AddRange(newMediaList);
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