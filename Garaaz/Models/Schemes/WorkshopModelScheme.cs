using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class WorkshopModelScheme
    {
        public int WorkShopId { get; set; }
        public string WorkShopName { get; set; }
        public string ConsPartyCode { get; set; }
        public string Address { get; set; }
    }

    public class DailySalesData
    {
        public int? WorkShopId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal NetRetailSelling { get; set; }
    }

    public class Datesinfo
    {
        public string LastYearDate { get; set; }
        public string LastMonthDate { get; set; }
        public string CurrentDate { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalTarget { get; set; }
    }
}