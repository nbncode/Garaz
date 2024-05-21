using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class clsDistributor
    {
        public string UserId { get; set; }
        [Required(ErrorMessage = "You must provide a phone number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public bool IsFromDistributorUser { get; set; }
        [Required]
        [DataType(DataType.Password)]
        //[RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)).+$")]
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string DistributorName { get; set; }
        public string WorkshopName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        public bool IsApproved { get; set; }
        public string Pincode { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
        public string LandlineNumber { get; set; }
        public string Gstin { get; set; }

        /// <summary>
        /// This is used to preserve the selection of radio button in Register view.
        /// </summary>
        public string RegTypeId { get; set; }

        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Field should be decimal number")]
        public decimal FValue { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Field should be decimal number")]
        public decimal MValue { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Field should be decimal number")]
        public decimal SValue { get; set; }

        [Required(ErrorMessage ="Employee code is required.")]
        public string EmployeeCode { get; set; }
        [Required(ErrorMessage = "Company is required.")]
        public string Company { get; set; }
        public string BrandIds { get; set; }
        public string UPIID { get; set; }
        public string BankName { get; set; }
        public string IFSCCode { get; set; }
        public string AccNo { get; set; }
    }

}