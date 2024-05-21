using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RegisterUserStep3Model
    {
        [Required]
        public string UserId { get; set; }
        public int DistributorId { get; set; }
        public int WorkshopId { get; set; }
        [Required]
        public string Role { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Gstin { get; set; }
        public string Pincode { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
        public string LandlineNumber { get; set; }
        public string Location { get; set; }
        public string Password { get; set; }
    }
}