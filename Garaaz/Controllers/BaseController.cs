using Garaaz.Models;
using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;

namespace Garaaz.Controllers
{
    public class BaseController : Controller
    {
        #region Import excel files

        protected void UploadOutlet(HttpPostedFileBase file, int distributorId)
        {
            var rc = new RepoCustomers();
            var path = rc.SaveExcelFile(file);
            var tuple = rc.FetchAndPopulateOutlets(path, distributorId, out var totalRecords, out var imported, out var skipped);

            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;

            SetImportTempMessage(isImported, totalRecords, imported, skipped, "outlets");
        }

        protected void UploadRoIncharge(HttpPostedFileBase file, int distributorId)
        {
            var rc = new RepoCustomers();
            var path = rc.SaveExcelFile(file);

            var tuple = rc.FetchAndPopulateRoIncharge(path, distributorId, out var totalRecords, out var imported, out var skipped);
            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;

            SetImportTempMessage(isImported, totalRecords, imported, skipped, "RO Incharge");
        }

        protected void UploadWorkshop(HttpPostedFileBase file, int distributorId)
        {
            var rc = new RepoCustomers();
            var path = rc.SaveExcelFile(file);

            var tuple = rc.FetchAndPopulateWorkshop(path, distributorId, out var totalRecords, out var imported, out var skipped,out string message);
            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;
            TempData["SaleMessage"] = message;

            SetImportTempMessage(isImported, totalRecords, imported, skipped, "workshops");
        }

        protected void UploadProduct(HttpPostedFileBase file, int distributorId)
        {
            var rc = new RepoCustomers();
            var path = rc.SaveExcelFile(file);
            var tuple = rc.FetchAndPopulateProducts(path, distributorId, out var totalRecords, out var imported, out var skipped);

            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;

            SetImportTempMessage(isImported, totalRecords, imported, skipped, "products");
        }

        protected void UploadDailyStock(HttpPostedFileBase file, int distributorId)
        {
            var rc = new RepoCustomers();
            var path = rc.SaveExcelFile(file);
            var tuple = rc.FetchAndPopulateDailyStock(path, distributorId, out var totalRecords, out var imported, out var skipped);

            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;
            SetImportTempMessage(isImported, totalRecords, imported, skipped, "daily stocks");
        }

        protected void UploadDailySale(HttpPostedFileBase file, int distributorId)
        {
            var repoCustomer = new RepoCustomers();
            var path = repoCustomer.SaveExcelFile(file);
            var tuple = repoCustomer.FetchAndPopulateDailySales(path, distributorId, out var totalRecords, out var imported, out var skipped, out var dataUploadMsg);

            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;

            // Set message for user
            var wasOrWere = imported > 1 ? "were" : "was";
            var skipWasOrWere = skipped > 1 ? "were" : "was";

            if (isImported)
            {
                TempData["success"] = skipped > 0 ? $"Successfully processed {totalRecords} records{dataUploadMsg}! {imported} {wasOrWere} imported and {skipped} {skipWasOrWere} skipped due to some issues." : $"Successfully processed {totalRecords} records{dataUploadMsg}! {imported} {wasOrWere} imported.";
            }
            else
            {
                TempData["error"] = skipped > 0 ? $"Failed to import daily sales. {skipped} {skipWasOrWere} skipped due to some issues." : "Failed to import daily sales.";
            }
        }

        protected void UploadCustomerBackOrder(HttpPostedFileBase file, int distributorId)
        {
            var repoCustomer = new RepoCustomers();
            var path = repoCustomer.SaveExcelFile(file);
            var tuple = repoCustomer.FetchAndPopulateCustomerBackOrder(path, distributorId, out var totalRecords, out var imported, out var skipped);

            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;
            SetImportTempMessage(isImported, totalRecords, imported, skipped, "customer back orders");
        }

        protected void UploadOutstanding(HttpPostedFileBase file, int distributorId)
        {
            var repoCustomer = new RepoCustomers();
            var path = repoCustomer.SaveExcelFile(file);
            var tuple = repoCustomer.FetchAndPopulateOutstanding(path, distributorId, out var totalRecords, out var imported, out var skipped);

            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;
            SetImportTempMessage(isImported, totalRecords, imported, skipped, "outstanding");
        }

        protected void UploadAccountLedger(HttpPostedFileBase file, int distributorId)
        {
            var rc = new RepoCustomers();
            var path = rc.SaveExcelFile(file);
            var tuple = rc.FetchAndPopulateAccountLedger(path, distributorId, out var totalRecords, out var imported, out var skipped);

            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;
            SetImportTempMessage(isImported, totalRecords, imported, skipped, "account ledgers");
        }

        protected void UploadSalesExecutive(HttpPostedFileBase file, int distributorId)
        {
            var rc = new RepoCustomers();
            var path = rc.SaveExcelFile(file);
            var tuple = rc.FetchAndPopulateSalesExecutives(path, distributorId, out var totalRecords, out var imported, out var skipped);

            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;

            SetImportTempMessage(isImported, totalRecords, imported, skipped, "sales executives");
        }

        protected void UploadRequestPartFilter(HttpPostedFileBase file, int distributorId)
        {
            var rc = new RepoCustomers();
            var path = rc.SaveExcelFile(file);

            var tuple = rc.FetchAndPopulateRequestPartFilter(path, distributorId, out var totalRecords, out var imported, out var skipped);
            var isImported = tuple.Item1;
            TempData["ImportStatusList"] = tuple.Item2;

            SetImportTempMessage(isImported, totalRecords, imported, skipped, "request part filter");
        }

        /// <summary>
        /// Set import message in TempData for success or error.
        /// </summary>
        /// <param name="isImported">Whether import succeed or failed.</param>
        /// <param name="totalRecords">Number of total records.</param>
        /// <param name="imported">Number of records that were imported.</param>
        /// <param name="skipped">Number of records that were skipped.</param>
        /// <param name="entity">The entity that was imported in plural form.</param>
        private void SetImportTempMessage(bool isImported, int totalRecords, int imported, int skipped, string entity)
        {
            var wasOrWere = imported > 1 ? "were" : "was";
            var skipWasOrWere = skipped > 1 ? "were" : "was";

            if (isImported)
            {
                TempData["success"] = skipped > 0 ? $"Successfully processed {totalRecords} records! {imported} {wasOrWere} imported and {skipped} {skipWasOrWere} skipped due to some issues." : $"Successfully processed {totalRecords} records! {imported} {wasOrWere} imported.";
            }
            else
            {
                TempData["error"] = skipped > 0 ? $"Failed to import {entity}. {skipped} {skipWasOrWere} skipped due to some issues." : $"Failed to import {entity}.";
            }
        }

        #endregion

        #region New Part Request

        public ActionResult NewPartRequest()
        {
            return View();
        }

        public ActionResult NewPartRequestPartialView()
        {
            var sys = new SystemController();
            var role = sys.GetUserRole();

            var repoNewPart = new RepoNewPart();
            return PartialView("_NewPartRequest", repoNewPart.GetNewPartRequests(role));
        }

        public JsonResult DeleteNewPartRequest(int id)
        {
            object data;

            try
            {
                var repoNewPart = new RepoNewPart();
                var requestDeleted = repoNewPart.DeleteNewPartRequest(id);

                data = new
                {
                    Message = requestDeleted ? "Part request deleted successfully!" : "Failed to delete part request.",
                    ResultFlag = requestDeleted ? 1 : 0
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                data = new
                {
                    Message = $"Failed to delete part request. Additional detail - { exc.Message}",
                    ResultFlag = 0
                };
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Approve Part Request

        public ActionResult ApprovePartRequest(int requestId)
        {
            TempData["RequestId"] = requestId;
            return View();
        }

        public ActionResult ApprovePartRequestPartialView()
        {
            PartRequestModel aprModel = null;
            try
            {
                var requestId = Convert.ToInt32(TempData["RequestId"]);

                var repoNewPart = new RepoNewPart();
                aprModel = repoNewPart.GetApprovePartRequest(requestId);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("_ApprovePartRequest", aprModel);
        }

        public JsonResult SearchPart(int requestId,string query)
        {
            object data;
            try
            {
                var repoNewPart = new RepoNewPart();
                var suggestions = repoNewPart.SearchPart(query, requestId);

                data = new
                {
                    query,
                    suggestions
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                data = new
                {
                    query,
                    suggestions = new[] { "" }
                };
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SavePartsForPartRequest(ApproveRequestParam arParam)
        {
            object data;
            try
            {
                var repoNewPart = new RepoNewPart();
                var saved = repoNewPart.SavePartsForPartRequest(arParam);

                data = new
                {
                    ResultFlag = saved ? 1 : 0,
                    Message = saved ? "Saved parts for current part request successfully!" : "Failed to save parts for current part request."
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                data = new
                {
                    ResultFlag = 0,
                    exc.Message
                };
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}