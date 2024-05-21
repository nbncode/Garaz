using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class AssuredGiftModel
    {
        public int AssuredGiftId { get; set; }
        public int SchemeId { get; set; }
        public string Target { get; set; }
        public string Point { get; set; }
        public string Reward { get; set; }
        public string Categories { get; set; }
       // public List<GiftCategoryModel> ListGiftCategory { get; set; }
    }
}