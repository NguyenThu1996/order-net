using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.OrderReport;
using OPS.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using static OPS.Utility.Constants;
using OrderReportCash = OPS.Entity.Schemas.OrderReportCash;

namespace OPS.Business.Repository
{
    public class OrderReportRepository : IOrderReportRepository
    {
        private readonly OpsContext _context;
        public OrderReportRepository(OpsContext context)
        {
            _context = context;
        }

        public IndexViewModel Load(IndexViewModel filtering)
        {
            var result = new IndexViewModel();
            var reports = _context.OrderReports.Where(x => 
                                        (filtering.EventCd == 0 || x.EventCd == filtering.EventCd) &&
                                        (filtering.HoldTime == null || x.HoldTime.Date == filtering.HoldTime.Value.Date));
            result.TotalRowsAfterFiltering = reports.Count();

            // sorting and paging
            reports = Filtering(reports, filtering);
            result.ReportOrders = reports
                .Select(x => new
                {
                    x.Cd,
                    x.EventCd,
                    x.HoldTime,
                    x.InsertDate,
                    EventCode = x.Event.Code,
                    EventName = x.Event.Name
                })
                .AsEnumerable()
                .Select(x => new ReportOrderRow()
                {
                    Cd             = x.Cd,
                    EventCd        = x.EventCd,
                    HoldTime       = x.HoldTime.ToString(ExactDateFormat),
                    EventCode      = x.EventCode,
                    EventName      = x.EventName,
                    InsertDate     = x.InsertDate?.ToString(ExactDateTimeFormat)
                }).ToList();
            if (result.ReportOrders.Any())
            {
                for(int i = 0; i < result.ReportOrders.Count; i++)
                {
                    result.ReportOrders[i].No = filtering.Start + i + 1;
                }
            }
            return result;
        }

        private IQueryable<OrderReport> Filtering(IQueryable<OrderReport> reports, IndexViewModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "HoldTime":
                    if (filtering.SortDirection == "asc")
                    {
                        reports = reports.OrderBy(x => x.HoldTime)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        reports = reports.OrderByDescending(x => x.HoldTime)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "EventCode":
                    if (filtering.SortDirection == "asc")
                    {
                        reports = reports.OrderBy(x => x.Event.Code.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        reports = reports.OrderByDescending(x => x.Event.Code.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "EventName":
                    if (filtering.SortDirection == "asc")
                    {
                        reports = reports.OrderBy(x => x.Event.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        reports = reports.OrderByDescending(x => x.Event.Name.ToLower())
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                case "InsertDate":
                    if (filtering.SortDirection == "asc")
                    {
                        reports = reports.OrderBy(x => x.InsertDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        reports = reports.OrderByDescending(x => x.InsertDate)
                            .Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;
                default:
                    reports = reports.OrderByDescending(x => x.InsertDate)
                        .Skip(filtering.Start).Take(filtering.Length);
                    break;
            }
            return reports;
        }

        public OrderReportDetailModel GetOrderReportForm(ModalSearchModel model)
        {
            var contracts =_context.Contracts
                .Where(x => x.IsCompleted && !x.IsDeleted && !x.FutureEventCd.HasValue &&
                            x.EventCd == model.EventCd && 
                            x.ContractDate.Value.Date == model.HoldTime.Value.Date)
                    .Include(x => x.Event)
                    .Include(x => x.SalesmanA)
                    .Include(x => x.SalesmanS)
                    .Include(x => x.SalesmanC)
                    .Include(x => x.Media)
                    .OrderBy(x => x.Cd);

            if (!contracts.Any()) return null;
            {
                var evt = contracts.FirstOrDefault()?.Event;
                if (evt == null) 
                {
                    return null;
                }

                var result = new OrderReportDetailModel
                {
                    EventCd    = model.EventCd,
                    EventName  = evt.Name,
                    HoldTime   = model.HoldTime,
                    PeriodTime = evt.StartDate.HasValue && evt.EndDate.HasValue ? $"{CommonExtension.GetWeekOfMonth(evt.StartDate.Value, evt.EndDate.Value)}【{evt.Name}】" : "",
                    PreviousDayOrderCash = _context.OrderReports
                                            .Where(or => or.EventCd == model.EventCd && or.HoldTime.Date < model.HoldTime.Value.Date)
                                            .Sum(or => or.SameDayOrderCash),
                };

                foreach (var contract in contracts)
                {
                    var rows = GetProductFromContract(contract, result);
                    result.OrderReportDetailRows.AddRange(rows);
                }

                return result;

            }
        }

        private IEnumerable<OrderReportDetailRowModel> GetProductFromContract(Contract contract, OrderReportDetailModel orderReportDetail)
        {
            var result = new List<OrderReportDetailRowModel>();

            orderReportDetail.TotalDownPayment += contract.DownPayment ?? 0;
            if (contract.DownPaymentMethod.HasValue &&
                    contract.DownPaymentMethod == (int)DownPaymentMethod.InCash)
            {
                orderReportDetail.TotalReceivedCash += contract.DownPayment ?? 0;
            }

            var row = new OrderReportDetailRowModel
            {
                LeftPaymentMethodCd = contract.LeftPaymentMethod,
                LeftPaymentMethodName = contract.LeftPaymentMethod != null ?
                                        ((LeftPaymentMethod)contract.LeftPaymentMethod).GetEnumDescription()
                                        : string.Empty,
                VerifyNumber = contract.VerifyNumber,
                SalesmanACd = contract.SalesmanACd,
                SalesmanACode = contract.SalesmanA?.Code,
                SalesmanAName = contract.SalesmanA?.Name,
                SalesmanCCd = contract.SalesmanCCd,
                SalesmanCCode = contract.SalesmanC?.Code,
                SalesmanCName = contract.SalesmanC?.Name,
                SalesmanSCd = contract.SalesmanSCd,
                SalesmanSCode = contract.SalesmanS?.Code,
                SalesmanSName = contract.SalesmanS?.Name,
                Name = $"{contract.FamilyName} {contract.FirstName}",
                NameFuri = $"{contract.FamilyNameFuri} {contract.FirstNameFuri}",
                Zipcode = contract.Zipcode,
                HomePhone = contract.HomePhone,
                DownPayment = contract.DownPayment ?? 0,
                DownPaymentMethodCd = contract.DownPaymentMethod,
                DownPaymentMethodName = contract.DownPaymentMethod != null ?
                                        ((DownPaymentMethod)contract.DownPaymentMethod).GetEnumDescription() :
                                        string.Empty,
                ReceivedCash = contract.DownPaymentMethod != null && contract.DownPaymentMethod == (int)DownPaymentMethod.InCash
                                ? contract.DownPayment ?? 0
                                : 0,
                ContractCd = contract.Cd,
                MediaCd = contract.MediaCd,
                MediaCode = contract.Media?.Code,
                Area = contract.Zipcode,
                AvClub = contract.ClubJoinStatus,
                JxClub = contract.MemberJoinStatus,
                NumberOfVisit = contract.VisitTime ?? 1,
            };

            if (!string.IsNullOrEmpty(contract.ProductName1))
            {
                var row1 = (OrderReportDetailRowModel)row.Clone();

                row1.ProductNo = 1;
                row1.AuthorName    = contract.ArtistName1;
                row1.ProductName   = contract.ProductName1;
                row1.Price         = (contract.ProductPrice1 ?? 0) * (contract.ProductQuantity1 ?? 0);
                row1.Discount      = (contract.ProductDiscount1 ?? 0) * (contract.ProductQuantity1 ?? 0);
                row1.TaxPrice      = contract.ProductTaxPrice1 ?? 0;
                row1.DeliverDate = contract.DeliveryDate1;
                row1.RemarkString = TransformRemarkString(contract.ProductRemarks1, contract.Approve);
                row1.CashVoucherValueString = contract.CashVoucherValue1.HasValue ? ((CashVoucherValue)contract.CashVoucherValue1.Value).GetEnumDescription() : "-";
                result.Add(row1);

                orderReportDetail.TotalPrice               += row1.Price;                                          // Cal total price
                orderReportDetail.TotalPriceDiscount       += row1.Discount;                                       // cal total discount
                orderReportDetail.TotalPriceSale           += row1.Price - row1.Discount;                           // cal total sale = price - discount
                orderReportDetail.TotalPriceSaleIncludeTax += (contract.ProductSalesPrice1 ?? 0) + row1.TaxPrice;  // cal last price (include tax)
            }

            if (!string.IsNullOrEmpty(contract.ProductName2))
            {
                var row2 = (OrderReportDetailRowModel)row.Clone();

                row2.ProductNo = 2;
                row2.AuthorName  = contract.ArtistName2;
                row2.ProductName = contract.ProductName2;
                row2.Price       = (contract.ProductPrice2 ?? 0) * (contract.ProductQuantity2 ?? 0);
                row2.Discount    = (contract.ProductDiscount2 ?? 0) * (contract.ProductQuantity2 ?? 0);
                row2.TaxPrice    = contract.ProductTaxPrice2 ?? 0;
                row2.DeliverDate = contract.DeliveryDate2;
                row2.RemarkString = TransformRemarkString(contract.ProductRemarks2, contract.Approve);
                row2.CashVoucherValueString = contract.CashVoucherValue2.HasValue ? ((CashVoucherValue)contract.CashVoucherValue2.Value).GetEnumDescription() : "-";
                result.Add(row2);

                orderReportDetail.TotalPrice += row2.Price;
                orderReportDetail.TotalPriceDiscount += row2.Discount;
                orderReportDetail.TotalPriceSale += row2.Price - row2.Discount;
                orderReportDetail.TotalPriceSaleIncludeTax += (contract.ProductSalesPrice2 ?? 0) + row2.TaxPrice;
            }

            if (!string.IsNullOrEmpty(contract.ProductName3))
            {
                var row3         = (OrderReportDetailRowModel)row.Clone();

                row3.ProductNo = 3;
                row3.DeliverDate = contract.DeliveryDate3;
                row3.AuthorName  = contract.ArtistName3;
                row3.ProductName = contract.ProductName3;
                row3.Price       = (contract.ProductPrice3 ?? 0) * (contract.ProductQuantity3 ?? 0);
                row3.Discount    = (contract.ProductDiscount3 ?? 0) * (contract.ProductQuantity3 ?? 0);
                row3.TaxPrice    = contract.ProductTaxPrice3 ?? 0;
                row3.DeliverDate = contract.DeliveryDate3;
                row3.RemarkString = TransformRemarkString(contract.ProductRemarks3, contract.Approve);
                row3.CashVoucherValueString = contract.CashVoucherValue3.HasValue ? ((CashVoucherValue)contract.CashVoucherValue3.Value).GetEnumDescription() : "-";
                result.Add(row3);

                orderReportDetail.TotalPrice               += row3.Price;
                orderReportDetail.TotalPriceDiscount       += row3.Discount;
                orderReportDetail.TotalPriceSale           += row3.Price - row3.Discount;
                orderReportDetail.TotalPriceSaleIncludeTax += (contract.ProductSalesPrice3 ?? 0) + row3.TaxPrice;
            }

            if (!string.IsNullOrEmpty(contract.ProductName4))
            {
                var row4         = (OrderReportDetailRowModel)row.Clone();

                row4.ProductNo = 4;
                row4.DeliverDate = contract.DeliveryDate4;
                row4.AuthorName  = contract?.ArtistName4;
                row4.ProductName = contract.ProductName4;
                row4.Price       = (contract.ProductPrice4 ?? 0) * (contract.ProductQuantity4 ?? 0);
                row4.Discount    = contract.ProductDiscount4 ?? 0 * (contract.ProductQuantity4 ?? 0);
                row4.TaxPrice    = contract.ProductTaxPrice4 ?? 0;
                row4.DeliverDate = contract.DeliveryDate4;
                row4.RemarkString = TransformRemarkString(contract.ProductRemarks4, contract.Approve);
                row4.CashVoucherValueString = contract.CashVoucherValue4.HasValue ? ((CashVoucherValue)contract.CashVoucherValue4.Value).GetEnumDescription() : "-";
                result.Add(row4);

                orderReportDetail.TotalPrice               += row4.Price;
                orderReportDetail.TotalPriceDiscount       += row4.Discount;
                orderReportDetail.TotalPriceSale           += row4.Price - row4.Discount;
                orderReportDetail.TotalPriceSaleIncludeTax += (contract.ProductSalesPrice4 ?? 0) + row4.TaxPrice;
            }

            if (!string.IsNullOrEmpty(contract.ProductName5))
            {
                var row5         = (OrderReportDetailRowModel)row.Clone();

                row5.ProductNo = 5;
                row5.DeliverDate = contract.DeliveryDate5;
                row5.AuthorName  = contract.ArtistName5;
                row5.ProductName = contract.ProductName5;
                row5.Price       = (contract.ProductPrice5 ?? 0) * (contract.ProductQuantity5 ?? 0);
                row5.Discount    = (contract.ProductDiscount5 ?? 0) * (contract.ProductQuantity5 ?? 0);
                row5.TaxPrice    = contract.ProductTaxPrice5 ?? 0;
                row5.DeliverDate = contract.DeliveryDate5;
                row5.RemarkString = TransformRemarkString(contract.ProductRemarks5, contract.Approve);
                row5.CashVoucherValueString = contract.CashVoucherValue5.HasValue ? ((CashVoucherValue)contract.CashVoucherValue5.Value).GetEnumDescription() : "-";
                result.Add(row5);

                orderReportDetail.TotalPrice               += row5.Price;
                orderReportDetail.TotalPriceDiscount       += row5.Discount;
                orderReportDetail.TotalPriceSale           += row5.Price - row5.Discount;
                orderReportDetail.TotalPriceSaleIncludeTax += (contract.ProductSalesPrice5 ?? 0) + row5.TaxPrice;
            }

            if (!string.IsNullOrEmpty(contract.ProductName6))
            {
                var row6         = (OrderReportDetailRowModel)row.Clone();

                row6.ProductNo = 6;
                row6.DeliverDate = contract.DeliveryDate6;
                row6.AuthorName  = contract.ArtistName6;
                row6.ProductName = contract.ProductName6;
                row6.Price       = (contract.ProductPrice6 ?? 0) * (contract.ProductQuantity6 ?? 0);
                row6.Discount    = (contract.ProductDiscount6 ?? 0) * (contract.ProductQuantity6 ?? 0);
                row6.TaxPrice    = contract.ProductTaxPrice6 ?? 0;
                row6.DeliverDate = contract.DeliveryDate6;
                row6.RemarkString = TransformRemarkString(contract.ProductRemarks6, contract.Approve);
                row6.CashVoucherValueString = contract.CashVoucherValue6.HasValue ? ((CashVoucherValue)contract.CashVoucherValue6.Value).GetEnumDescription() : "-";
                result.Add(row6);

                orderReportDetail.TotalPrice               += row6.Price;
                orderReportDetail.TotalPriceDiscount       += row6.Discount;
                orderReportDetail.TotalPriceSale           += row6.Price - row6.Discount;
                orderReportDetail.TotalPriceSaleIncludeTax += (contract.ProductSalesPrice6 ?? 0) + row6.TaxPrice;
            }

            return result;
        }

        private string TransformRemarkString(string remarkStr, string approve = "")
        {
            var result = "-";
            var remarkTextList = new List<string>();

            if (!string.IsNullOrEmpty(remarkStr))
            {
                var remarkCds = Commons.SplitStringToNumbers(remarkStr);

                for (var i = 0; i < remarkCds.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            if (remarkCds[i] > 0)
                            {
                                remarkTextList.Add(((ProductRemarkA)remarkCds[i]).GetEnumDescription());
                            }
                            break;
                        case 1:
                            if (remarkCds[i] > 0)
                            {
                                remarkTextList.Add(((ProductRemarkB)remarkCds[i]).GetEnumDescription());
                            }
                            break;
                        case 2:
                            if (remarkCds[i] > 0)
                            {
                                remarkTextList.Add(((ProductRemarkC)remarkCds[i]).GetEnumDescription());
                            }
                            break;
                        case 3:
                            if (remarkCds[i] > 0)
                            {
                                remarkTextList.Add(((ProductRemarkD)remarkCds[i]).GetEnumDescription());
                            }
                            break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(approve))
            {
                remarkTextList.Add(approve);
            }

            if (remarkTextList.Count > 0)
            {
                result = string.Join(", ", remarkTextList);
            }

            return result;
        }

        public int SaveOrderReport(OrderReportDetailModel model, string userId)
        {
            var orderReport = new OrderReport
            {
                // ReSharper disable once PossibleInvalidOperationException
                HoldTime               = model.HoldTime.Value,
                PersonInChargeCd       = model.PersonInChargeCd,
                LastConfirmSalesmanCd  = model.LastConfirmCd,
                EventCd                = model.EventCd,
                PreviousDayOrderCash   = model.PreviousDayOrderCash,
                SameDayOrderCash       = model.SameDayOrderCash,
                FinalNumberOfCustomers = model.FinalNumberOfCustomers,
                TotalReceivedCash      = model.TotalReceivedCash,
                TotalDownPayment       = model.TotalDownPayment,
                InsertDate             = DateTime.Now,
                InsertUserId           = userId
            };


            if (model.OrderReportCashes.Any())
            {
                foreach (var orderCash in model.OrderReportCashes)
                {
                    if (orderReport.OrderReportCashes == null)
                    {
                        orderReport.OrderReportCashes = new List<OrderReportCash>();
                    }
                    orderReport.OrderReportCashes.Add(new OrderReportCash
                    {
                        OrderDate     = orderCash.OrderDate,
                        CustomerName  = orderCash.CustomerName,
                        AmountOfMoney = orderCash.AmountOfMoney,
                        ReceiptNo     = !string.IsNullOrEmpty(orderCash.ReceiptNo) ? orderCash.ReceiptNo.Trim() : orderCash.ReceiptNo,
                        InsertDate    = DateTime.Now,
                        InsertUserId  = userId
                    });
                }
            }

            if (model.OrderReportDetailRows.Any())
            {
                foreach (var orderReportDetail in model.OrderReportDetailRows)
                {
                    if (orderReport.OrderReportDetails == null)
                    {
                        orderReport.OrderReportDetails = new List<OrderReportDetail>();
                    }
                    orderReport.OrderReportDetails.Add(new OrderReportDetail
                    {
                        MediaCd           = orderReportDetail.MediaCd,
                        VerifyNumber      = orderReportDetail.VerifyNumber?.Trim(),
                        LeftPaymentMethod = orderReportDetail.LeftPaymentMethodCd,
                        SalesmanSCd       = orderReportDetail.SalesmanSCd,
                        SalesmanCCd       = orderReportDetail.SalesmanCCd,
                        SalesmanACd       = orderReportDetail.SalesmanACd,
                        Name              = orderReportDetail.Name?.Trim(),
                        NameFuri          = orderReportDetail.NameFuri?.Trim(),
                        // ReSharper disable once PossibleInvalidOperationException
                        Zipcode           = orderReportDetail.Zipcode,
                        HomePhone         = orderReportDetail.HomePhone,
                        AuthorName        = orderReportDetail.AuthorName,
                        ProductName       = orderReportDetail.ProductName,
                        Item              = orderReportDetail.Item,
                        Price             = orderReportDetail.Price,
                        Discount          = orderReportDetail.Discount,
                        TaxPrice          = orderReportDetail.TaxPrice,
                        ReceivedCash      = orderReportDetail.ReceivedCash,
                        DownPayment       = orderReportDetail.DownPayment,
                        DownPaymentMethod = orderReportDetail.DownPaymentMethodCd,
                        CashKeeper        = orderReportDetail.CashKeeper?.Trim(),
                        AvClub            = orderReportDetail.AvClub,
                        NumberOfVisit     = orderReportDetail.NumberOfVisit,
                        DeliverDate       = orderReportDetail.DeliverDate,
                        JxClub            = orderReportDetail.JxClub,
                        Area              = orderReportDetail.Area,
                        Remarks            = orderReportDetail.RemarkString,
                        CashVoucherValue = orderReportDetail.CashVoucherValueString,
                        ContractCd = orderReportDetail.ContractCd,
                        ProductNo = orderReportDetail.ProductNo,
                        InsertUserId      = userId,
                        InsertDate        = DateTime.Now
                    });
                }
            }

            _context.OrderReports.Add(orderReport);
            _context.SaveChanges();
            return orderReport.Cd;
        }

        public OrderReportDetailModel GetOrderReportDetail(ModalSearchModel model)
        {
            var entity = _context.OrderReports
                .Include(x => x.PersonInCharge)
                .Include(x => x.LastConfirmSalesman)
                .Include(x => x.Event)
                .Include(x => x.OrderReportCashes)
                .Include(x => x.OrderReportDetails)
                    .ThenInclude(x => x.SalesmanA)
                .Include(x => x.OrderReportDetails)
                    .ThenInclude(x => x.SalesmanC)
                .Include(x => x.OrderReportDetails)
                    .ThenInclude(x => x.SalesmanS)
                .Include(x => x.OrderReportDetails)
                    .ThenInclude(x => x.Media)
                .FirstOrDefault(x => x.Cd == model.Cd);
            if (entity == null) return new OrderReportDetailModel();
            {
                var result = new OrderReportDetailModel
                {
                    Cd                     = model.Cd,
                    HoldTime               = entity.HoldTime,
                    PersonInCharge         = entity.PersonInCharge?.Name,
                    PersonInChargeCd       = entity.PersonInChargeCd,
                    LastConfirm            = entity.LastConfirmSalesman?.Name,
                    LastConfirmCd          = entity.LastConfirmSalesmanCd,
                    EventName              = entity.Event?.Name,
                    PreviousDayOrderCash   = entity.PreviousDayOrderCash,
                    SameDayOrderCash       = entity.SameDayOrderCash,
                    FinalNumberOfCustomers = entity.FinalNumberOfCustomers,
                    TotalReceivedCash      = entity.TotalReceivedCash ?? 0,
                    TotalDownPayment       = entity.TotalDownPayment ?? 0
                };
                result.PeriodTime = entity.Event.StartDate.HasValue && entity.Event.EndDate.HasValue
                                    ? $"{CommonExtension.GetWeekOfMonth(entity.Event.StartDate.Value, entity.Event.EndDate.Value)}【{result.EventName}】"
                                    : "";
                
                result.OrderReportCashes = entity.OrderReportCashes.OrderBy(rc => rc.OrderDate).ThenBy(rc => rc.Cd)
                    .Select(x => new ViewModels.Admin.OrderReport.OrderReportCash()
                    {
                        Cd              = x.Cd,
                        OrderDate       = x.OrderDate,
                        OrderDateString = x.OrderDate.ToString(ExactDateFormat),
                        CustomerName    = x.CustomerName,
                        AmountOfMoney   = x.AmountOfMoney,
                        ReceiptNo       = x.ReceiptNo
                    }).ToList();

                result.OrderReportDetailRows = entity.OrderReportDetails.OrderBy(rd => rd.ContractCd).ThenBy(rd => rd.ProductNo)
                    .Select(x => new OrderReportDetailRowModel()
                    {
                        Cd                    = x.Cd,
                        VerifyNumber          = x.VerifyNumber,
                        LeftPaymentMethodName = x.LeftPaymentMethod != null ? ((LeftPaymentMethod)x.LeftPaymentMethod).GetEnumDescription() : string.Empty,
                        SalesmanACd           = x.SalesmanACd,
                        SalesmanACode = x.SalesmanA?.Code,
                        SalesmanAName         = x.SalesmanA?.Name,
                        SalesmanCCd           = x.SalesmanCCd,
                        SalesmanCCode = x.SalesmanC?.Code,
                        SalesmanCName         = x.SalesmanC?.Name,
                        SalesmanSCd           = x.SalesmanSCd,
                        SalesmanSCode = x.SalesmanS?.Code,
                        SalesmanSName         = x.SalesmanS?.Name,
                        Name                  = x.Name,
                        NameFuri              = x.NameFuri,
                        Zipcode               = x.Zipcode,
                        HomePhone             = x.HomePhone,
                        AuthorName            = x.AuthorName,
                        ProductName           = x.ProductName,
                        Item                  = x.Item,
                        Price                 = x.Price ?? 0,
                        Discount              = x.Discount ?? 0,
                        TaxPrice              = x.TaxPrice ?? 0,
                        DownPayment           = x.DownPayment ?? 0,
                        ReceivedCash          = x.ReceivedCash ?? 0,
                        DownPaymentMethodName = x.DownPaymentMethod != null ? ((DownPaymentMethod)x.DownPaymentMethod).GetEnumDescription() : string.Empty,
                        CashKeeper            = x.CashKeeper,
                        AvClubName            = x.AvClub != null ? ((Club)x.AvClub).GetEnumDescription() : string.Empty,
                        AvClub                = x.AvClub,
                        MediaName             = string.Concat(x.Media?.Code, " - ", x.Media?.Name),
                        MediaCode             = x.Media?.Code,
                        NumberOfVisitString   = x.NumberOfVisit != null ? ((NumberOfVisit)x.NumberOfVisit).GetEnumDescription() : string.Empty,
                        NumberOfVisit         = x.NumberOfVisit.Value,
                        DeliverDate           = x.DeliverDate,
                        DeliverDateString     = x.DeliverDate.Value.ToString(ExactDateFormat),
                        JxClubName            = x.JxClub != null ? ((Club)x.JxClub).GetEnumDescription() : string.Empty,
                        JxClub                = x.JxClub,
                        Area                  = x.Area,
                        CashVoucherValueString = x.CashVoucherValue,
                        RemarkString          = x.Remarks,
                    }).ToList();
                var index = 0;
                if (result.OrderReportDetailRows.Any())
                {
                    foreach (var row in result.OrderReportDetailRows)
                    {
                        result.TotalPrice               += row.Price;
                        result.TotalPriceDiscount       += row.Discount;
                        result.TotalPriceSale           += row.Price - row.Discount;
                        result.TotalPriceSaleIncludeTax += row.Price - row.Discount + row.TaxPrice;
                        row.No                           = ++index;

                    }
                }

                return result;
            }
        }

        public List<OrderReportDetailModel> GetAllOrderReportsForPreview(int eventCd)
        {
            var result = _context.OrderReports.Where(or => or.EventCd == eventCd && !or.Event.IsDeleted)
                        .OrderBy(or => or.HoldTime)
                        .Select(or => new
                        {
                            or.Cd,
                            or.HoldTime,
                            PersonInCharge = or.PersonInCharge.Name,
                            LastConfirm = or.LastConfirmSalesman.Name,
                            or.EventCd,
                            EventName = or.Event.Name,
                            or.Event.StartDate,
                            or.Event.EndDate,
                            or.PreviousDayOrderCash,
                            or.SameDayOrderCash,
                            or.FinalNumberOfCustomers,
                            or.TotalReceivedCash,
                            or.TotalDownPayment,
                            OrderReportCashes = or.OrderReportCashes.OrderBy(rc => rc.OrderDate).ThenBy(rc => rc.Cd).Select(rc => new
                            {
                                rc.Cd,
                                rc.OrderDate,
                                rc.CustomerName,
                                rc.AmountOfMoney,
                                rc.ReceiptNo,
                            }),
                            OrderReportDetails = or.OrderReportDetails.OrderBy(rd => rd.ContractCd).ThenBy(rd => rd.ProductNo).Select(rd => new
                            {
                                rd.Cd,
                                rd.VerifyNumber,
                                rd.LeftPaymentMethod,
                                SalesmanSCode = rd.SalesmanS.Code,
                                SalesmanSName = rd.SalesmanS.Name,
                                SalesmanCCode = rd.SalesmanC.Code,
                                SalesmanCName = rd.SalesmanC.Name,
                                SalesmanACode = rd.SalesmanA.Code,
                                SalesmanAName = rd.SalesmanA.Name,
                                rd.Name,
                                rd.NameFuri,
                                rd.Zipcode,
                                rd.HomePhone,
                                rd.AuthorName,
                                rd.ProductName,
                                rd.Item,
                                rd.Price,
                                rd.Discount,
                                rd.TaxPrice,
                                rd.DownPayment,
                                rd.ReceivedCash,
                                rd.DownPaymentMethod,
                                rd.CashKeeper,
                                rd.AvClub,
                                MediaName = rd.Media.Name,
                                MediaCode = rd.Media.Code,
                                rd.NumberOfVisit,
                                rd.DeliverDate,
                                rd.JxClub,
                                rd.Area,
                                rd.CashVoucherValue,
                                rd.Remarks,
                            }),
                        })
                        .AsEnumerable()
                        .Select(or => new OrderReportDetailModel
                        {
                            Cd = or.Cd,
                            HoldTime = or.HoldTime,
                            PersonInCharge = or.PersonInCharge,
                            LastConfirm = or.LastConfirm,
                            EventCd = or.EventCd,
                            EventName = or.EventName,
                            PreviousDayOrderCash = or.PreviousDayOrderCash,
                            SameDayOrderCash = or.SameDayOrderCash,
                            FinalNumberOfCustomers = or.FinalNumberOfCustomers,
                            TotalReceivedCash = or.TotalReceivedCash ?? 0,
                            TotalDownPayment = or.TotalDownPayment ?? 0,
                            PeriodTime = or.StartDate.HasValue && or.EndDate.HasValue
                                    ? $"{CommonExtension.GetWeekOfMonth(or.StartDate.Value, or.EndDate.Value)}【{or.EventName}】"
                                    : "",
                            OrderReportCashes = or.OrderReportCashes != null
                                                ? or.OrderReportCashes.Select(rc => new ViewModels.Admin.OrderReport.OrderReportCash
                                                {
                                                    Cd = rc.Cd,
                                                    OrderDate = rc.OrderDate,
                                                    OrderDateString = rc.OrderDate.ToString(ExactDateFormat),
                                                    CustomerName = rc.CustomerName,
                                                    AmountOfMoney = rc.AmountOfMoney,
                                                    ReceiptNo = rc.ReceiptNo,
                                                }).ToList()
                                                : new List<ViewModels.Admin.OrderReport.OrderReportCash>(),
                            OrderReportDetailRows = or.OrderReportDetails != null
                                                ? or.OrderReportDetails.Select(rd => new OrderReportDetailRowModel
                                                {
                                                    Cd = rd.Cd,
                                                    VerifyNumber = rd.VerifyNumber,
                                                    LeftPaymentMethodName = rd.LeftPaymentMethod != null ? ((LeftPaymentMethod)rd.LeftPaymentMethod).GetEnumDescription() : string.Empty,
                                                    SalesmanSCode = rd.SalesmanSCode,
                                                    SalesmanSName = rd.SalesmanSName,
                                                    SalesmanCCode = rd.SalesmanCCode,
                                                    SalesmanCName = rd.SalesmanCName,
                                                    SalesmanACode = rd.SalesmanACode,
                                                    SalesmanAName = rd.SalesmanAName,
                                                    Name = rd.Name,
                                                    NameFuri = rd.NameFuri,
                                                    Zipcode = rd.Zipcode,
                                                    HomePhone = rd.HomePhone,
                                                    AuthorName = rd.AuthorName,
                                                    ProductName = rd.ProductName,
                                                    Item = rd.Item,
                                                    Price = rd.Price ?? 0,
                                                    Discount = rd.Discount ?? 0,
                                                    TaxPrice = rd.TaxPrice ?? 0,
                                                    DownPayment = rd.DownPayment ?? 0,
                                                    ReceivedCash = rd.ReceivedCash ?? 0,
                                                    DownPaymentMethodName = rd.DownPaymentMethod != null ? ((DownPaymentMethod)rd.DownPaymentMethod).GetEnumDescription() : string.Empty,
                                                    CashKeeper = rd.CashKeeper,
                                                    AvClubName = rd.AvClub != null ? ((Club)rd.AvClub).GetEnumDescription() : string.Empty,
                                                    AvClub = rd.AvClub,
                                                    MediaName = string.Concat(rd.MediaCode, " - ", rd.MediaName),
                                                    MediaCode = rd.MediaCode,
                                                    NumberOfVisitString = rd.NumberOfVisit != null ? ((NumberOfVisit)rd.NumberOfVisit).GetEnumDescription() : string.Empty,
                                                    NumberOfVisit = rd.NumberOfVisit.Value,
                                                    DeliverDate = rd.DeliverDate,
                                                    DeliverDateString = rd.DeliverDate.Value.ToString(ExactDateFormat),
                                                    JxClubName = rd.JxClub != null ? ((Club)rd.JxClub).GetEnumDescription() : string.Empty,
                                                    JxClub = rd.JxClub,
                                                    Area = rd.Area,
                                                    CashVoucherValueString = rd.CashVoucherValue,
                                                    RemarkString = rd.Remarks,
                                                }).ToList()
                                                : new List<OrderReportDetailRowModel>(),
                        })
                        .ToList();

            if(result == null || !result.Any())
            {
                return null;
            }

            foreach(var orderReport in result)
            {
                var index = 0;
                if (orderReport.OrderReportDetailRows.Any())
                {
                    foreach (var row in orderReport.OrderReportDetailRows)
                    {
                        orderReport.TotalPrice += row.Price;
                        orderReport.TotalPriceDiscount += row.Discount;
                        orderReport.TotalPriceSale += row.Price - row.Discount;
                        orderReport.TotalPriceSaleIncludeTax += row.Price - row.Discount + row.TaxPrice;
                        row.No = ++index;
                    }
                }
            }

            return result;
        }

        public OrderReportDetailModel GetOrderReportDetailForEdit(ModalSearchModel model)
        {
            var reportInDb = _context.OrderReports
                .Include(x => x.OrderReportCashes)
                .FirstOrDefault(x => x.Cd == model.Cd);

            if (reportInDb == null)
            {
                return new OrderReportDetailModel();
            }

            var result = new OrderReportDetailModel
            {
                Cd = model.Cd,
                HoldTime = reportInDb.HoldTime,
                PersonInChargeCd = reportInDb.PersonInChargeCd,
                LastConfirmCd = reportInDb.LastConfirmSalesmanCd,
                SameDayOrderCash = reportInDb.SameDayOrderCash,
                FinalNumberOfCustomers = reportInDb.FinalNumberOfCustomers,
            };

            result.OrderReportCashes = reportInDb.OrderReportCashes.OrderBy(rc => rc.OrderDate).ThenBy(rc => rc.Cd).Select(x =>
                new ViewModels.Admin.OrderReport.OrderReportCash()
                {
                    Cd = x.Cd,
                    OrderDate = x.OrderDate,
                    OrderDateString = x.OrderDate.ToString(ExactDateFormat),
                    CustomerName = x.CustomerName,
                    AmountOfMoney = x.AmountOfMoney,
                    ReceiptNo = x.ReceiptNo
                }).ToList();

            //Reload contracts
            model.EventCd = reportInDb.EventCd;
            model.HoldTime = reportInDb.HoldTime;
            var latestReportDetail = GetOrderReportForm(model);
            if(latestReportDetail == null)
            {
                return result;
            }

            result.EventName = latestReportDetail.EventName;
            result.PeriodTime = latestReportDetail.PeriodTime;
            result.PreviousDayOrderCash = latestReportDetail.PreviousDayOrderCash;
            result.TotalReceivedCash = latestReportDetail.TotalReceivedCash;
            result.TotalDownPayment = latestReportDetail.TotalDownPayment;
            result.OrderReportDetailRows = latestReportDetail.OrderReportDetailRows;

            var index = 0;
            if (result.OrderReportDetailRows.Any())
            {
                var oldInputtedValues = _context.OrderReportDetails
                                        .Where(od => od.OrderReportCd == model.Cd)
                                        .Select(od => new
                                        {
                                            od.ContractCd,
                                            od.ProductNo,
                                            od.Item,
                                            od.CashKeeper,
                                        })
                                        .ToList();

                foreach (var row in result.OrderReportDetailRows)
                {
                    result.TotalPrice += row.Price;
                    result.TotalPriceDiscount += row.Discount;
                    result.TotalPriceSale += row.Price - row.Discount;
                    result.TotalPriceSaleIncludeTax += row.Price - row.Discount + row.TaxPrice;
                    row.No = ++index;

                    var oldValue = oldInputtedValues.FirstOrDefault(ov => ov.ContractCd == row.ContractCd && ov.ProductNo == row.ProductNo);
                    if(oldValue != null)
                    {
                        row.Item = oldValue.Item;
                        row.CashKeeper = oldValue.CashKeeper;
                    }
                }
            }

            return result;
        }

        public AjaxResponseResultModel<int> UpdateOrderReport(OrderReportDetailModel model, string userId)
        {
            var resultModel = new AjaxResponseResultModel<int>(false, null, 0);

            var strategy = _context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using (var dbTran = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var order = _context.OrderReports.Include(o => o.OrderReportDetails)
                                                        .FirstOrDefault(o => o.Cd == model.Cd);

                        if (order != null)
                        {
                            order.PersonInChargeCd       = model.PersonInChargeCd;
                            order.LastConfirmSalesmanCd  = model.LastConfirmCd;
                            order.PreviousDayOrderCash   = model.PreviousDayOrderCash;
                            order.SameDayOrderCash       = model.SameDayOrderCash;
                            order.FinalNumberOfCustomers = model.FinalNumberOfCustomers;
                            order.TotalReceivedCash = model.TotalReceivedCash;
                            order.TotalDownPayment = model.TotalDownPayment;
                            order.UpdateDate             = DateTime.Now;
                            order.UpdateUserId           = userId;

                            _context.OrderReportDetails.RemoveRange(order.OrderReportDetails);

                            var newOrderReportDetails = new List<OrderReportDetail>();
                            foreach (var orderReportDetail in model.OrderReportDetailRows)
                            {
                                var newOrderDetail = new OrderReportDetail
                                {
                                    OrderReportCd = order.Cd,
                                    MediaCd = orderReportDetail.MediaCd,
                                    VerifyNumber = orderReportDetail.VerifyNumber?.Trim(),
                                    LeftPaymentMethod = orderReportDetail.LeftPaymentMethodCd,
                                    SalesmanSCd = orderReportDetail.SalesmanSCd,
                                    SalesmanCCd = orderReportDetail.SalesmanCCd,
                                    SalesmanACd = orderReportDetail.SalesmanACd,
                                    Name = orderReportDetail.Name?.Trim(),
                                    NameFuri = orderReportDetail.NameFuri?.Trim(),
                                    Zipcode = orderReportDetail.Zipcode,
                                    HomePhone = orderReportDetail.HomePhone,
                                    AuthorName = orderReportDetail.AuthorName,
                                    ProductName = orderReportDetail.ProductName,
                                    Item = orderReportDetail.Item,
                                    Price = orderReportDetail.Price,
                                    Discount = orderReportDetail.Discount,
                                    TaxPrice = orderReportDetail.TaxPrice,
                                    ReceivedCash = orderReportDetail.ReceivedCash,
                                    DownPayment = orderReportDetail.DownPayment,
                                    DownPaymentMethod = orderReportDetail.DownPaymentMethodCd,
                                    CashKeeper = orderReportDetail.CashKeeper?.Trim(),
                                    AvClub = orderReportDetail.AvClub,
                                    NumberOfVisit = orderReportDetail.NumberOfVisit,
                                    DeliverDate = orderReportDetail.DeliverDate,
                                    JxClub = orderReportDetail.JxClub,
                                    Area = orderReportDetail.Area,
                                    Remarks = orderReportDetail.RemarkString,
                                    CashVoucherValue = orderReportDetail.CashVoucherValueString,
                                    ContractCd = orderReportDetail.ContractCd,
                                    ProductNo = orderReportDetail.ProductNo,
                                    InsertUserId = userId,
                                    InsertDate = DateTime.Now,
                                };

                                newOrderReportDetails.Add(newOrderDetail);
                            }

                            _context.OrderReportDetails.AddRange(newOrderReportDetails);

                            foreach (var orderCash in model.OrderReportCashes)
                            {
                                var cash = _context.OrderReportCash.FirstOrDefault(c => c.Cd == orderCash.Cd);

                                if (cash != null)
                                {
                                    if (string.IsNullOrEmpty(orderCash.CustomerName) && orderCash.AmountOfMoney == 0 && string.IsNullOrEmpty(orderCash.ReceiptNo))
                                    {
                                        _context.OrderReportCash.Remove(cash);
                                    }
                                    else
                                    {
                                        cash.OrderDate     = orderCash.OrderDate.Date;
                                        cash.CustomerName  = orderCash.CustomerName;
                                        cash.AmountOfMoney = orderCash.AmountOfMoney;
                                        cash.ReceiptNo     = orderCash.ReceiptNo;
                                        cash.UpdateDate    = DateTime.Now;
                                        cash.UpdateUserId  = userId;

                                        _context.OrderReportCash.Update(cash);
                                    }
                                }
                                else
                                {
                                    var newCash = new OrderReportCash
                                    {
                                        OrderReportCd = model.Cd,
                                        OrderDate     = orderCash.OrderDate.Date,
                                        CustomerName  = orderCash.CustomerName,
                                        AmountOfMoney = orderCash.AmountOfMoney,
                                        ReceiptNo     = orderCash.ReceiptNo,
                                        InsertDate    = DateTime.Now,
                                        InsertUserId  = userId
                                    };

                                    _context.OrderReportCash.Add(newCash);
                                }
                            }
                            _context.OrderReports.Update(order);
                            _context.SaveChanges();
                            dbTran.Commit();

                            resultModel.Status  = true;
                            resultModel.Message = "受注速報の編集に成功しました。";
                            resultModel.Result  = order.Cd;
                            return;
                        }

                        resultModel.Status  = false;
                        resultModel.Message = "この受注速報が見つかりません。";
                    }
                    catch (Exception e)
                    {
                        dbTran.Rollback();
                        resultModel.Status  = false;
                        resultModel.Message = e.Message;
                    }
                }
            });

            return resultModel;
        }

        private List<SelectListItem> GetSelectListGenders(bool isNullItemFirst = false)
        {
            var rs = CommonExtension.ToEnumSelectList<Gender>()
                .Select(d => new SelectListItem
                {
                    Value = d.Value,
                    Text  = d.Name,
                }).ToList();

            if (isNullItemFirst)
            {
                rs.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text  = "",
                });
            }

            return rs;
        }

        public bool IsOrderReportExisted(ModalSearchModel model)
        {
            return _context.OrderReports.Any(or => or.HoldTime.Date == model.HoldTime.Value.Date && or.EventCd == model.EventCd);
        }
    }
}
