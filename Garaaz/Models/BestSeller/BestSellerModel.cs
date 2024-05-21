using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class BestSellerModel
    {
        //[Required(ErrorMessage ="WorkshopId Required")]
        public int GroupId { get; set; }
        public string Group { get; set; }
        public bool IsBestSeller { get; set; }
    }
}