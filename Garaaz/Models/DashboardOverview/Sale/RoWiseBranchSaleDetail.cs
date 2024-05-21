namespace Garaaz.Models.DashboardOverview.Sale
{
    public class RoWiseBranchSaleDetail
    {
        public int SlNo { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public int TotalCustomers { get; set; }

        /// <summary>
        /// Gets or sets average sale of 3 months.
        /// </summary>
        public decimal AverageSale { get; set; }

        /// <summary>
        /// Gets or sets sales done in previous time period.
        /// </summary>
        public decimal? PrevAchieved { get; set; }

        /// <summary>
        /// Gets or sets sales sum value in selected time period.
        /// </summary>
        public decimal Achieved { get; set; }

        /// <summary>
        /// Calculate sale achieved % based on PrevAchieved and Achieved sales.
        /// </summary>
        public string AchievedPercentage { get; set; }
    }
}