using System.Collections.Generic;

namespace Garaaz.Models
{
    public class UserRequestAndParts
    {
        public string Request{ get; set; }
        public int RequestId { get; set; }
        public List<ApprovedPartRequestModel> ApprovedPartRequests { get; set; }

        public UserRequestAndParts()
        {
            ApprovedPartRequests = new List<ApprovedPartRequestModel>();
        }
    }
}