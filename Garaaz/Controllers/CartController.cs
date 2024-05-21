using System;
using System.Linq;
using System.Web.Mvc;
using Garaaz.Models;

namespace Garaaz.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult IsProductExist(string partNumber)
        {
            object data = null;
            try
            {
                garaazEntities db = new garaazEntities();                
                var prodExist = db.Products.Any(p => p.PartNo == partNumber);

                if (prodExist)
                {
                    data = new
                    {
                        ResultFlag = 1,
                        Message = "Product exists"
                    };
                }
                else
                {
                    data = new
                    {
                        ResultFlag = 0,
                        Message = "Product doesn't exist"
                    };
                }
            }
            catch (Exception exc)
            {
                data = new
                {
                    ResultFlag = 0,
                    Message = exc.Message
                };
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}