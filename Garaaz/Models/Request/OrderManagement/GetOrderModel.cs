using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class GetOrderModel
    {
        [Required(ErrorMessage = "UserId required.")]
        public string  UserId { get; set; }
        [Required(ErrorMessage = "OrderId required.")]
        public int  OrderId { get; set; }
    }
}