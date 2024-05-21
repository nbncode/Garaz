namespace Garaaz.Models.DashboardOverview.Sale
{
    public class PgWiseSaleDetail
    {
        public int SlNo { get; set; }
        public string GroupType { get; set; }
        public int NumberOfCustomer { get; set; }

        /// <summary>
        /// Gets or sets average sale of 3 months.
        /// </summary>
        public decimal AverageSale { get; set; }

        public decimal NetRetailSelling { get; set; }
        public decimal Contribution { get; set; }

        /// <summary>
        /// Gets or sets sales done in previous time period.
        /// </summary>
        /// 
        public decimal? PrevAchieved { get; set; }
    }
}