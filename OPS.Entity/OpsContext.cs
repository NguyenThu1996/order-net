using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OPS.Entity.Schemas;
using System.Linq;

namespace OPS.Entity
{
    public class OpsContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, 
        ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public OpsContext(DbContextOptions<OpsContext> options) : base(options) { }
        public DbSet<MstCareer> MstCareers { get; set; }
        public DbSet<MstMedia> MstMedias { get; set; }
        public DbSet<MstEvent> MstEvents { get; set; }
        public DbSet<MstArtist> MstArtists { get; set; }
        public DbSet<MstOrganization> MstOrganizations { get; set; }
        public DbSet<MstPayment> MstPayments { get; set; }
        public DbSet<MstProduct> MstProducts { get; set; }
        public DbSet<MstTax> MstTaxes { get; set; }
        public DbSet<MstCompanyType> MstCompanyTypes { get; set; }
        public DbSet<MstSalesman> MstSalesmen { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<EventSalesAssigment> EventSalesAssigments { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<OrderReport> OrderReports { get; set; }
        public DbSet<OrderReportDetail> OrderReportDetails { get; set; }
        public DbSet<OrderReportCash> OrderReportCash { get; set; }
        public DbSet<Map> Maps { get; set; }
        public DbSet<SalesmanTarget> SalesmanTargets { get; set; }
        public DbSet<PlanMedia> PlanMedias { get; set; }
        public DbSet<EventRequest> EventRequests { get; set; }
        public DbSet<BusinessHope> BusinessHopes { get; set; }
        public DbSet<CustomerInfo> CustomerInfos { get; set; }
        public DbSet<Ranking> Rankings { get; set; }
        public DbSet<MstPrefecture> MstPrefectures { get; set; }
        public DbSet<MstAddress> MstAddresses { get; set; }
        public DbSet<PurchaseStatistics> PurchaseStatistics { get; set; }
        public DbSet<PurchaseStatisticsDetail> PurchaseStatisticsDetails { get; set; }
        public DbSet<PurchaseStatisticsDate> PurchaseStatisticsDates { get; set; }
        public DbSet<PurchaseStatisticsContract> PurchaseStatisticsContracts { get; set; }
        public DbSet<MstDepartment> MstDepartments { get; set; }
        public DbSet<MstProductAgreementType> MstProductAgreementTypes { get; set; }
        public DbSet<ArtistDepartment> ArtistDepartments { get; set; }
        public DbSet<EventMedia> EventMedias { get; set; }
        public DbSet<MstTechnique> MstTechniques { get; set; }
        public DbSet<MstTelephone> MstTelephones { get; set; }
        public DbSet<ProductTechnique> ProductTechniques { get; set; }
        public DbSet<SurveyArtist> SurveyArtists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // customize here

            modelBuilder.Entity<ApplicationUserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });
                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<EventSalesAssigment>(assignment =>
            {
                assignment.HasKey(a => new { a.EventCd, a.SalesmanCd });
                assignment.HasOne(a => a.Event)
                    .WithMany(e => e.EventSalesAssigments)
                    .HasForeignKey(a => a.EventCd)
                    .IsRequired();
                assignment.HasOne(a => a.Salesman)
                    .WithMany(s => s.EventSalesAssigments)
                    .HasForeignKey(a => a.SalesmanCd)
                    .IsRequired();
            });

            modelBuilder.Entity<ArtistDepartment>(at =>
            {
                at.HasKey(a => new { a.DepartmentCd, a.ArtistCd });
                at.HasOne(a => a.Department)
                    .WithMany(e => e.ArtistDepartments)
                    .HasForeignKey(a => a.DepartmentCd)
                    .IsRequired();
                at.HasOne(a => a.Artist)
                    .WithMany(s => s.ArtistDepartments)
                    .HasForeignKey(a => a.ArtistCd)
                    .IsRequired();
            });

            modelBuilder.Entity<EventMedia>(eventMedia =>
            {
                eventMedia.HasKey(em => new { em.EventCd, em.MediaCd });
                eventMedia.HasOne(em => em.Event)
                    .WithMany(e => e.EventMedias)
                    .HasForeignKey(em => em.EventCd)
                    .IsRequired();
                eventMedia.HasOne(em => em.Media)
                    .WithMany(m => m.EventMedias)
                    .HasForeignKey(em => em.MediaCd)
                    .IsRequired();
            });

            modelBuilder.Entity<ProductTechnique>(producTech =>
            {
                producTech.HasKey(pt => new { pt.ProductCd, pt.TechniqueCd });
                producTech.HasOne(pt => pt.Product)
                    .WithMany(p => p.ProductTechniques)
                    .HasForeignKey(pt => pt.ProductCd)
                    .IsRequired();
                producTech.HasOne(pt => pt.Technique)
                    .WithMany(t => t.ProductTechniques)
                    .HasForeignKey(pt => pt.TechniqueCd)
                    .IsRequired();
            });

            modelBuilder.Entity<SurveyArtist>(surveyArtist =>
            {
                surveyArtist.HasKey(pt => new { pt.SurveyCd, pt.ArtistCd });
                surveyArtist.HasOne(sa => sa.Survey)
                    .WithMany(s => s.SurveyArtists)
                    .HasForeignKey(sa => sa.SurveyCd)
                    .IsRequired();
                surveyArtist.HasOne(sa => sa.Artist)
                    .WithMany(a => a.SurveyArtists)
                    .HasForeignKey(sa => sa.ArtistCd)
                    .IsRequired();
            });

            foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                if (property.GetColumnType() == null)
                    property.SetColumnType("decimal(16,2)");
            }

            modelBuilder.Entity<Contract>()
                .HasIndex(a => a.Id)
                .IsUnique();

            modelBuilder.Entity<MstProduct>()
                .HasIndex(p => new { p.ArtistCd, p.Code, p.ItemCategory })
                .IsUnique();

            modelBuilder.Entity<MstArtist>()
                .HasIndex(a => a.Code)
                .IsUnique();

            modelBuilder.Entity<MstTechnique>()
                .HasIndex(t => t.Code)
                .IsUnique();

            modelBuilder.Entity<MstEvent>()
                .HasIndex(e => e.Code)
                .IsUnique();

            modelBuilder.Entity<MstSalesman>()
                .HasIndex(s => s.Code)
                .IsUnique();

            modelBuilder.Entity<MstMedia>()
                .HasIndex(m => new { m.Code, m.BranchCode })
                .IsUnique();

            modelBuilder.Entity<MstPayment>()
                .HasIndex(p => new { p.Code, p.Category })
                .IsUnique();

            modelBuilder.Entity<MstOrganization>()
                .HasIndex(o => o.Code)
                .IsUnique();

            modelBuilder.Entity<MstCompanyType>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<MstCareer>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<MstDepartment>()
                .HasIndex(d => d.Code)
                .IsUnique();

            modelBuilder.Entity<MstProductAgreementType>()
                .HasIndex(t => t.Code)
                .IsUnique();

            modelBuilder.Entity<MstPrefecture>()
                .HasNoKey();

            modelBuilder.Entity<MstAddress>()
                .HasNoKey();

            modelBuilder.Entity<MstTelephone>()
                .HasNoKey();
        }
    }
}
