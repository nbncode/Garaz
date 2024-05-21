namespace Garaaz.Models.DashboardOverview.Cbo
{
    public abstract class CboDetail
    {
        public int SlNo { get; set; }
        public int NumberOfCboCustomers { get; set; }
        public decimal NumberOfCboOrders { get; set; }
        public decimal CboPrice { get; set; }
        public decimal CboPrice0To7Days { get; set; }
        public decimal CboPrice7To15Days { get; set; }
        public decimal CboPriceMoreThan15Days { get; set; }
        public decimal AvgSale { get; set; }
        public decimal CboPercentage { get; set; }
    }
}