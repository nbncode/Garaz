using System.Collections.Generic;

namespace Garaaz.Models
{
    public class WorkshopCoupons
    {
        /// <summary>
        /// Get or set the workshop.
        /// </summary>
        public WorkShop Workshop { get; set; }
        /// <summary>
        /// Get or set the coupons allocated to current workshop.
        /// </summary>
        public List<string> Coupons { get; set; }      
        
        public WorkshopCoupons()
        {
            Coupons = new List<string>();
        }
    }
}