using System.Collections.Generic;
using System.Web.Mvc;

namespace Garaaz.Models.Notifications
{
    public class ClsNewWorkshop
    {
        public string RefUserId { get; set; }
        public int WorkshopId { get; set; }
        public int[] DistributorId { get; set; }
        public IEnumerable<SelectListItem> Distributors { get; set; }
        public UserDetail UserDetail { get; set; }
    }
}