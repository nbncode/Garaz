using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class MgaBannerRequest
    {
        [Required(ErrorMessage = "UserId Required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Role Required")]
        public string Role { get; set; }
        [Required(ErrorMessage = "PageNumber Required")]
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    public class MgaBannerProductRequest
    {
        [Required(ErrorMessage = "BannerId Required")]
        public int BannerId { get; set; }
        [Required(ErrorMessage = "PageNumber Required")]
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? TempOrderId { get; set; }
    }
}