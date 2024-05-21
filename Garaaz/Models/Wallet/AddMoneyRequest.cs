using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class AddMoneyRequest
    {
        [Required(ErrorMessage ="WorkshopId Required")]
        public int WorkShopId { get; set; }
        [Required(ErrorMessage = "WorkshopId Required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "WorkshopId Required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "WorkshopId Required")]
        public string Type { get; set; }
        public string RefId { get; set; }
        [Required(ErrorMessage = "WorkshopId Required")]
        public decimal WalletAmount { get; set; }
    }
}