namespace Garaaz.Models.DashboardOverview.Sale
{
    /// <summary>
    /// Parameters to be used for getting Sales Executives wise sale details using stored procedure.
    /// </summary>
    public class SeWiseSaleDetailResponse
    {
        public string CustomerType { get; set; }
        public int NumberOfCustomers { get; set; }
        public decimal AvgSale { get; set; }
        public decimal NetRetailSelling { get; set; }
        public string BranchCode { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string SalesExecutiveName { get; set; }
        public int TotalRows { get; set; }
    }
}