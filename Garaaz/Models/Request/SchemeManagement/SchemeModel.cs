using System;

namespace Garaaz.Models
{
    public class SchemeModel
    {
        public int SchemeId { get; set; }
        public string SchemeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DispersalFrequency { get; set; }
        public string SchemesType { get; set; }
        public string SubSchemeType { get; set; }
        public bool? IsAssuredGift { get; set; }
        public bool? IsCashBack { get; set; }
        public bool? IsLuckyDraw { get; set; }
        public string Types { get; set; }
        public string UserId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public int? DistributorId { get; set; }
        public string BannerImage { get; set; } 
        public string ThumbnailImage { get; set; }
        
        // FOR STEP - PART INFO
        public string PartType { get; set; }
        public string PartCategory { get; set; }
        public decimal? FocusPartTarget { get; set; }
        public string FocusPartBenifitType { get; set; }
        public string FocusPartBenifitTypeValue { get; set; }
        public string FocusPartBenifitTypeNumber { get; set; }
        public DateTime? StartRange { get; set; }
        public DateTime? EndRange { get; set; }
        public decimal? FValue { get; set; }
        public decimal? MValue { get; set; }
        public decimal? SValue { get; set; }
        public string PartCreations { get; set; }

        // FOR STEP - CUSTOMER SEGMENT
        public string BranchCode { get; set; }
        public string SalesExecutiveId { get; set; }
        public string PartyType { get; set; }

        // FOR STEP - TARGET INFO
        /// <summary>
        /// Gets or sets path of excel file holding workshops data.
        /// </summary>
        public string FilePath { get; set; }
        public DateTime? PrevYearFromDate { get; set; }
        public DateTime? PrevYearToDate { get; set; }
        public DateTime? PrevMonthFromDate { get; set; }
        public DateTime? PrevMonthToDate { get; set; }
        /// <summary>
        /// Gets or sets growth comparison percent minimum value that will be used for checking growth comparison minimum level.
        /// </summary>
        public decimal GrowthCompPercentMinValue { get; set; }
        /// <summary>
        /// Gets or sets growth comparison percent base value by which growth comparison value will be increased.
        /// </summary>
        public decimal GrowthCompPercentBaseValue { get; set; }
        
        // FOR STEP - REWARD INFO
        public string CashbackCriteria { get; set; }

        /// <summary>
        /// Gets or sets whether both cashback and assured gift applicable.
        /// </summary>
        public bool? AreBothCbAgApplicable { get; set; }

        /// <summary>
        /// Gets or sets whether both assured gift and lucky draw applicable.
        /// </summary>
        public bool AreBothAgLdApplicable { get; set; }
        public bool? CanTakeMoreThanOneGift { get; set; }
        public int? MaxGiftsAllowed { get; set; }

        // OTHERS
        public bool? IsFocusPartApplicable { get; set; }
        public string TargetCriteria { get; set; }
        public string SchemeFor { get; set; }
        public string Locations { get; set; }
        public string TargetWorkshopCriteria { get; set; }
        public string Role { get; set; }
        
        
    }
}