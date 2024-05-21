using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ProductAvailablityRequest
    {
        [Required(ErrorMessage = "TempOrderId required.")]
        public int TempOrderId { get; set; }
        [Required(ErrorMessage = "ProductId required.")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Product Availablity Type required.")]
        public string AvailablityTypeId { get; set; }
    }
}