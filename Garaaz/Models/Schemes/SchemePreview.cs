using System.Collections.Generic;
using System.Threading.Tasks;

namespace Garaaz.Models.Schemes
{
    /// <summary>
    /// Represents data that is used in scheme's preview page.
    /// </summary>
    public class SchemePreview
    {
        #region Basic Information

        public string SchemeName { get; set; }
        public string SchemeType { get; set; }
        public string SubSchemeType { get; set; }
        public string RewardOptions { get; set; }
        public string DistributorName { get; set; }
        public string SchemeDateRange { get; set; }
        public string PaybackPeriod { get; set; }
        public string PaybackType { get; set; }
        public string ThumbnailImgUrl { get; set; }
        public string BannerImgUrl { get; set; }
        public bool IsActive { get; set; }

        #endregion

        #region Parts Information

        public readonly string FocusPartsGroup = "Focus Parts Group";
        public readonly string FmsPartsGroup = "FMS Parts Group";

        public string PartCategory { get; set; }
        public string PartGroup { get; set; }
        public string DateRangeForFms { get; set; }
        public string FPercentage { get; set; }
        public string MPercentage { get; set; }
        public string SPercentage { get; set; }
        public string PartCreation { get; set; }
        public List<SchemePreviewFocusPart> FocusParts { get; set; }

        #endregion

        #region Customer Segment

        public string BranchCode { get; set; }
        public string SalesPerson { get; set; }
        public string CustomerType { get; set; }
        public List<TargetGrowth> TargetGrowths { get; set; }
        public List<ResponseSchemeCategoryModel> Categories { get; set; }

        #endregion

        #region Target Information

        public string PrevYearDateRange { get; set; }
        public string PrevMonthDateRange { get; set; }
        public List<ResponseTargetWorkshopModel> TargetWorkshops { get; set; }

        #endregion

        #region Reward Information

        // To show or hide div
        public bool ShowCashback { get; set; }
        public bool ShowAssuredGift { get; set; }
        public bool ShowLuckyDraw { get; set; }
        public bool ShowCouponAllocation { get; set; }

        public List<TargetOverview> TargetOverviews { get; set; }

        // Tab: Cashback

        /// <summary>
        /// Gets or sets Cashback range column text
        /// </summary>
        public List<string> CbrColText { get; set; }
        public string CashbackCriteria { get; set; }
        public bool AreBothCbAgApplicable { get; set; }
        public List<ResponseCashBackModel> Cashbacks { get; set; }

        // Tab: Assured Gifts
        public bool AreBothAgLdApplicable { get; set; }
        public List<ResponseAssuredGiftModel> AssuredGifts { get; set; }

        // Tab: Lucky Draw
        public bool CanTakeMoreThanOneGift { get; set; }
        public string MaxGiftsAllowed { get; set; }
        public List<ResponseGiftManagementModel> LuckyDraws { get; set; }

        // Tab: Coupon Allocation
        public List<SchemePreviewCouponAllocation> CouponAllocations { get; set; }

        #endregion

        public SchemePreview()
        {
            // Default initialized
            FocusParts = new List<SchemePreviewFocusPart>();
            TargetGrowths = new List<TargetGrowth>();
            Categories = new List<ResponseSchemeCategoryModel>();
            TargetWorkshops = new List<ResponseTargetWorkshopModel>();
            Cashbacks = new List<ResponseCashBackModel>();
            CbrColText = new List<string>();
            AssuredGifts = new List<ResponseAssuredGiftModel>();
            LuckyDraws = new List<ResponseGiftManagementModel>();
            CouponAllocations = new List<SchemePreviewCouponAllocation>();
            TargetOverviews = new List<TargetOverview>();
        }
    }
}