using Microsoft.AspNetCore.Mvc;
using OPS.Business;
using OPS.Utility;
using OPS.ViewModels.Admin.PurchaseStatistics;
using OPS.ViewModels.Shared;
using System;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Stimulsoft.Base;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Mvc;

namespace OPS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PurchaseStatisticsController : AdminBaseController
    {
        private IUnitOfWork _unitOfWork { get; }

        public PurchaseStatisticsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult PurchaseStatisticsA()
        {
            var vm = new IndexViewModel
            {
                Type   = (int)Constants.PurchaseType.A,
                Events = _unitOfWork.MasterDataRepository.GetSelectListEvent(true)
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult PurchaseStatisticsB()
        {
            var vm = new IndexViewModel
            {
                Type   = (int)Constants.PurchaseType.B,
                Events = _unitOfWork.MasterDataRepository.GetSelectListEvent(true)
            };

            return View(vm);
        }

        [HttpPost]
        public JsonResult LoadPurchaseStatisticsAByFiltering(IndexViewModel filtering)
        {
            filtering.SortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            filtering.SortDirection  = Request.Form["order[0][dir]"].FirstOrDefault();

            var data = _unitOfWork.PurchaseStatisticsRepository.Load(filtering);
            return Json(new
            {
                data            = data.PurchaseStatistics,
                draw            = filtering.Draw,
                recordsTotal    = data.TotalRows,
                recordsFiltered = data.TotalRowsAfterFiltering
            });
        }

        [HttpGet]
        public IActionResult DetailPurchaseStatistics(SearchFormModel model)
        {
            try
            {
                var result = model.Cd > 0 ? 
                    _unitOfWork.PurchaseStatisticsRepository.GetListOfContractByPurchaseStatisticsCd(model) : 
                    _unitOfWork.PurchaseStatisticsRepository.GetListOfContract(model);

                if (result == null)
                {
                    return Json(false);
                }

                result.Salesmen = _unitOfWork.MasterDataRepository.GetSelectListSalesMen();
                return PartialView(model.Type == (int)Constants.PurchaseType.A ? "Partial/PurchaseStatisticsAPartial" : "Partial/PurchaseStatisticsBPartial", result);
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
        }

        [HttpPost]
        public IActionResult Save(PurchaseStatisticsDetailModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = model.Cd > 0
                    ? _unitOfWork.PurchaseStatisticsRepository.UpdatePurchaseStatistics(model, userId) : 
                    _unitOfWork.PurchaseStatisticsRepository.CreatePurchaseStatistics(model, userId);
                _unitOfWork.SaveChanges();
                var successMessage = model.Cd > 0 ? 
                    (model.PurchaseType == (int) Constants.PurchaseType.A ? "購入者リスト（a）の編集が成功しました。" : "購入者リスト（b）の編集が成功しました。") : 
                (model.PurchaseType == (int)Constants.PurchaseType.A ? "購入者リスト（a）の追加が成功しました。" : "購入者リスト（b）の追加が成功しました。");
                return Json(new AjaxResponseResultModel<int>(true, successMessage, result.Cd));
            }
            catch (Exception e)
            {
                return Json(new AjaxResponseModel(false, e.Message));
            }
            
        }

        public IActionResult PurchaseStatisticsAPrint(SearchFormModel model)
        {
            model.Type = (int) Constants.PurchaseType.A;
            var result             = _unitOfWork.PurchaseStatisticsRepository.GetListOfContractByPurchaseStatisticsCd(model);
            var reportLocation     = "./wwwroot/report/PurchaseStatisticsAReport.mrt";
            var report             = new StiReport();
            StiFontCollection.AddFontFile("./wwwroot/assets/fonts/meiryo/Meiryo W53 Regular.ttf", "Meiryo");
            StiFontCollection.AddFontFile("./wwwroot/assets/fonts/meiryo/MeiryoUI-Bold.ttf", "Meiryo UI");
            report.Load(reportLocation);
            report.ReportName      = string.Concat("購入者リスト集計表 A_", DateTime.Now.ToString(Constants.ExactDateTimeFormat));
            report["EventName"]    = $"【{result.EventName}】";
            report["EventAddress"] = result.EventAddress + (string.IsNullOrEmpty(result.EventPlace) ? string.Empty : $"（{result.EventPlace}）");
            report["PeriodTime"]   = result.PeriodTime;
            report["EventManager"] = result.EventManagerString;
            report["InputPerson"]  = result.InputPersonString;

            var content            = report.Pages["content"];
            var border             = new StiBorder(StiBorderSides.All, Color.Black, 1, StiPenStyle.Solid);
            double top = .2, left = 0, basicWidth = .4, basicHeight = .2;
            var countDays = (result.EndDate - result.StartDate).TotalDays + 1;
            var dynamicWidth = Math.Round(basicWidth + (7 - countDays) * 1.4 / countDays / 3, 2);

            // draw title
            DrawTitleA(content, result.StartDate, result.EndDate, dynamicWidth);

            // draw 1 row (1 media) in table
            foreach (var media in result.MediasTotal)
            {
                media.MaxCell = media.MaxCell < 6 ? 6 : media.MaxCell;
                while (media.MaxCell % 3 != 0)
                {
                    media.MaxCell++;
                }

                var mediaName = new StiText(new RectangleD(left, top, .5, Math.Round((double)media.MaxCell / 3) * .2))
                {
                    Text          = media.MediaName,
                    Font          = new Font("Meiryo", 6),
                    WordWrap      = true,
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(mediaName);

                var mediaCode = new StiText(new RectangleD(left, top + mediaName.Height, .5, basicHeight))
                {
                    Text          = media.MediaCode,
                    Font          = new Font("Meiryo", 6),
                    WordWrap      = true,
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(mediaCode);
                left += mediaName.Width;

                for (var date = result.StartDate; date.Date <= result.EndDate.Date; date = date.AddDays(1))
                {
                    var cell = result.RowDetails.FirstOrDefault(x => x.MediaCd == media.MediaCd && x.HoldDate.Date.CompareTo(date.Date) == 0);
                    var numberOfAttractingCustomers = new StiText(new RectangleD(left, top, 0.2, mediaName.Height + mediaCode.Height))
                    {
                        Text          = (cell?.NumberOfAttractingCustomers ?? 0).ToString(),
                        Font          = new Font("Meiryo", 5),
                        WordWrap      = true,
                        Border        = border,
                        HorAlignment  = StiTextHorAlignment.Center,
                        VertAlignment = StiVertAlignment.Center
                    };
                    content.Components.Add(numberOfAttractingCustomers);

                    var emptyText = new StiText(new RectangleD(left + .2, top + mediaName.Height, dynamicWidth, basicHeight))
                    {
                        Text   = string.Empty,
                        Border = border,
                    };
                    content.Components.Add(emptyText);

                    var numberOfContracts = new StiText(new RectangleD(left + .2 + dynamicWidth, top + mediaName.Height, dynamicWidth, basicHeight))
                    {
                        Text          = (cell?.NumberOfContracts ?? 0).ToString(),
                        Font          = new Font("Meiryo", 5),
                        WordWrap      = true,
                        Border        = border,
                        HorAlignment  = StiTextHorAlignment.Right,
                        VertAlignment = StiVertAlignment.Center,
                        Margins       = new StiMargins(0, 2, 0, 0)
                    };
                    content.Components.Add(numberOfContracts);

                    var totalPriceOfContracts = new StiText(new RectangleD(left + .2 + dynamicWidth * 2, top + mediaName.Height, dynamicWidth, basicHeight))
                    {
                        Text                       = (cell?.TotalPriceOfContracts ?? 0).ToString(Constants.CurrencyFormat),
                        Font                       = new Font("Meiryo", 5),
                        Border                     = border,
                        HorAlignment               = StiTextHorAlignment.Right,
                        VertAlignment              = StiVertAlignment.Center,
                        Margins                    = new StiMargins(0, 2, 0, 0),
                        ShrinkFontToFit            = true,
                        ShrinkFontToFitMinimumSize = 4
                    };
                    content.Components.Add(totalPriceOfContracts);

                    double topPrice = top, leftPrice = left + .2;
                    for (var i = 0; i < media.MaxCell; i++)
                    {
                        var rec   = new RectangleD(leftPrice, topPrice, dynamicWidth, basicHeight);
                        var price = new StiText(rec)
                        {
                            Text          = (cell == null || i >= cell.ContractPrices.Count) ? string.Empty : cell.ContractPrices[i].ToString(Constants.CurrencyFormat),
                            Font          = new Font("Meiryo", 5),
                            WordWrap      = true,
                            Border        = border,
                            HorAlignment  = StiTextHorAlignment.Right,
                            VertAlignment = StiVertAlignment.Center,
                            Margins       = new StiMargins(0, 2, 0, 0)
                        };
                        if ((i + 1) % 3 == 0)
                        {
                            leftPrice -= dynamicWidth * 2;
                            topPrice += .2;
                        }
                        else
                        {
                            leftPrice += dynamicWidth;
                        }

                        content.Components.Add(price);
                    }
                    left += .2 + dynamicWidth * 3;
                }

                var totalAttractingCustomers = new StiText(new RectangleD(left, top, .2, mediaName.Height + mediaCode.Height))
                {
                    Text          = media.TotalAttractingCustomers.ToString(),
                    Font          = new Font("Meiryo", 5),
                    WordWrap      = true,
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(totalAttractingCustomers);
                left += .2;

                var totalContracts = new StiText(new RectangleD(left, top, .2, mediaName.Height + mediaCode.Height))
                {
                    Text          = media.TotalContracts.ToString(),
                    Font          = new Font("Meiryo", 5),
                    WordWrap      = true,
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(totalContracts);
                left += .2;

                var subTotal = new StiText(new RectangleD(left, top, .6, mediaName.Height + mediaCode.Height))
                {
                    Text                       = media.SubTotal.ToString(Constants.CurrencyFormat),
                    Font                       = new Font("Meiryo", 5),
                    Border                     = border,
                    HorAlignment               = StiTextHorAlignment.Right,
                    VertAlignment              = StiVertAlignment.Center,
                    Margins                    = new StiMargins(0, 2, 0, 0),
                    ShrinkFontToFit            = true,
                    ShrinkFontToFitMinimumSize = 4
                };
                content.Components.Add(subTotal);

                left = 0;
                top += mediaName.Height + mediaCode.Height;
            }

            // total of 1 row
            var totalText = new StiText(new RectangleD(left, top, .5, basicHeight))
            {
                Text          = "合計",
                Font          = new Font("Meiryo", 6),
                WordWrap      = true,
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(totalText);
            left += .5;

            for (var date = result.StartDate; date.Date <= result.EndDate.Date; date = date.AddDays(1))
            {
                var cell                        = result.DateTotal.FirstOrDefault(x => date.Date.CompareTo(x.Date.Value.Date) == 0);
                var numberOfAttractingCustomers = new StiText(new RectangleD(left, top, .2, basicHeight))
                {
                    Text          = (cell?.TotalAttractingCustomers ?? 0).ToString(),
                    Font          = new Font("Meiryo", 5),
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(numberOfAttractingCustomers);
                left += .2;

                var emptyText = new StiText(new RectangleD(left, top, dynamicWidth, basicHeight))
                {
                    Text   = string.Empty,
                    Border = border,
                };
                content.Components.Add(emptyText);
                left += dynamicWidth;

                var numberOfContracts = new StiText(new RectangleD(left, top, dynamicWidth, basicHeight))
                {
                    Text          = (cell?.TotalContracts ?? 0).ToString(),
                    Font          = new Font("Meiryo", 5),
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Right,
                    VertAlignment = StiVertAlignment.Center,
                    Margins       = new StiMargins(0, 2, 0, 0)
                };
                content.Components.Add(numberOfContracts);
                left += dynamicWidth;

                var totalPriceOfContracts = new StiText(new RectangleD(left, top, dynamicWidth, basicHeight))
                {
                    Text                       = (cell?.SubTotal ?? 0).ToString(Constants.CurrencyFormat),
                    Font                       = new Font("Meiryo", 5),
                    Border                     = border,
                    HorAlignment               = StiTextHorAlignment.Right,
                    VertAlignment              = StiVertAlignment.Center,
                    Margins                    = new StiMargins(0, 2, 0, 0),
                    ShrinkFontToFit            = true,
                    ShrinkFontToFitMinimumSize = 4
                };
                content.Components.Add(totalPriceOfContracts);
                left += dynamicWidth;
            }

            var sumNumberOfAttractingCustomers = new StiText(new RectangleD(left, top, .2, basicHeight))
            {
                Text          = result.MediasTotal.Sum(x => x.TotalAttractingCustomers).ToString(),
                Font          = new Font("Meiryo", 5),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(sumNumberOfAttractingCustomers);
            left += .2;

            var sumNumberOfContracts = new StiText(new RectangleD(left, top, .2, basicHeight))
            {
                Text          = result.MediasTotal.Sum(x => x.TotalContracts).ToString(),
                Font          = new Font("Meiryo", 5),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(sumNumberOfContracts);
            left += .2;

            var total = new StiText(new RectangleD(left, top, basicWidth + .2, basicHeight))
            {
                Text                       = result.MediasTotal.Sum(x => x.SubTotal).ToString(Constants.CurrencyFormat),
                Font                       = new Font("Meiryo", 5),
                Border                     = border,
                HorAlignment               = StiTextHorAlignment.Right,
                VertAlignment              = StiVertAlignment.Center,
                Margins                    = new StiMargins(0, 2, 0, 0),
                ShrinkFontToFit            = true,
                ShrinkFontToFitMinimumSize = 4
            };
            content.Components.Add(total);

            return StiNetCoreReportResponse.PrintAsPdf(report);
        }

        private void DrawTitleA(StiContainer content, DateTime startDate, DateTime endDate, double dynamicWidth)
        {
            double left = 0, top = 0, basicHeight = .2;
            var border = new StiBorder(StiBorderSides.All, Color.Black, 1, StiPenStyle.Solid);
            var mediaText = new StiText(new RectangleD(left, top, .5, basicHeight))
            {
                Text          = "媒体",
                Font          = new Font("Meiryo", 6),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(mediaText);
            left += .5;

            for (var date = startDate; date.Date <= endDate.Date; date = date.AddDays(1))
            {
                var attractingCustomersText = new StiText(new RectangleD(left, top, .2, basicHeight))
                {
                    Text          = "集客",
                    Font          = new Font("Meiryo", 6),
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(attractingCustomersText);
                left += .2;

                var dateText = new StiText(new RectangleD(left, top, dynamicWidth * 3, basicHeight))
                {
                    Text          = date.ToString(Constants.ExactMonthDayFormatJP),
                    Font          = new Font("Meiryo", 6),
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(dateText);
                left += dateText.Width;
            }

            var totalAttractingCustomersText = new StiText(new RectangleD(left, top, .2, basicHeight))
            {
                Text          = "集客",
                Font          = new Font("Meiryo", 6),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(totalAttractingCustomersText);
            left += .2;

            var totalOfContractMedia = new StiText(new RectangleD(left, top, .2, basicHeight))
            {
                Text          = "本数",
                Font          = new Font("Meiryo", 6),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(totalOfContractMedia);
            left += .2;

            var subTotalMedia = new StiText(new RectangleD(left, top, .6, basicHeight))
            {
                Text          = "小計",
                Font          = new Font("Meiryo", 6),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(subTotalMedia);
        }

        public IActionResult PurchaseStatisticsBPrint(SearchFormModel model)
        {
            model.Type = (int) Constants.PurchaseType.B;
            var result             = _unitOfWork.PurchaseStatisticsRepository.GetListOfContractByPurchaseStatisticsCd(model);
            var reportLocation     = "./wwwroot/report/PurchaseStatisticsBReport.mrt";
            var report             = new StiReport();
            StiFontCollection.AddFontFile("./wwwroot/assets/fonts/meiryo/Meiryo W53 Regular.ttf", "Meiryo");
            StiFontCollection.AddFontFile("./wwwroot/assets/fonts/meiryo/MeiryoUI-Bold.ttf", "Meiryo UI");
            report.Load(reportLocation);
            report.ReportName      = string.Concat("購入者リスト集計表 B_", DateTime.Now.ToString(Constants.ExactDateTimeFormat));
            report["EventName"]    = $"【{result.EventName}】";
            report["EventAddress"] = result.EventAddress + (string.IsNullOrEmpty(result.EventPlace) ? string.Empty : $"（{result.EventPlace}）");
            report["PeriodTime"]   = result.PeriodTime;
            report["EventManager"] = result.EventManagerString;
            report["InputPerson"]  = result.InputPersonString;

            var content            = report.Pages["content"];
            var border             = new StiBorder(StiBorderSides.All, Color.Black, 1, StiPenStyle.Solid);
            double top = .2, left = 0, basicWidth = .4, basicHeight = .2;
            var countDays = (result.EndDate - result.StartDate).TotalDays + 1;
            var dynamicWidth = Math.Round(basicWidth + (7 - countDays) * 1.4 / countDays / 3, 2);

            DrawTitleB(content, result.StartDate, result.EndDate, dynamicWidth);

            foreach (var artist in result.ArtistsTotal)
            {
                artist.MaxCell = artist.MaxCell < 6 ? 6 : artist.MaxCell;
                while (artist.MaxCell % 3 != 0)
                {
                    artist.MaxCell++;
                }

                var artistName = new StiText(new RectangleD(left, top, 1.2, Math.Round((double)artist.MaxCell / 3) * .2 + basicHeight))
                {
                    Text          = artist.ArtistName,
                    Font          = new Font("Meiryo", 6),
                    WordWrap      = true,
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(artistName);
                left += artistName.Width;

                for (var date = result.StartDate; date.Date <= result.EndDate.Date; date = date.AddDays(1))
                {
                    var cell = result.RowDetails.FirstOrDefault(x => x.ArtistCd == artist.ArtistCd && x.HoldDate.Date.CompareTo(date.Date) == 0);

                    var emptyText = new StiText(new RectangleD(left, top + artistName.Height - basicHeight, dynamicWidth, basicHeight))
                    {
                        Text   = string.Empty,
                        Border = border,
                    };
                    content.Components.Add(emptyText);

                    var numberOfContracts = new StiText(new RectangleD(left + dynamicWidth, top + artistName.Height - basicHeight, dynamicWidth, basicHeight))
                    {
                        Text          = (cell?.NumberOfContracts ?? 0).ToString(),
                        Font          = new Font("Meiryo", 5),
                        WordWrap      = true,
                        Border        = border,
                        HorAlignment  = StiTextHorAlignment.Right,
                        VertAlignment = StiVertAlignment.Center,
                        Margins       = new StiMargins(0, 2, 0, 0)
                    };
                    content.Components.Add(numberOfContracts);

                    var totalPriceOfContracts = new StiText(new RectangleD(left + dynamicWidth * 2, top + artistName.Height - basicHeight, dynamicWidth, basicHeight))
                    {
                        Text                       = (cell?.TotalPriceOfContracts ?? 0).ToString(Constants.CurrencyFormat),
                        Font                       = new Font("Meiryo", 5),
                        Border                     = border,
                        HorAlignment               = StiTextHorAlignment.Right,
                        VertAlignment              = StiVertAlignment.Center,
                        Margins                    = new StiMargins(0, 2, 0, 0),
                        ShrinkFontToFit            = true,
                        ShrinkFontToFitMinimumSize = 4
                    };
                    content.Components.Add(totalPriceOfContracts);

                    double topPrice = top, leftPrice = left;
                    for (var i = 0; i < artist.MaxCell; i++)
                    {
                        var rec   = new RectangleD(leftPrice, topPrice, dynamicWidth, basicHeight);
                        var price = new StiText(rec)
                        {
                            Text          = (cell == null || i >= cell.ContractPrices.Count) ? string.Empty : cell.ContractPrices[i].ToString(Constants.CurrencyFormat),
                            Font          = new Font("Meiryo", 5),
                            WordWrap      = true,
                            Border        = border,
                            HorAlignment  = StiTextHorAlignment.Right,
                            VertAlignment = StiVertAlignment.Center,
                            Margins       = new StiMargins(0, 2, 0, 0)
                        };
                        if ((i + 1) % 3 == 0)
                        {
                            leftPrice -= dynamicWidth * 2;
                            topPrice += .2;
                        }
                        else
                        {
                            leftPrice += dynamicWidth;
                        }

                        content.Components.Add(price);
                    }

                    left += dynamicWidth * 3;
                }

                var totalContracts = new StiText(new RectangleD(left, top, .6, artistName.Height))
                {
                    Text          = artist.TotalContracts.ToString(),
                    Font          = new Font("Meiryo", 5),
                    WordWrap      = true,
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(totalContracts);
                left += .6;

                var subTotal = new StiText(new RectangleD(left, top, 1.1, artistName.Height))
                {
                    Text                       = artist.SubTotal.ToString(Constants.CurrencyFormat),
                    Font                       = new Font("Meiryo", 5),
                    Border                     = border,
                    HorAlignment               = StiTextHorAlignment.Right,
                    VertAlignment              = StiVertAlignment.Center,
                    Margins                    = new StiMargins(0, 2, 0, 0),
                    ShrinkFontToFit            = true,
                    ShrinkFontToFitMinimumSize = 4
                };
                content.Components.Add(subTotal);

                left = 0;
                top += artistName.Height;
            }

            var totalText = new StiText(new RectangleD(left, top, 1.2, basicHeight))
            {
                Text          = "合計",
                Font          = new Font("Meiryo", 6),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(totalText);
            left += 1.2;

            for (var date = result.StartDate; date.Date <= result.EndDate.Date; date = date.AddDays(1))
            {
                var cell = result.DateTotal.FirstOrDefault(x => date.Date.CompareTo(x.Date.Value.Date) == 0);
                var emptyText = new StiText(new RectangleD(left, top, dynamicWidth, basicHeight))
                {
                    Text   = string.Empty,
                    Border = border,
                };
                content.Components.Add(emptyText);
                left += dynamicWidth;

                var numberOfContracts = new StiText(new RectangleD(left, top, dynamicWidth, basicHeight))
                {
                    Text          = (cell?.TotalContracts ?? 0).ToString(),
                    Font          = new Font("Meiryo", 5),
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Right,
                    VertAlignment = StiVertAlignment.Center,
                    Margins       = new StiMargins(0, 2, 0, 0)
                };
                content.Components.Add(numberOfContracts);
                left += dynamicWidth;

                var totalPriceOfContracts = new StiText(new RectangleD(left, top, dynamicWidth, basicHeight))
                {
                    Text                       = (cell?.SubTotal ?? 0).ToString(Constants.CurrencyFormat),
                    Font                       = new Font("Meiryo", 5),
                    Border                     = border,
                    HorAlignment               = StiTextHorAlignment.Right,
                    VertAlignment              = StiVertAlignment.Center,
                    Margins                    = new StiMargins(0, 2, 0, 0),
                    ShrinkFontToFit            = true,
                    ShrinkFontToFitMinimumSize = 4
                };
                content.Components.Add(totalPriceOfContracts);
                left += dynamicWidth;
            }

            var sumNumberOfContracts = new StiText(new RectangleD(left, top, .6, basicHeight))
            {
                Text          = result.ArtistsTotal.Sum(x => x.TotalContracts).ToString(),
                Font          = new Font("Meiryo", 5),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(sumNumberOfContracts);
            left += sumNumberOfContracts.Width;

            var total = new StiText(new RectangleD(left, top, 1.1, basicHeight))
            {
                Text                       = result.ArtistsTotal.Sum(x => x.SubTotal).ToString(Constants.CurrencyFormat),
                Font                       = new Font("Meiryo", 5),
                Border                     = border,
                HorAlignment               = StiTextHorAlignment.Right,
                VertAlignment              = StiVertAlignment.Center,
                Margins                    = new StiMargins(0, 2, 0, 0),
                ShrinkFontToFit            = true,
                ShrinkFontToFitMinimumSize = 4
            };
            content.Components.Add(total);

            return StiNetCoreReportResponse.PrintAsPdf(report);
        }
        
        private void DrawTitleB(StiContainer content, DateTime startDate, DateTime endDate, double dynamicWidth)
        {
            double left    = 0, top = 0, basicHeight = .2;
            var border     = new StiBorder(StiBorderSides.All, Color.Black, 1, StiPenStyle.Solid);
            var artistText = new StiText(new RectangleD(left, top, 1.2, basicHeight))
            {
                Text          = "作家",
                Font          = new Font("Meiryo", 6),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(artistText);
            left += 1.2;

            for (var date = startDate; date.Date <= endDate.Date; date = date.AddDays(1))
            {
                var dateText = new StiText(new RectangleD(left, top, dynamicWidth * 3, basicHeight))
                {
                    Text          = date.ToString(Constants.ExactMonthDayFormatJP),
                    Font          = new Font("Meiryo", 6),
                    Border        = border,
                    HorAlignment  = StiTextHorAlignment.Center,
                    VertAlignment = StiVertAlignment.Center
                };
                content.Components.Add(dateText);
                left += dateText.Width;
            }

            var totalOfContractArtist = new StiText(new RectangleD(left, top, .6, basicHeight))
            {
                Text          = "本数",
                Font          = new Font("Meiryo", 6),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(totalOfContractArtist);
            left += .6;

            var subTotalArtist = new StiText(new RectangleD(left, top, 1.1, basicHeight))
            {
                Text          = "小計",
                Font          = new Font("Meiryo", 6),
                Border        = border,
                HorAlignment  = StiTextHorAlignment.Center,
                VertAlignment = StiVertAlignment.Center
            };
            content.Components.Add(subTotalArtist);
        }
    }
}
