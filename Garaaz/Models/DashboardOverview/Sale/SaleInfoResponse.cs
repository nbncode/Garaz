namespace Garaaz.Models.DashboardOverview.Sale
{
    public class SaleInfoResponse
    {
        public string DateInfo { get; set; }
        public string TotalSale { get; set; }
        public decimal GrowthPercentage { get; set; }
        public string CoDealerDistSale { get; set; }
        public string Footer { get; set; }
        
        /// <summary>
        /// Gets or sets the category for further filtering sales.
        /// </summary>
        public string Category { get; set; }
    }
    public class SaleCategory
    {
        public string Category { get; set; }
        public decimal Sale { get; set; }
    }
}