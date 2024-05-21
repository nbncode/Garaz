
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class TargetGrowthRequest
    {
        [Required(ErrorMessage ="Please select schemeId.")]
        public int SchemeId { get; set; }
        //public decimal Min { get; set; }
        //public decimal Max { get; set; }
        //public decimal Growth { get; set; }
    }
}