namespace Garaaz.Models.DashboardOverview.Outstanding
{
    public class CustomerOsDetail
    {
        public int SlNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public decimal OutstandingDays { get; set; }
        public decimal Outstanding { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CriticalPayment { get; set; }
        public decimal? ZeroToFourteenDays { get; set; }
        public decimal? FourteenToTwentyEightDays { get; set; }
        public decimal? TwentyEightToFiftyDays { get; set; }
        public decimal? FiftyToSeventyDays { get; set; }
        public decimal? MoreThanSeventyDays { get; set; }
        public int TotalRows { get; set; }
    }
}