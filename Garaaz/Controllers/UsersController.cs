using System.Web.Mvc;

namespace Garaaz.Controllers
{
    [Authorize(Roles = "DistributorUsers")]
    public class UsersController : Controller
    {
        #region Dashboard
        // GET: Users
        public ActionResult Index()
        {
            return View();
        }
        #endregion        
    }
}