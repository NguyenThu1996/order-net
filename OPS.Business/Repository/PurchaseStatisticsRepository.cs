using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.PurchaseStatistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OPS.Business.Repository
{
    public class PurchaseStatisticsRepository : IPurchaseStatisticsRepository
    {
        private readonly OpsContext _context;

        public PurchaseStatisticsRepository(OpsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Loading purchase statistics A and B data to DataTable
        /// </summary>
        /// <param name="filtering">EventCd and Type</param>
        /// <returns>IndexViewModel</returns>
        public IndexViewModel Load(IndexViewModel filtering)
        {
            var result = new IndexViewModel();

            var purchaseStatistics = _context.PurchaseStatistics
                .Where(x => (filtering.EventCd == 0 || x.EventCd == filtering.EventCd) &&
                            x.Type == filtering.Type);
            
            result.TotalRowsAfterFiltering = purchaseStatistics.Count();
            purchaseStatistics             = Filtering(purchaseStatistics, filtering);
            result.PurchaseStatistics      = purchaseStatistics
                .Select(x => new
                {
                    x.Cd,
                    x.InsertUser.FirstName,
                    x.InsertUser.LastName,
                    x.InsertDate,
                    x.Event.StartDate,
                    x.Event.EndDate,
                    x.Event.Name
                })
                .AsEnumerable()
                .Select(x => new PerchaseStatisticsRow
                {
                    Cd           = x.Cd,
                    Time         = x.StartDate.HasValue && x.EndDate.HasValue
                                    ? $"{x.StartDate.Value.Date.Year}年{CommonExtension.GetWeekOfMonth(x.StartDate.Value, x.EndDate.Value, false)}"
                                    : "",
                    EventName    = x.Name,
                    OutputPerson = string.Concat(x.FirstName, x.LastName),
                    InsertDate   = x.InsertDate.Value.ToString(Constants.ExactDateTimeFormat)
                }).ToList();
            
            if (result.PurchaseStatistics.Any())
            {
                for (var i = 0; i < result.PurchaseStatistics.Count; i++)
                {
                    result.PurchaseStatistics[i].No = filtering.Start + i + 1;
                }
            }
            return result;

        }

        /// <summary>
        /// Sorting and paging data from Load
        /// </summary>
        /// <param name="purchaseStatistics"></param>
        /// <param name="filtering"></param>
        /// <returns>IQueryable</returns>
        private IQueryable<PurchaseStatistics> Filtering(IQueryable<PurchaseStatistics> purchaseStatistics,
            IndexViewModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "Time":
                    if (filtering.SortDirection == "asc")
                    {
                        purchaseStatistics = purchaseStatistics.OrderBy(x => x.PeriodTime)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        purchaseStatistics = purchaseStatistics.OrderByDescending(x => x.PeriodTime)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "EventName":
                    if (filtering.SortDirection == "asc")
                    {
                        purchaseStatistics = purchaseStatistics.OrderBy(x => x.Event.Name)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        purchaseStatistics = purchaseStatistics.OrderByDescending(x => x.Event.Name)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "OutputPerson":
                    if (filtering.SortDirection == "asc")
                    {
                        purchaseStatistics = purchaseStatistics.OrderBy(x => string.Concat(x.InsertUser.FirstName, x.InsertUser.LastName))
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        purchaseStatistics = purchaseStatistics.OrderByDescending(x => string.Concat(x.InsertUser.FirstName, x.InsertUser.LastName))
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "InsertDate":
                    if (filtering.SortDirection == "asc")
                    {
                        purchaseStatistics = purchaseStatistics.OrderBy(x => x.InsertDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        purchaseStatistics = purchaseStatistics.OrderByDescending(x => x.InsertDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                    purchaseStatistics = purchaseStatistics.OrderByDescending(x => x.InsertDate)
                        .Skip(filtering.Start).Take(filtering.Length);
                    break;
            }

            return purchaseStatistics;
        }

        /// <summary>
        /// Get purchase statistics A & B data to create
        /// </summary>
        /// <param name="model"></param>
        /// <returns>PurchaseStatisticsDetailModel</returns>
        public PurchaseStatisticsDetailModel GetListOfContract(SearchFormModel model)
        {
            var evt = _context.MstEvents.Find(model.EventCd);
            if (evt == null || !evt.StartDate.HasValue || !evt.EndDate.HasValue)
            {
                return null;
            }

            var result = new PurchaseStatisticsDetailModel
            {
                PurchaseType = model.Type,
                PeriodTime   = evt.StartDate.HasValue && evt.EndDate.HasValue
                                ? $"{CommonExtension.GetWeekOfMonth(evt.StartDate.Value, evt.EndDate.Value)}【{evt.Name}】"
                                : "",
                EventCd      = evt.Cd,
                EventName    = evt.Name,
                EventAddress = evt.Address,
                EventPlace   = evt.Place,
                StartDate    = evt.StartDate.Value,
                EndDate      = evt.EndDate.Value
            };

            var contracts = _context.Contracts
                .Include(x => x.Media)
                .Where(x => x.EventCd == evt.Cd && !x.IsDeleted && x.IsCompleted);

            if (contracts.Any())
            {
                var rowsDetail = new List<RowDetail>();
                var dates = new List<DateTime>();
                if (model.Type == (int) Constants.PurchaseType.A)
                {
                    foreach (var contract in contracts)
                    {
                        dates.Add(contract.ContractDate.Value);
                        var row = rowsDetail.FirstOrDefault(x => x.MediaCd == contract.MediaCd &&
                                                                     // ReSharper disable once PossibleInvalidOperationException
                                                                     x.HoldDate.Date.CompareTo(contract.ContractDate.Value.Date) == 0);
                        if (row == null)
                        {
                            row = new RowDetail
                            {
                                // ReSharper disable once PossibleInvalidOperationException
                                HoldDate  = contract.ContractDate.Value,
                                MediaCd   = contract.Media.Cd,
                                MediaName = contract.Media.Name,
                                MediaCode = contract.Media.Code
                            };

                            row.NumberOfAttractingCustomers = _context.Surveys
                                .Count(x => x.MediaCd == row.MediaCd && row.HoldDate.Date == x.InsertDate.Value.Date);
                            rowsDetail.Add(row);
                        }
                        
                        row.NumberOfContracts++;
                        row.TotalPriceOfContracts += contract.TotalPrice ?? 0;
                        row.ContractPrices.Add(Math.Floor((contract.TotalPrice ?? 0) / 10000));
                    }
                }
                else
                {
                    foreach (var contract in contracts)
                    {
                        var tempRowDetail = DistinctProductFromContract(contract);
                        foreach (var temp in tempRowDetail)
                        {
                            var artist = _context.MstArtists.FirstOrDefault(x => temp.ArtistCd != 0 && x.Cd == temp.ArtistCd);
                            if (artist != null && !artist.Name.Equals(temp.ArtistName)) {
                                temp.ArtistName = artist.Name;
                            }
                            
                            var cell = rowsDetail.FirstOrDefault(x => x.ArtistCd == temp.ArtistCd && x.ArtistName.Equals(temp.ArtistName) && x.HoldDate.Date.CompareTo(temp.HoldDate.Date) == 0);
                            if (cell == null)
                            {
                                rowsDetail.Add(temp);
                            }
                            else
                            {
                                cell.NumberOfContracts++;
                                cell.TotalPriceOfContracts += temp.TotalPriceOfContracts;
                                cell.ContractPrices.AddRange(temp.ContractPrices);
                            }
                        }
                    }
                }

                if (rowsDetail.Any())
                {
                    result.RowDetails = rowsDetail;

                    if (model.Type == (int) Constants.PurchaseType.A)
                    {
                        result.MediasTotal = rowsDetail.GroupBy(x => new
                        {
                            x.MediaCd,
                            x.MediaCode,
                            x.MediaName
                        })
                        .Select(x => new Total
                        {
                            MediaCd                  = x.Key.MediaCd,
                            MediaName                = x.Key.MediaName,
                            MediaCode                = x.Key.MediaCode,
                            TotalAttractingCustomers = x.Sum(y => y.NumberOfAttractingCustomers),
                            TotalContracts           = x.Sum(y => y.NumberOfContracts),
                            SubTotal                 = x.Sum(y => y.TotalPriceOfContracts),
                            MaxCell                  = x.Max(y => y.ContractPrices.Count),
                        }).ToList();

                        // add number of attracting customer for the dayafowithout a contract
                        for (var date = evt.StartDate.Value; date <= evt.EndDate; date = date.AddDays(1))
                        {
                            foreach (var media in result.MediasTotal)
                            {
                                var isExisted = rowsDetail.FirstOrDefault(x => x.HoldDate.Date == date.Date && 
                                                            x.MediaCd == media.MediaCd) != null;
                                if (!isExisted)
                                {
                                    var row = new RowDetail
                                    {
                                        // ReSharper disable once PossibleInvalidOperationException
                                        HoldDate  = date,
                                        MediaCd   = media.MediaCd,
                                        MediaName = media.MediaName,
                                        MediaCode = media.MediaCode
                                    };

                                    row.NumberOfAttractingCustomers = _context.Surveys.Count(x => x.EventCd == evt.Cd &&
                                        x.MediaCd == row.MediaCd && row.HoldDate.Date == x.InsertDate.Value.Date);
                                    
                                    // add to total attracting customer
                                    media.TotalAttractingCustomers += row.NumberOfAttractingCustomers;
                                    
                                    rowsDetail.Add(row);
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        result.ArtistsTotal = rowsDetail.GroupBy(x => new
                            {
                                x.ArtistCd,
                                x.ArtistName
                            })
                            .Select(x => new Total
                            {
                                ArtistCd       = x.Key.ArtistCd,
                                ArtistName     = x.Key.ArtistName,
                                TotalContracts = x.Sum(y => y.NumberOfContracts),
                                SubTotal       = x.Sum(y => y.TotalPriceOfContracts),
                                MaxCell        = x.Max(y => y.ContractPrices.Count)
                            }).ToList();
                    }

                    result.DateTotal = rowsDetail.GroupBy(x => x.HoldDate)
                        .Select(x => new Total
                        {
                            Date                     = x.Key,
                            TotalAttractingCustomers = x.Sum(y => y.NumberOfAttractingCustomers),
                            TotalContracts           = x.Sum(y => y.NumberOfContracts),
                            SubTotal                 = x.Sum(y => y.TotalPriceOfContracts)
                        }).ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Get product detail from contract for purchase statistics B
        /// </summary>
        /// <param name="contract"></param>
        /// <returns>IEnumerable</returns>
        private IEnumerable<RowDetail> DistinctProductFromContract(Contract contract)
        {
            var tempList = new List<RowDetail>();
            if (!string.IsNullOrEmpty(contract.ProductName1))
            {
                var row = new RowDetail
                {
                    ArtistCd   = contract.ArtistCd1 == null ? 0 : contract.ArtistCd1.Value,
                    ArtistName = contract.ArtistName1,
                    // ReSharper disable once PossibleInvalidOperationException
                    HoldDate   = contract.ContractDate.Value
                };
                var productPrice = (contract.ProductSalesPrice1 ?? 0) + contract.ProductTaxPrice1 ?? 0;
                row.NumberOfContracts++;
                row.TotalPriceOfContracts += productPrice;
                row.ContractPrices.Add(Math.Floor(productPrice / 10000));
                tempList.Add(row);
            }

            if (!string.IsNullOrEmpty(contract.ProductName2))
            {
                var row = tempList.FirstOrDefault(x => x.ArtistName.Equals(contract.ArtistName2));
                var productPrice = (contract.ProductSalesPrice2 ?? 0) + contract.ProductTaxPrice2 ?? 0;
                if (row == null)
                {
                    row = new RowDetail
                    {
                        ArtistCd   = contract.ArtistCd2 == null ? 0 : contract.ArtistCd2.Value,
                        ArtistName = contract.ArtistName2,
                        // ReSharper disable once PossibleInvalidOperationException
                        HoldDate   = contract.ContractDate.Value
                    };
                    row.NumberOfContracts++;
                    row.ContractPrices.Add(Math.Floor(productPrice / 10000));
                    tempList.Add(row);
                }
                else
                {
                    row.ContractPrices[0] += Math.Floor(productPrice / 10000);
                }

                row.TotalPriceOfContracts += productPrice;

            }

            if (!string.IsNullOrEmpty(contract.ProductName3))
            {
                var row = tempList.FirstOrDefault(x => x.ArtistName.Equals(contract.ArtistName3));
                var productPrice = (contract.ProductSalesPrice3 ?? 0) + contract.ProductTaxPrice3 ?? 0;
                if (row == null)
                {
                    row = new RowDetail
                    {
                        ArtistCd   = contract.ArtistCd3 == null ? 0 : contract.ArtistCd3.Value,
                        ArtistName = contract.ArtistName3,
                        // ReSharper disable once PossibleInvalidOperationException
                        HoldDate   = contract.ContractDate.Value
                    };
                    row.NumberOfContracts++;
                    row.ContractPrices.Add(Math.Floor(productPrice / 10000));
                    tempList.Add(row);
                }
                else
                {
                    row.ContractPrices[0] += Math.Floor(productPrice / 10000);
                }

                row.TotalPriceOfContracts += productPrice;

            }

            if (!string.IsNullOrEmpty(contract.ProductName4))
            {
                var row = tempList.FirstOrDefault(x => x.ArtistName.Equals(contract.ArtistName4));
                var productPrice = (contract.ProductSalesPrice4 ?? 0) + contract.ProductTaxPrice4 ?? 0;
                if (row == null)
                {
                    row = new RowDetail
                    {
                        ArtistCd   = contract.ArtistCd4 == null ? 0 : contract.ArtistCd4.Value,
                        ArtistName = contract.ArtistName4,
                        // ReSharper disable once PossibleInvalidOperationException
                        HoldDate   = contract.ContractDate.Value
                    };
                    row.NumberOfContracts++;
                    row.ContractPrices.Add(Math.Floor(productPrice / 10000));
                    tempList.Add(row);
                }
                else
                {
                    row.ContractPrices[0] += Math.Floor(productPrice / 10000);
                }

                row.TotalPriceOfContracts += productPrice;

            }

            if (!string.IsNullOrEmpty(contract.ProductName5))
            {
                var row = tempList.FirstOrDefault(x => x.ArtistName.Equals(contract.ArtistName5));
                var productPrice = (contract.ProductSalesPrice5 ?? 0) + contract.ProductTaxPrice5 ?? 0;
                if (row == null)
                {
                    row = new RowDetail
                    {
                        ArtistCd   = contract.ArtistCd5 == null ? 0 : contract.ArtistCd5.Value,
                        ArtistName = contract.ArtistName5,
                        // ReSharper disable once PossibleInvalidOperationException
                        HoldDate   = contract.ContractDate.Value
                    };
                    row.NumberOfContracts++;
                    row.ContractPrices.Add(Math.Floor(productPrice / 10000));
                    tempList.Add(row);
                }
                else
                {
                    row.ContractPrices[0] += Math.Floor(productPrice / 10000);
                }

                row.TotalPriceOfContracts += productPrice;

            }

            if (!string.IsNullOrEmpty(contract.ProductName6))
            {
                var row = tempList.FirstOrDefault(x => x.ArtistName.Equals(contract.ArtistName6));
                var productPrice = (contract.ProductSalesPrice6 ?? 0) + contract.ProductTaxPrice6 ?? 0;
                if (row == null)
                {
                    row = new RowDetail
                    {
                        ArtistCd = contract.ArtistCd6 == null ? 0 : contract.ArtistCd6.Value,
                        ArtistName = contract.ArtistName6,
                        // ReSharper disable once PossibleInvalidOperationException
                        HoldDate = contract.ContractDate.Value
                    };
                    row.NumberOfContracts++;
                    row.ContractPrices.Add(Math.Floor(productPrice / 10000));
                    tempList.Add(row);
                }
                else
                {
                    row.ContractPrices[0] += Math.Floor(productPrice / 10000);
                }

                row.TotalPriceOfContracts += productPrice;

            }

            return tempList;
        }

        /// <summary>
        /// Get purchase statistics A & B data to update
        /// </summary>
        /// <param name="model"></param>
        /// <returns>PurchaseStatisticsDetailModel</returns>
        public PurchaseStatisticsDetailModel GetListOfContractByPurchaseStatisticsCd(SearchFormModel model)
        {
            var purchaseStatistics = _context.PurchaseStatistics
                .Include(x => x.Event)
                .FirstOrDefault(x => x.Cd == model.Cd);
            if (purchaseStatistics == null || !purchaseStatistics.Event.StartDate.HasValue || !purchaseStatistics.Event.EndDate.HasValue)
            {
                return null;
            }

            var result = new PurchaseStatisticsDetailModel
            {
                Cd                 = model.Cd,
                PeriodTime         = purchaseStatistics.PeriodTime,
                EventCd            = purchaseStatistics.EventCd,
                EventName          = purchaseStatistics.Event?.Name,
                EventAddress       = purchaseStatistics.Event?.Address,
                EventPlace         = purchaseStatistics.Event?.Place,
                // ReSharper disable once PossibleNullReferenceException
                StartDate          = purchaseStatistics.Event.StartDate.Value,
                EndDate            = purchaseStatistics.Event.EndDate.Value,
                // ReSharper disable once PossibleInvalidOperationException
                EventManagerCd     = purchaseStatistics.EventManagerCd.Value,
                EventManagerString = _context.MstSalesmen.Find(purchaseStatistics.EventManagerCd).Name,
                // ReSharper disable once PossibleInvalidOperationException
                InputPersonCd      = purchaseStatistics.InputPersonCd.Value,
                InputPersonString  = _context.MstSalesmen.Find(purchaseStatistics.InputPersonCd).Name,
                PurchaseType       = model.Type
            };

            var dates = _context.PurchaseStatisticsDates
                .Include(x => x.PurchaseStatisticsDetail)
                .ThenInclude(x => x.PurchaseStatistics)
                .Where(x => x.PurchaseStatisticsDetail.PurchaseStatistics.Cd == model.Cd)
                .Select(x => new
                {
                    MediaCd    = x.PurchaseStatisticsDetail.Media.Cd,
                    MediaName  = x.PurchaseStatisticsDetail.Media.Name,
                    MediaCode  = x.PurchaseStatisticsDetail.Media.Code,
                    ArtistCd   = x.PurchaseStatisticsDetail.ArtistCd.Value,
                    ArtistName = x.PurchaseStatisticsDetail.ArtistName,
                    x.HoldDate,
                    x.NumberOfContracts,
                    x.AttractingCustomers,
                    x.TotalPrice,
                    x.PurchaseStatisticsContracts
                })
                .AsEnumerable()
                .Select(x => new RowDetail
                {
                    MediaCd                     = x.MediaCd,
                    MediaName                   = x.MediaName,
                    MediaCode                   = x.MediaCode,
                    ArtistCd                    = x.ArtistCd,
                    ArtistName                  = x.ArtistName,
                    HoldDate                    = x.HoldDate,
                    NumberOfContracts           = x.NumberOfContracts,
                    NumberOfAttractingCustomers = x.AttractingCustomers,
                    TotalPriceOfContracts       = x.TotalPrice,
                    ContractPrices              = x.PurchaseStatisticsContracts.Select(y => y.TotalOfMoney).ToList()
                }).ToList();
            
            if (dates.Any())
            {
                result.RowDetails = dates;
                result.DateTotal = dates.GroupBy(x => x.HoldDate)
                    .Select(x => new Total
                    {
                        Date                     = x.Key,
                        TotalAttractingCustomers = x.Sum(y => y.NumberOfAttractingCustomers),
                        TotalContracts           = x.Sum(y => y.NumberOfContracts),
                        SubTotal                 = x.Sum(y => y.TotalPriceOfContracts)
                    }).ToList();

                if (model.Type == (int)Constants.PurchaseType.A)
                {
                    result.MediasTotal = dates.GroupBy(x => new
                    {
                        x.MediaCd,
                        x.MediaCode,
                        x.MediaName
                    })
                    .Select(x => new Total
                    {
                        MediaCd                  = x.Key.MediaCd,
                        MediaName                = x.Key.MediaName,
                        MediaCode                = x.Key.MediaCode,
                        TotalAttractingCustomers = x.Sum(y => y.NumberOfAttractingCustomers),
                        TotalContracts           = x.Sum(y => y.NumberOfContracts),
                        SubTotal                 = x.Sum(y => y.TotalPriceOfContracts),
                        MaxCell                  = x.Max(y => y.ContractPrices.Count)
                    }).ToList();
                }
                else
                {
                    result.ArtistsTotal = dates.GroupBy(x => new
                    {
                        x.ArtistCd,
                        x.ArtistName
                    })
                    .Select(x => new Total
                    {
                        ArtistCd       = x.Key.ArtistCd,
                        ArtistName     = x.Key.ArtistName,
                        TotalContracts = x.Sum(y => y.NumberOfContracts),
                        SubTotal       = x.Sum(y => y.TotalPriceOfContracts),
                        MaxCell        = x.Max(y => y.ContractPrices.Count)
                    }).ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Add new purchase statistics A & B
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns>PurchaseStatistics</returns>
        public PurchaseStatistics CreatePurchaseStatistics(PurchaseStatisticsDetailModel model, string userId)
        {
            var purchaseStatistics = new PurchaseStatistics
            {
                EventCd        = model.EventCd,
                PeriodTime     = model.PeriodTime,
                Type           = model.PurchaseType,
                EventManagerCd = model.EventManagerCd,
                InputPersonCd  = model.InputPersonCd,
                InsertDate     = DateTime.Now,
                InsertUserId   = userId
            };

            var details = new List<PurchaseStatisticsDetail>();
            foreach (var row in model.RowDetails)
            {
                PurchaseStatisticsDetail cell;
                if (model.PurchaseType == (int) Constants.PurchaseType.A)
                {
                    cell = details.FirstOrDefault(x => x.MediaCd == row.MediaCd);
                }
                else
                {
                    cell = details.FirstOrDefault(x => x.ArtistName.Equals(row.ArtistName));
                }

                if (cell == null)
                {
                    cell = new PurchaseStatisticsDetail
                    {
                        MediaCd      = row.MediaCd == 0 ? (int?)null : row.MediaCd,
                        ArtistCd     = row.ArtistCd == 0 ? (int?)null : row.ArtistCd,
                        ArtistName   = row.ArtistName,
                        InsertDate   = DateTime.Now,
                        InsertUserId = userId
                    };
                    details.Add(cell);
                }

                cell.PurchaseStatisticsDates ??= new List<PurchaseStatisticsDate>();

                cell.PurchaseStatisticsDates.Add(new PurchaseStatisticsDate
                {
                    AttractingCustomers = row.NumberOfAttractingCustomers,
                    HoldDate            = row.HoldDate,
                    NumberOfContracts   = row.NumberOfContracts,
                    TotalPrice          = row.TotalPriceOfContracts,
                    InsertDate          = DateTime.Now,
                    InsertUserId        = userId,
                    PurchaseStatisticsContracts = row.ContractPrices.Select(y => new PurchaseStatisticsContract
                    {
                        TotalOfMoney = y,
                        InsertDate   = DateTime.Now,
                        InsertUserId = userId
                    }).ToList()
                });

            }

            if (details.Any())
            {
                purchaseStatistics.PurchaseStatisticsDetails = details;
            }

            _context.PurchaseStatistics.Add(purchaseStatistics);
            return purchaseStatistics;
        }

        /// <summary>
        /// Update purchase statistic A & B
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns>PurchaseStatistics</returns>
        public PurchaseStatistics UpdatePurchaseStatistics(PurchaseStatisticsDetailModel model, string userId)
        {
            var entity = _context.PurchaseStatistics.Find(model.Cd);
            if (entity == null) return null;
            {
                entity.EventManagerCd = model.EventManagerCd;
                entity.InputPersonCd  = model.InputPersonCd;
                entity.UpdateDate     = DateTime.Now;
                entity.UpdateUserId   = userId;
                return entity;
            }
        }
    }
}
