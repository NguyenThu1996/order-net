using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master.Artist;
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
    public class MstArtistRepository : GenericRepository<MstArtist>, IMstArtistRepository<MstArtist> 
    {
        public MstArtistRepository(OpsContext context) :base(context)
        {

        }

        public AjaxResponseModel Create( ArtistModel model, string userId)
        {
            var resultModel = new AjaxResponseModel();

            try
            {
                var entity = new MstArtist
                {
                    Code = model.Code.Trim(),
                    Name = model.Name.Trim(),
                    NameKana = model.NameKana.Trim(),
                    ItemName = model.ItemName.Trim(),
                    CategoryName = model.NameCategory.Trim(),
                    IsFavorited = model.IsFavorited,
                    InsertDate = DateTime.Now,
                    IsDeleted = false,
                    InsertUserId = userId,
                };

                // add department for artist
                List<ArtistDepartment> listArtistDepartments = AddNewArtistDepartments(model, 0);

                entity.ArtistDepartments = listArtistDepartments;

                _context.MstArtists.Add(entity);
                _context.SaveChanges();

                resultModel.Status = true;
                resultModel.Message = "新規作成に成功しました。";
                return resultModel;
            }
            catch (Exception)
            {
                resultModel.Status = false;
                resultModel.Message = "作家の新規作成に失敗しました。";
                return resultModel;
            }
        }

        public AjaxResponseModel Update(ArtistModel model, string userId)
        {
            var resultModel = new AjaxResponseModel();

            try
            {
                var artist = _context.MstArtists.Include(x => x.ArtistDepartments)
                                                .FirstOrDefault(x => x.Cd == model.Cd);

                if (artist != null)
                {
                    artist.Code = model.Code.Trim();
                    artist.Name = model.Name.Trim();
                    artist.ItemName = model.ItemName.Trim();
                    artist.CategoryName = model.NameCategory.Trim();
                    artist.NameKana = model.NameKana.Trim();
                    artist.IsFavorited = model.IsFavorited;
                    artist.UpdateDate = DateTime.Now;
                    artist.UpdateUserId = userId;

                    Update(artist);

                    if (artist.ArtistDepartments != null)
                    {
                        // remove department of artist
                        _context.ArtistDepartments.RemoveRange(artist.ArtistDepartments);

                        // add department agian for artist
                        List<ArtistDepartment> result = AddNewArtistDepartments(model, model.Cd);

                        _context.ArtistDepartments.AddRange(result);
                    }

                    _context.SaveChanges();

                    resultModel.Status = true;
                    resultModel.Message = "編集に成功しました。";
                    return resultModel;
                }

                resultModel.Status = false;
                resultModel.Message = "作家が見つかりません。";
                return resultModel;
            }
            catch (Exception)
            {
                resultModel.Status = false;
                resultModel.Message = "作家の編集に失敗しました。";
                return resultModel;
            }
        }

        private static List<ArtistDepartment> AddNewArtistDepartments(ArtistModel model, int artistCd)
        {
            return model.ListArtistDepartments
                              .Where(p => !p.IsDelete && p.DepartmentCd > 0).GroupBy(p => p.DepartmentCd)
                .Select(x => new ArtistDepartment
                {
                    ArtistCd = artistCd,
                    DepartmentCd = x.Key,
                })
                .ToList();
        }

        public AjaxResponseModel Remove(int cd, string userId)
        {  
            try
            {
                var artist = _context.MstArtists
                         .Where(a => !a.Artist1Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                         && !a.Artist2Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                         && !a.Artist3Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                         && !a.Artist4Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                         && !a.Artist5Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                         && !a.Artist6Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                         && !a.BusinessHopes.Any()
                         && !a.Products.Any(p => !p.IsDeleted)
                         && !a.PurchaseStatisticsDetails.Any()
                         && !a.Rankings.Any()
                         && a.Cd == cd)
                         .FirstOrDefault();

                if (artist == null)
                {
                    return new AjaxResponseModel
                    {
                        Message = "この作家が使われている為、削除できません。",
                        Status = false,
                    };
                }

                artist.IsDeleted    = true;
                artist.UpdateDate   = DateTime.Now;
                artist.UpdateUserId = userId;

                Update(artist);

                var artistDepartments = _context.ArtistDepartments.Where(ad => ad.ArtistCd == cd);
                _context.ArtistDepartments.RemoveRange(artistDepartments);

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

        public ArtistViewModel  Load(ArtistViewModel model)
        {
            model.Keyword = model.Keyword?.Trim().ToLower();
            model.keywordKana = model.keywordKana?.Trim().ToLower();
            var result = new ArtistViewModel();
            var artists       = _context.MstArtists.Where(x => !x.IsDeleted);

            if(!string.IsNullOrEmpty(model.Keyword))
            {
                artists = artists.Where(x => x.Name.ToLower().Contains(model.Keyword) 
                                             || x.NameKana.ToLower().Contains(model.keywordKana)
                                             || x.NameKana.ToLower().Contains(model.Keyword)
                                             || x.CategoryName.ToLower().Contains(model.Keyword)
                                             || x.ItemName.ToLower().Contains(model.Keyword)
                                             || x.Code.ToLower().Contains(model.Keyword));
                                             
            }

            result.TotalRowsAfterFiltering = artists.Count();
            //Sort And Paging
            artists = Filtering(artists, model);
            result.Artists = artists
                .Select(x => new {
                    x.Cd,
                    x.Code,
                    x.Name,
                    x.NameKana,
                    x.ItemName,
                    x.CategoryName,
                    DepartmentName = x.ArtistDepartments.Select(a => new {
                        a.Department.Name,
                        a.Department.Code
                    }),
                    x.IsFavorited,
                })
                .AsEnumerable()
                .Select(x => new ArtistVModel()
                {
                    Cd           = x.Cd,
                    Code         = x.Code,
                    Name         = x.Name,
                    NameKana     = x.NameKana,
                    ItemName     = x.ItemName,
                    NameCategory = x.CategoryName,
                    DepartmentName = x.DepartmentName.OrderBy(x => x.Code).Select(y => y.Name).ToList(),
                    IsFavorited = x.IsFavorited,
                }).ToList();

            return result;
        }

        private IQueryable<MstArtist> Filtering(IQueryable<MstArtist> artists, OpsFilteringDataTableModel filtering)
        {
            switch(filtering.SortColumnName)
            {
                case "code":
                    if (filtering.SortDirection == "asc")
                    {
                        artists = artists.OrderBy(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        artists = artists.OrderByDescending(x => x.Code).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "name":
                    if(filtering.SortDirection == "asc")
                    {
                        artists = artists.OrderBy(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        artists = artists.OrderByDescending(x => x.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "itemName":
                    if (filtering.SortDirection == "asc")
                    {
                        artists = artists.OrderBy(x => x.ItemName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        artists = artists.OrderByDescending(x => x.ItemName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameCategory":
                    if (filtering.SortDirection == "asc")
                    {
                        artists = artists.OrderBy(x => x.CategoryName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        artists = artists.OrderByDescending(x => x.CategoryName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "nameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        artists = artists.OrderBy(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        artists = artists.OrderByDescending(x => x.NameKana).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "isFavorited":
                    if (filtering.SortDirection == "asc")
                    {
                        artists = artists.OrderBy(x => x.IsFavorited).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        artists = artists.OrderByDescending(x => x.IsFavorited).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
            }
            return artists;
        }

        public List<SelectListItem> GetSelectListArtist()
        {
            var artists = _context.MstArtists.Where(x => !x.IsDeleted)
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
               
            return artists;
        }

        public bool IsDuplicated(ArtistModel model)
        {
            return _context.MstArtists.Any(c => c.Code.ToLower().Equals(model.Code.ToLower().Trim()) && (model.Cd == 0 || c.Cd != model.Cd));
            // !isDelele

        }

        public ArtistModel GetArtistDetails(int cd)
        {
            var entity = _context.MstArtists.Where(x => x.Cd == cd).Select(x => new {
                x.Cd,
                x.Name,
                x.NameKana,
                x.Code,
                x.ItemName,
                x.CategoryName,
                lstDepartments = x.ArtistDepartments.OrderBy(a => a.Department.Code).Select(a => new
                {
                    a.ArtistCd,
                    a.DepartmentCd,
                    IsExist = (a.Department.Department1Contracts.Any(s => s.ArtistCd1 == cd && s.IsCompleted && !s.IsDeleted)
                               || a.Department.Department2Contracts.Any(s => s.ArtistCd2 == cd && s.IsCompleted && !s.IsDeleted)
                               || a.Department.Department3Contracts.Any(s => s.ArtistCd3 == cd && s.IsCompleted && !s.IsDeleted)
                               || a.Department.Department4Contracts.Any(s => s.ArtistCd4 == cd && s.IsCompleted && !s.IsDeleted)
                               || a.Department.Department5Contracts.Any(s => s.ArtistCd5 == cd && s.IsCompleted && !s.IsDeleted)
                               || a.Department.Department6Contracts.Any(s => s.ArtistCd6 == cd && s.IsCompleted && !s.IsDeleted))
                }),
                x.IsFavorited

            })
            .AsEnumerable()
            .Select(y => new ArtistModel() {
                Cd = y.Cd,
                Name = y.Name,
                NameKana = y.NameKana,
                ItemName = y.ItemName,
                NameCategory = y.CategoryName,
                Code = y.Code,
                ListArtistDepartments = y.lstDepartments.Any() ? y.lstDepartments.Select(z => new ArtistDepartmentVModel
                {
                    ArtistCd = z.ArtistCd,
                    DepartmentCd = z.DepartmentCd,
                    IsExist = z.IsExist,
                }).ToList() : new List<ArtistDepartmentVModel> { new ArtistDepartmentVModel() },
                IsFavorited = y.IsFavorited,

            }).FirstOrDefault();

            return entity;
        }

        public AjaxResponseModel ImportCSV(IFormFile file, string userId)
        {
            string[] headers = new string[] { "作家コード", "作家名", "作家名（略称）", "事業区分" };
            var result = new AjaxResponseModel(false, "");
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);

                if (fileExtension.ToUpper() != ".CSV")
                {
                    result.Message = "アプロードファイルはCSVのみ指定ください。";
                    return result;
                }

                var artists = new List<ImportedArtistModel>();
                var errorRecords = new CustomLookup();
                var isBadRecord = false;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding(932)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<ImportedArtistModelMap>();
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
                            var artist = csv.GetRecord<ImportedArtistModel>();
                            var context = new ValidationContext(artist, null, null);
                            var isValid = Validator.TryValidateObject(artist, context, validationResults, true);

                            if (isValid)
                            {
                                artists.Add(artist);
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

                if(errorRecords.Count > 0)
                {
                    var message = "データが無効です。\n";
                    message += string.Join("\n", errorRecords.Select(a => $"{a.Key}行目・{a.Value}"));
                    message += "\n再度確認してください。";

                    result.Message = message;
                    return result;
                }

                if (artists.Count > 0)
                {
                    //check duplicates
                    var duplicateDic = artists.GroupBy(a => a.Code)
                                                .Where(a => a.Count() > 1)
                                                .ToDictionary(a => a.Key, a => a.Count());

                    if (duplicateDic.Count > 0)
                    {
                        var message = "作家コードは以下の値が重複しています。\n";
                        message += string.Join("\n", duplicateDic.Select(a => $"「{a.Key}」（{a.Value}回）"));
                        message += "\n再度確認してください。";

                        result.Message = message;
                        return result;
                    }

                    var now = DateTime.Now;
                    var newArtistList = new List<MstArtist>();
                    foreach (var csvArtist in artists)
                    {
                        var artistInDb = _context.MstArtists.Include(a => a.ArtistDepartments).FirstOrDefault(a => a.Code == csvArtist.Code);
                        if (artistInDb != null)
                        {
                            artistInDb.Name = csvArtist.Name;
                            artistInDb.ItemName = csvArtist.ItemName;
                            artistInDb.CategoryName = csvArtist.CategoryName;
                            artistInDb.UpdateUserId = userId;
                            artistInDb.UpdateDate = now;

                            if (csvArtist.DepartmentCd.HasValue)
                            {
                                if (!artistInDb.ArtistDepartments.Any(ad => ad.DepartmentCd == csvArtist.DepartmentCd))
                                {
                                    artistInDb.ArtistDepartments.Add(new ArtistDepartment
                                    {
                                        ArtistCd = artistInDb.Cd,
                                        DepartmentCd = csvArtist.DepartmentCd.Value,
                                    });
                                }
                            }
                        }
                        else
                        {
                            var newArtist = new MstArtist
                            {
                                Code = csvArtist.Code,
                                Name = csvArtist.Name,
                                ItemName = csvArtist.ItemName,
                                CategoryName = csvArtist.CategoryName,
                                InsertUserId = userId,
                                InsertDate = now,
                                UpdateUserId = userId,
                                UpdateDate = now,
                            };

                            if (csvArtist.DepartmentCd.HasValue)
                            {
                                newArtist.ArtistDepartments = new List<ArtistDepartment>
                                {
                                    new ArtistDepartment
                                    {
                                        DepartmentCd = csvArtist.DepartmentCd.Value,
                                    }
                                };
                            }

                            newArtistList.Add(newArtist);
                        }
                    }

                    if (newArtistList.Count > 0)
                    {
                        _context.MstArtists.AddRange(newArtistList);
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
