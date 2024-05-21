using Products.Garaaz.Models;
using System.Web.Mvc;

namespace Products.Garaaz.Controllers
{
    public class SearchProductController : Controller
    {
        // GET: SearchProduct
        public ActionResult Index(string q)
        {
            var rp = new RepoProducts();
            ViewBag.ProductId = rp.GetProductId(q, out var price);
            ViewBag.Price = price;
            return View();
        }
    }
}