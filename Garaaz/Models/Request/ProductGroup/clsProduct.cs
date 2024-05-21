using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class clsProduct
    {
        public int? BrandId { get; set; }
        public int? VehicleId { get; set; }
        public int? VariantId { get; set; }
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Product Select GroupId")]
        public int? GroupId { get; set; }
        [Required(ErrorMessage = "Product Name Required")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "Please Enter Part Number")]
        public string PartNumber { get; set; }
        //[Required(ErrorMessage = "Please Enter Description")]
        public string Description { get; set; }
        public string Remark { get; set; }
        [Required(ErrorMessage = "Please Select Prduct Image")]
        public string ImagePath { get; set; }
        public decimal? Price { get; set; }
        public string ProductType { get; set; }
        public int? DistributorId { get; set; }
        public string TaxValue { get; set; }
        public int? PackQuantity { get; set; }
        public string RootPartNo { get; set; }
        public int? CurrentStock { get; set; }
    }

    public class ProductWithBreadcrumb
    {
        public clsProduct Product { get; set; }
        public List<string> lstBreadCrumb { get; set; }
    }
}