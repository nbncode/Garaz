namespace Garaaz.Models.DashboardOverview.Customer
{
    public abstract class BaseCustomer
    {
        public int SlNo { get; set; }
        public decimal TotalCustomers { get; set; }
        public decimal NonBilledCustomers { get; set; }
        public decimal BilledCustomers { get; set; }
        public decimal NonBilledCustomersRatio { get; set; }
        public decimal BilledCustomersRatio { get; set; }
    }
}