using Garaaz.Models.DashboardOverview;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Controllers.Dashboard
{
    public class MobileDashboardController : BaseDbController
    {
        private const bool IsFromMobile = true;

        public MobileDashboardController() : base(IsFromMobile)
        {

        }

        public ActionResult Index(string userId, string role)
        {
            var allowedRoles = new[] { "SuperAdmin", "Distributor", "DistributorUsers", "RoIncharge", "SalesExecutive" };
            if (!allowedRoles.Contains(role))
            {
                throw new Exception("You are not authorized to access the Dashboard.");
            }

            DbCommon.SetDashboardCookies(userId, role);
            return View("~/Views/Dashboard/Index.cshtml");
        }
    }
}