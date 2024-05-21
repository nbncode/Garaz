using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class OrderStatusRequest
    {
        [Required(ErrorMessage = "OrderId required.")]
        public int  OrderId { get; set; }
    }
}