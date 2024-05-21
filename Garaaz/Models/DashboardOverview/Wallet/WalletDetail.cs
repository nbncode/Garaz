namespace Garaaz.Models.DashboardOverview.Wallet
{
    public class WalletDetail
    {
        public int SlNo { get; set; }
        public string CustomerType { get; set; }
        public int NumberOfCustomers { get; set; }
        public decimal AverageSale { get; set; }
        public decimal WalletBalance { get; set; }
        public decimal PayoutOfSalesPercentage { get; set; }

        public string BranchCode { get; set; }
        public string SubGroup { get; set; }
    }
}