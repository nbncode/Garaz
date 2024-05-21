using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class WorkshopCouponRequest
    {
        //[Required(ErrorMessage = "WorkshopId Required")]
        //public int WorkShopId { get; set; }
        [Required(ErrorMessage = "UserId Required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "PageNumber Required")]
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}