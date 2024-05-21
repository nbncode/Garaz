namespace Garaaz.Models.Schemes
{
    /// <summary>
    /// Represent a row of target overview.
    /// </summary>
    public class TargetOverview
    {
        public string TargetGrowthRange { get; set; }
        public string GrowthPercentage { get; set; }
        public int TotalWorkshops { get; set; }
        public double TotalTarget { get; set; }
        public double AverageTarget { get; set; }
        public double MaximumTarget { get; set; }
        public double MinimumTarget { get; set; }
    }
}