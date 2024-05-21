using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class CouponNoRequest
    {
        [Required(ErrorMessage = "Coupon Number Required")]
        public string CouponNumber { get; set; }
    }
}