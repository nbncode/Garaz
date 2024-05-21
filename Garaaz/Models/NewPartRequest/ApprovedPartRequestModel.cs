namespace Garaaz.Models
{
    public class ApprovedPartRequestModel
    {
        public int RequestId { get; set; }
        public int ApprovedPartRequestId { get; set; }
        public string PartNumber { get; set; }
        public string ApproverId { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsOriparts { get; set; }
        public int ProductId { get; set; }
        public int? CartQty { get; set; }
        public string CartAvailabilityType { get; set; }
        public int? CartOutletId { get; set; }
        public int Stock { get; set; }
    }
}