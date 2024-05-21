using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ClsRoSalesExecutive
    {
        public string SeUserId { get; set; }
        public string SeUserName { get; set; }

        public string RoUserId { get; set; }
        /// <summary>
        /// Get or set the sales executive user ids.
        /// </summary>
        public List<string> SeUserIds { get; set; }
    }
}