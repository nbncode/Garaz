using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class WalletRequest
    {
        [Required(ErrorMessage ="PageNumber Required")]
        public int PageNumber { get; set; }
        public string CouponNumber { get; set; }
        public int? DistributorId { get; set; }
    }
}