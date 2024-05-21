using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class CriteriaWorkShopModel
    {
        public int DistributorId { get; set; }
        public int? SchemeId { get; set; }
        public int? GroupId { get; set; }
        public int? ProductId { get; set; }
        public string Condition { get; set; }
        public int? Qty { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }
        public string SalesExecutiveId { get; set; }
    }
}