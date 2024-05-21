using System.Collections.Generic;

namespace Garaaz.Models
{
    public class DecideWinnersModel
    {
        public int SchemeId { get; set; }
        public int GiftId { get; set; }
        public string GiftName { get; set; }
        public int GiftRemainingQty { get; set; }
        /// <summary>
        /// Get or set list of non-allocated coupons for workshop.
        /// </summary>
        public List<WorkshopCoupons> WorkshopCoupons { get; set; }
        /// <summary>
        /// Get or set list of marked coupons for workshop.
        /// </summary>
        public List<WorkshopCoupons> DefaultWinners { get; set; }
        
        public DecideWinnersModel()
        {
            WorkshopCoupons = new List<WorkshopCoupons>();
            DefaultWinners = new List<WorkshopCoupons>();
        }
    }
}