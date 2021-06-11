using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.Master.Event;
using OPS.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS.Business.Repository
{
    public class MstEventRepository : GenericRepository<MstEvent>, IMstEventRepository<MstEvent>
    {
        private IMasterDataRepository _mstDataRepo;

        private readonly UserManager<ApplicationUser> _userManager;

        private RoleManager<ApplicationRole> _roleManager { get; set; }

        public MstEventRepository(OpsContext context, IMasterDataRepository mstDataRepo, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) : base (context)
        {
            _mstDataRepo = mstDataRepo;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IndexViewModel Load(SearchEventModel filter)
        {
            var result = new IndexViewModel();

            var mstEvents = _context.MstEvents.Where(e => !e.IsDeleted);

            filter.Keyword = filter.Keyword?.Trim().ToLower();
            filter.KeywordKana = filter.KeywordKana?.Trim().ToLower();
            
            if (!string.IsNullOrEmpty(filter.EventCode?.Trim().ToLower()))
            {
                mstEvents = mstEvents.Where(x => x.Code.ToLower().Contains(filter.EventCode.Trim().ToLower()));
            }
            if (!string.IsNullOrEmpty(filter.Keyword) || !string.IsNullOrEmpty(filter.KeywordKana))
            {
                mstEvents = mstEvents.Where(x => x.Name.ToLower().Contains(filter.Keyword)
                    || x.NameAbbr.ToLower().Contains(filter.Keyword)
                    || x.NameKana.ToLower().Contains(filter.Keyword) 
                    || x.NameKana.ToLower().Contains(filter.KeywordKana));
            }

            result.TotalRowsAfterFiltering = mstEvents.Count();

            mstEvents = Filtering(mstEvents, filter);

            result.ListEvent = mstEvents
            .Select(e => new
            {
                e.Cd,
                e.Name,
                e.Code,
                e.NameKana,
                e.StartDate,
                e.EndDate,
                e.StartTime,
                e.EndTime,
                SalemanName = e.MainSalesman.Name,
                e.ApplicationUser.Id,
                e.InsertDate
            })
            .AsEnumerable()
            .Select(y => new ListEventModel()
            {
                Cd = y.Cd,
                EventNameKana = y.NameKana,
                Name = y.Name,
                EventCode = y.Code,
                StartDate = y.StartDate?.ToString(Constants.ExactDateFormat),
                EndDate = y.EndDate?.ToString(Constants.ExactDateFormat),
                EventTime = ((y.StartTime == null && y.EndTime == null) ? "" 
                            : (string.Format("{0:hh\\:mm}", y.StartTime) + " ~ " + string.Format("{0:hh\\:mm}", y.EndTime))),
                SalemanName = y.SalemanName,
                ApplicationUserId = y.Id,
            }).ToList();

            return result;
        }

        private IQueryable<MstEvent> Filtering(IQueryable<MstEvent> events, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "EventCode":
                    if (filtering.SortDirection == "asc")
                    {
                        events = events.OrderBy(x => x.Code)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        events = events.OrderByDescending(x => x.Code)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "Name":
                    if (filtering.SortDirection == "asc")
                    {
                        events = events.OrderBy(x => x.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        events = events.OrderByDescending(x => x.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "EventNameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        events = events.OrderBy(x => x.NameKana.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        events = events.OrderByDescending(x => x.NameKana.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "StartDate":
                    if (filtering.SortDirection == "asc")
                    {
                        events = events.OrderBy(x => x.StartDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        events = events.OrderByDescending(x => x.StartDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "EndDate":
                    if (filtering.SortDirection == "asc")
                    {
                        events = events.OrderBy(x => x.EndDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        events = events.OrderByDescending(x => x.EndDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "SalemanName":
                    if (filtering.SortDirection == "asc")
                    {
                        events = events.OrderBy(x => x.MainSalesman.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        events = events.OrderByDescending(x => x.MainSalesman.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                        events = events.OrderByDescending(x => x.StartDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    break;
            }
            return events;
        }

        public async Task<ErrorModel> Create(AddOrUpdateEventModel model, string userId)
        {
            var errorModel = new ErrorModel();
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using (var dbTran = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (!CheckDuplicate(model.EventCode))
                        {
                            errorModel.Message = "この催事コードが既に存在しています。もう一度お試しください。";
                            errorModel.Status = false;
                            return;
                        }
                        var userEvent = new ApplicationUser
                        {
                            UserName = model.EventCode,
                            FirstName = "催事",
                            LastName = model.Name,
                            EmailConfirmed = false,
                            PhoneNumberConfirmed = false,
                            TwoFactorEnabled = false
                        };
                        var createAccountEvent = await _userManager.CreateAsync(userEvent, model.Password);

                        //アンケート
                        var userSurvey = new ApplicationUser
                        {
                            UserName = $"{model.EventCode}{Constants.SurveyUserName}",
                            FirstName = "アンケート",
                            LastName = model.Name,
                            EmailConfirmed = false,
                            PhoneNumberConfirmed = false,
                            TwoFactorEnabled = false
                        };
                        var createAccountSurvey = await _userManager.CreateAsync(userSurvey, model.PasswordSurvey);

                        if (createAccountEvent.Succeeded && createAccountSurvey.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(userEvent, Role.User.ToString());

                            await _userManager.AddToRoleAsync(userSurvey, Role.Survey.ToString());

                            var eventEntity = new MstEvent
                            {
                                Code = model.EventCode,
                                ApplicationUserId = userEvent.Id,
                                Name = model.Name,
                                NameAbbr = model.EventNameAbbr,
                                NameKana = model.EventNameKana,
                                Place = model.Place,
                                Address = model.Address,
                                StartDate = model.StartDate.Value.Date,
                                EndDate = model.EndDate.Value.Date,
                                StartTime = model.StartTime,
                                EndTime = model.EndTime,
                                Remark = model.Remark,
                                Decorate = model.Decorate,
                                SalesmanCd = model.SalesmanCd,
                                InsertUserId = userId,
                                InsertDate = DateTime.Now,
                                UpdateDate = DateTime.Now,
                                UpdateUserId = userId
                            };

                            Add(eventEntity);
                            _context.SaveChanges();

                            // add Saleman for event
                            List<EventSalesAssigment> listEventsalemans = AddNewEventSalesmanEssigment(model, eventEntity.Cd);

                            _context.EventSalesAssigments.AddRange(listEventsalemans);

                            // add Media for event
                            List<EventMedia> listEventMedia = AddNewEventMedias(model, eventEntity.Cd);

                            _context.EventMedias.AddRange(listEventMedia);

                            _context.SaveChanges();
                            dbTran.Commit();

                            errorModel.Status = true;
                            errorModel.Message = "催事の新規作成に成功しました。";
                            return;
                        }

                        errorModel.Message = "催事の新規作成に失敗しました。もう一度お試しください。";
                        errorModel.Status = false;
                    }
                    catch (Exception)
                    {
                        dbTran.Rollback();
                        errorModel.Status = false;
                        errorModel.Message = "催事の新規作成に失敗しました。もう一度お試しください。";
                    }
                }
            });

            return errorModel;
        }

        public AddOrUpdateEventModel GetEventForUpdate(int eventCd)
        {
            var eventUpdate = _context.MstEvents.Where(x => x.Cd == eventCd && !x.IsDeleted).Select(x => new
            {
                x.Cd,
                EventCode = x.ApplicationUser.UserName,
                x.Code,
                x.Name,
                x.NameAbbr,
                x.NameKana,
                x.StartDate,
                x.EndDate,
                x.StartTime,
                x.EndTime,
                x.Address,
                x.Place,
                x.SalesmanCd,
                x.Remark,
                x.Decorate,
                lstSaleman = x.EventSalesAssigments.OrderBy(e => e.Salesman.Code).Select(e => new
                {
                    e.SalesmanCd,
                    e.NewTargetAmount,
                    e.NewTargetRevenue,
                    e.EffortTargetAmount,
                    e.EffortTargetRevenue,
                    IsExist = (e.Salesman.ARespContracts.Any(s => s.EventCd == eventCd && !s.IsDeleted && s.IsCompleted) 
                                                             || e.Salesman.CRespContracts.Any(s => s.EventCd == eventCd && !s.IsDeleted && s.IsCompleted) 
                                                             || e.Salesman.SRespContracts.Any(s => s.EventCd == eventCd && !s.IsDeleted && s.IsCompleted))
                }),
                lstMedia = x.EventMedias.OrderBy(m => m.Media.Code).Select(m => new
                {
                    m.MediaCd,
                    IsMediaExist = (m.Media.Surveys.Any(s => s.EventCd == eventCd)
                                    || m.Media.Contracts.Any(c => c.EventCd == eventCd && !c.IsDeleted && c.IsCompleted)
                                    || m.Media.PlanMedias.Any(p => p.EventCd == eventCd))
                }),
                IsExistContract = x.Contracts.Any(x => !x.IsDeleted && x.IsCompleted),
            })
            .AsEnumerable()
            .Select(s => new AddOrUpdateEventModel()
            {
                Password = Constants.FakePass,
                PasswordSurvey = Constants.FakePass,
                Cd = s.Cd,
                EventCode = s.EventCode,
                EventCodeOld = s.EventCode,
                Name = s.Name,
                EventNameAbbr = s.NameAbbr,
                EventNameKana = s.NameKana,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Place = s.Place,
                Address = s.Address,
                Decorate = s.Decorate,
                Remark = s.Remark,
                SalesmanCd = s.SalesmanCd,
                ListSaleManEvent = s.lstSaleman.Any() ? s.lstSaleman.Select(y => new EventSaleAssignmentVModel
                {
                    NewTargetAmount = y.NewTargetAmount,
                    NewTargetRevenue = y.NewTargetRevenue,
                    EffortTargetAmount = y.EffortTargetAmount,
                    EffortTargetRevenue = y.EffortTargetRevenue,
                    SalemanCd = y.SalesmanCd,
                    IsExist = y.IsExist,
                }).ToList() : new List<EventSaleAssignmentVModel> { new EventSaleAssignmentVModel() },

                ListEventMedia = s.lstMedia.Any() ? s.lstMedia.Select(m => new EventMediaVModel
                {
                    MediaCd = m.MediaCd,
                    IsExist = m.IsMediaExist,
                }).ToList() : new List<EventMediaVModel> { new EventMediaVModel() },
               
                IsExistContract = s.IsExistContract

            }).FirstOrDefault();

            if(eventUpdate != null)
            {
                eventUpdate.ListSalesman = _mstDataRepo.GetSelectsListSalesman();
                eventUpdate.ListMedia = _mstDataRepo.GetSelectListEventMedias();
            }

            return eventUpdate;
        }

        public async Task<ErrorModel> UpdateEvent(AddOrUpdateEventModel model, string userLoginId)
        {
            var errorModel = new ErrorModel();

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using (var dbTran = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (!CheckDuplicate(model.EventCode, model.Cd))
                        {
                            errorModel.Message = "この催事コードが既に存在しています。もう一度お試しください。";
                            errorModel.Status = false;
                            return;
                        }
                        // User sale
                        var user = _context.Users.Where(u => u.Event.Cd == model.Cd).FirstOrDefault();

                        user.UserName = model.EventCode;
                        user.NormalizedUserName = model.EventCode;
                        user.LastName = model.Name;

                        if (!model.Password.Equals(Constants.FakePass))
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            await _userManager.ResetPasswordAsync(user, token, model.Password);
                        }

                        // User survey
                        var userSurvey = _context.Users.Where(u => u.UserName == $"{model.EventCodeOld}{Constants.SurveyUserName}").FirstOrDefault();
                        if(userSurvey != null)
                        {
                            userSurvey.UserName = $"{model.EventCode}{Constants.SurveyUserName}";
                            userSurvey.NormalizedUserName = $"{model.EventCode}{Constants.SurveyUserName}";
                            userSurvey.LastName = model.Name;

                            if (!model.PasswordSurvey.Equals(Constants.FakePass))
                            {
                                var token = await _userManager.GeneratePasswordResetTokenAsync(userSurvey);

                                await _userManager.ResetPasswordAsync(userSurvey, token, model.PasswordSurvey);
                            }
                        }
                        else
                        {
                            //アンケート
                            userSurvey = new ApplicationUser
                            {
                                UserName = $"{model.EventCode}{Constants.SurveyUserName}",
                                FirstName = "アンケート",
                                LastName = model.Name,
                                EmailConfirmed = false,
                                PhoneNumberConfirmed = false,
                                TwoFactorEnabled = false
                            };

                            var createAccountSurvey = await _userManager.CreateAsync(userSurvey, model.PasswordSurvey);

                            await _userManager.AddToRoleAsync(userSurvey, Role.Survey.ToString());
                        }

                        var eventEntity = _context.MstEvents.Include(x => x.EventSalesAssigments)
                                                            .Include(x => x.EventMedias)
                                                            .FirstOrDefault(e => e.Cd == model.Cd);

                        if (eventEntity != null)
                        {
                            if (eventEntity.Code != model.EventCode)
                            {
                                var contract = _context.Contracts.Where(c => c.EventCd == model.Cd && !c.IsDeleted && c.IsCompleted).ToList();

                                foreach (var contractItem in contract)
                                {
                                    var lastCharOfId = contractItem.Id.Substring(contractItem.Id.Length - 3);

                                    contractItem.Id = model.EventCode + lastCharOfId;
                                    contractItem.UpdateDate = DateTime.Now;
                                    contractItem.UpdateUserId = userLoginId;

                                    _context.Contracts.Update(contractItem);
                                }
                            }
                            eventEntity.Code = model.EventCode;
                            eventEntity.Name = model.Name;
                            eventEntity.NameAbbr = model.EventNameAbbr;
                            eventEntity.NameKana = model.EventNameKana;
                            eventEntity.StartDate = model.StartDate;
                            eventEntity.EndDate = model.EndDate;
                            eventEntity.StartTime = model.StartTime;
                            eventEntity.EndTime = model.EndTime;
                            eventEntity.Place = model.Place;
                            eventEntity.Address = model.Address;
                            eventEntity.SalesmanCd = model.SalesmanCd;
                            eventEntity.Remark = model.Remark;
                            eventEntity.Decorate = model.Decorate;
                            eventEntity.UpdateDate = DateTime.Now;
                            eventEntity.UpdateUserId = userLoginId;

                            Update(eventEntity);

                            // update EventSaleman
                            if (eventEntity.EventSalesAssigments != null)
                            {
                                // remove saleman of event
                                _context.EventSalesAssigments.RemoveRange(eventEntity.EventSalesAssigments);

                                // add saleman for event
                                List<EventSalesAssigment> listEventSalemans = AddNewEventSalesmanEssigment(model, eventEntity.Cd);

                                _context.EventSalesAssigments.AddRange(listEventSalemans);
                            }

                            //update media
                            if (eventEntity.EventMedias != null)
                            {
                                //remove media of event
                                _context.EventMedias.RemoveRange(eventEntity.EventMedias);

                                // add media for event
                                List<EventMedia> listEventMedias = AddNewEventMedias(model, model.Cd);

                                _context.EventMedias.AddRange(listEventMedias);
                            }

                            _context.SaveChanges();
                            dbTran.Commit();

                            errorModel.Message = "催事の編集に成功しました。";
                            errorModel.Status = true;
                            return;
                        }

                        errorModel.Message = "催事の編集に失敗しました。もう一度お試しください。";
                        errorModel.Status = false;
                    }
                    catch (Exception)
                    {
                        dbTran.Rollback();
                        errorModel.Message = "催事の編集に失敗しました。もう一度お試しください。";
                        errorModel.Status = false;
                    }
                }
            });

            return errorModel;
        }

        private static List<EventMedia> AddNewEventMedias(AddOrUpdateEventModel model, int eventCd)
        {
            return model.ListEventMedia
                              .Where(p => !p.IsDelete && p.MediaCd > 0).GroupBy(p => p.MediaCd)
                .Select(x => new EventMedia
                {
                    EventCd = eventCd,
                    MediaCd = x.Key,
                })
                .ToList();
        }

        private static List<EventSalesAssigment> AddNewEventSalesmanEssigment(AddOrUpdateEventModel model, int eventCd)
        {
            return model.ListSaleManEvent
                              .Where(p => !p.IsDelete && p.SalemanCd > 0).GroupBy(p => p.SalemanCd)
                              .Select(g => g.FirstOrDefault())
                .Select(x => new EventSalesAssigment
                {
                    EventCd = eventCd,
                    SalesmanCd = x.SalemanCd,
                    NewTargetAmount = x.NewTargetAmount,
                    NewTargetRevenue = x.NewTargetRevenue,
                    EffortTargetAmount = x.EffortTargetAmount,
                    EffortTargetRevenue = x.EffortTargetRevenue
                })
                .ToList();
        }

        public bool CheckDuplicate(string eventCode, int? eventCd = null)
        {
            if(eventCd == null)
            {
               return _context.MstEvents.FirstOrDefault(x => x.Code.Equals(eventCode) || x.ApplicationUser.UserName == eventCode) == null;
            }
            
             return _context.MstEvents.FirstOrDefault(x => (x.Code.Equals(eventCode) || x.ApplicationUser.UserName == eventCode) 
                             && x.Cd != eventCd) == null;
        }

        public bool DeleteEvent(int eventCd, string userId)
        {
            var entity = _context.MstEvents
                        .Where(x => x.Cd == eventCd
                                    && !x.OrderReports.Any()
                                    && !x.Contracts.Any(c => !c.IsDeleted && c.IsCompleted)
                                    && !x.ContractsFromOtherEvent.Any(c => !c.IsDeleted && c.IsCompleted))
                        .FirstOrDefault();

            if(entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.UpdateDate = DateTime.Now;
            entity.UpdateUserId = userId;

            Update(entity);
            return true;
        }

        public AjaxResponseModel LoadEventInfoByCode(string eventCode)
        {
            eventCode = eventCode.Replace(Constants.SurveyUserName, "").Replace(Constants.SurveyUserName.ToLower(), "");
            var entity = _context.MstEvents.Where(q => q.ApplicationUser.UserName == eventCode).FirstOrDefault();
            if (entity == null)
                return new AjaxResponseModel(false, null);
            EventCookieModel eventVM = new EventCookieModel
            {
                Cd = entity.Cd,
                EventCode = entity.Code,
                StartDate = entity.StartDate.ToString(),
                EndDate = entity.EndDate.ToString(),
                EventTime =  string.Format("初日 {0}({1})|最終日 {2}({3})", entity.StartDate?.ToString(Constants.ExactDateFormat)
                               , Commons.GetDayOfWeekJP(entity.StartDate)
                               , entity.EndDate?.ToString(Constants.ExactDateFormat)
                               , Commons.GetDayOfWeekJP(entity.EndDate)),
                ApplicationUserId = entity.ApplicationUserId,
                Name = entity.Name
            };
            string result = Newtonsoft.Json.JsonConvert.SerializeObject(eventVM);
            return new AjaxResponseModel(true, result.ToString());
        }

        public List<SelectListItem> GetLstEventLogin()
        {
            var dateNow = DateTime.Now.Date;
            var result = _context.MstEvents.Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.Cd,
                    x.Code,
                    x.Name,
                    x.StartDate,
                    x.EndDate,
                    SortDate = (x.StartDate.HasValue && x.StartDate.Value.Date == dateNow) ? 1 : ((x.StartDate.HasValue && x.StartDate.Value.Date < dateNow && x.EndDate.HasValue && x.EndDate.Value.Date > dateNow) ? 2 : 3)
                })
                .OrderBy(x => x.SortDate)
                .ThenByDescending(x => x.StartDate)
                .AsEnumerable()
                .Select(x => new SelectListItem()
                {
                    Value = x.Code,
                    Text = $"{x.Name} ー {x.StartDate?.ToString(Constants.ExactDateFormatJP)}～{x.EndDate?.ToString(Constants.ExactDateFormatJP)}",
                }).ToList();

            return result;
        }

        public async Task<AjaxResponseModel> ImportCSVAsync(IFormFile file, string userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                using (var dbTran = _context.Database.BeginTransaction())
                {
                    string[] headers = new string[] { "催事コード", "催事名称", "催事略称" };
                    var opResult = new AjaxResponseModel(false, "");
                    try
                    {
                        string fileExtension = Path.GetExtension(file.FileName);

                        if (fileExtension.ToUpper() != ".CSV")
                        {
                            opResult.Message = "アプロードファイルはCSVのみ指定ください。";
                            return opResult;
                        }

                        var events = new List<ImportedEventModel>();
                        var errorRecords = new CustomLookup();
                        var isBadRecord = false;

                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        using (var reader = new StreamReader(file.OpenReadStream(), Encoding.GetEncoding(932)))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            csv.Configuration.RegisterClassMap<ImportedEventModelMap>();
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
                                opResult.Message = message;
                                return opResult;
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
                                    var importedEvent = csv.GetRecord<ImportedEventModel>();
                                    var context = new ValidationContext(importedEvent, null, null);
                                    var isValid = Validator.TryValidateObject(importedEvent, context, validationResults, true);

                                    if (isValid)
                                    {
                                        events.Add(importedEvent);
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

                            opResult.Message = message;
                            return opResult;
                        }

                        if (events.Count > 0)
                        {
                            //check duplicates
                            var duplicateDic = events.GroupBy(e => e.Code)
                                                        .Where(e => e.Count() > 1)
                                                        .ToDictionary(e => e.Key, e => e.Count());

                            if (duplicateDic.Count > 0)
                            {
                                var message = "催事コードは以下の値が重複しています。\n";
                                message += string.Join("\n", duplicateDic.Select(e => $"「{e.Key}」（{e.Value}回）"));
                                message += "\n再度確認してください。";

                                opResult.Message = message;
                                return opResult;
                            }

                            var now = DateTime.Now;
                            var newEventList = new List<MstEvent>();
                            foreach (var csvEvent in events)
                            {
                                var eventInDb = _context.MstEvents.FirstOrDefault(e => e.Code == csvEvent.Code);
                                if (eventInDb != null)
                                {
                                    eventInDb.Name = csvEvent.Name;
                                    eventInDb.NameAbbr = csvEvent.NameAbbr;
                                    eventInDb.UpdateUserId = userId;
                                    eventInDb.UpdateDate = now;
                                }
                                else
                                {
                                    var eventAccount = new ApplicationUser
                                    {
                                        UserName = csvEvent.Code,
                                        FirstName = "催事",
                                        LastName = csvEvent.Name,
                                        EmailConfirmed = false,
                                        PhoneNumberConfirmed = false,
                                        TwoFactorEnabled = false
                                    };
                                    var createEventAccResult = await _userManager.CreateAsync(eventAccount, Constants.ImportedEventPwd);

                                    var surveyAccount = new ApplicationUser
                                    {
                                        UserName = $"{csvEvent.Code}{Constants.SurveyUserName}",
                                        FirstName = "アンケート",
                                        LastName = csvEvent.Name,
                                        EmailConfirmed = false,
                                        PhoneNumberConfirmed = false,
                                        TwoFactorEnabled = false
                                    };
                                    var createSurveyAccResult = await _userManager.CreateAsync(surveyAccount, Constants.ImportedEventPwd);

                                    if (createEventAccResult.Succeeded && createSurveyAccResult.Succeeded)
                                    {
                                        var addToRoleEventResult = await _userManager.AddToRoleAsync(eventAccount, Role.User.ToString());
                                        var addToRoleSurveyResult = await _userManager.AddToRoleAsync(surveyAccount, Role.Survey.ToString());

                                        if (addToRoleEventResult.Succeeded && addToRoleSurveyResult.Succeeded)
                                        {
                                            var newEvent = new MstEvent
                                            {
                                                Code = csvEvent.Code,
                                                ApplicationUserId = eventAccount.Id,
                                                Name = csvEvent.Name,
                                                NameAbbr = csvEvent.NameAbbr,
                                                InsertUserId = userId,
                                                InsertDate = now,
                                                UpdateUserId = userId,
                                                UpdateDate = now,
                                            };

                                            newEventList.Add(newEvent);
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                            }

                            if (newEventList.Count > 0)
                            {
                                _context.MstEvents.AddRange(newEventList);
                            }

                            _context.SaveChanges();
                            dbTran.Commit();

                            opResult.Status = true;
                            return opResult;
                        }
                        else
                        {
                            opResult.Message = "CSVファイルにデータがありません。再度確認してください。";
                            return opResult;
                        }
                    }
                    catch
                    {
                        dbTran.Rollback();
                        opResult.Status = false;
                        opResult.Message = "エラーが発生しました。もう一度お試しください。";
                        return opResult;
                    }
                }
            });

            return result;
        }
    }
}
