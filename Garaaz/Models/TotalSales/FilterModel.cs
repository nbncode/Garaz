using System;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class FilterModel
    {
        [Required (ErrorMessage ="Start date is required")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "Group Id is required")]
        public int GroupId { get; set; }
        [Required(ErrorMessage = "Frequency is required")]
        public Frequency Frequency { get; set; }
        [Required(ErrorMessage = "Growth is required")]
        public Growth Growth { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }
        [Required(ErrorMessage = "User Id is required")]
        public string UserId { get; set; }

        /// <summary>
        /// Get or set to check if request is coming from mobile app.
        /// </summary>
        public bool IsFromMobile { get; set; }
    }
}