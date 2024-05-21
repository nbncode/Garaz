using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseLabelCriteriaModel
    {
        public int CriteriaId { get; set; }
        public int LabelId { get; set; }
        public int GroupId { get; set; }
        public int ProductId { get; set; }
        public string Type { get; set; }
        public string value { get; set; }
    }
}