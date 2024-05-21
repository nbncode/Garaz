using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models.DashboardOverview.LoserAndGainer
{
    public class BranchWiseLGInfo: LooserAndGainersDetails
    {
        public string BranchCode { get; set; }
        public string BranchOrSalesName { get; set; }
        public string SeUserId { get; set; }
    }
}