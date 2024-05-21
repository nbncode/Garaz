using Garaaz.Controllers.Dashboard;
using System.Web.Mvc;

namespace Garaaz.Controllers
{
    [Authorize(Roles = "SuperAdmin,Distributor,DistributorUsers,RoIncharge,SalesExecutive")]
    public class DashboardController : BaseDbController
    {
        private const bool IsFromMobile = false;

        public DashboardController() : base(IsFromMobile)
        {
            
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}