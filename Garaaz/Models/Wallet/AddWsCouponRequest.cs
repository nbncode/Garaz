using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class GenerateCouponRequest
    {
        //[Required(ErrorMessage = "WorkshopId Required")]
        //public int WorkShopId { get; set; }
        [Required(ErrorMessage = "WorkshopId Required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Coupon Amount Required")]
        public decimal CouponAmount { get; set; }
    }
}