using Microsoft.AspNetCore.Identity;
using OPS.Business.IRepository;
using OPS.Business.Repository;
using OPS.Entity;
using OPS.Entity.Schemas;
using OPS.Utility;
using OPS.Utility.Exceptions;
using System;

namespace OPS.Business
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        // ReSharper disable once InconsistentNaming
        public OpsContext _context { get; }

        private IAccountRepository<ApplicationUser> _accountRepository;

        private IMstCareerRepository<MstCareer> _mstCareerRepository;

        private ISurveyRepository _surveyRepository;

        private IMasterDataRepository _masterRepository;

        private IMstPaymentRepository<MstPayment> _mstPaymentRepository;

        private IMstTaxRepository<MstTax> _mstTaxRepository;

        private IMstTechniqueRepository<MstTechnique> _mstTechniqueRepository;

        private IMstDepartmentRepository<MstDepartment> _mstDepartmentRepository;
        
        private IMstArtistRepository<MstArtist> _mstArtistRepository;

        private IMstEventRepository<MstEvent> _mstEventRepository;

        private IOrderReportRepository _orderReportRepository;

        private IMstProductRepository<MstProduct> _mstProductRepository;

        private IMstMediaRepository<MstMedia> _mstMediaRepository;

        private IMstCompanyTypeRepository<MstCompanyType> _mstCompanyTypeRepository;

        private IMstOrganizationRepository<MstOrganization> _mstOrganizationRepository;
        private IMstProductAgreementTypeRepository<MstProductAgreementType> _mstProductAgreementTypeRepository;

        private IMapRepository _mapRepository;

        private UserManager<ApplicationUser> _userManager;

        private RoleManager<ApplicationRole> _roleManager;

        private IContractRepository _contractRepository;

        private ISalemanRegisterRepository<MstSalesman> _salemanRegisterRepository;

        private IPurchaseStatisticsRepository _purchaseStatisticsRepository;

        private IExportASRepository _exportAsRepository;

        private IMstSalesmanRepository<MstSalesman> _mstSalesmanRepository;

        public UnitOfWork(OpsContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public IAccountRepository<ApplicationUser> AccountRepository
        {
            get { return _accountRepository ??= new AccountRepository(_context); }
        }

        public IMstCareerRepository<MstCareer> MstCareerRepository
        {
            get { return _mstCareerRepository ??= new MstCareerRepository(_context); }
        }
        public IMstArtistRepository<MstArtist> MstArtistRepository
        {
            get { return _mstArtistRepository ??= new MstArtistRepository(_context); }
        }

        public IMstEventRepository<MstEvent> MstEventRepository
        {
            get { return _mstEventRepository ??= new MstEventRepository(_context, MasterDataRepository, _userManager, _roleManager); }
        }

        public ISurveyRepository SurveyRepository
        {
            get { return _surveyRepository ??= new SurveyRepository(_context); }
        }

        public IMstPaymentRepository<MstPayment> MstPaymentRepository
        {
            get { return _mstPaymentRepository ??= new MstPaymentRepository(_context); }
        }

        public IMstTaxRepository<MstTax> MstTaxRepository
        {
            get { return _mstTaxRepository ??= new MstTaxRepository(_context); }
        }

        public IMasterDataRepository MasterDataRepository
        {
            get { return _masterRepository ??= new MasterDataRepository(_context); }
        } 

        public IOrderReportRepository OrderReportRepository
        {
            get { return _orderReportRepository ??= new OrderReportRepository(_context); }
        }
        public IContractRepository ContractRepository
        {
            get { return _contractRepository ??= new ContractRepository(_context); }
        }

        public IMstProductRepository<MstProduct> MstProductRepository 
        {
            get { return _mstProductRepository ??= new MstProductRepository(_context); }
        }

        public IMstMediaRepository<MstMedia> mstMediaRepository
        {
            get { return _mstMediaRepository ??= new MstMediaRepository(_context); }
        }

        public IMstCompanyTypeRepository<MstCompanyType> MstCompanyTypeRepository
        {
            get { return _mstCompanyTypeRepository ??= new MstCompanyTypeRepository(_context); }
        }

        public ISalemanRegisterRepository<MstSalesman> SalemanRegisterRepository
        {
            get { return _salemanRegisterRepository ??= new SalemanRegisterRepository(_context); }
        }

        public IMstOrganizationRepository<MstOrganization> MstOrganizationRepository
        {
            get { return _mstOrganizationRepository ??= new MstOrganizationRepository(_context); }
        }

        public IPurchaseStatisticsRepository PurchaseStatisticsRepository
        {
            get { return _purchaseStatisticsRepository ??= new PurchaseStatisticsRepository(_context); }
        }

        public IMstSalesmanRepository<MstSalesman> MstSalesmanRepository
        {
            get { return _mstSalesmanRepository ??= new MstSalesmanRepository(_context); }
        }

        public IExportASRepository ExportAsRepository
        {
            get { return _exportAsRepository ??= new ExportAsRepository(_context); }
        }

        public IMapRepository MapRepository
        {
            get { return _mapRepository ??= new MapRepository(_context); }
        }

        public IMstProductAgreementTypeRepository<MstProductAgreementType> MstProductAgreementTypeRepository
        {
            get { return _mstProductAgreementTypeRepository ??= new MstProductAgreementTypeRepository(_context); }
        }

        public IMstDepartmentRepository<MstDepartment> MstDepartmentRepository
        {
            get { return _mstDepartmentRepository ??= new MstDepartmentRepository(_context); }
        }

        public IMstTechniqueRepository<MstTechnique> MstTechniqueRepository
        {
            get { return _mstTechniqueRepository ??= new MstTechniqueRepository(_context); }
        }

        public void SaveChanges()
        {
            if (_context.SaveChanges() == 0)
            {
                throw new BusinessException((int)GeneralErrors.Failed, "UnexpectedError");
            }
        }

        public async void Dispose()
        {
            await _context.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
