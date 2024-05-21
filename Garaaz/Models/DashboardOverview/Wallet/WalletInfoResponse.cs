namespace Garaaz.Models.DashboardOverview.Wallet
{
    public class WalletInfoResponse
    {
        public string DateInfo { get; set; }
        public string TotalWalletBalance { get; set; }
        public decimal PayoutOfSalesPercentage { get; set; }
        public string Category { get; set; }
        public string Footer { get; set; }
    }
}