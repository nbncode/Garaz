using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class QualifyCriteriaUserLevel
    {
        public Nullable<decimal> AmountUpto { get; set; }
        public Nullable<int> NumberOfCoupons { get; set; }
        public Nullable<decimal> AdditionalCouponAmount { get; set; }
        public Nullable<int> AdditionalNumberOfCoupons { get; set; }
        public double AchievedTarget { get; set; }
    }
}