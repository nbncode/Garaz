using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class ChangePasswordModel
    {
        public string UserId { get; set; }
        [Required(ErrorMessage = "Please Enter New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Please Enter Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        //public string Controller { get; set; }
        //public string Action { get; set; }

    }
    public class ChangeMobileModel
    {
        [Required(ErrorMessage = "Please enter new mobile number")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string NewMobileNumber { get; set; }
        [Required(ErrorMessage = "Please enter confirm mobile number")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string ConfirmMobileNumber { get; set; }

    }
}