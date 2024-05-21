using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class FocusPartModel
    {
        public int FocusPartId { get; set; }
        public int SchemeId { get; set; }
        public int? GroupId { get; set; }
        public int? ProductId { get; set; }
        public int? Qty { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
    }

    public class FocusPartBenifitTypeModel
    {
        public int SchemeId { get; set; }
        public decimal? FocusPartTarget { get; set; }
        public string FocusPartBenifitType { get; set; }
        public string FocusPartBenifitTypeValue { get; set; }
        public string FocusPartBenifitTypeNumber { get; set; }
    }
}