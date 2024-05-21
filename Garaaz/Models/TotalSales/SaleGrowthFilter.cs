using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class SaleGrowthFilter
    {
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
        public string PartCategory { get; set; }
        [Required(ErrorMessage = "Frequency is required")]
        public Frequency Frequency { get; set; }
        [Required(ErrorMessage = "Growth is required")]
        public Growth Growth { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }
        [Required(ErrorMessage = "User Id is required")]
        public string UserId { get; set; }
    }
}