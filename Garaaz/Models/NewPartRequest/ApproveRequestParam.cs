using System.Collections.Generic;

namespace Garaaz.Models
{
    public class ApproveRequestParam
    {
        public int RequestId { get; set; }
        public string PartNumber { get; set; }
        public decimal Price { get; set; }
        public List<ApprovedPartRequest> Parts { get; set; }
    }
}