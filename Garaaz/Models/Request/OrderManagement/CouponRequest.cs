using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class CouponRequest
    {
        [Required(ErrorMessage ="SchemeId Required")]
        public int SchemeId { get; set; }
        [Required(ErrorMessage = "UserId Required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Role Required")]
        public string Role { get; set; }
        [Required(ErrorMessage = "PageNumber Required")]
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}