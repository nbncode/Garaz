namespace Garaaz.Models.DashboardOverview.Customer
{
    public class BranchCustomerDetail
    {
        public int SlNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public decimal AvgSale { get; set; }
        public decimal CurrentMonthSale { get; set; }
        public decimal PrvYrSale { get; set; }

        public int NonBilledFromDays { get; set; }
        public int TotalRows { get; set; }
    }
}