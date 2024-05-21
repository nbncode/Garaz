using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class AdditionalCouponModel
    {
        public int AdditionalCouponId { get; set; }
        public int SchemeId { get; set; }
        public decimal? Amount { get; set; }
        public int? NumberOfCoupons { get; set; }
    }
}