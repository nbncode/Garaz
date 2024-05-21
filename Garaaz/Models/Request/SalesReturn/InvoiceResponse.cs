using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class InvoiceResponse
    {
        public string CoNo { get; set; }
        public decimal TotalDec { get; set; }
        public string Total { get; set; }
        public DateTime Date { get; set; }
        public string OrderDate { get; set; }
        public int QtyCount { get; set; }
        public int Count { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
    }
}