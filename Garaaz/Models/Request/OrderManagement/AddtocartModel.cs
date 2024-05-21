using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class AddtocartModel
    {
        public int TempOrderId { get; set; }
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public string UserId { get; set; }
        public string PartNumber { get; set; }
        public int? OutletId { get; set; }
        public string Role { get; set; }
    }
}