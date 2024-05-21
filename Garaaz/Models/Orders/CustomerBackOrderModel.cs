using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class CustomerBackOrderModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public int PageNumber { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public int Qty { get; set; }
        public string Status { get; set; }
        public List<ProductModel> Products { get; set; }
        public string PartNumber { get; set; }
        public string PartyName { get; set; }
    }
}