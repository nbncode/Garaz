namespace Garaaz.Models.DashboardOverview.Wallet
{
    public class WalCustomerDetail
    {
        public int SlNo { get; set; }
        public string DateOfTransaction { get; set; }
        public string TransactionDetails { get; set; }
        public decimal Amount { get; set; }
        public int TotalRows { get; set; }
    }
}