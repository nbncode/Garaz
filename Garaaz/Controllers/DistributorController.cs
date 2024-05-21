using Garaaz.Models;
using Garaaz.Models.Attributes;
using Garaaz.Models.DeleteData;
using Garaaz.Models.Notifications;
using Garaaz.Models.Schemes.SchemeLevel;
using Garaaz.Models.Users.DistributorUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Controllers
{
    [Authorize(Roles = "Distributor,DistributorUsers")]
    public class DistributorController : BaseController
    {
        #region Dashboard
        // GET: Distributor
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
        #endregion

        #region Import Outlet

        public ActionResult ImportOutlet()
        {
            ViewBag.Controller = "Distributor";

            var loginUserId = General.GetUserId();
            var general = new General();
            ViewBag.distributorId = general.GetDistributorId(loginUserId);

            return View(new DailySale());
        }

        [HttpPost]
        public ActionResult ImportOutlet(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Distributor";

            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportOutlet");
            }

            try
            {
                UploadOutlet(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportOutlet");
            }

            return RedirectToAction("ImportOutlet");
        }

        #endregion

        #region Import RO Incharge

        public ActionResult ImportRoIncharge()
        {
            ViewBag.Controller = "Distributor";

            var loginUserId = General.GetUserId();
            var general = new General();
            ViewBag.distributorId = general.GetDistributorId(loginUserId);

            return View(new DailySale());
        }

        [HttpPost]
        public ActionResult ImportRoIncharge(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Distributor";

            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportRoIncharge");
            }

            try
            {
                UploadRoIncharge(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportRoIncharge");
            }

            return RedirectToAction("ImportRoIncharge");
        }

        #endregion

        #region Import SalesExecutive

        public ActionResult ImportSalesExecutive()
        {
            ViewBag.Controller = "Distributor";
            var loginUserId = General.GetUserId();
            var general = new General();
            ViewBag.distributorId = general.GetDistributorId(loginUserId);
            return View(new DailySale());
        }

        [HttpPost]
        public ActionResult ImportSalesExecutive(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Distributor";

            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportSalesExecutive");
            }

            try
            {
                UploadSalesExecutive(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportSalesExecutive");
            }

            return RedirectToAction("ImportSalesExecutive");
        }

        #endregion

        #region Import WorkShop
        public ActionResult ImportWorkShop()
        {
            ViewBag.Controller = "Distributor";

            var loginUserId = General.GetUserId();
            var general = new General();
            ViewBag.distributorId = general.GetDistributorId(loginUserId);

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));
        }

        [HttpPost]
        public ActionResult ImportWorkShop(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Distributor";

            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportWorkShop");
            }

            try
            {
                UploadWorkshop(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportWorkShop");
            }

            return RedirectToAction("ImportWorkShop");
        }

        #endregion

        #region Import Products
        public ActionResult ImportProducts()
        {
            ViewBag.Controller = "Distributor";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));

        }
        [HttpPost]
        public ActionResult ImportProducts(HttpPostedFileBase file, int distributorId)
        {
            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportProducts");
            }

            try
            {
                UploadProduct(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportProducts");
            }

            return RedirectToAction("ImportProducts");
        }
        #endregion

        #region Import DailyStock
        public ActionResult ImportDailyStock()
        {
            ViewBag.Controller = "Distributor";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));
        }
        [HttpPost]
        public ActionResult ImportDailyStock(HttpPostedFileBase file, int distributorId)
        {
            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportDailyStock");
            }

            try
            {
                UploadDailyStock(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportDailyStock");
            }

            return RedirectToAction("ImportDailyStock");
        }

        #endregion

        #region Import DailySales
        public ActionResult ImportDailySalesTrackerWithInvoice()
        {
            ViewBag.Controller = "Distributor";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));
        }
        [HttpPost]
        public ActionResult ImportDailySalesTrackerWithInvoice(HttpPostedFileBase file, int distributorId)
        {
            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportDailySalesTrackerWithInvoice");
            }

            try
            {
                UploadDailySale(file, distributorId);
            }
            catch (AggregateException exc)
            {
                foreach (var innerExc in exc.InnerExceptions)
                {
                    TempData["error"] = innerExc.Message;
                    RepoUserLogs.LogException(innerExc);
                }
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            return RedirectToAction("ImportDailySalesTrackerWithInvoice");
        }

        #endregion

        #region Import Customer Back Order
        public ActionResult ImportCustomerBackOrder()
        {
            ViewBag.Controller = "Distributor";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));
        }

        [HttpPost]
        public ActionResult ImportCustomerBackOrder(HttpPostedFileBase file, int distributorId)
        {
            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportCustomerBackOrder");
            }

            try
            {
                UploadCustomerBackOrder(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportCustomerBackOrder");
            }

            return RedirectToAction("ImportCustomerBackOrder");
        }

        #endregion

        #region Import Outstanding
        public ActionResult ImportOutstanding()
        {
            ViewBag.Controller = "Distributor";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));
        }
        [HttpPost]
        public ActionResult ImportOutstanding(HttpPostedFileBase file, int distributorId)
        {
            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportOutstanding");
            }

            try
            {
                UploadOutstanding(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportOutstanding");
            }

            return RedirectToAction("ImportOutstanding");
        }
        #endregion

        #region Import Account ledger
        public ActionResult ImportAccountLedgers()
        {
            ViewBag.Controller = "Distributor";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));

        }
        [HttpPost]
        public ActionResult ImportAccountLedgers(HttpPostedFileBase file, int distributorId)
        {
            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportAccountLedgers");
            }

            try
            {
                UploadAccountLedger(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportAccountLedgers");
            }

            return RedirectToAction("ImportAccountLedgers");
        }
        #endregion

        #region Import Request Part Filter

        public ActionResult ImportRequestPartFilter()
        {
            ViewBag.Controller = "Distributor";
            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));
        }

        [HttpPost]
        public ActionResult ImportRequestPartFilter(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Distributor";

            if (file == null)
            {
                TempData["error"] = "Please provide a file";
                return RedirectToAction("ImportRequestPartFilter");
            }

            try
            {
                UploadRequestPartFilter(file, distributorId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("ImportRequestPartFilter");
            }

            return RedirectToAction("ImportRequestPartFilter");
        }

        #endregion

        #region Distributor Outlets
        [CustomAuthorize("Outlets")]
        public ActionResult DistributorOutlets()
        {
            ViewBag.Controller = "Distributor";

            string loginUserId = General.GetUserId();
            var general = new General();
            ViewBag.distributorId = general.GetDistributorId(loginUserId);
            return View();
        }
        public ActionResult DistributorOutletsPartialView(int distributorId)
        {
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;

            var ul = new RepoUserList();
            return PartialView(ul.GetDistributorOutlets(distributorId));
        }

        public ActionResult AddDistributorOutlets(string mode, string distributorId, int? id)
        {
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;
            //ViewBag.Locations = OutletModel.GetLocations(Convert.ToInt32(distributorId));

            SystemController systemController = new SystemController();
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetDistributorOutletById(new OutletModel()
                {
                    OutletId = Convert.ToInt32(id)
                });
                return View(data.Data);
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddDistributorOutlets(OutletModel model)
        {
            try
            {
                ViewBag.distributorId = model.DistributorId;
                ViewBag.Controller = "Distributor";

                var sc = new SystemController();
                var result = sc.OutletRegisterOrUpdate(model);

                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("DistributorOutlets", "Distributor", new { distributorId = model.DistributorId });
                }

                TempData["error"] = result.Message;
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            return View(model);
        }
        #endregion

        #region Distributor Users
        [CustomAuthorize("Users")]
        public ActionResult DistributorUsers(int? distributorId, int? outletId, int? workshopId)
        {
            ViewBag.Controller = "Distributor";

            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
            }
            else
            {
                // When distributor users link is click, the distributorId is null
                // So get the distributor id based on login id
                string loginUserId = General.GetUserId();
                RepoUsers db = new RepoUsers();
                ViewBag.distributorId = db.GetDistributorByUserId(loginUserId).DistributorId;
            }

            if (outletId != null && outletId > 0)
            {
                ViewBag.outletId = outletId;
            }

            if (workshopId != null && workshopId > 0)
            {
                ViewBag.workshopId = workshopId;
            }

            return View();
        }
        public ActionResult DistributorUsersPartialView(int? distributorId, int? outletId, int? workshopId)
        {
            ViewBag.Controller = "Distributor";
            RepoUserList db = new RepoUserList();

            if (outletId != null && outletId > 0)
            {
                ViewBag.outletId = outletId;
                return PartialView(db.GetOutletUsers(outletId.Value));
            }

            if (workshopId != null && workshopId > 0)
            {
                ViewBag.workshopId = workshopId;
                return PartialView(db.GetWorkshopUsers(workshopId.Value));
            }

            // This line of code should be in last
            // Else each time we will receive distributor users
            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
                return PartialView(db.GetDistributorUsers(distributorId.Value));
            }

            return PartialView();
        }

        public ActionResult AddDistributorUsers(string mode, int? distributorId, int? outletId, int? workshopId, string id)
        {
            ViewBag.Controller = "Distributor";
            SystemController systemController = new SystemController();

            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
            }

            if (outletId != null && outletId > 0)
            {
                ViewBag.outletId = outletId;
            }

            if (workshopId != null && workshopId > 0)
            {
                ViewBag.workshopId = workshopId;
            }

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                if (distributorId != null && distributorId > 0)
                {
                    var data = systemController.GetDistributorUserById(new clsDistributorUserInfo()
                    {
                        DistributorId = distributorId.Value,
                        UserId = id
                    });
                    return View(data.Data);
                }

                if (outletId != null && outletId > 0)
                {
                    var data = systemController.GetOutletsUserById(new clsDistributorUserInfo()
                    {
                        UserId = id
                    });
                    return View(data.Data);
                }

                if (workshopId != null && workshopId > 0)
                {
                    var data = systemController.GetWorkshopsUserById(new clsDistributorUserInfo()
                    {
                        UserId = id
                    });
                    return View(data.Data);
                }

                return View();

            }

            return View();
        }

        [HttpPost]
        public ActionResult AddDistributorUsers(clsDistributorUserInfo model)
        {
            try
            {
                SystemController systemController = new SystemController();
                ViewBag.distributorId = model.DistributorId;

                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                ResultModel result = null;
                if (model.Role == Constants.Distributor)
                {
                    result = systemController.DistributorRegisterOrUpdate(new clsDistributor()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserId = model.UserId,
                        IsFromDistributorUser = true

                    });
                }
                else
                {
                    if (model.OutletId > 0)
                    {
                        // Cast model to clsOutlet so as to update Outlet users
                        var clsOutlet = new clsOutlet()
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            PhoneNumber = model.PhoneNumber,
                            Email = model.Email,
                            UserId = model.UserId,
                            Address = model.Address,
                            Latitude = model.Latitude,
                            Longitude = model.Longitude,
                            OutletId = model.OutletId,
                            Password = model.Password,
                            EmployeeCode = model.EmployeeCode,
                            Designations = model.Designations
                        };
                        result = systemController.OutletUserRegisterOrUpdate(clsOutlet);
                    }
                    else if (model.WorkshopId > 0)
                    {
                        var clsWorkshop = new clsWorkshop()
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            PhoneNumber = model.PhoneNumber,
                            Email = model.Email,
                            UserId = model.UserId,
                            Address = model.Address,
                            Latitude = model.Latitude,
                            Longitude = model.Longitude,
                            WorkshopId = model.WorkshopId,
                            Password = model.Password,
                            EmployeeCode = model.EmployeeCode,
                            Designations = model.Designations
                        };
                        result = systemController.WorkshopUserRegisterOrUpdate(clsWorkshop);
                    }
                    else if (model.DistributorId > 0)
                    {
                        result = systemController.DistributorUsersRegisterOrUpdate(model);
                    }
                }

                if (result != null)
                {
                    if (result.ResultFlag == 1)
                    {
                        TempData["success"] = result.Message;
                        return RedirectToAction("DistributorUsers", "distributor", new { distributorId = model.DistributorId, outletId = model.OutletId, workshopId = model.WorkshopId });
                    }

                    TempData["error"] = result.Message;
                }

            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }

            return View(model);
        }
        #endregion

        #region Distributor Locations
        [CustomAuthorize("LocationCode")]
        public ActionResult DistributorLocations(int? distributorId)
        {
            ViewBag.Controller = "Distributor";
            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
            }
            else
            {
                string loginUserId = General.GetUserId();
                RepoUsers ru = new RepoUsers();
                ViewBag.distributorId = ru.GetDistributorByUserId(loginUserId).DistributorId;
            }

            return View();
        }

        public ActionResult DistributorLocationsPartialView(int distributorId)
        {
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;
            var ru = new RepoUsers();
            return PartialView(ru.GetDistributorLocations(distributorId));
        }

        public ActionResult AddDistributorLocations(string mode, int distributorId, int? locationId)
        {
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var ru = new RepoUsers();
                var data = ru.GetDistributorLocation(distributorId, Convert.ToInt32(locationId));
                var dl = new clsDistributorLocation
                {
                    LocationId = data.LocationId,
                    LocationCode = data.LocationCode,
                    Location = data.Location
                };
                return View(dl);
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddDistributorLocations(clsDistributorLocation model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please enter all required fields.";
                return View(model);
            }

            try
            {
                var ru = new RepoUsers();
                ViewBag.distributorId = model.DistributorId;

                if (model.LocationId > 0)
                {
                    var result = ru.UpdateDistributorLocation(model, model.LocationId);
                    if (result)
                    {
                        TempData["success"] = "Updated location for distributor successfully.";
                        return RedirectToAction("DistributorLocations", "Distributor", new { distributorId = model.DistributorId });
                    }

                    TempData["error"] = "Failed to update location for distributor.";
                }
                else
                {
                    var result = ru.SaveDistributorLocation(model);
                    if (result)
                    {
                        TempData["success"] = "Saved location for distributor successfully.";
                        return RedirectToAction("DistributorLocations", "Distributor", new { distributorId = model.DistributorId });

                    }

                    TempData["error"] = "Failed to save location for distributor.";
                }

            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteLocation(int locationId)
        {
            object data;
            try
            {
                var repoUsers = new RepoUsers();
                var result = repoUsers.DeleteDistributorLocation(locationId);
                if (result)
                {
                    data = new
                    {
                        Message = "Location deleted successfully!",
                        ResultFlag = 1
                    };
                }
                else
                {
                    data = new
                    {
                        Message = "Failed to delete location.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception exc)
            {
                data = new
                {
                    exc.Message,
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Sales Executive
        [CustomAuthorize("SalesExecutives")]
        public ActionResult SalesExecutive(int? distributorId)
        {
            ViewBag.Controller = "Distributor";
            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
            }
            else
            {
                string loginUserId = General.GetUserId();
                RepoUsers ru = new RepoUsers();
                ViewBag.distributorId = ru.GetDistributorByUserId(loginUserId).DistributorId;
            }

            return View();
        }

        public ActionResult SalesExecutivePartialView(int? distributorId)
        {
            ViewBag.Controller = "Distributor";
            var rul = new RepoUserList();
            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
                return PartialView(rul.GetSalesExecutiveUsers(distributorId.Value, Constants.SalesExecutive));
            }

            return PartialView();

        }

        public ActionResult AddSalesExecutive(string mode, int? distributorId, string id)
        {
            ViewBag.Controller = "Distributor";
            var db = new RepoUsers();

            var systemController = new SystemController();

            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
                ViewBag.RoIncharge = db.RoInchargeSelectList(distributorId.Value);
            }

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                if (distributorId != null && distributorId > 0)
                {
                    var data = systemController.GetDistributorUserByRoleAndId(new clsDistributorUserInfo()
                    {
                        DistributorId = distributorId.Value,
                        UserId = id,
                        Role = Constants.SalesExecutive
                    });
                    return View(data.Data);
                }

                return View();
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddSalesExecutive(clsDistributorUserInfo model)
        {
            try
            {
                var systemController = new SystemController();
                ViewBag.distributorId = model.DistributorId;

                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                ResultModel result = null;
                if (model.DistributorId > 0)
                {
                    result = systemController.DistributorUsersRegisterOrUpdate(model);
                }

                if (result != null && result.ResultFlag == 1)
                {
                    // Add or update RoIncharge for SalesExecutive
                    RepoUsers repoUsers = new RepoUsers();
                    repoUsers.SaveRoSalesExecutive(model.RoInchargeId, result.Data.ToString());

                    TempData["success"] = result.Message;
                    return RedirectToAction("SalesExecutive", "Distributor", new { distributorId = model.DistributorId });
                }

                var db = new RepoUsers();
                ViewBag.RoIncharge = db.RoInchargeSelectList(model.DistributorId);
                TempData["error"] = result?.Message;
            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }

            return View(model);
        }

        #endregion

        #region RO Incharge
        [CustomAuthorize("ROIncharge")]
        public ActionResult RoIncharge(int? distributorId)
        {
            ViewBag.Controller = "Distributor";
            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
            }
            else
            {
                string loginUserId = General.GetUserId();
                RepoUsers ru = new RepoUsers();
                ViewBag.distributorId = ru.GetDistributorByUserId(loginUserId).DistributorId;
            }

            return View();
        }

        public ActionResult RoInchargePartialView(int? distributorId)
        {
            ViewBag.Controller = "Distributor";
            var db = new RepoUserList();

            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
                return PartialView(db.GetRoInchargeUsers(distributorId.Value, Constants.RoIncharge));
            }

            return PartialView();

        }

        public ActionResult AddRoIncharge(string mode, int? distributorId, string id)
        {
            ViewBag.Controller = "Distributor";
            var systemController = new SystemController();

            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;

                var ro = new RepoOutlet();
                ViewBag.Outlets = ro.OutletsSelectList(distributorId.Value, id);
            }

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                if (distributorId != null && distributorId > 0)
                {
                    var data = systemController.GetDistributorUserByRoleAndId(new clsDistributorUserInfo()
                    {
                        DistributorId = distributorId.Value,
                        UserId = id,
                        Role = Constants.RoIncharge
                    });

                    // Get outlet Id as per distributor Id and user Id                    
                    var distUserInfo = data.Data as clsDistributorUserInfo;
                    var ro = new RepoOutlet();
                    distUserInfo.OutletId = ro.GetOutletId(id, distributorId.Value);

                    return View(distUserInfo);
                }

                return View();

            }

            return View();
        }

        [HttpPost]
        public ActionResult AddRoIncharge(clsDistributorUserInfo model)
        {
            try
            {
                ViewBag.distributorId = model.DistributorId;

                var ro = new RepoOutlet();
                ViewBag.Outlets = ro.OutletsSelectList(model.DistributorId, model.UserId);

                if (model.OutletId == 0)
                {
                    TempData["error"] = "Please select outlet.";
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                ResultModel result = null;
                if (model.DistributorId > 0)
                {
                    var systemController = new SystemController();
                    result = systemController.DistributorUsersRegisterOrUpdate(model);

                    if (result != null && result.ResultFlag == 1 && result.Data is string userId && model.OutletId > 0)
                    {
                        var isNew = ro.SaveUserForOutlet(userId, model.DistributorId, model.OutletId);
                        if (isNew)
                        {
                            ro.RegisterWalkInCustomer(model);
                        }
                    }
                }

                if (result != null && result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("RoIncharge", "Distributor", new { distributorId = model.DistributorId });
                }

                TempData["error"] = result?.Message;
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            return View(model);
        }
        #endregion

        #region Workshop
        [CustomAuthorize("Workshop")]
        public ActionResult Workshop()
        {
            ViewBag.Controller = "Distributor";
            string loginUserId = General.GetUserId();
            RepoUsers db = new RepoUsers();
            ViewBag.distributorId = db.GetDistributorByUserId(loginUserId).DistributorId;
            return View();
        }
        public ActionResult WorkshopPartialView(int distributorId)
        {
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;
            RepoUserList db = new RepoUserList();
            return PartialView(db.GetWorkshop(distributorId));
        }

        public ActionResult AddWorkshop(string mode, int distributorId, string id)
        {
            SystemController systemController = new SystemController();
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;
            ViewBag.Outlets = clsWorkshop.GetDistributorOutlets(distributorId);
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetWorkshopUserById(new clsWorkshop()
                {
                    DistributorId = distributorId,
                    UserId = id
                });
                return View(data.Data);
            }

            return View();
        }
        public ActionResult AddWorkShops(int distributorId)
        {
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;
            ViewBag.Outlets = clsWorkshop.GetDistributorOutlets(distributorId);
            return View();
        }
        [HttpPost]
        public ActionResult AddWorkshop(clsWorkshop model)
        {
            try
            {
                SystemController systemController = new SystemController();
                ViewBag.distributorId = model.DistributorId;
                ViewBag.Outlets = clsWorkshop.GetDistributorOutlets(model.DistributorId);
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                dynamic result;
                if (model.Role == Constants.Distributor)
                {
                    result = systemController.DistributorRegisterOrUpdate(new clsDistributor()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserId = model.UserId,
                        IsFromDistributorUser = true

                    });
                }
                else
                {
                    result = systemController.WorkshopRegisterOrUpdate(model);
                }

                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("Workshop", "distributor", new { distributorId = model.DistributorId });
                }

                TempData["error"] = result.Message;
            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }

            return View(model);
        }
        #endregion

        #region User Features
        [CustomAuthorize("Users")]
        public ActionResult AddUserFeatures(string userId)
        {
            ViewBag.userId = userId;

            var ru = new RepoUsers();
            var roles = ru.GetRolesByUserId(userId);

            SystemController systemController = new SystemController();
            ViewBag.FeatureList = systemController.GetAllFeatures(roles).Data;
            var data = systemController.GetAllUserFeatures(new RequestUserFeature()
            {
                UserId = userId
            }).Data;
            return View(data);
        }
        #endregion

        #region Features
        public ActionResult Feature()
        {
            return View();
        }

        public ActionResult FeaturePartialView()
        {
            SystemController systemController = new SystemController();
            return PartialView(systemController.GetFeatureList());
        }

        public ActionResult AddFeatures(int FeatureId, string mode)
        {
            SystemController systemController = new SystemController();
            clsFeatures clsFeatures = new clsFeatures();
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetFeaturesNamebyUserId(FeatureId);
                return View(data.Data);
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddFeatures(clsFeatures model)
        {
            try

            {
                if (!string.IsNullOrEmpty(model.FeatureName))
                {
                    SystemController systemController = new SystemController();
                    var result = systemController.SaveFeatures(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["success"] = result.Message;
                        return RedirectToAction("Feature", "Admin");
                    }

                    TempData["error"] = result.Message;
                }
            }

            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }

            return View(model);
        }
        #endregion

        #region Schemes
        [CustomAuthorize("Schemes")]
        public ActionResult Schemes()
        {
            return View();
        }

        public ActionResult GenerateCouponsFromFile(string filePath, int schemeId)
        {
            object data;
            try
            {
                var rs = new RepoCustomers();
                rs.FetchAndPopulateCoupons(filePath, schemeId);

                TempData["success"] = "Coupons saved successfully for Scheme!";
                data = new
                {
                    Message = "Coupons saved successfully for Scheme!",
                    ResultFlag = 1
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    Message = $"Failed to save coupons. Additional details - { exc.Message}",
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Show Sales Tracker with Invoice
        [CustomAuthorize("ShowSales")]
        public ActionResult ShowSalesTrackerWithInvoice()
        {
            RepoUsers repoUsers = new RepoUsers();
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = General.GetCurrentDistributorId();
            ViewBag.workshopList = repoUsers.GetWorkshopByDistId(ViewBag.distributorId); // repoUsers.getUsers();
            return View();
        }

        public ActionResult ShowSalesTrackerWithInvoicePartialView(int distributorId, string startDate, string endDate, string fromAmt, string toAmt, string workshopId)
        {
            ViewBag.Controller = "Distributor";
            var repoCustomer = new RepoCustomers();
            return PartialView("ShowSalesPartialView", null);
            //return PartialView(repoCustomer.GetDailySales(distributorId, startDate, endDate, fromAmt, toAmt, workshopId));
        }
        #endregion

        #region Orders and their status
        [CustomAuthorize("CurrentOrder")]
        public ActionResult AllOrders()
        {
            ViewBag.Controller = "Distributor";
            int distributorId = General.GetCurrentDistributorId();
            var ro = new RepoOrder();
            return View(ro.GetallOrders("distributor", distributorId));
        }

        public ActionResult UpdateOrderStatus(List<OrderTable> model)
        {
            object data;

            try
            {
                var ro = new RepoOrder();
                var updated = ro.UpdateOrderStatus(model);

                if (updated)
                {
                    data = new
                    {
                        Message = "Status of the orders updated successfully!",
                        ResultFlag = 1
                    };
                    TempData["success"] = "Status of the orders updated successfully!";
                }
                else
                {
                    data = new
                    {
                        Message = "Status of the orders not updated.",
                        ResultFlag = 0
                    };
                    TempData["error"] = "Status of the orders not updated.";
                }

            }
            catch (Exception exc)
            {
                data = new
                {
                    exc.Message,
                    ResultFlag = 0
                };
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region MgaCatalouge Banner
        [CustomAuthorize("MgaBanner")]
        public ActionResult MgaBanner()
        {
            ViewBag.Controller = "Distributor";
            return View();
        }

        public ActionResult MgaBannerPartialView()
        {
            ViewBag.Controller = "Distributor";
            int DistributorId = General.GetCurrentDistributorId();
            RepoMgaCatalougeBanner cls = new RepoMgaCatalougeBanner();
            return PartialView(cls.GetDistributorBanner(DistributorId));
        }

        public ActionResult MgaAddBanner(int Id, string mode)
        {
            int DistributorId = General.GetCurrentDistributorId();
            ViewBag.Controller = "Distributor";
            var repoCustomer = new RepoCustomers();
            ViewBag.Distributors = repoCustomer.GetDailySales(DistributorId).Distributors.Items;
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                RepoMgaCatalougeBanner cls = new RepoMgaCatalougeBanner();
                var data = cls.GetBannerById(Id);
                return View(data);
            }
            else
            {
                MgaBanner model = new MgaBanner();
                model.DistributorId = DistributorId;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult MgaAddBanner(MgaBanner model)
        {
            ViewBag.Controller = "Distributor";
            var repoCustomer = new RepoCustomers();
            ViewBag.Distributors = repoCustomer.GetDailySales(General.GetCurrentDistributorId()).Distributors.Items;
            if (string.IsNullOrEmpty(model.ImagePath))
            {
                ModelState.AddModelError("ImagePath", "Please Upload Image.");
                return View(model);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    RepoMgaCatalougeBanner cls = new RepoMgaCatalougeBanner();
                    var result = cls.SaveBanner(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["Success"] = result.Message;
                        return RedirectToAction("MgaBanner", "Distributor");
                    }

                    TempData["error"] = result.Message;
                }

                else
                {
                    return View(model);
                }

            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }
            return View(model);
        }
        public ActionResult MgaDeleteBanner(int Id, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "delete")
            {
                RepoMgaCatalougeBanner cls = new RepoMgaCatalougeBanner();
                var result = cls.DeleteBannerById(Id);
                if (result.ResultFlag == 1)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction("MgaBanner", "Distributor");
                }

                TempData["error"] = result.Message;
            }
            return RedirectToAction("MgaBanner", "Distributor");
        }
        #endregion

        #region MgaCatalouge Product
        [CustomAuthorize("MgaBanner")]
        public ActionResult MgaProduct(int bannerId, int distributoId)
        {
            ViewBag.Controller = "Distributor";
            ViewBag.BannerId = bannerId;
            ViewBag.DistributorId = distributoId;
            RepoMgaCatalougeProduct cls = new RepoMgaCatalougeProduct();
            var data = cls.GetBannerProducts(distributoId, bannerId);
            return View(data);
        }

        [HttpPost]
        public ActionResult SaveMgaProduct(int BannerId, string productId)
        {
            object data;
            try
            {
                string[] Productstr = !string.IsNullOrEmpty(productId) ? productId.Split(',') : null;

                var lstproducts = new List<MgaCatalougeProduct>();
                if (Productstr != null)
                {
                    foreach (var ProductId in Productstr)
                    {
                        lstproducts.Add(new MgaCatalougeProduct
                        {
                            BannerId = BannerId,
                            ProductId = Convert.ToInt32(ProductId),
                            CreateDate = DateTime.Now
                        });
                    }
                }
                RepoMgaCatalougeProduct cls = new RepoMgaCatalougeProduct();
                cls.SaveProducts(BannerId, lstproducts);
                data = new
                {
                    Message = "Products saved for Banner successfully.",
                    ResultFlag = 1
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    exc.Message,
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Sales Executive's workshops
        [CustomAuthorize("Workshop")]
        public ActionResult AddSeWorkshops(int distributorId, string userId)
        {
            var ru = new RepoUsers();
            ViewBag.Controller = "Distributor";
            ViewBag.UserId = userId;
            ViewBag.DistributorId = distributorId;
            //ViewBag.WorkshopList = ru.GetWorkshopByDistId(distributorId);
            ViewBag.WorkshopList = ru.GetSalesExecutiveWorkshopBySalesExecutiveId(userId);
            var salesExecutive = ru.GetUserDetailByUserId(userId);
            var cuw = new clsAddUserWorkshop
            {
                SalesExecutiveName = salesExecutive != null ? salesExecutive.FirstName + " " + salesExecutive.LastName : "",
                UserId = userId,
                WorkshopIds = new List<int>()
            };
            foreach (var wid in ru.GetWorkshopIdsByUserId(userId))
            {
                cuw.WorkshopIds.Add(wid);
            }

            return View(cuw);
        }

        [HttpPost]
        public ActionResult SaveSeWorkshops(string userId, string workshopIds)
        {
            object data;

            try
            {
                ViewBag.Controller = "Distributor";
                string[] workshopIdArr = null;
                if (!string.IsNullOrEmpty(workshopIds))
                    workshopIdArr = workshopIds.Split(',');

                var lstSeWorkshop = new List<SalesExecutiveWorkshop>();
                if (workshopIdArr != null)
                {
                    foreach (var wid in workshopIdArr)
                    {
                        lstSeWorkshop.Add(new SalesExecutiveWorkshop()
                        {
                            UserId = userId,
                            WorkshopId = Convert.ToInt32(wid)
                        });
                    }
                }

                var ru = new RepoUsers();
                bool status = ru.SaveWorkshopsForSalesExecutive(lstSeWorkshop, userId);

                data = new
                {
                    Message = status ? "Workshops saved for Sales Executive successfully." : "Failed to save workshops.",
                    ResultFlag = status ? 1 : 0
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    exc.Message,
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Set Sales Executives
        public ActionResult SetSalesExecutive(int distributorId, string roUserId)
        {
            // Get SE users as per dist id            
            var ru = new RepoUsers();
            ViewBag.Controller = "Distributor";
            ViewBag.SeUsers = ru.GetDistributorUsers(distributorId, Constants.SalesExecutive);
            ViewBag.RoUserId = roUserId;

            // Get existing sales executive by ROI user id
            var roSe = new ClsRoSalesExecutive
            {
                RoUserId = roUserId,
                SeUserIds = new List<string>()
            };
            var seUserIds = ru.GetRoSalesExecutives(roUserId);
            roSe.SeUserIds.AddRange(seUserIds);

            return View(roSe);
        }

        [HttpPost]
        public ActionResult SaveSalesExecutives(string roUserId, string seUserIds)
        {
            object data;

            try
            {
                ViewBag.Controller = "Distributor";
                string[] seUserIdArr = !string.IsNullOrEmpty(seUserIds) ? seUserIds.Split(',') : null;

                var lstRoSe = new List<RoSalesExecutive>();
                if (seUserIdArr != null)
                {
                    foreach (var seId in seUserIdArr)
                    {
                        lstRoSe.Add(new RoSalesExecutive
                        {
                            RoUserId = roUserId,
                            SeUserId = seId
                        });
                    }
                }

                var ru = new RepoUsers();
                ru.SaveSalesExecutives(roUserId, lstRoSe);

                data = new
                {
                    Message = "Sales Executives saved for RO Incharge successfully.",
                    ResultFlag = 1
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    exc.Message,
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Product
        [CustomAuthorize("Product")]
        public ActionResult Product()
        {
            ViewBag.DistributorId = General.GetCurrentDistributorId();
            ViewBag.Controller = "Distributor";
            return View();
        }

        public ActionResult ProductPartialView()
        {
            ViewBag.Controller = "Distributor";
            int DistributorId = General.GetCurrentDistributorId();
            SystemController systemController = new SystemController();
            return PartialView(systemController.GetProductByDistributorId(DistributorId).Data);
        }

        public ActionResult AddProduct(int ProductId, string mode)
        {
            List<OutletListResponse> lst = new List<OutletListResponse>();
            SystemController systemController = new SystemController();
            var distributorId = General.GetCurrentDistributorId();
            ViewBag.ddlBrands = new SelectList(systemController.DistributorBrands(distributorId), "BrandId", "Name");
            ViewBag.ddlGroups = new SelectList(systemController.ComboGroupsDistributor(), "GroupId", "GroupName");
            ViewBag.Controller = "Distributor";
            var repoCustomer = new RepoCustomers();
            ViewBag.Distributors = repoCustomer.GetDailySales(distributorId).Distributors.Items;

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetProductNameById(ProductId);
                var d = (clsProduct)data.Data;

                RepoOutlet repoOutlet = new RepoOutlet();
                lst = repoOutlet.GetOutlets(d.DistributorId.Value);
                var lstDily = repoOutlet.GetDailyStockQty(ProductId);
                ViewBag.lstOutlet = lst;
                ViewBag.lstDaily = lstDily;
                return View(data.Data);
            }

            ViewBag.lstOutlet = lst;
            clsProduct model = new clsProduct();
            model.DistributorId = distributorId;
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(clsProduct model, FormCollection frm)
        {
            SystemController systemController = new SystemController();
            var DisGroups = systemController.ddlProductGroupsDistributor().Count;
            if (model.GroupId <= 0 && DisGroups > 1)
            {
                RepoOutlet repoOutlet = new RepoOutlet();
                var distributoId = General.GetCurrentDistributorId();
                var lst = repoOutlet.GetOutlets(distributoId);
                var lstDily = repoOutlet.GetDailyStockQty(model.ProductId);
                ViewBag.lstOutlet = lst;
                ViewBag.lstDaily = lstDily;
                ViewBag.ddlBrands = new SelectList(systemController.ddlBrands(), "BrandId", "Name");
                ViewBag.ddlGroups = new SelectList(systemController.ComboGroupsDistributor(), "GroupId", "GroupName");
                ViewBag.Controller = "Distributor";
                var repoCustomer = new RepoCustomers();
                ViewBag.Distributors = repoCustomer.GetDailySales(distributoId).Distributors.Items;
                ModelState.AddModelError("GroupId", "Plese select group");
                model.DistributorId = distributoId;
                return View(model);
            }

            try
            {
                if (!string.IsNullOrEmpty(model.ProductName))
                {
                    RepoOutlet repoOutlet = new RepoOutlet();
                    var distributoId = General.GetCurrentDistributorId();
                    var lstOutlet = repoOutlet.GetOutlets(distributoId);
                    //records
                    List<DailyStock> lstDailyStocks = new List<DailyStock>();
                    int currentStock = 0;
                    foreach (var item in lstOutlet)
                    {
                        if (frm["outlet_" + item.OutletId] != null && frm["outlet_" + item.OutletId] != "")
                        {
                            DailyStock obj = new DailyStock();
                            obj.OutletId = item.OutletId;
                            obj.DistributorId = distributoId;
                            obj.PartNum = model.PartNumber;
                            obj.Mrp = model.Price.ToString();
                            obj.StockQuantity = frm["txt_" + item.OutletId] != null && frm["txt_" + item.OutletId] != "" ? frm["txt_" + item.OutletId] : "";
                            int.TryParse(obj.StockQuantity, out int sQty);
                            if (sQty > 0)
                            {
                                currentStock = currentStock + Convert.ToInt32(obj.StockQuantity);
                            }
                            lstDailyStocks.Add(obj);
                        }
                    }
                    model.CurrentStock = currentStock;

                    var result = systemController.SaveProduct(model);
                    if (result.ResultFlag == 1)
                    {
                        repoOutlet.SaveDailyStock(lstDailyStocks, distributoId, model.PartNumber);
                        TempData["Success"] = result.Message;
                        return RedirectToAction("Product", "Distributor");
                    }

                    TempData["error"] = result.Message;

                }
            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);

                RepoOutlet repoOutlet = new RepoOutlet();
                var distributoId = General.GetCurrentDistributorId();
                var lst = repoOutlet.GetOutlets(distributoId);
                var lstDily = repoOutlet.GetDailyStockQty(model.ProductId);
                ViewBag.lstOutlet = lst;
                ViewBag.lstDaily = lstDily;
                ViewBag.ddlBrands = new SelectList(systemController.ddlBrands(), "BrandId", "Name");
                ViewBag.ddlGroups = new SelectList(systemController.ComboGroupsDistributor(), "GroupId", "GroupName");
                ViewBag.Controller = "Distributor";
                var repoCustomer = new RepoCustomers();
                ViewBag.Distributors = repoCustomer.GetDailySales(distributoId).Distributors.Items;
            }
            return View(model);
        }
        #endregion

        #region Product Group
        [CustomAuthorize("ProductGroup")]
        public ActionResult ProductGroup()
        {
            ViewBag.Controller = "Distributor";
            return View();
        }

        public ActionResult ProductGroupPartialView()
        {
            ViewBag.Controller = "Distributor";
            SystemController systemController = new SystemController();
            return PartialView(systemController.GetProductListGroupByDistributorId().Data);
        }

        public ActionResult AddProductGroup(int GroupId, string mode)
        {
            ViewBag.Controller = "Distributor";
            var repoCustomer = new RepoCustomers();
            ViewBag.Distributors = repoCustomer.GetDailySales(General.GetCurrentDistributorId()).Distributors.Items;
            var sc = new SystemController();
            ViewBag.ddlBrands = new SelectList(sc.ddlBrands(), "BrandId", "Name");
            ViewBag.ddlGroups = sc.ddlProductGroupsDistributor();
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = sc.GetProductGroupNameById(GroupId);
                return View(data.Data);
            }

            clsProductGroup model = new clsProductGroup();
            model.DistributorId = General.GetCurrentDistributorId();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProductGroup(clsProductGroup model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.GroupName))
                {
                    var systemController = new SystemController();
                    var result = systemController.SaveProductGroup(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["Success"] = result.Message;
                        return RedirectToAction("ProductGroup", "Distributor");
                    }

                    TempData["error"] = result.Message;
                }
            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }
            return View(model);
        }
        #endregion

        #region Scheme workshop
        public ActionResult SchemeWorkshop(int schemeId)
        {
            ViewBag.SchemeId = schemeId;
            return View();
        }

        public ActionResult SchemeWorkshopPartialView(int schemeId)
        {
            try
            {
                ViewBag.SchemeId = schemeId;
                var rs = new RepoSchemeCashback();
                return PartialView(rs.GetSchemeWorkshops(schemeId));
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }
            return PartialView(null);
        }
        #endregion

        #region Show workshop levels
        [CustomAuthorize("Schemes")]
        public ActionResult ShowWorkshopLevels(int schemeId, int workshopId)
        {
            try
            {
                var rsd = new RepoSchemesDescription();
                return PartialView("_WorkshopLevels", rsd.GetWorkshopLevels(schemeId, workshopId));
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }
            return PartialView("_WorkshopLevels", null);
        }
        #endregion

        #region Allocate, Save & Get Coupons
        [CustomAuthorize("Schemes")]
        public ActionResult AllocateCoupon(int schemeId)
        {
            ViewBag.SchemeId = schemeId;
            return View();
        }
        public ActionResult AllocateCouponPartialView(int schemeId)
        {
            var rs = new RepoSchemes();
            return PartialView(rs.GetCouponSchemeAndWorkshops(schemeId));
        }

        [HttpPost]
        public ActionResult SaveCoupons(List<SchemeWorkshop> schemeWorkshops)
        {
            object data;

            try
            {
                var rs = new RepoSchemes();
                rs.GenerateAndSaveCoupons(schemeWorkshops);

                data = new
                {
                    Message = "Coupons saved successfully for Scheme!",
                    ResultFlag = 1
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    Message = $"Failed to save coupons.Additional details - { exc.Message}",
                    ResultFlag = 1
                };
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCoupons(SchemeWorkshop model)
        {
            var rs = new RepoSchemes();
            return PartialView("_WorkshopCoupons", rs.GetCoupons(model.SchemeId, model.WorkshopId));
        }

        public ActionResult GetWorkshopByCoupon(SchemeWorkshop model)
        {
            var rs = new RepoSchemes();
            return PartialView("_GetWorkshopWithCoupons", rs.GetWorkshopByCoupon(model.SchemeId, model.CouponNumber));
        }

        #endregion

        #region Assign Gift & Get Winners
        public ActionResult AssignGift(int schemeId)
        {
            ViewBag.SchemeId = schemeId;
            return View();
        }
        public ActionResult AssignGiftPartialView(int schemeId)
        {
            var rs = new RepoSchemes();
            return PartialView(rs.GetCategoryGifts(schemeId));
        }

        public ActionResult GetWinners(int giftId)
        {
            var giftName = "Gift";

            var rs = new RepoSchemes();
            var winners = rs.GetWinners(giftId, ref giftName);

            ViewBag.GiftName = giftName;
            return PartialView("_GetWinners", winners);
        }

        #endregion

        #region Set and Save coupon for Gift
        public ActionResult SetCouponForGift(int schemeId, int giftId)
        {
            ViewBag.Url = $"{Request?.Url?.GetLeftPart(UriPartial.Authority)}/Distributor/AssignGift?schemeId={schemeId}";

            var rs = new RepoSchemes();
            return View(rs.GetCouponsModel(schemeId, giftId));
        }

        [HttpPost]
        public ActionResult SaveCouponForGift(SchemeGift model)
        {

            object data;
            try
            {
                var rs = new RepoSchemes();
                var couponSaved = rs.SaveCouponForGift(model.GiftId, model.CouponNumber, out string workshopName);

                if (couponSaved)
                {
                    var nu = new NotificationUtil();
                    nu.SaveNotification(model.CouponNumber, model.SchemeName, model.GiftName);
                }

                data = new
                {
                    Message = couponSaved ? $"Coupon '{model.CouponNumber}' saved for gift successfully!" : $"Failed to save coupon '{model.CouponNumber}' for gift.",
                    ResultFlag = couponSaved ? 1 : 0,
                    Data = couponSaved ? workshopName : null
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    exc.Message,
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Wallet
        public ActionResult Wallet()
        {
            return View();
        }
        public ActionResult WalletPartialView(string couponNumber = null)
        {
            var distributorId = General.GetCurrentDistributorId();
            var rw = new SystemController();
            //RepoWallet repoWallet = new RepoWallet();
            var data = rw.GetWsWallet(new WalletRequest()
            {
                PageNumber = -1,
                CouponNumber = couponNumber,
                DistributorId = distributorId
            }).Data;
            return PartialView(data);
        }
        #endregion

        #region Best Seller
        public ActionResult BestSeller()
        {
            var loginUserId = General.GetUserId();
            var general = new General();
            var distributorId = general.GetDistributorId(loginUserId);

            var rbs = new RepoBestSeller(distributorId);
            return View(rbs);
        }

        [HttpPost]
        public ActionResult SaveBestSeller(int[] groupIds, int distributorId)
        {
            object data;
            try
            {
                var bestSellers = new List<BestSeller>();
                if (groupIds != null)
                {
                    bestSellers.AddRange(groupIds.Select(groupId =>
                        new BestSeller { GroupId = groupId, DistributorId = distributorId }
                    ));
                }

                var rbs = new RepoBestSeller(distributorId);
                var status = rbs.SaveBestSellers(bestSellers, distributorId);
                data = new
                {
                    Message = status ? "Best sellers saved successfully!" : "Failed to save best sellers.",
                    ResultFlag = status ? 1 : 0
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    exc.Message,
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Distributor Growth Percentage
        public ActionResult GrowthPercentage()
        {
            ViewBag.Controller = "Distributor";
            GrowthPercentageModel model = new GrowthPercentageModel();
            model.DistributorId = General.GetCurrentDistributorId();
            var cls = new ClsGrowthPercentage();
            var data = cls.GetGrowthPercentage(model);
            if (data != null)
            {
                model.GrowthPercentage = Convert.ToDecimal(data.GrowthPercentage1);
                model.MinValue = Convert.ToDecimal(data.MinValue);
            }

            var repousers = new RepoUsers();
            ViewBag.Distributor = repousers.GetAllDistributorsNew();


            return View(model);
        }
        [HttpPost]
        public ActionResult GrowthPercentage(GrowthPercentageModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Please enter all required fields.";
                    return View(model);
                }

                var cls = new ClsGrowthPercentage();

                var isPopulated = cls.SaveGrowthPercentage(model);

                if (isPopulated)
                    TempData["success"] = "Distributor growth percentage save successfully.";
                else
                    TempData["error"] = "Failed to save distributor growth percentage.";
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("GrowthPercentage");
            }

            return RedirectToAction("GrowthPercentage");
        }

        #endregion

        #region Banner
        public ActionResult Banner()
        {
            ViewBag.Controller = "Distributor";
            return View();
        }

        public ActionResult BannerPartialView()
        {
            var loginUserId = General.GetUserId();
            var general = new General();
            var distributorId = general.GetDistributorId(loginUserId);
            RepoBanner Banner = new RepoBanner();
            ViewBag.Controller = "Distributor";
            return PartialView(Banner.GetAllBanner(distributorId));
        }

        public ActionResult AddBanner(int BannerId, string mode)
        {
            ViewBag.Controller = "Distributor";
            var repousers = new RepoUsers();
            ViewBag.Distributors = repousers.GetAllDistributorsNew();

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {

                RepoBanner cls = new RepoBanner();
                var data = cls.GetBannerById(BannerId);
                return View(data);
            }

            ResponseBannerMobile model = new ResponseBannerMobile();
            var loginUserId = General.GetUserId();
            var general = new General();
            model.DistributorId = general.GetDistributorId(loginUserId);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddBanner(ResponseBannerMobile model)
        {
            try
            {
                ViewBag.Controller = "Distributor";
                var repousers = new RepoUsers();
                ViewBag.Distributors = repousers.GetAllDistributorsNew();

                if (string.IsNullOrEmpty(model.BannerImage))
                {

                    ModelState.AddModelError("BannerImage", "Please Upload Image.");
                    return View(model);
                }
                if (ModelState.IsValid)
                {

                    RepoBanner cls = new RepoBanner();
                    var result = cls.SaveBanner(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["Success"] = result.Message;
                        return RedirectToAction("Banner", "Distributor");
                    }

                    TempData["error"] = result.Message;
                }

                else
                {
                    return View(model);
                }

            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }
            return View(model);
        }
        public ActionResult DeleteBanner(int BannerId, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "delete")
            {

                RepoBanner cls = new RepoBanner();
                var result = cls.DeleteBannerById(BannerId);
                if (result.ResultFlag == 1)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction("Banner", "Distributor");
                }

                TempData["error"] = result.Message;
            }
            return RedirectToAction("Banner", "Distributor");
        }
        #endregion

        #region Request Part Filter
        public ActionResult RequestPartFilter()
        {
            ViewBag.Controller = "Distributor";
            string loginUserId = General.GetUserId();
            RepoUsers db = new RepoUsers();
            ViewBag.distributorId = db.GetDistributorByUserId(loginUserId).DistributorId;
            return View();
        }

        public ActionResult RequestPartFilterPartialView(int distributorId)
        {
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;
            var model = new ClsPartfilterRequest();
            model.DistributorId = distributorId;
            RepoRequestPartFilter db = new RepoRequestPartFilter();
            return PartialView(db.GetAllRequestPartFilter(model));
        }

        public ActionResult AddRequestPartFilter(string mode, int distributorId, int? id)
        {
            RepoRequestPartFilter db = new RepoRequestPartFilter();
            ViewBag.Controller = "Distributor";
            ViewBag.distributorId = distributorId;
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = db.GetRequestPartFilterById(id.Value);
                return View(data);
            }
            var model = new clsPartfilter() { DistributorId = distributorId };
            return View(model);
        }

        [HttpPost]
        public ActionResult AddRequestPartFilter(clsPartfilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Controller = "Distributor";
                    return View(model);
                }
                RepoRequestPartFilter db = new RepoRequestPartFilter();
                var result = db.AddOrUpdateRequestPartFilter(model);
                if (result)
                {
                    TempData["Success"] = "Record added successfully.";
                    return RedirectToAction("RequestPartFilter", "Distributor");
                }

                TempData["error"] = "Failled to add record."; ;
            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }
            return View(model);
        }
        #endregion

        #region Customer back orders
        public ActionResult CustomerBackOrders()
        {
            var loginUserId = General.GetUserId();
            var general = new General();
            var distributorId = general.GetDistributorId(loginUserId);

            var cbo = new RepoCBO(distributorId);
            return View(cbo);
        }
        #endregion

        #region Delete Accounts data

        public ActionResult DeleteAccountData()
        {
            ViewBag.Controller = "Distributor";
            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(General.GetCurrentDistributorId()));
        }

        [HttpPost]
        public ActionResult DeleteAccountData(DailySale model)
        {
            ViewBag.Controller = "Distributor";

            if (!ModelState.IsValid)
            {
                ViewBag.Controller = "Distributor";
                return View(model);
            }
            try
            {
                RepoDataDelete repoDelete = new RepoDataDelete();
                var isdelete = repoDelete.DistributorAccountsDelete(model);
                if (isdelete)
                {
                    TempData["Success"] = $"Account data deleted!!";
                    return RedirectToAction("DeleteAccountData");
                }

                TempData["error"] = "The distributor does not have any accounts data";
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
                return RedirectToAction("DeleteAccountData");
            }

            return RedirectToAction("DeleteAccountData");
        }

        #endregion
    }
}