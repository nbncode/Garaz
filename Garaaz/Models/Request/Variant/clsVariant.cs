using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class clsVariant
    {
        public int VariantId { get; set; }
        [Required(ErrorMessage = "Please select parent id")]
        public int ParentId { get; set; }
        [Required(ErrorMessage = "Please select vehicle id")]
        public int VehicleId { get; set; }
        public int BrandId { get; set; }
        [Required(ErrorMessage = "Variant name required")]
        public string VariantName { get; set; }
        [Required(ErrorMessage = "Production year is required")]
        public string ProductionYear { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        public string Region { get; set; }
        public string Engine { get; set; }
        public string ChassisType { get; set; }
        public string VNo { get; set; }

    }

    public class VariantWithBreadcrumb
    {
        public List<clsVariant> lstVarient { get; set; }
        public List<string> lstBreadCrumb { get; set; }
    }

}