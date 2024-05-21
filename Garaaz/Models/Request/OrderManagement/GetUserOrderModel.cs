using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class GetUserOrderModel
    {
        [Required(ErrorMessage = "UserId required.")]
        public string  UserId { get; set; }
    }
    
}