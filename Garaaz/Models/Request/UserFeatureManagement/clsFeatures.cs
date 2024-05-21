using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class clsFeatures
    {
        public int FeatureId { get; set; }

        [Required(ErrorMessage = "Features Name Required")]
        public string FeatureName { get; set; }

        [Required(ErrorMessage = "Features Value Required")]
        public string FeatureValue { get; set; }
        public bool IsDefault { get; set; }
    }

}