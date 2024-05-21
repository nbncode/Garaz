using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class FastMovingResponse
    {
        public int GroupId { get; set; }
        public string Group { get; set; }
        public double Total { get; set; }
        public List<MonthWiseSale> data { get; set; }
    }
    public class MonthWiseSale
    {
        public string Name { get; set; }
        public string Qty { get; set; }
    }
}