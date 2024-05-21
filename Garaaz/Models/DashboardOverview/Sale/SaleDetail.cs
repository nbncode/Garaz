namespace Garaaz.Models.DashboardOverview.Sale
{
    public class SaleDetail
    {
        public decimal PreviousSale { get; set; }
        public decimal CurrentSale { get; set; }
        public decimal CoDealerOrDistSale { get; set; }

        // Following properties are used as common
        public int SlNo { get; set; }
        public string CustomerType { get; set; }
        public int NumberOfCustomers { get; set; }

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