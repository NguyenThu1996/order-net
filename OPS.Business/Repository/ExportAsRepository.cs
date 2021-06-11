using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using OPS.Utility.Extensions;
using OPS.ViewModels.Admin.ExportFileAS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static OPS.Utility.Constants;

namespace OPS.Business.Repository
{
    public class ExportAsRepository : IExportASRepository
    {
        protected readonly OpsContext _context;
        List<string> lstContent = new List<string>();
        public ExportAsRepository(OpsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Method to get List Contract And call Method ExportFileAs to Export CSV
        /// </summary>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public CsvExport ExportFileAsFilter(ExportFileASFilterModel filterModel, out List<string> listErrorEncode, out List<int> listUpdateFlagCSV)
        {
            try
            {
                IQueryable<Contract> contracts;

                if (filterModel.ExportMode == (int)CsvExportMode.Active)
                {
                    contracts = _context.Contracts.Where(x => x.IsCompleted && !x.IsDeleted && (filterModel.IsIncludeExported || !x.FlagCsv));
                }
                else
                {
                    contracts = _context.Contracts.Where(x => (x.IsDeleted || x.IsEdited) && x.FlagCsv && (filterModel.IsIncludeExported || !x.FlagCsvDeleted));
                }

                if (filterModel.ContractDateFrom != null)
                {
                    contracts = contracts.Where(x => x.ContractDate.Value.Date >= filterModel.ContractDateFrom.Value.Date);
                }

                if (filterModel.ContractDateTo != null)
                {
                    contracts = contracts.Where(x => x.ContractDate.Value.Date <= filterModel.ContractDateTo.Value.Date);
                }

                if (filterModel.EventCd != null)
                {
                    contracts = contracts.Where(x => x.Event.Cd == filterModel.EventCd);
                }

                List<ExportFileASModel> listExportFileAsModel = GetListContractExportAs(contracts);

                if (!listExportFileAsModel.Any())
                {
                    listErrorEncode = null;
                    listUpdateFlagCSV = null;

                    return null;
                }

                return ExportFileAs(listExportFileAsModel, out listErrorEncode, out listUpdateFlagCSV);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Method Export CSV File AS
        /// </summary>
        /// <param name="listExportFileAS"></param>
        /// <returns></returns>
        public CsvExport ExportFileAs(List<ExportFileASModel> listExportFileAS, out List<string> listErrorEncode, out List<int> listUpdateFlagCSV)
        {
            CsvExport csvExport = new CsvExport(",", false);

            //record header
            AddRowData(csvExport, new[] {
                 "契約ID",
                 "販売組織選択（営業部・ｅ・ジュネックス事業部）",
                 "催事CD",
                 "会場名",
                 "営業担当S",
                 "営業担当A",
                 "営業担当C",
                 "所属",
                 "契約日",
                 "お客様名（フリ仮名）",
                 "お客様名（漢字）",
                 "性別CD",
                 "性別",
                 "生年月日",
                 "歳",
                 "〒番号",
                 "住所1（フリ仮名）",
                 "住所2（フリ仮名）",
                 "住所3（フリ仮名）",
                 "住所1（漢字）",
                 "住所2（漢字）",
                 "住所3（漢字）",
                 "自宅電話番号",
                 "携帯電話番号",
                 "勤務先名称（フリ仮名）",
                 "勤務先名称（漢字等）",
                 "勤務先〒番号",
                 "勤務先住所1（フリ仮名）",
                 "勤務先住所2（フリ仮名）",
                 "勤務先住所3（フリ仮名）",
                 "勤務先住所1（漢字）",
                 "勤務先住所2（漢字）",
                 "勤務先住所3（漢字）",
                 "勤務先電話番号",
                 "勤務先内線",

                 "作家コード1",
                 "作家名/アイテム1",
                 "作品コード1",
                 "アイテム区分1",
                 "作品名1",
                 "技法1",
                 "数量1",
                 "税抜価格1",
                 "値引き金額1",
                 "消費税1",
                 "備考欄1",
                 "備考欄1",
                 "備考欄1",
                 "備考欄1",
                 "金券1",

                 "作家コード2",
                 "作家名/アイテム2",
                 "作品コード2",
                 "アイテム区分2",
                 "作品名2",
                 "技法2",
                 "数量2",
                 "税抜価格2",
                 "値引き金額2",
                 "消費税2",
                 "備考欄2",
                 "備考欄2",
                 "備考欄2",
                 "備考欄2",
                 "金券2",

                 "作家コード3",
                 "作家名/アイテム3",
                 "作品コード3",
                 "アイテム区分3",
                 "作品名3",
                 "技法3",
                 "数量3",
                 "税抜価格3",
                 "値引き金額3",
                 "消費税3",
                 "備考欄3",
                 "備考欄3",
                 "備考欄3",
                 "備考欄3",
                 "金券3",

                 "作家コード4",
                 "作家名/アイテム4",
                 "作品コード4",
                 "アイテム区分4",
                 "作品名4",
                 "技法4",
                 "数量4",
                 "税抜価格4",
                 "値引き金額4",
                 "消費税4",
                 "備考欄4",
                 "備考欄4",
                 "備考欄4",
                 "備考欄4",
                 "金券4",

                 "作家コード5",
                 "作家名/アイテム5",
                 "作品コード5",
                 "アイテム区分5",
                 "作品名5",
                 "技法5",
                 "数量5",
                 "税抜価格5",
                 "値引き金額5",
                 "消費税5",
                 "備考欄5",
                 "備考欄5",
                 "備考欄5",
                 "備考欄5",
                 "金券5",

                 "作家コード6",
                 "作家名/アイテム6",
                 "作品コード6",
                 "アイテム区分6",
                 "作品名6",
                 "技法6",
                 "数量6",
                 "税抜価格6",
                 "値引き金額6",
                 "消費税6",
                 "備考欄6",
                 "備考欄6",
                 "備考欄6",
                 "備考欄6",
                 "金券6",

                 "合計金額",
                 "頭金",
                 "残金",
                 "頭金支払方法（現金・銀行振込）",
                 "頭金支払予定年月日",
                 "領収書NO",
                 "残金支払予定年月日",
                 "残金支払方法（信販・カード・現金）",
                 "残金支払先（各信販会社・各カード会社）",
                 "オリコの場合のみ、照合管理番号",
                 "納期（年/月/日・AM/PM）",
                 "配送先（自宅・勤務先・実家・その他）",
                 "ID友の会入会（AVする・しない/EJする・しない）",
                 "ID友の会入会絵柄指定（ラッセン・天野・ディズニー・ｅ・ジュネックスメンバーシップ）",
                 "本人確認資料（免許証・保険証・旅券・他（））",
                 "備考　値引き承認者/配送先承認依頼/額装書類有無/その他販売承認/契約書後送り等（フリー入力）",
                 "媒体コード",
                 "枝番",
                 "同意書有無",
                 "同意書有の場合書類名"
            });

            int rowNumber = 2;
            var errorList = new List<string>();
            var listUpdateFlagCSVTemp = new List<int>();

            //record data
            foreach (var item in listExportFileAS)
            {
                string outMessage = string.Empty;
                string outMessageByteChecked = string.Empty;
                string itemResult = string.Empty;
                int length = 256;
                listUpdateFlagCSVTemp.Add(item.ContractCd);

                // Check Encode Error
                if (CheckEncodeError(rowNumber, item.SaleDepartment, "販売組織選択", out outMessage, out itemResult, length))
                {
                    item.SaleDepartment = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.EventName, "会場名", out outMessage, out itemResult, length))
                {
                    item.EventName = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.Organiazation, "所属", out outMessage, out itemResult, length))
                {
                    item.Organiazation = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CustomerNameFuri, "お客様名（フリ仮名）", out outMessage, out itemResult, length))
                {
                    item.CustomerNameFuri = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CustomerName, "お客様名（漢字）", out outMessage, out itemResult, length))
                {
                    item.CustomerName = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CustomerAddressFuri1, "住所1（フリ仮名）", out outMessage, out itemResult, length))
                {
                    item.CustomerAddressFuri1 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CustomerAddressFuri2, "住所2（フリ仮名）", out outMessage, out itemResult, length))
                {
                    item.CustomerAddressFuri2 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CustomerAddressFuri3, "住所3（フリ仮名）", out outMessage, out itemResult, length))
                {
                    item.CustomerAddressFuri3 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CustomerAddress1, "住所1（漢字）", out outMessage, out itemResult, length))
                {
                    item.CustomerAddress1 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CustomerAddress2, "住所2（漢字）", out outMessage, out itemResult, length))
                {
                    item.CustomerAddress2 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CustomerAddress3, "住所3（漢字）", out outMessage, out itemResult, length))
                {
                    item.CustomerAddress3 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CompanyNameFuri, "勤務先名称（フリ仮名）", out outMessage, out itemResult, length))
                {
                    item.CompanyNameFuri = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CompanyName, "勤務先名称（漢字等）", out outMessage, out itemResult, length))
                {
                    item.CompanyName = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CompanyAddressFuri1, "勤務先住所1（フリ仮名）", out outMessage, out itemResult, length))
                {
                    item.CompanyAddressFuri1 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CompanyAddressFuri2, "勤務先住所2（フリ仮名）", out outMessage, out itemResult, length))
                {
                    item.CompanyAddressFuri2 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CompanyAddressFuri3, "勤務先住所3（フリ仮名）", out outMessage, out itemResult, length))
                {
                    item.CompanyAddressFuri3 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CompanyAddress1, "勤務先住所1（漢字）", out outMessage, out itemResult, length))
                {
                    item.CompanyAddress1 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CompanyAddress2, "勤務先住所2（漢字）", out outMessage, out itemResult, length))
                {
                    item.CompanyAddress2 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.CompanyAddress3, "勤務先住所3（漢字）", out outMessage, out itemResult, length))
                {
                    item.CompanyAddress3 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.Artist1, "作家名/アイテム1", out outMessage, out itemResult, length))
                {
                    item.Artist1 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ProductName1, "作品名1", out outMessage, out itemResult, length))
                {
                    item.ProductName1 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.TechniqueName1, "技法1", out outMessage, out itemResult, length))
                {
                    item.TechniqueName1 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.Artist2, "作家名/アイテム2", out outMessage, out itemResult, length))
                {
                    item.Artist2 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ProductName2, "作品名2", out outMessage, out itemResult, length))
                {
                    item.ProductName2 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.TechniqueName2, "技法2", out outMessage, out itemResult, length))
                {
                    item.TechniqueName2 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.Artist3, "作家名/アイテム3", out outMessage, out itemResult, length))
                {
                    item.Artist3 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ProductName3, "作品名3", out outMessage, out itemResult, length))
                {
                    item.ProductName3 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.TechniqueName3, "技法3", out outMessage, out itemResult, length))
                {
                    item.TechniqueName3 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.Artist4, "作家名/アイテム4", out outMessage, out itemResult, length))
                {
                    item.Artist4 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ProductName4, "作品名4", out outMessage, out itemResult, length))
                {
                    item.ProductName4 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.TechniqueName4, "技法4", out outMessage, out itemResult, length))
                {
                    item.TechniqueName4 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.Artist5, "作家名/アイテム5", out outMessage, out itemResult, length))
                {
                    item.Artist5 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ProductName5, "作品名5", out outMessage, out itemResult, length))
                {
                    item.ProductName5 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.TechniqueName5, "技法5", out outMessage, out itemResult, length))
                {
                    item.TechniqueName5 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.Artist6, "作家名/アイテム6", out outMessage, out itemResult, length))
                {
                    item.Artist6 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ProductName6, "作品名6", out outMessage, out itemResult, length))
                {
                    item.ProductName6 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.TechniqueName6, "技法6", out outMessage, out itemResult, length))
                {
                    item.TechniqueName6 = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.DownPaymentMethod, "頭金支払方法（現金・銀行振込）", out outMessage, out itemResult, length))
                {
                    item.DownPaymentMethod = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ReceiptNo, "領収書NO", out outMessage, out itemResult, length))
                {
                    item.ReceiptNo = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.LeftPaymentMethod, "残金支払方法", out outMessage, out itemResult, length))
                {
                    item.LeftPaymentMethod = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.LeftPaymentPlace, "残金支払先", out outMessage, out itemResult, length))
                {
                    item.LeftPaymentPlace = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.VerifyNumer, "オリコの場合のみ、照合管理番号", out outMessage, out itemResult, length))
                {
                    item.VerifyNumer = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ClubRegistrationStatus, "ID友の会入会", out outMessage, out itemResult, length))
                {
                    item.ClubRegistrationStatus = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.MemberCard, "ID友の会入会絵柄指定", out outMessage, out itemResult, length))
                {
                    item.MemberCard = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.Remark, "備考", out outMessage, out itemResult, length))
                {
                    item.Remark = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.IsHaveProductAgreement, "同意書有無", out outMessage, out itemResult, length))
                {
                    item.IsHaveProductAgreement = itemResult;
                    errorList.Add(outMessage);
                }

                if (CheckEncodeError(rowNumber, item.ProductAgreementType, "同意書有の場合書類名", out outMessage, out itemResult, length))
                {
                    item.ProductAgreementType = itemResult;
                    errorList.Add(outMessage);
                }

                AddRowData(csvExport, new[] {
                    item.ContractId,
                    item.SaleDepartment,
                    item.EventCode,
                    item.EventName,
                    item.SalesManSCode,
                    item.SalesManCCode,
                    item.SalesManACode,
                    item.Organiazation,
                    item.OrderDate,
                    item.CustomerNameFuri,
                    item.CustomerName,
                    item.GenderCd,
                    item.Gender,
                    item.DateOfBirth,
                    item.Age,
                    item.ZipCode,
                    item.CustomerAddressFuri1,
                    item.CustomerAddressFuri2,
                    item.CustomerAddressFuri3,
                    item.CustomerAddress1,
                    item.CustomerAddress2,
                    item.CustomerAddress3,
                    item.HomePhone,
                    item.MobilePhone,
                    item.CompanyNameFuri,
                    item.CompanyName,
                    item.CompanyZipcode,
                    item.CompanyAddressFuri1,
                    item.CompanyAddressFuri2,
                    item.CompanyAddressFuri3,
                    item.CompanyAddress1,
                    item.CompanyAddress2,
                    item.CompanyAddress3,
                    item.CompanyPhone,
                    item.CompanyLocalPhone,
                    //Product 1
                    item.ArtistCode1,
                    item.Artist1,
                    item.ProductCode1,
                    item.ProductCategory1,
                    item.ProductName1,
                    item.TechniqueName1,
                    item.ProductQuantity1,
                    item.ProductPrice1,
                    item.ProductDiscount1,
                    item.ProductTaxPrice1,
                    item.ProductRemarkA1,
                    item.ProductRemarkB1,
                    item.ProductRemarkC1,
                    item.ProductRemarkD1,
                    item.CashVoucherValue1,
                    //Product 2
                    item.ArtistCode2,
                    item.Artist2,
                    item.ProductCode2,
                    item.ProductCategory2,
                    item.ProductName2,
                    item.TechniqueName2,
                    item.ProductQuantity2,
                    item.ProductPrice2,
                    item.ProductDiscount2,
                    item.ProductTaxPrice2,
                    item.ProductRemarkA2,
                    item.ProductRemarkB2,
                    item.ProductRemarkC2,
                    item.ProductRemarkD2,
                    item.CashVoucherValue2,
                    //Product 3
                    item.ArtistCode3,
                    item.Artist3,
                    item.ProductCode3,
                    item.ProductCategory3,
                    item.ProductName3,
                    item.TechniqueName3,
                    item.ProductQuantity3,
                    item.ProductPrice3,
                    item.ProductDiscount3,
                    item.ProductTaxPrice3,
                    item.ProductRemarkA3,
                    item.ProductRemarkB3,
                    item.ProductRemarkC3,
                    item.ProductRemarkD3,
                    item.CashVoucherValue3,
                    //Product 4
                    item.ArtistCode4,
                    item.Artist4,
                    item.ProductCode4,
                    item.ProductCategory4,
                    item.ProductName4,
                    item.TechniqueName4,
                    item.ProductQuantity4,
                    item.ProductPrice4,
                    item.ProductDiscount4,
                    item.ProductTaxPrice4,
                    item.ProductRemarkA4,
                    item.ProductRemarkB4,
                    item.ProductRemarkC4,
                    item.ProductRemarkD4,
                    item.CashVoucherValue4,
                    //Product 5
                    item.ArtistCode5,
                    item.Artist5,
                    item.ProductCode5,
                    item.ProductCategory5,
                    item.ProductName5,
                    item.TechniqueName5,
                    item.ProductQuantity5,
                    item.ProductPrice5,
                    item.ProductDiscount5,
                    item.ProductTaxPrice5,
                    item.ProductRemarkA5,
                    item.ProductRemarkB5,
                    item.ProductRemarkC5,
                    item.ProductRemarkD5,
                    item.CashVoucherValue5,
                    //Product 6
                    item.ArtistCode6,
                    item.Artist6,
                    item.ProductCode6,
                    item.ProductCategory6,
                    item.ProductName6,
                    item.TechniqueName6,
                    item.ProductQuantity6,
                    item.ProductPrice6,
                    item.ProductDiscount6,
                    item.ProductTaxPrice6,
                    item.ProductRemarkA6,
                    item.ProductRemarkB6,
                    item.ProductRemarkC6,
                    item.ProductRemarkD6,
                    item.CashVoucherValue6,
                    item.TotalPrice,
                    item.DownPayment,
                    item.LeftPayment,
                    item.DownPaymentMethod,
                    item.DownPaymentDate,
                    item.ReceiptNo,
                    item.LeftPaymentDate,
                    item.LeftPaymentMethod,
                    item.LeftPaymentPlace,
                    item.VerifyNumer,
                    item.DeliveryDate,
                    item.DeliveryPlace,
                    item.ClubRegistrationStatus,
                    item.MemberCard,
                    item.IdentifyDocument,
                    item.Remark,
                    item.MediaCode,
                    item.BranchCode,
                    item.IsHaveProductAgreement,
                    item.ProductAgreementType
                });

                rowNumber++;

            }

            listErrorEncode = errorList;
            listUpdateFlagCSV = listUpdateFlagCSVTemp;

            return csvExport;
        }

        /// <summary>
        /// Method to add Row data to CSV
        /// </summary>
        /// <param name="csvExport"></param>
        /// <param name="data"></param>
        private void AddRowData(CsvExport csvExport, string[] data)
        {
            csvExport.AddRow();
            for (int i = 0; i < data.Length; i++)
            {
                var strData = string.IsNullOrEmpty(data[i]) ? string.Empty : data[i].ToUpper();
                csvExport[(i + 1).ToString()] = strData;
            }
        }

        /// <summary>
        /// Method to get name of clubRegistrationStatus
        /// </summary>
        /// <param name="clubRegistrationStatus"></param>
        /// <param name="memberCard"></param>
        /// <returns></returns>
        public string ClubRegistrationStatus(int? clubRegistrationStatus, string memberId)
        {
            try
            {
                if (clubRegistrationStatus != null)
                {
                    if (clubRegistrationStatus == ((int)ClubJoin.Joined))
                    {
                        return memberId;
                    }
                    else
                    {
                        return ((ClubJoin)clubRegistrationStatus).GetEnumDescription();
                    }
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Method to get the max date for delivery product.
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="time1"></param>
        /// <param name="date2"></param>
        /// <param name="time2"></param>
        /// <param name="date3"></param>
        /// <param name="time3"></param>
        /// <param name="date4"></param>
        /// <param name="time4"></param>
        /// <param name="date5"></param>
        /// <param name="time5"></param>
        /// <param name="date6"></param>
        /// <param name="time6"></param>
        /// <returns></returns>
        private string[] GetMaxDate(DateTime? date1, int? time1, int? deliveryPlace1, DateTime? date2, int? time2, int? deliveryPlace2, DateTime? date3, int? time3, int? deliveryPlace3,
            DateTime? date4, int? time4, int? deliveryPlace4, DateTime? date5, int? time5, int? deliveryPlace5, DateTime? date6, int? time6, int? deliveryPlace6)
        {
            try
            {
                var listDeliveryDate = new List<DeliveryDateTimeToExport>
                {
                    new DeliveryDateTimeToExport()
                    {
                        date          = date1,
                        time          = time1,
                        deliveryPlace = deliveryPlace1
                    },
                    new DeliveryDateTimeToExport()
                    {
                        date          = date2,
                        time          = time2,
                        deliveryPlace = deliveryPlace2
                    },
                    new DeliveryDateTimeToExport()
                    {
                        date          = date3,
                        time          = time3,
                        deliveryPlace = deliveryPlace3
                    },
                    new DeliveryDateTimeToExport()
                    {
                        date          = date4,
                        time          = time4,
                        deliveryPlace = deliveryPlace4
                    },
                    new DeliveryDateTimeToExport()
                    {
                        date          = date5,
                        time          = time5,
                        deliveryPlace = deliveryPlace5
                    },
                    new DeliveryDateTimeToExport()
                    {
                        date          = date6,
                        time          = time6,
                        deliveryPlace = deliveryPlace6
                    }
                };

                var ordered = listDeliveryDate.OrderByDescending(x => x.date).FirstOrDefault();
                var returnList = new string[2];

                if (ordered.date != null)
                {
                    returnList[0] = $"{ordered.date?.ToString(ExactDateFormat)} {((DeliveryTime)ordered.time).GetEnumDescription()}";
                    returnList[1] = $"{((DeliveryPlace)ordered.deliveryPlace).GetEnumDescription()}";
                    return returnList;
                }

                return new[] { string.Empty, string.Empty };
            }
            catch (Exception)
            {
                return new[] { string.Empty, string.Empty };
            }
        }

        /// <summary>
        /// Method to check item Encode error when Export CSV
        /// </summary>
        /// <param name="rowNumber"></param>
        /// <param name="contractCd"></param>
        /// <param name="strinInput"></param>
        /// <param name="nameColumn"></param>
        /// <param name="outMessage"></param>
        /// <param name="checkItemResult"></param>
        public bool CheckEncodeError(int rowNumber, string strinInput, string nameColumn, out string outMessage, out string itemResult, int length)
        {
            bool result = false;
            outMessage = string.Empty;
            itemResult = string.Empty;

            if (string.IsNullOrEmpty(strinInput))
            {
                result = false;
            }
            else
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                //Check special character
                var data = Encoding.GetEncoding(932,
                                new EncoderReplacementFallback("(unknown)"),
                                new DecoderReplacementFallback("(error)"))
                            .GetBytes(strinInput);

                var decodeString = Encoding.GetEncoding(932).GetString(data);

                if (decodeString.Contains("(unknown)"))
                {
                    result = true;
                    itemResult = strinInput;
                    outMessage = $"[{rowNumber}]行目、[{nameColumn}]項目で変換できない文字が含まれています。";
                }

                //Check byte length
                if (Encoding.GetEncoding(932).GetByteCount(strinInput) > length)
                {
                    var bytes = 0;
                    var stringBuilder = new StringBuilder();
                    var enumerator = StringInfo.GetTextElementEnumerator(strinInput);

                    while (enumerator.MoveNext())
                    {
                        var textElement = enumerator.GetTextElement();
                        bytes += Encoding.GetEncoding(932).GetByteCount(textElement);

                        if (bytes <= length)
                        {
                            stringBuilder.Append(textElement);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (result)
                    {
                        outMessage = string.Concat(outMessage, "\n");
                    }
                    else
                    {
                        result = true;
                    }

                    itemResult = stringBuilder.ToString();
                    outMessage = string.Concat(outMessage, $"[{rowNumber}]行目、[{nameColumn}]項目で入力値が上限（256バイト）を超えています。256バイト以降は切ります。");
                }

            }

            return result;
        }

        /// <summary>
        /// Method to put 0 character to the left of base on codeinput and maxlength.
        /// </summary>
        /// <param name="codeInput"></param>
        /// <returns></returns>
        public string PadLeftCode(string codeInput, int length)
        {
            var codeReturn = "";
            char pad = '0';
            if (string.IsNullOrEmpty(codeInput))
            {
                codeReturn = codeReturn.PadLeft(length, pad);
            }
            else
            {
                codeReturn = codeInput;
                if (codeInput.Length < length)
                {
                    codeReturn = codeInput.PadLeft(length, pad);
                }
            }
            return codeReturn;
        }

        /// <summary>
        /// Function to update Contracts.Flag_CSV = True for Contract after export csv.
        /// </summary>
        /// <param name="listExportFileAsModel"></param>
        public void UpdateFlagCSV(List<int> listUpdateFlagCSV, int exportMode)
        {
            var contracts = _context.Contracts.Where(c => listUpdateFlagCSV.Contains(c.Cd)).ToList();

            if (exportMode == (int)CsvExportMode.Active)
            {
                contracts.ForEach(c => c.FlagCsv = true);
            }
            else
            {
                contracts.ForEach(c => c.FlagCsvDeleted = true);
            }

            _context.SaveChanges();
        }

        #region XUAT FILE TXT COMPARE CÁC HỢP ĐỒNG ĐÃ EDIT
        public List<string> ExportFileAsFilter(ExportFileASFilterModel filterModel, out List<int> listContractCd)
        {
            try
            {
                //IQueryable<Contract> contract1 = _context.Contracts.Where(x => x.IsCompleted && (x.IsDeleted || x.IsEdited) && x.FlagCsv && (filterModel.IsIncludeExported || !x.FlagCsvDeleted));

                //IQueryable<Contract> contract2 = _context.Contracts.Where(x => x.IsCompleted && !x.IsDeleted && (filterModel.IsIncludeExported || !x.FlagCsv));

                IQueryable<Contract> contracts = _context.Contracts.Where(x => x.IsCompleted && (((x.IsDeleted || x.IsEdited) && x.FlagCsv && (filterModel.IsIncludeExported || !x.FlagCsvDeleted)) || (!x.IsDeleted && (filterModel.IsIncludeExported || !x.FlagCsv))));

                if (filterModel.ContractDateFrom != null)
                {
                    contracts = contracts.Where(x => x.ContractDate.Value.Date >= filterModel.ContractDateFrom.Value.Date);
                }

                if (filterModel.ContractDateTo != null)
                {
                    contracts = contracts.Where(x => x.ContractDate.Value.Date <= filterModel.ContractDateTo.Value.Date);
                }

                if (filterModel.EventCd != null)
                {
                    contracts = contracts.Where(x => x.Event.Cd == filterModel.EventCd);
                }

                List<ExportFileASModel> listExportFileAsModel = GetListContractExportAs(contracts);

                var lstEditOrDel = listExportFileAsModel.Where(x => (x.IsDeleted || x.IsEdited) && x.FlagCsv && (filterModel.IsIncludeExported || !x.FlagCsvDeleted)).ToList();

                var lstNew = listExportFileAsModel.Where(x => !x.IsDeleted && (filterModel.IsIncludeExported || !x.FlagCsv)).ToList();

                listContractCd = lstEditOrDel.Select(x => x.ContractCd).ToList();
                lstContent = new List<string>();

                foreach (var itemEditOrDel in lstEditOrDel)
                {
                    if (!itemEditOrDel.IsEdited && itemEditOrDel.IsDeleted && itemEditOrDel.FlagCsv && (filterModel.IsIncludeExported || !itemEditOrDel.FlagCsvDeleted))
                    {
                        lstContent.Add("----------");
                        lstContent.Add($"[ASシステム側の契約ID]: {itemEditOrDel.ContractId}");
                        // Hợp đồng bị hủy
                        lstContent.Add("この契約が取消されました。");
                    }
                    else
                    {
                        ExportFileASModel itemNew = null;
                        // Hợp đồng edit
                        if (itemEditOrDel.ContractOldId == null)
                        {
                            itemNew = lstNew.Where(x => x.ContractOldId == itemEditOrDel.ContractId).OrderByDescending(x => x.ContractCd).FirstOrDefault();
                        }
                        else
                        {
                            itemNew = lstNew.Where(x => x.ContractOldId == itemEditOrDel.ContractOldId).OrderByDescending(x => x.ContractCd).FirstOrDefault();
                        }
                        if (itemNew != null)
                        {
                            lstContent.Add("----------");
                            lstContent.Add($"[ASシステム側の契約ID]: {itemEditOrDel.ContractId}");

                            //Compare 2 hợp đồng
                            CompareItemEdit("契約ID", itemEditOrDel.ContractId, itemNew.ContractId);
                            CompareItemEdit("販売組織選択（営業部・ｅ・ジュネックス事業部）", itemEditOrDel.SaleDepartment, itemNew.SaleDepartment);
                            CompareItemEdit("催事CD", itemEditOrDel.EventCode, itemNew.EventCode);
                            CompareItemEdit("会場名", itemEditOrDel.EventName, itemNew.EventName);
                            CompareItemEdit("営業担当S", itemEditOrDel.SalesManSCode, itemNew.SalesManSCode);
                            CompareItemEdit("営業担当A", itemEditOrDel.SalesManCCode, itemNew.SalesManCCode);
                            CompareItemEdit("営業担当C", itemEditOrDel.SalesManACode, itemNew.SalesManACode);
                            CompareItemEdit("所属", itemEditOrDel.Organiazation, itemNew.Organiazation);
                            CompareItemEdit("契約日", itemEditOrDel.OrderDate, itemNew.OrderDate);
                            CompareItemEdit("お客様名（フリ仮名）", itemEditOrDel.CustomerNameFuri, itemNew.CustomerNameFuri);
                            CompareItemEdit("お客様名（漢字）", itemEditOrDel.CustomerName, itemNew.CustomerName);
                            CompareItemEdit("性別CD", itemEditOrDel.GenderCd, itemNew.GenderCd);
                            CompareItemEdit("性別", itemEditOrDel.Gender, itemNew.Gender);
                            CompareItemEdit("生年月日", itemEditOrDel.DateOfBirth, itemNew.DateOfBirth);
                            CompareItemEdit("歳", itemEditOrDel.Age, itemNew.Age);
                            CompareItemEdit("〒番号", itemEditOrDel.ZipCode, itemNew.ZipCode);
                            CompareItemEdit("住所1（フリ仮名）", itemEditOrDel.CustomerAddressFuri1, itemNew.CustomerAddressFuri1, true);
                            CompareItemEdit("住所2（フリ仮名）", itemEditOrDel.CustomerAddressFuri2, itemNew.CustomerAddressFuri2, true);
                            CompareItemEdit("住所3（フリ仮名）", itemEditOrDel.CustomerAddressFuri3, itemNew.CustomerAddressFuri3, true);
                            CompareItemEdit("住所1（漢字）", itemEditOrDel.CustomerAddress1, itemNew.CustomerAddress1, true);
                            CompareItemEdit("住所2（漢字）", itemEditOrDel.CustomerAddress2, itemNew.CustomerAddress2, true);
                            CompareItemEdit("住所3（漢字）", itemEditOrDel.CustomerAddress3, itemNew.CustomerAddress3, true);
                            CompareItemEdit("自宅電話番号", itemEditOrDel.HomePhone, itemNew.HomePhone);
                            CompareItemEdit("携帯電話番号", itemEditOrDel.MobilePhone, itemNew.MobilePhone);
                            CompareItemEdit("勤務先名称（フリ仮名）", itemEditOrDel.CompanyNameFuri, itemNew.CompanyNameFuri);
                            CompareItemEdit("勤務先名称（漢字等）", itemEditOrDel.CompanyName, itemNew.CompanyName);
                            CompareItemEdit("勤務先〒番号", itemEditOrDel.CompanyZipcode, itemNew.CompanyZipcode);
                            CompareItemEdit("勤務先住所1（フリ仮名）", itemEditOrDel.CompanyAddressFuri1, itemNew.CompanyAddressFuri1, true);
                            CompareItemEdit("勤務先住所2（フリ仮名）", itemEditOrDel.CompanyAddressFuri2, itemNew.CompanyAddressFuri2, true);
                            CompareItemEdit("勤務先住所3（フリ仮名）", itemEditOrDel.CompanyAddressFuri3, itemNew.CompanyAddressFuri3, true);
                            CompareItemEdit("勤務先住所1（漢字）", itemEditOrDel.CompanyAddress1, itemNew.CompanyAddress1, true);
                            CompareItemEdit("勤務先住所2（漢字）", itemEditOrDel.CompanyAddress2, itemNew.CompanyAddress2, true);
                            CompareItemEdit("勤務先住所3（漢字）", itemEditOrDel.CompanyAddress3, itemNew.CompanyAddress3, true);
                            CompareItemEdit("勤務先電話番号", itemEditOrDel.CompanyPhone, itemNew.CompanyPhone);
                            CompareItemEdit("勤務先内線", itemEditOrDel.CompanyLocalPhone, itemNew.CompanyLocalPhone);

                            CompareItemEdit("作家コード1", itemEditOrDel.ArtistCode1, itemNew.ArtistCode1);
                            CompareItemEdit("作家名/アイテム1", itemEditOrDel.Artist1, itemNew.Artist1);
                            CompareItemEdit("作品コード1", itemEditOrDel.ProductCode1, itemNew.ProductCode1);
                            CompareItemEdit("アイテム区分1", itemEditOrDel.ProductCategory1, itemNew.ProductCategory1);
                            CompareItemEdit("作品名1", itemEditOrDel.ProductName1, itemNew.ProductName1);
                            CompareItemEdit("技法1", itemEditOrDel.TechniqueName1, itemNew.TechniqueName1);
                            CompareItemEdit("数量1", itemEditOrDel.ProductQuantity1, itemNew.ProductQuantity1);
                            CompareItemEdit("税抜価格1", itemEditOrDel.ProductPrice1, itemNew.ProductPrice1);
                            CompareItemEdit("値引き金額1", itemEditOrDel.ProductDiscount1, itemNew.ProductDiscount1);
                            CompareItemEdit("消費税1", itemEditOrDel.ProductTaxPrice1, itemNew.ProductTaxPrice1);
                            CompareItemEdit("備考欄1", itemEditOrDel.ProductRemarkA1, itemNew.ProductRemarkA1);
                            CompareItemEdit("備考欄1", itemEditOrDel.ProductRemarkB1, itemNew.ProductRemarkB1);
                            CompareItemEdit("備考欄1", itemEditOrDel.ProductRemarkC1, itemNew.ProductRemarkC1);
                            CompareItemEdit("備考欄1", itemEditOrDel.ProductRemarkD1, itemNew.ProductRemarkD1);
                            CompareItemEdit("金券1", itemEditOrDel.CashVoucherValue1, itemNew.CashVoucherValue1);

                            CompareItemEdit("作家コード2", itemEditOrDel.ArtistCode2, itemNew.ArtistCode2);
                            CompareItemEdit("作家名/アイテム2", itemEditOrDel.Artist2, itemNew.Artist2);
                            CompareItemEdit("作品コード2", itemEditOrDel.ProductCode2, itemNew.ProductCode2);
                            CompareItemEdit("アイテム区分2", itemEditOrDel.ProductCategory2, itemNew.ProductCategory2);
                            CompareItemEdit("作品名2", itemEditOrDel.ProductName2, itemNew.ProductName2);
                            CompareItemEdit("技法2", itemEditOrDel.TechniqueName2, itemNew.TechniqueName2);
                            CompareItemEdit("数量2", itemEditOrDel.ProductQuantity2, itemNew.ProductQuantity2);
                            CompareItemEdit("税抜価格2", itemEditOrDel.ProductPrice2, itemNew.ProductPrice2);
                            CompareItemEdit("値引き金額2", itemEditOrDel.ProductDiscount2, itemNew.ProductDiscount2);
                            CompareItemEdit("消費税2", itemEditOrDel.ProductTaxPrice2, itemNew.ProductTaxPrice2);
                            CompareItemEdit("備考欄2", itemEditOrDel.ProductRemarkA2, itemNew.ProductRemarkA2);
                            CompareItemEdit("備考欄2", itemEditOrDel.ProductRemarkB2, itemNew.ProductRemarkB2);
                            CompareItemEdit("備考欄2", itemEditOrDel.ProductRemarkC2, itemNew.ProductRemarkC2);
                            CompareItemEdit("備考欄2", itemEditOrDel.ProductRemarkD2, itemNew.ProductRemarkD2);
                            CompareItemEdit("金券2", itemEditOrDel.CashVoucherValue2, itemNew.CashVoucherValue2);

                            CompareItemEdit("作家コード3", itemEditOrDel.ArtistCode3, itemNew.ArtistCode3);
                            CompareItemEdit("作家名/アイテム3", itemEditOrDel.Artist3, itemNew.Artist3);
                            CompareItemEdit("作品コード3", itemEditOrDel.ProductCode3, itemNew.ProductCode3);
                            CompareItemEdit("アイテム区分3", itemEditOrDel.ProductCategory3, itemNew.ProductCategory3);
                            CompareItemEdit("作品名3", itemEditOrDel.ProductName3, itemNew.ProductName3);
                            CompareItemEdit("技法3", itemEditOrDel.TechniqueName3, itemNew.TechniqueName3);
                            CompareItemEdit("数量3", itemEditOrDel.ProductQuantity3, itemNew.ProductQuantity3);
                            CompareItemEdit("税抜価格3", itemEditOrDel.ProductPrice3, itemNew.ProductPrice3);
                            CompareItemEdit("値引き金額3", itemEditOrDel.ProductDiscount3, itemNew.ProductDiscount3);
                            CompareItemEdit("消費税3", itemEditOrDel.ProductTaxPrice3, itemNew.ProductTaxPrice3);
                            CompareItemEdit("備考欄3", itemEditOrDel.ProductRemarkA3, itemNew.ProductRemarkA3);
                            CompareItemEdit("備考欄3", itemEditOrDel.ProductRemarkB3, itemNew.ProductRemarkB3);
                            CompareItemEdit("備考欄3", itemEditOrDel.ProductRemarkC3, itemNew.ProductRemarkC3);
                            CompareItemEdit("備考欄3", itemEditOrDel.ProductRemarkD3, itemNew.ProductRemarkD3);
                            CompareItemEdit("金券3", itemEditOrDel.CashVoucherValue3, itemNew.CashVoucherValue3);

                            CompareItemEdit("作家コード4", itemEditOrDel.ArtistCode4, itemNew.ArtistCode4);
                            CompareItemEdit("作家名/アイテム4", itemEditOrDel.Artist4, itemNew.Artist4);
                            CompareItemEdit("作品コード4", itemEditOrDel.ProductCode4, itemNew.ProductCode4);
                            CompareItemEdit("アイテム区分4", itemEditOrDel.ProductCategory4, itemNew.ProductCategory4);
                            CompareItemEdit("作品名4", itemEditOrDel.ProductName4, itemNew.ProductName4);
                            CompareItemEdit("技法4", itemEditOrDel.TechniqueName4, itemNew.TechniqueName4);
                            CompareItemEdit("数量4", itemEditOrDel.ProductQuantity4, itemNew.ProductQuantity4);
                            CompareItemEdit("税抜価格4", itemEditOrDel.ProductPrice4, itemNew.ProductPrice4);
                            CompareItemEdit("値引き金額4", itemEditOrDel.ProductDiscount4, itemNew.ProductDiscount4);
                            CompareItemEdit("消費税4", itemEditOrDel.ProductTaxPrice4, itemNew.ProductTaxPrice4);
                            CompareItemEdit("備考欄4", itemEditOrDel.ProductRemarkA4, itemNew.ProductRemarkA4);
                            CompareItemEdit("備考欄4", itemEditOrDel.ProductRemarkB4, itemNew.ProductRemarkB4);
                            CompareItemEdit("備考欄4", itemEditOrDel.ProductRemarkC4, itemNew.ProductRemarkC4);
                            CompareItemEdit("備考欄4", itemEditOrDel.ProductRemarkD4, itemNew.ProductRemarkD4);
                            CompareItemEdit("金券4", itemEditOrDel.CashVoucherValue4, itemNew.CashVoucherValue4);

                            CompareItemEdit("作家コード5", itemEditOrDel.ArtistCode5, itemNew.ArtistCode5);
                            CompareItemEdit("作家名/アイテム5", itemEditOrDel.Artist5, itemNew.Artist5);
                            CompareItemEdit("作品コード5", itemEditOrDel.ProductCode5, itemNew.ProductCode5);
                            CompareItemEdit("アイテム区分5", itemEditOrDel.ProductCategory5, itemNew.ProductCategory5);
                            CompareItemEdit("作品名5", itemEditOrDel.ProductName5, itemNew.ProductName5);
                            CompareItemEdit("技法5", itemEditOrDel.TechniqueName5, itemNew.TechniqueName5);
                            CompareItemEdit("数量5", itemEditOrDel.ProductQuantity5, itemNew.ProductQuantity5);
                            CompareItemEdit("税抜価格5", itemEditOrDel.ProductPrice5, itemNew.ProductPrice5);
                            CompareItemEdit("値引き金額5", itemEditOrDel.ProductDiscount5, itemNew.ProductDiscount5);
                            CompareItemEdit("消費税5", itemEditOrDel.ProductTaxPrice5, itemNew.ProductTaxPrice5);
                            CompareItemEdit("備考欄5", itemEditOrDel.ProductRemarkA5, itemNew.ProductRemarkA5);
                            CompareItemEdit("備考欄5", itemEditOrDel.ProductRemarkB5, itemNew.ProductRemarkB5);
                            CompareItemEdit("備考欄5", itemEditOrDel.ProductRemarkC5, itemNew.ProductRemarkC5);
                            CompareItemEdit("備考欄5", itemEditOrDel.ProductRemarkD5, itemNew.ProductRemarkD5);
                            CompareItemEdit("金券5", itemEditOrDel.CashVoucherValue5, itemNew.CashVoucherValue5);

                            CompareItemEdit("作家コード6", itemEditOrDel.ArtistCode6, itemNew.ArtistCode6);
                            CompareItemEdit("作家名/アイテム6", itemEditOrDel.Artist6, itemNew.Artist6);
                            CompareItemEdit("作品コード6", itemEditOrDel.ProductCode6, itemNew.ProductCode6);
                            CompareItemEdit("アイテム区分6", itemEditOrDel.ProductCategory6, itemNew.ProductCategory6);
                            CompareItemEdit("作品名6", itemEditOrDel.ProductName6, itemNew.ProductName6);
                            CompareItemEdit("技法6", itemEditOrDel.TechniqueName6, itemNew.TechniqueName6);
                            CompareItemEdit("数量6", itemEditOrDel.ProductQuantity6, itemNew.ProductQuantity6);
                            CompareItemEdit("税抜価格6", itemEditOrDel.ProductPrice6, itemNew.ProductPrice6);
                            CompareItemEdit("値引き金額6", itemEditOrDel.ProductDiscount6, itemNew.ProductDiscount6);
                            CompareItemEdit("消費税6", itemEditOrDel.ProductTaxPrice6, itemNew.ProductTaxPrice6);
                            CompareItemEdit("備考欄6", itemEditOrDel.ProductRemarkA6, itemNew.ProductRemarkA6);
                            CompareItemEdit("備考欄6", itemEditOrDel.ProductRemarkB6, itemNew.ProductRemarkB6);
                            CompareItemEdit("備考欄6", itemEditOrDel.ProductRemarkC6, itemNew.ProductRemarkC6);
                            CompareItemEdit("備考欄6", itemEditOrDel.ProductRemarkD6, itemNew.ProductRemarkD6);
                            CompareItemEdit("金券6", itemEditOrDel.CashVoucherValue6, itemNew.CashVoucherValue6);

                            CompareItemEdit("合計金額", itemEditOrDel.TotalPrice, itemNew.TotalPrice);
                            CompareItemEdit("頭金", itemEditOrDel.DownPayment, itemNew.DownPayment);
                            CompareItemEdit("残金", itemEditOrDel.LeftPayment, itemNew.LeftPayment);
                            CompareItemEdit("頭金支払方法（現金・銀行振込）", itemEditOrDel.DownPaymentMethod, itemNew.DownPaymentMethod);
                            CompareItemEdit("頭金支払予定年月日", itemEditOrDel.DownPaymentDate, itemNew.DownPaymentDate);
                            CompareItemEdit("領収書NO", itemEditOrDel.ReceiptNo, itemNew.ReceiptNo);
                            CompareItemEdit("残金支払予定年月日", itemEditOrDel.LeftPaymentDate, itemNew.LeftPaymentDate);
                            CompareItemEdit("残金支払方法（信販・カード・現金）", itemEditOrDel.LeftPaymentMethod, itemNew.LeftPaymentMethod);
                            CompareItemEdit("残金支払先（各信販会社・各カード会社）", itemEditOrDel.LeftPaymentPlace, itemNew.LeftPaymentPlace);
                            CompareItemEdit("オリコの場合のみ、照合管理番号", itemEditOrDel.VerifyNumer, itemNew.VerifyNumer);
                            CompareItemEdit("納期（年/月/日・AM/PM）", itemEditOrDel.DeliveryDate, itemNew.DeliveryDate);
                            CompareItemEdit("配送先（自宅・勤務先・実家・その他）", itemEditOrDel.DeliveryPlace, itemNew.DeliveryPlace);
                            CompareItemEdit("ID友の会入会（AVする・しない/EJする・しない）", itemEditOrDel.ClubRegistrationStatus, itemNew.ClubRegistrationStatus);
                            CompareItemEdit("ID友の会入会絵柄指定（ラッセン・天野・ディズニー・ｅ・ジュネックスメンバーシップ）", itemEditOrDel.MemberCard, itemNew.MemberCard);
                            CompareItemEdit("本人確認資料（免許証・保険証・旅券・他（））", itemEditOrDel.IdentifyDocument, itemNew.IdentifyDocument, true);
                            CompareItemEdit("備考　値引き承認者/配送先承認依頼/額装書類有無/その他販売承認/契約書後送り等（フリー入力）", itemEditOrDel.Remark, itemNew.Remark, true);
                            CompareItemEdit("媒体コード", itemEditOrDel.MediaCode, itemNew.MediaCode);
                            CompareItemEdit("枝番", itemEditOrDel.BranchCode, itemNew.BranchCode);
                            CompareItemEdit("同意書有無", itemEditOrDel.IsHaveProductAgreement, itemNew.IsHaveProductAgreement);
                            CompareItemEdit("同意書有の場合書類名 ", itemEditOrDel.ProductAgreementType, itemNew.ProductAgreementType);
                        }
                        else if(itemEditOrDel.IsEdited && itemEditOrDel.IsDeleted && itemEditOrDel.FlagCsv && (filterModel.IsIncludeExported || !itemEditOrDel.FlagCsvDeleted))
                        {
                            lstContent.Add("----------");
                            lstContent.Add($"[ASシステム側の契約ID]: {itemEditOrDel.ContractId}");
                            // Hợp đồng bị hủy
                            lstContent.Add("この契約が取消されました。");
                        }
                    }
                }

                return lstContent;
            }
            catch (Exception)
            {
                listContractCd = new List<int>();
                return lstContent;
            }
        }

        private List<ExportFileASModel> GetListContractExportAs(IQueryable<Contract> contracts)
        {
            var listExportFileAsModel = contracts.Select(c => new
            {
                c.Cd,
                c.Id,

                c.OldId,
                c.IsEdited,
                c.IsDeleted,
                c.FlagCsv,
                c.FlagCsvDeleted,

                c.SalesDepartment,
                EventCode              = c.Event.Code,
                EventName              = c.Event.Name,
                SalesmanSCode          = c.SalesmanS.Code,
                SalesmanACode          = c.SalesmanA.Code,
                SalesmanCCode          = c.SalesmanC.Code,
                OrganizationName       = c.Organization.Name,
                c.OrderDate,
                c.FamilyName,
                c.FirstName,
                c.FamilyNameFuri,
                c.FirstNameFuri,
                c.Gender,
                c.DateOfBirth,
                c.Age,
                c.Zipcode,
                c.ProvinceFuri,
                c.AddressFuri,
                c.BuildingFuri,
                c.Province,
                c.Address,
                c.Building,
                c.HomePhone,
                c.Mobiphone,
                c.CompanyNameFuri,
                c.CompanyName,
                c.CompanyZipCode,
                c.CompanyProvinceFuri,
                c.CompanyAddressFuri,
                c.CompanyBuildingFuri,
                c.CompanyProvince,
                c.CompanyAddress,
                c.CompanyBuilding,
                c.CompanyPhone,
                c.CompanyLocalPhone,
                c.Remark,
                //product1
                c.DepartmentCd1,
                ArtistCode1            = c.Artist1.Code,
                ArtistName1            = c.ArtistName1,
                ProductCode1           = c.Product1.Code,
                ProductCategory1       = c.Product1.ItemCategory,
                c.ProductName1,
                //TechniqueName1       = c.Technique1.Name,
                TechniqueName1         = c.TechniqueName1,
                c.ProductQuantity1,
                c.ProductPrice1,
                c.ProductDiscount1,
                c.ProductUnitTaxPrice1,
                c.ProductRemarks1,
                c.CashVoucherValue1,
                c.DeliveryDate1,
                c.DeliveryTime1,
                c.DeliveryPlace1,
                //product2
                c.DepartmentCd2,
                ArtistCode2            = c.Artist2.Code,
                ArtistName2            = c.ArtistName2,
                ProductCode2           = c.Product2.Code,
                ProductCategory2       = c.Product2.ItemCategory,
                c.ProductName2,
                //TechniqueName2       = c.Technique2.Name,
                TechniqueName2         = c.TechniqueName2,
                c.ProductQuantity2,
                c.ProductPrice2,
                c.ProductDiscount2,
                c.ProductUnitTaxPrice2,
                c.ProductRemarks2,
                c.CashVoucherValue2,
                c.DeliveryDate2,
                c.DeliveryTime2,
                c.DeliveryPlace2,
                //product3
                c.DepartmentCd3,
                ArtistCode3            = c.Artist3.Code,
                ArtistName3            = c.ArtistName3,
                ProductCode3           = c.Product3.Code,
                ProductCategory3       = c.Product3.ItemCategory,
                c.ProductName3,
                //TechniqueName3       = c.Technique3.Name,
                TechniqueName3         = c.TechniqueName3,
                c.ProductQuantity3,
                c.ProductPrice3,
                c.ProductDiscount3,
                c.ProductUnitTaxPrice3,
                c.ProductRemarks3,
                c.CashVoucherValue3,
                c.DeliveryDate3,
                c.DeliveryTime3,
                c.DeliveryPlace3,
                //product4
                c.DepartmentCd4,
                ArtistCode4            = c.Artist4.Code,
                ArtistName4            = c.ArtistName4,
                ProductCode4           = c.Product4.Code,
                ProductCategory4       = c.Product4.ItemCategory,
                c.ProductName4,
                //TechniqueName4       = c.Technique4.Name,
                TechniqueName4         = c.TechniqueName4,
                c.ProductQuantity4,
                c.ProductPrice4,
                c.ProductDiscount4,
                c.ProductUnitTaxPrice4,
                c.ProductRemarks4,
                c.CashVoucherValue4,
                c.DeliveryDate4,
                c.DeliveryTime4,
                c.DeliveryPlace4,
                //product5
                c.DepartmentCd5,
                ArtistCode5            = c.Artist5.Code,
                ArtistName5            = c.ArtistName5,
                ProductCode5           = c.Product5.Code,
                ProductCategory5       = c.Product5.ItemCategory,
                c.ProductName5,
                //TechniqueName5       = c.Technique5.Name,
                TechniqueName5         = c.TechniqueName5,
                c.ProductQuantity5,
                c.ProductPrice5,
                c.ProductDiscount5,
                c.ProductUnitTaxPrice5,
                c.ProductRemarks5,
                c.CashVoucherValue5,
                c.DeliveryDate5,
                c.DeliveryTime5,
                c.DeliveryPlace5,
                //product6
                c.DepartmentCd6,
                ArtistCode6            = c.Artist6.Code,
                ArtistName6            = c.ArtistName6,
                ProductCode6           = c.Product6.Code,
                ProductCategory6       = c.Product6.ItemCategory,
                c.ProductName6,
                //TechniqueName6       = c.Technique6.Name,
                TechniqueName6         = c.TechniqueName6,
                c.ProductQuantity6,
                c.ProductPrice6,
                c.ProductDiscount6,
                c.ProductUnitTaxPrice6,
                c.ProductRemarks6,
                c.CashVoucherValue6,
                c.DeliveryDate6,
                c.DeliveryTime6,
                c.DeliveryPlace6,
                c.TaxPrice,
                c.TotalPrice,
                c.DownPayment,
                c.LeftPayment,
                c.DownPaymentMethod,
                c.LeftPaymentMethod,
                c.DownPaymentDate,
                c.LeftPaymentDate,
                LeftPaymentPlace_query = c.Payment.Name,
                c.ReceiptNo,
                c.VerifyNumber,
                c.ClubRegistrationStatus,
                c.MemberCard,
                c.MemberId,
                c.IdentifyDocument,
                c.IsHaveProductAgreement,
                ProductAgreementType   = c.ProductAgreementType.Name,
                PaymentCode            = c.Payment.Code,
                MediaCode              = c.Media.Code,
                BranchCode             = c.Media.BranchCode
            })
            .OrderByDescending(x => x.OrderDate)
            .AsEnumerable()
            .Select(x => new ExportFileASModel
            {
                ContractCd             = x.Cd,
                ContractId             = x.Id,
                //
                ContractOldId          = x.OldId,
                IsEdited               = x.IsEdited,
                IsDeleted              = x.IsDeleted,
                FlagCsv                = x.FlagCsv,
                FlagCsvDeleted         = x.FlagCsvDeleted,
                //
                SaleDepartment         = ((SalesDepartment)x.SalesDepartment).GetEnumDescription(),
                EventCode              = x.EventCode,
                EventName              = x.EventName,
                SalesManSCode          = x.SalesmanSCode,
                SalesManCCode          = x.SalesmanCCode,
                SalesManACode          = x.SalesmanACode,
                Organiazation          = x.OrganizationName,
                OrderDate              = x.OrderDate?.ToString(ExactDateFormat),
                CustomerName           = $"{x.FamilyName} {x.FirstName}",
                CustomerNameFuri       = $"{x.FamilyNameFuri} {x.FirstNameFuri}",
                GenderCd               = x.Gender?.ToString(),
                Gender                 = ((Gender)Convert.ToInt32(x.Gender)).GetEnumDescription(),
                DateOfBirth            = x.DateOfBirth?.ToString(ExactDateFormat),
                Age                    = $"{x.Age} 歳",
                ZipCode                = x.Zipcode,
                CustomerAddressFuri1   = x.ProvinceFuri,
                CustomerAddressFuri2   = x.AddressFuri,
                CustomerAddressFuri3   = x.BuildingFuri,
                CustomerAddress1       = x.Province,
                CustomerAddress2       = x.Address,
                CustomerAddress3       = x.Building,
                HomePhone              = x.HomePhone,
                MobilePhone            = x.Mobiphone,
                CompanyNameFuri        = x.CompanyNameFuri,
                CompanyName            = x.CompanyName,
                CompanyZipcode         = x.CompanyZipCode,
                CompanyAddressFuri1    = x.CompanyProvinceFuri,
                CompanyAddressFuri2    = x.CompanyAddressFuri,
                CompanyAddressFuri3    = x.CompanyBuildingFuri,
                CompanyAddress1        = x.CompanyProvince,
                CompanyAddress2        = x.CompanyAddress,
                CompanyAddress3        = x.CompanyBuilding,
                CompanyPhone           = x.CompanyPhone,
                CompanyLocalPhone      = x.CompanyLocalPhone,
                //Artist1 / Product1
                ArtistCode1            = x.DepartmentCd1.HasValue ? PadLeftCode(x.ArtistCode1, 4) : string.Empty,
                Artist1                = x.ArtistName1,
                ProductCode1           = x.DepartmentCd1.HasValue ? PadLeftCode(x.ProductCode1, 4) : string.Empty,
                ProductCategory1       = x.DepartmentCd1.HasValue ? PadLeftCode(x.ProductCategory1, 2) : string.Empty,
                ProductName1           = x.ProductName1,
                TechniqueName1         = x.TechniqueName1,
                ProductQuantity1       = x.DepartmentCd1.HasValue ? x.ProductQuantity1?.ToString() : string.Empty,
                ProductPrice1          = x.DepartmentCd1.HasValue ? x.ProductPrice1?.ToString("F0") : string.Empty,
                ProductDiscount1       = x.DepartmentCd1.HasValue ? x.ProductDiscount1?.ToString("F0") : string.Empty,
                ProductTaxPrice1       = x.DepartmentCd1.HasValue ? x.ProductUnitTaxPrice1?.ToString("F0") : string.Empty,
                ProductRemark1         = ConvertRemarkStringToArr(x.ProductRemarks1),
                CashVoucherValue1      = x.CashVoucherValue1.HasValue ? ((CashVoucherValue)x.CashVoucherValue1).GetEnumDescription() : string.Empty,
                //Artist2 / Product2
                ArtistCode2            = x.DepartmentCd2.HasValue ? PadLeftCode(x.ArtistCode2, 4) : string.Empty,
                Artist2                = x.ArtistName2,
                ProductCode2           = x.DepartmentCd2.HasValue ? PadLeftCode(x.ProductCode2, 4) : string.Empty,
                ProductCategory2       = x.DepartmentCd2.HasValue ? PadLeftCode(x.ProductCategory2, 2) : string.Empty,
                ProductName2           = x.ProductName2,
                TechniqueName2         = x.TechniqueName2,
                ProductQuantity2       = x.DepartmentCd2.HasValue ? x.ProductQuantity2?.ToString() : string.Empty,
                ProductPrice2          = x.DepartmentCd2.HasValue ? x.ProductPrice2?.ToString("F0") : string.Empty,
                ProductDiscount2       = x.DepartmentCd2.HasValue ? x.ProductDiscount2?.ToString("F0") : string.Empty,
                ProductTaxPrice2       = x.DepartmentCd2.HasValue ? x.ProductUnitTaxPrice2?.ToString("F0") : string.Empty,
                ProductRemark2         = ConvertRemarkStringToArr(x.ProductRemarks2),
                CashVoucherValue2      = x.CashVoucherValue2.HasValue ? ((CashVoucherValue)x.CashVoucherValue2).GetEnumDescription() : string.Empty,
                //Artist3 / Product3
                ArtistCode3            = x.DepartmentCd3.HasValue ? PadLeftCode(x.ArtistCode3, 4) : string.Empty,
                Artist3                = x.ArtistName3,
                ProductCode3           = x.DepartmentCd3.HasValue ? PadLeftCode(x.ProductCode3, 4) : string.Empty,
                ProductCategory3       = x.DepartmentCd3.HasValue ? PadLeftCode(x.ProductCategory3, 2) : string.Empty,
                ProductName3           = x.ProductName3,
                TechniqueName3         = x.TechniqueName3,
                ProductQuantity3       = x.DepartmentCd3.HasValue ? x.ProductQuantity3?.ToString() : string.Empty,
                ProductPrice3          = x.DepartmentCd3.HasValue ? x.ProductPrice3?.ToString("F0") : string.Empty,
                ProductDiscount3       = x.DepartmentCd3.HasValue ? x.ProductDiscount3?.ToString("F0") : string.Empty,
                ProductTaxPrice3       = x.DepartmentCd3.HasValue ? x.ProductUnitTaxPrice3?.ToString("F0") : string.Empty,
                ProductRemark3         = ConvertRemarkStringToArr(x.ProductRemarks3),
                CashVoucherValue3      = x.CashVoucherValue3.HasValue ? ((CashVoucherValue)x.CashVoucherValue3).GetEnumDescription() : string.Empty,
                //Artist4 / Product4
                ArtistCode4            = x.DepartmentCd4.HasValue ? PadLeftCode(x.ArtistCode4, 4) : string.Empty,
                Artist4                = x.ArtistName4,
                ProductCode4           = x.DepartmentCd4.HasValue ? PadLeftCode(x.ProductCode4, 4) : string.Empty,
                ProductCategory4       = x.DepartmentCd4.HasValue ? PadLeftCode(x.ProductCategory4, 2) : string.Empty,
                ProductName4           = x.ProductName4,
                TechniqueName4         = x.TechniqueName4,
                ProductQuantity4       = x.DepartmentCd4.HasValue ? x.ProductQuantity4?.ToString() : string.Empty,
                ProductPrice4          = x.DepartmentCd4.HasValue ? x.ProductPrice4?.ToString("F0") : string.Empty,
                ProductDiscount4       = x.DepartmentCd4.HasValue ? x.ProductDiscount4?.ToString("F0") : string.Empty,
                ProductTaxPrice4       = x.DepartmentCd4.HasValue ? x.ProductUnitTaxPrice4?.ToString("F0") : string.Empty,
                ProductRemark4         = ConvertRemarkStringToArr(x.ProductRemarks4),
                CashVoucherValue4      = x.CashVoucherValue4.HasValue ? ((CashVoucherValue)x.CashVoucherValue4).GetEnumDescription() : string.Empty,
                //Artist5 / Product5
                ArtistCode5            = x.DepartmentCd5.HasValue ? PadLeftCode(x.ArtistCode5, 4) : string.Empty,
                Artist5                = x.ArtistName5,
                ProductCode5           = x.DepartmentCd5.HasValue ? PadLeftCode(x.ProductCode5, 4) : string.Empty,
                ProductCategory5       = x.DepartmentCd5.HasValue ? PadLeftCode(x.ProductCategory5, 2) : string.Empty,
                ProductName5           = x.ProductName5,
                TechniqueName5         = x.TechniqueName5,
                ProductQuantity5       = x.DepartmentCd5.HasValue ? x.ProductQuantity5?.ToString() : string.Empty,
                ProductPrice5          = x.DepartmentCd5.HasValue ? x.ProductPrice5?.ToString("F0") : string.Empty,
                ProductDiscount5       = x.DepartmentCd5.HasValue ? x.ProductDiscount5?.ToString("F0") : string.Empty,
                ProductTaxPrice5       = x.DepartmentCd5.HasValue ? x.ProductUnitTaxPrice5?.ToString("F0") : string.Empty,
                ProductRemark5         = ConvertRemarkStringToArr(x.ProductRemarks5),
                CashVoucherValue5      = x.CashVoucherValue5.HasValue ? ((CashVoucherValue)x.CashVoucherValue5).GetEnumDescription() : string.Empty,
                //Artist6 / Product6
                ArtistCode6            = x.DepartmentCd6.HasValue ? PadLeftCode(x.ArtistCode6, 4) : string.Empty,
                Artist6                = x.ArtistName6,
                ProductCode6           = x.DepartmentCd6.HasValue ? PadLeftCode(x.ProductCode6, 4) : string.Empty,
                ProductCategory6       = x.DepartmentCd6.HasValue ? PadLeftCode(x.ProductCategory6, 2) : string.Empty,
                ProductName6           = x.ProductName6,
                TechniqueName6         = x.TechniqueName6,
                ProductQuantity6       = x.DepartmentCd6.HasValue ? x.ProductQuantity6?.ToString() : string.Empty,
                ProductPrice6          = x.DepartmentCd6.HasValue ? x.ProductPrice6?.ToString("F0") : string.Empty,
                ProductDiscount6       = x.DepartmentCd6.HasValue ? x.ProductDiscount6?.ToString("F0") : string.Empty,
                ProductTaxPrice6       = x.DepartmentCd6.HasValue ? x.ProductUnitTaxPrice6?.ToString("F0") : string.Empty,
                ProductRemark6         = ConvertRemarkStringToArr(x.ProductRemarks6),
                CashVoucherValue6      = x.CashVoucherValue6.HasValue ? ((CashVoucherValue)x.CashVoucherValue6).GetEnumDescription() : string.Empty,

                TotalPrice             = x.TotalPrice?.ToString("F0"),
                DownPayment            = x.DownPayment?.ToString("F0"),
                LeftPayment            = x.LeftPayment?.ToString("F0"),
                LeftPaymentMethod      = x.LeftPaymentMethod != null ? ((LeftPaymentMethod)x.LeftPaymentMethod).GetEnumDescription() : string.Empty,
                DownPaymentMethod      = x.DownPaymentMethod != null ? ((DownPaymentMethod)x.DownPaymentMethod).GetEnumDescription() : string.Empty,
                DownPaymentDate        = x.DownPaymentDate?.ToString(ExactDateFormat),
                LeftPaymentDate        = x.LeftPaymentDate?.ToString(ExactDateFormat),
                LeftPaymentPlace       = x.LeftPaymentPlace_query,
                DeliveryDate           = GetMaxDate(
                                                    x.DeliveryDate1, x.DeliveryTime1, x.DeliveryPlace1,
                                                    x.DeliveryDate2, x.DeliveryTime2, x.DeliveryPlace2,
                                                    x.DeliveryDate3, x.DeliveryTime3, x.DeliveryPlace3,
                                                    x.DeliveryDate4, x.DeliveryTime4, x.DeliveryPlace4,
                                                    x.DeliveryDate5, x.DeliveryTime5, x.DeliveryPlace5,
                                                    x.DeliveryDate6, x.DeliveryTime6, x.DeliveryPlace6
                                                    )[0],
                DeliveryPlace          = GetMaxDate(
                                                    x.DeliveryDate1, x.DeliveryTime1, x.DeliveryPlace1,
                                                    x.DeliveryDate2, x.DeliveryTime2, x.DeliveryPlace2,
                                                    x.DeliveryDate3, x.DeliveryTime3, x.DeliveryPlace3,
                                                    x.DeliveryDate4, x.DeliveryTime4, x.DeliveryPlace4,
                                                    x.DeliveryDate5, x.DeliveryTime5, x.DeliveryPlace5,
                                                    x.DeliveryDate6, x.DeliveryTime6, x.DeliveryPlace6
                                                    )[1],
                ReceiptNo              = x.ReceiptNo,
                PaymentCode            = x.PaymentCode,
                VerifyNumer            = x.VerifyNumber,
                ClubRegistrationStatus = ClubRegistrationStatus(x.ClubRegistrationStatus, x.MemberId),
                IdentifyDocument       = x.IdentifyDocument != null ? ((IdentifyDocument)x.IdentifyDocument).GetEnumDescription() : string.Empty,
                IsHaveProductAgreement = x.IsHaveProductAgreement != null ? ((IsHaveProductAgreement)Convert.ToInt32(x.IsHaveProductAgreement)).GetEnumDescription() : string.Empty,
                ProductAgreementType   = x.ProductAgreementType,
                MemberCard             = x.MemberCard != null ? ((MemberCard)x.MemberCard).GetEnumDescription() : string.Empty,
                Remark                 = !string.IsNullOrEmpty(x.Remark) ? Regex.Replace(x.Remark, @"\t|\n|\r", "　") : string.Empty,
                MediaCode              = x.MediaCode,
                BranchCode             = x.BranchCode
            }).ToList();
            return listExportFileAsModel;
        }

        private void CompareItemEdit(string itemName, string itemBefore, string itemAfter, bool isCheckLength = false)
        {
            if (itemBefore != itemAfter)
            {
                lstContent.Add($"[項目名]: {itemName}");
                if (isCheckLength)
                {
                    lstContent.Add($"[変更前データ]: {CheckStringLengthOfItem(itemBefore)}");
                    lstContent.Add($"[変更後データ]: {CheckStringLengthOfItem(itemAfter)}");
                }
                else
                {
                    lstContent.Add($"[変更前データ]: {itemBefore}");
                    lstContent.Add($"[変更後データ]: {itemAfter}");
                }
                lstContent.Add("");
            }
        }

        public string CheckStringLengthOfItem(string stringInput)
        {
            int length = 256;

            if (!string.IsNullOrEmpty(stringInput))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var countbytesOfString = Encoding.GetEncoding(932).GetByteCount(stringInput);

                if (countbytesOfString > length)
                {
                    var bytes = 0;
                    var stringBuilder = new StringBuilder();
                    var enumerator = StringInfo.GetTextElementEnumerator(stringInput);

                    while (enumerator.MoveNext())
                    {
                        var textElement = enumerator.GetTextElement();
                        bytes += Encoding.GetEncoding(932).GetByteCount(textElement);

                        if (bytes <= length)
                        {
                            stringBuilder.Append(textElement);
                        }
                        else
                        {
                            break;
                        }
                    }

                    stringInput = stringBuilder.ToString();

                    return stringInput;
                }
            }

            return stringInput;
        }

        #endregion

        private string[] ConvertRemarkStringToArr(string remarkStr)
        {
            string[] result = { "", "", "", "" };

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
                                result[i] = ((ProductRemarkA)remarkCds[i]).GetEnumDescription();
                            }
                            break;
                        case 1:
                            if (remarkCds[i] > 0)
                            {
                                result[i] = ((ProductRemarkB)remarkCds[i]).GetEnumDescription();
                            }
                            break;
                        case 2:
                            if (remarkCds[i] > 0)
                            {
                                result[i] = ((ProductRemarkC)remarkCds[i]).GetEnumDescription();
                            }
                            break;
                        case 3:
                            if (remarkCds[i] > 0)
                            {
                                result[i] = ((ProductRemarkD)remarkCds[i]).GetEnumDescription();
                            }
                            break;
                    }
                }
            }

            return result;
        }
    }
}