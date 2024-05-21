namespace Garaaz.Models.DashboardOverview.Outstanding
{
    public class OsInfoResponse
    {
        public string DateInfo { get; set; }
        public string TotalOutstanding { get; set; }
        public string OutstandingDays { get; set; }
        public string CriticalPayment { get; set; }

        /// <summary>
        /// Gets or sets the category for further filtering outstandings.
        /// </summary>
        public string Category { get; set; }

        public string Footer { get; set; }
    }
}