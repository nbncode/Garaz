using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class InvoiceDetailResponse
    {
        public string CoNo { get; set; }
        public string PartNumber { get; set; }
        public string Date { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public string OutletName { get; set; }
    }
}