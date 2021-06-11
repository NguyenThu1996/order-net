using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Utility;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin;
using OPS.ViewModels.Admin.Map;
using OPS.ViewModels.Shared;
using OPS.ViewModels.User.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using OPS.Entity.Schemas;
using OPS.ViewModels.Admin.Map.MapForPrint;
using static OPS.Utility.Constants;

namespace OPS.Business.Repository
{
    public class MapRepository : IMapRepository
    {
        protected readonly OpsContext _context;

        public MapRepository(OpsContext context)
        {
            _context = context;
        }

        public bool CheckExistMap(int EventCd)
        {
            return _context.Maps.Any(x => x.EventCd == EventCd && !x.IsDeleted);
        }

        public IndexViewModel GetListMapItem(IndexViewModel model)
        {
            IndexViewModel result = new IndexViewModel();
            var maps = _context.Maps.Where(x => !x.IsDeleted).AsQueryable();
            if (model.EventCd != 0)
            {
                maps = maps.Where(x => x.EventCd == model.EventCd);
            }
            result.TotalRowsAfterFiltering = maps.Count();
            maps = Filtering(maps, model);
            result.MapItems = maps.Select(x => new
            {
                x.Cd,
                x.Period,
                x.Week,
                x.MonthWeek,
                x.EventCd,
                x.Event.Name,
                x.InsertUserId,
                x.InsertDate
            })
            .AsEnumerable()
            .Select(y => new MapItem
            {
                MapCd = y.Cd,
                EventCd = y.EventCd,
                EventName = y.Name,
                Period = y.Period,
                Week = y.Week,
                MonthWeek = y.MonthWeek,
                OutputDate = y.InsertDate,
                OutputUser = y.InsertUserId,
            }).ToList();

            return result;
        }

        public MapModel GetMapInfo(int EventCd, int? MapCd)
        {
            var result = new MapModel();
            var query = _context.MstEvents.Where(x => x.Cd == EventCd && !x.IsDeleted)
                .Select(x => new
                {
                    x.Cd,
                    x.Name,
                    x.Address,
                    x.Place,
                    x.StartDate,
                    x.EndDate,
                    x.StartTime,
                    x.EndTime,
                    MainSalesmanName = x.MainSalesman.Name,
                })
                .AsEnumerable();
            var itemMap = new MapModel();
            if (query.Any())
            {
                itemMap = query.Select(y => new MapModel
                {
                    EventCd = y.Cd,
                    EventName = y.Name,
                    EventAddress = y.Address,
                    EventPlace = y.Place,
                    EventStartDate = y.StartDate,
                    EventEndDate = y.EndDate,
                    StartTime = y.StartTime,
                    EndTime = y.EndTime,
                    SaleManResponsible = y.MainSalesmanName,
                    SaleManTargets = new List<SaleManTargetModel>()
                }).FirstOrDefault();
            }
            else
            {
                return null;
            }
            //get sales man target
            //get media target
            //get customer infor
            //get ranking
            var listSaleManTarget = _context.EventSalesAssigments
                                    .Where(x => x.EventCd == EventCd && !x.Event.IsDeleted).Select(x => new
                                    {
                                        x.EventCd,
                                        x.Salesman.Name,
                                        x.Salesman.Code,
                                        x.SalesmanCd,
                                        x.NewTargetAmount,
                                        x.NewTargetRevenue,
                                        x.EffortTargetAmount,
                                        x.EffortTargetRevenue,
                                    })
                                    .Take(26)
                                    .OrderBy(s => s.Code)
                                    .AsEnumerable()
                                    .Select(x => new SaleManTargetModel
                                    {
                                        EventCd = x.EventCd,
                                        SaleManCd = x.SalesmanCd,
                                        SaleManName = x.Name,
                                        TargetAmount = x.NewTargetAmount,
                                        TargetEvenue = x.NewTargetRevenue,
                                        EffortTargetAmount = x.EffortTargetAmount,
                                        EffortTargetEvenue = x.EffortTargetRevenue,
                                    })
                                    .ToList();

            if (listSaleManTarget.Any())
            {
                listSaleManTarget = GetResultSaleMan(listSaleManTarget, EventCd, MapCd);
                listSaleManTarget = GetResultEffortSaleMan(listSaleManTarget, EventCd, MapCd);
                itemMap.SaleManTargets = listSaleManTarget;
            }
            List<PlanMediaModel> resultMedias = new List<PlanMediaModel>();
            List<PlanMediaModel> mediaNews = GetListPlanMediaNew(EventCd);
            List<PlanMediaModel> mediaEfforts = GetListPlanMediaEffort(EventCd);
            itemMap.PlanMediaNews = ResultMediaNews(mediaNews, EventCd, MapCd);
            itemMap.PlanMediaEfforts = ResultMediaEfforts(mediaEfforts, EventCd, MapCd);
            itemMap.PlanMedias = ResultMedia(resultMedias, EventCd, MapCd);
            //Get customter info
            itemMap.CustomerInfo = GetCustomerInfo(EventCd);
            //get rankings
            itemMap.Rankings = GetRanking(EventCd);
            if (itemMap.PlanMedias.Any())
            {
                foreach (var item in itemMap.PlanMedias)
                {
                    itemMap.SaleResultAmount = itemMap.SaleResultAmount + item.ResultAmount;
                    itemMap.SaleResult = itemMap.SaleResult + item.ResultRevenueRound;
                }
            }
            if (itemMap.PlanMediaNews.Any())
            {
                foreach (var item in itemMap.PlanMediaNews)
                {
                    //Don't calculate 顧客ＤＭ 
                    if (!string.IsNullOrEmpty(item.MediaCode) && !item.MediaCode.Equals(MediaSale.SaleDM.GetEnumDescription()))
                    {
                        itemMap.NewResultAmount = itemMap.NewResultAmount + item.ResultAmount;
                        itemMap.NewResult = itemMap.NewResult + item.ResultRevenueRound;
                    }
                }
            }
            if (itemMap.PlanMediaEfforts.Any())
            {
                foreach (var item in itemMap.PlanMediaEfforts)
                {
                    itemMap.EffortResultAmount += item.ResultAmount;
                    itemMap.EffortResult += item.ResultRevenueRound;
                }
            }
            //Sort DM
            var tempMediaSales = itemMap.PlanMediaNews.FindAll(x => x.MediaCode == MediaSale.SaleDM.GetEnumDescription());
            itemMap.PlanMediaNews.RemoveAll(x => x.MediaCode == MediaSale.SaleDM.GetEnumDescription());
            tempMediaSales.AddRange(itemMap.PlanMediaNews);
            itemMap.PlanMediaNews = tempMediaSales;
            //End sort DM
            itemMap.BusinessHopes = GetBusinessHopes(MapCd);
            itemMap.EventRequests = GetEventRequests(MapCd);
            if (MapCd == null || MapCd == 0)
            {
                
            }
            //edit
            else
            {
                var map = _context.Maps.Where(x => x.Cd == MapCd && x.EventCd == EventCd && !x.IsDeleted)
                          .Select(x => new
                          {
                              x.Cd,
                              x.Plan,
                              x.MarginalProfitRatio,
                              x.BreakEvenPoint,
                              x.Profit,
                              x.BusinessHopes,
                              x.CallArea,
                              x.ClubMemberJoin,
                              x.ClubMemberTarget,
                              x.Decoration,
                              x.EffortTargetAmount,
                              x.EffortTargetRevenue,
                              x.EventTargetAmount,
                              x.EventTargetRevenue,
                              x.NewTargetAmount,
                              x.NewTargetRevenue,
                              x.IsLayoutChecked,
                              x.IsLayoutSubmitted,
                              x.Precaution,
                              x.ProductExAmount,
                              x.ProductExDeadline,
                              x.ProductExResponsible,
                              x.ProductIdea,
                              x.Progress,
                              x.StrategiesAndTactics,
                              x.TransportExpense,
                              x.VenueAnalysis,
                          })
                          .AsEnumerable()
                          .Select(y => new MapModel
                          {
                              Cd = y.Cd,
                              BreakEventPoint = y.BreakEvenPoint.HasValue ? Decimal.ToInt32(y.BreakEvenPoint.Value) : (int?)null,
                              MarginalProfitRatio = y.MarginalProfitRatio,
                              Profit = y.Profit,
                              VenueAnalysis = y.VenueAnalysis,
                              CallArea = y.CallArea,
                              ClubMemberJoin = y.ClubMemberJoin,
                              ClubMemberTarget = y.ClubMemberTarget,
                              Decoration = y.Decoration,
                              EffortTargetAmount = y.EffortTargetAmount,
                              EffortTargetRevenue = y.EffortTargetRevenue.HasValue ? Decimal.ToInt32(y.EffortTargetRevenue.Value) : (decimal?)null,
                              EventTargetAmount = y.EventTargetAmount,
                              EventTargetRevenue = y.EventTargetRevenue.HasValue ? Decimal.ToInt32(y.EventTargetRevenue.Value) : (decimal?)null,
                              NewTargetAmount = y.NewTargetAmount,
                              NewTargetRevenue = y.NewTargetRevenue.HasValue ? Decimal.ToInt32(y.NewTargetRevenue.Value) : (decimal?) null,
                              LayoutCheck = y.IsLayoutChecked.HasValue ? (y.IsLayoutChecked.Value ? (int)LayoutCheck.Done : (int)LayoutCheck.NotDone) : (int?)null,
                              LayoutSubmit = y.IsLayoutSubmitted.HasValue ? (y.IsLayoutSubmitted.Value ? (int)LayoutCheck.Done : (int)LayoutCheck.NotDone) : (int?)null,
                              Precaution = y.Precaution,
                              ProductExAmount = y.ProductExAmount,
                              ProductExDeadline = y.ProductExDeadline,
                              ProductExResponsible = y.ProductExResponsible,
                              ProductIdea = y.ProductIdea,
                              Progress = y.Progress,
                              StrategiesAndTactics = y.StrategiesAndTactics,
                              TransportExpense = y.TransportExpense,
                              Plan = y.Plan,
                          }).FirstOrDefault();

                if (map != null)
                {
                    itemMap.Cd = map.Cd;
                    itemMap.BreakEventPoint = map.BreakEventPoint;
                    itemMap.MarginalProfitRatio = map.MarginalProfitRatio;
                    itemMap.Profit = map.Profit;
                    itemMap.VenueAnalysis = map.VenueAnalysis;
                    itemMap.CallArea = map.CallArea;
                    itemMap.ClubMemberJoin = map.ClubMemberJoin;
                    itemMap.ClubMemberTarget = map.ClubMemberTarget;
                    itemMap.Decoration = map.Decoration;
                    itemMap.EffortTargetAmount = map.EffortTargetAmount;
                    itemMap.EffortTargetRevenue = map.EffortTargetRevenue;
                    itemMap.EventTargetAmount = map.EventTargetAmount;
                    itemMap.EventTargetRevenue = map.EventTargetRevenue;
                    itemMap.NewTargetAmount = map.NewTargetAmount;
                    itemMap.NewTargetRevenue = map.NewTargetRevenue;
                    itemMap.LayoutCheck = map.LayoutCheck;
                    itemMap.LayoutSubmit = map.LayoutSubmit;
                    itemMap.Precaution = map.Precaution;
                    itemMap.ProductExAmount = map.ProductExAmount;
                    itemMap.ProductExDeadline = map.ProductExDeadline;
                    itemMap.ProductExResponsible = map.ProductExResponsible;
                    itemMap.ProductIdea = map.ProductIdea;
                    itemMap.Progress = map.Progress;
                    itemMap.StrategiesAndTactics = map.StrategiesAndTactics;
                    itemMap.TransportExpense = map.TransportExpense;
                    itemMap.Plan = map.Plan;
                    itemMap.SaleManTargets = GetSaleManCertain(itemMap.SaleManTargets, EventCd, MapCd);
                }
                else
                {
                    return null;
                }
            }
            return itemMap;
        }

        private List<SaleManTargetModel> GetResultSaleMan(List<SaleManTargetModel> SaleManTargets, int EventCd, int? MapCd)
        {
            var saleManResult = _context.Contracts
                                .Where(x => x.EventCd == EventCd && x.IsCompleted && !x.IsDeleted && !x.FutureEventCd.HasValue)
                                .Include(x => x.SalesmanA)
                                .Include(x => x.SalesmanC)
                                .Select(x => new
                                {
                                    x.Cd,
                                    x.SalesmanACd,
                                    SalesmanAName = x.SalesmanA.Name,
                                    x.SalesmanCCd,
                                    SalesmanCName = x.SalesmanC.Name,
                                    x.SalesPrice,
                                })
                                .AsEnumerable()
                                .Select(y => new SaleManTargetTemp
                                {
                                    ContractCd = y.Cd,
                                    SalemanACd = y.SalesmanACd,
                                    SalemanAName = y.SalesmanAName,
                                    SalemanCCd = y.SalesmanCCd,
                                    SalemanCName = y.SalesmanCName,
                                    Price = y.SalesPrice
                                });
            var lstSaleman = new List<SaleManTargetTemp>();
            if (saleManResult.Any())
            {
                foreach (var item in saleManResult)
                {
                    var saleManA = new SaleManTargetTemp
                    {
                        ContractCd = item.ContractCd,
                        SalemanACd = item.SalemanACd,
                        SalemanAName = item.SalemanAName,
                        Price = item.Price / 2
                    };
                    lstSaleman.Add(saleManA);
                    var saleManC = new SaleManTargetTemp
                    {
                        ContractCd = item.ContractCd,
                        SalemanACd = item.SalemanCCd,
                        SalemanAName = item.SalemanCName,
                        Price = item.Price / 2
                    };
                    lstSaleman.Add(saleManC);
                }
            }
            var saleManResults = new List<SaleManTargetModel>();
            if (lstSaleman.Any())
            {
                saleManResults = lstSaleman
                                .GroupBy(x => x.SalemanACd)
                                .Select(y => new SaleManTargetModel
                                {
                                    SaleManCd = y.Key.GetValueOrDefault(),
                                    SaleManName = y.FirstOrDefault().SalemanAName,
                                    ResultAmount = y.Count(),
                                    ResultEvenue = y.Sum(z => z.Price)
                                })
                                .ToList();
            }
            if (saleManResults.Any())
            {
                foreach (var item in SaleManTargets)
                {
                    foreach (var rs in saleManResults)
                    {
                        if (item.SaleManCd == rs.SaleManCd)
                        {
                            item.ResultAmount = rs.ResultAmount;
                            item.ResultEvenue = rs.ResultEvenue;
                        }
                    }
                }
            }
            return SaleManTargets;
        }

        private List<SaleManTargetModel> GetResultEffortSaleMan(List<SaleManTargetModel> SaleManEffortTargets, int EventCd, int? MapCd)
        {
            var saleManResult = _context.Contracts
                                .Where(x => x.EventCd == EventCd && x.IsCompleted && !x.IsDeleted && !x.FutureEventCd.HasValue && x.SalesmanSCd.HasValue)
                                .Include(x => x.SalesmanS)
                                .AsEnumerable()
                                .GroupBy(x => x.SalesmanSCd.Value)
                                .Select(y => new SaleManTargetModel
                                {
                                    SaleManCd = y.Key,
                                    SaleManName = y.FirstOrDefault().SalesmanS.Name,
                                    ResultEffortAmount = y.Count(),
                                    ResultEffortEvenue = y.Sum(z => z.SalesPrice),
                                }).ToList();

            if (saleManResult.Any())
            {
                foreach (var item in SaleManEffortTargets)
                {
                    foreach (var rs in saleManResult)
                    {
                        if (item.SaleManCd == rs.SaleManCd)
                        {
                            item.ResultEffortAmount = rs.ResultEffortAmount;
                            item.ResultEffortEvenue = rs.ResultEffortEvenue;
                        }
                    }
                }
            }
            return SaleManEffortTargets;
        }

        private List<SaleManTargetModel> GetSaleManCertain(List<SaleManTargetModel> ListSaleMan, int EventCd, int? MapCd)
        {
            var saleManCertains = _context.SalesmanTargets
                     .Where(x => x.EventCd == EventCd && x.MapCd == MapCd.Value)
                     .Select(x => new
                     {
                         x.SalesmanCd,
                         x.CertainTargetAmount,
                         x.ResultCertainAmount,
                         x.EventCd,
                     })
                     .AsEnumerable()
                     .Select(y => new SaleManTargetModel
                     {
                         EventCd = y.EventCd,
                         SaleManCd = y.SalesmanCd,
                         CertainTargetAmount = y.CertainTargetAmount,
                         ResultCertainAmount = y.ResultCertainAmount,
                     })
                     .ToList();
            if (saleManCertains.Any())
            {
                foreach (var saleMan in ListSaleMan)
                {
                    foreach (var certain in saleManCertains)
                    {
                        if (saleMan.SaleManCd == certain.SaleManCd)
                        {
                            saleMan.CertainTargetAmount = certain.CertainTargetAmount;
                            saleMan.ResultCertainAmount = certain.ResultCertainAmount;
                        }
                    }
                }
            }
            return ListSaleMan;
        }
        private List<PlanMediaModel> ResultMedia(List<PlanMediaModel> PlanMedias, int EventCd, int? MapCd)
        {
            var planMediasResults = _context.Contracts
                                    .Where(x => x.EventCd == EventCd && x.IsCompleted && !x.IsDeleted && !x.FutureEventCd.HasValue)
                                    .Include(x => x.Event.Surveys)
                                    .Include(x => x.Media)
                                    .Select(x => new
                                    {
                                        x.MediaCd,
                                        x.Media,
                                        x.Event.Surveys,
                                        x.SalesPrice,
                                    })
                                    .AsEnumerable()
                                    .GroupBy(x => x.MediaCd)
                                    .Select(y => new PlanMediaModel
                                    {
                                        MediaCd = y.Key.GetValueOrDefault(),
                                        MediaName = y.FirstOrDefault().Media.Name,
                                        ResultAmount = y.Count(),
                                        ResultRevenue = y.Sum(x => x.SalesPrice.Value),
                                        EventCd = EventCd,
                                        AttractCustomers = y.FirstOrDefault().Surveys.Where(z => z.EventCd == EventCd && z.MediaCd == y.Key && !z.IsDeleted).Count(),
                                        IsNotChangable = true
                                    })
                                    .ToList();

            if (planMediasResults.Any())
            {
                var medias = planMediasResults.Except(PlanMedias);
                // have result but not have register
                if (medias.Any())
                {
                    foreach (var item in medias)
                    {
                        PlanMedias.Add(item);
                    }
                }
                // have register
                else
                {
                    foreach (var rs in planMediasResults)
                    {
                        foreach (var media in PlanMedias)
                        {
                            if (rs.MediaCd == media.MediaCd && rs.EventCd == media.EventCd)
                            {
                                media.ResultAmount = rs.ResultAmount;
                                media.ResultRevenue = rs.ResultRevenue;
                                media.AttractCustomers = rs.AttractCustomers;
                                media.IsNotChangable = rs.IsNotChangable;
                            }
                        }
                    }
                }
            }
            return PlanMedias.OrderBy(x => x.MediaCode).ToList(); ;
        }

        private List<PlanMediaModel> ResultMediaNews(List<PlanMediaModel> PlanMedias, int EventCd, int? MapCd)
        {
            var queryContract = _context.Contracts
                                    .Include(x => x.Media)
                                    .Where(x => x.EventCd == EventCd && x.IsCompleted && !x.IsDeleted && !x.FutureEventCd.HasValue &&
                                                !x.Media.IsDeleted &&
                                                !x.Media.Code.Equals(MediaEffort.CallO.GetEnumDescription()) &&
                                                !x.Media.Code.Equals(MediaEffort.CallQ.GetEnumDescription()) &&
                                                !x.Media.Code.Equals(MediaEffort.CallR.GetEnumDescription())
                                                )
                                    .Select(x => new
                                    {
                                        x.MediaCd,
                                        x.Media,
                                        x.SalesPrice,
                                    })
                                    .AsEnumerable();
            var querySurvey = _context.Surveys
                              .Where(x => x.EventCd == EventCd && !x.IsDeleted)
                              .Select(x => new
                              {
                                  x.MediaCd,
                              })
                              .AsEnumerable();
            var planMediasResults = new List<PlanMediaModel>();
            if (queryContract.Any())
            {
                planMediasResults = queryContract.GroupBy(x => x.MediaCd)
                                    .Select(y => new PlanMediaModel
                                    {
                                        MediaCd = y.Key.GetValueOrDefault(),
                                        MediaFlag = (int)MediaFlag.New,
                                        MediaCode = y.FirstOrDefault().Media.Code,
                                        MediaName = y.FirstOrDefault().Media.Name,
                                        MediaBranch = y.FirstOrDefault().Media.BranchCode,
                                        ResultAmount = y.Count(),
                                        ResultRevenue = y.Sum(x => x.SalesPrice.Value),
                                        EventCd = EventCd,
                                        IsNotChangable = true
                                    })
                                    .OrderBy(y => y.MediaCd)
                                    .ToList();
            }
            var planMediasSuveys = new List<PlanMediaModel>();
            if (querySurvey.Any()) {
                planMediasSuveys = querySurvey.GroupBy(x => x.MediaCd)
                                   .Select(y => new PlanMediaModel
                                   {
                                       MediaCd = y.Key,
                                       AttractCustomers = y.Count(),
                                       IsNotChangable = true
                                   })
                                   .ToList();
            }
            foreach (var media in PlanMedias)
            {
                var survey  = planMediasSuveys.FirstOrDefault(x => x.MediaCd == media.MediaCd);
                if(survey != null)
                {
                    media.AttractCustomers = survey.AttractCustomers;
                    media.IsNotChangable = survey.IsNotChangable;
                }
            }
            if (MapCd != null || MapCd > 0)
            {
                var planMediasMap = _context.PlanMedias
                                    .Include(x => x.Media)
                                    .Where(x => x.EventCd == EventCd && x.MapCd == MapCd &&
                                                !x.Media.IsDeleted &&
                                                !x.Media.Code.Equals(MediaEffort.CallO.GetEnumDescription()) &&
                                                !x.Media.Code.Equals(MediaEffort.CallQ.GetEnumDescription()) &&
                                                !x.Media.Code.Equals(MediaEffort.CallR.GetEnumDescription()))
                                    .Select(x => new
                                    {
                                        x.MediaCd,
                                        x.Media,
                                        x.OrderAmount,
                                        x.Unit,
                                        x.NumberOfCustomers,
                                        x.EstimatedRevenue,
                                        x.Cost,
                                    })
                                    .AsEnumerable()
                                    .Select(y => new PlanMediaModel
                                    {
                                        MediaCd = y.MediaCd,
                                        MediaCode = y.Media.Code,
                                        MediaBranch = y.Media.BranchCode,
                                        MediaName = y.Media.Name,
                                        Spec = y.Media.Spec,
                                        OrderUnit = y.Unit,
                                        OrderAmount = y.OrderAmount,
                                        NumberOfCustomers = y.NumberOfCustomers,
                                        EstimatedRevenue = y.EstimatedRevenue,
                                        Cost = y.Cost
                                    })
                                    .ToList();
                if (planMediasMap.Any())
                {
                    var medias = planMediasMap.Except(PlanMedias);
                    // have map but not have register
                    if (medias.Any())
                    {
                        foreach (var item in medias)
                        {
                            PlanMedias.Add(item);
                        }
                    }
                    // have register
                    else
                    {
                        foreach (var map in planMediasMap)
                        {
                            foreach (var media in PlanMedias)
                            {
                                if (map.MediaCd == media.MediaCd)
                                {
                                    media.OrderAmount = map.OrderAmount;
                                    media.OrderUnit = map.OrderUnit;
                                    media.NumberOfCustomers = map.NumberOfCustomers;
                                    media.EstimatedRevenue = map.EstimatedRevenue;
                                    media.Cost = map.Cost;
                                }
                            }
                        }
                    }
                }
            }
            if (planMediasResults.Any())
            {
                var medias = planMediasResults.Except(PlanMedias);
                // have result but not have register
                if (medias.Any())
                {
                    foreach (var item in medias)
                    {
                        PlanMedias.Add(item);
                    }
                }
                // have register
                else
                {
                    foreach (var rs in planMediasResults)
                    {
                        foreach (var media in PlanMedias)
                        {
                            if (rs.MediaCd == media.MediaCd)
                            {
                                media.ResultAmount = rs.ResultAmount;
                                media.ResultRevenue = rs.ResultRevenue;
                                media.IsNotChangable = rs.IsNotChangable;
                            }
                        }
                    }
                }
            }
            PlanMedias = PlanMedias.OrderBy(x => x.MediaCode).ThenBy(x => x.MediaBranch).ToList();
            var length = PlanMedias.Count();
            if (length < 4)
            {
                for (var i = 0; i < 4 - length; i++)
                {
                    PlanMedias.Add(new PlanMediaModel());
                }
            }
            return PlanMedias;
        }

        private List<PlanMediaModel> ResultMediaEfforts(List<PlanMediaModel> PlanMedias, int EventCd, int? MapCd)
        {
            var query = _context.Contracts
                                    .Include(x => x.Media)
                                    .Where(x => x.EventCd == EventCd && x.IsCompleted && !x.IsDeleted && !x.FutureEventCd.HasValue &&
                                                (!x.Media.IsDeleted && x.Media.Code.Equals(MediaEffort.CallO.GetEnumDescription()) ||
                                                  x.Media.Code.Equals(MediaEffort.CallQ.GetEnumDescription()) ||
                                                  x.Media.Code.Equals(MediaEffort.CallR.GetEnumDescription())))
                                    .Select(x => new
                                    {
                                        x.MediaCd,
                                        x.Media,
                                        x.SalesPrice,
                                    })
                                    .AsEnumerable();
            var querySurvey = _context.Surveys
                              .Where(x => x.EventCd == EventCd && !x.IsDeleted)
                              .Select(x => new
                              {
                                  x.MediaCd,
                              })
                              .AsEnumerable();
            var planMediasResults = new List<PlanMediaModel>();
            if (query.Any())
            {
                planMediasResults = query.GroupBy(x => x.MediaCd)
                                    .Select(y => new PlanMediaModel
                                    {
                                        MediaCd = y.Key.GetValueOrDefault(),
                                        MediaFlag = (int)MediaFlag.Customer,
                                        MediaName = y.FirstOrDefault().Media.Name,
                                        MediaCode = y.FirstOrDefault().Media.Code,
                                        MediaBranch = y.FirstOrDefault().Media.BranchCode,
                                        ResultAmount = y.Count(),
                                        ResultRevenue = y.Sum(x => x.SalesPrice.Value),
                                        EventCd = EventCd,
                                        IsNotChangable = true
                                    })
                                    .OrderBy(y => y.MediaCd)
                                    .ToList();
            }
            var planMediasSuveys = new List<PlanMediaModel>();
            if (querySurvey.Any())
            {
                planMediasSuveys = querySurvey.GroupBy(x => x.MediaCd)
                                   .Select(y => new PlanMediaModel
                                   {
                                       MediaCd = y.Key,
                                       AttractCustomers = y.Count(),
                                       IsNotChangable = true
                                   })
                                   .ToList();
            }
            foreach (var media in PlanMedias)
            {
                var survey = planMediasSuveys.FirstOrDefault(x => x.MediaCd == media.MediaCd);
                if (survey != null)
                {
                    media.AttractCustomers = survey.AttractCustomers;
                    media.IsNotChangable = survey.IsNotChangable;
                }
            }
            if (MapCd != null || MapCd > 0)
            {
                var planMediasMap = _context.PlanMedias
                                    .Include(x => x.Media)
                                    .Where(x => x.EventCd == EventCd && x.MapCd == MapCd &&
                                                  (!x.Media.IsDeleted && x.Media.Code.Equals(MediaEffort.CallO.GetEnumDescription()) ||
                                                  x.Media.Code.Equals(MediaEffort.CallQ.GetEnumDescription()) ||
                                                  x.Media.Code.Equals(MediaEffort.CallR.GetEnumDescription())))
                                    .Select(x => new
                                    {
                                        x.MediaCd,
                                        x.Media,
                                        x.OrderAmount,
                                        x.Unit,
                                        x.NumberOfCustomers,
                                        x.EstimatedRevenue,
                                        x.Cost,
                                    })
                                    .AsEnumerable()
                                    .Select(y => new PlanMediaModel
                                    {
                                        MediaCd = y.MediaCd,
                                        MediaCode = y.Media.Code,
                                        MediaBranch = y.Media.BranchCode,
                                        MediaName = y.Media.Name,
                                        Spec = y.Media.Spec,
                                        OrderUnit = y.Unit,
                                        OrderAmount = y.OrderAmount,
                                        NumberOfCustomers = y.NumberOfCustomers,
                                        EstimatedRevenue = y.EstimatedRevenue,
                                        Cost = y.Cost,
                                    })
                                    .ToList();
                if (planMediasMap.Any())
                {
                    var medias = planMediasMap.Except(PlanMedias);
                    // have map but not have register
                    if (medias.Any())
                    {
                        foreach (var item in medias)
                        {
                            PlanMedias.Add(item);
                        }
                    }
                    // have register
                    else
                    {
                        foreach (var map in planMediasMap)
                        {
                            foreach (var media in PlanMedias)
                            {
                                if (map.MediaCd == media.MediaCd)
                                {
                                    media.OrderAmount = map.OrderAmount;
                                    media.OrderUnit = map.OrderUnit;
                                    media.NumberOfCustomers = map.NumberOfCustomers;
                                    media.EstimatedRevenue = map.EstimatedRevenue;
                                    media.Cost = map.Cost;
                                }
                            }
                        }
                    }
                }
            }

            if (planMediasResults.Any())
            {
                var medias = planMediasResults.Except(PlanMedias);
                // have result but not have register
                if (medias.Any())
                {
                    foreach (var item in medias)
                    {
                        PlanMedias.Add(item);
                    }
                }
                // have register
                else
                {
                    foreach (var rs in planMediasResults)
                    {
                        foreach (var media in PlanMedias)
                        {
                            if (rs.MediaCd == media.MediaCd)
                            {
                                media.ResultAmount = rs.ResultAmount;
                                media.ResultRevenue = rs.ResultRevenue;
                                media.IsNotChangable = rs.IsNotChangable;
                            }
                        }
                    }
                }
            }
            PlanMedias = PlanMedias.OrderBy(x => x.MediaCode).ThenBy(x => x.MediaBranch).ToList();
            var length = PlanMedias.Count();
            if (length < 4)
            {
                for (var i = 0; i < 4 - length; i++)
                {
                    PlanMedias.Add(new PlanMediaModel());
                }
            }
            return PlanMedias;
        }

        private List<EventRequestModel> GetEventRequests(int? MapCd)
        {
            var rs = new List<EventRequestModel>();
            if (MapCd.HasValue)
            {
                var data = _context.EventRequests.Where(x => x.MapCd == MapCd && !x.Map.IsDeleted)
                           .OrderBy(x => x.InsertDate)
                           .Select(x => new
                           {
                               x.MapCd,
                               x.Name,
                               x.Unit,
                               x.Cd,
                           })
                           .AsEnumerable()
                           .Select(y => new EventRequestModel
                           {
                               Cd = y.Cd,
                               MapCd = y.MapCd,
                               Name = y.Name,
                               Unit = y.Unit,
                           })
                           .ToList();
                rs = data;
            }
            var length = rs.Count;
            if (length < 7)
            {
                for (var i = 0; i < 7 - length; i++)
                {
                    rs.Add(new EventRequestModel());
                }
            }
            return rs;
        }

        private List<BusinessHopeModel> GetBusinessHopes(int? MapCd)
        {
            var rs = new List<BusinessHopeModel>();
            if (MapCd.HasValue)
            {
                var data = _context.BusinessHopes.Where(x => x.MapCd == MapCd && !x.Map.IsDeleted)
                           .OrderBy(x => x.InsertDate)
                           .Select(x => new
                           {
                               x.Cd,
                               x.MapCd,
                               x.Artist.Name,
                               x.Artist.Code,
                               x.ArtistCd,
                               x.Type,
                               x.Desgin,
                               x.DesiredNumber,
                               x.Remark
                           })
                           .AsEnumerable()
                           .Select(y => new BusinessHopeModel
                           {
                               Cd = y.Cd,
                               MapCd = y.MapCd,
                               ArtistCd = y.ArtistCd,
                               ArtistCode = y.Code,
                               ArtistName = y.Name,
                               Desgin = y.Desgin,
                               Type = y.Type,
                               DesiredNumber = y.DesiredNumber,
                               Remark = y.Remark
                           })
                           .ToList();
                rs = data;
            }
            var length = rs.Count;
            if (length < 7)
            {
                for (var i = 0; i < 7 - length; i++)
                {
                    rs.Add(new BusinessHopeModel());
                }
            }
            return rs;
        }

        private CustomerInfoModel GetCustomerInfo(int EventCd)
        {
            var query = _context.Contracts
                        .Where(x => x.EventCd == EventCd && x.IsCompleted && !x.IsDeleted && !x.FutureEventCd.HasValue)
                        .Include(x => x.Event)
                        .AsEnumerable();
            var rs = new CustomerInfoModel();
            if (query.Any())
            {
                rs = query.GroupBy(x => x.EventCd)
                     .Select(y => new CustomerInfoModel
                     {
                         EventCd = y.Key,
                         Card = y.Count(c => c.LeftPaymentMethod == (int)LeftPaymentMethod.Card),
                         Cash = y.Count(c => c.LeftPaymentMethod == (int)LeftPaymentMethod.InCash || c.LeftPaymentMethod == (int)LeftPaymentMethod.BankTranfer),
                         Credit = y.Count(c => c.LeftPaymentMethod == (int)LeftPaymentMethod.Credit),
                         U20FeMale = y.Count(c => c.Age < (int)AgeRangeMap.U30 && c.Gender == (int?)Gender.Female),
                         U30FeMale = y.Count(c => c.Age < (int)AgeRangeMap.U40 && c.Age >= (int)AgeRangeMap.U30 && c.Gender == (int?)Gender.Female),
                         U40FeMale = y.Count(c => c.Age < (int)AgeRangeMap.U50 && c.Age >= (int)AgeRangeMap.U40 && c.Gender == (int?)Gender.Female),
                         U50FeMale = y.Count(c => c.Age < (int)AgeRangeMap.U60 && c.Age >= (int)AgeRangeMap.U50 && c.Gender == (int?)Gender.Female),
                         U60FeMale = y.Count(c => c.Age < (int)AgeRangeMap.U70 && c.Age >= (int)AgeRangeMap.U60 && c.Gender == (int?)Gender.Female),
                         U70FeMale = y.Count(c => c.Age < (int)AgeRangeMap.U80 && c.Age >= (int)AgeRangeMap.U70 && c.Gender == (int?)Gender.Female),
                         U80FeMale = y.Count(c => c.Age >= (int)AgeRangeMap.U80 && c.Gender == (int?)Gender.Female),
                         U20Male = y.Count(c => c.Age < (int)AgeRangeMap.U30 && c.Gender == (int?)Gender.Male),
                         U30Male = y.Count(c => c.Age < (int)AgeRangeMap.U40 && c.Age >= (int)AgeRangeMap.U30 && c.Gender == (int?)Gender.Male),
                         U40Male = y.Count(c => c.Age < (int)AgeRangeMap.U50 && c.Age >= (int)AgeRangeMap.U40 && c.Gender == (int?)Gender.Male),
                         U50Male = y.Count(c => c.Age < (int)AgeRangeMap.U60 && c.Age >= (int)AgeRangeMap.U50 && c.Gender == (int?)Gender.Male),
                         U60Male = y.Count(c => c.Age < (int)AgeRangeMap.U70 && c.Age >= (int)AgeRangeMap.U60 && c.Gender == (int?)Gender.Male),
                         U70Male = y.Count(c => c.Age < (int)AgeRangeMap.U80 && c.Age >= (int)AgeRangeMap.U70 && c.Gender == (int?)Gender.Male),
                         U80Male = y.Count(c => c.Age >= (int)AgeRangeMap.U80 && c.Gender == (int?)Gender.Male),
                         AverageIncome = 1000,
                         FirstTime = y.Count(c => c.VisitTime == (int)VisitTime.first_time),
                         SecondTime = y.Count(c => c.VisitTime == (int)VisitTime.second_times),
                         ManyTime = y.Count(c => c.VisitTime == (int)VisitTime.more_than_2_times),
                     }).FirstOrDefault();
            }

            return rs;
        }

        private List<RankingModel> GetRanking(int EventCd)
        {
            var rs = new List<RankingModel>();
            var products = _context.Contracts.Where(x => x.EventCd == EventCd && x.IsCompleted && !x.IsDeleted && !x.FutureEventCd.HasValue)
                             .Select(x => new
                             {
                                 x.ArtistCd1,
                                 x.ArtistName1,
                                 x.ProductCd1,
                                 x.ProductName1,
                                 x.ProductQuantity1,
                                 x.ProductPrice1,
                                 x.ArtistCd2,
                                 x.ArtistName2,
                                 x.ProductCd2,
                                 x.ProductName2,
                                 x.ProductQuantity2,
                                 x.ProductPrice2,
                                 x.ArtistCd3,
                                 x.ArtistName3,
                                 x.ProductCd3,
                                 x.ProductName3,
                                 x.ProductQuantity3,
                                 x.ProductPrice3,
                                 x.ArtistCd4,
                                 x.ArtistName4,
                                 x.ProductCd4,
                                 x.ProductName4,
                                 x.ProductQuantity4,
                                 x.ProductPrice4,
                                 x.ArtistCd5,
                                 x.ArtistName5,
                                 x.ProductCd5,
                                 x.ProductName5,
                                 x.ProductQuantity5,
                                 x.ProductPrice5,
                                 x.ArtistCd6,
                                 x.ArtistName6,
                                 x.ProductCd6,
                                 x.ProductName6,
                                 x.ProductQuantity6,
                                 x.ProductPrice6,
                             })
                             .AsEnumerable()
                             .Select(x => new ContractForPrint
                             {
                                 ArtistCd1 = x.ArtistCd1,
                                 ActistName1 = x.ArtistName1,
                                 ProductName1 = x.ProductName1,
                                 ProductCd1 = x.ProductCd1,
                                 ProductQuantity1 = x.ProductQuantity1,
                                 ProductPrice1 = x.ProductPrice1,
                                 ArtistCd2 = x.ArtistCd2,
                                 ActistName2 = x.ArtistName2,
                                 ProductName2 = x.ProductName2,
                                 ProductCd2 = x.ProductCd2,
                                 ProductQuantity2 = x.ProductQuantity2,
                                 ProductPrice2 = x.ProductPrice2,
                                 ArtistCd3 = x.ArtistCd3,
                                 ActistName3 = x.ArtistName3,
                                 ProductName3 = x.ProductName3,
                                 ProductCd3 = x.ProductCd3,
                                 ProductQuantity3 = x.ProductQuantity3,
                                 ProductPrice3 = x.ProductPrice3,
                                 ArtistCd4 = x.ArtistCd4,
                                 ActistName4 = x.ArtistName4,
                                 ProductName4 = x.ProductName4,
                                 ProductCd4 = x.ProductCd4,
                                 ProductQuantity4 = x.ProductQuantity4,
                                 ProductPrice4 = x.ProductPrice4,
                                 ArtistCd5 = x.ArtistCd5,
                                 ActistName5 = x.ArtistName5,
                                 ProductName5 = x.ProductName5,
                                 ProductCd5 = x.ProductCd5,
                                 ProductQuantity5 = x.ProductQuantity5,
                                 ProductPrice5 = x.ProductPrice5,
                                 ArtistCd6 = x.ArtistCd6,
                                 ActistName6 = x.ArtistName6,
                                 ProductName6 = x.ProductName6,
                                 ProductCd6 = x.ProductCd6,
                                 ProductQuantity6 = x.ProductQuantity6,
                                 ProductPrice6 = x.ProductPrice6,
                             }).ToList();

            var productItems = new List<ProductItem>();
            if (products.Any())
            {
                foreach (var item in products)
                {
                    if (item.ArtistCd1.HasValue || !string.IsNullOrEmpty(item.ActistName1))
                    {
                        var product1 = new ProductItem
                        {
                            ArtistCd = item.ArtistCd1,
                            ArtistName = item.ActistName1,
                            ProductCd = item.ProductCd1,
                            ProductName = item.ProductName1,
                            Amount = item.ProductQuantity1,
                            Price = item.ProductPrice1
                        };
                        productItems.Add(product1);
                    }
                    if (item.ArtistCd2.HasValue || !string.IsNullOrEmpty(item.ActistName2))
                    {
                        var product2 = new ProductItem
                        {
                            ArtistCd = item.ArtistCd2,
                            ArtistName = item.ActistName2,
                            ProductCd = item.ProductCd2,
                            ProductName = item.ProductName2,
                            Amount = item.ProductQuantity2,
                            Price = item.ProductPrice2
                        };
                        productItems.Add(product2);
                    }
                    if (item.ArtistCd3.HasValue || !string.IsNullOrEmpty(item.ActistName3))
                    {
                        var product3 = new ProductItem
                        {
                            ArtistCd = item.ArtistCd3,
                            ArtistName = item.ActistName3,
                            ProductCd = item.ProductCd3,
                            ProductName = item.ProductName3,
                            Amount = item.ProductQuantity3,
                            Price = item.ProductPrice3
                        };
                        productItems.Add(product3);
                    }
                    if (item.ArtistCd4.HasValue || !string.IsNullOrEmpty(item.ActistName4))
                    {
                        var product4 = new ProductItem
                        {
                            ArtistCd = item.ArtistCd4,
                            ArtistName = item.ActistName4,
                            ProductCd = item.ProductCd4,
                            ProductName = item.ProductName4,
                            Amount = item.ProductQuantity4,
                            Price = item.ProductPrice4
                        };
                        productItems.Add(product4);
                    }
                    if (item.ArtistCd5.HasValue || !string.IsNullOrEmpty(item.ActistName5))
                    {
                        var product5 = new ProductItem
                        {
                            ArtistCd = item.ArtistCd5,
                            ArtistName = item.ActistName5,
                            ProductCd = item.ProductCd5,
                            ProductName = item.ProductName5,
                            Amount = item.ProductQuantity5,
                            Price = item.ProductPrice5
                        };
                        productItems.Add(product5);
                    }
                    if (item.ArtistCd6.HasValue || !string.IsNullOrEmpty(item.ActistName6))
                    {
                        var product6 = new ProductItem
                        {
                            ArtistCd = item.ArtistCd6,
                            ArtistName = item.ActistName6,
                            ProductCd = item.ProductCd6,
                            ProductName = item.ProductName6,
                            Amount = item.ProductQuantity6,
                            Price = item.ProductPrice6
                        };
                        productItems.Add(product6);
                    }
                }

                var lsRanking = productItems.GroupBy(x => new { x.ArtistCd, x.ArtistName, x.ProductName, x.ProductCd })
                                    .Select(y => new ProductItem
                                    {
                                        ArtistCd = y.Key.ArtistCd,
                                        ArtistName = y.Key.ArtistName,
                                        ProductCd = y.Key.ProductCd,
                                        ProductName = y.Key.ProductName,
                                        Amount = y.Sum(z => z.Amount),
                                        Price = y.Sum(z => z.Price),
                                    }).OrderByDescending(y => y.Amount).ThenByDescending(y => y.Price).ThenByDescending(y => y.ProductName).ToList();

                if (lsRanking.Any())
                {
                    for (int i = 0; i < lsRanking.Count && i < 3; i++)
                    {
                        var rank = new RankingModel()
                        {
                            ArtistCd = lsRanking[i].ArtistCd,
                            ArtistName = lsRanking[i].ArtistName,
                            ProductCd = lsRanking[i].ProductCd,
                            ProductName = lsRanking[i].ProductName,
                            Amount = lsRanking[i].Amount,
                            Rank = i + 1,
                        };
                        rs.Add(rank);
                    }
                }
                else
                {
                    for (var i = 0; i < 3; i++)
                    {
                        rs.Add(new RankingModel());
                    }
                }

            }
            else
            {
                for (var i = 0; i < 3; i++)
                {
                    rs.Add(new RankingModel());
                }
            }
            return rs;
        }

        private IQueryable<Entity.Schemas.Map> Filtering(IQueryable<Entity.Schemas.Map> maps, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "MonthWeek":
                    if (filtering.SortDirection == "asc")
                    {
                        //maps = maps.OrderBy(x => x.Period).ThenBy(x => x.Week).Skip(filtering.Start).Take(filtering.Length);
                        maps = maps.OrderBy(x => x.MonthWeek).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        //maps = maps.OrderByDescending(x => x.Period).ThenByDescending(x => x.Week).Skip(filtering.Start).Take(filtering.Length);
                        maps = maps.OrderByDescending(x => x.MonthWeek).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "EventName":
                    if (filtering.SortDirection == "asc")
                    {
                        maps = maps.OrderBy(x => x.Event.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        maps = maps.OrderByDescending(x => x.Event.Name).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "OutputUser":
                    if (filtering.SortDirection == "asc")
                    {
                        maps = maps.OrderBy(x => x.InsertUserId).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        maps = maps.OrderByDescending(x => x.InsertUserId).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "OutputDate":
                    if (filtering.SortDirection == "asc")
                    {
                        maps = maps.OrderBy(x => x.InsertDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        maps = maps.OrderByDescending(x => x.InsertDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                    maps = maps.OrderByDescending(x => x.InsertDate).Skip(filtering.Start).Take(filtering.Length);
                    break;
            }
            return maps;
        }

        private List<PlanMediaModel> GetListPlanMediaNew(int EventCd)
        {
            var rs = new List<PlanMediaModel>();
            var query = _context.EventMedias
                    .Include(x => x.Media)
                    .Where(x => x.EventCd == EventCd &&
                                !x.Media.IsDeleted &&
                                !x.Media.Code.Equals(MediaEffort.CallO.GetEnumDescription()) &&
                                !x.Media.Code.Equals(MediaEffort.CallQ.GetEnumDescription()) &&
                                !x.Media.Code.Equals(MediaEffort.CallR.GetEnumDescription()))
                    .Select(x => new
                    {
                        x.Media.Cd,
                        x.Media.Code,
                        x.Media.Spec,
                        x.Media.Name,
                        x.Media.BranchCode,
                    })
                    .AsEnumerable();
            if (query.Any())
            {
                rs = query.Select(y => new PlanMediaModel
                {
                    MediaCd = y.Cd,
                    Spec = y.Spec,
                    MediaCode = y.Code,
                    MediaName = y.Name,
                    MediaBranch = y.BranchCode
                })
                .OrderBy(y => y.MediaCode).ThenBy(y => y.MediaBranch)
                .ToList();
            }
            return rs;
        }

        public List<PlanMediaModel> GetListPlanMediaEffort(int EventCd)
        {
            var rs = new List<PlanMediaModel>();
            var query = _context.EventMedias
                     .Include(x => x.Media)
                     .Where(x => x.EventCd == EventCd &&
                                 !x.Media.IsDeleted &&
                                 (x.Media.Code.Equals(MediaEffort.CallO.GetEnumDescription()) ||
                                  x.Media.Code.Equals(MediaEffort.CallQ.GetEnumDescription()) ||
                                  x.Media.Code.Equals(MediaEffort.CallR.GetEnumDescription())))
                     .Select(x => new
                     {
                         x.Media.Cd,
                         x.Media.Code,
                         x.Media.Spec,
                         x.Media.Name,
                         x.Media.BranchCode,
                     })
                    .AsEnumerable();
            if (query.Any())
            {
                rs = query.Select(y => new PlanMediaModel
                    {
                        MediaCd = y.Cd,
                        Spec = y.Spec,
                        MediaCode = y.Code,
                        MediaName = y.Name,
                        MediaBranch = y.BranchCode
                    })
                    .OrderBy(y => y.MediaCode).ThenBy(y => y.MediaBranch)
                    .ToList();
            }
            return rs;
        }

        public List<SelectListItem> GetListArtist()
        {
            var rs = _context.MstArtists.Where(x => !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Cd,
                        x.Code,
                        x.Name,
                    })
                    .AsEnumerable()
                    .Select(y => new SelectListItem
                    {
                        Value = y.Cd.ToString(),
                        Text = y.Name,

                    })
                    .OrderBy(y => y.Value)
                    .ToList();
            return rs;
        }

        public List<SelectListItem> GetLayout()
        {
            var layoutCheck = CommonExtension.ToEnumSelectList<LayoutCheck>()
                              .Select(d => new SelectListItem
                              {
                                  Value = d.Value,
                                  Text = d.Name,
                              }).ToList();
            return layoutCheck;
        }

        public AjaxResponseResultModel<int> AddMap(MapModel model, string userId)
        {
            var resultModel = new AjaxResponseResultModel<int>(false, null, 0);
            try
            {
                //Add Map
                var entityMap = new Map
                {
                    MonthWeek = model.EventPeriod,
                    BreakEvenPoint = model.BreakEventPoint,
                    CallArea = model.CallArea,
                    ClubMemberJoin = model.ClubMemberJoin,
                    ClubMemberTarget = model.ClubMemberTarget,
                    Decoration = model.Decoration,
                    EffortResult = model.EffortResult,
                    EffortTargetAmount = model.EffortTargetAmount,
                    EffortTargetRevenue = model.EffortTargetRevenue,
                    EventCd = model.EventCd,
                    EventTargetAmount = model.EventTargetAmount,
                    EventTargetRevenue = model.EventTargetRevenue,
                    SaleResult = model.SaleResult,
                    IsLayoutChecked = model.LayoutCheck.HasValue ? (model.LayoutCheck.Value == (int)LayoutCheck.Done ? true : false) : (bool?)null,
                    IsLayoutSubmitted = model.LayoutSubmit.HasValue ? (model.LayoutSubmit.Value == (int)LayoutCheck.Done ? true : false) : (bool?)null,
                    MarginalProfitRatio = model.MarginalProfitRatio,
                    NewResult = model.NewResult,
                    NewTargetAmount = model.NewTargetAmount,
                    NewTargetRevenue = model.NewTargetRevenue,
                    Plan = model.Plan,
                    Precaution = model.Precaution,
                    ProductExAmount = model.ProductExAmount,
                    ProductExDeadline = model.ProductExDeadline,
                    ProductExResponsible = model.ProductExResponsible,
                    ProductIdea = model.ProductIdea,
                    Profit = model.Profit,
                    Progress = model.Progress,
                    StrategiesAndTactics = model.StrategiesAndTactics,
                    TransportExpense = model.TransportExpense,
                    VenueAnalysis = model.VenueAnalysis,
                    PlanMedias = new List<PlanMedia>(),
                    CustomerInfos = new List<Entity.Schemas.CustomerInfo>(),
                    BusinessHopes = new List<BusinessHope>(),
                    EventRequests = new List<EventRequest>(),
                    SalesmanTargets = new List<SalesmanTarget>(),
                    Rankings = new List<Ranking>(),
                    InsertUserId = userId,
                    InsertDate = DateTime.Now,
                };
                foreach (var media in model.PlanMediaNews)
                {
                    if(media.MediaCd > 0)
                    {
                        var entityPlanMedia = new PlanMedia
                        {
                            EventCd = model.EventCd,
                            MediaCd = media.MediaCd,
                            Spec = media.Spec,
                            OrderAmount = media.OrderAmount,
                            Unit = media.OrderUnit,
                            Cost = media.Cost,
                            NumberOfCustomers = media.NumberOfCustomers,
                            EstimatedRevenue = media.EstimatedRevenue,
                            AttractCustomers = media.AttractCustomers,
                            ResultAmount = media.ResultAmount,
                            ResultRevenue = media.ResultRevenue,
                            InsertDate = DateTime.Now,
                            InsertUserId = userId
                        };
                        entityMap.PlanMedias.Add(entityPlanMedia);
                    }
                }
                foreach (var media in model.PlanMediaEfforts)
                {
                    if(media.MediaCd > 0)
                    {
                        var entityPlanMedia = new PlanMedia
                        {
                            EventCd = model.EventCd,
                            MediaCd = media.MediaCd,
                            Spec = media.Spec,
                            OrderAmount = media.OrderAmount,
                            Unit = media.OrderUnit,
                            Cost = media.Cost,
                            NumberOfCustomers = media.NumberOfCustomers,
                            EstimatedRevenue = media.EstimatedRevenue,
                            AttractCustomers = media.AttractCustomers,
                            ResultAmount = media.ResultAmount,
                            ResultRevenue = media.ResultRevenue,
                            InsertDate = DateTime.Now,
                            InsertUserId = userId
                        };
                        entityMap.PlanMedias.Add(entityPlanMedia);
                    }
                }
                //Event Request
                foreach (var request in model.EventRequests)
                {
                    if (!string.IsNullOrEmpty(request.Name) || !string.IsNullOrEmpty(request.Unit))
                    {
                        var entityRequest = new EventRequest
                        {
                            EventCd = model.EventCd,
                            Name = request.Name,
                            Unit = request.Unit,
                            InsertDate = DateTime.Now,
                            InsertUserId = userId
                        };
                        entityMap.EventRequests.Add(entityRequest);
                    }
                }
                //Bussines Hope
                foreach (var business in model.BusinessHopes)
                {
                    if (business.ArtistCd.HasValue)
                    {
                        var entityBusinesHope = new BusinessHope
                        {
                            ArtistCd = business.ArtistCd,
                            Desgin = business.Desgin,
                            EventCd = model.EventCd,
                            Type = business.Type,
                            Remark = business.Remark,
                            DesiredNumber = business.DesiredNumber,
                            InsertDate = DateTime.Now,
                            InsertUserId = userId
                        };
                        entityMap.BusinessHopes.Add(entityBusinesHope);
                    }
                }
                //Sale man tartget
                foreach (var saleMan in model.SaleManTargets)
                {
                    var entitySaleMan = new SalesmanTarget
                    {
                        EventCd = model.EventCd,
                        SalesmanCd = saleMan.SaleManCd,
                        ResultEffortAmount = saleMan.ResultEffortAmount,
                        ResultEffortEvenue = saleMan.ResultEffortEvenue,
                        EffortTargetAmount = saleMan.EffortTargetAmount,
                        EffortTargetEvenue = saleMan.EffortTargetEvenue,
                        ResultAmount = saleMan.ResultAmount,
                        ResultEvenue = saleMan.ResultEvenue,
                        TargetAmount = saleMan.TargetAmount,
                        TargetEvenue = saleMan.TargetEvenue,
                        CertainTargetAmount = saleMan.CertainTargetAmount,
                        ResultCertainAmount = saleMan.ResultCertainAmount,
                        InsertDate = DateTime.Now,
                        InsertUserId = userId,
                    };
                    entityMap.SalesmanTargets.Add(entitySaleMan);
                }
                //Update Assign SaleMan Event
                var dataSalemans = _context.EventSalesAssigments.Where(x => x.EventCd == model.EventCd).ToList();

                if (dataSalemans.Any())
                {
                    foreach (var saleMan in dataSalemans)
                    {
                        foreach (var editSaleMan in model.SaleManTargets)
                        {
                            if (saleMan.SalesmanCd == editSaleMan.SaleManCd)
                            {
                                saleMan.NewTargetAmount = editSaleMan.TargetAmount;
                                saleMan.NewTargetRevenue = editSaleMan.TargetEvenue;
                                saleMan.EffortTargetAmount = editSaleMan.EffortTargetAmount;
                                saleMan.EffortTargetRevenue = editSaleMan.EffortTargetEvenue;
                                saleMan.UpdateDate = DateTime.Now;
                                saleMan.UpdateUserId = userId;
                            }
                        }
                    }
                    _context.EventSalesAssigments.UpdateRange(dataSalemans);
                }
                //Customer Infor
                var entityCustomer = new Entity.Schemas.CustomerInfo
                {
                    Card = model.CustomerInfo.Card,
                    Cash = model.CustomerInfo.Cash,
                    Credit = model.CustomerInfo.Credit,
                    FisrtTime = model.CustomerInfo.FirstTime,
                    SecondTime = model.CustomerInfo.SecondTime,
                    ManyTime = model.CustomerInfo.ManyTime,
                    U20Female = model.CustomerInfo.U20FeMale,
                    U30Female = model.CustomerInfo.U30FeMale,
                    U40Female = model.CustomerInfo.U40FeMale,
                    U50Female = model.CustomerInfo.U50FeMale,
                    U60Female = model.CustomerInfo.U60FeMale,
                    U70Female = model.CustomerInfo.U70FeMale,
                    U80Female = model.CustomerInfo.U80FeMale,
                    U20Male = model.CustomerInfo.U20Male,
                    U30Male = model.CustomerInfo.U30Male,
                    U40Male = model.CustomerInfo.U40Male,
                    U50Male = model.CustomerInfo.U50Male,
                    U60Male = model.CustomerInfo.U60Male,
                    U70Male = model.CustomerInfo.U70Male,
                    U80Male = model.CustomerInfo.U80Male,
                    EventCd = model.EventCd,
                    InsertDate = DateTime.Now,
                    InsertUserId = userId,
                };
                entityMap.CustomerInfos.Add(entityCustomer);
                //Rankings
                foreach (var rank in model.Rankings)
                {
                    if(rank.Rank != null)
                    {
                        var entityRank = new Ranking
                        {
                            ArtistCd = rank.ArtistCd,
                            ArtistName = rank.ArtistName,
                            Rank = rank.Rank,
                            ProductCd = rank.ProductCd,
                            ProductName = rank.ProductName,
                            Amount = rank.Amount,
                            EventCd = model.EventCd,
                            InsertDate = DateTime.Now,
                            InsertUserId = userId,
                        };
                        entityMap.Rankings.Add(entityRank);
                    }
                }
                _context.Maps.Add(entityMap);
                _context.SaveChanges();
                resultModel.Status = true;
                resultModel.Message = "追加に成功しました。";
                resultModel.Result = entityMap.Cd;
            }
            catch (Exception e)
            {
                resultModel.Status = false;
                resultModel.Message = e.Message;
            }
            return resultModel;
        }

        public AjaxResponseResultModel<int> UpdateMap(MapModel model, string userId)
        {
            var resultModel = new AjaxResponseResultModel<int>(false, null, 0);
            try
            {
                var map = _context.Maps
                            .Include(x => x.PlanMedias)
                            .Include(x => x.EventRequests)
                            .Include(x => x.BusinessHopes)
                            .Include(x => x.CustomerInfos)
                            .Include(x => x.SalesmanTargets)
                            .Include(x => x.Rankings)
                            .FirstOrDefault(x => x.Cd == model.Cd);
                if (map != null)
                {
                    //Map
                    map.MonthWeek = model.EventPeriod;
                    map.BreakEvenPoint = model.BreakEventPoint;
                    map.CallArea = model.CallArea;
                    map.ClubMemberJoin = model.ClubMemberJoin;
                    map.ClubMemberTarget = model.ClubMemberTarget;
                    map.Decoration = model.Decoration;
                    map.EffortResult = model.EffortResult;
                    map.EffortTargetAmount = model.EffortTargetAmount;
                    map.EffortTargetRevenue = model.EffortTargetRevenue;
                    map.EventCd = model.EventCd;
                    map.EventTargetAmount = model.EventTargetAmount;
                    map.EventTargetRevenue = model.EventTargetRevenue;
                    map.SaleResult = model.SaleResult;
                    map.IsLayoutChecked = model.LayoutCheck.HasValue ? (model.LayoutCheck.Value == (int)LayoutCheck.Done ? true : false) : (bool?)null;
                    map.IsLayoutSubmitted = model.LayoutSubmit.HasValue ? (model.LayoutSubmit.Value == (int)LayoutCheck.Done ? true : false) : (bool?)null;
                    map.MarginalProfitRatio = model.MarginalProfitRatio;
                    map.NewResult = model.NewResult;
                    map.NewTargetAmount = model.NewTargetAmount;
                    map.NewTargetRevenue = model.NewTargetRevenue;
                    map.Plan = model.Plan;
                    map.Precaution = model.Precaution;
                    map.ProductExAmount = model.ProductExAmount;
                    map.ProductExDeadline = model.ProductExDeadline;
                    map.ProductExResponsible = model.ProductExResponsible;
                    map.ProductIdea = model.ProductIdea;
                    map.Profit = model.Profit;
                    map.Progress = model.Progress;
                    map.StrategiesAndTactics = model.StrategiesAndTactics;
                    map.TransportExpense = model.TransportExpense;
                    map.VenueAnalysis = model.VenueAnalysis;
                    map.UpdateUserId = userId;
                    map.UpdateDate = DateTime.Now;
                    //PlanMedia
                    var planMedias = model.PlanMediaNews;
                    planMedias.AddRange(model.PlanMediaEfforts);
                    foreach (var media in planMedias)
                    {
                        var updateMedia = map.PlanMedias.FirstOrDefault(x => x.MediaCd == media.MediaCd);
                        if (updateMedia != null && updateMedia.Cd > 0 && media.MediaCd > 0)
                        {

                            updateMedia.Spec = media.Spec;
                            updateMedia.OrderAmount = media.OrderAmount;
                            updateMedia.Unit = media.OrderUnit;
                            updateMedia.Cost = media.Cost;
                            updateMedia.NumberOfCustomers = media.NumberOfCustomers;
                            updateMedia.EstimatedRevenue = media.EstimatedRevenue;
                            updateMedia.AttractCustomers = media.AttractCustomers;
                            updateMedia.ResultAmount = media.ResultAmount;
                            updateMedia.ResultRevenue = media.ResultRevenue;
                            updateMedia.UpdateDate = DateTime.Now;
                            updateMedia.UpdateUserId = userId;
                        }
                        else
                        {
                            if(media.MediaCd > 0)
                            {
                                var entityPlanMedia = new PlanMedia
                                {
                                    MapCd = model.Cd,
                                    EventCd = model.EventCd,
                                    MediaCd = media.MediaCd,
                                    Spec = media.Spec,
                                    OrderAmount = media.OrderAmount,
                                    Unit = media.OrderUnit,
                                    Cost = media.Cost,
                                    NumberOfCustomers = media.NumberOfCustomers,
                                    EstimatedRevenue = media.EstimatedRevenue,
                                    AttractCustomers = media.AttractCustomers,
                                    ResultAmount = media.ResultAmount,
                                    ResultRevenue = media.ResultRevenue,
                                    InsertDate = DateTime.Now,
                                    InsertUserId = userId
                                };
                                _context.PlanMedias.Add(entityPlanMedia);
                            }
                        }
                    }
                    //SalesMan Target
                    foreach (var saleMan in model.SaleManTargets)
                    {
                        var updateSaleMan = map.SalesmanTargets.FirstOrDefault(x => x.SalesmanCd == saleMan.SaleManCd);
                        if (updateSaleMan != null && updateSaleMan.Cd > 0)
                        {
                            updateSaleMan.ResultEffortAmount = saleMan.ResultEffortAmount;
                            updateSaleMan.ResultEffortEvenue = saleMan.ResultEffortEvenue;
                            updateSaleMan.EffortTargetAmount = saleMan.EffortTargetAmount;
                            updateSaleMan.EffortTargetEvenue = saleMan.EffortTargetEvenue;
                            updateSaleMan.ResultAmount = saleMan.ResultAmount;
                            updateSaleMan.ResultEvenue = saleMan.ResultEvenue;
                            updateSaleMan.TargetAmount = saleMan.TargetAmount;
                            updateSaleMan.TargetEvenue = saleMan.TargetEvenue;
                            updateSaleMan.CertainTargetAmount = saleMan.CertainTargetAmount;
                            updateSaleMan.ResultCertainAmount = saleMan.ResultCertainAmount;
                            updateSaleMan.UpdateDate = DateTime.Now;
                            updateSaleMan.UpdateUserId = userId;
                        }
                        else
                        {
                            var entitySaleMan = new SalesmanTarget
                            {
                                MapCd = model.Cd,
                                EventCd = model.EventCd,
                                SalesmanCd = saleMan.SaleManCd,
                                ResultEffortAmount = saleMan.ResultEffortAmount,
                                ResultEffortEvenue = saleMan.ResultEffortEvenue,
                                EffortTargetAmount = saleMan.EffortTargetAmount,
                                EffortTargetEvenue = saleMan.EffortTargetEvenue,
                                ResultAmount = saleMan.ResultAmount,
                                ResultEvenue = saleMan.ResultEvenue,
                                TargetAmount = saleMan.TargetAmount,
                                TargetEvenue = saleMan.TargetEvenue,
                                CertainTargetAmount = saleMan.CertainTargetAmount,
                                ResultCertainAmount = saleMan.ResultCertainAmount,
                                InsertDate = DateTime.Now,
                                InsertUserId = userId,
                            };
                            map.SalesmanTargets.Add(entitySaleMan);
                        }
                    }
                    //Update Assign SaleMan Event
                    var dataSalemans = _context.EventSalesAssigments.Where(x => x.EventCd == model.EventCd).ToList();
                    if (dataSalemans.Any())
                    {
                        foreach (var editSaleMan in model.SaleManTargets)
                        {
                            var saleMan = dataSalemans.FirstOrDefault(x => x.SalesmanCd == editSaleMan.SaleManCd);
                            if (saleMan != null)
                            {
                                saleMan.NewTargetAmount = editSaleMan.TargetAmount;
                                saleMan.NewTargetRevenue = editSaleMan.TargetEvenue;
                                saleMan.EffortTargetAmount = editSaleMan.EffortTargetAmount;
                                saleMan.EffortTargetRevenue = editSaleMan.EffortTargetEvenue;
                                saleMan.UpdateDate = DateTime.Now;
                                saleMan.UpdateUserId = userId;
                            }
                        }
                    }
                    _context.EventSalesAssigments.UpdateRange(dataSalemans);
                    //Event Request
                    foreach (var request in model.EventRequests)
                    {
                        var updateRequest = map.EventRequests.FirstOrDefault(x => x.Cd == request.Cd);
                        if (updateRequest != null && updateRequest.Cd > 0)
                        {
                            if (string.IsNullOrEmpty(request.Unit) && string.IsNullOrEmpty(request.Name))
                            {
                                _context.EventRequests.Remove(updateRequest);
                            }
                            else
                            {
                                updateRequest.Name = request.Name;
                                updateRequest.Unit = request.Unit;
                                updateRequest.UpdateDate = DateTime.Now;
                                updateRequest.UpdateUserId = userId;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(request.Unit) || !string.IsNullOrEmpty(request.Name))
                            {
                                var entityRequest = new EventRequest
                                {
                                    MapCd = model.Cd,
                                    EventCd = model.EventCd,
                                    Name = request.Name,
                                    Unit = request.Unit,
                                    InsertDate = DateTime.Now,
                                    InsertUserId = userId
                                };
                                map.EventRequests.Add(entityRequest);
                            }

                        }
                    }
                    // BusinessHope
                    foreach (var business in model.BusinessHopes)
                    {
                        var updateBusiness = map.BusinessHopes.FirstOrDefault(x => x.Cd == business.Cd);
                        if (updateBusiness != null && updateBusiness.Cd > 0)
                        {
                            if ((!business.ArtistCd.HasValue || business.ArtistCd == 0))
                            {
                                _context.BusinessHopes.Remove(updateBusiness);
                            }
                            else
                            {
                                updateBusiness.ArtistCd = business.ArtistCd;
                                updateBusiness.Desgin = business.Desgin;
                                updateBusiness.Type = business.Type;
                                updateBusiness.Remark = business.Remark;
                                updateBusiness.DesiredNumber = business.DesiredNumber;
                                updateBusiness.UpdateDate = DateTime.Now;
                                updateBusiness.UpdateUserId = userId;
                            }
                        }
                        else
                        {
                            if (business.ArtistCd != null && business.ArtistCd > 0)
                            {
                                var entityBusinesHope = new BusinessHope
                                {
                                    MapCd = model.Cd,
                                    ArtistCd = business.ArtistCd,
                                    Desgin = business.Desgin,
                                    EventCd = model.EventCd,
                                    Type = business.Type,
                                    Remark = business.Remark,
                                    DesiredNumber = business.DesiredNumber,
                                    InsertDate = DateTime.Now,
                                    InsertUserId = userId
                                };
                                map.BusinessHopes.Add(entityBusinesHope);
                            }

                        }
                    }
                    //CustomerInfo
                    var updateCustomerInfo = map.CustomerInfos.FirstOrDefault();
                    if (updateCustomerInfo != null)
                    {
                        updateCustomerInfo.Card = model.CustomerInfo.Card;
                        updateCustomerInfo.Cash = model.CustomerInfo.Cash;
                        updateCustomerInfo.Credit = model.CustomerInfo.Credit;
                        updateCustomerInfo.FisrtTime = model.CustomerInfo.FirstTime;
                        updateCustomerInfo.SecondTime = model.CustomerInfo.SecondTime;
                        updateCustomerInfo.ManyTime = model.CustomerInfo.ManyTime;
                        updateCustomerInfo.U20Female = model.CustomerInfo.U20FeMale;
                        updateCustomerInfo.U30Female = model.CustomerInfo.U30FeMale;
                        updateCustomerInfo.U40Female = model.CustomerInfo.U40FeMale;
                        updateCustomerInfo.U50Female = model.CustomerInfo.U50FeMale;
                        updateCustomerInfo.U60Female = model.CustomerInfo.U60FeMale;
                        updateCustomerInfo.U70Female = model.CustomerInfo.U70FeMale;
                        updateCustomerInfo.U80Female = model.CustomerInfo.U80FeMale;
                        updateCustomerInfo.U20Male = model.CustomerInfo.U20Male;
                        updateCustomerInfo.U30Male = model.CustomerInfo.U30Male;
                        updateCustomerInfo.U40Male = model.CustomerInfo.U40Male;
                        updateCustomerInfo.U50Male = model.CustomerInfo.U50Male;
                        updateCustomerInfo.U60Male = model.CustomerInfo.U60Male;
                        updateCustomerInfo.U70Male = model.CustomerInfo.U70Male;
                        updateCustomerInfo.U80Male = model.CustomerInfo.U80Male;
                        updateCustomerInfo.UpdateDate = DateTime.Now;
                        updateCustomerInfo.UpdateUserId = userId;
                    }
                    // Rankings
                    // Remove old rankings
                    var oldRankings = map.Rankings.Where(x => x.MapCd == model.Cd).ToList();
                    if (oldRankings.Any())
                    {
                        _context.Rankings.RemoveRange(oldRankings);
                    }
                    //Add new rankings
                    foreach (var rank in model.Rankings)
                    {
                        if(rank.Rank != null)
                        {
                            var entityRank = new Ranking
                            {
                                MapCd = model.Cd,
                                ArtistCd = rank.ArtistCd,
                                ArtistName = rank.ArtistName,
                                Rank = rank.Rank,
                                ProductCd = rank.ProductCd,
                                ProductName = rank.ProductName,
                                EventCd = model.EventCd,
                                Amount = rank.Amount,
                                InsertDate = DateTime.Now,
                                InsertUserId = userId,
                            };
                            _context.Rankings.Add(entityRank);
                        }
                    }
                    _context.Maps.Update(map);
                    _context.SaveChanges();
                    resultModel.Status = true;
                    resultModel.Message = "編集に成功しました。";
                    resultModel.Result = model.Cd;
                }
            }
            catch (Exception e)
            {
                resultModel.Status = false;
                resultModel.Message = e.Message;
            }
            return resultModel;
        }
        public MapPrint GetInfoPrintMap(int MapCd)
        {
            var rs = new MapPrint();
            var queryMap = _context.Maps.Where(x => x.Cd == MapCd && !x.IsDeleted)
                        .Include(x => x.Event)
                        .Select(x => new
                        {
                            x.MonthWeek,
                            x.Event.Address,
                            x.Event.Place,
                            x.Event.StartTime,
                            x.Event.EndTime,
                            x.Event.StartDate,
                            x.Event.EndDate,
                            x.Event.MainSalesman.Name,
                            x.Plan,
                            x.Decoration,
                            x.TransportExpense,
                            x.Precaution,
                            x.EventTargetAmount,
                            x.EventTargetRevenue,
                            x.NewTargetAmount,
                            x.NewTargetRevenue,
                            x.EffortTargetAmount,
                            x.EffortTargetRevenue,
                            x.BreakEvenPoint,
                            x.SaleResult,
                            x.NewResult,
                            x.EffortResult,
                            x.Profit,
                            x.MarginalProfitRatio,
                            x.IsLayoutChecked,
                            x.IsLayoutSubmitted,
                            x.ClubMemberJoin,
                            x.ClubMemberTarget,
                            x.ProductExDeadline,
                            x.ProductExResponsible,
                            x.ProductExAmount,
                            x.CallArea,
                            x.Progress,
                            x.ProductIdea,
                            x.StrategiesAndTactics,
                            x.VenueAnalysis,
                        })
                        .AsEnumerable();

            if (queryMap.Any())
            {
                rs = queryMap.Select(x => new MapPrint
                {
                    EventDate = x.MonthWeek,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    EventSaleMan = x.Name,
                    EventPlan = x.Plan,
                    EventDecoration = x.Decoration,
                    EventPrecaution = x.Precaution,
                    EventTranportExpense = x.TransportExpense,
                    EventAddress = x.Address,
                    EventPlace = x.Place,
                    EventRemark = x.VenueAnalysis,
                    EventTargetAmount = x.EventTargetAmount.HasValue ? x.EventTargetAmount.Value.ToString("N0") : string.Empty,
                    EventTartgetRevenue = x.EventTargetRevenue.HasValue ? x.EventTargetRevenue.Value.ToString("N0") : string.Empty,
                    EffortTargetAmount = x.EffortTargetAmount.HasValue ? x.EffortTargetAmount.Value.ToString("N0") : string.Empty,
                    EffortTargetRevenue = x.EffortTargetRevenue.HasValue ? x.EffortTargetRevenue.Value.ToString("N0") : string.Empty,
                    NewTargetAmount = x.NewTargetAmount.HasValue ? x.NewTargetAmount.Value.ToString("N0") : string.Empty,
                    NewTargetRevenue = x.NewTargetRevenue.HasValue ? x.NewTargetRevenue.Value.ToString("N0") : string.Empty,
                    BreakEventPoint = x.BreakEvenPoint.HasValue ? x.BreakEvenPoint.Value.ToString("N0") : string.Empty,
                    SaleResult = x.SaleResult.HasValue ? x.SaleResult.Value.ToString("N0") : string.Empty,
                    NewResult = x.NewResult.HasValue ? x.NewResult.Value.ToString("N0") : string.Empty,
                    EffortResult = x.EffortResult.HasValue ? x.EffortResult.Value.ToString("N0") : string.Empty,
                    Profit = x.Profit.HasValue ? x.Profit.Value.ToString("N0") : string.Empty,
                    MarginalProfitRatio = x.MarginalProfitRatio.HasValue ? Math.Round((decimal)x.MarginalProfitRatio.Value / 100, 2).ToString() : string.Empty,
                    ClubMemberTarget = x.ClubMemberTarget.HasValue ? x.ClubMemberTarget.Value.ToString("N0") : string.Empty,
                    ClubMemberJoin = x.ClubMemberJoin.HasValue ? x.ClubMemberJoin.Value.ToString("N0") : string.Empty,
                    MemberRatio = x.ClubMemberTarget.HasValue && x.ClubMemberJoin.HasValue ? Math.Round((decimal)x.ClubMemberJoin.Value / (decimal)x.ClubMemberTarget.Value * 100, 0).ToString() : string.Empty,
                    LayoutCheck = x.IsLayoutChecked.HasValue ? (x.IsLayoutChecked.Value ? LayoutCheck.Done.GetEnumDescription() : LayoutCheck.NotDone.GetEnumDescription()) : string.Empty,
                    LayoutSubmit = x.IsLayoutSubmitted.HasValue ? (x.IsLayoutSubmitted.Value ? LayoutCheck.Done.GetEnumDescription() : LayoutCheck.NotDone.GetEnumDescription()) : string.Empty,
                    ProductExDeadLine = x.ProductExDeadline.HasValue ? x.ProductExDeadline.Value.ToString(ExactDateFormatJP) : string.Empty,
                    ProductExResponsible = x.ProductExResponsible,
                    ProductExAmount = x.ProductExAmount.HasValue ? x.ProductExAmount.Value.ToString("N0") : string.Empty,
                    CallArea = x.CallArea,
                    ProductIdea = x.ProductIdea,
                    Progress = x.Progress,
                    StrategiesAndTactics = x.StrategiesAndTactics,
                    SaleManTarget1s = new List<SaleManTargetPrint>(),
                    SaleManTarget2s = new List<SaleManTargetPrint>(),
                    PlanMediaNews = new List<PlanMediaPrint>(),
                    PlanMediaEfforts = new List<PlanMediaPrint>(),
                    CustomerInfo1s = new List<CustomerInfoPrint>(),
                    CustomerInfo2s = new List<CustomerInfoPrint>(),
                    EventRequests = new List<EventRequestPrint>(),
                    BusinessHopes = new List<BusinessHopesPrint>(),
                    Ranks = new List<RankingPrint>(),
                })
                .FirstOrDefault();
                //Print SaleMan
                var querySaleMan = _context.SalesmanTargets.Where(x => x.MapCd == MapCd)
                               .Include(x => x.Salesman)
                               .Select(x => new
                               {
                                   x.SalesmanCd,
                                   x.Salesman.Name,
                                   x.TargetAmount,
                                   x.TargetEvenue,
                                   x.ResultAmount,
                                   x.ResultEvenue,
                                   x.EffortTargetAmount,
                                   x.EffortTargetEvenue,
                                   x.ResultEffortAmount,
                                   x.ResultEffortEvenue,
                                   x.CertainTargetAmount,
                                   x.ResultCertainAmount
                               })
                               .AsEnumerable();
                var lstSaleMan = new List<SaleManTargetPrint>();
                if (querySaleMan.Any())
                {
                    lstSaleMan = querySaleMan.OrderBy(x => x.SalesmanCd).Select(x => new SaleManTargetPrint
                    {
                        SaleManCd = x.SalesmanCd.ToString(),
                        SaleManName = x.Name,
                        TargetAmount = x.TargetAmount.HasValue ? x.TargetAmount.Value.ToString("N0") : string.Empty,
                        TargetRevenue = x.TargetEvenue.HasValue ? x.TargetEvenue.Value.ToString("N0") : string.Empty,
                        ResultAmount = x.ResultAmount.HasValue ? x.ResultAmount.Value.ToString("N0") : "0",
                        ResultRevenue = x.ResultEvenue.HasValue ? Commons.RoundRevenue(x.ResultEvenue.Value, 4).ToString("N0") : "0",
                        TargetEffortAmount = x.EffortTargetAmount.HasValue ? x.EffortTargetAmount.Value.ToString("N0") : string.Empty,
                        TargetEffortRevenue = x.EffortTargetEvenue.HasValue ? x.EffortTargetEvenue.Value.ToString("N0") : string.Empty,
                        ResultEffortAmount = x.ResultEffortAmount.HasValue ? x.ResultEffortAmount.Value.ToString("N0") : "0",
                        ResultEffortRevenue = x.ResultEffortEvenue.HasValue ? Commons.RoundRevenue(x.ResultEffortEvenue.Value, 4).ToString("N0") : "0",
                        CertainTarget = x.CertainTargetAmount.HasValue ? x.CertainTargetAmount.Value.ToString("N0") : string.Empty,
                        CertainResult = x.ResultCertainAmount.HasValue ? x.ResultCertainAmount.Value.ToString("N0") : string.Empty,
                    }).ToList();
                }
                var length = lstSaleMan.Count();
                var lstSaleMan1 = new List<SaleManTargetPrint>();
                var lstSaleMan2 = new List<SaleManTargetPrint>();
                if (length <= 13)
                {
                    for (int i = 0; i < 13 - length; i++)
                    {
                        lstSaleMan.Add(new SaleManTargetPrint());
                    }
                    for (int i = 0; i < 13; i++)
                    {
                        lstSaleMan2.Add(new SaleManTargetPrint());
                    }
                    lstSaleMan1 = lstSaleMan;
                }
                else
                {
                    for (int i = 0; i < 13; i++)
                    {
                        lstSaleMan1.Add(lstSaleMan[i]);
                    }
                    for (int i = 13; i <= length && i < length; i++)
                    {
                        lstSaleMan2.Add(lstSaleMan[i]);
                    }
                    for (int i = 0; i < 26 - length; i++)
                    {
                        lstSaleMan2.Add(new SaleManTargetPrint());
                    }
                }
                rs.SaleManTarget1s = lstSaleMan1;
                rs.SaleManTarget2s = lstSaleMan2;
                //Print Media
                var queryMedias = _context.PlanMedias
                                 .Include(x => x.Media)
                                 .Where(x => x.MapCd == MapCd && !x.Media.IsDeleted)
                                 .OrderBy(x => x.Media.Code).ThenBy(x => x.Media.BranchCode)
                                 .Select(x => new
                                 {
                                     x.MediaCd,
                                     x.Media.Name,
                                     x.Media.Code,
                                     x.Media.Spec,
                                     x.OrderAmount,
                                     x.Unit,
                                     x.Cost,
                                     x.NumberOfCustomers,
                                     x.EstimatedRevenue,
                                     x.AttractCustomers,
                                     x.ResultAmount,
                                     x.ResultRevenue
                                 })
                                 .AsEnumerable();
                var planMediaSales = new List<PlanMediaPrint>();
                var planMediaNews = new List<PlanMediaPrint>();
                var planMediaEfforts = new List<PlanMediaPrint>();
                if (queryMedias.Any())
                {
                    planMediaSales = queryMedias.Where(x => x.Code.Equals(MediaSale.SaleDM.GetEnumDescription()))
                         .Select(x => new PlanMediaPrint
                         {
                             MediaCode = x.Code,
                             MediaName = x.Name,
                             Spec = x.Spec,
                             OrderAmount = x.OrderAmount.HasValue ? x.OrderAmount.Value.ToString("N0") : string.Empty,
                             OrderUnit = x.Unit,
                             Cost = x.Cost.HasValue ? x.Cost.Value.ToString("N0") : string.Empty,
                             NumberOfCustomers = x.NumberOfCustomers.HasValue ? x.NumberOfCustomers.Value.ToString("N0") : string.Empty,
                             EstimatedRevenue = x.EstimatedRevenue.HasValue ? x.EstimatedRevenue.Value.ToString("N0") : string.Empty,
                             AttractCustomers = x.AttractCustomers.HasValue ? x.AttractCustomers.Value.ToString("N0") : "0",
                             ResultAmount = x.ResultAmount.HasValue ? x.ResultAmount.Value.ToString("N0") : "0",
                             ResultRevenue = x.ResultRevenue.HasValue ? Commons.RoundRevenue(x.ResultRevenue.Value, 4).ToString("N0") : "0",
                         }).ToList();

                    planMediaNews = queryMedias.Where(x => !x.Code.Equals(MediaSale.SaleDM.GetEnumDescription()) &&
                                                           !x.Code.Equals(MediaEffort.CallO.GetEnumDescription()) &&
                                                           !x.Code.Equals(MediaEffort.CallQ.GetEnumDescription()) &&
                                                           !x.Code.Equals(MediaEffort.CallR.GetEnumDescription()))
                                        .Select(x => new PlanMediaPrint
                                        {
                                            MediaCode = x.Code,
                                            MediaName = x.Name,
                                            Spec = x.Spec,
                                            OrderAmount = x.OrderAmount.HasValue ? x.OrderAmount.Value.ToString("N0") : string.Empty,
                                            OrderUnit = x.Unit,
                                            Cost = x.Cost.HasValue ? x.Cost.Value.ToString("N0") : string.Empty,
                                            NumberOfCustomers = x.NumberOfCustomers.HasValue ? x.NumberOfCustomers.Value.ToString("N0") : string.Empty,
                                            EstimatedRevenue = x.EstimatedRevenue.HasValue ? x.EstimatedRevenue.Value.ToString("N0") : string.Empty,
                                            AttractCustomers = x.AttractCustomers.HasValue ? x.AttractCustomers.Value.ToString("N0") : "0",
                                            ResultAmount = x.ResultAmount.HasValue ? x.ResultAmount.Value.ToString("N0") : "0",
                                            ResultRevenue = x.ResultRevenue.HasValue ? Commons.RoundRevenue(x.ResultRevenue.Value, 4).ToString("N0") : "0",
                                        }).ToList();

                    planMediaEfforts = queryMedias.Where(x => x.Code.Equals(MediaEffort.CallO.GetEnumDescription()) ||
                                                                  x.Code.Equals(MediaEffort.CallQ.GetEnumDescription()) ||
                                                                  x.Code.Equals(MediaEffort.CallR.GetEnumDescription()))
                                        .Select(x => new PlanMediaPrint
                                        {
                                            MediaCode = x.Code,
                                            MediaName = x.Name,
                                            Spec = x.Spec,
                                            OrderAmount = x.OrderAmount.HasValue ? x.OrderAmount.Value.ToString("N0") : string.Empty,
                                            OrderUnit = x.Unit,
                                            Cost = x.Cost.HasValue ? x.Cost.Value.ToString("N0") : string.Empty,
                                            NumberOfCustomers = x.NumberOfCustomers.HasValue ? x.NumberOfCustomers.Value.ToString("N0") : string.Empty,
                                            EstimatedRevenue = x.EstimatedRevenue.HasValue ? x.EstimatedRevenue.Value.ToString("N0") : string.Empty,
                                            AttractCustomers = x.AttractCustomers.HasValue ? x.AttractCustomers.Value.ToString("N0") : "0",
                                            ResultAmount = x.ResultAmount.HasValue ? x.ResultAmount.Value.ToString("N0") : "0",
                                            ResultRevenue = x.ResultRevenue.HasValue ? Commons.RoundRevenue(x.ResultRevenue.Value, 4).ToString("N0") : "0",
                                        }).ToList();

                }
                long resultAmount = 0;
                long resultNewAmount = 0;
                long resultEffortAmount = 0;
                long resultRevenue = 0;
                long resultNewRevenue = 0;
                long resultEffortRevenue = 0;
                long costNewsTotal = 0;
                long numCustomerNewsTotal = 0;
                long estimatedRevenueNewsTotal = 0;
                long attractCustomerNewsTotal = 0;
                long resultAmountNewsTotal = 0;
                long resultRevenueNewsTotal = 0;
                long costEffortsTotal = 0;
                long numCustomerEffortsTotal = 0;
                long estimatedRevenueEffortsTotal = 0;
                long attractCustomerEffortsTotal = 0;
                long resultAmountEffortsTotal = 0;
                long resultRevenueEffortsTotal = 0;
                long costTotal = 0;
                long numCustomerTotal = 0;
                long estimatedRevenueTotal = 0;
                long attractCustomerTotal = 0;
                long resultAmountTotal = 0;
                long resultRevenueTotal = 0;
                for (int i = 0; i < planMediaSales.Count(); i++)
                {
                    resultAmount = resultAmount + Commons.TryParseLong(planMediaSales[i].ResultAmount.Replace(",", ""));
                    resultRevenue = resultRevenue + Commons.TryParseLong(planMediaSales[i].ResultRevenue.Replace(",", ""));
                    costNewsTotal = costNewsTotal + Commons.TryParseLong(planMediaSales[i].Cost.Replace(",", ""));
                    numCustomerNewsTotal = numCustomerNewsTotal + Commons.TryParseLong(planMediaSales[i].NumberOfCustomers.Replace(",", ""));
                    estimatedRevenueNewsTotal = estimatedRevenueNewsTotal + Commons.TryParseLong(planMediaSales[i].EstimatedRevenue.Replace(",", ""));
                    attractCustomerNewsTotal = attractCustomerNewsTotal + Commons.TryParseLong(planMediaSales[i].AttractCustomers.Replace(",", ""));
                    resultAmountNewsTotal = resultAmountNewsTotal + Commons.TryParseLong(planMediaSales[i].ResultAmount.Replace(",", ""));
                    resultRevenueNewsTotal = resultRevenueNewsTotal + Commons.TryParseLong(planMediaSales[i].ResultRevenue.Replace(",", ""));
                }
                for(int i = 0; i < planMediaNews.Count(); i++)
                {
                    resultAmount = resultAmount + Commons.TryParseLong(planMediaNews[i].ResultAmount.Replace(",", ""));
                    resultRevenue = resultRevenue + Commons.TryParseLong(planMediaNews[i].ResultRevenue.Replace(",", ""));
                    resultNewAmount = resultNewAmount + Commons.TryParseLong(planMediaNews[i].ResultAmount.Replace(",", ""));
                    resultNewRevenue = resultNewRevenue + Commons.TryParseLong(planMediaNews[i].ResultRevenue.Replace(",", ""));
                    costNewsTotal = costNewsTotal + Commons.TryParseLong(planMediaNews[i].Cost.Replace(",", ""));
                    numCustomerNewsTotal = numCustomerNewsTotal + Commons.TryParseLong(planMediaNews[i].NumberOfCustomers.Replace(",", ""));
                    estimatedRevenueNewsTotal = estimatedRevenueNewsTotal + Commons.TryParseLong(planMediaNews[i].EstimatedRevenue.Replace(",", ""));
                    attractCustomerNewsTotal = attractCustomerNewsTotal + Commons.TryParseLong(planMediaNews[i].AttractCustomers.Replace(",", ""));
                    resultAmountNewsTotal = resultAmountNewsTotal + Commons.TryParseLong(planMediaNews[i].ResultAmount.Replace(",", ""));
                    resultRevenueNewsTotal = resultRevenueNewsTotal + Commons.TryParseLong(planMediaNews[i].ResultRevenue.Replace(",", ""));
                }
                for(int i = 0; i < planMediaEfforts.Count(); i++)
                {
                    resultAmount = resultAmount + Commons.TryParseLong(planMediaEfforts[i].ResultAmount.Replace(",", ""));
                    resultRevenue = resultRevenue + Commons.TryParseLong(planMediaEfforts[i].ResultRevenue.Replace(",", ""));
                    resultEffortAmount = resultEffortAmount + Commons.TryParseLong(planMediaEfforts[i].ResultAmount.Replace(",", ""));
                    resultEffortRevenue = resultEffortRevenue + Commons.TryParseLong(planMediaEfforts[i].ResultRevenue.Replace(",", ""));
                    costEffortsTotal = costEffortsTotal + Commons.TryParseLong(planMediaEfforts[i].Cost.Replace(",", ""));
                    numCustomerEffortsTotal = numCustomerEffortsTotal + Commons.TryParseLong(planMediaEfforts[i].NumberOfCustomers.Replace(",", ""));
                    estimatedRevenueEffortsTotal = estimatedRevenueEffortsTotal + Commons.TryParseLong(planMediaEfforts[i].EstimatedRevenue.Replace(",", ""));
                    attractCustomerEffortsTotal = attractCustomerEffortsTotal + Commons.TryParseLong(planMediaEfforts[i].AttractCustomers.Replace(",", ""));
                    resultAmountEffortsTotal = resultAmountEffortsTotal + Commons.TryParseLong(planMediaEfforts[i].ResultAmount.Replace(",", ""));
                    resultRevenueEffortsTotal = resultRevenueEffortsTotal + Commons.TryParseLong(planMediaEfforts[i].ResultRevenue.Replace(",", ""));
                }
                costTotal = costNewsTotal + costEffortsTotal;
                numCustomerTotal = numCustomerNewsTotal + numCustomerEffortsTotal;
                estimatedRevenueTotal = estimatedRevenueNewsTotal + estimatedRevenueEffortsTotal;
                attractCustomerTotal = attractCustomerNewsTotal + attractCustomerEffortsTotal;
                resultAmountTotal = resultAmountNewsTotal + resultAmountEffortsTotal;
                resultRevenueTotal = resultRevenueNewsTotal + resultRevenueEffortsTotal;

                planMediaSales.AddRange(planMediaNews);
                var lengthNews = planMediaSales.Count();
                var lengthEfforts = planMediaEfforts.Count();
                if(lengthNews <= 18)
                {
                    for(int i = 0; i < 18 - lengthNews; i++)
                    {
                        planMediaSales.Add(new PlanMediaPrint());
                    }
                }
                if(lengthEfforts <= 4)
                {
                    for (int i = 0; i < 4 - lengthEfforts; i++)
                    {
                        planMediaEfforts.Add(new PlanMediaPrint());
                    }
                }
                rs.SaleResultAmount = resultAmount.ToString("N0");
                rs.NewResultAmount = resultNewAmount.ToString("N0");
                rs.EffortResultAmount = resultEffortAmount.ToString("N0");
                rs.PlanMediaNews = planMediaSales;
                rs.PlanMediaEfforts = planMediaEfforts;
                rs.CostNewsTotal = costNewsTotal.ToString("N0");
                rs.NumberCustomerNewsTotal = numCustomerNewsTotal.ToString("N0");
                rs.EstimateNewsTotal = estimatedRevenueNewsTotal.ToString("N0");
                rs.ActractNewsTotal = attractCustomerNewsTotal.ToString("N0");
                rs.ResultAmountNewsTotal = resultAmountNewsTotal.ToString("N0");
                rs.ResultRevenueNewsTotal = resultRevenueNewsTotal.ToString("N0");
                rs.CostEffortsTotal = costEffortsTotal.ToString("N0");
                rs.NumberCustomerEffortsTotal = numCustomerEffortsTotal.ToString("N0");
                rs.EstimateEffortsTotal = estimatedRevenueEffortsTotal.ToString("N0");
                rs.ActractEffortsTotal = attractCustomerEffortsTotal.ToString("N0");
                rs.ResultAmountEffortsTotal = resultAmountEffortsTotal.ToString("N0");
                rs.ResultRevenueEffortsTotal = resultRevenueEffortsTotal.ToString("N0");
                rs.CostTotal = costTotal.ToString("N0");
                rs.NumberCustomerTotal = numCustomerTotal.ToString("N0");
                rs.EstimateTotal = estimatedRevenueTotal.ToString("N0");
                rs.ActractTotal = attractCustomerTotal.ToString("N0");
                rs.ResultAmountTotal = resultAmountTotal.ToString("N0");
                rs.ResultRevenueTotal = resultRevenueTotal.ToString("N0");
                //EventRequest
                var queryEventRequest = _context.EventRequests.Where(x => x.MapCd == MapCd)
                                        .Select(x => new
                                        {
                                            x.Name,
                                            x.Unit
                                        })
                                        .AsEnumerable();
                var lstEventRequest = new List<EventRequestPrint>();
                if (queryEventRequest.Any())
                {
                    lstEventRequest = queryEventRequest.Select(x => new EventRequestPrint
                    {
                        Name = x.Name,
                        Unit = x.Unit
                    }).ToList();
                }
                var lengthEventRequest = lstEventRequest.Count();
                if(lengthEventRequest <= 9)
                {
                    for(int i = 0; i < 9 - lengthEventRequest; i++)
                    {
                        lstEventRequest.Add(new EventRequestPrint());
                    }
                }
                rs.EventRequests = lstEventRequest;
                //BusinessHope
                var queryBusinessHope = _context.BusinessHopes.Where(x => x.MapCd == MapCd)
                                        .Include(x => x.Artist)
                                        .Select(x => new
                                        {
                                            x.ArtistCd,
                                            x.Artist.Name,
                                            x.Type,
                                            x.Desgin,
                                            x.DesiredNumber,
                                            x.Remark,
                                        })
                                        .AsEnumerable();
                var lstBusinessHope = new List<BusinessHopesPrint>();
                if (queryBusinessHope.Any())
                {
                    lstBusinessHope = queryBusinessHope.Select(x => new BusinessHopesPrint
                    {
                        ArtistName = x.Name,
                        Type = x.Type,
                        Design = x.Desgin,
                        DesiredNumber = x.DesiredNumber.HasValue ? x.DesiredNumber.Value.ToString("N0") : string.Empty,
                        Remark = x.Remark,
                    }).ToList();
                }
                var lengthBusinessHope = lstBusinessHope.Count();
                if(lengthBusinessHope <= 7)
                {
                    for(int i = 0; i < 7 - lengthBusinessHope; i++)
                    {
                        lstBusinessHope.Add(new BusinessHopesPrint());
                    }
                }
                rs.BusinessHopes = lstBusinessHope;
                //CustommerInfo
                var queryCustomerInfor = _context.CustomerInfos.Where(x => x.MapCd == MapCd)
                                         .AsEnumerable();
                var customerInfo = new CustomerInfoModel();
                if (queryCustomerInfor.Any())
                {
                    customerInfo = queryCustomerInfor.Select(x => new CustomerInfoModel
                    {
                        Card = x.Card.HasValue ? x.Card.Value : 0,
                        Credit = x.Credit.HasValue ? x.Credit.Value : 0,
                        Cash = x.Cash.HasValue ? x.Cash.Value : 0,
                        FirstTime = x.FisrtTime.HasValue ? x.FisrtTime.Value : 0,
                        SecondTime = x.SecondTime.HasValue ? x.SecondTime.Value : 0,
                        ManyTime = x.ManyTime.HasValue ? x.ManyTime.Value : 0,
                        U20FeMale = x.U20Female.HasValue ? x.U20Female.Value : 0,
                        U30FeMale = x.U30Female.HasValue ? x.U30Female.Value : 0,
                        U40FeMale = x.U40Female.HasValue ? x.U40Female.Value : 0,
                        U50FeMale = x.U50Female.HasValue ? x.U50Female.Value : 0,
                        U60FeMale = x.U60Female.HasValue ? x.U60Female.Value : 0,
                        U70FeMale = x.U70Female.HasValue ? x.U70Female.Value : 0,
                        U80FeMale = x.U80Female.HasValue ? x.U80Female.Value : 0,
                        U20Male = x.U20Male.HasValue ? x.U20Male.Value : 0,
                        U30Male = x.U30Male.HasValue ? x.U30Male.Value : 0,
                        U40Male = x.U40Male.HasValue ? x.U40Male.Value : 0,
                        U50Male = x.U50Male.HasValue ? x.U50Male.Value : 0,
                        U60Male = x.U60Male.HasValue ? x.U60Male.Value : 0,
                        U70Male = x.U70Male.HasValue ? x.U70Male.Value : 0,
                        U80Male = x.U80Male.HasValue ? x.U80Male.Value : 0,
                    }).FirstOrDefault();
                }
                var customerInfoPrint1 = new List<CustomerInfoPrint> 
                {
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "信販",
                        NameInfo2 = "初見",
                        ResultInfo1 = customerInfo.Credit.ToString("N0"),
                        ResultInfo2 = customerInfo.FirstTime.ToString("N0"),
                    },
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "現金",
                        NameInfo2 = "二回目",
                        ResultInfo1 = customerInfo.Cash.ToString("N0"),
                        ResultInfo2 = customerInfo.SecondTime.ToString("N0"),
                    },
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "カード",
                        NameInfo2 = "複数回",
                        ResultInfo1 = customerInfo.Card.ToString("N0"),
                        ResultInfo2 = customerInfo.ManyTime.ToString("N0"),
                    }
                };
                var customerInfoPrint2 = new List<CustomerInfoPrint>
                {
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "20代男性",
                        NameInfo2 = "20代女性",
                        ResultInfo1 = customerInfo.U20Male.ToString("N0"),
                        ResultInfo2 = customerInfo.U20FeMale.ToString("N0"),
                    },
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "30代男性",
                        NameInfo2 = "30代女性",
                        ResultInfo1 = customerInfo.U30Male.ToString("N0"),
                        ResultInfo2 = customerInfo.U30FeMale.ToString("N0"),
                    },
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "40代男性",
                        NameInfo2 = "40代女性",
                        ResultInfo1 = customerInfo.U40Male.ToString("N0"),
                        ResultInfo2 = customerInfo.U40FeMale.ToString("N0"),
                    },
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "50代男性",
                        NameInfo2 = "50代女性",
                        ResultInfo1 = customerInfo.U50Male.ToString("N0"),
                        ResultInfo2 = customerInfo.U50FeMale.ToString("N0"),
                    },
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "60代男性",
                        NameInfo2 = "60代女性",
                        ResultInfo1 = customerInfo.U60Male.ToString("N0"),
                        ResultInfo2 = customerInfo.U60FeMale.ToString("N0"),
                    },
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "70代男性",
                        NameInfo2 = "70代女性",
                        ResultInfo1 = customerInfo.U70Male.ToString("N0"),
                        ResultInfo2 = customerInfo.U70FeMale.ToString("N0"),
                    },
                    new CustomerInfoPrint
                    {
                        NameInfo1 = "80代以上男性",
                        NameInfo2 = "80代以上女性",
                        ResultInfo1 = customerInfo.U80Male.ToString("N0"),
                        ResultInfo2 = customerInfo.U80FeMale.ToString("N0"),
                    },
                };
                rs.CustomerInfo1s = customerInfoPrint1;
                rs.CustomerInfo2s = customerInfoPrint2;
                rs.CountFeMale = customerInfo.CountFeMale.ToString("N0");
                rs.CountMale = customerInfo.CountMale.ToString("N0");
                //Ranking
                var queryRanking = _context.Rankings.Where(x => x.MapCd == MapCd)
                    .Select(x => new
                    {
                        x.Rank,
                        x.ArtistName,
                        x.ProductName,
                        x.Amount
                    })
                    .AsEnumerable();
                var lstRank = new List<RankingPrint>();
                if (queryRanking.Any())
                {
                    lstRank = queryRanking.Select(x => new RankingPrint
                    {
                        Rank = x.Rank,
                        ArtistName = x.Rank.HasValue ? (x.Rank.Value.ToString() + "位: " + x.ArtistName) : x.ArtistName,
                        Product = x.ProductName,
                        Amount = x.Amount.HasValue ? x.Amount.Value.ToString("N0") : "",
                    })
                    .OrderBy(x => x.Rank)
                    .ToList();
                }
                var lengthRanking = lstRank.Count();
                if(lengthRanking < 7)
                {
                    for (int i = 0; i < 7 - lengthRanking; i++)
                    {
                        lstRank.Add(new RankingPrint());
                    }
                }
                rs.Ranks = lstRank;
            }
            else
            {
                return null;
            }
            return rs;
        }
    }
}
