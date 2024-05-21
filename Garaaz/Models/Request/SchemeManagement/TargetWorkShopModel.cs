namespace Garaaz.Models
{
    public class TargetWorkshopModel
    {
        public int TargetWorkShopId { get; set; }
        public int WorkShopId { get; set; }
        public int SchemeId { get; set; }

        public string CustomerType { get; set; }
        public decimal? PrevYearAvgSale { get; set; }
        public decimal? GrowthPercentage { get; set; }
        public string NewTarget { get; set; }
        public decimal? PrevMonthAvgSale { get; set; }
        public decimal? GrowthComparisonPercentage { get; set; }
        public bool IsQualifiedAsDefault { get; set; }
    }
}