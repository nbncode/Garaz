namespace Garaaz.Models.DashboardOverview.Sale
{
    public class PgCustomerSaleDetail
    {
        public int SlNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }

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

        public string ContributionTxt { get; set; }
        public int TotalRows { get; set; }
    }
}