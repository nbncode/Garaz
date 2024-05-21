using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class WorkShopSchemes
    {
        public int WorkShopId { get; set; }
        public string WorkShopName { get; set; }
        public string ConstPartyCode { get; set; }
        public bool IsQualifiedAsDefault { get; set; }
        public decimal? Growth { get; set; }
        public decimal? TargetWithoutGrowth { get; set; }
    }
}