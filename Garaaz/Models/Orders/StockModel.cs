using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class StockModel
    {
        [Required(ErrorMessage = "The User Id is required.")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "The role is required.")]
        public string Role { get; set; }
        public int? OutletId { get; set; }
        public string PartNumber { get; set; }
        public int? StockColorId { get; set; }
        [Required(ErrorMessage = "The page number is required.")]
        public int PageNumber { get; set; }        
        public int? TempOrderId { get; set; }
    }    
}