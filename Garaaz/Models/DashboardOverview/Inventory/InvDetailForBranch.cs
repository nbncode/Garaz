namespace Garaaz.Models.DashboardOverview.Inventory
{
    public class InvDetailForBranch
    {
        public int SlNo { get; set; }
        public string PartGroup { get; set; }
        public string PartCategory { get; set; }
        public string RootPartNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public decimal Mrp { get; set; }
        public decimal AvgConsumption { get; set; }
        public decimal NumberOfStock { get; set; }
    }
}