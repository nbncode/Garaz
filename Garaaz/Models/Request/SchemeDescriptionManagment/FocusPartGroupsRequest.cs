using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class FocusPartGroupsRequest
    {
        public int SchemeId { get; set; }
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string GroupsSearchText { get; set; }
        public int? TempOrderId { get; set; }
    }
}