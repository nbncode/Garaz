using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class OrderPartAutoComplete
    {
        [Required(ErrorMessage ="Search Text required")]
        public string SearchText { get; set; }
        public int? TempOrderId { get; set; }
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}