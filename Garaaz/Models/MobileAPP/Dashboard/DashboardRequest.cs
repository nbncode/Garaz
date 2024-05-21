using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class DashboardRequest
    {
        [Required(ErrorMessage ="UserId Required")]
        public string UserId { get; set; }        
        public IEnumerable<string> Roles { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}