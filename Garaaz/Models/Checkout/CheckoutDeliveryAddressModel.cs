using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class CheckoutDeliveryAddressModel
    {
        public int TempOrderId { get; set; }

        public string UserId { get; set; }

        [Required (ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Pincode is required.")]
        public string Pincode { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }
        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }
    }
}