namespace Garaaz.Models.Schemes
{
    public class SchemePreviewCouponAllocation
    {
        public decimal? Amount { get; set; }
        public string Type { get; set; }
        public string PartsOrAccessories{ get; set; }
        public int? NumberOfCoupons { get; set; }
        public bool IsAll { get; set; }
        public decimal? AdditionalCouponAmount { get; set; }
        public int? AdditionalNumberOfCoupons { get; set; }
    }
}