using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseSchemeCategoryModel
    {
        public int CategoryId { get; set; }
        public int SchemeId { get; set; }
        public string Category { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}