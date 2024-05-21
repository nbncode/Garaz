using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models.DashboardOverview.LoserAndGainer
{
    public class LooserAndGainersDetails
    {
        public int SlNo { get; set; }
        public string CustomerType { get; set; }
        public int NumberOfCustomers { get; set; }
        public int Loosers { get; set; }
        public decimal LostRetailValue { get; set; }
        public int Gainers { get; set; }
        public decimal GainedRetailValue { get; set; }
        public int CurrentOrderDays { get; set; }
        public int PreviousOrderDays { get; set; }
        public decimal GrowthDays { get; set; }
        public decimal CurrentOrderValue { get; set; }
        public decimal PreviousOrderValue { get; set; }
        public decimal GrowthValue { get; set; }
        public string SubGroup { get; set; }
    }
}