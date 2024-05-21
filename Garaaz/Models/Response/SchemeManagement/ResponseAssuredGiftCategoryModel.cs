using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseAssuredGiftCategoryModel
    {
        public int AssuredGiftCategoryId { get; set; }
        public int AssuredGiftId { get; set; }
        public int? CategoryId { get; set; }
        public bool IsAll { get; set; }
    }
}