using Garaaz.Models;
using System;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;

namespace Garaaz.Controllers
{
    [Authorize(Roles = "Workshop,WorkshopUsers")]
    public class WorkShopController : BaseController
    {
        #region Dashboard
        // GET: WorkShop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
        #endregion

        #region Workshop Users
        public ActionResult WorkshopUsers()
        {
            string loginUserId = General.GetUserId();

            RepoUsers db = new RepoUsers();
            var workshop = db.GetWorkshopByUserId(loginUserId);
            if (workshop != null) 
                ViewBag.workshopId = workshop.WorkShopId;

            return View();
        }

        public ActionResult WorkshopUsersPartialView(int workshopId)
        {
            ViewBag.workshopId = workshopId;
            RepoUsers db = new RepoUsers();
            var users = db.GetWorkshopUsers(workshopId);
            return PartialView(users);
        }

        public ActionResult AddWorkshopUsers(string mode, int workshopId, string id)
        {
            SystemController systemController = new SystemController();
            ViewBag.workshopId = workshopId;

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetWorkshopsUserById(new clsDistributorUserInfo()
                {
                    WorkshopId = workshopId,
                    UserId = id
                });
                return View(data.Data);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddWorkshopUsers(clsDistributorUserInfo model)
        {
            try
            {
                SystemController systemController = new SystemController();
                ViewBag.workshopId = model.WorkshopId;
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                // Create Workshop model
                var WorkshopModel = new clsWorkshop()
                {
                    UserId = model.UserId,
                    WorkshopId = model.WorkshopId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = model.Password,
                    Address = model.Address,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    EmployeeCode = model.EmployeeCode,
                    Designations = model.Designations
                };
                var result = systemController.WorkshopUserRegisterOrUpdate(WorkshopModel);

                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("WorkshopUsers", "Workshop", new { workshopId = model.WorkshopId });
                }
                else
                {
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

        #region User Features
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

        #region Order Functions    

        public ActionResult PlaceOrder()
        {
            string loginUserId = General.GetUserId();
            ViewBag.UserID = loginUserId;

            RepoUsers db = new RepoUsers();
            var workshop = db.GetWorkshopByUserId(loginUserId);

            if (workshop != null)
                ViewBag.WorkshopID = workshop.WorkShopId;
            return View();
        }

        #endregion

        #region Show all active schemes
        public ActionResult Schemes()
        {
            ViewBag.Controller = "Workshop";

            var rs = new RepoSchemes();
            var data = rs.GetWorkshopSchemes(User.Identity.GetUserId());
            return View(data);
        }
        #endregion

        #region Get coupons
        public ActionResult GetCoupons(SchemeWorkshop model)
        {
            var rs = new RepoSchemes();
            return PartialView("_WorkshopCoupons", rs.GetCoupons(model.SchemeId, model.WorkshopId));
        }
        #endregion              

        #region Orders and their status
        public ActionResult AllOrders()
        {
            ViewBag.Controller = "Workshop";
            string loginUserId = General.GetUserId();
            var gn = new General();
            int workshopId = gn.GetWorkshopId(loginUserId);
            var ro = new RepoOrder();
            return View(ro.GetallOrders("workshop", workshopId));
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

        #region Show allocated gift for workshop
        public ActionResult ShowWorkshopGifts(SchemeWorkshop model)
        {
            var rs = new RepoSchemes();
            return PartialView("_ShowWorkshopGifts", rs.GetGiftForWorkshop(model.SchemeId, model.WorkshopId));
        }
        #endregion
    }
}