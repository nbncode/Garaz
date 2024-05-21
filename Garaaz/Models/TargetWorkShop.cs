//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Garaaz.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TargetWorkShop
    {
        public int TargetWorkShopId { get; set; }
        public Nullable<int> WorkShopId { get; set; }
        public Nullable<int> SchemeId { get; set; }
        public string CustomerType { get; set; }
        public Nullable<decimal> PrevYearAvgSale { get; set; }
        public Nullable<decimal> GrowthPercentage { get; set; }
        public string NewTarget { get; set; }
        public Nullable<decimal> PrevMonthAvgSale { get; set; }
        public Nullable<decimal> GrowthComparisonPercentage { get; set; }
        public Nullable<bool> IsQualifiedAsDefault { get; set; }
        public Nullable<decimal> TargetAchieved { get; set; }
        public Nullable<decimal> TargetAchievedPercentage { get; set; }
    
        public virtual WorkShop WorkShop { get; set; }
        public virtual Scheme Scheme { get; set; }
    }
}
