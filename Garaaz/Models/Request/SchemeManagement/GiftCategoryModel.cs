using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class GiftCategoryModel
    {
        public int GiftCategoryId { get; set; }
        public int GiftId { get; set; }
        public int CategoryId { get; set; }
    }
}