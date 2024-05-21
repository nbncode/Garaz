namespace Garaaz.Models.DashboardOverview.Inventory
{
    public class InvDetail
    {
        public int SlNo { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public decimal StockDays { get; set; }
        public decimal AverageSales { get; set; }
        public decimal StockPrice { get; set; }
        public decimal PartLinesInStock { get; set; }
    }
}