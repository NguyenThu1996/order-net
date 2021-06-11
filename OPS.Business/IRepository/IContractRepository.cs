using OPS.ViewModels.Shared;
using OPS.ViewModels.User.Contract;
using OPS.ViewModels.User.Contract.Details;
using OPS.ViewModels.User.Contract.Search;
using System;
using System.Collections.Generic;

namespace OPS.Business.IRepository
{
    public interface IContractRepository
    {
        int GetEventCdByUserId(string userId);
        int CreateContractPIC(ContractPICInfo contractPIC);
        bool ConfirmTermAndCodition(TermAndCondition confirmInfo);
        string GetEventCodeByUserId(string userId);
        ContractForPrint GetContractForPrint(int contractCd, string eventCode);
        void CheckContractPrinted(int contractCd);
        ContractListSearchModel GetListContractByUser(ContractListSearchModel contractListSearch);
        ContractDetailModel GetContractDetail(int cd, string userCode);
        AjaxResponseModel CancelContract(int cd, string userId);
        int? GetTaxValueByDate(DateTime date);
        bool CreateOrderInfo(OrderInfo orderInfo);
        bool CreateCustomerInfo(CustomerInfo customerInfo);
        bool CreateCustomerWorkplace(CustomerWorkplace customerWorkplace);
        bool RegisterClub(ClubRegistration clubRegistration);
        bool RegisterAssociation(AssociationRegistration assoRegistration);
        ContractForConfirm GetContractDetailForConfirm(int cd);
        bool ConfirmContract(int cd, string userId, bool isPrinted, int? oldContractCd);
        ContractForUpdate GetContractForUpdate(int cd, string userId);
        List<ProductListItem> GetProductListForOrder(string artistName, string productName);
        int UpdateContract(ContractForUpdate contract);
        AjaxResponseModel FormatTelephone(string input);
    }
}
