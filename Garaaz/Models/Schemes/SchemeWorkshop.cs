namespace Garaaz.Models
{
    public class SchemeWorkshop
    {
        public int SchemeId { get; set; }
        public int WorkshopId { get; set; }
        public int GiftId { get; set; }
        public string WorkshopName { get; set; }
        public bool Qualified { get; set; }
        public int NumberOfCoupon { get; set; }
        public string CouponNumber { get; set; }

        /// <summary>
        /// Gets or sets whether workshop has selected lucky draw or not.
        /// </summary>
        public bool IsLuckyDrawSelected { get; set; }

        /// <summary>
        /// Gets or sets the explanation message if coupons not set for workshop when selected option other than lucky draw.
        /// </summary>
        public string MsgForNoCoupons { get; set; }

        public SchemeWorkshop()
        {
            // By default, so that we can show zero count on page
            IsLuckyDrawSelected = true;
        }
    }
}