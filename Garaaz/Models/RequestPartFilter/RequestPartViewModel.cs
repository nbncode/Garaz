using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class RequestPartViewModel
    {
        public int Id { get; set; }
        public int DistributorId { get; set; }
        public string CarMake { get; set; }
        public string ModelNumber { get; set; }
        public string Year { get; set; }
        public string Modification { get; set; }
        public string PartGroup { get; set; }
        public string PartNumber { get; set; }
        public string RootPartNumber { get; set; }
        public string PartDescription { get; set; }
    }
    public class ClsPartfilterRequest
    {
        [Required]
        public string UserId { get; set; }
        public int DistributorId { get; set; }
        public string CarMake { get; set; }
        public string ModelNumber { get; set; }
        public string Year { get; set; }
        public string Modification { get; set; }
        public string PartNumber { get; set; }
        public int? TempOrderId { get; set; }
    }
    public class clsPartfilter
    {
        public int Id { get; set; }
        public int DistributorId { get; set; }
        [Required(ErrorMessage = "Car Make is required.")]
        public string CarMake { get; set; }
        [Required(ErrorMessage = "Model is required.")]
        public string ModelNumber { get; set; }
        [Required(ErrorMessage = "Year is required.")]
        public string Year { get; set; }
        [Required(ErrorMessage = "Modification is required.")]
        public string Modification { get; set; }
        [Required]
        public string PartGroup { get; set; }
        [Required]
        public string PartNumber { get; set; }
        [Required]
        public string RootPartNumber { get; set; }
        [Required]
        public string PartDescription { get; set; }
    }
    public class RequestPartFilterValue
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class PartRequestFilters
    {
        public List<RequestPartFilterValue> CarMake { get; set; }
        public List<RequestPartFilterValue> Model { get; set; }
        public List<RequestPartFilterValue> Year { get; set; }
        public List<RequestPartFilterValue> Modification { get; set; }
    }
    public class RequestNewPart : ClsPartfilterRequest
    {
        public string PartName { get; set; }
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}
    