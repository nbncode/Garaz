using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class DistributorWorkshopUser
    {
        [Required]
        public int DistributorId { get; set; }
        [Required]
        public int WorkshopId { get; set; }
    }
}