using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class ResponseProductModel
    {
        public int ProductId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Remark { get; set; }
        public string ImagePath { get; set; }
        public decimal PriceDecimal { get; set; }
        public string Price { get; set; }
        public int? CartQty { get; set; }
        public int? CartOutletId { get; set; }
        public string CartAvailabilityType { get; set; }
        public int? BrandId { get; set; }
        public bool IsOriparts { get; set; }
    }
}