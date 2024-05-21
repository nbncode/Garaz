using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class BannerProductResponse
    {
        public int ProductId { get; set; }
        public int GroupId { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string ImagePath { get; set; }
    }
}