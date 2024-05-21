namespace Garaaz.Models
{
    public class ResponseTargetWorkshopModel
    {
        public int TargetWorkShopId { get; set; }
        public int WorkShopId { get; set; }
        public string WorkShopName { get; set; }
        public string WorkShopCode { get; set; }
        public int SchemeId { get; set; }

        public decimal Min { get; set; }
        public decimal Max { get; set; }

        /// <summary>
        /// Gets or sets the customer type of workshop.
        /// </summary>
        public string CustomerType { get; set; }

        /// <summary>
        /// Gets or sets previous year's average sale which is calculated by sum of sale as per date range divided by number of months.
        /// </summary>
        public decimal? PrevYearAvgSale { get; set; }

        /// <summary>
        /// Gets or sets growth percentage based on the range, that was set on 'Target Growth' page, in which previous year sale falls.
        /// </summary>
        public decimal? GrowthPercentage { get; set; }

        /// <summary>
        /// Gets or sets new target which is calculated by adding growth percentage of previous year average sale to previous year average sale.
        /// </summary>
        public string NewTarget { get; set; }

        /// <summary>
        /// Gets or sets previous month's average sale which is calculated by sum of sale as per date range divided by number of months.
        /// </summary>
        public decimal? PrevMonthAvgSale { get; set; }

        /// <summary>
        /// Gets or sets growth comparison percentage which is calculated by subtracting previous month average sale from new target.
        /// </summary>
        public decimal? GrowthComparisonPercentage { get; set; }
        public bool IsQualifiedAsDefault { get; set; }

        /// <summary>
        /// Gets or sets the Outlet Id for workshop.
        /// </summary>
        public int OutletId { get; set; }

        /// <summary>
        /// Gets or sets target achieved by workshop.
        /// </summary>
        public decimal TargetAchieved { get; set; }

        /// <summary>
        /// Gets or sets percentage of target achieved.
        /// </summary>
        public decimal AchievedPercentage { get; set; }
        public string LocationCode { get; set; }
        public string RoIncharge { get; set; }
        public string SalesExecutive { get; set; }
    }
    public class ResponseTargetWorkshopExport
    {
        public int SrNo { get; set; }
        public string LocationCode { get; set; }
        public string WorkShopName { get; set; }
        public string RoIncharge { get; set; }
        public string CustomerType { get; set; }
        public decimal? PrevYearAvgSale { get; set; }
        public decimal? GrowthPercentage { get; set; }
        public string NewTarget { get; set; }
        public decimal? PrevMonthAvgSale { get; set; }
        public decimal? GrowthComparisonPercentage { get; set; }
        public string IsQualified { get; set; }
        public decimal? TargetAchieved { get; set; }
        public decimal? AchievedPercentage { get; set; }
        public string SalesExecutive { get; set; }
    }
}