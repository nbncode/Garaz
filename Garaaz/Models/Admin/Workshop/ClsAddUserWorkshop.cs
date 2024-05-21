using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class clsAddUserWorkshop
    {
        public string UserId { get; set; }
        public List<int> WorkshopIds { get; set; }
        public string SalesExecutiveName { get; set; }
    }
    public class SalesExeWorkshop
    {
        public int WorkshopId { get; set; }
        public string WorkShopCode { get; set; }
        public string WorkShopName { get; set; }
    }
}