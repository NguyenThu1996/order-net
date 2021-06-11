using System.ComponentModel;

namespace OPS.Utility
{
    public static class Constants
    {
        public static readonly string ExactDateFormat = "yyyy/MM/dd";

        public static readonly string ExactDateTimeFormat = "yyyy/MM/dd HH:mm:ss";

        public static readonly string ExactDateFormatJP = "yyyy年MM月dd日";

        public static readonly string ExactMonthFormatJP = "yyyy年MM月";

        public static readonly string ExactMonthDayFormat = "MM/dd";

        public static readonly string ExactMonthDayFormatJP = "MM月dd日";

        public static readonly string CurrencyFormat = "#,##0";

        public static readonly string EventCookie = "eventinfo";

        public static readonly string ExactDateJapanEraFormat = "ggyy年MM月dd日";

        public static readonly string SurveyUserName = "SURVEY";

        public static readonly string FakePass = "cGUXtTYg9C";

        public static readonly string ImportedEventPwd = "M9fnpc6mTf";

        public static readonly string NonTechniqueCode = "";

        public static readonly string CustomerFlagMediaCodes = "AOVW";

        public static readonly string PaymentPlace_JN = "ＪＮ";

        public static readonly string PaymentPlace_JN_Halfsize = "JN";

        public static readonly string Department_Const = "共通";

        public enum AgeRange
        {
            [Description("10代")]
            U10     = 1,
            [Description("20～23才")]
            U20To23 = 2,
            [Description("24～29才")]
            U24To29 = 3,
            [Description("30代")]
            U30     = 4,
            [Description("40代")]
            U40     = 5,
            [Description("50代")]
            U50     = 6,
            [Description("60代")]
            U60     = 7,
            [Description("70代以上")]
            U70     = 8
        }

        public enum VisitTime
        {
            [Description("初めて")]
            first_time        = 1,
            [Description("2回目")]
            second_times      = 2,
            [Description("それ以上")]
            more_than_2_times = 3
        }

        public enum Gender
        {
            [Description("男性")]
            Male = 1,
            [Description("女性")]
            Female = 2,
            [Description("法人")]
            Corporation = 3,
        }

        public enum GenderOfSurvey
        {
            [Description("男性")]
            Male = 1,
            [Description("女性")]
            Female = 0,
        }

        public enum MstMediaFlag
        {
            [Description("新規")]
            New = 0,
            [Description("営業努力")]
            Effort = 1
        }

        public enum IsMarried
        {
            [Description("既婚")]
            Yes = 1,
            [Description("未婚")]
            No  = 0
        }

        public enum LivingStatus
        {
            [Description("単身")]
            Alone      = 1,
            [Description("家族同居")]
            WithFamily = 0
        }

        public enum IsComeToBuy
        {
            [Description("はい")]
            Yes = 1,
            [Description("いいえ")]
            No  = 0
        }

        public enum SalesDepartment
        {
            [Description("営業部")]
            Sales  = 1,
            [Description("e・ジュネックス事業部")]
            EJunex = 2,
        }

        public enum DepartmentAndCategoryNameMap
        {
            [Description("10 ｴﾝﾀｰﾃｲﾒﾝﾄ作家")]
            Sales = 1,
            [Description("20 ｱｰﾙｼﾞｭﾈｽ作家")]
            Genex = 2,
        }

        public enum IsDeleted
        {
            Yes = 1,
            No  = 0
        }

        public enum Inputter
        {
            [Description("お客様入力")]
            Self     = 1,
            [Description("代理入力")]
            Agency   = 2,
            [Description("営業担当")]
            Salesman = 3,
        }

        public enum DownPaymentMethod
        {
            [Description("現金")]
            InCash   = 1,
            [Description("銀行振込")]
            Transfer = 2,
        }

        public enum LeftPaymentMethod
        {
            //[Description("信販")]
            //Credit = 1,
            //[Description("カード")]
            //Card   = 2,
            //[Description("現金")]
            //InCash = 3,
            //[Description("銀行振込")]
            //BankTranfer = 4,
            [Description("現金")]
            InCash = 1,
            [Description("銀行振込")]
            BankTranfer = 2,
            [Description("カード")]
            Card = 3,
            [Description("アートクレジット")]
            Credit = 4,
        }

        public enum IsConfirmed
        {
            [Description("無")]
            No  = 0,
            [Description("有")]
            Yes = 1,
        }

        public enum HomePhoneOwner
        {
            [Description("本人")]
            Self   = 1,
            [Description("その他")]
            Others = 2,
        }

        public enum PhoneBranch
        {
            [Description("自宅")]
            Home = 1,
            [Description("呼び出し")]
            Call = 2,
        }

        public enum CompanyTypeDisplay
        {
            [Description("前")]
            Before = 1,
            [Description("後")]
            After  = 2,
        }

        public enum Club
        {
            [Description("済")]
            Already = 1,
            [Description("国際")]
            International = 2,
            [Description("ID")]
            Id = 3,
            [Description("非")]
            Nope = 4
        }

        public enum NumberOfVisit
        {
            [Description("初")]
            First = 1,
            [Description("2回目")]
            Second = 2,
            [Description("複数回")]
            More = 3
        }

        public enum Remarks
        {
            [Description("特Ａ")]
            SpecialA = 1,
            [Description("特Ｂ")]
            SpecialB = 2,
            [Description("値引き")]
            Discount = 3,
            [Description("金券（）万")]
            CashVoucher = 4,
            [Description("特価")]
            SpecialPrice = 5,
            [Description("予約特価")]
            ReservedSpecialPrice = 6,
            [Description("後送り")]
            PostDelivery = 7,
            [Description("特別仕入")]
            SpecialPurchase = 8,
        }

        public enum DeliveryTime
        {
            [Description("AM")]
            AM = 1,
            [Description("PM")]
            PM = 2,
        }

        public enum DeliveryPlace
        {
            [Description("自宅")]
            Home = 1,
            [Description("勤務先")]
            Workplace = 2,
            [Description("実家")]
            ParentHome = 3,
            [Description("その他")]
            Others = 4,
        }

        public enum EraName
        {
            [Description("明")]
            Meiji  = 1,
            [Description("大")]
            Taisho = 2,
            [Description("昭")]
            Showa  = 3,
            [Description("平")]
            Heisei = 4,
            [Description("令 和")]
            Reiwa  = 5,
        }

        // Using for Contract.IdentifyDocument
        public enum IdentifyDocument
        {
            [Description("免許証")]
            License       = 1,
            [Description("保険証")]
            InsuranceCard = 2,
            [Description("旅券")]
            Passport      = 3,
            [Description("その他")]
            Other         = 4,
        }

        // Using for Contract.ClubRegistrationStatus
        public enum ClubJoin
        {
            [Description("入会する")]
            Yes = 1,
            [Description("入会しない")]
            No = 2,
            [Description("入会済")]
            Joined = 3,
        }

        public enum MemberCard
        {
            [Description("ラッセン")]
            Lassen = 1,
            [Description("天野")]
            Amano = 2,
            [Description("ディズニー")]
            Disney = 3,
            //[Description("ＥＪメンバーシップ（入会区分がＥＪ固定)")]
            [Description("ＥＪメンバーシップ")]
            EJ_Membership = 4
        }

        //Using for Contracts.ClubJoinStatus Contracts.MemberJoinStatus
        public enum StatusJoinClubMember
        {
            [Description("入会済")]
            Joined = 1,
            [Description("国際カード入会")]
            JoinInternationCard = 2,
            [Description("IDカード入会")]
            JoinIDCard = 3,
            [Description("非入会")]
            None = 4
        }

        public enum LeftPaymentPlace
        {
            [Description("その他")]
            Other = 10,
        }

        public enum LeftPaymentPlaceCategory
        {
            InCash = 81,
            BankTranfer = 21,
            Card = 12,
            Credit = 11,
        }

        public enum DateOfWeekJP
        {
            [Description("日")]
            Sunday,
            [Description("月")]
            Monday,
            [Description("火")]
            Tuesday,
            [Description("水")]
            Wednesday,
            [Description("木")]
            Thursday,
            [Description("金")]
            Friday,
            [Description("土")]
            Saturday,
        }

        public enum PurchaseType
        {
            A = 1,
            B = 2
        }

        public enum AgeRangeMap
        {
            [Description("20代")]
            U20 = 20,
            [Description("30代")]
            U30 = 30,
            [Description("40代")]
            U40 = 40,
            [Description("50代")]
            U50 = 50,
            [Description("60代")]
            U60 = 60,
            [Description("70代")]
            U70 = 70,
            [Description("80代以上")]
            U80 = 80
        }

        public enum IsHaveProductAgreement
        {
            [Description("有り")]
            Yes = 1,
            [Description("無し")]
            No = 0,
        }

        public enum MediaFlag
        {
            [Description("顧客")]
            Customer = 0,
            [Description("新規")]
            New = 1,
        }

        public enum LayoutCheck
        {
            [Description("未")]
            NotDone = 1,
            [Description("済")]
            Done = 2,
        }

        public enum MediaEffort
        {
            [Description("O")]
            CallO = 1,
            [Description("Q")]
            CallQ = 2,
            [Description("R")]
            CallR = 3,
        }
        public enum MediaSale
        {
            [Description("A")]
            SaleDM = 1,
        }

        public enum CsvExportMode
        {
            Active = 1,
            Deleted = 2,
        }

        public enum UserType
        {
            Sale = 1,
            Survey = 2,
        }

        public enum Organization
        {
            [Description("営業部")]
            SalesDepartment = 1,
            [Description("AJ東京")]
            AJTokyo = 2,
            [Description("AJ名古屋")]
            AJNagoya = 3,
            [Description("AJ大阪")]
            AJOsaka = 4,
            [Description("AJ福岡")]
            AJFukuoka = 5,
        }

        public enum PaymentType
        {
            [Description("AV")]
            AV = 1,
            [Description("AJ秋葉原")]
            AJAkihabara = 2,
            [Description("AJ名古屋")]
            AJNagoya = 3,
            [Description("AJ日本橋")]
            AJNihonbashi = 4,
            [Description("AJ福岡")]
            AJFukuoka = 5,
            [Description("EJ")]
            EJ = 6,
        }

        public enum PaymentNameCase
        {
            [Description("オリエントコーポレーション")]
            OrientCorporation_Fullsize = 1,
            [Description("ｵﾘｴﾝﾄｺｰﾎﾟﾚｰｼｮﾝ")]
            OrientCorporation_Halfsize = 2,
            [Description("オリコ")]
            Orico_Fullsize = 3,
            [Description("ｵﾘｺ")]
            Orico_Halfsize = 4 ,
        }

        public enum ProductRemarkA
        {
            [Description("後送り")]
            PostDelivery = 1,
            [Description("特別仕入")]
            SpecialPurchase = 2,
        }

        public enum ProductRemarkB
        {
            [Description("特Ａ")]
            SpecialA = 1,
            [Description("特Ｂ")]
            SpecialB = 2,
            [Description("特価")]
            SpecialPrice = 3,
            [Description("予約特価")]
            ReservedSpecialPrice = 4,
        }

        public enum ProductRemarkC
        {
            [Description("値引き")]
            Discount = 1,
        }

        public enum ProductRemarkD
        {
            [Description("金券")]
            CashVoucher = 1,
        }

        public enum CashVoucherValue
        {
            [Description("1万円")]
            TenThousandYen = 1,
            [Description("3万円")]
            ThirtyThousandYen = 2,
            [Description("10%")]
            TenPercent = 3,
        }
    }
}
