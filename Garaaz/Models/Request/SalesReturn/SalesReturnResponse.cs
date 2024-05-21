using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class SalesReturnResponse
    {
        public int ProductId { get; set; }
        public int GroupId { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string ImagePath { get; set; }
        public string ReturnDate { get; set; }
        public DateTime? Date { get; set; }
        public int? CartQty { get; set; }
        public int? CartOutletId { get; set; }
        public string CartAvailabilityType { get; set; }
        //public string Status { get; set; }
    }
}