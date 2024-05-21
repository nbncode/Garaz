namespace Garaaz.Models.DashboardOverview.Outstanding
{
    public class OsCustomer
    {
        public string PartyCode { get; set; }
        public string PartyName { get; set; }
        public string CustomerType { get; set; }
        public string SalesExecutiveOrBranchCode { get; set; }
        public string PendingBills { get; set; }
        public string LessThan7Days { get; set; }
        public string C7To14Days { get; set; }
        public string C14To21Days { get; set; }
        public string C21To28Days { get; set; }
        public string C28To35Days { get; set; }
        public string C35To50Days { get; set; }
        public string C50To70Days { get; set; }
        public string MoreThan70Days { get; set; }
    }
}