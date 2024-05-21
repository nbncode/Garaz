using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class QualifyCriteriaModel
    {
        public int QualifyCriteriaId { get; set; }
        public int SchemeId { get; set; }
        public decimal? AmountUpto { get; set; }
        public string Type { get; set; }
        public int? NumberOfCoupons { get; set; }
        public string TypeValue { get; set; }
        public decimal? AdditionalCouponAmount { get; set; }
        public int? AdditionalNumberOfCoupons { get; set; }
    }
}