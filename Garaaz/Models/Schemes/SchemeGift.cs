namespace Garaaz.Models
{
    public class SchemeGift
    {
        public int SchemeId { get; set; }
        public int WorkshopId { get; set; }
        public int GiftId { get; set; }
        public string CouponNumber { get; set; }

        /// <summary>
        /// Get or set comma separated coupons.
        /// </summary>
        public string Coupons { get; set; }
        /// <summary>
        /// Get or set the number of gifts remaining.
        /// </summary>
        public int GiftRemainingQty { get; set; }
        public string GiftName { get; set; }
        public string SchemeName { get; set; }

    }
}