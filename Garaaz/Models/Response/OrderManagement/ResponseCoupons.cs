using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class ResponseCoupons
    {
        public int CouponId { get; set; }
        public int SchemeId { get; set; }
        public int WorkshopId { get; set; }
        public string CouponNumber { get; set; }
        public bool IsGifted { get; set; }
        public string GiftName { get; set; }
    }

}