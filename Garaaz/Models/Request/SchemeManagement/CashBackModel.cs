using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class CashBackModel
    {
        public int CashbackId { get; set; }
        public int SchemeId { get; set; }
        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
        public string Benifit { get; set; }
        public string BenifitType { get; set; }
        public List<CashBackRangeValues> lstCashbackRange { get; set; }

    }

    public class CashBackRangeModel
    {
        public int CashbackRangeId { get; set; }
        public int SchemeId { get; set; }
        public decimal? StartRange { get; set; }
        public decimal? EndRange { get; set; }
    }

    public class CashBackRangeValues
    {
        public int CashbackRangeId { get; set; }
        public decimal Percentage { get; set; }
    }
}