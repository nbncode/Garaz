using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public int GroupId { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string ImagePath { get; set; }
        public int Stock { get; set; }
        public string Color { get; set; }
        public string ColorTag { get; set; }
        public string DistributorName { get; set; }
        public int? DistributorId { get; set; }
        public int? CartQty { get; set; }
        public int? CartOutletId { get; set; }
        public string CartAvailabilityType { get; set; }
        public bool IsOriparts { get; set; }
        public int? BrandId { get; set; }
    }
}