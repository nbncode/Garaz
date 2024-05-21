using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class WalletResponse
    {
        public int WorkShopId { get; set; }
        public string WorkShop { get; set; }
        public string UserId { get; set; }
        public decimal WalletAmount { get; set; }
        public string Sign { get; set; }
        public int TotalCoupon { get; set; }
    }
}