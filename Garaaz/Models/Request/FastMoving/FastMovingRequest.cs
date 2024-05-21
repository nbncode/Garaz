using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class FastMovingRequest
    {
        [Required(ErrorMessage = "UserId Required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Role Required")]
        public string Role { get; set; }
        [Required(ErrorMessage = "Start Date Required")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End Date Required")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "Page number Required")]
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }

    }
    
}