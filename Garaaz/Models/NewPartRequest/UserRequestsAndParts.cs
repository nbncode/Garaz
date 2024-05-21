using System.Collections.Generic;

namespace Garaaz.Models
{
    public class UserRequestsAndParts
    {
        public List<UserRequest> Requests { get; set; }
        public List<UserRequestAndParts> RequestAndParts { get; set; }

        public UserRequestsAndParts()
        {
            Requests = new List<UserRequest>();
            RequestAndParts = new List<UserRequestAndParts>();
        }
    }
}