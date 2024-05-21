using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;

namespace Garaaz.Models
{
    public class clsProductGroup
    {
        public int? BrandId { get; set; }
        public int VehicleId { get; set; }
        public int VariantId { get; set; }
        public int GroupId { get; set; }
        [Required(ErrorMessage = "Product Name Required")]
        public string GroupName { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public string ImagePath { get; set; }
        public List<clsProductGroup> childs { get; set; }
        public int ChildCount { get; set; }
        public int? DistributorId { get; set; }
    }

    public class ProductGroupWithBreadcrumb
    {
        public List<clsProductGroup> lstProductGroup { get; set; }
        public List<string> lstBreadCrumb { get; set; }
    }

    public class PartCategory 
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}