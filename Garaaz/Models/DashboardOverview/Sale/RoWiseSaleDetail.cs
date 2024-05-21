namespace Garaaz.Models.DashboardOverview.Sale
{
    public class RoWiseSaleDetail : SaleDetail
    {
        /// <summary>
        /// Gets or sets average sale of 3 months.
        /// </summary>
        public decimal AverageSale { get; set; }

        public string BranchCode { get; set; }
    }
}