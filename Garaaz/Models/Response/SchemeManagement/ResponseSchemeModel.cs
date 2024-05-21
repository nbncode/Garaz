using System.Collections.Generic;

namespace Garaaz.Models
{
    public class ResponseSchemeModel
    {
        /// <summary>
        /// Gets or sets current scheme Id.
        /// </summary>
        public int SchemeId { get; set; }

        /// <summary>
        /// Gets or sets scheme name.
        /// </summary>
        public string SchemeName { get; set; }

        /// <summary>
        /// Gets or sets scheme's type (Cashback or Lucky draw).
        /// </summary>
        public string SchemeType { get; set; }

        /// <summary>
        /// Gets or sets sub-scheme of 'Cashback' scheme type.
        /// </summary>
        public string SubSchemeType { get; set; }

        /// <summary>
        /// Gets or sets whether 'Cashback' tab will be displayed to user.
        /// </summary>
        public bool? CashBack { get; set; }

        /// <summary>
        /// Gets or sets whether 'Assured Gift' tab will be displayed to user.
        /// </summary>
        public bool? AssuredGift { get; set; }

        /// <summary>
        /// Gets or sets whether 'Lucky Draw' tab will be displayed to user.
        /// </summary>
        public bool? LuckyDraw { get; set; }

        public int? DistributorId { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string DispersalFrequency { get; set; }

        /// <summary>
        /// Gets or sets the payback type for scheme.
        /// </summary>
        public string PaybackType { get; set; }
        public string ThumbnailImage { get; set; }
        public string BannerImage { get; set; }

        /// <summary>
        /// Gets or sets the part category.
        /// </summary>
        public string PartCategory { get; set; }
        /// <summary>
        /// Gets or sets the part group.
        /// </summary>
        public string PartType { get; set; }

        /// <summary>
        /// Gets or sets FMS start date.
        /// </summary>
        public string StartRange { get; set; }
        /// <summary>
        /// Gets or sets FMS end date.
        /// </summary>
        public string EndRange { get; set; }
        public decimal FValue { get; set; }
        public decimal MValue { get; set; }
        public decimal SValue { get; set; }
        public string PartCreations { get; set; }

        public bool? AreBothCbAgApplicable { get; set; }
        public bool AreBothAgLdApplicable { get; set; }

        public bool? IsFocusPartApplicable { get; set; }



        public string PartyType { get; set; }

        public string SchemeFor { get; set; }
        public string TargetWorkshopCriteria { get; set; }
        public List<SchemeLocation> schemeLocations { get; set; }
        public string TargetCriteria { get; set; }
        public List<LabelCriteria> LabelCriterias { get; set; }
        public string RoInchargeId { get; set; }
        public string SalesExecutiveId { get; set; }
        public bool? IsAllRoInchargeSelected { get; set; }
        public bool? IsAllSalesExecutiveSelected { get; set; }
        public int WorkshopId { get; set; }
        public string CashbackCriteria { get; set; }
        /// <summary>
        /// Get or set if coupon have been allocated to scheme.
        /// </summary>
        public bool IsCouponAllocated { get; set; }
        /// <summary>
        /// Get or set if gift have been allocated to workshop of current scheme.
        /// </summary>
        public bool IsGiftAllocated { get; set; }

        public string FocusPartBenifitType { get; set; }
        public string FocusPartBenifitTypeValue { get; set; }
        public decimal FocusPartTarget { get; set; }
        public string FocusPartBenifitTypeNumber { get; set; }


        public string BranchCode { get; set; }

        /// <summary>
        /// Gets or sets the previous year 'From' date.
        /// </summary>
        public string PrevYearFromDate { get; set; }

        /// <summary>
        /// Gets or sets the previous year 'To' date.
        /// </summary>
        public string PrevYearToDate { get; set; }

        /// <summary>
        /// Gets or sets the previous month 'From' date.
        /// </summary>
        public string PrevMonthFromDate { get; set; }

        /// <summary>
        /// Gets or sets the previous month 'To' date.
        /// </summary>
        public string PrevMonthToDate { get; set; }

        /// <summary>
        /// Gets or sets growth comparison percent minimum value that will be used for checking growth comparison minimum level.
        /// </summary>
        public decimal? GrowthCompPercentMinValue { get; set; }

        /// <summary>
        /// Gets or sets growth comparison percent base value by which growth comparison value will be increased.
        /// </summary>
        public decimal? GrowthCompPercentBaseValue { get; set; }
    }
}