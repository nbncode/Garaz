using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class InvoiceDetailRequest
    {
        [Required(ErrorMessage = "Co Number Required")]
        public string CoNo { get; set; }
        [Required(ErrorMessage = "UserId Required")]
        public string UserId { get; set; }
        
        public IEnumerable<string> Roles { get; set; }
        [Required(ErrorMessage = "Start Date Required")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End Date Required")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "PageNumber Required")]
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

}