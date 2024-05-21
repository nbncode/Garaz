namespace Garaaz.Models.DashboardOverview.Collection
{
    public class ColCustomerDetail : Customer
    {
        public string PaymentDate { get; set; }
        public string Particulars { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string VoucherType { get; set; }
    }
}