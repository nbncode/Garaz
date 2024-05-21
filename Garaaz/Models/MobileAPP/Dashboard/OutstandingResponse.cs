using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class OutstandingResponse
    {
        public string UPID { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
        public string TotalOutstanding { get; set; }
        public string CriticalOutstanding { get; set; }
        public string CreditLimit { get; set; }
        public string CriticalOsLessThan { get; set; }
        public int CriticalOsLessThanDays { get; set; }
        public string CriticalOsGraterThan { get; set; }
        public int CriticalOsGraterThanDays { get; set; }
        public string ClosingDate { get; set; }

        public string Outstanding0To7Days { get; set; }
        public string Outstanding7To14Days { get; set; }
        public string Outstanding14To21Days { get; set; }
        public string Outstanding21To28Days { get; set; }
        public string Outstanding28To35Days { get; set; }
        public string Outstanding35To50Days { get; set; }
        public string Outstanding50To70Days { get; set; }
        public string OutstandingAbove70Days { get; set; }

    }
}