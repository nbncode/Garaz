using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class FastMovingProductRequest
    {
        [Required(ErrorMessage = "GroupId Required")]
        public int GroupId { get; set; }
        [Required(ErrorMessage = "UserId Required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Role Required")]
        public string Role { get; set; }
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? TempOrderId { get; set; }

    }

}