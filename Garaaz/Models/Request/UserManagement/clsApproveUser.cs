using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class clsApproveUser
    {
        public string UserId { get; set; }
        public bool Approved { get; set; }
        public bool Locked { get; set; }
        public string NewPassword { get; set; }
        public string Code { get; set; }
    }
}