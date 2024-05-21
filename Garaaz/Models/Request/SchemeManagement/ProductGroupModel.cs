using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ProductGroupModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
    public class ProductGroupComboList
    {
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
    }
}