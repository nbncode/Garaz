﻿namespace Garaaz.Models.DashboardOverview.Outstanding
{
    public class BranchOsDetail
    {
        public int SlNo { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public int NoOfCustomers { get; set; }
        public decimal OutstandingDays { get; set; }
        public decimal Outstanding { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CriticalPayment { get; set; }
        public decimal? ZeroToFourteenDays { get; set; }
        public decimal? FourteenToTwentyEightDays { get; set; }
        public decimal? TwentyEightToFiftyDays { get; set; }
        public decimal? FiftyToSeventyDays { get; set; }
        public decimal? MoreThanSeventyDays { get; set; }
    }
}