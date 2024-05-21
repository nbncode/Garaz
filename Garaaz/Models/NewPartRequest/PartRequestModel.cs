using System.Collections.Generic;

namespace Garaaz.Models
{
    public class PartRequestModel : BaseRequestModel
    {
        public int RequestId { get; set; }
        public List<ApprovedPartRequest> ApprovedPartRequests { get; set; }

        public PartRequestModel()
        {
            ApprovedPartRequests = new List<ApprovedPartRequest>();
        }
    }
}