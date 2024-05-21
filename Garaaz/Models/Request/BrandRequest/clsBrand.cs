using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class clsBrand
    {
        public int? BrandId { get; set; }

        [Required]
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public string SearchString { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public bool IsOriparts { get; set; }
    }
    
}