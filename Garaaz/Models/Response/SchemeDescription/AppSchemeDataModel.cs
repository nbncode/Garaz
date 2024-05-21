using System;
using System.Collections.Generic;
using Garaaz.Models.Response.SchemeDescription;

namespace Garaaz.Models
{
    public class AppSchemeDataModel
    {
        public AppSchemeDataModel()
        {
            // Initialize lists
            GiftDatas = new List<GiftData>();
            AssuredGiftDatas = new List<AssuredGiftData>();
            CashBacks = new List<SchemeCashBack>();
            QualifyCriterias = new List<SchemeQualifyCriteria>();
            TargetWorkshops = new List<SchemeTargetWorkshop>();
            //FocusParts = new List<SchemeFocusPart>();
        }

        public string SchemeName { get; set; }
        public string BannerImage { get; set; }   
        public bool AreBothCbAgApplicable { get; set; }
        public bool AreBothAgLdApplicable { get; set; }
        public string SchemeStartDate { get; set; }
        public string SchemeEndDate { get; set; }

        public List<GiftData> GiftDatas { get; set; }
        public List<AssuredGiftData> AssuredGiftDatas { get; set; }

        public List<SchemeCashBack> CashBacks { get; set; }
        public List<SchemeQualifyCriteria> QualifyCriterias { get; set; }
        public List<SchemeTargetWorkshop> TargetWorkshops { get; set; }
       // public List<SchemeFocusPart> FocusParts { get; set; }

        public string PartCategory { get; set; }
        public string PartGroup { get; set; }
        public string FPercentage { get; set; }
        public string MPercentage { get; set; }
        public string SPercentage { get; set; }
        public string PartCreation { get; set; }
        public string DateRangeForFms { get; set; }
        public string BranchCode { get; set; }
        public string SalesPerson { get; set; }
        public string CustomerType { get; set; }
        public string SchemeType { get; set; }
        public string SchemeDateRange { get; set; }
        public string DistributorName { get; set; }
        public bool AllowFocusPartsDownload { get; set; }


    }    

    public class GiftData
    {
        public string CategoryName { get; set; }
        public string ColorCode { get; set; }
        public List<SchemeCategoryGift> data { get; set; }
    }

    public class AssuredGiftData
    {
        public string CategoryName { get; set; }
        public string ColorCode { get; set; }
        public List<SchemeAssuredGift> data { get; set; }
    }

    public class SchemeCategoryGift
    {
        public int GiftId { get; set; }
        public string GiftName { get; set; }
        public string GiftImage { get; set; }
        public int Qty { get; set; }
        public bool CouponAllocated { get; set; }
    }

    public class SchemeAssuredGift
    {
        public int AssuredGiftId { get; set; }
        public string SalesTarget { get; set; }
        public string Point { get; set; }
        public string Reward { get; set; }
    }

    public class SchemeCashBack
    {
        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
        public List<CashbackBenefit> data { get; set; }

        public SchemeCashBack()
        {
            data = new List<CashbackBenefit>();
        }
    }

    public class SchemeQualifyCriteria
    {
        public decimal? AmountUpto { get; set; }
        public string Type { get; set; }
        public string PartsOrAccessories { get; set; }
        public int? NumberOfCoupons { get; set; }
        public decimal? AdditionalCouponAmount { get; set; }
        public int? AdditionalNumberOfCoupons { get; set; }
    }

    public class SchemeTargetWorkshop
    {
        public string Workshop { get; set; }
        public string Target { get; set; }
        public string TargetAchieved { get; set; }
        public string TargetAchievedPercentage { get; set; }
    }
    
    public class SchemeFocusPart: AddToCartOptions
    {
        public string PartGroup { get; set; }
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public string MRP { get; set; }
    }
    public class AddToCartOptions
    {
        // used  parameters for add to cart in app
        public decimal Price { get; set; }
        public bool IsOriparts { get; set; }
        public int ProductId { get; set; }
        public int? CartQty { get; set; }
        public string CartAvailabilityType { get; set; }
        public int? CartOutletId { get; set; }
        public int Stock { get; set; }
    }
}