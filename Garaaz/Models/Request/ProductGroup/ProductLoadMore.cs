using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class ProductLoadMore
    {
        [Required(ErrorMessage = "BannerId Required")]
        public int BannerId { get; set; }
        [Required(ErrorMessage = "DistributorId Required")]
        public int DistributorId { get; set; }
        [Required(ErrorMessage = "GroupId Required")]
        public int GroupId { get; set; }
        [Required(ErrorMessage = "PageNumber Required")]
        public int PageNumber { get; set; }
    }
}