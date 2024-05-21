using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseLabelModel
    {
        public int Id { get; set; }
        public string LabelName { get; set; }
        public string UserId { get; set; }
        public List<ResponseLabelCriteriaModel> lstLabelCriteria { get; set; }
    }
}