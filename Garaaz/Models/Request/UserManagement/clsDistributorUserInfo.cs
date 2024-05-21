using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class clsDistributorUserInfo
    {
        public int DistributorId { get; set; }
        public string UserId { get; set; }

        [Required]
        public int OutletId { get; set; }
        public int WorkshopId { get; set; }
        [Required(ErrorMessage = "You must provide a phone number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        public bool IsApproved { get; set; }
        [Required(ErrorMessage = "Employee code is required.")]
        public string EmployeeCode { get; set; }
        public string Designations { get; set; }
        [Required(ErrorMessage = "Please select RoIncharge.")]
        public string RoInchargeId { get; set; }
    }
}