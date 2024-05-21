using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class WorkshopCouponResponse
    {
        public int CouponId { get; set; }
        public int WorkshopId { get; set; }
        public string UserId { get; set; }
        public string CouponNo { get; set; }
        public decimal Amount { get; set; }
        public string Sign { get; set; }
        public string Date { get; set; }

    }
}