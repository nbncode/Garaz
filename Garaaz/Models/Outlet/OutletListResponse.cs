using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class OutletListResponse
    {
        public int OutletId { get; set; }        
        public string OutletName { get; set; }
        public bool IsDefault { get; set; }
    }

    public class DailyStockResponse
    {
        public int OutletId { get; set; }
        public string Qty { get; set; }
    }
}