namespace Garaaz.Models.DashboardOverview.Cbo
{
    public class CboCustomerDetail
    {
        public int SlNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CoNumber { get; set; }
        public int NumberOfParts { get; set; }

        /// <summary>
        /// Gets or sets number of days passed since ordered.
        /// </summary>
        public double NumberOfDaysSinceOrder { get; set; }

        public decimal CboPrice { get; set; }
        public int TotalRows { get; set; }
    }
}