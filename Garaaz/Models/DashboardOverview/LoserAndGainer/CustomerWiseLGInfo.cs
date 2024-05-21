using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models.DashboardOverview.LoserAndGainer
{
    /// <summary>
    /// Define Customer wise Looser and Gainers details.
    /// </summary>
    public class CustomerWiseLGInfo
    {
        public int SlNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public int CurrentOrderDays { get; set; }
        public int PreviousOrderDays { get; set; }
        public string GrowthDays { get; set; }
        public decimal CurrentOrderValue { get; set; }
        public decimal PreviousOrderValue { get; set; }
        public string GrowthValue { get; set; }
    }
}