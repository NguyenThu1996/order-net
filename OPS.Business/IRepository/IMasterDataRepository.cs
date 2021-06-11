using Microsoft.AspNetCore.Mvc.Rendering;
using OPS.ViewModels.Admin.Master.Event;
using OPS.ViewModels.User.Contract;
using System;
using System.Collections.Generic;

namespace OPS.Business.IRepository
{
    public interface IMasterDataRepository
    {
        List<SelectListItem> GetSelectListMedia(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListAgeRange();
        List<SelectListItem> GetSelectListCareer();
        List<SelectListItem> GetSelectListSalesDepartment(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListEventSalesman(string userId, bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListOrganization(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListInputter(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListArtist(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListDeliveryTime(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListDeliveryPlace(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListDownPaymentMethod(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListLeftPaymentMethod(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListLeftPaymentPlace(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListEvent(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListSalesMen(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListLeftPaymentMethods(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListClubs(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListNumberOfVisit(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListRemarks(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectsListSalesman();
        List<SelectListItem> GetSelectListProvince(bool isNullItemFirst = false);
        List<ProductSelectListItem> GetSelectListProductByArtistCd(int artistCd, bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListGender(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListPhoneBranch(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListClubJoinStatus(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListIdentifyDoc(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListClubRegStatus(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListMemberCard(bool isNullItemFirst = false);
        string[] GetListAddress(string key);
        TechniqueSelectListItem[] GetListTechnique(string key, string productName);
        AddressViewModel GetAddressByZipcode(string zip1, string zip2);
        List<SelectListItem> GetEmptySelectList();
        List<SelectListItem> GetSelectListLeftPaymentPlace(int paymentMethod, int saleDepartment);
        DateTime[] GetEventStartAndEndDate(string userId);
        DateTimeEventModel GetDateTimeEvent(int eventCd);
        List<SelectListItem> GetSelectListSalesmanByEvent(int eventCd, bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListIsHaveProductAgreement(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListProductAgreementType(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListVisitTime(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListDepartment(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListDepartments(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListEventMedia(string eventCode, bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListEventMedias(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListTechnique(bool isNullItemFirst = false);
        List<SelectListItem> GetListArtistByDepartmentAndProduct(string productName = "", int? departmentCd = null);
        List<ProductSelectListItem> GetListProductByDepartmentAndArtist(string artistName = "", int? departmentCd = null);
        List<SelectListItem> GetSelectListTechniqueByProductName(string productName = "", bool isNullItemFirst = false);
        ProductSearchByCodeResult GetProductByCode(int? departmentCd, string code);
        List<EventSelectListItem> GetSelectListFutureEvent(string userId, DateTime currentEventStartDate, bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListProductRemarkA(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListProductRemarkB(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListProductRemarkC(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListProductRemarkD(bool isNullItemFirst = false);
        List<SelectListItem> GetSelectListCashVoucherValue(bool isNullItemFirst = false);
    }
}
