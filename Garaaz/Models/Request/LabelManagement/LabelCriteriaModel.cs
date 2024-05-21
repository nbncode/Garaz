using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class LabelCriteriaModel
    {
        public int CriteriaId { get; set; }
        public string TypeOfCriteria { get; set; }
        public int LabelId { get; set; }
        public int? GroupId { get; set; }
        public int? ProductId { get; set; }
        public string Condition { get; set; }
        public string Value { get; set; }
        public string SaleCondition { get; set; }
        public decimal? SaleAmount { get; set; }
    }
}