using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Entity.Schemas
{
    [Table("Contract")]
    public class Contract : DbTable
    {
        [MaxLength(8)]
        [Required]
        public string Id { get; set; }

        [MaxLength(8)]
        public string OldId { get; set; }

        public int EventCd { get; set; }

        public int? FutureEventCd { get; set; }

        public int SalesDepartment { get; set; }

        public int? SalesmanSCd { get; set; }

        public int SalesmanCCd { get; set; }

        public int SalesmanACd { get; set; }

        public int OrganizationCd { get; set; }

        public int Inputter { get; set; }

        public bool? IsConfNotice { get; set; }

        public DateTime? ConfNoticeTime { get; set; }

        public bool? IsConfCoolingOff { get; set; }

        public DateTime? ConfCoolingOffTime { get; set; }

        public bool? IsConfPersonalInfo { get; set; }

        public DateTime? ConfPersonalInfoTime { get; set; }

        public DateTime? OrderDate { get; set; }

        public int? DepartmentCd1 { get; set; }

        public int? ArtistCd1 { get; set; }

        [MaxLength(120)]
        public string ArtistName1 { get; set; }

        public int? ProductCd1 { get; set; }

        [MaxLength(150)]
        public string ProductName1 { get; set; }

        public int? TechniqueCd1 { get; set; }

        [MaxLength(120)]
        public string TechniqueName1 { get; set; }

        public int? ProductQuantity1 { get; set; }

        public decimal? ProductPrice1 { get; set; }

        public DateTime? DeliveryDate1 { get; set; }

        public int? DeliveryTime1 { get; set; }

        public int? DeliveryPlace1 { get; set; }

        public decimal? ProductUnitTaxPrice1 { get; set; }

        public decimal? ProductTaxPrice1 { get; set; }

        public decimal? ProductDiscount1 { get; set; }

        public decimal? ProductSalesPrice1 { get; set; }

        [MaxLength(10)]
        public string ProductRemarks1 { get; set; }

        public int? CashVoucherValue1 { get; set; }

        public int? DepartmentCd2 { get; set; }

        public int? ArtistCd2 { get; set; }

        [MaxLength(120)]
        public string ArtistName2 { get; set; }

        public int? ProductCd2 { get; set; }

        [MaxLength(150)]
        public string ProductName2 { get; set; }

        public int? TechniqueCd2 { get; set; }

        [MaxLength(120)]
        public string TechniqueName2 { get; set; }

        public int? ProductQuantity2 { get; set; }

        public decimal? ProductPrice2 { get; set; }

        public DateTime? DeliveryDate2 { get; set; }

        public int? DeliveryTime2 { get; set; }

        public int? DeliveryPlace2 { get; set; }

        public decimal? ProductUnitTaxPrice2 { get; set; }

        public decimal? ProductTaxPrice2 { get; set; }

        public decimal? ProductDiscount2 { get; set; }

        public decimal? ProductSalesPrice2 { get; set; }

        [MaxLength(10)]
        public string ProductRemarks2 { get; set; }

        public int? CashVoucherValue2 { get; set; }

        public int? DepartmentCd3 { get; set; }

        public int? ArtistCd3 { get; set; }

        [MaxLength(120)]
        public string ArtistName3 { get; set; }

        public int? ProductCd3 { get; set; }

        [MaxLength(150)]
        public string ProductName3 { get; set; }

        public int? TechniqueCd3 { get; set; }

        [MaxLength(120)]
        public string TechniqueName3 { get; set; }

        public int? ProductQuantity3 { get; set; }

        public decimal? ProductPrice3 { get; set; }

        public DateTime? DeliveryDate3 { get; set; }

        public int? DeliveryTime3 { get; set; }

        public int? DeliveryPlace3 { get; set; }

        public decimal? ProductUnitTaxPrice3 { get; set; }

        public decimal? ProductTaxPrice3 { get; set; }

        public decimal? ProductDiscount3 { get; set; }

        public decimal? ProductSalesPrice3 { get; set; }

        [MaxLength(10)]
        public string ProductRemarks3 { get; set; }

        public int? CashVoucherValue3 { get; set; }

        public int? DepartmentCd4 { get; set; }

        public int? ArtistCd4 { get; set; }

        [MaxLength(120)]
        public string ArtistName4 { get; set; }

        public int? ProductCd4 { get; set; }

        [MaxLength(150)]
        public string ProductName4 { get; set; }

        public int? TechniqueCd4 { get; set; }

        [MaxLength(120)]
        public string TechniqueName4 { get; set; }

        public int? ProductQuantity4 { get; set; }

        public decimal? ProductPrice4 { get; set; }

        public DateTime? DeliveryDate4 { get; set; }

        public int? DeliveryTime4 { get; set; }

        public int? DeliveryPlace4 { get; set; }

        public decimal? ProductUnitTaxPrice4 { get; set; }

        public decimal? ProductTaxPrice4 { get; set; }

        public decimal? ProductDiscount4 { get; set; }

        public decimal? ProductSalesPrice4 { get; set; }

        [MaxLength(10)]
        public string ProductRemarks4 { get; set; }

        public int? CashVoucherValue4 { get; set; }

        public int? DepartmentCd5 { get; set; }

        public int? ArtistCd5 { get; set; }

        [MaxLength(120)]
        public string ArtistName5 { get; set; }

        public int? ProductCd5 { get; set; }

        [MaxLength(150)]
        public string ProductName5 { get; set; }

        public int? TechniqueCd5 { get; set; }

        [MaxLength(120)]
        public string TechniqueName5 { get; set; }

        public int? ProductQuantity5 { get; set; }

        public decimal? ProductPrice5 { get; set; }

        public DateTime? DeliveryDate5 { get; set; }

        public int? DeliveryTime5 { get; set; }

        public int? DeliveryPlace5 { get; set; }

        public decimal? ProductUnitTaxPrice5 { get; set; }

        public decimal? ProductTaxPrice5 { get; set; }

        public decimal? ProductDiscount5 { get; set; }

        public decimal? ProductSalesPrice5 { get; set; }

        [MaxLength(10)]
        public string ProductRemarks5 { get; set; }

        public int? CashVoucherValue5 { get; set; }

        public int? DepartmentCd6 { get; set; }

        public int? ArtistCd6 { get; set; }

        [MaxLength(120)]
        public string ArtistName6 { get; set; }

        public int? ProductCd6 { get; set; }

        [MaxLength(150)]
        public string ProductName6 { get; set; }

        public int? TechniqueCd6 { get; set; }

        [MaxLength(120)]
        public string TechniqueName6 { get; set; }

        public int? ProductQuantity6 { get; set; }

        public decimal? ProductPrice6 { get; set; }

        public DateTime? DeliveryDate6 { get; set; }

        public int? DeliveryTime6 { get; set; }

        public int? DeliveryPlace6 { get; set; }

        public decimal? ProductUnitTaxPrice6 { get; set; }

        public decimal? ProductTaxPrice6 { get; set; }

        public decimal? ProductDiscount6 { get; set; }

        public decimal? ProductSalesPrice6 { get; set; }

        [MaxLength(10)]
        public string ProductRemarks6 { get; set; }

        public int? CashVoucherValue6 { get; set; }

        public decimal? SalesPrice { get; set; }

        public decimal? Discount { get; set; }

        public decimal? TaxPrice { get; set; }

        public decimal? TotalPrice { get; set; }

        public decimal? DownPayment { get; set; }

        public decimal? LeftPayment { get; set; }

        public int? DownPaymentMethod { get; set; }

        public DateTime? DownPaymentDate { get; set; }

        [MaxLength(30)]
        public string ReceiptNo { get; set; }

        public int? LeftPaymentMethod { get; set; }

        public DateTime? LeftPaymentDate { get; set; }

        public int? LeftPaymentPlace { get; set; }

        [MaxLength(60)]
        public string LeftPaymentOtherPlace { get; set; }

        [MaxLength(60)]
        public string VerifyNumber { get; set; }

        public DateTime? ContractDate { get; set; }

        public bool? IsHaveProductAgreement { get; set; }

        public int? ProductAgreementTypeCd { get; set; }

        [MaxLength(90)]
        public string FamilyName { get; set; }

        [MaxLength(90)]
        public string FirstName { get; set; }

        [MaxLength(120)]
        public string FamilyNameFuri { get; set; }

        [MaxLength(120)]
        public string FirstNameFuri { get; set; }

        public int? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? Age { get; set; }

        [MaxLength(8)]
        public string Zipcode { get; set; }

        [MaxLength(42)]
        public string Province { get; set; }

        [MaxLength(42)]
        public string Address { get; set; }

        [MaxLength(80)]
        public string Building { get; set; }

        [MaxLength(42)]
        public string ProvinceFuri { get; set; }

        [MaxLength(100)]
        public string AddressFuri { get; set; }

        [MaxLength(100)]
        public string BuildingFuri { get; set; }

        [MaxLength(16)]
        public string HomePhone { get; set; }

        public int? HomePhoneOwner { get; set; }//remove

        [MaxLength(150)]
        public string HomePhoneOtherOwner { get; set; }//remove

        public int? PhoneBranch { get; set; }

        [MaxLength(16)]
        public string Mobiphone { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        public int? CompanyTypeCd { get; set; }//remove

        public int? CompanyTypeDisplay { get; set; }//remove

        [MaxLength(90)]
        public string CompanyName { get; set; }

        [MaxLength(120)]
        public string CompanyNameFuri { get; set; }

        [MaxLength(16)]
        public string CompanyPhone { get; set; }

        [MaxLength(16)]
        public string CompanyLocalPhone { get; set; }

        [MaxLength(8)]
        public string CompanyZipCode { get; set; }

        [MaxLength(42)]
        public string CompanyProvince { get; set; }

        [MaxLength(42)]
        public string CompanyAddress { get; set; }

        [MaxLength(80)]
        public string CompanyBuilding { get; set; }

        [MaxLength(42)]
        public string CompanyProvinceFuri { get; set; }

        [MaxLength(100)]
        public string CompanyAddressFuri { get; set; }

        [MaxLength(100)]
        public string CompanyBuildingFuri { get; set; }

        public int? MediaCd { get; set; }

        public int? VisitTime { get; set; }

        public int? JoinAV { get; set; }//remove

        public int? JoinEJ { get; set; }//remove

        public int? ClubJoinStatus { get; set; }

        public int? MemberJoinStatus { get; set; }

        [MaxLength(50)]
        public string ClubJoinedText { get; set; }

        [MaxLength(50)]
        public string MemberJoinedText { get; set; }

        [MaxLength(100)]
        public string Approve { get; set; }

        [MaxLength(150)]
        public string Remark { get; set; }

        public int? ClubRegistrationStatus { get; set; }

        [MaxLength(15)]
        public string MemberId { get; set; }

        public bool? IsConfirmMember { get; set; }

        public DateTime? ConfirmMemberTime { get; set; }

        public int? MemberCardAV { get; set; }//remove

        public int? MemberCardEJ { get; set; }//remove

        public int? MemberCard { get; set; }

        public int? IdentifyDocument { get; set; }

        [MaxLength(60)]
        public string OtherIdentifyDoc { get; set; }

        [DefaultValue(false)]
        public bool IsCompleted { get; set; }

        [DefaultValue(false)]
        public bool IsEdited { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [DefaultValue(false)]
        public bool FlagCsv { get; set; }

        [DefaultValue(false)]
        public bool FlagCsvDeleted { get; set; }

        [DefaultValue(false)]
        public bool IsPrinted { get; set; }

        [ForeignKey("EventCd")]
        public virtual MstEvent Event { get; set; }

        [ForeignKey("FutureEventCd")]
        public virtual MstEvent FutureEvent { get; set; }

        [ForeignKey("SalesmanSCd")]
        public virtual MstSalesman SalesmanS { get; set; }

        [ForeignKey("SalesmanCCd")]
        public virtual MstSalesman SalesmanC { get; set; }

        [ForeignKey("SalesmanACd")]
        public virtual MstSalesman SalesmanA { get; set; }

        [ForeignKey("OrganizationCd")]
        public virtual MstOrganization Organization { get; set; }

        [ForeignKey("DepartmentCd1")]
        public virtual MstDepartment Department1 { get; set; }

        [ForeignKey("TechniqueCd1")]
        public virtual MstTechnique Technique1 { get; set; }

        [ForeignKey("ArtistCd1")]
        public virtual MstArtist Artist1 { get; set; }

        [ForeignKey("ProductCd1")]
        public virtual MstProduct Product1 { get; set; }

        [ForeignKey("DepartmentCd2")]
        public virtual MstDepartment Department2 { get; set; }

        [ForeignKey("TechniqueCd2")]
        public virtual MstTechnique Technique2 { get; set; }

        [ForeignKey("ArtistCd2")]
        public virtual MstArtist Artist2 { get; set; }

        [ForeignKey("ProductCd2")]
        public virtual MstProduct Product2 { get; set; }

        [ForeignKey("DepartmentCd3")]
        public virtual MstDepartment Department3 { get; set; }

        [ForeignKey("TechniqueCd3")]
        public virtual MstTechnique Technique3 { get; set; }

        [ForeignKey("ArtistCd3")]
        public virtual MstArtist Artist3 { get; set; }

        [ForeignKey("ProductCd3")]
        public virtual MstProduct Product3 { get; set; }

        [ForeignKey("DepartmentCd4")]
        public virtual MstDepartment Department4 { get; set; }

        [ForeignKey("TechniqueCd4")]
        public virtual MstTechnique Technique4 { get; set; }

        [ForeignKey("ArtistCd4")]
        public virtual MstArtist Artist4 { get; set; }

        [ForeignKey("ProductCd4")]
        public virtual MstProduct Product4 { get; set; }

        [ForeignKey("DepartmentCd5")]
        public virtual MstDepartment Department5 { get; set; }

        [ForeignKey("TechniqueCd5")]
        public virtual MstTechnique Technique5 { get; set; }

        [ForeignKey("ArtistCd5")]
        public virtual MstArtist Artist5 { get; set; }

        [ForeignKey("ProductCd5")]
        public virtual MstProduct Product5 { get; set; }

        [ForeignKey("DepartmentCd6")]
        public virtual MstDepartment Department6 { get; set; }

        [ForeignKey("TechniqueCd6")]
        public virtual MstTechnique Technique6 { get; set; }

        [ForeignKey("ArtistCd6")]
        public virtual MstArtist Artist6 { get; set; }

        [ForeignKey("ProductCd6")]
        public virtual MstProduct Product6 { get; set; }

        [ForeignKey("LeftPaymentPlace")]
        public virtual MstPayment Payment { get; set; }

        [ForeignKey("CompanyTypeCd")]
        public virtual MstCompanyType CompanyType { get; set; }

        [ForeignKey("MediaCd")]
        public virtual MstMedia Media { get; set; }

        [ForeignKey("ProductAgreementTypeCd")]
        public virtual MstProductAgreementType ProductAgreementType { get; set; }
    }
}
