using Garaaz.Models;
using Garaaz.Models.DeleteData;
using Garaaz.Models.Notifications;
using Garaaz.Models.Schemes.SchemeLevel;
using Garaaz.Models.Users.DistributorUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Constants = Garaaz.Models.Constants;

namespace Garaaz.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : BaseController
    {
        #region Dashboard
        // GET: admin
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
        #endregion

        #region Users
        public ActionResult Users()
        {
            return View();
        }
        public ActionResult UsersPartialView()
        {
            RepoUsers db = new RepoUsers();
            return PartialView(db.getUsers());
        }

        public ActionResult AddUser(string mode, string id)
        {
            SystemController systemController = new SystemController();
            var roles = systemController.GetRoles();
            ViewBag.Roles = roles.Data;
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetUserById(new clsRegister()
                {
                    UserId = id
                });
                return View(data.Data);
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddUser(clsRegister model)
        {
            try
            {
                SystemController systemController = new SystemController();
                var roles = systemController.GetRoles();
                ViewBag.Roles = roles.Data;

                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.Username = "123123";
                    model.Password = "123123";
                }

                var result = systemController.RegisterOrUpdateUser(model);

                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("Users", "Admin");
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

        #region Distributor
        public ActionResult Distributor()
        {
            return View();
        }
        public ActionResult DistributorPartialView()
        {
            RepoUsers db = new RepoUsers();
            return PartialView(db.getDistributor());
        }

        public ActionResult AddDistributor(string mode, string id)
        {
            SystemController systemController = new SystemController();
            ViewBag.AllBrands = systemController.GetAllBrand(new clsBrand()
            {
                UserId = null,
                Role = null
            }).Data;
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetUserById(new clsRegister()
                {
                    UserId = id
                });
                return View(data.Data);
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddDistributor(clsDistributor model)
        {
            try
            {
                SystemController systemController = new SystemController();

                //model.Role = Constansts.Distributor;
                //model.IsFromDistributorPage = true;

                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }
                model.IsApproved = true;
                var result = systemController.DistributorRegisterOrUpdate(model);

                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("Distributor", "Admin");
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

        #region Distributor Outlets
        public ActionResult DistributorOutlets(int distributorId)
        {
            ViewBag.Controller = "Admin";
            ViewBag.distributorId = distributorId;
            return View();
        }
        public ActionResult DistributorOutletsPartialView(int distributorId)
        {
            ViewBag.Controller = "Admin";
            ViewBag.distributorId = distributorId;

            var ul = new RepoUserList();
            return PartialView(ul.GetDistributorOutlets(distributorId));
        }

        public ActionResult AddDistributorOutlets(string mode, int distributorId, int? id)
        {
            ViewBag.distributorId = distributorId;
            ViewBag.Controller = "Admin";
            ViewBag.Locations = OutletModel.GetLocations(distributorId);

            var sc = new SystemController();
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = sc.GetDistributorOutletById(new OutletModel()
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
                ViewBag.Controller = "Admin";
                ViewBag.distributorId = model.DistributorId;
                ViewBag.Locations = OutletModel.GetLocations(model.DistributorId);
                var sc = new SystemController();
                var result = sc.OutletRegisterOrUpdate(model);
                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("DistributorOutlets", "Admin", new { distributorId = model.DistributorId });
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

        public ActionResult DistributorUsers(int? distributorId, int? outletId, int? workshopId)
        {
            ViewBag.Controller = "admin";
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

            return View();
        }
        public ActionResult DistributorUsersPartialView(int? distributorId, int? outletId, int? workshopId)
        {
            ViewBag.Controller = "admin";
            RepoUserList db = new RepoUserList();

            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
                return PartialView(db.GetDistributorUsers(distributorId.Value));
            }

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

            return PartialView();

        }
        public ActionResult AddDistributorUsers(string mode, int? distributorId, int? outletId, int? workshopId, string id)
        {
            ViewBag.Controller = "admin";
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
                        EmployeeCode = model.EmployeeCode,
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
                            Designations = model.Designations,
                            EmployeeCode = model.EmployeeCode
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

                if (result != null && result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("DistributorUsers", "Admin", new { distributorId = model.DistributorId, outletId = model.OutletId, workshopId = model.WorkshopId });
                }

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

        #region Distributor Locations

        public ActionResult DistributorLocations(int distributorId)
        {
            ViewBag.Controller = "admin";
            ViewBag.distributorId = distributorId;
            return View();
        }

        public ActionResult DistributorLocationsPartialView(int distributorId)
        {
            ViewBag.Controller = "admin";
            ViewBag.distributorId = distributorId;
            var ru = new RepoUsers();
            return PartialView(ru.GetDistributorLocations(distributorId));
        }

        public ActionResult AddDistributorLocations(string mode, int distributorId, int? locationId)
        {
            ViewBag.Controller = "admin";
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
                        return RedirectToAction("DistributorLocations", "Admin", new { distributorId = model.DistributorId });
                    }

                    TempData["error"] = "Failed to update location for distributor.";
                }
                else
                {
                    var result = ru.SaveDistributorLocation(model);
                    if (result)
                    {
                        TempData["success"] = "Saved location for distributor successfully.";
                        return RedirectToAction("DistributorLocations", "Admin", new { distributorId = model.DistributorId });

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
        public ActionResult SalesExecutive(int? distributorId)
        {
            ViewBag.Controller = "admin";
            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
            }

            return View();
        }

        public ActionResult SalesExecutivePartialView(int? distributorId)
        {
            ViewBag.Controller = "admin";
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
            ViewBag.Controller = "admin";
            var systemController = new SystemController();

            var ru = new RepoUsers();
            if (distributorId != null && distributorId > 0)
            {
                ViewBag.RoIncharge = ru.RoInchargeSelectList(distributorId.Value);
                ViewBag.distributorId = distributorId;
                ViewBag.WorkshopList = ru.GetWorkshopByDistId(distributorId.Value);
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
                var ru = new RepoUsers();
                ViewBag.RoIncharge = ru.RoInchargeSelectList(model.DistributorId);
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                ResultModel result = null;
                if (model.DistributorId > 0)
                {
                    ViewBag.WorkshopList = ru.GetWorkshopByDistId(model.DistributorId);
                    ViewBag.distributorId = model.DistributorId;

                    var sc = new SystemController();
                    result = sc.DistributorUsersRegisterOrUpdate(model);
                }

                if (result?.ResultFlag == 1)
                {
                    // Add or update RoIncharge for SalesExecutive
                    ru.SaveRoSalesExecutive(model.RoInchargeId, result.Data.ToString());
                    TempData["success"] = result.Message;
                    return RedirectToAction("SalesExecutive", "Admin", new { distributorId = model.DistributorId });
                }

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
        public ActionResult RoIncharge(int? distributorId)
        {
            ViewBag.Controller = "admin";
            if (distributorId != null && distributorId > 0)
            {
                ViewBag.distributorId = distributorId;
            }

            return View();
        }

        public ActionResult RoInchargePartialView(int? distributorId)
        {
            ViewBag.Controller = "admin";
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
            ViewBag.Controller = "admin";

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
                    var sc = new SystemController();
                    var data = sc.GetDistributorUserByRoleAndId(new clsDistributorUserInfo()
                    {
                        DistributorId = distributorId.Value,
                        UserId = id,
                        Role = Constants.RoIncharge
                    });

                    // Get outlet Id as per distributor Id and user Id                    
                    var distUserInfo = data.Data as clsDistributorUserInfo;
                    if (distUserInfo != null)
                    {
                        var ro = new RepoOutlet();
                        distUserInfo.OutletId = ro.GetOutletId(id, distributorId.Value);
                    }

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

                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                ResultModel result = null;
                if (model.DistributorId > 0)
                {
                    var sc = new SystemController();
                    result = sc.DistributorUsersRegisterOrUpdate(model);

                    if (result?.ResultFlag == 1 && result.Data is string userId && model.OutletId > 0)
                    {
                        var isNew=ro.SaveUserForOutlet(userId, model.DistributorId, model.OutletId);
                        if(isNew)
                        {
                            ro.RegisterWalkInCustomer(model);
                        }
                    }
                }

                if (result?.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("RoIncharge", "Admin", new { distributorId = model.DistributorId });
                }

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

        #region Workshop
        public ActionResult Workshop(int distributorId)
        {
            ViewBag.Controller = "admin";
            ViewBag.distributorId = distributorId;
            return View();
        }
        public ActionResult WorkshopPartialView(int distributorId)
        {
            ViewBag.Controller = "admin";
            ViewBag.distributorId = distributorId;
            RepoUserList db = new RepoUserList();
            return PartialView(db.GetWorkshop(distributorId));
        }

        public ActionResult AddWorkshop(string mode, int distributorId, string id)
        {
            SystemController systemController = new SystemController();
            ViewBag.Controller = "admin";
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
            ViewBag.Controller = "admin";
            ViewBag.distributorId = distributorId;
            ViewBag.Outlets = clsWorkshop.GetDistributorOutlets(distributorId);
            return View();
        }

        [HttpPost]
        public ActionResult AddWorkshop(clsWorkshop model)
        {
            try
            {
                ViewBag.distributorId = model.DistributorId;
                ViewBag.Controller = "admin";
                ViewBag.Outlets = clsWorkshop.GetDistributorOutlets(model.DistributorId);
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                SystemController systemController = new SystemController();
                dynamic result;
                if (model.Role == Constants.Distributor)
                {
                    result = systemController.DistributorRegisterOrUpdate(new clsDistributor()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserId = model.UserId,
                        IsFromDistributorUser = true,
                        City = model.City,
                        State = model.State,
                        Pincode = model.Pincode,
                        Gender = model.Gender,
                        LandlineNumber = model.LandlineNumber,
                        Gstin = model.Gstin,
                        EmployeeCode = model.EmployeeCode,
                        IsApproved = true
                    });
                }
                else
                {
                    model.IsApproved = true;
                    result = systemController.WorkshopRegisterOrUpdate(model);
                }

                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("Workshop", "Admin", new { distributorId = model.DistributorId });
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

        [HttpPost]
        public ActionResult getDistributorsList(clsDistributorsWorkShop model)
        {
            SystemController controller = new SystemController();
            RepoUsers db = new RepoUsers();
            ViewBag.Distributors = controller.GetDistributorsListByWorkShop(model).Data;
            ViewBag.WorkShopId = model.WorkShopId;
            return PartialView(db.getDistributor());
        }
        [HttpPost]
        public ActionResult saveDistributorsList(clsDistributorsWorkShop model)
        {
            SystemController controller = new SystemController();
            return Json(controller.SaveDistributorsListByWorkShop(model), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Import Outlet

        public ActionResult ImportOutlet()
        {
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
        }

        [HttpPost]
        public ActionResult ImportOutlet(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Admin";

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
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
        }

        [HttpPost]
        public ActionResult ImportRoIncharge(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Admin";

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
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
        }

        [HttpPost]
        public ActionResult ImportSalesExecutive(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Admin";

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
            ViewBag.Controller = "Admin";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
        }

        [HttpPost]
        public ActionResult ImportWorkShop(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Admin";

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
            ViewBag.Controller = "Admin";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));

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
            ViewBag.Controller = "Admin";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
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
            ViewBag.Controller = "Admin";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));

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
                return RedirectToAction("ImportDailySalesTrackerWithInvoice");
            }

            return RedirectToAction("ImportDailySalesTrackerWithInvoice");
        }

        #endregion

        #region Import Customer Back Order
        public ActionResult ImportCustomerBackOrder()
        {
            ViewBag.Controller = "Admin";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
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
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
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
            ViewBag.Controller = "Admin";

            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));

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
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
        }

        [HttpPost]
        public ActionResult ImportRequestPartFilter(HttpPostedFileBase file, int distributorId)
        {
            ViewBag.Controller = "Admin";

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

        #region Schemes
        public ActionResult Schemes()
        {
            return View();
        }
        #endregion

        #region User Features
        public ActionResult AddUserFeatures(string userId)
        {
            ViewBag.userId = userId;
            SystemController systemController = new SystemController();

            var ru = new RepoUsers();
            var roles = ru.GetRolesByUserId(userId);

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

            return View(clsFeatures);
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

        #region Feature Roles
        public ActionResult FeatureRole(int FeatureId)
        {
            ViewBag.FeatureId = FeatureId;
            RepoFeaturesRole cls = new RepoFeaturesRole();
            var data = cls.GetFeaturesRoles(FeatureId);
            return View(data);
        }
        [HttpPost]
        public ActionResult SaveRole(int FeatureId, string RoleId)
        {
            object data;
            try
            {
                string[] Rolestr = !string.IsNullOrEmpty(RoleId) ? RoleId.Split(',') : null;

                var lstFeaturerole = new List<FeaturesRole>();
                if (Rolestr != null)
                {
                    foreach (var Roleid in Rolestr)
                    {
                        lstFeaturerole.Add(new FeaturesRole
                        {
                            FeatureId = FeatureId,
                            RoleId = Roleid
                        });
                    }
                }
                RepoFeaturesRole cls = new RepoFeaturesRole();
                bool roleSaved = cls.SaveFeatures(FeatureId, lstFeaturerole);
                data = new
                {
                    Message = roleSaved ? "Roles saved for this feature successfully!" : "Failed to save roles for this feature.",
                    ResultFlag = roleSaved ? 1 : 0
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

        #region Product Group
        public ActionResult ProductGroup()
        {
            ViewBag.Controller = "Admin";
            return View();
        }

        public ActionResult ProductGroupPartialView()
        {
            ViewBag.Controller = "Admin";
            SystemController systemController = new SystemController();
            return PartialView(systemController.GetProductGroup().Data);
        }

        public ActionResult AddProductGroup(int GroupId, string mode)
        {
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            ViewBag.Distributors = repoCustomer.GetDailySales(0).Distributors.Items;
            var sc = new SystemController();
            ViewBag.ddlBrands = new SelectList(sc.ddlBrands(), "BrandId", "Name");
            ViewBag.ddlGroups = sc.ddlProductGroups();
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = sc.GetProductGroupNameById(GroupId);
                return View(data.Data);
            }

            return View();
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
                        return RedirectToAction("ProductGroup", "Admin");
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

        #region Product
        public ActionResult Product()
        {
            ViewBag.Controller = "Admin";
            ViewBag.DistributorId = 0;
            return View();
        }

        public ActionResult ProductPartialView()
        {
            ViewBag.Controller = "Admin";
            SystemController systemController = new SystemController();
            return PartialView(systemController.GetProduct().Data);
        }

        public ActionResult AddProduct(int ProductId, string mode)
        {
            List<OutletListResponse> lst = new List<OutletListResponse>();
            SystemController systemController = new SystemController();
            ViewBag.ddlBrands = new SelectList(systemController.ddlBrands(), "BrandId", "Name");
            ViewBag.ddlGroups = new SelectList(systemController.ComboProductGroup(), "GroupId", "GroupName");
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            ViewBag.Distributors = repoCustomer.GetDailySales(0).Distributors.Items;

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetProductNameById(ProductId);
                var d = (clsProduct)data.Data;

                RepoOutlet repoOutlet = new RepoOutlet();
                if (d?.DistributorId != null)
                {
                    lst = repoOutlet.GetOutlets(d.DistributorId.Value);
                    ViewBag.lstOutlet = lst;
                }

                var lstDily = repoOutlet.GetDailyStockQty(ProductId);
                ViewBag.lstDaily = lstDily;
                return View(data.Data);
            }

            ViewBag.lstOutlet = lst;
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(clsProduct model, FormCollection frm)
        {
            if (model.GroupId <= 0)
            {
                RepoOutlet repoOutlet = new RepoOutlet();

                if (model.DistributorId != null)
                {
                    var lst = repoOutlet.GetOutlets(model.DistributorId.Value);
                    ViewBag.lstOutlet = lst;
                }

                var lstDily = repoOutlet.GetDailyStockQty(model.ProductId);
                ViewBag.lstDaily = lstDily;
                SystemController systemController = new SystemController();
                ViewBag.ddlBrands = new SelectList(systemController.ddlBrands(), "BrandId", "Name");
                ViewBag.ddlGroups = new SelectList(systemController.ComboProductGroup(), "GroupId", "GroupName");
                ViewBag.Controller = "Admin";
                var repoCustomer = new RepoCustomers();
                ViewBag.Distributors = repoCustomer.GetDailySales(0).Distributors.Items;
                ModelState.AddModelError("GroupId", "Plese select group");
                return View(model);
            }

            try
            {
                if (!string.IsNullOrEmpty(model.ProductName))
                {
                    RepoOutlet repoOutlet = new RepoOutlet();
                    var distributoId = model.DistributorId.Value;
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
                    SystemController systemController = new SystemController();
                    var result = systemController.SaveProduct(model);
                    if (result.ResultFlag == 1)
                    {
                        repoOutlet.SaveDailyStock(lstDailyStocks, distributoId, model.PartNumber);
                        TempData["Success"] = result.Message;
                        return RedirectToAction("Product", "Admin");
                    }

                    TempData["error"] = result.Message;
                }
            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);

                List<OutletListResponse> lst = new List<OutletListResponse>();
                RepoOutlet repoOutlet = new RepoOutlet();
                lst = repoOutlet.GetOutlets(model.DistributorId.Value);
                var lstDily = repoOutlet.GetDailyStockQty(model.ProductId);
                ViewBag.lstOutlet = lst;
                ViewBag.lstDaily = lstDily;
                SystemController systemController = new SystemController();
                ViewBag.ddlBrands = new SelectList(systemController.ddlBrands(), "BrandId", "Name");
                ViewBag.ddlGroups = new SelectList(systemController.ComboProductGroup(), "GroupId", "GroupName");
                ViewBag.Controller = "Admin";
                var repoCustomer = new RepoCustomers();
                ViewBag.Distributors = repoCustomer.GetDailySales(0).Distributors.Items;
            }
            return View(model);
        }
        #endregion

        #region Notifications - New Overview

        public ActionResult NewDistributorOverview(int nId, string refUserId)
        {
            UserDetailModel udm = null;

            // Mark notification as read            
            var nu = new NotificationUtil();
            nu.MarkNotificationRead(nId, General.GetUserId());

            if (!string.IsNullOrEmpty(refUserId))
            {
                RepoUsers repoUsers = new RepoUsers();
                var userDetail = repoUsers.getUserById(refUserId);
                var distributor = repoUsers.GetDistributorByUserId(refUserId);

                udm = new UserDetailModel
                {
                    UserDetailId = userDetail.UserDetailId,
                    ConsPartyCode = userDetail.ConsPartyCode,
                    FirstName = userDetail.FirstName,
                    LastName = userDetail.LastName,
                    Address = userDetail.Address,
                    AspNetUser = userDetail.AspNetUser,
                    DistributorName = distributor != null ? distributor.DistributorName : "-"
                };
            }

            return View(udm);
        }

        public ActionResult NewWorkshopOverview(int nId, string refUserId, int workshopId)
        {
            // Mark notification as read            
            var nu = new NotificationUtil();
            nu.MarkNotificationRead(nId, General.GetUserId());

            return View(nu.GetWorkshopModel(refUserId, workshopId));
        }

        #endregion

        #region Vehicle
        public ActionResult Vehicle()
        {
            return View();
        }

        public ActionResult VehiclePartialView()
        {
            // Create model to pass
            var systemController = new SystemController();
            return PartialView(systemController.GetVehicle().Data);
        }

        public ActionResult AddVehicle(int VehicleId, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var systemController = new SystemController();
                var data = systemController.GetVehicleNameById(VehicleId);

                ViewBag.ddlBrand = new SelectList(systemController.ddlBrands(), "BrandId", "Name");

                return View(data.Data);
            }
            else
            {
                var systemController = new SystemController();
                ViewBag.ddlBrand = new SelectList(systemController.ddlBrands(), "BrandId", "Name");
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddVehicle(clsVehicle model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.VehicleName))
                {
                    var systemController = new SystemController();
                    var result = systemController.SaveVehicle(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["Success"] = result.Message;
                        return RedirectToAction("Vehicle", "Admin");
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

        #region Brand
        public ActionResult Brand()
        {
            return View();
        }

        public ActionResult BrandPartialView()
        {
            SystemController systemController = new SystemController();
            return PartialView(systemController.GetAllBrand(new clsBrand()).Data);
        }

        public ActionResult AddBrand(int BrandId, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {

                SystemController systemController = new SystemController();
                var data = systemController.GetBrandNameById(BrandId);


                return View(data.Data);
            }
            else
            {
                SystemController systemController = new SystemController();

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddBrand(clsBrand model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.Name))
                {
                    SystemController systemController = new SystemController();
                    var result = systemController.SaveBrand(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["Success"] = result.Message;
                        return RedirectToAction("Brand", "Admin");
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

        #region Variant
        public ActionResult Variant()
        {
            return View();
        }

        public ActionResult VariantPartialView()
        {
            // Create model to pass
            var systemController = new SystemController();
            return PartialView(systemController.GetVariant().Data);
        }

        public ActionResult AddVariant(int VariantId, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var systemController = new SystemController();
                var data = systemController.GetVariantNameById(VariantId);

                ViewBag.ddlBrand = new SelectList(systemController.ddlBrands(), "BrandId", "Name");
                ViewBag.ddlVehicle = new SelectList(systemController.ddlVehicles(), "VehicleId", "Name");
                ViewBag.ddlVParent = new SelectList(systemController.ddlVariants(), "VariantId", "Name");

                return View(data.Data);
            }
            else
            {
                var systemController = new SystemController();
                ViewBag.ddlBrand = new SelectList(systemController.ddlBrands(), "BrandId", "Name");
                //ViewBag.ddlVehicle = new SelectList(systemController.ddlVariantVehicleName(), "VehicleId", "Name");
                ViewBag.ddlVParent = new SelectList(systemController.ddlVariants(), "VariantId", "Name");
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddVariant(clsVariant model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.VariantName))
                {
                    var systemController = new SystemController();
                    var result = systemController.SaveVariant(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["Success"] = result.Message;
                        return RedirectToAction("Variant", "Admin");
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

        #region User's workshops
        public ActionResult AddUserWorkshops(int distributorId, string userId)
        {
            var ru = new RepoUsers();

            ViewBag.UserId = userId;
            ViewBag.DistributorId = distributorId;
            ViewBag.WorkshopList = ru.GetWorkshopByDistId(distributorId);

            var cuw = new clsAddUserWorkshop
            {
                UserId = userId,
                WorkshopIds = new List<int>()
            };
            var listUserWorkshop = ru.GetUserWorkshop(userId);
            foreach (var uw in listUserWorkshop)
            {
                cuw.WorkshopIds.Add(uw.WorkshopId);
            }

            return View(cuw);
        }

        public ActionResult SaveUserWorkshops(int distributorId, string userId, string workshopIds)
        {
            object data = null;

            try
            {
                string[] workshopIdArr = null;
                if (!string.IsNullOrEmpty(workshopIds))
                    workshopIdArr = workshopIds.Split(',');

                var lstUserWorkshop = new List<UserWorkshop>();
                if (workshopIdArr != null)
                {
                    foreach (var wid in workshopIdArr)
                    {
                        lstUserWorkshop.Add(new UserWorkshop()
                        {
                            DistributorId = distributorId,
                            UserId = userId,
                            WorkshopId = Convert.ToInt32(wid)
                        });
                    }
                }

                var ru = new RepoUsers();
                bool status = ru.SaveUserWorkshop(lstUserWorkshop, userId);

                data = new
                {
                    Message = "Workshops saved for user successfully.",
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

        #region Show Sales Tracker with Invoice
        public ActionResult ShowSalesTrackerWithInvoice()
        {
            RepoUsers repoUsers = new RepoUsers();
            ViewBag.Controller = "admin";
            ViewBag.distributorId = 0; //General.getCurrentDistributorId();
            ViewBag.workshopList = repoUsers.WorkshopList();
            return View();
        }

        public ActionResult ShowSalesTrackerWithInvoicePartialView(int distributorId, string startDate, string endDate, string fromAmt, string toAmt, string workshopId)
        {
            ViewBag.Controller = "admin";
            var repoCustomer = new RepoCustomers();
            return PartialView("ShowSalesPartialView", null);
            //return PartialView("ShowSalesPartialView",repoCustomer.GetDailySales(distributorId, startDate, endDate, fromAmt, toAmt, workshopId));
        }
        #endregion

        #region Set Sales Executives
        public ActionResult SetSalesExecutive(int distributorId, string roUserId)
        {
            // Get SE users as per dist id            
            var ru = new RepoUsers();
            ViewBag.SeUsers = ru.GetDistributorUsers(distributorId, Constants.SalesExecutive);
            ViewBag.Controller = "Admin";
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
            object data = null;

            try
            {
                ViewBag.Controller = "Admin";
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
                bool status = ru.SaveSalesExecutives(roUserId, lstRoSe);

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

        #region Sales Executive's workshops
        public ActionResult AddSeWorkshops(int distributorId, string userId)
        {
            var ru = new RepoUsers();
            ViewBag.Controller = "Admin";
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
            object data = null;

            try
            {
                ViewBag.Controller = "Admin";
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

        #region Stock color settings
        public ActionResult StockColorSettings()
        {
            var ro = new RepoOrder();
            return View(ro.GetStockColorModel());
        }

        [HttpPost]
        public ActionResult SaveStockColorSettings(StockColorModel scm)
        {
            object data = null;
            try
            {
                var ro = new RepoOrder();
                ro.SaveOrUpdateStockColor(scm);

                data = new
                {
                    Message = "Stock color saved successfully!",
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

        #region Banner
        public ActionResult Banner()
        {
            ViewBag.Controller = "Admin";
            return View();
        }

        public ActionResult BannerPartialView()
        {
            RepoBanner Banner = new RepoBanner();
            ViewBag.Controller = "Admin";
            return PartialView(Banner.GetAllBanner());
        }

        public ActionResult AddBanner(int BannerId, string mode)
        {
            ViewBag.Controller = "Admin";

            var repousers = new RepoUsers();
            ViewBag.Distributors = repousers.GetAllDistributorsNew();

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {

                RepoBanner cls = new RepoBanner();
                var data = cls.GetBannerById(BannerId);
                return View(data);
            }

            ResponseBannerMobile model = new ResponseBannerMobile();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddBanner(ResponseBannerMobile model)
        {
            try
            {
                ViewBag.Controller = "Admin";
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
                        return RedirectToAction("Banner", "Admin");
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
                    return RedirectToAction("Banner", "Admin");
                }

                TempData["error"] = result.Message;
            }
            return RedirectToAction("Banner", "Admin");
        }
        #endregion

        #region MgaCatalouge Banner
        public ActionResult MgaBanner()
        {
            ViewBag.Controller = "Admin";
            return View();
        }

        public ActionResult MgaBannerPartialView()
        {
            ViewBag.Controller = "Admin";
            RepoMgaCatalougeBanner cls = new RepoMgaCatalougeBanner();
            return PartialView(cls.GetAllBanner());
        }

        public ActionResult MgaAddBanner(int Id, string mode)
        {
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            ViewBag.Distributors = repoCustomer.GetDailySales(0).Distributors.Items;
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                RepoMgaCatalougeBanner cls = new RepoMgaCatalougeBanner();
                var data = cls.GetBannerById(Id);
                return View(data);
            }

            return View();
        }

        [HttpPost]
        public ActionResult MgaAddBanner(MgaBanner model)
        {
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            ViewBag.Distributors = repoCustomer.GetDailySales(0).Distributors.Items;
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
                        return RedirectToAction("MgaBanner", "Admin");
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
                    return RedirectToAction("MgaBanner", "Admin");
                }

                TempData["error"] = result.Message;
            }
            return RedirectToAction("MgaBanner", "Admin");
        }
        #endregion

        #region MgaCatalouge Product
        public ActionResult MgaProduct(int bannerId, int distributoId)
        {
            ViewBag.Controller = "Admin";
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
                bool status = cls.SaveProducts(BannerId, lstproducts);
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

        #region ProductAvailabilityType
        public ActionResult ProductAvailabilityType()
        {
            return View();
        }

        public ActionResult ProductAvailabilityTypePartialView()
        {
            RepoProductAvailabilityType cls = new RepoProductAvailabilityType();
            return PartialView(cls.GetProductAvailabilities());
        }

        public ActionResult AddProductAvailabilityType(int Id, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                RepoProductAvailabilityType cls = new RepoProductAvailabilityType();
                var data = cls.GetProductAvailabilityById(Id);
                return View(data);
            }
            else
            {
                RepoProductAvailabilityType cls = new RepoProductAvailabilityType();
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddProductAvailabilityType(ProductAvailabilityType model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Text))
                {
                    ModelState.AddModelError("Text", "Please Enter Availability Type.");
                    return View(model);
                }
                if (ModelState.IsValid)
                {
                    RepoProductAvailabilityType cls = new RepoProductAvailabilityType();
                    var result = cls.SaveProductAvailabilityType(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["Success"] = result.Message;
                        return RedirectToAction("ProductAvailabilityType", "Admin");
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
        public ActionResult DeleteProductAvailabilityType(int Id, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "delete")
            {
                RepoProductAvailabilityType cls = new RepoProductAvailabilityType();
                var result = cls.DeleteGetProductAvailability(Id);
                if (result.ResultFlag == 1)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction("ProductAvailabilityType", "Admin");
                }

                TempData["error"] = result.Message;
            }
            return RedirectToAction("ProductAvailabilityType", "Admin");
        }
        #endregion

        #region Allocate, Save & Get Coupons
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
        public JsonResult SaveCoupons(List<SchemeWorkshop> schemeWorkshops)
        {
            object data;
            try
            {
                var rs = new RepoSchemes();
                rs.GenerateAndSaveCoupons(schemeWorkshops);

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

        public ActionResult ResetWorkshopWithCoupons(int schemeId)
        {
            var rs = new RepoSchemes();
            return PartialView("_GetWorkshopWithCoupons", rs.GetCouponSchemeAndWorkshops(schemeId));
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
            ViewBag.Url = $"{Request.Url.GetLeftPart(UriPartial.Authority)}/Admin/AssignGift?schemeId={schemeId}";

            var rs = new RepoSchemes();
            return View(rs.GetCouponsModel(schemeId, giftId));
        }

        [HttpPost]
        public ActionResult SaveCouponForGift(SchemeGift model)
        {

            object data = null;
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

        #region Orders and their status
        public ActionResult AllOrders()
        {
            ViewBag.Controller = "Admin";
            var ro = new RepoOrder();
            return View(ro.GetallOrders("Admin", 0));
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

        #region Scheme Cashback Transfer To Wallet
        public JsonResult SchemeCashbackTransferToWallet(int? workshopId, int schemeId, bool isAll)
        {
            object data = null;
            try
            {
                var rcs = new RepoSchemeCashback();
               var isSaved= rcs.DistributeSchemeCashback(workshopId, schemeId, isAll);

                data = new
                {
                    Message = isSaved? "Cashback successfully transfer!" : "Failled to transfer cashback",
                    ResultFlag = isSaved?1:0
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

        #region Show workshop levels
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

        #region Save Additional Coupons
        public JsonResult SaveAdditionalCoupons(int schemeId, int workshopId, int numberOfCoupons)
        {
            object data = null;
            try
            {
                var rs = new RepoSchemes();
                rs.GenerateAndSaveCoupons(schemeId, workshopId, numberOfCoupons);

                data = new
                {
                    Message = "Coupons saved successfully!",
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

        #region Decide Winners
        public ActionResult DecideWinners(int schemeId, int giftId)
        {
            try
            {
                var rs = new RepoSchemes();
                return PartialView("_DecideWinners", rs.GetDecideWinners(schemeId, giftId));
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }
            return PartialView("_DecideWinners", null);

        }

        [HttpPost]
        public JsonResult SaveDecideWinners(List<SchemeWorkshop> model)
        {
            object data = null;
            try
            {
                var rs = new RepoSchemes();
                var winnersSaved = rs.SaveDecideWinners(model);

                data = new
                {
                    Message = winnersSaved ? "Default winners saved for gift successfully!" : "Failed to save default winners.",
                    ResultFlag = winnersSaved ? 1 : 0
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
            var rw = new SystemController();
            //RepoWallet repoWallet = new RepoWallet();
            var data = rw.GetWsWallet(new WalletRequest()
            {
                PageNumber = -1,
                CouponNumber = couponNumber
            }).Data;
            return PartialView(data);
        }
        #endregion

        #region MailTemplate
        public ActionResult MailTemplate()
        {
            return View();
        }
        public ActionResult MailTemplatePartialView()
        {
            RepoMailTemplate repoMailTemplate = new RepoMailTemplate();
            return PartialView(repoMailTemplate.GetAllMailTemplate());
        }

        public ActionResult AddMailTemplate(string mode, string id)
        {
            RepoMailTemplate repoMailTemplate = new RepoMailTemplate();
            if (!string.IsNullOrEmpty(id))
            {
                var data = repoMailTemplate.GetMailTemplateById(Convert.ToInt32(id));
                return View(data);
            }
            return View(new MailTemplate());

        }

        [HttpPost]
        public ActionResult AddMailTemplate(MailTemplate model)
        {
            try
            {
                RepoMailTemplate repoMailTemplate = new RepoMailTemplate();
                string resultMsg = string.Empty;
                var data = repoMailTemplate.Add_Update_MailTemplate(model, out resultMsg);
                return Json(new { success = data, message = resultMsg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return Json(new { success = false, message = "Record Added & Updated Unsuccessfully" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteMailTemplate(string id)
        {
            try
            {
                RepoMailTemplate repoMailTemplate = new RepoMailTemplate();
                var data = repoMailTemplate.Delete(Convert.ToInt32(id));
                string msg = data ? "Mail template deleted successfully." : "Mail template deleted failed.";
                return Json(new { success = data, message = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return Json(new { success = false, message = "Record deleted Unsuccessfully" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Best Seller
        public ActionResult BestSeller()
        {
            var rbs = new RepoBestSeller(0);
            return View(rbs);
        }

        public ActionResult BestSellerByDistributor(int distributorId)
        {
            var rbs = new RepoBestSeller(distributorId);
            return PartialView("_AllSellers", rbs.AllSellers);
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

        #region Support Html
        public ActionResult SupportHtml()
        {
            return View();
        }

        public ActionResult SupportHtmlPartialView()
        {
            SystemController systemController = new SystemController();
            return PartialView(systemController.GetSupportHtml().Data);
        }

        public ActionResult AddSupportHtml(int? Id, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                RepoSupport rs = new RepoSupport();
                var data = rs.GetSupportHtmlById(Id.Value);
                return View(data);
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddSupportHtml(Support model)
        {
            try
            {
                SystemController systemController = new SystemController();
                var result = systemController.SaveSupportHtml(model);
                if (result.ResultFlag == 1)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction("SupportHtml", "Admin");
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

        #region Product Type
        public ActionResult ProductType()
        {
            return View();
        }

        public ActionResult ProductTypePartialView()
        {
            RepoProductType rpt = new RepoProductType();
            return PartialView(rpt.GetProductType());
        }

        public ActionResult AddProductType(int? Id, string mode)
        {
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                RepoProductType rpt = new RepoProductType();
                var data = rpt.GetProductTypeById(Id.Value);
                return View(data);
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddProductType(ProductType model)
        {
            try
            {
                RepoProductType rpt = new RepoProductType();
                var result = rpt.AddProductType(model);
                if (result)
                {
                    TempData["Success"] = "Product type save/update successfully.";
                    return RedirectToAction("ProductType", "Admin");
                }

                TempData["error"] = "Product type save/update failed.";
            }
            catch (Exception exc)
            {
                TempData["error"] = "Record Added & Updated Unsuccessfully";
                RepoUserLogs.LogException(exc);
            }
            return View(model);
        }
        #endregion

        #region Support Query
        public ActionResult SupportQuery()
        {
            return View();
        }

        public ActionResult SupportQueryPartialView()
        {
            SystemController systemController = new SystemController();
            return PartialView(systemController.GetAllSupportQuery().Data);
        }

        public ActionResult AddSupportQuery(int Id, string mode, bool IsFromNotification = false)
        {
            ViewBag.IsFromNotification = IsFromNotification;
            if (IsFromNotification)
            {
                // Mark notification as read            
                var nu = new NotificationUtil();
                nu.MarkNotificationRead(Id, General.GetUserId());
            }
            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                SystemController systemController = new SystemController();
                var data = systemController.GetSupportQueryById(new UpdateSupportQuery() { Id = Id });
                return View(data.Data);
            }
            else
            {
                SystemController systemController = new SystemController();
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddSupportQuery(ResponseSupportQuery model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                SystemController systemController = new SystemController();
                var result = systemController.SaveSupportQuery(model);
                if (result.ResultFlag == 1)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction("SupportQuery", "Admin");
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

        #region Distributor Growth Percentage
        public ActionResult GrowthPercentage()
        {
            var repousers = new RepoUsers();
            ViewBag.Distributor = repousers.GetAllDistributorsNew();
            ViewBag.Controller = "Admin";

            return View();
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

        #region Request Part Filter
        public ActionResult RequestPartFilter(int distributorId)
        {
            ViewBag.Controller = "admin";
            ViewBag.distributorId = distributorId;
            return View();
        }

        public ActionResult RequestPartFilterPartialView(int distributorId)
        {
            ViewBag.Controller = "admin";
            ViewBag.distributorId = distributorId;
            var model = new ClsPartfilterRequest();
            model.DistributorId = distributorId;
            RepoRequestPartFilter db = new RepoRequestPartFilter();
            return PartialView(db.GetAllRequestPartFilter(model));
        }

        public ActionResult AddRequestPartFilter(string mode, int distributorId, int? id)
        {
            RepoRequestPartFilter db = new RepoRequestPartFilter();
            ViewBag.Controller = "admin";
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
                    ViewBag.Controller = "admin";
                    return View(model);
                }
                RepoRequestPartFilter db = new RepoRequestPartFilter();
                var result = db.AddOrUpdateRequestPartFilter(model);
                if (result)
                {
                    TempData["Success"] = "Record added successfully.";
                    return RedirectToAction("RequestPartFilter", "Admin", new { distributorId = model.DistributorId });
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
            var cbo = new RepoCBO(0);
            return View(cbo);
        }

        public ActionResult CustomerBackOrdersByDistributor(int? distributorId)
        {
            if (distributorId == null)
            {
                var cbo = new RepoCBO(0);
                return PartialView("_BackOrders", cbo.CustomerBackOrders);
            }
            var cbos = new RepoCBO(distributorId.Value);
            return PartialView("_BackOrders", cbos.CustomerBackOrders);
        }
        #endregion

        #region Delete Accounts data

        public ActionResult DeleteAccountData()
        {
            ViewBag.Controller = "Admin";
            var repoCustomer = new RepoCustomers();
            return View(repoCustomer.GetDailySales(0));
        }

        [HttpPost]
        public ActionResult DeleteAccountData(DailySale model)
        {
            ViewBag.Controller = "Admin";

            if (!ModelState.IsValid)
            {
                ViewBag.Controller = "admin";
                return View(model);
            }
            try
            {
                RepoDataDelete repoDelete = new RepoDataDelete();
                var isdelete= repoDelete.DistributorAccountsDelete(model);
                if (isdelete)
                {
                    var repoUser = new RepoUsers();
                    var dist=repoUser.GetDistributorByDistributorId(model.DistributorId);
                    TempData["Success"] = $"Account data deleted for distributor :  {dist.DistributorName} !!";
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

