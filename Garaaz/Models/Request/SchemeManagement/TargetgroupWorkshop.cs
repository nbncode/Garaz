using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class TargetgroupWorkshop
    {
        public int? workshopId { get; set; }
        public decimal Total { get; set; }
        public int Qty { get; set; }
    }
}