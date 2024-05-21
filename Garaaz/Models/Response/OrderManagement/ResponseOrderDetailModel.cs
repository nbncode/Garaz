using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class ResponseOrderDetailModel
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public Nullable<int> ProductID { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public string Brand { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<int> Qty { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        public string ImagePath { get; set; }
        public int? OutletId { get; set; }
        public string AvailabilityType { get; set; }      
        public int? Quantity { get; set; }
        public string OutletName { get; set; }
    }
    public class OrderPartsDetailModel
    {
        public string PartNumber { get; set; }
        public int? Quantity { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
    }
}