using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class clsRegisterBrand
    {
        public int DistributorId { get; set; }
        public int WorkshopId { get; set; }
        public string Role { get; set; }
        public string BrandIds { get; set; }
    }
}