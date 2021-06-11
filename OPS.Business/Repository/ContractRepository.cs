using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using OPS.Business.IRepository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using OPS.Utility.Extensions;
using OPS.ViewModels.Shared;
using OPS.ViewModels.User.Contract;
using OPS.ViewModels.User.Contract.Details;
using OPS.ViewModels.User.Contract.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using static OPS.Utility.Constants;
using CustomerInfo = OPS.ViewModels.User.Contract.CustomerInfo;

namespace OPS.Business.Repository
{
    public class ContractRepository : IContractRepository
    {
        protected readonly OpsContext _context;

        public ContractRepository(OpsContext context)
        {
            _context = context;
        }

        public int CreateContractPIC(ContractPICInfo contractPIC)
        {
            try
            {
                var newContract = new Contract
                {
                    EventCd = contractPIC.EventCd,
                    SalesDepartment = contractPIC.SalesDepartment,
                    SalesmanSCd = contractPIC.SalesmanSCd,
                    SalesmanCCd = contractPIC.SalesmanCCd,
                    SalesmanACd = contractPIC.SalesmanACd,
                    OrganizationCd = contractPIC.OrganizationCd,
                    Inputter = contractPIC.Inputter,
                    Id = GenerateContractId(contractPIC.EventCd, contractPIC.EventCode),
                    InsertUserId = contractPIC.InsertUserId,
                    InsertDate = DateTime.Now,
                };

                _context.Contracts.Add(newContract);

                while (true)
                {
                    try
                    {
                        _context.SaveChanges();
                        break;
                    }
                    catch (DbUpdateException e) when (e.InnerException?.InnerException is MySqlException sqlEx && sqlEx.Number == 1062)
                    {
                        newContract.Id = GenerateContractId(contractPIC.EventCd, contractPIC.EventCode);
                    }
                }

                return newContract.Cd;
            }
            catch
            {
                return 0;
            }
        }

        public int GetEventCdByUserId(string userId)
        {
            try
            {
                return _context.MstEvents.Where(e => e.ApplicationUserId == userId).Select(e => e.Cd).FirstOrDefault();
            }
            catch
            {
                return 0;
            }
        }

        public bool ConfirmTermAndCodition(TermAndCondition confirmInfo)
        {
            try
            {
                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == confirmInfo.ContractCd && !c.IsDeleted && c.Event.ApplicationUserId == confirmInfo.UpdateUserId);

                if(contractInDb != null)
                {
                    var confirmMoment = DateTime.Now;
                    contractInDb.ConfNoticeTime = !(contractInDb.IsConfNotice ?? false) && confirmInfo.IsConfNotice
                                                ? confirmMoment
                                                : !confirmInfo.IsConfNotice ? null : contractInDb.ConfNoticeTime;

                    contractInDb.ConfCoolingOffTime = !(contractInDb.IsConfCoolingOff ?? false) && confirmInfo.IsConfCoolingOff
                                                    ? confirmMoment
                                                    : !confirmInfo.IsConfCoolingOff ? null : contractInDb.ConfCoolingOffTime;

                    contractInDb.ConfPersonalInfoTime = !(contractInDb.IsConfPersonalInfo ?? false) && confirmInfo.IsConfPersonalInfo
                                                    ? confirmMoment
                                                    : !confirmInfo.IsConfPersonalInfo ? null : contractInDb.ConfPersonalInfoTime;

                    contractInDb.IsConfNotice = confirmInfo.IsConfNotice;
                    contractInDb.IsConfCoolingOff = confirmInfo.IsConfCoolingOff;
                    contractInDb.IsConfPersonalInfo = confirmInfo.IsConfPersonalInfo;
                    contractInDb.UpdateUserId = confirmInfo.UpdateUserId;
                    contractInDb.UpdateDate = confirmMoment;

                    _context.SaveChanges();

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetEventCodeByUserId(string userId)
        {
            try
            {
                return _context.MstEvents.Where(e => e.ApplicationUserId == userId).Select(e => e.Code).FirstOrDefault();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Method to get List Contract from DB and put it to User/Search Contract
        /// </summary>
        /// <param name="contractSearch"></param>
        /// <returns></returns>
        public ContractListSearchModel GetListContractByUser(ContractListSearchModel contractSearch)
        {
            try
            {
                var contractListReturn = new ContractListSearchModel();
                var contractIQuery     = _context.Contracts.Where(x => !x.IsDeleted && x.IsCompleted && x.Event.Code == contractSearch.EventCode);

                //Search survey with contractDate if OrderDate is not null
                if (contractSearch.OrderDate != null)
                {
                    contractIQuery = contractIQuery.Where(x => x.OrderDate.Value.Date == contractSearch.OrderDate.Value.Date);
                }

                //Search survey with Client Furikana name if Client name is not null
                if (!string.IsNullOrEmpty(contractSearch.Keyword))
                {
                    var keyWord = contractSearch.Keyword.ToLower().Trim();
                    var keyWordKana = contractSearch.KeywordKana.ToLower().Trim();
                    contractIQuery = contractIQuery.Where(x => (x.FamilyNameFuri.ToLower() + " " + x.FirstNameFuri.ToLower()).Contains(keyWordKana) 
                                                            || (x.FamilyName.ToLower() + " " + x.FirstName.ToLower()).Contains(keyWord));
                }

                contractListReturn.TotalRowsAfterFiltering = contractIQuery.Count();
                //Sort And Paging
                contractIQuery = Filtering(contractIQuery, contractSearch);

                contractListReturn.ListContract = contractIQuery
                    .Select(s => new {
                        s.Cd,
                        s.OrderDate,
                        s.FamilyName,
                        s.FirstName,
                        s.FamilyNameFuri,
                        s.FirstNameFuri,
                        ArtistName1    = s.ArtistName1,
                        s.ProductName1,
                        ArtistName2    = s.ArtistName2,
                        s.ProductName2,
                        ArtistName3    = s.ArtistName3,
                        s.ProductName3,
                        ArtistName4    = s.ArtistName4,
                        s.ProductName4,
                        ArtistName5    = s.ArtistName5,
                        s.ProductName5,
                        ArtistName6    = s.ArtistName6,
                        s.ProductName6
                    })
                    .AsEnumerable()
                    .Select(c => new ContractItemSearchModel()
                    {
                        Cd             = c.Cd,
                        OrderDate      = c.OrderDate?.ToString(ExactDateFormat),
                        ClientName     = $"{c.FamilyName} {c.FirstName}",
                        ClientNameKana = $"{c.FamilyNameFuri} {c.FirstNameFuri}",
                        ArtistName1    = c.ArtistName1,
                        ProductName1   = c.ProductName1,
                        ArtistName2    = c.ArtistName2,
                        ProductName2   = c.ProductName2,
                        ArtistName3    = c.ArtistName3,
                        ProductName3   = c.ProductName3,
                        ArtistName4    = c.ArtistName4,
                        ProductName4   = c.ProductName4,
                        ArtistName5    = c.ArtistName5,
                        ProductName5   = c.ProductName5,
                        ArtistName6    = c.ArtistName6,
                        ProductName6   = c.ProductName6
                    }).ToList();

                return contractListReturn;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Method to set sort asc, desc for List Contract
        /// </summary>
        /// <param name="contracts"></param>
        /// <param name="filtering"></param>
        /// <returns></returns>
        private IQueryable<Contract> Filtering(IQueryable<Contract> contracts, OpsFilteringDataTableModel filtering)
        {
            switch (filtering.SortColumnName)
            {
                case "OrderDate":
                    if (filtering.SortDirection == "asc")
                    {
                        contracts = contracts.OrderBy(x => x.OrderDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        contracts = contracts.OrderByDescending(x => x.OrderDate).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "ClientName":
                    if (filtering.SortDirection == "asc")
                    {
                        contracts = contracts.OrderBy(x => x.FamilyName).ThenBy(x=>x.FirstName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        contracts = contracts.OrderByDescending(x => x.FamilyName).ThenByDescending(x => x.FirstName).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                case "ClientNameKana":
                    if (filtering.SortDirection == "asc")
                    {
                        contracts = contracts.OrderBy(x => x.FamilyNameFuri).ThenBy(x => x.FirstNameFuri).Skip(filtering.Start).Take(filtering.Length);
                    }
                    else
                    {
                        contracts = contracts.OrderByDescending(x => x.FamilyNameFuri).ThenByDescending(x => x.FirstNameFuri).Skip(filtering.Start).Take(filtering.Length);
                    }
                    break;

                default:
                    contracts = contracts.OrderByDescending(x => x.OrderDate).Skip(filtering.Start).Take(filtering.Length);
                    break;
            }

            return contracts;
        }
        //Print Contract
        public ContractForPrint GetContractForPrint(int contractCd, string eventCode)
        {
            var contractForPrint = _context.Contracts.Where(c => c.Cd == contractCd && c.Event.Code.Equals(eventCode) && !c.IsDeleted)
                .Select(c => new
                {
                    c.Cd,
                    c.Id,
                    EventName        = c.Event.Name,
                    c.ContractDate,
                    SalesmanS        = c.SalesmanS.Name,
                    SalesmanSCode    = c.SalesmanS.Code,
                    SalesmanA        = c.SalesmanA.Name,
                    SalesmanACode    = c.SalesmanA.Code,
                    SalesmanC        = c.SalesmanC.Name,
                    SalesmanCCode    = c.SalesmanC.Code,
                    OrganizationName = c.Organization.Name,
                    c.FamilyNameFuri,
                    c.FamilyName,
                    c.FirstName,
                    c.FirstNameFuri,
                    c.DateOfBirth,
                    c.Age,
                    c.Gender,
                    c.HomePhone,
                    c.Mobiphone,
                    c.ProvinceFuri,
                    c.Province,
                    c.AddressFuri,
                    c.Address,
                    c.Building,
                    c.BuildingFuri,
                    c.Zipcode,
                    c.IdentifyDocument,
                    c.Remark,
                    c.OtherIdentifyDoc,
                    c.CompanyName,
                    c.CompanyNameFuri,
                    c.CompanyZipCode,
                    c.CompanyProvince,
                    c.CompanyProvinceFuri,
                    c.CompanyAddress,
                    c.CompanyAddressFuri,
                    c.CompanyBuilding,
                    c.CompanyBuildingFuri,
                    c.CompanyLocalPhone,
                    c.CompanyPhone,
                    c.ArtistName1,
                    c.ProductName1,
                    //TechniqueName1 = c.Technique1.Name,
                    TechniqueName1   = c.TechniqueName1,
                    c.ProductQuantity1,
                    c.ProductPrice1,
                    c.ProductTaxPrice1,
                    c.ProductDiscount1,
                    c.ProductRemarks1,
                    c.CashVoucherValue1,
                    c.DeliveryDate1,
                    c.DeliveryTime1,
                    c.DeliveryPlace1,
                    c.ArtistName2,
                    c.ProductName2,
                    //TechniqueName2 = c.Technique2.Name,
                    TechniqueName2   = c.TechniqueName2,
                    c.ProductQuantity2,
                    c.ProductPrice2,
                    c.ProductTaxPrice2,
                    c.ProductDiscount2,
                    c.ProductRemarks2,
                    c.CashVoucherValue2,
                    c.DeliveryDate2,
                    c.DeliveryTime2,
                    c.DeliveryPlace2,
                    c.ArtistName3,
                    c.ProductName3,
                    //TechniqueName3 = c.Technique3.Name,
                    TechniqueName3   = c.TechniqueName3,
                    c.ProductQuantity3,
                    c.ProductPrice3,
                    c.ProductTaxPrice3,
                    c.ProductDiscount3,
                    c.ProductRemarks3,
                    c.CashVoucherValue3,
                    c.DeliveryDate3,
                    c.DeliveryTime3,
                    c.DeliveryPlace3,
                    c.ArtistName4,
                    c.ProductName4,
                    //TechniqueName4 = c.Technique4.Name,
                    TechniqueName4   = c.TechniqueName4,
                    c.ProductQuantity4,
                    c.ProductPrice4,
                    c.ProductTaxPrice4,
                    c.ProductDiscount4,
                    c.ProductRemarks4,
                    c.CashVoucherValue4,
                    c.DeliveryDate4,
                    c.DeliveryTime4,
                    c.DeliveryPlace4,
                    c.ArtistName5,
                    c.ProductName5,
                    //TechniqueName5 = c.Technique5.Name,
                    TechniqueName5   = c.TechniqueName5,
                    c.ProductQuantity5,
                    c.ProductPrice5,
                    c.ProductTaxPrice5,
                    c.ProductDiscount5,
                    c.ProductRemarks5,
                    c.CashVoucherValue5,
                    c.DeliveryDate5,
                    c.DeliveryTime5,
                    c.DeliveryPlace5,
                    c.ArtistName6,
                    c.ProductName6,
                    //TechniqueName6 = c.Technique6.Name,
                    TechniqueName6   = c.TechniqueName6,
                    c.ProductQuantity6,
                    c.ProductPrice6,
                    c.ProductTaxPrice6,
                    c.ProductDiscount6,
                    c.ProductRemarks6,
                    c.CashVoucherValue6,
                    c.DeliveryDate6,
                    c.DeliveryTime6,
                    c.DeliveryPlace6,
                    c.SalesPrice,
                    c.TaxPrice,
                    c.TotalPrice,
                    c.DownPayment,
                    c.DownPaymentDate,
                    c.DownPaymentMethod,
                    c.LeftPayment,
                    c.LeftPaymentDate,
                    c.LeftPaymentMethod,
                    LeftPaymentPlace = c.Payment.Name,
                    c.LeftPaymentOtherPlace,
                    Media            = c.Media.Code,
                    c.SalesDepartment,
                    c.MemberCard,
                    c.ClubRegistrationStatus,
                    c.ClubJoinStatus,
                    c.MemberJoinStatus,
                    c.Approve,
                    c.ClubJoinedText,
                    c.MemberJoinedText,
                    c.MemberId,
                    c.Email,
                    c.IsHaveProductAgreement,
                    c.ProductAgreementType,
                    c.PhoneBranch,
                })
                .AsEnumerable()
                .Select(m => new ContractForPrint
                {
                    ContractCd             = m.Cd,
                    ContractId             = m.Id,
                    EventName              = m.EventName,
                    ContractDate           = m.ContractDate,
                    SaleManSCode           = m.SalesmanSCode,
                    SaleManS               = m.SalesmanS,
                    SaleManA               = m.SalesmanA,
                    SaleManACode           = m.SalesmanACode,
                    SaleManC               = m.SalesmanC,
                    SaleManCCode           = m.SalesmanCCode,
                    Organization           = m.OrganizationName,
                    FamilyName             = m.FamilyName,
                    FamilyNameFuri         = m.FamilyNameFuri,
                    FirstName              = m.FirstName,
                    FirstNameFuri          = m.FirstNameFuri,
                    DateOfBirth            = m.DateOfBirth,
                    Age                    = m.Age,
                    Gender                 = m.Gender,
                    HomePhone              = m.HomePhone,
                    Mobiphone              = m.Mobiphone,
                    ProvinceFuri           = m.ProvinceFuri,
                    AddressFuri            = m.AddressFuri,
                    Province               = m.Province,
                    Address                = m.Address,
                    Building               = m.Building,
                    BuildingFuri           = m.BuildingFuri,
                    Zipcode                = m.Zipcode,
                    IdentifyDocument       = m.IdentifyDocument,
                    OtherIdentifyDocument  = m.OtherIdentifyDoc,
                    Remark                 = m.Remark,
                    CompanyName            = m.CompanyName,
                    CompanyNameFuri        = m.CompanyNameFuri,
                    CompanyZipcode         = m.CompanyZipCode,
                    CompanyProvince        = m.CompanyProvince,
                    CompanyProvinceFuri    = m.CompanyProvinceFuri,
                    CompanyAddress         = m.CompanyAddress,
                    CompanyAddressFuri     = m.CompanyAddressFuri,
                    CompanyBuilding        = m.CompanyBuilding,
                    CompanyBuildingFuri    = m.CompanyBuildingFuri,
                    CompanyLocalPhone      = m.CompanyLocalPhone,
                    CompanyPhone           = m.CompanyPhone,
                    ActistName1            = m.ArtistName1,
                    ProductName1           = m.ProductName1,
                    ProductTech1           = m.TechniqueName1, 
                    ProductQuantity1       = m.ProductQuantity1,
                    ProductPrice1          = m.ProductPrice1 * m.ProductQuantity1,
                    ProductTaxPrice1       = m.ProductTaxPrice1,
                    ProductDiscount1       = m.ProductDiscount1 * m.ProductQuantity1,
                    ProductRemarks1        = TransformRemarkStringForPrint(m.ProductRemarks1, m.CashVoucherValue1),
                    DeliveryDate1          = m.DeliveryDate1,
                    DeliveryTime1          = m.DeliveryTime1,
                    DeliveryPlace1         = m.DeliveryPlace1,
                    ActistName2            = m.ArtistName2,
                    ProductName2           = m.ProductName2,
                    ProductTech2           = m.TechniqueName2,
                    ProductQuantity2       = m.ProductQuantity2,
                    ProductPrice2          = m.ProductPrice2 * m.ProductQuantity2,
                    ProductTaxPrice2       = m.ProductTaxPrice2,
                    ProductDiscount2       = m.ProductDiscount2 * m.ProductQuantity2,
                    ProductRemarks2        = TransformRemarkStringForPrint(m.ProductRemarks2, m.CashVoucherValue2),
                    DeliveryDate2          = m.DeliveryDate2,
                    DeliveryTime2          = m.DeliveryTime2,
                    DeliveryPlace2         = m.DeliveryPlace2,
                    ActistName3            = m.ArtistName3,
                    ProductName3           = m.ProductName3,
                    ProductTech3           = m.TechniqueName3,
                    ProductQuantity3       = m.ProductQuantity3,
                    ProductPrice3          = m.ProductPrice3 * m.ProductQuantity3,
                    ProductTaxPrice3       = m.ProductTaxPrice3,
                    ProductDiscount3       = m.ProductDiscount3 * m.ProductQuantity3,
                    ProductRemarks3        = TransformRemarkStringForPrint(m.ProductRemarks3, m.CashVoucherValue3),
                    DeliveryDate3          = m.DeliveryDate3,
                    DeliveryTime3          = m.DeliveryTime3,
                    DeliveryPlace3         = m.DeliveryPlace3,
                    ActistName4            = m.ArtistName4,
                    ProductName4           = m.ProductName4,
                    ProductTech4           = m.TechniqueName4,
                    ProductQuantity4       = m.ProductQuantity4,
                    ProductPrice4          = m.ProductPrice4 * m.ProductQuantity4,
                    ProductTaxPrice4       = m.ProductTaxPrice4,
                    ProductDiscount4       = m.ProductDiscount4 * m.ProductQuantity4,
                    ProductRemarks4        = TransformRemarkStringForPrint(m.ProductRemarks4, m.CashVoucherValue4),
                    DeliveryDate4          = m.DeliveryDate4,
                    DeliveryTime4          = m.DeliveryTime4,
                    DeliveryPlace4         = m.DeliveryPlace4,
                    ActistName5            = m.ArtistName5,
                    ProductName5           = m.ProductName5,
                    ProductTech5           = m.TechniqueName5,
                    ProductQuantity5       = m.ProductQuantity5,
                    ProductPrice5          = m.ProductPrice5 * m.ProductQuantity5,
                    ProductTaxPrice5       = m.ProductTaxPrice5,
                    ProductDiscount5       = m.ProductDiscount5 * m.ProductQuantity5,
                    ProductRemarks5        = TransformRemarkStringForPrint(m.ProductRemarks5, m.CashVoucherValue5),
                    DeliveryDate5          = m.DeliveryDate5,
                    DeliveryTime5          = m.DeliveryTime5,
                    DeliveryPlace5         = m.DeliveryPlace5,
                    ActistName6            = m.ArtistName6,
                    ProductName6           = m.ProductName6,
                    ProductTech6           = m.TechniqueName6,
                    ProductQuantity6       = m.ProductQuantity6,
                    ProductPrice6          = m.ProductPrice6 * m.ProductQuantity6,
                    ProductTaxPrice6       = m.ProductTaxPrice6,
                    ProductDiscount6       = m.ProductDiscount6 * m.ProductQuantity6,
                    ProductRemarks6        = TransformRemarkStringForPrint(m.ProductRemarks6, m.CashVoucherValue6),
                    DeliveryDate6          = m.DeliveryDate6,
                    DeliveryTime6          = m.DeliveryTime6,
                    DeliveryPlace6         = m.DeliveryPlace6,
                    SellPrice              = m.SalesPrice,
                    TotalTax               = m.TaxPrice,
                    TotalPrice             = m.TotalPrice,
                    DownPayment            = m.DownPayment,
                    DownPayMentDate        = m.DownPaymentDate,
                    DownPayMentMethod      = m.DownPaymentMethod,
                    LeftPayment            = m.LeftPayment,
                    LeftPaymentDate        = m.LeftPaymentDate,
                    LeftPaymentMethod      = m.LeftPaymentMethod,
                    LeftPaymentPlaceStr    = m.LeftPaymentPlace,
                    LeftPaymentOtherPlace  = m.LeftPaymentOtherPlace,
                    MediaName              = m.Media,
                    SaleDeparment          = m.SalesDepartment,
                    MemberCard             = m.MemberCard,
                    ClubJoin               = m.ClubRegistrationStatus,
                    StatusJoinClub         = m.ClubJoinStatus,
                    StatusJoinMembers      = m.MemberJoinStatus,
                    ClubJoinedText         = m.ClubJoinedText,
                    MemberJoinedText       = m.MemberJoinedText,
                    MemberId               = m.MemberId,
                    Approve                = m.Approve,
                    Email                  = m.Email,
                    IsHaveProductAgreement = m.IsHaveProductAgreement.HasValue ? m.IsHaveProductAgreement.Value : false,
                    ProductAgreementType   = m.ProductAgreementType == null ? string.Empty : m.ProductAgreementType.Name,
                    PhoneBranch            = m.PhoneBranch, 
                }).FirstOrDefault();
            return contractForPrint;
        }
        // End Print Contract

        public void CheckContractPrinted(int contractCd)
        {
            try
            {
                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == contractCd);
                if (!contractInDb.IsPrinted)
                {
                    contractInDb.IsPrinted = true;
                    contractInDb.UpdateDate = DateTime.Now;

                    _context.SaveChanges();
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Method to return Contract Information detail
        /// </summary>
        /// <param name="cd"></param>
        /// <returns></returns>
        public ContractDetailModel GetContractDetail(int cd, string userCode)
        {
            var contractDetail = _context.Contracts
                .Where(x => x.Cd == cd && x.Event.Code == userCode && !x.IsDeleted && x.IsCompleted)
                .Select(c => new
                {
                    c.Cd,
                    c.EventCd,
                    c.SalesDepartment,
                    c.ContractDate,
                    c.OrderDate,
                    c.FutureEventCd,
                    FutureEventName = c.FutureEvent.Name,
                    FutureEventStartDate = c.FutureEvent.StartDate,
                    FutureEventEndDate = c.FutureEvent.EndDate,
                    c.SalesmanSCd,
                    SalesmanSCode        = c.SalesmanS.Code,
                    SalesmanSName        = c.SalesmanS.Name,
                    SalesmanCCode        = c.SalesmanC.Code,
                    SalesmanCName        = c.SalesmanC.Name,
                    SalesmanACode        = c.SalesmanA.Code,
                    SalesmanAName        = c.SalesmanA.Name,
                    OrganizationName     = c.Organization.Name,
                    c.Inputter,
                    c.IsConfCoolingOff,
                    c.IsConfNotice,
                    c.IsConfPersonalInfo,
                    c.FamilyName,
                    c.FirstName,
                    c.FamilyNameFuri,
                    c.FirstNameFuri,
                    c.DateOfBirth,
                    c.Age,
                    c.Gender,
                    c.HomePhone,
                    c.Mobiphone,
                    c.Email,
                    c.Province,
                    c.ProvinceFuri,
                    c.Address,
                    c.AddressFuri,
                    c.Building,
                    c.BuildingFuri,
                    c.Zipcode,
                    c.Remark,
                    c.CompanyName,
                    c.CompanyNameFuri,
                    c.CompanyZipCode,
                    c.CompanyProvince,
                    c.CompanyAddress,
                    c.CompanyBuilding,
                    c.CompanyProvinceFuri,
                    c.CompanyAddressFuri,
                    c.CompanyBuildingFuri,
                    c.CompanyLocalPhone,
                    c.CompanyPhone,
                    c.PhoneBranch,
                    c.DepartmentCd1,
                    DeparmentName1 = c.Department1.Name,
                    c.ArtistName1,
                    c.ProductName1,
                    //TechniqueName1 = c.Technique1.Name,
                    TechniqueName1 = c.TechniqueName1,
                    c.ProductQuantity1,
                    c.ProductPrice1,
                    c.ProductDiscount1,
                    c.ProductSalesPrice1,
                    c.ProductRemarks1,
                    c.CashVoucherValue1,
                    c.DeliveryDate1,
                    c.DeliveryTime1,
                    c.DeliveryPlace1,
                    c.DepartmentCd2,
                    DeparmentName2 = c.Department2.Name,
                    c.ArtistName2,
                    c.ProductName2,
                    //TechniqueName2 = c.Technique2.Name,
                    TechniqueName2 = c.TechniqueName2,
                    c.ProductQuantity2,
                    c.ProductPrice2,
                    c.ProductDiscount2,
                    c.ProductSalesPrice2,
                    c.ProductRemarks2,
                    c.CashVoucherValue2,
                    c.DeliveryDate2,
                    c.DeliveryTime2,
                    c.DeliveryPlace2,
                    c.DepartmentCd3,
                    DeparmentName3 = c.Department3.Name,
                    c.ArtistName3,
                    c.ProductName3,
                    //TechniqueName3 = c.Technique3.Name,
                    TechniqueName3 = c.TechniqueName3,
                    c.ProductQuantity3,
                    c.ProductPrice3,
                    c.ProductDiscount3,
                    c.ProductSalesPrice3,
                    c.ProductRemarks3,
                    c.CashVoucherValue3,
                    c.DeliveryDate3,
                    c.DeliveryTime3,
                    c.DeliveryPlace3,
                    c.DepartmentCd4,
                    DeparmentName4 = c.Department4.Name,
                    c.ArtistName4,
                    c.ProductName4,
                    //TechniqueName4 = c.Technique4.Name,
                    TechniqueName4 = c.TechniqueName4,
                    c.ProductQuantity4,
                    c.ProductPrice4,
                    c.ProductDiscount4,
                    c.ProductSalesPrice4,
                    c.ProductRemarks4,
                    c.CashVoucherValue4,
                    c.DeliveryDate4,
                    c.DeliveryTime4,
                    c.DeliveryPlace4,
                    c.DepartmentCd5,
                    DeparmentName5 = c.Department5.Name,
                    c.ArtistName5,
                    c.ProductName5,
                    //TechniqueName5 = c.Technique5.Name,
                    TechniqueName5 = c.TechniqueName5,
                    c.ProductQuantity5,
                    c.ProductPrice5,
                    c.ProductDiscount5,
                    c.ProductSalesPrice5,
                    c.ProductRemarks5,
                    c.CashVoucherValue5,
                    c.DeliveryDate5,
                    c.DeliveryTime5,
                    c.DeliveryPlace5,
                    c.DepartmentCd6,
                    DeparmentName6 = c.Department6.Name,
                    c.ArtistName6,
                    c.ProductName6,
                    //TechniqueName6 = c.Technique6.Name,
                    TechniqueName6 = c.TechniqueName6,
                    c.ProductQuantity6,
                    c.ProductPrice6,
                    c.ProductDiscount6,
                    c.ProductSalesPrice6,
                    c.ProductRemarks6,
                    c.CashVoucherValue6,
                    c.DeliveryDate6,
                    c.DeliveryTime6,
                    c.DeliveryPlace6,
                    c.SalesPrice,
                    c.TaxPrice,
                    c.TotalPrice,
                    ProductAgreementType = c.ProductAgreementType.Name,
                    c.IsHaveProductAgreement,
                    c.DownPayment,
                    c.DownPaymentDate,
                    c.DownPaymentMethod,
                    c.LeftPayment,
                    c.ReceiptNo,
                    c.LeftPaymentDate,
                    c.LeftPaymentMethod,
                    LeftPaymentPlace     = c.Payment.Name,
                    c.LeftPaymentOtherPlace,
                    c.VerifyNumber,
                    Media                = c.Media.Name,
                    c.VisitTime,
                    c.ClubJoinStatus,
                    c.ClubJoinedText,
                    c.MemberJoinStatus,
                    c.MemberJoinedText,
                    c.ClubRegistrationStatus,
                    c.MemberId,
                    c.IsConfirmMember,
                    c.MemberCard,
                    c.IdentifyDocument,
                    c.OtherIdentifyDoc,
                    c.Approve,
                    c.IsPrinted,
                })
                .AsEnumerable()
                .Select(x => new ContractDetailModel
                {
                    EventCd                = x.EventCd.ToString(),
                    ContractCd             = x.Cd.ToString(),
                    SalesDepartment        = ((SalesDepartment)x.SalesDepartment).GetEnumDescription(),
                    SalesmanSName          = x.SalesmanSCd.HasValue ? $"{x.SalesmanSCode} - {x.SalesmanSName}" : "",
                    SalesmanCName          = $"{x.SalesmanCCode} - {x.SalesmanCName}",
                    SalesmanAName          = $"{x.SalesmanACode} - {x.SalesmanAName}",
                    OrganizationName       = x.OrganizationName,
                    InputterName           = ((Inputter)x.Inputter).GetEnumDescription(),
                    IsConfCoolingOff       = x.IsConfCoolingOff.HasValue ? (x.IsConfCoolingOff.Value ? "有" : string.Empty) : string.Empty,
                    IsConfPersonalInfo     = x.IsConfPersonalInfo.HasValue ? (x.IsConfPersonalInfo.Value ? "有" : string.Empty) : string.Empty,
                    IsConfNotice           = x.IsConfNotice.HasValue ? (x.IsConfNotice.Value ? "有" : string.Empty) : string.Empty,
                    //Artist1 / Product1
                    DepartmentCd1          = x.DepartmentCd1, 
                    DepartmentName1        = x.DeparmentName1,
                    ArtistName1            = x.ArtistName1,
                    ProductName1           = x.ProductName1,
                    TechniqueName1         = x.TechniqueName1,
                    ProductQuantity1       = x.ProductQuantity1.ToString(),
                    ProductPrice1          = x.ProductPrice1?.ToString("N0"),
                    ProductDiscount1       = x.ProductDiscount1?.ToString("N0"),
                    ProductSalesPrice1     = x.DepartmentCd1.HasValue ? x.ProductSalesPrice1?.ToString("N0") : string.Empty,
                    ProductRemark1 = TransformRemarkString(x.ProductRemarks1),
                    CashVoucherValue1 = x.CashVoucherValue1.HasValue ? ((CashVoucherValue)x.CashVoucherValue1.Value).GetEnumDescription() : "",
                    DeliveryDateTime1      = x.DepartmentCd1.HasValue ? $"{x.DeliveryDate1?.ToString(ExactDateFormat)} {((DeliveryTime)x.DeliveryTime1.Value).GetEnumDescription()}" : string.Empty,
                    DeliveryPlace1         = x.DeliveryPlace1 != null ? ((DeliveryPlace)x.DeliveryPlace1).GetEnumDescription() : string.Empty,
                    ////Artist2 / Product2
                    DepartmentCd2          = x.DepartmentCd2,
                    DepartmentName2        = x.DeparmentName2,
                    ArtistName2            = x.ArtistName2,
                    ProductName2           = x.ProductName2,
                    TechniqueName2         = x.TechniqueName2,
                    ProductQuantity2       = x.ProductQuantity2.ToString(),
                    ProductPrice2          = x.ProductPrice2?.ToString("N0"),
                    ProductDiscount2       = x.ProductDiscount2?.ToString("N0"),
                    ProductSalesPrice2     = x.DepartmentCd2.HasValue ? x.ProductSalesPrice2?.ToString("N0") : string.Empty,
                    ProductRemark2 = TransformRemarkString(x.ProductRemarks2),
                    CashVoucherValue2 = x.CashVoucherValue2.HasValue ? ((CashVoucherValue)x.CashVoucherValue2.Value).GetEnumDescription() : "",
                    DeliveryDateTime2      = x.DepartmentCd2.HasValue ? $"{x.DeliveryDate2?.ToString(ExactDateFormat)} {((DeliveryTime)x.DeliveryTime2.Value).GetEnumDescription()}" : string.Empty,
                    DeliveryPlace2         = x.DeliveryPlace2 != null ? ((DeliveryPlace)x.DeliveryPlace2).GetEnumDescription() : string.Empty,
                    //Artist3 / Product3
                    DepartmentCd3          = x.DepartmentCd3,
                    DepartmentName3        = x.DeparmentName3,
                    ArtistName3            = x.ArtistName3,
                    ProductName3           = x.ProductName3,
                    TechniqueName3         = x.TechniqueName3,
                    ProductQuantity3       = x.ProductQuantity3.ToString(),
                    ProductPrice3          = x.ProductPrice3?.ToString("N0"),
                    ProductDiscount3       = x.ProductDiscount3?.ToString("N0"),
                    ProductSalesPrice3     = x.DepartmentCd3.HasValue ? x.ProductSalesPrice3?.ToString("N0") : string.Empty,
                    ProductRemark3 = TransformRemarkString(x.ProductRemarks3),
                    CashVoucherValue3 = x.CashVoucherValue3.HasValue ? ((CashVoucherValue)x.CashVoucherValue3.Value).GetEnumDescription() : "",
                    DeliveryDateTime3      = x.DepartmentCd3.HasValue ? $"{x.DeliveryDate3?.ToString(ExactDateFormat)} {((DeliveryTime)x.DeliveryTime3.Value).GetEnumDescription()}" : string.Empty,
                    DeliveryPlace3         = x.DeliveryPlace3 != null ? ((DeliveryPlace)x.DeliveryPlace3).GetEnumDescription() : string.Empty,
                    //Artist4 / Product4
                    DepartmentCd4          = x.DepartmentCd4,
                    DepartmentName4        = x.DeparmentName4,
                    ArtistName4            = x.ArtistName4,
                    ProductName4           = x.ProductName4,
                    TechniqueName4         = x.TechniqueName4,
                    ProductQuantity4       = x.ProductQuantity4.ToString(),
                    ProductPrice4          = x.ProductPrice4?.ToString("N0"),
                    ProductDiscount4       = x.ProductDiscount4?.ToString("N0"),
                    ProductSalesPrice4     = x.DepartmentCd4.HasValue ? x.ProductSalesPrice4?.ToString("N0") : string.Empty,
                    ProductRemark4 = TransformRemarkString(x.ProductRemarks4),
                    CashVoucherValue4 = x.CashVoucherValue4.HasValue ? ((CashVoucherValue)x.CashVoucherValue4.Value).GetEnumDescription() : "",
                    DeliveryDateTime4      = x.DepartmentCd4.HasValue ? $"{x.DeliveryDate4?.ToString(ExactDateFormat)} {((DeliveryTime)x.DeliveryTime4.Value).GetEnumDescription()}" : string.Empty,
                    DeliveryPlace4         = x.DeliveryPlace4 != null ? ((DeliveryPlace)x.DeliveryPlace4).GetEnumDescription() : string.Empty,
                    //Artist5 / Product5
                    DepartmentCd5          = x.DepartmentCd5,
                    DepartmentName5        = x.DeparmentName5,
                    ArtistName5            = x.ArtistName5,
                    ProductName5           = x.ProductName5,
                    TechniqueName5         = x.TechniqueName5,
                    ProductQuantity5       = x.ProductQuantity5.ToString(),
                    ProductPrice5          = x.ProductPrice5?.ToString("N0"),
                    ProductDiscount5       = x.ProductDiscount5?.ToString("N0"),
                    ProductSalesPrice5     = x.DepartmentCd5.HasValue ? x.ProductSalesPrice5?.ToString("N0") : string.Empty,
                    ProductRemark5 = TransformRemarkString(x.ProductRemarks5),
                    CashVoucherValue5 = x.CashVoucherValue5.HasValue ? ((CashVoucherValue)x.CashVoucherValue5.Value).GetEnumDescription() : "",
                    DeliveryDateTime5      = x.DepartmentCd5.HasValue ? $"{x.DeliveryDate5?.ToString(ExactDateFormat)} {((DeliveryTime)x.DeliveryTime5.Value).GetEnumDescription()}" : string.Empty,
                    DeliveryPlace5         = x.DeliveryPlace5 != null ? ((DeliveryPlace)x.DeliveryPlace5).GetEnumDescription() : string.Empty,
                    //Artist6 / Product6
                    DepartmentCd6          = x.DepartmentCd6,
                    DepartmentName6        = x.DeparmentName6,
                    ArtistName6            = x.ArtistName6,
                    ProductName6           = x.ProductName6,
                    TechniqueName6         = x.TechniqueName6,
                    ProductQuantity6       = x.ProductQuantity6.ToString(),
                    ProductPrice6          = x.ProductPrice6?.ToString("N0"),
                    ProductDiscount6       = x.ProductDiscount6?.ToString("N0"),
                    ProductSalesPrice6     = x.DepartmentCd6.HasValue ? x.ProductSalesPrice6?.ToString("N0") : string.Empty,
                    ProductRemark6 = TransformRemarkString(x.ProductRemarks6),
                    CashVoucherValue6 = x.CashVoucherValue6.HasValue ? ((CashVoucherValue)x.CashVoucherValue6.Value).GetEnumDescription() : "",
                    DeliveryDateTime6      = x.DepartmentCd6.HasValue ? $"{x.DeliveryDate6?.ToString(ExactDateFormat)} {((DeliveryTime)x.DeliveryTime6.Value).GetEnumDescription()}" : string.Empty,
                    DeliveryPlace6         = x.DeliveryPlace6 != null ? ((DeliveryPlace)x.DeliveryPlace6).GetEnumDescription() : string.Empty,
                    SalesPrice             = x.SalesPrice?.ToString("N0"),
                    TaxPrice               = x.TaxPrice?.ToString("N0"),
                    TotalPrice             = x.TotalPrice?.ToString("N0"),
                    DownPayment            = x.DownPayment?.ToString("N0"),
                    LeftPayment            = x.LeftPayment?.ToString("N0"),
                    DownPaymentMethod      = x.DownPaymentMethod != null ? ((DownPaymentMethod)x.DownPaymentMethod).GetEnumDescription() : string.Empty,
                    DownPaymentDate        = x.DownPaymentDate?.ToString(ExactDateFormat),
                    ReceiptNo              = x.ReceiptNo,
                    LeftPaymentMethod      = x.LeftPaymentMethod != null ? ((LeftPaymentMethod)x.LeftPaymentMethod).GetEnumDescription() : string.Empty,
                    LeftPaymentDate        = x.LeftPaymentDate?.ToString(ExactDateFormat),
                    LeftPaymentPlace       = x.LeftPaymentPlace,
                    LeftPaymentOtherPlace  = x.LeftPaymentOtherPlace,
                    IsHaveProductAgreement = x.IsHaveProductAgreement.HasValue ? (x.IsHaveProductAgreement.Value ? IsHaveProductAgreement.Yes : IsHaveProductAgreement.No).GetEnumDescription() : "",
                    ProductAgreementType   = (x.IsHaveProductAgreement ?? false) ? x.ProductAgreementType : "",
                    VerifyNumber           = x.VerifyNumber,
                    ContractDate           = x.ContractDate?.ToString(ExactDateFormat),
                    OrderDate              = x.OrderDate?.ToString(ExactDateFormat),
                    FutureEvent = x.FutureEventCd.HasValue ? $"{x.FutureEventName} ー {x.FutureEventStartDate?.ToString(ExactDateFormatJP)}～{x.FutureEventEndDate?.ToString(ExactDateFormatJP)}" : "",
                    CustomerName           = $"{x.FamilyName} {x.FirstName}",
                    CustomerNameFuri       = $"{x.FamilyNameFuri} {x.FirstNameFuri}",
                    Gender                 = x.Gender.HasValue ? ((Gender)x.Gender.Value).GetEnumDescription() : string.Empty,
                    Birthday               = x.DateOfBirth?.ToString(ExactDateFormat),
                    Age                    = x.Age.ToString(),
                    Zipcode                = x.Zipcode,
                    ProvineName            = x.Province,
                    Address                = x.Address,
                    Building               = x.Building,
                    ProvineNameFuri        = x.ProvinceFuri,
                    AddressFuri            = x.AddressFuri,
                    BuildingFuri           = x.BuildingFuri,
                    HomePhone              = x.HomePhone,
                    PhoneBranch            = x.PhoneBranch != null ? ((PhoneBranch)x.PhoneBranch).GetEnumDescription() : string.Empty,
                    MobilePhone            = x.Mobiphone,
                    Email                  = x.Email,
                    CompanyName            = x.CompanyName,
                    CompanyNameFuri        = x.CompanyNameFuri,
                    CompanyPhone           = x.CompanyPhone,
                    CompanyLocalPhone      = x.CompanyLocalPhone,
                    CompanyZipCode         = x.CompanyZipCode,
                    CompanyProvince        = x.CompanyProvince,
                    CompanyAddress         = x.CompanyAddress,
                    CompanyBuilding        = x.CompanyBuilding,
                    CompanyProvineFuri     = x.CompanyProvinceFuri,
                    CompanyAddressFuri     = x.CompanyAddressFuri,
                    CompanyBuildingFuri    = x.CompanyBuildingFuri,
                    Media                  = x.Media,
                    VisitTime              = x.VisitTime.HasValue ? ((VisitTime)x.VisitTime).GetEnumDescription() : "",
                    ClubJoinCheck          = x.ClubJoinStatus,
                    ClubJoinStatus         = x.ClubJoinStatus.HasValue ? ((StatusJoinClubMember)x.ClubJoinStatus.Value).GetEnumDescription() : string.Empty,
                    ClubJoinedText         = x.ClubJoinStatus == (int?)StatusJoinClubMember.Joined ? x.ClubJoinedText : string.Empty,
                    MemberJoinStatus       = x.MemberJoinStatus.HasValue ? ((StatusJoinClubMember)x.MemberJoinStatus.Value).GetEnumDescription() : string.Empty,
                    MemberJoinedText       = x.MemberJoinStatus == (int?)StatusJoinClubMember.Joined ? x.MemberJoinedText : string.Empty,
                    ClubRegistrationCheck  = x.ClubRegistrationStatus,
                    ClubRegistrationStatus = x.ClubRegistrationStatus.HasValue ? ((ClubJoin)x.ClubRegistrationStatus).GetEnumDescription() : string.Empty,
                    Remark                 = x.Remark,
                    IsConfirmMember        = x.IsConfirmMember.HasValue ? (x.IsConfirmMember.Value ? "有" : string.Empty) : string.Empty,
                    MemberCard             = x.MemberCard.HasValue ? ((MemberCard)x.MemberCard).GetEnumDescription() : string.Empty,
                    IdentifyDocument       = x.IdentifyDocument.HasValue ? ((IdentifyDocument)x.IdentifyDocument.Value).GetEnumDescription() : string.Empty,
                    OtherIdentifyDoc       = x.IdentifyDocument == (int?)IdentifyDocument.Other ? x.OtherIdentifyDoc : string.Empty,
                    Approve                = x.Approve,
                    MemberId               = x.ClubRegistrationStatus == (int?)ClubJoin.Joined ? x.MemberId : string.Empty,
                    IsPrinted = x.IsPrinted,
                }).FirstOrDefault();

            return contractDetail;
        }

        /// <summary>
        /// Method to delete Contract
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="userId"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public AjaxResponseModel CancelContract(int cd, string userId)
        {
            try
            {
                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == cd && c.Event.ApplicationUserId == userId);

                if (contractInDb != null)
                {
                    var confirmMoment         = DateTime.Now;
                    contractInDb.IsDeleted    = true;
                    contractInDb.UpdateUserId = userId;
                    contractInDb.UpdateDate   = confirmMoment;

                    _context.SaveChanges();

                    return new AjaxResponseModel
                    {
                        Status  = true
                    };
                }
                return new AjaxResponseModel
                {
                    Message = "契約の取消に失敗しました。もう一度お試しください。",
                    Status  = false
                };
            }
            catch (Exception)
            {
                return new AjaxResponseModel
                {
                    Message = "契約の取消に失敗しました。もう一度お試しください",
                    Status  = false

                };
            }
        }
            

        public int? GetTaxValueByDate(DateTime date)
        {
            try
            {
                return _context.MstTaxes.Where(t => !t.IsDeleted && t.StartDate <= date && (t.EndDate == null || t.EndDate >= date)).Select(e => e.Value).Single();
            }
            catch
            {
                return null;
            }
        }

        public bool CreateOrderInfo(OrderInfo orderInfo)
        {
            try
            {
                MappingArtistAndProductByName(orderInfo);
                orderInfo.CalculateBeforeSave();

                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == orderInfo.ContractCd && !c.IsDeleted && c.Event.ApplicationUserId == orderInfo.UpdateUserId);

                if (contractInDb != null)
                {
                    var saveMoment = DateTime.Now;

                    contractInDb.ContractDate = orderInfo.ContractDate.Date;
                    contractInDb.OrderDate = orderInfo.OrderDate;
                    contractInDb.FutureEventCd = orderInfo.FutureEventCd;

                    contractInDb.DepartmentCd1 = orderInfo.DepartmentCd1;
                    contractInDb.ArtistCd1 = orderInfo.ArtistCd1;
                    contractInDb.ArtistName1 = orderInfo.ArtistName1;
                    contractInDb.ProductCd1 = orderInfo.ProductCd1;
                    contractInDb.ProductName1 = orderInfo.ProductName1;
                    contractInDb.TechniqueCd1 = orderInfo.TechniqueCd1;
                    contractInDb.TechniqueName1 = orderInfo.TechniqueName1;
                    contractInDb.ProductQuantity1 = orderInfo.ProductQuantity1;
                    contractInDb.ProductPrice1 = orderInfo.ProductPrice1;
                    contractInDb.ProductRemarks1 = orderInfo.DepartmentCd1.HasValue ? orderInfo.ProductRemarks1 : null;
                    contractInDb.CashVoucherValue1 = orderInfo.CashVoucherValue1;
                    contractInDb.DeliveryDate1 = orderInfo.DeliveryDate1;
                    contractInDb.DeliveryTime1 = orderInfo.DepartmentCd1.HasValue ? orderInfo.DeliveryTime1 : null;
                    contractInDb.DeliveryPlace1 = orderInfo.DepartmentCd1.HasValue ? orderInfo.DeliveryPlace1 : null;
                    contractInDb.ProductUnitTaxPrice1 = orderInfo.DepartmentCd1.HasValue ? orderInfo.ProductUnitTaxPrice1 : null;
                    contractInDb.ProductTaxPrice1 = orderInfo.DepartmentCd1.HasValue ? orderInfo.ProductTaxPrice1 : null;
                    contractInDb.ProductDiscount1 = orderInfo.ProductDiscount1;
                    contractInDb.ProductSalesPrice1 = orderInfo.ProductSalesPrice1;

                    contractInDb.DepartmentCd2 = orderInfo.DepartmentCd2;
                    contractInDb.ArtistCd2 = orderInfo.ArtistCd2;
                    contractInDb.ArtistName2 = orderInfo.ArtistName2;
                    contractInDb.ProductCd2 = orderInfo.ProductCd2;
                    contractInDb.ProductName2 = orderInfo.ProductName2;
                    contractInDb.TechniqueCd2 = orderInfo.TechniqueCd2;
                    contractInDb.TechniqueName2 = orderInfo.TechniqueName2;
                    contractInDb.ProductQuantity2 = orderInfo.ProductQuantity2;
                    contractInDb.ProductPrice2 = orderInfo.ProductPrice2;
                    contractInDb.ProductRemarks2 = orderInfo.DepartmentCd2.HasValue ? orderInfo.ProductRemarks2 : null;
                    contractInDb.CashVoucherValue2 = orderInfo.CashVoucherValue2;
                    contractInDb.DeliveryDate2 = orderInfo.DeliveryDate2;
                    contractInDb.DeliveryTime2 = orderInfo.DepartmentCd2.HasValue ? orderInfo.DeliveryTime2 : null;
                    contractInDb.DeliveryPlace2 = orderInfo.DepartmentCd2.HasValue ? orderInfo.DeliveryPlace2 : null;
                    contractInDb.ProductUnitTaxPrice2 = orderInfo.DepartmentCd2.HasValue ? orderInfo.ProductUnitTaxPrice2 : null;
                    contractInDb.ProductTaxPrice2 = orderInfo.DepartmentCd2.HasValue ? orderInfo.ProductTaxPrice2 : null;
                    contractInDb.ProductDiscount2 = orderInfo.ProductDiscount2;
                    contractInDb.ProductSalesPrice2 = orderInfo.ProductSalesPrice2;

                    contractInDb.DepartmentCd3 = orderInfo.DepartmentCd3;
                    contractInDb.ArtistCd3 = orderInfo.ArtistCd3;
                    contractInDb.ArtistName3 = orderInfo.ArtistName3;
                    contractInDb.ProductCd3 = orderInfo.ProductCd3;
                    contractInDb.ProductName3 = orderInfo.ProductName3;
                    contractInDb.TechniqueCd3 = orderInfo.TechniqueCd3;
                    contractInDb.TechniqueName3 = orderInfo.TechniqueName3;
                    contractInDb.ProductQuantity3 = orderInfo.ProductQuantity3;
                    contractInDb.ProductPrice3 = orderInfo.ProductPrice3;
                    contractInDb.ProductRemarks3 = orderInfo.DepartmentCd3.HasValue ? orderInfo.ProductRemarks3 : null;
                    contractInDb.CashVoucherValue3 = orderInfo.CashVoucherValue3;
                    contractInDb.DeliveryDate3 = orderInfo.DeliveryDate3;
                    contractInDb.DeliveryTime3 = orderInfo.DepartmentCd3.HasValue ? orderInfo.DeliveryTime3 : null;
                    contractInDb.DeliveryPlace3 = orderInfo.DepartmentCd3.HasValue ? orderInfo.DeliveryPlace3 : null;
                    contractInDb.ProductUnitTaxPrice3 = orderInfo.DepartmentCd3.HasValue ? orderInfo.ProductUnitTaxPrice3 : null;
                    contractInDb.ProductTaxPrice3 = orderInfo.DepartmentCd3.HasValue ? orderInfo.ProductTaxPrice3 : null;
                    contractInDb.ProductDiscount3 = orderInfo.ProductDiscount3;
                    contractInDb.ProductSalesPrice3 = orderInfo.ProductSalesPrice3;

                    contractInDb.DepartmentCd4 = orderInfo.DepartmentCd4;
                    contractInDb.ArtistCd4 = orderInfo.ArtistCd4;
                    contractInDb.ArtistName4 = orderInfo.ArtistName4;
                    contractInDb.ProductCd4 = orderInfo.ProductCd4;
                    contractInDb.ProductName4 = orderInfo.ProductName4;
                    contractInDb.TechniqueCd4 = orderInfo.TechniqueCd4;
                    contractInDb.TechniqueName4 = orderInfo.TechniqueName4;
                    contractInDb.ProductQuantity4 = orderInfo.ProductQuantity4;
                    contractInDb.ProductPrice4 = orderInfo.ProductPrice4;
                    contractInDb.ProductRemarks4 = orderInfo.DepartmentCd4.HasValue ? orderInfo.ProductRemarks4 : null;
                    contractInDb.CashVoucherValue4 = orderInfo.CashVoucherValue4;
                    contractInDb.DeliveryDate4 = orderInfo.DeliveryDate4;
                    contractInDb.DeliveryTime4 = orderInfo.DepartmentCd4.HasValue ? orderInfo.DeliveryTime4 : null;
                    contractInDb.DeliveryPlace4 = orderInfo.DepartmentCd4.HasValue ? orderInfo.DeliveryPlace4 : null;
                    contractInDb.ProductUnitTaxPrice4 = orderInfo.DepartmentCd4.HasValue ? orderInfo.ProductUnitTaxPrice4 : null;
                    contractInDb.ProductTaxPrice4 = orderInfo.DepartmentCd4.HasValue ? orderInfo.ProductTaxPrice4 : null;
                    contractInDb.ProductDiscount4 = orderInfo.ProductDiscount4;
                    contractInDb.ProductSalesPrice4 = orderInfo.ProductSalesPrice4;

                    contractInDb.DepartmentCd5 = orderInfo.DepartmentCd5;
                    contractInDb.ArtistCd5 = orderInfo.ArtistCd5;
                    contractInDb.ArtistName5 = orderInfo.ArtistName5;
                    contractInDb.ProductCd5 = orderInfo.ProductCd5;
                    contractInDb.ProductName5 = orderInfo.ProductName5;
                    contractInDb.TechniqueCd5 = orderInfo.TechniqueCd5;
                    contractInDb.TechniqueName5 = orderInfo.TechniqueName5;
                    contractInDb.ProductQuantity5 = orderInfo.ProductQuantity5;
                    contractInDb.ProductPrice5 = orderInfo.ProductPrice5;
                    contractInDb.ProductRemarks5 = orderInfo.DepartmentCd5.HasValue ? orderInfo.ProductRemarks5 : null;
                    contractInDb.CashVoucherValue5 = orderInfo.CashVoucherValue5;
                    contractInDb.DeliveryDate5 = orderInfo.DeliveryDate5;
                    contractInDb.DeliveryTime5 = orderInfo.DepartmentCd5.HasValue ? orderInfo.DeliveryTime5 : null;
                    contractInDb.DeliveryPlace5 = orderInfo.DepartmentCd5.HasValue ? orderInfo.DeliveryPlace5 : null;
                    contractInDb.ProductUnitTaxPrice5 = orderInfo.DepartmentCd5.HasValue ? orderInfo.ProductUnitTaxPrice5 : null;
                    contractInDb.ProductTaxPrice5 = orderInfo.DepartmentCd5.HasValue ? orderInfo.ProductTaxPrice5 : null;
                    contractInDb.ProductDiscount5 = orderInfo.ProductDiscount5;
                    contractInDb.ProductSalesPrice5 = orderInfo.ProductSalesPrice5;

                    contractInDb.DepartmentCd6 = orderInfo.DepartmentCd6;
                    contractInDb.ArtistCd6 = orderInfo.ArtistCd6;
                    contractInDb.ArtistName6 = orderInfo.ArtistName6;
                    contractInDb.ProductCd6 = orderInfo.ProductCd6;
                    contractInDb.ProductName6 = orderInfo.ProductName6;
                    contractInDb.TechniqueCd6 = orderInfo.TechniqueCd6;
                    contractInDb.TechniqueName6 = orderInfo.TechniqueName6;
                    contractInDb.ProductQuantity6 = orderInfo.ProductQuantity6;
                    contractInDb.ProductPrice6 = orderInfo.ProductPrice6;
                    contractInDb.ProductRemarks6 = orderInfo.DepartmentCd6.HasValue ? orderInfo.ProductRemarks6 : null;
                    contractInDb.CashVoucherValue6 = orderInfo.CashVoucherValue6;
                    contractInDb.DeliveryDate6 = orderInfo.DeliveryDate6;
                    contractInDb.DeliveryTime6 = orderInfo.DepartmentCd6.HasValue ? orderInfo.DeliveryTime6 : null;
                    contractInDb.DeliveryPlace6 = orderInfo.DepartmentCd6.HasValue ? orderInfo.DeliveryPlace6 : null;
                    contractInDb.ProductUnitTaxPrice6 = orderInfo.DepartmentCd6.HasValue ? orderInfo.ProductUnitTaxPrice6 : null;
                    contractInDb.ProductTaxPrice6 = orderInfo.DepartmentCd6.HasValue ? orderInfo.ProductTaxPrice6 : null;
                    contractInDb.ProductDiscount6 = orderInfo.ProductDiscount6;
                    contractInDb.ProductSalesPrice6 = orderInfo.ProductSalesPrice6;

                    contractInDb.SalesPrice = orderInfo.SalesPrice;
                    contractInDb.Discount = orderInfo.Discount;
                    contractInDb.TaxPrice = orderInfo.TaxPrice;
                    contractInDb.TotalPrice = orderInfo.TotalPrice;
                    contractInDb.IsHaveProductAgreement = orderInfo.IsHaveProductAgreement == (int?)IsHaveProductAgreement.Yes;
                    contractInDb.ProductAgreementTypeCd = orderInfo.ProductAgreementTypeCd;
                    contractInDb.DownPayment = orderInfo.DownPayment;
                    contractInDb.LeftPayment = orderInfo.LeftPayment;
                    contractInDb.DownPaymentMethod = orderInfo.DownPaymentMethod;
                    contractInDb.DownPaymentDate = orderInfo.DownPaymentDate;
                    contractInDb.ReceiptNo = orderInfo.ReceiptNo;
                    contractInDb.LeftPaymentMethod = orderInfo.LeftPaymentMethod;
                    contractInDb.LeftPaymentDate = orderInfo.LeftPaymentDate;
                    contractInDb.LeftPaymentPlace = orderInfo.LeftPaymentPlace;
                    contractInDb.LeftPaymentOtherPlace = orderInfo.LeftPaymentOtherPlace;
                    contractInDb.VerifyNumber = orderInfo.VerifyNumber;

                    contractInDb.UpdateUserId = orderInfo.UpdateUserId;
                    contractInDb.UpdateDate = saveMoment;

                    _context.SaveChanges();

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void MappingArtistAndProductByName(OrderInfo orderInfo)
        {
            orderInfo.ArtistName1 = orderInfo.ArtistName1?.Trim();
            orderInfo.ArtistName2 = orderInfo.ArtistName2?.Trim();
            orderInfo.ArtistName3 = orderInfo.ArtistName3?.Trim();
            orderInfo.ArtistName4 = orderInfo.ArtistName4?.Trim();
            orderInfo.ArtistName5 = orderInfo.ArtistName5?.Trim();
            orderInfo.ArtistName6 = orderInfo.ArtistName6?.Trim();

            orderInfo.ProductName1 = orderInfo.ProductName1?.Trim();
            orderInfo.ProductName2 = orderInfo.ProductName2?.Trim();
            orderInfo.ProductName3 = orderInfo.ProductName3?.Trim();
            orderInfo.ProductName4 = orderInfo.ProductName4?.Trim();
            orderInfo.ProductName5 = orderInfo.ProductName5?.Trim();
            orderInfo.ProductName6 = orderInfo.ProductName6?.Trim();

            orderInfo.ArtistCd1 = GetArtistCdFromNameAndDepartment(orderInfo.DepartmentCd1, orderInfo.ArtistCd1, orderInfo.ArtistName1);
            orderInfo.ArtistCd2 = GetArtistCdFromNameAndDepartment(orderInfo.DepartmentCd2, orderInfo.ArtistCd2, orderInfo.ArtistName2);
            orderInfo.ArtistCd3 = GetArtistCdFromNameAndDepartment(orderInfo.DepartmentCd3, orderInfo.ArtistCd3, orderInfo.ArtistName3);
            orderInfo.ArtistCd4 = GetArtistCdFromNameAndDepartment(orderInfo.DepartmentCd4, orderInfo.ArtistCd4, orderInfo.ArtistName4);
            orderInfo.ArtistCd5 = GetArtistCdFromNameAndDepartment(orderInfo.DepartmentCd5, orderInfo.ArtistCd5, orderInfo.ArtistName5);
            orderInfo.ArtistCd6 = GetArtistCdFromNameAndDepartment(orderInfo.DepartmentCd6, orderInfo.ArtistCd6, orderInfo.ArtistName6);

            orderInfo.ProductCd1 = GetProductCdFromNameAndDepartment(orderInfo.ArtistCd1, orderInfo.ProductCd1, orderInfo.ProductName1);
            orderInfo.ProductCd2 = GetProductCdFromNameAndDepartment(orderInfo.ArtistCd2, orderInfo.ProductCd2, orderInfo.ProductName2);
            orderInfo.ProductCd3 = GetProductCdFromNameAndDepartment(orderInfo.ArtistCd3, orderInfo.ProductCd3, orderInfo.ProductName3);
            orderInfo.ProductCd4 = GetProductCdFromNameAndDepartment(orderInfo.ArtistCd4, orderInfo.ProductCd4, orderInfo.ProductName4);
            orderInfo.ProductCd5 = GetProductCdFromNameAndDepartment(orderInfo.ArtistCd5, orderInfo.ProductCd5, orderInfo.ProductName5);
            orderInfo.ProductCd6 = GetProductCdFromNameAndDepartment(orderInfo.ArtistCd6, orderInfo.ProductCd6, orderInfo.ProductName6);

            orderInfo.TechniqueCd1 = GetTechniqueCdFromName(orderInfo.TechniqueCd1, orderInfo.TechniqueName1);
            orderInfo.TechniqueCd2 = GetTechniqueCdFromName(orderInfo.TechniqueCd2, orderInfo.TechniqueName2);
            orderInfo.TechniqueCd3 = GetTechniqueCdFromName(orderInfo.TechniqueCd3, orderInfo.TechniqueName3);
            orderInfo.TechniqueCd4 = GetTechniqueCdFromName(orderInfo.TechniqueCd4, orderInfo.TechniqueName4);
            orderInfo.TechniqueCd5 = GetTechniqueCdFromName(orderInfo.TechniqueCd5, orderInfo.TechniqueName5);
            orderInfo.TechniqueCd6 = GetTechniqueCdFromName(orderInfo.TechniqueCd6, orderInfo.TechniqueName6);
        }

        private void MappingArtistAndProductByName(ContractForUpdate contractForUpdate)
        {
            contractForUpdate.ArtistName1 = contractForUpdate.ArtistName1?.Trim();
            contractForUpdate.ArtistName2 = contractForUpdate.ArtistName2?.Trim();
            contractForUpdate.ArtistName3 = contractForUpdate.ArtistName3?.Trim();
            contractForUpdate.ArtistName4 = contractForUpdate.ArtistName4?.Trim();
            contractForUpdate.ArtistName5 = contractForUpdate.ArtistName5?.Trim();
            contractForUpdate.ArtistName6 = contractForUpdate.ArtistName6?.Trim();

            contractForUpdate.ProductName1 = contractForUpdate.ProductName1?.Trim();
            contractForUpdate.ProductName2 = contractForUpdate.ProductName2?.Trim();
            contractForUpdate.ProductName3 = contractForUpdate.ProductName3?.Trim();
            contractForUpdate.ProductName4 = contractForUpdate.ProductName4?.Trim();
            contractForUpdate.ProductName5 = contractForUpdate.ProductName5?.Trim();
            contractForUpdate.ProductName6 = contractForUpdate.ProductName6?.Trim();

            contractForUpdate.ArtistCd1 = GetArtistCdFromNameAndDepartment(contractForUpdate.DepartmentCd1, contractForUpdate.ArtistCd1, contractForUpdate.ArtistName1);
            contractForUpdate.ArtistCd2 = GetArtistCdFromNameAndDepartment(contractForUpdate.DepartmentCd2, contractForUpdate.ArtistCd2, contractForUpdate.ArtistName2);
            contractForUpdate.ArtistCd3 = GetArtistCdFromNameAndDepartment(contractForUpdate.DepartmentCd3, contractForUpdate.ArtistCd3, contractForUpdate.ArtistName3);
            contractForUpdate.ArtistCd4 = GetArtistCdFromNameAndDepartment(contractForUpdate.DepartmentCd4, contractForUpdate.ArtistCd4, contractForUpdate.ArtistName4);
            contractForUpdate.ArtistCd5 = GetArtistCdFromNameAndDepartment(contractForUpdate.DepartmentCd5, contractForUpdate.ArtistCd5, contractForUpdate.ArtistName5);
            contractForUpdate.ArtistCd6 = GetArtistCdFromNameAndDepartment(contractForUpdate.DepartmentCd6, contractForUpdate.ArtistCd6, contractForUpdate.ArtistName6);

            contractForUpdate.ProductCd1 = GetProductCdFromNameAndDepartment(contractForUpdate.ArtistCd1, contractForUpdate.ProductCd1, contractForUpdate.ProductName1);
            contractForUpdate.ProductCd2 = GetProductCdFromNameAndDepartment(contractForUpdate.ArtistCd2, contractForUpdate.ProductCd2, contractForUpdate.ProductName2);
            contractForUpdate.ProductCd3 = GetProductCdFromNameAndDepartment(contractForUpdate.ArtistCd3, contractForUpdate.ProductCd3, contractForUpdate.ProductName3);
            contractForUpdate.ProductCd4 = GetProductCdFromNameAndDepartment(contractForUpdate.ArtistCd4, contractForUpdate.ProductCd4, contractForUpdate.ProductName4);
            contractForUpdate.ProductCd5 = GetProductCdFromNameAndDepartment(contractForUpdate.ArtistCd5, contractForUpdate.ProductCd5, contractForUpdate.ProductName5);
            contractForUpdate.ProductCd6 = GetProductCdFromNameAndDepartment(contractForUpdate.ArtistCd6, contractForUpdate.ProductCd6, contractForUpdate.ProductName6);

            contractForUpdate.TechniqueCd1 = GetTechniqueCdFromName(contractForUpdate.TechniqueCd1, contractForUpdate.TechniqueName1);
            contractForUpdate.TechniqueCd2 = GetTechniqueCdFromName(contractForUpdate.TechniqueCd2, contractForUpdate.TechniqueName2);
            contractForUpdate.TechniqueCd3 = GetTechniqueCdFromName(contractForUpdate.TechniqueCd3, contractForUpdate.TechniqueName3);
            contractForUpdate.TechniqueCd4 = GetTechniqueCdFromName(contractForUpdate.TechniqueCd4, contractForUpdate.TechniqueName4);
            contractForUpdate.TechniqueCd5 = GetTechniqueCdFromName(contractForUpdate.TechniqueCd5, contractForUpdate.TechniqueName5);
            contractForUpdate.TechniqueCd6 = GetTechniqueCdFromName(contractForUpdate.TechniqueCd6, contractForUpdate.TechniqueName6);
        }

        private int? GetArtistCdFromNameAndDepartment(int? departmentCd, int? artistCd, string artistName)
        {
            if (string.IsNullOrEmpty(artistName) || !departmentCd.HasValue)
            {
                return null;
            }

            var cd = _context.MstArtists
                        .Where(a => a.Name == artistName && a.ArtistDepartments.Any(ad => ad.DepartmentCd == departmentCd.Value) && !a.IsDeleted)
                        .Select(a => a.Cd)
                        .AsEnumerable()
                        .OrderByDescending(cd => cd == artistCd)
                        .FirstOrDefault();

            return cd > 0 ? cd : (int?)null;
        }

        private int? GetProductCdFromNameAndDepartment(int? artistCd, int? productCd, string productName)
        {
            if (string.IsNullOrEmpty(productName) || !artistCd.HasValue)
            {
                return null;
            }

            var cd = _context.MstProducts
                        .Where(p => p.OriginalName == productName && p.ArtistCd == artistCd.Value && !p.IsDeleted)
                        .Select(p => p.Cd)
                        .AsEnumerable()
                        .OrderByDescending(cd => cd == productCd)
                        .FirstOrDefault();

            return cd > 0 ? cd : (int?)null;
        }

        private int? GetTechniqueCdFromName(int? techniqueCd, string techniqueName)
        {
            if (string.IsNullOrEmpty(techniqueName))
            {
                return null;
            }

            var cd = _context.MstTechniques
                        .Where(t => t.Name == techniqueName && !t.IsDeleted)
                        .Select(t => t.Cd)
                        .AsEnumerable()
                        .OrderByDescending(cd => cd == techniqueCd)
                        .FirstOrDefault();

            return cd > 0 ? cd : (int?)null;
        }

        public bool CreateCustomerInfo(CustomerInfo customerInfo)
        {
            try
            {
                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == customerInfo.ContractCd && !c.IsDeleted && c.Event.ApplicationUserId == customerInfo.UpdateUserId);

                if (contractInDb != null)
                {
                    var saveMoment = DateTime.Now;

                    contractInDb.FamilyName = customerInfo.FamilyName;
                    contractInDb.FirstName = customerInfo.FirstName;
                    contractInDb.FamilyNameFuri = customerInfo.FamilyNameFuri;
                    contractInDb.FirstNameFuri = customerInfo.FirstNameFuri;
                    contractInDb.Gender = customerInfo.Gender;
                    contractInDb.DateOfBirth = customerInfo.DateOfBirth.Value.Date;
                    contractInDb.Age = customerInfo.Age;
                    contractInDb.Zipcode = customerInfo.Zipcode;
                    contractInDb.Province = customerInfo.Province;
                    contractInDb.Address = customerInfo.Address;
                    contractInDb.Building = customerInfo.Building;
                    contractInDb.ProvinceFuri = customerInfo.ProvinceFuri;
                    contractInDb.AddressFuri = customerInfo.AddressFuri;
                    contractInDb.BuildingFuri = customerInfo.BuildingFuri;
                    contractInDb.HomePhone = customerInfo.HomePhone;
                    contractInDb.PhoneBranch = customerInfo.PhoneBranch;
                    contractInDb.Mobiphone = customerInfo.Mobiphone;
                    contractInDb.Email = customerInfo.Email;

                    contractInDb.UpdateUserId = customerInfo.UpdateUserId;
                    contractInDb.UpdateDate = saveMoment;

                    _context.SaveChanges();

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateCustomerWorkplace(CustomerWorkplace customerWorkplace)
        {
            try
            {
                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == customerWorkplace.ContractCd && !c.IsDeleted && c.Event.ApplicationUserId == customerWorkplace.UpdateUserId);

                if (contractInDb != null)
                {
                    var saveMoment = DateTime.Now;

                    contractInDb.CompanyName = customerWorkplace.CompanyName;
                    contractInDb.CompanyNameFuri = customerWorkplace.CompanyNameFuri;
                    contractInDb.CompanyPhone = customerWorkplace.CompanyPhone;
                    contractInDb.CompanyLocalPhone = customerWorkplace.CompanyLocalPhone;
                    contractInDb.CompanyZipCode = customerWorkplace.CompanyZipCode;
                    contractInDb.CompanyProvince = customerWorkplace.CompanyProvince;
                    contractInDb.CompanyAddress = customerWorkplace.CompanyAddress;
                    contractInDb.CompanyBuilding = customerWorkplace.CompanyBuilding;
                    contractInDb.CompanyProvinceFuri = customerWorkplace.CompanyProvinceFuri;
                    contractInDb.CompanyAddressFuri = customerWorkplace.CompanyAddressFuri;
                    contractInDb.CompanyBuildingFuri = customerWorkplace.CompanyBuildingFuri;

                    contractInDb.UpdateUserId = customerWorkplace.UpdateUserId;
                    contractInDb.UpdateDate = saveMoment;

                    _context.SaveChanges();

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool RegisterClub(ClubRegistration clubRegistration)
        {
            try
            {
                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == clubRegistration.ContractCd && !c.IsDeleted && c.Event.ApplicationUserId == clubRegistration.UpdateUserId);

                if (contractInDb != null)
                {
                    var saveMoment = DateTime.Now;

                    contractInDb.ClubRegistrationStatus = clubRegistration.ClubRegistrationStatus;
                    contractInDb.MemberId = clubRegistration.MemberId;
                    contractInDb.IsConfirmMember = clubRegistration.IsConfirmMember;
                    contractInDb.ConfirmMemberTime = clubRegistration.IsConfirmMember ? saveMoment : (DateTime?)null;
                    contractInDb.MemberCard = clubRegistration.MemberCard;

                    contractInDb.UpdateUserId = clubRegistration.UpdateUserId;
                    contractInDb.UpdateDate = saveMoment;

                    _context.SaveChanges();

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool RegisterAssociation(AssociationRegistration assoRegistration)
        {
            try
            {
                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == assoRegistration.ContractCd && !c.IsDeleted && c.Event.ApplicationUserId == assoRegistration.UpdateUserId);

                if (contractInDb != null)
                {
                    var saveMoment = DateTime.Now;

                    contractInDb.MediaCd = assoRegistration.MediaCd;
                    contractInDb.VisitTime = assoRegistration.VisitTime;
                    contractInDb.ClubJoinStatus = assoRegistration.ClubJoinStatus;
                    contractInDb.ClubJoinedText = assoRegistration.ClubJoinedText;
                    contractInDb.MemberJoinStatus = assoRegistration.MemberJoinStatus;
                    contractInDb.MemberJoinedText = assoRegistration.MemberJoinedText;
                    contractInDb.IdentifyDocument = assoRegistration.IdentifyDocument;
                    contractInDb.OtherIdentifyDoc = assoRegistration.OtherIdentifyDoc;
                    contractInDb.Approve = assoRegistration.Approve;
                    contractInDb.Remark = assoRegistration.Remark;

                    contractInDb.UpdateUserId = assoRegistration.UpdateUserId;
                    contractInDb.UpdateDate = saveMoment;

                    _context.SaveChanges();

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public ContractForConfirm GetContractDetailForConfirm(int cd)
        {
            var result = _context.Contracts
                        .Where(c => c.Cd == cd && !c.IsDeleted)
                        .Select(c => new
                        {
                            c.Cd,
                            c.SalesDepartment,
                            c.SalesmanSCd,
                            SalesmanSCode        = c.SalesmanS.Code,
                            SalesmanSName        = c.SalesmanS.Name,
                            SalesmanCCode        = c.SalesmanC.Code,
                            SalesmanCName        = c.SalesmanC.Name,
                            SalesmanACode        = c.SalesmanA.Code,
                            SalesmanAName        = c.SalesmanA.Name,
                            Organization         = c.Organization.Name,
                            c.Inputter,
                            c.IsConfNotice,
                            c.IsConfCoolingOff,
                            c.IsConfPersonalInfo,

                            c.ContractDate,
                            c.OrderDate,
                            c.FutureEventCd,
                            FutureEventName      = c.FutureEvent.Name,
                            FutureEventStartDate = c.FutureEvent.StartDate,
                            FutureEventEndDate   = c.FutureEvent.EndDate,
                            c.DepartmentCd1,
                            DeparmentName1       = c.Department1.Name,
                            c.ArtistName1,
                            c.ProductName1,
                            //TechniqueName1     = c.Technique1.Name,
                            TechniqueName1       = c.TechniqueName1,
                            c.ProductPrice1,
                            c.ProductDiscount1,
                            c.ProductQuantity1,
                            c.ProductSalesPrice1,
                            c.ProductRemarks1,
                            c.CashVoucherValue1,
                            c.DeliveryDate1,
                            c.DeliveryTime1,
                            c.DeliveryPlace1,
                            c.DepartmentCd2,
                            DeparmentName2       = c.Department2.Name,
                            c.ArtistName2,
                            c.ProductName2,
                            //TechniqueName2     = c.Technique2.Name,
                            TechniqueName2       = c.TechniqueName2,
                            c.ProductPrice2,
                            c.ProductDiscount2,
                            c.ProductQuantity2,
                            c.ProductSalesPrice2,
                            c.ProductRemarks2,
                            c.CashVoucherValue2,
                            c.DeliveryDate2,
                            c.DeliveryTime2,
                            c.DeliveryPlace2,
                            c.DepartmentCd3,
                            DeparmentName3       = c.Department3.Name,
                            c.ArtistName3,
                            c.ProductName3,
                            //TechniqueName3     = c.Technique3.Name,
                            TechniqueName3       = c.TechniqueName3,
                            c.ProductPrice3,
                            c.ProductDiscount3,
                            c.ProductQuantity3,
                            c.ProductSalesPrice3,
                            c.ProductRemarks3,
                            c.CashVoucherValue3,
                            c.DeliveryDate3,
                            c.DeliveryTime3,
                            c.DeliveryPlace3,
                            c.DepartmentCd4,
                            DeparmentName4       = c.Department4.Name,
                            c.ArtistName4,
                            c.ProductName4,
                            //TechniqueName4     = c.Technique4.Name,
                            TechniqueName4       = c.TechniqueName4,
                            c.ProductPrice4,
                            c.ProductDiscount4,
                            c.ProductQuantity4,
                            c.ProductSalesPrice4,
                            c.ProductRemarks4,
                            c.CashVoucherValue4,
                            c.DeliveryDate4,
                            c.DeliveryTime4,
                            c.DeliveryPlace4,
                            c.DepartmentCd5,
                            DeparmentName5       = c.Department5.Name,
                            c.ArtistName5,
                            c.ProductName5,
                            TechniqueName5       = c.TechniqueName5,
                            c.ProductPrice5,
                            c.ProductDiscount5,
                            c.ProductQuantity5,
                            c.ProductSalesPrice5,
                            c.ProductRemarks5,
                            c.CashVoucherValue5,
                            c.DeliveryDate5,
                            c.DeliveryTime5,
                            c.DeliveryPlace5,
                            c.DepartmentCd6,
                            DeparmentName6       = c.Department6.Name,
                            c.ArtistName6,
                            c.ProductName6,
                            //TechniqueName6     = c.Technique6.Name,
                            TechniqueName6       = c.TechniqueName6,
                            c.ProductPrice6,
                            c.ProductDiscount6,
                            c.ProductQuantity6,
                            c.ProductSalesPrice6,
                            c.ProductRemarks6,
                            c.CashVoucherValue6,
                            c.DeliveryDate6,
                            c.DeliveryTime6,
                            c.DeliveryPlace6,
                            c.SalesPrice,
                            c.TaxPrice,
                            c.TotalPrice,
                            c.IsHaveProductAgreement,
                            ProductAgreementType = c.ProductAgreementType.Name,
                            c.DownPayment,
                            c.LeftPayment,
                            c.DownPaymentDate,
                            c.DownPaymentMethod,
                            c.ReceiptNo,
                            c.LeftPaymentDate,
                            c.LeftPaymentMethod,
                            LeftPaymentPlace     = c.Payment.Name,
                            c.LeftPaymentOtherPlace,
                            c.VerifyNumber,

                            c.FamilyName,
                            c.FirstName,
                            c.FamilyNameFuri,
                            c.FirstNameFuri,
                            c.Gender,
                            c.DateOfBirth,
                            c.Age,
                            c.Zipcode,
                            c.Province,
                            c.Address,
                            c.Building,
                            c.ProvinceFuri,
                            c.AddressFuri,
                            c.BuildingFuri,
                            c.HomePhone,
                            c.PhoneBranch,
                            c.Mobiphone,
                            c.Email,

                            c.CompanyName,
                            c.CompanyNameFuri,
                            c.CompanyPhone,
                            c.CompanyLocalPhone,
                            c.CompanyZipCode,
                            c.CompanyProvince,
                            c.CompanyAddress,
                            c.CompanyBuilding,
                            c.CompanyProvinceFuri,
                            c.CompanyAddressFuri,
                            c.CompanyBuildingFuri,

                            Media                = c.Media.Name,
                            c.VisitTime,
                            c.ClubJoinStatus,
                            c.ClubJoinedText,
                            c.MemberJoinStatus,
                            c.MemberJoinedText,
                            c.IdentifyDocument,
                            c.OtherIdentifyDoc,
                            c.Approve,
                            c.Remark,
                            c.ClubRegistrationStatus,
                            c.MemberId,
                            c.IsConfirmMember,
                            c.MemberCard,
                        })
                        .AsEnumerable()
                        .Select(c => new ContractForConfirm
                        {
                            Cd                     = c.Cd,
                            SalesDepartment        = ((SalesDepartment)c.SalesDepartment).GetEnumDescription(),
                            SalesmanS              = c.SalesmanSCd.HasValue ? $"{c.SalesmanSCode} - {c.SalesmanSName}" : "",
                            SalesmanC              = $"{c.SalesmanCCode} - {c.SalesmanCName}",
                            SalesmanA              = $"{c.SalesmanACode} - {c.SalesmanAName}",
                            Organization           = c.Organization,
                            Inputter               = ((Inputter)c.Inputter).GetEnumDescription(),

                            IsConfNotice           = c.IsConfNotice.HasValue ? (c.IsConfNotice.Value ? "有" : "") : "",
                            IsConfCoolingOff       = c.IsConfCoolingOff.HasValue ? (c.IsConfCoolingOff.Value ? "有" : "") : "",
                            IsConfPersonalInfo     = c.IsConfPersonalInfo.HasValue ? (c.IsConfPersonalInfo.Value ? "有" : "") : "",

                            ContractDate           = c.ContractDate?.ToString(ExactDateFormat),
                            OrderDate              = c.OrderDate?.ToString(ExactDateFormat),
                            FutureEvent            = c.FutureEventCd.HasValue ? $"{c.FutureEventName} ー {c.FutureEventStartDate?.ToString(ExactDateFormatJP)}～{c.FutureEventEndDate?.ToString(ExactDateFormatJP)}" : "",
                            DepartmentCd1          = c.DepartmentCd1,
                            DepartmentName1        = c.DeparmentName1,
                            ArtistName1            = c.ArtistName1,
                            ProductName1           = c.ProductName1,
                            TechniqueName1         = c.TechniqueName1,
                            ProductPrice1          = c.ProductPrice1?.ToString("N0"),
                            ProductDiscount1       = c.ProductDiscount1?.ToString("N0"),
                            ProductQuantity1       = c.ProductQuantity1?.ToString("N0"),
                            ProductSalesPrice1     = c.DepartmentCd1.HasValue ? c.ProductSalesPrice1?.ToString("N0") : "",
                            ProductRemark1         = TransformRemarkString(c.ProductRemarks1),
                            CashVoucherValue1      = c.CashVoucherValue1.HasValue ? ((CashVoucherValue)c.CashVoucherValue1.Value).GetEnumDescription() : "",
                            DeliveryDateTime1      = c.DepartmentCd1.HasValue ? $"{c.DeliveryDate1?.ToString(ExactDateFormat)} {((DeliveryTime)c.DeliveryTime1.Value).GetEnumDescription()}" : "",
                            DeliveryPlace1         = c.DepartmentCd1.HasValue ? ((DeliveryPlace)c.DeliveryPlace1.Value).GetEnumDescription() : "",
                            DepartmentCd2          = c.DepartmentCd2,
                            DepartmentName2        = c.DeparmentName2,
                            ArtistName2            = c.ArtistName2,
                            ProductName2           = c.ProductName2,
                            TechniqueName2         = c.TechniqueName2,
                            ProductPrice2          = c.ProductPrice2?.ToString("N0"),
                            ProductDiscount2       = c.ProductDiscount2?.ToString("N0"),
                            ProductQuantity2       = c.ProductQuantity2?.ToString("N0"),
                            ProductSalesPrice2     = c.DepartmentCd2.HasValue ? c.ProductSalesPrice2?.ToString("N0") : "",
                            ProductRemark2         = TransformRemarkString(c.ProductRemarks2),
                            CashVoucherValue2      = c.CashVoucherValue2.HasValue ? ((CashVoucherValue)c.CashVoucherValue2.Value).GetEnumDescription() : "",
                            DeliveryDateTime2      = c.DepartmentCd2.HasValue ? $"{c.DeliveryDate2?.ToString(ExactDateFormat)} {((DeliveryTime)c.DeliveryTime2.Value).GetEnumDescription()}" : "",
                            DeliveryPlace2         = c.DepartmentCd2.HasValue ? ((DeliveryPlace)c.DeliveryPlace2.Value).GetEnumDescription() : "",
                            DepartmentCd3          = c.DepartmentCd3,
                            DepartmentName3        = c.DeparmentName3,
                            ArtistName3            = c.ArtistName3,
                            ProductName3           = c.ProductName3,
                            TechniqueName3         = c.TechniqueName3,
                            ProductPrice3          = c.ProductPrice3?.ToString("N0"),
                            ProductDiscount3       = c.ProductDiscount3?.ToString("N0"),
                            ProductQuantity3       = c.ProductQuantity3?.ToString("N0"),
                            ProductSalesPrice3     = c.DepartmentCd3.HasValue ? c.ProductSalesPrice3?.ToString("N0") : "",
                            ProductRemark3         = TransformRemarkString(c.ProductRemarks3),
                            CashVoucherValue3      = c.CashVoucherValue3.HasValue ? ((CashVoucherValue)c.CashVoucherValue3.Value).GetEnumDescription() : "",
                            DeliveryDateTime3      = c.DepartmentCd3.HasValue ? $"{c.DeliveryDate3?.ToString(ExactDateFormat)} {((DeliveryTime)c.DeliveryTime3.Value).GetEnumDescription()}" : "",
                            DeliveryPlace3         = c.DepartmentCd3.HasValue ? ((DeliveryPlace)c.DeliveryPlace3.Value).GetEnumDescription() : "",
                            DepartmentCd4          = c.DepartmentCd4,
                            DepartmentName4        = c.DeparmentName4,
                            ArtistName4            = c.ArtistName4,
                            ProductName4           = c.ProductName4,
                            TechniqueName4         = c.TechniqueName4,
                            ProductPrice4          = c.ProductPrice4?.ToString("N0"),
                            ProductDiscount4       = c.ProductDiscount4?.ToString("N0"),
                            ProductQuantity4       = c.ProductQuantity4?.ToString("N0"),
                            ProductSalesPrice4     = c.DepartmentCd4.HasValue ? c.ProductSalesPrice4?.ToString("N0") : "",
                            ProductRemark4         = TransformRemarkString(c.ProductRemarks4),
                            CashVoucherValue4      = c.CashVoucherValue4.HasValue ? ((CashVoucherValue)c.CashVoucherValue4.Value).GetEnumDescription() : "",
                            DeliveryDateTime4      = c.DepartmentCd4.HasValue ? $"{c.DeliveryDate4?.ToString(ExactDateFormat)} {((DeliveryTime)c.DeliveryTime4.Value).GetEnumDescription()}" : "",
                            DeliveryPlace4         = c.DepartmentCd4.HasValue ? ((DeliveryPlace)c.DeliveryPlace4.Value).GetEnumDescription() : "",
                            DepartmentCd5          = c.DepartmentCd5,
                            DepartmentName5        = c.DeparmentName5,
                            ArtistName5            = c.ArtistName5,
                            ProductName5           = c.ProductName5,
                            TechniqueName5         = c.TechniqueName5,
                            ProductPrice5          = c.ProductPrice5?.ToString("N0"),
                            ProductDiscount5       = c.ProductDiscount5?.ToString("N0"),
                            ProductQuantity5       = c.ProductQuantity5?.ToString("N0"),
                            ProductSalesPrice5     = c.DepartmentCd5.HasValue ? c.ProductSalesPrice5?.ToString("N0") : "",
                            ProductRemark5         = TransformRemarkString(c.ProductRemarks5),
                            CashVoucherValue5      = c.CashVoucherValue5.HasValue ? ((CashVoucherValue)c.CashVoucherValue5.Value).GetEnumDescription() : "",
                            DeliveryDateTime5      = c.DepartmentCd5.HasValue ? $"{c.DeliveryDate5?.ToString(ExactDateFormat)} {((DeliveryTime)c.DeliveryTime5.Value).GetEnumDescription()}" : "",
                            DeliveryPlace5         = c.DepartmentCd5.HasValue ? ((DeliveryPlace)c.DeliveryPlace5.Value).GetEnumDescription() : "",
                            DepartmentCd6          = c.DepartmentCd6,
                            DepartmentName6        = c.DeparmentName6,
                            ArtistName6            = c.ArtistName6,
                            ProductName6           = c.ProductName6,
                            TechniqueName6         = c.TechniqueName6,
                            ProductPrice6          = c.ProductPrice6?.ToString("N0"),
                            ProductDiscount6       = c.ProductDiscount6?.ToString("N0"),
                            ProductQuantity6       = c.ProductQuantity6?.ToString("N0"),
                            ProductSalesPrice6     = c.DepartmentCd6.HasValue ? c.ProductSalesPrice6?.ToString("N0") : "",
                            ProductRemark6         = TransformRemarkString(c.ProductRemarks6),
                            CashVoucherValue6      = c.CashVoucherValue6.HasValue ? ((CashVoucherValue)c.CashVoucherValue6.Value).GetEnumDescription() : "",
                            DeliveryDateTime6      = c.DepartmentCd6.HasValue ? $"{c.DeliveryDate6?.ToString(ExactDateFormat)} {((DeliveryTime)c.DeliveryTime6.Value).GetEnumDescription()}" : "",
                            DeliveryPlace6         = c.DepartmentCd6.HasValue ? ((DeliveryPlace)c.DeliveryPlace6.Value).GetEnumDescription() : "",
                            SalesPrice             = c.SalesPrice?.ToString("N0"),
                            TaxPrice               = c.TaxPrice?.ToString("N0"),
                            TotalPrice             = c.TotalPrice?.ToString("N0"),
                            IsHaveProductAgreement = c.IsHaveProductAgreement.HasValue ? (c.IsHaveProductAgreement.Value ? IsHaveProductAgreement.Yes : IsHaveProductAgreement.No).GetEnumDescription() : "",
                            ProductAgreementType   = (c.IsHaveProductAgreement ?? false) ? c.ProductAgreementType : "",
                            DownPayment            = c.DownPayment?.ToString("N0"),
                            LeftPayment            = c.LeftPayment?.ToString("N0"),
                            DownPaymentDate        = c.DownPaymentDate?.ToString(ExactDateFormat),
                            DownPaymentMethod      = c.DownPaymentMethod.HasValue ? ((DownPaymentMethod)c.DownPaymentMethod.Value).GetEnumDescription() : "",
                            ReceiptNo              = c.ReceiptNo,
                            LeftPaymentDate        = c.LeftPaymentDate?.ToString(ExactDateFormat),
                            LeftPaymentMethod      = c.LeftPaymentMethod.HasValue ? ((LeftPaymentMethod)c.LeftPaymentMethod.Value).GetEnumDescription() : "",
                            LeftPaymentPlace       = (c.LeftPaymentMethod == (int?)LeftPaymentMethod.Card || c.LeftPaymentMethod == (int?)LeftPaymentMethod.Credit) ? c.LeftPaymentPlace : "",
                            LeftPaymentOtherPlace  = (!string.IsNullOrEmpty(c.LeftPaymentPlace) && c.LeftPaymentPlace.StartsWith("その他")) ? c.LeftPaymentOtherPlace : "",
                            //VerifyNumber         = (!string.IsNullOrEmpty(c.LeftPaymentPlace) && 
                            //                    (c.LeftPaymentPlace.StartsWith("オリエントコーポレーション") ||
                            //                    c.LeftPaymentPlace.StartsWith("ｵﾘｴﾝﾄｺｰﾎﾟﾚｰｼｮﾝ") ||
                            //                    c.LeftPaymentPlace.StartsWith("オリコ") ||
                            //                    c.LeftPaymentPlace.StartsWith("ｵﾘｺ")
                            //                    )) ? c.VerifyNumber : "",

                            VerifyNumber           = (!string.IsNullOrEmpty(c.LeftPaymentPlace) &&
                                                        (c.LeftPaymentPlace.Contains("オリエントコーポレーション") ||
                                                        c.LeftPaymentPlace.Contains("ｵﾘｴﾝﾄｺｰﾎﾟﾚｰｼｮﾝ") ||
                                                        c.LeftPaymentPlace.Contains("オリコ") ||
                                                        c.LeftPaymentPlace.Contains("ｵﾘｺ")
                                                        )) ? c.VerifyNumber : "",

                            CustomerName           = $"{c.FamilyName} {c.FirstName}",
                            CustomerNameFuri       = $"{c.FamilyNameFuri} {c.FirstNameFuri}",
                            Gender                 = c.Gender.HasValue ? ((Gender)c.Gender.Value).GetEnumDescription() : "",
                            DateOfBirth            = $"{c.DateOfBirth?.ToString(ExactDateFormat)} ({c.Age?.ToString()}歳)",
                            Zipcode                = c.Zipcode,
                            Province               = c.Province,
                            Address                = c.Address,
                            Building               = c.Building,
                            ProvinceFuri           = c.ProvinceFuri,
                            AddressFuri            = c.AddressFuri,
                            BuildingFuri           = c.BuildingFuri,
                            HomePhone              = c.HomePhone,
                            PhoneBranch            = c.PhoneBranch.HasValue ? ((PhoneBranch)c.PhoneBranch.Value).GetEnumDescription() : "",
                            Mobiphone              = c.Mobiphone,
                            Email                  = c.Email,

                            CompanyName            = c.CompanyName,
                            CompanyNameFuri        = c.CompanyNameFuri,
                            CompanyPhone           = c.CompanyPhone,
                            CompanyLocalPhone      = c.CompanyLocalPhone,
                            CompanyZipCode         = c.CompanyZipCode,
                            CompanyProvince        = c.CompanyProvince,
                            CompanyAddress         = c.CompanyAddress,
                            CompanyBuilding        = c.CompanyBuilding,
                            CompanyProvinceFuri    = c.CompanyProvinceFuri,
                            CompanyAddressFuri     = c.CompanyAddressFuri,
                            CompanyBuildingFuri    = c.CompanyBuildingFuri,

                            Media                  = c.Media,
                            VisitTime              = c.VisitTime.HasValue ? ((VisitTime)c.VisitTime).GetEnumDescription() : "",
                            ClubJoinStatus         = c.ClubJoinStatus.HasValue ? ((StatusJoinClubMember)c.ClubJoinStatus.Value).GetEnumDescription() : "",
                            ClubJoinedText         = c.ClubJoinStatus == (int?)StatusJoinClubMember.Joined ? c.ClubJoinedText : "",
                            MemberJoinStatus       = c.MemberJoinStatus.HasValue ? ((StatusJoinClubMember)c.MemberJoinStatus.Value).GetEnumDescription() : "",
                            MemberJoinedText       = c.MemberJoinStatus == (int?)StatusJoinClubMember.Joined ? c.MemberJoinedText : "",
                            IdentifyDocument       = c.IdentifyDocument.HasValue ? ((IdentifyDocument)c.IdentifyDocument.Value).GetEnumDescription() : "",
                            OtherIdentifyDoc       = c.IdentifyDocument == (int?)IdentifyDocument.Other ? c.OtherIdentifyDoc : "",
                            Approve                = c.Approve,
                            Remark                 = c.Remark,
                            ClubRegistrationStatus = c.ClubRegistrationStatus.HasValue ? ((ClubJoin)c.ClubRegistrationStatus.Value).GetEnumDescription() : "",
                            MemberId               = c.ClubRegistrationStatus == (int?)ClubJoin.Joined ? c.MemberId : "",
                            IsConfirmMember        = c.IsConfirmMember.HasValue ? (c.IsConfirmMember.Value ? "有" : "") : "",
                            MemberCard             = c.MemberCard.HasValue ? ((MemberCard)c.MemberCard).GetEnumDescription() : "",
                        })
                        .FirstOrDefault();

            return result;
        }

        private string TransformRemarkString(string remarkStr)
        {
            var result = "";

            if (!string.IsNullOrEmpty(remarkStr))
            {
                var remarkCds = Commons.SplitStringToNumbers(remarkStr);
                var remarkTextList = new List<string>();

                for (var i = 0; i < remarkCds.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            if(remarkCds[i] > 0)
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

                result = string.Join(", ", remarkTextList);
            }

            return result;
        }

        private string TransformRemarkStringForPrint(string remarkStr, int? cashVoucherValue)
        {
            var result = "";

            if (!string.IsNullOrEmpty(remarkStr))
            {
                var remarkCds = Commons.SplitStringToNumbers(remarkStr);
                var remarkTextList = new List<string>();

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
                        case 3:
                            if (remarkCds[i] > 0)
                            {
                                if(remarkCds[i] == (int)ProductRemarkD.CashVoucher && cashVoucherValue.HasValue)
                                {
                                    remarkTextList.Add($"{ProductRemarkD.CashVoucher.GetEnumDescription()} {((CashVoucherValue)cashVoucherValue.Value).GetEnumDescription()}");
                                }
                                else
                                {
                                    remarkTextList.Add(((ProductRemarkD)remarkCds[i]).GetEnumDescription());
                                }
                            }
                            break;
                    }
                }

                result = string.Join(", ", remarkTextList);
            }

            return result;
        }

        public bool ConfirmContract(int cd, string userId, bool isPrinted, int? oldContractCd)
        {
            try
            {
                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == cd && !c.IsDeleted && c.Event.ApplicationUserId == userId);

                if (contractInDb != null)
                {
                    var now = DateTime.Now;
                    contractInDb.IsCompleted = true;
                    contractInDb.IsPrinted = isPrinted;

                    contractInDb.UpdateUserId = userId;
                    contractInDb.UpdateDate = now;

                    if (oldContractCd.HasValue)
                    {
                        var oldContractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == oldContractCd.Value && !c.IsDeleted && c.Event.ApplicationUserId == userId);

                        if(oldContractInDb != null)
                        {
                            if(oldContractInDb.OldId == null)
                            {
                                contractInDb.OldId = oldContractInDb.Id;
                            }
                            else
                            {
                                contractInDb.OldId = oldContractInDb.OldId;
                            }

                            oldContractInDb.IsEdited = true;
                            oldContractInDb.IsDeleted = true;
                            oldContractInDb.UpdateUserId = userId;
                            oldContractInDb.UpdateDate = now;
                        }
                        else
                        {
                            throw new Exception("Old contract does not exist!");
                        }
                    }

                    _context.SaveChanges();

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateContractId(int eventCd, string eventCode)
        {
            var lastId = _context.Contracts
                        .Where(c => c.EventCd == eventCd)
                        .Select(c => c.Id)
                        .OrderByDescending(id => id)
                        .FirstOrDefault();

            if (!string.IsNullOrEmpty(lastId))
            {
                var lastIndex = int.Parse(lastId.Substring(Math.Max(0, lastId.Length - 3)));
                var currentIndex = (++lastIndex).ToString("D3");

                return $"{eventCode}{currentIndex}";
            }
            else // for first contract of event
            {
                return $"{eventCode}001";
            }
        }

        public ContractForUpdate GetContractForUpdate(int cd, string userId)
        {
            var result = _context.Contracts
                        .Where(c => c.Cd == cd && c.Event.ApplicationUserId == userId && !c.IsDeleted && c.IsCompleted)
                        .Select(c => new
                        {
                            c.Cd,
                            c.SalesDepartment,
                            c.SalesmanSCd,
                            c.SalesmanCCd,
                            c.SalesmanACd,
                            c.OrganizationCd,

                            c.ContractDate,
                            c.OrderDate,
                            c.FutureEventCd,
                            c.DepartmentCd1,
                            c.ArtistCd1,
                            c.ArtistName1,
                            c.ProductCd1,
                            c.ProductName1,
                            c.TechniqueCd1,
                            c.TechniqueName1,
                            c.ProductPrice1,
                            c.ProductDiscount1,
                            c.ProductQuantity1,
                            c.ProductSalesPrice1,
                            c.ProductRemarks1,
                            c.CashVoucherValue1,
                            c.DeliveryDate1,
                            c.DeliveryTime1,
                            c.DeliveryPlace1,
                            c.DepartmentCd2,
                            c.ArtistCd2,
                            c.ArtistName2,
                            c.ProductCd2,
                            c.ProductName2,
                            c.TechniqueCd2,
                            c.TechniqueName2,
                            c.ProductPrice2,
                            c.ProductDiscount2,
                            c.ProductQuantity2,
                            c.ProductSalesPrice2,
                            c.ProductRemarks2,
                            c.CashVoucherValue2,
                            c.DeliveryDate2,
                            c.DeliveryTime2,
                            c.DeliveryPlace2,
                            c.DepartmentCd3,
                            c.ArtistCd3,
                            c.ArtistName3,
                            c.ProductCd3,
                            c.ProductName3,
                            c.TechniqueCd3,
                            c.TechniqueName3,
                            c.ProductPrice3,
                            c.ProductDiscount3,
                            c.ProductQuantity3,
                            c.ProductSalesPrice3,
                            c.ProductRemarks3,
                            c.CashVoucherValue3,
                            c.DeliveryDate3,
                            c.DeliveryTime3,
                            c.DeliveryPlace3,
                            c.DepartmentCd4,
                            c.ArtistCd4,
                            c.ArtistName4,
                            c.ProductCd4,
                            c.ProductName4,
                            c.TechniqueCd4,
                            c.TechniqueName4,
                            c.ProductPrice4,
                            c.ProductDiscount4,
                            c.ProductQuantity4,
                            c.ProductSalesPrice4,
                            c.ProductRemarks4,
                            c.CashVoucherValue4,
                            c.DeliveryDate4,
                            c.DeliveryTime4,
                            c.DeliveryPlace4,
                            c.DepartmentCd5,
                            c.ArtistCd5,
                            c.ArtistName5,
                            c.ProductCd5,
                            c.ProductName5,
                            c.TechniqueCd5,
                            c.TechniqueName5,
                            c.ProductPrice5,
                            c.ProductDiscount5,
                            c.ProductQuantity5,
                            c.ProductSalesPrice5,
                            c.ProductRemarks5,
                            c.CashVoucherValue5,
                            c.DeliveryDate5,
                            c.DeliveryTime5,
                            c.DeliveryPlace5,
                            c.DepartmentCd6,
                            c.ArtistCd6,
                            c.ArtistName6,
                            c.ProductCd6,
                            c.ProductName6,
                            c.TechniqueCd6,
                            c.TechniqueName6,
                            c.ProductPrice6,
                            c.ProductDiscount6,
                            c.ProductQuantity6,
                            c.ProductSalesPrice6,
                            c.ProductRemarks6,
                            c.CashVoucherValue6,
                            c.DeliveryDate6,
                            c.DeliveryTime6,
                            c.DeliveryPlace6,
                            c.SalesPrice,
                            c.TaxPrice,
                            c.TotalPrice,
                            c.IsHaveProductAgreement,
                            c.ProductAgreementTypeCd,
                            c.DownPayment,
                            c.LeftPayment,
                            c.DownPaymentDate,
                            c.DownPaymentMethod,
                            c.ReceiptNo,
                            c.LeftPaymentDate,
                            c.LeftPaymentMethod,
                            c.LeftPaymentPlace,
                            LeftPaymentPlaceText = c.Payment.Name,
                            c.LeftPaymentOtherPlace,
                            c.VerifyNumber,

                            c.FamilyName,
                            c.FirstName,
                            c.FamilyNameFuri,
                            c.FirstNameFuri,
                            c.Gender,
                            c.DateOfBirth,
                            c.Age,
                            c.Zipcode,
                            c.Province,
                            c.Address,
                            c.Building,
                            c.ProvinceFuri,
                            c.AddressFuri,
                            c.BuildingFuri,
                            c.HomePhone,
                            c.PhoneBranch,
                            c.Mobiphone,
                            c.Email,

                            c.CompanyName,
                            c.CompanyNameFuri,
                            c.CompanyPhone,
                            c.CompanyLocalPhone,
                            c.CompanyZipCode,
                            c.CompanyProvince,
                            c.CompanyAddress,
                            c.CompanyBuilding,
                            c.CompanyProvinceFuri,
                            c.CompanyAddressFuri,
                            c.CompanyBuildingFuri,

                            c.MediaCd,
                            c.VisitTime,
                            c.ClubJoinStatus,
                            c.ClubJoinedText,
                            c.MemberJoinStatus,
                            c.MemberJoinedText,
                            c.IdentifyDocument,
                            c.OtherIdentifyDoc,
                            c.Approve,
                            c.Remark,
                            c.ClubRegistrationStatus,
                            c.MemberId,
                            c.IsConfirmMember,
                            c.MemberCard,
                        })
                        .AsEnumerable()
                        .Select(c => new ContractForUpdate
                        {
                            Cd = c.Cd,
                            SalesDepartment = c.SalesDepartment,
                            SalesmanSCd = c.SalesmanSCd,
                            SalesmanCCd = c.SalesmanCCd,
                            SalesmanACd = c.SalesmanACd,
                            OrganizationCd = c.OrganizationCd,

                            ContractDate = c.ContractDate,
                            OrderDate = c.OrderDate,
                            FutureEventCd = c.FutureEventCd,
                            TaxValue = GetTaxValueByDate(c.ContractDate.Value.Date),
                            DepartmentCd1 = c.DepartmentCd1,
                            ArtistCd1 = c.ArtistCd1,
                            ArtistName1 = c.ArtistName1,
                            ProductCd1 = c.ProductCd1,
                            ProductName1 = c.ProductName1,
                            TechniqueCd1 = c.TechniqueCd1,
                            TechniqueName1 = c.TechniqueName1,
                            ProductPrice1 = c.ProductPrice1,
                            ProductDiscount1 = c.ProductDiscount1,
                            ProductQuantity1 = c.ProductQuantity1,
                            ProductSalesPrice1 = c.ProductSalesPrice1,
                            ProductRemarks1 = c.ProductRemarks1,
                            CashVoucherValue1 = c.CashVoucherValue1,
                            DeliveryDate1 = c.DeliveryDate1,
                            DeliveryTime1 = c.DeliveryTime1,
                            DeliveryPlace1 = c.DeliveryPlace1,
                            DepartmentCd2 = c.DepartmentCd2,
                            ArtistCd2 = c.ArtistCd2,
                            ArtistName2 = c.ArtistName2,
                            ProductCd2 = c.ProductCd2,
                            ProductName2 = c.ProductName2,
                            TechniqueCd2 = c.TechniqueCd2,
                            TechniqueName2 = c.TechniqueName2,
                            ProductPrice2 = c.ProductPrice2,
                            ProductDiscount2 = c.ProductDiscount2,
                            ProductQuantity2 = c.ProductQuantity2,
                            ProductSalesPrice2 = c.ProductSalesPrice2,
                            ProductRemarks2 = c.ProductRemarks2,
                            CashVoucherValue2 = c.CashVoucherValue2,
                            DeliveryDate2 = c.DeliveryDate2,
                            DeliveryTime2 = c.DeliveryTime2,
                            DeliveryPlace2 = c.DeliveryPlace2,
                            DepartmentCd3 = c.DepartmentCd3,
                            ArtistCd3 = c.ArtistCd3,
                            ArtistName3 = c.ArtistName3,
                            ProductCd3 = c.ProductCd3,
                            ProductName3 = c.ProductName3,
                            TechniqueCd3 = c.TechniqueCd3,
                            TechniqueName3 = c.TechniqueName3,
                            ProductPrice3 = c.ProductPrice3,
                            ProductDiscount3 = c.ProductDiscount3,
                            ProductQuantity3 = c.ProductQuantity3,
                            ProductSalesPrice3 = c.ProductSalesPrice3,
                            ProductRemarks3 = c.ProductRemarks3,
                            CashVoucherValue3 = c.CashVoucherValue3,
                            DeliveryDate3 = c.DeliveryDate3,
                            DeliveryTime3 = c.DeliveryTime3,
                            DeliveryPlace3 = c.DeliveryPlace3,
                            DepartmentCd4 = c.DepartmentCd4,
                            ArtistCd4 = c.ArtistCd4,
                            ArtistName4 = c.ArtistName4,
                            ProductCd4 = c.ProductCd4,
                            ProductName4 = c.ProductName4,
                            TechniqueCd4 = c.TechniqueCd4,
                            TechniqueName4 = c.TechniqueName4,
                            ProductPrice4 = c.ProductPrice4,
                            ProductDiscount4 = c.ProductDiscount4,
                            ProductQuantity4 = c.ProductQuantity4,
                            ProductSalesPrice4 = c.ProductSalesPrice4,
                            ProductRemarks4 = c.ProductRemarks4,
                            CashVoucherValue4 = c.CashVoucherValue4,
                            DeliveryDate4 = c.DeliveryDate4,
                            DeliveryTime4 = c.DeliveryTime4,
                            DeliveryPlace4 = c.DeliveryPlace4,
                            DepartmentCd5 = c.DepartmentCd5,
                            ArtistCd5 = c.ArtistCd5,
                            ArtistName5 = c.ArtistName5,
                            ProductCd5 = c.ProductCd5,
                            ProductName5 = c.ProductName5,
                            TechniqueCd5 = c.TechniqueCd5,
                            TechniqueName5 = c.TechniqueName5,
                            ProductPrice5 = c.ProductPrice5,
                            ProductDiscount5 = c.ProductDiscount5,
                            ProductQuantity5 = c.ProductQuantity5,
                            ProductSalesPrice5 = c.ProductSalesPrice5,
                            ProductRemarks5 = c.ProductRemarks5,
                            CashVoucherValue5 = c.CashVoucherValue5,
                            DeliveryDate5 = c.DeliveryDate5,
                            DeliveryTime5 = c.DeliveryTime5,
                            DeliveryPlace5 = c.DeliveryPlace5,
                            DepartmentCd6 = c.DepartmentCd6,
                            ArtistCd6 = c.ArtistCd6,
                            ArtistName6 = c.ArtistName6,
                            ProductCd6 = c.ProductCd6,
                            ProductName6 = c.ProductName6,
                            TechniqueCd6 = c.TechniqueCd6,
                            TechniqueName6 = c.TechniqueName6,
                            ProductPrice6 = c.ProductPrice6,
                            ProductDiscount6 = c.ProductDiscount6,
                            ProductQuantity6 = c.ProductQuantity6,
                            ProductSalesPrice6 = c.ProductSalesPrice6,
                            ProductRemarks6 = c.ProductRemarks6,
                            CashVoucherValue6 = c.CashVoucherValue6,
                            DeliveryDate6 = c.DeliveryDate6,
                            DeliveryTime6 = c.DeliveryTime6,
                            DeliveryPlace6 = c.DeliveryPlace6,
                            SalesPrice = c.SalesPrice,
                            TaxPrice = c.TaxPrice,
                            TotalPrice = c.TotalPrice,
                            IsHaveProductAgreement = c.IsHaveProductAgreement.HasValue 
                                                    ? (c.IsHaveProductAgreement.Value ? (int?)IsHaveProductAgreement.Yes : (int?)IsHaveProductAgreement.No)
                                                    : null,
                            ProductAgreementTypeCd = c.ProductAgreementTypeCd,
                            DownPayment = c.DownPayment,
                            LeftPayment = c.LeftPayment,
                            DownPaymentDate = c.DownPaymentDate,
                            DownPaymentMethod = c.DownPaymentMethod,
                            ReceiptNo = c.ReceiptNo,
                            LeftPaymentDate = c.LeftPaymentDate,
                            LeftPaymentMethod = c.LeftPaymentMethod,
                            LeftPaymentPlace = c.LeftPaymentPlace,
                            LeftPaymentPlaceText = c.LeftPaymentPlaceText,
                            LeftPaymentOtherPlace = c.LeftPaymentOtherPlace,
                            VerifyNumber = c.VerifyNumber,

                            FirstName = c.FirstName,
                            FamilyName = c.FamilyName,
                            FirstNameFuri = c.FirstNameFuri,
                            FamilyNameFuri = c.FamilyNameFuri,
                            Gender = c.Gender,
                            DateOfBirth = c.DateOfBirth,
                            Age = c.Age,
                            Zipcode = c.Zipcode,
                            Province = c.Province,
                            Address = c.Address,
                            Building = c.Building,
                            ProvinceFuri = c.ProvinceFuri,
                            AddressFuri = c.AddressFuri,
                            BuildingFuri = c.BuildingFuri,
                            HomePhone = c.HomePhone,
                            PhoneBranch = c.PhoneBranch,
                            Mobiphone = c.Mobiphone,
                            Email = c.Email,

                            CompanyName = c.CompanyName,
                            CompanyNameFuri = c.CompanyNameFuri,
                            CompanyPhone = c.CompanyPhone,
                            CompanyLocalPhone = c.CompanyLocalPhone,
                            CompanyZipCode = c.CompanyZipCode,
                            CompanyProvince = c.CompanyProvince,
                            CompanyAddress = c.CompanyAddress,
                            CompanyBuilding = c.CompanyBuilding,
                            CompanyProvinceFuri = c.CompanyProvinceFuri,
                            CompanyAddressFuri = c.CompanyAddressFuri,
                            CompanyBuildingFuri = c.CompanyBuildingFuri,

                            MediaCd = c.MediaCd,
                            VisitTime = c.VisitTime,
                            ClubJoinStatus = c.ClubJoinStatus,
                            ClubJoinedText = c.ClubJoinedText,
                            MemberJoinStatus = c.MemberJoinStatus,
                            MemberJoinedText = c.MemberJoinedText,
                            IdentifyDocument = c.IdentifyDocument,
                            OtherIdentifyDoc = c.OtherIdentifyDoc,
                            Approve = c.Approve,
                            Remark = c.Remark,
                            ClubRegistrationStatus = c.ClubRegistrationStatus,
                            MemberId = c.MemberId,
                            IsConfirmMember = c.IsConfirmMember ?? false,
                            MemberCard = c.MemberCard,
                        })
                        .FirstOrDefault();

            if(result != null)
            {
                result.SplitProductRemarkValues();
            }

            return result;
        }

        public List<ProductListItem> GetProductListForOrder(string artistName, string productName)
        {
            var products = _context.MstProducts.Where(p => !p.IsDeleted && !p.Artist.IsDeleted);

            if (!string.IsNullOrEmpty(artistName))
            {
                var key = artistName.Trim().ToUpper();
                products = products.Where(p => p.Artist.Name.ToUpper().Contains(key) || p.Artist.NameKana.ToUpper().Contains(key));
            }

            if (!string.IsNullOrEmpty(productName))
            {
                var key = productName.Trim().ToUpper();
                products = products.Where(p => p.OriginalName.ToUpper().Contains(key) || p.NameKana.ToUpper().Contains(key));
            }

            var result = products
                        .Select(p => new
                        {
                            p.ArtistCd,
                            ArtistName = p.Artist.Name,
                            ArtistCode = p.Artist.Code,
                            p.Cd,
                            p.OriginalName,
                            p.Code,
                            DepartmentCd = p.Artist.ArtistDepartments.Select(ad => ad.DepartmentCd).FirstOrDefault(),
                            TechniqueCd = p.ProductTechniques.Select(pt => pt.TechniqueCd).FirstOrDefault(),
                            TechniqueName = p.ProductTechniques.Select(pt => pt.Technique.Name).FirstOrDefault()
                        })
                        .AsEnumerable()
                        .Select(p => new ProductListItem
                        {
                            ArtistCd = p.ArtistCd.ToString(),
                            ArtistName = p.ArtistName,
                            ArtistCode = p.ArtistCode,
                            Cd = p.Cd.ToString(),
                            Name = p.OriginalName,
                            Code = p.Code,
                            DepartmentCd = p.DepartmentCd > 0 ? p.DepartmentCd.ToString() : "",
                            TechniqueCd = p.TechniqueCd > 0 ? p.TechniqueCd.ToString() : "",
                            TechniqueName = p.TechniqueName
                        })
                        .OrderBy(p => p.ArtistCode)
                        .ThenBy(p => p.Code)
                        .ToList();

            return result ?? new List<ProductListItem>();
        }

        public int UpdateContract(ContractForUpdate contract)
        {
            try
            {
                MappingArtistAndProductByName(contract);
                contract.CalculateBeforeSave();
                contract.JoinProductRemarkValues();

                var contractInDb = _context.Contracts.FirstOrDefault(c => c.Cd == contract.Cd && !c.IsDeleted && c.Event.ApplicationUserId == contract.UpdateUserId);

                if (contractInDb != null)
                {
                    var updateMoment = DateTime.Now;

                    var newContract = new Contract
                    {
                        EventCd = contractInDb.EventCd,
                        Id = GenerateContractId(contractInDb.EventCd, contract.EventCode),

                        SalesDepartment = contract.SalesDepartment,
                        SalesmanSCd = contract.SalesmanSCd,
                        SalesmanCCd = contract.SalesmanCCd,
                        SalesmanACd = contract.SalesmanACd,
                        OrganizationCd = contract.OrganizationCd,
                        Inputter = contractInDb.Inputter,

                        ConfNoticeTime = contractInDb.ConfNoticeTime,
                        ConfCoolingOffTime = contractInDb.ConfCoolingOffTime,
                        ConfPersonalInfoTime = contractInDb.ConfPersonalInfoTime,
                        IsConfNotice = contractInDb.IsConfNotice,
                        IsConfCoolingOff = contractInDb.IsConfCoolingOff,
                        IsConfPersonalInfo = contractInDb.IsConfPersonalInfo,

                        ContractDate = contract.ContractDate,
                        OrderDate = contract.OrderDate,
                        FutureEventCd = contract.FutureEventCd,

                        DepartmentCd1 = contract.DepartmentCd1,
                        ArtistCd1 = contract.ArtistCd1,
                        ArtistName1 = contract.ArtistName1,
                        ProductCd1 = contract.ProductCd1,
                        ProductName1 = contract.ProductName1,
                        TechniqueCd1 = contract.TechniqueCd1,
                        TechniqueName1 = contract.TechniqueName1,
                        ProductQuantity1 = contract.ProductQuantity1,
                        ProductPrice1 = contract.ProductPrice1,
                        DeliveryDate1 = contract.DeliveryDate1,
                        DeliveryTime1 = contract.DepartmentCd1.HasValue ? contract.DeliveryTime1 : null,
                        DeliveryPlace1 = contract.DepartmentCd1.HasValue ? contract.DeliveryPlace1 : null,
                        ProductUnitTaxPrice1 = contract.DepartmentCd1.HasValue ? contract.ProductUnitTaxPrice1 : null,
                        ProductTaxPrice1 = contract.DepartmentCd1.HasValue ? contract.ProductTaxPrice1 : null,
                        ProductDiscount1 = contract.ProductDiscount1,
                        ProductSalesPrice1 = contract.ProductSalesPrice1,
                        ProductRemarks1 = contract.DepartmentCd1.HasValue ? contract.ProductRemarks1 : null,
                        CashVoucherValue1 = contract.CashVoucherValue1,

                        DepartmentCd2 = contract.DepartmentCd2,
                        ArtistCd2 = contract.ArtistCd2,
                        ArtistName2 = contract.ArtistName2,
                        ProductCd2 = contract.ProductCd2,
                        ProductName2 = contract.ProductName2,
                        TechniqueCd2 = contract.TechniqueCd2,
                        TechniqueName2 = contract.TechniqueName2,
                        ProductQuantity2 = contract.ProductQuantity2,
                        ProductPrice2 = contract.ProductPrice2,
                        DeliveryDate2 = contract.DeliveryDate2,
                        DeliveryTime2 = contract.DepartmentCd2.HasValue ? contract.DeliveryTime2 : null,
                        DeliveryPlace2 = contract.DepartmentCd2.HasValue ? contract.DeliveryPlace2 : null,
                        ProductUnitTaxPrice2 = contract.DepartmentCd2.HasValue ? contract.ProductUnitTaxPrice2 : null,
                        ProductTaxPrice2 = contract.DepartmentCd2.HasValue ? contract.ProductTaxPrice2 : null,
                        ProductDiscount2 = contract.ProductDiscount2,
                        ProductSalesPrice2 = contract.ProductSalesPrice2,
                        ProductRemarks2 = contract.DepartmentCd2.HasValue ? contract.ProductRemarks2 : null,
                        CashVoucherValue2 = contract.CashVoucherValue2,

                        DepartmentCd3 = contract.DepartmentCd3,
                        ArtistCd3 = contract.ArtistCd3,
                        ArtistName3 = contract.ArtistName3,
                        ProductCd3 = contract.ProductCd3,
                        ProductName3 = contract.ProductName3,
                        TechniqueCd3 = contract.TechniqueCd3,
                        TechniqueName3 = contract.TechniqueName3,
                        ProductQuantity3 = contract.ProductQuantity3,
                        ProductPrice3 = contract.ProductPrice3,
                        DeliveryDate3 = contract.DeliveryDate3,
                        DeliveryTime3 = contract.DepartmentCd3.HasValue ? contract.DeliveryTime3 : null,
                        DeliveryPlace3 = contract.DepartmentCd3.HasValue ? contract.DeliveryPlace3 : null,
                        ProductUnitTaxPrice3 = contract.DepartmentCd3.HasValue ? contract.ProductUnitTaxPrice3 : null,
                        ProductTaxPrice3 = contract.DepartmentCd3.HasValue ? contract.ProductTaxPrice3 : null,
                        ProductDiscount3 = contract.ProductDiscount3,
                        ProductSalesPrice3 = contract.ProductSalesPrice3,
                        ProductRemarks3 = contract.DepartmentCd3.HasValue ? contract.ProductRemarks3 : null,
                        CashVoucherValue3 = contract.CashVoucherValue3,

                        DepartmentCd4 = contract.DepartmentCd4,
                        ArtistCd4 = contract.ArtistCd4,
                        ArtistName4 = contract.ArtistName4,
                        ProductCd4 = contract.ProductCd4,
                        ProductName4 = contract.ProductName4,
                        TechniqueCd4 = contract.TechniqueCd4,
                        TechniqueName4 = contract.TechniqueName4,
                        ProductQuantity4 = contract.ProductQuantity4,
                        ProductPrice4 = contract.ProductPrice4,
                        DeliveryDate4 = contract.DeliveryDate4,
                        DeliveryTime4 = contract.DepartmentCd4.HasValue ? contract.DeliveryTime4 : null,
                        DeliveryPlace4 = contract.DepartmentCd4.HasValue ? contract.DeliveryPlace4 : null,
                        ProductUnitTaxPrice4 = contract.DepartmentCd4.HasValue ? contract.ProductUnitTaxPrice4 : null,
                        ProductTaxPrice4 = contract.DepartmentCd4.HasValue ? contract.ProductTaxPrice4 : null,
                        ProductDiscount4 = contract.ProductDiscount4,
                        ProductSalesPrice4 = contract.ProductSalesPrice4,
                        ProductRemarks4 = contract.DepartmentCd4.HasValue ? contract.ProductRemarks4 : null,
                        CashVoucherValue4 = contract.CashVoucherValue4,

                        DepartmentCd5 = contract.DepartmentCd5,
                        ArtistCd5 = contract.ArtistCd5,
                        ArtistName5 = contract.ArtistName5,
                        ProductCd5 = contract.ProductCd5,
                        ProductName5 = contract.ProductName5,
                        TechniqueCd5 = contract.TechniqueCd5,
                        TechniqueName5 = contract.TechniqueName5,
                        ProductQuantity5 = contract.ProductQuantity5,
                        ProductPrice5 = contract.ProductPrice5,
                        DeliveryDate5 = contract.DeliveryDate5,
                        DeliveryTime5 = contract.DepartmentCd5.HasValue ? contract.DeliveryTime5 : null,
                        DeliveryPlace5 = contract.DepartmentCd5.HasValue ? contract.DeliveryPlace5 : null,
                        ProductUnitTaxPrice5 = contract.DepartmentCd5.HasValue ? contract.ProductUnitTaxPrice5 : null,
                        ProductTaxPrice5 = contract.DepartmentCd5.HasValue ? contract.ProductTaxPrice5 : null,
                        ProductDiscount5 = contract.ProductDiscount5,
                        ProductSalesPrice5 = contract.ProductSalesPrice5,
                        ProductRemarks5 = contract.DepartmentCd5.HasValue ? contract.ProductRemarks5 : null,
                        CashVoucherValue5 = contract.CashVoucherValue5,

                        DepartmentCd6 = contract.DepartmentCd6,
                        ArtistCd6 = contract.ArtistCd6,
                        ArtistName6 = contract.ArtistName6,
                        ProductCd6 = contract.ProductCd6,
                        ProductName6 = contract.ProductName6,
                        TechniqueCd6 = contract.TechniqueCd6,
                        TechniqueName6 = contract.TechniqueName6,
                        ProductQuantity6 = contract.ProductQuantity6,
                        ProductPrice6 = contract.ProductPrice6,
                        DeliveryDate6 = contract.DeliveryDate6,
                        DeliveryTime6 = contract.DepartmentCd6.HasValue ? contract.DeliveryTime6 : null,
                        DeliveryPlace6 = contract.DepartmentCd6.HasValue ? contract.DeliveryPlace6 : null,
                        ProductUnitTaxPrice6 = contract.DepartmentCd6.HasValue ? contract.ProductUnitTaxPrice6 : null,
                        ProductTaxPrice6 = contract.DepartmentCd6.HasValue ? contract.ProductTaxPrice6 : null,
                        ProductDiscount6 = contract.ProductDiscount6,
                        ProductSalesPrice6 = contract.ProductSalesPrice6,
                        ProductRemarks6 = contract.DepartmentCd6.HasValue ? contract.ProductRemarks6 : null,
                        CashVoucherValue6 = contract.CashVoucherValue6,

                        SalesPrice = contract.SalesPrice,
                        Discount = contract.Discount,
                        TaxPrice = contract.TaxPrice,
                        TotalPrice = contract.TotalPrice,
                        IsHaveProductAgreement = contract.IsHaveProductAgreement == (int?)IsHaveProductAgreement.Yes,
                        ProductAgreementTypeCd = contract.ProductAgreementTypeCd,
                        DownPayment = contract.DownPayment,
                        LeftPayment = contract.LeftPayment,
                        DownPaymentMethod = contract.DownPaymentMethod,
                        DownPaymentDate = contract.DownPaymentDate,
                        ReceiptNo = contract.ReceiptNo,
                        LeftPaymentMethod = contract.LeftPaymentMethod,
                        LeftPaymentDate = contract.LeftPaymentDate,
                        LeftPaymentPlace = contract.LeftPaymentPlace,
                        LeftPaymentOtherPlace = contract.LeftPaymentOtherPlace,
                        VerifyNumber = contract.VerifyNumber,

                        FamilyName = contract.FamilyName,
                        FirstName = contract.FirstName,
                        FamilyNameFuri = contract.FamilyNameFuri,
                        FirstNameFuri = contract.FirstNameFuri,
                        Gender = contract.Gender,
                        DateOfBirth = contract.DateOfBirth.Value.Date,
                        Age = contract.Age,
                        Zipcode = contract.Zipcode,
                        Province = contract.Province,
                        Address = contract.Address,
                        Building = contract.Building,
                        ProvinceFuri = contract.ProvinceFuri,
                        AddressFuri = contract.AddressFuri,
                        BuildingFuri = contract.BuildingFuri,
                        HomePhone = contract.HomePhone,
                        PhoneBranch = contract.PhoneBranch,
                        Mobiphone = contract.Mobiphone,
                        Email = contract.Email,

                        CompanyName = contract.CompanyName,
                        CompanyNameFuri = contract.CompanyNameFuri,
                        CompanyPhone = contract.CompanyPhone,
                        CompanyLocalPhone = contract.CompanyLocalPhone,
                        CompanyZipCode = contract.CompanyZipCode,
                        CompanyProvince = contract.CompanyProvince,
                        CompanyAddress = contract.CompanyAddress,
                        CompanyBuilding = contract.CompanyBuilding,
                        CompanyProvinceFuri = contract.CompanyProvinceFuri,
                        CompanyAddressFuri = contract.CompanyAddressFuri,
                        CompanyBuildingFuri = contract.CompanyBuildingFuri,

                        MediaCd = contract.MediaCd,
                        VisitTime = contract.VisitTime,
                        ClubJoinStatus = contract.ClubJoinStatus,
                        ClubJoinedText = contract.ClubJoinedText,
                        MemberJoinStatus = contract.MemberJoinStatus,
                        MemberJoinedText = contract.MemberJoinedText,
                        IdentifyDocument = contract.IdentifyDocument,
                        OtherIdentifyDoc = contract.OtherIdentifyDoc,
                        Approve = contract.Approve,
                        Remark = contract.Remark,
                        ClubRegistrationStatus = contract.ClubRegistrationStatus,
                        MemberId = contract.MemberId,
                        IsConfirmMember = contract.IsConfirmMember,
                        ConfirmMemberTime = !(contractInDb.IsConfirmMember ?? false) && contract.IsConfirmMember
                                            ? updateMoment
                                            : !contract.IsConfirmMember ? null : contractInDb.ConfNoticeTime,
                        MemberCard = contract.MemberCard,

                        InsertDate = updateMoment,
                        InsertUserId = contract.UpdateUserId,
                    };

                    contractInDb.UpdateUserId = contract.UpdateUserId;
                    contractInDb.UpdateDate = updateMoment;

                    _context.Contracts.Add(newContract);

                    while (true)
                    {
                        try
                        {
                            _context.SaveChanges();
                            break;
                        }
                        catch (DbUpdateException e) when (e.InnerException?.InnerException is MySqlException sqlEx && sqlEx.Number == 1062)
                        {
                            newContract.Id = GenerateContractId(contractInDb.EventCd, contract.EventCode);
                        }
                    }

                    return newContract.Cd;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public AjaxResponseModel FormatTelephone(string input)
        {
            var result = new AjaxResponseModel();

            if (string.IsNullOrEmpty(input))
            {
                result.Status = false;
                return result;
            }

            input = input.Trim();

            if (input.Length < 6)
            {
                result.Status = false;
                return result;
            }

            var remains = input.Substring(6);

            var telCode = _context.MstTelephones
                            .Where(t => input.StartsWith(t.Number))
                            .Select(t => new
                            {
                                t.AreaCode,
                                t.CityCode,
                            })
                            .AsEnumerable()
                            .Select(t => $"{t.AreaCode}-{t.CityCode}-")
                            .FirstOrDefault();

            if (string.IsNullOrEmpty(telCode))
            {
                result.Status = false;
            }
            else
            {
                result.Status = true;
                result.Message = telCode + remains;
            }

            return result;
        }
    }
}
