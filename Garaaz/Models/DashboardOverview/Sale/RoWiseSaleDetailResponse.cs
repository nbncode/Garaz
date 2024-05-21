namespace Garaaz.Models.DashboardOverview.Sale
{
    /// <summary>
    /// Parameters to be used for getting Ro wise sale details using stored procedure.
    /// </summary>
    public class SaleDetailResponse
    {
        public string CustomerType { get; set; }
        public int NumberOfCustomers { get; set; }
        public decimal AvgSale { get; set; }
        public decimal NetRetailSelling { get; set; }

        // used in RoWiseBranchSaleDetails
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        
        public int TotalRows { get; set; }
    }
}