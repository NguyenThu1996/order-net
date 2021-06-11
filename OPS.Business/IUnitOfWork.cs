using OPS.Business.IRepository;
using OPS.Entity.Schemas;

namespace OPS.Business
{
    public interface IUnitOfWork
    {
        IAccountRepository<ApplicationUser> AccountRepository { get; }

        IMstCareerRepository<MstCareer> MstCareerRepository { get; }

        IMstTechniqueRepository<MstTechnique> MstTechniqueRepository { get; }

        IMstDepartmentRepository<MstDepartment> MstDepartmentRepository { get; }

        ISurveyRepository SurveyRepository { get; }

        IMasterDataRepository MasterDataRepository { get; }

        IMstPaymentRepository<MstPayment> MstPaymentRepository { get; }

        IMstTaxRepository<MstTax> MstTaxRepository { get; }

        IMstArtistRepository<MstArtist> MstArtistRepository { get; }

        IMstEventRepository<MstEvent> MstEventRepository { get; }

        IOrderReportRepository OrderReportRepository { get; }

        IMstProductRepository<MstProduct> MstProductRepository { get; }

        IContractRepository ContractRepository { get; }

        IMstMediaRepository<MstMedia> mstMediaRepository { get; }

        IMstCompanyTypeRepository<MstCompanyType> MstCompanyTypeRepository { get; }

        ISalemanRegisterRepository<MstSalesman> SalemanRegisterRepository { get; }

        IMstOrganizationRepository<MstOrganization> MstOrganizationRepository{ get; }

        IMstProductAgreementTypeRepository<MstProductAgreementType> MstProductAgreementTypeRepository { get; }

        IPurchaseStatisticsRepository PurchaseStatisticsRepository { get; }

        IExportASRepository ExportAsRepository { get; }

        IMapRepository MapRepository { get; }

        IMstSalesmanRepository<MstSalesman> MstSalesmanRepository { get; }
        void SaveChanges();
    }
}
