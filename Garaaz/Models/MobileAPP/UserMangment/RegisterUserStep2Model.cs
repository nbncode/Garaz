using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RegisterUserStep2Model
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Role { get; set; }

        public int WorkshopId { get; set; }
        public int DistributorId { get; set; }
    }
}