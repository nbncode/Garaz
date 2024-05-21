using Garaaz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Garaaz.Controllers
{
    [Authorize(Roles = "DistributorOutlets,RoIncharge")]
    public class OutletController : BaseController
    {
        #region Dashboard
        // GET: Outlet
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
            //return View();
        }
        #endregion

        #region Outlet Users
        public ActionResult OutletUsers()
        {
            string loginUserId = General.GetUserId();

            RepoUsers db = new RepoUsers();
            var outlet = db.GetOutletByUserId(loginUserId);

            if (outlet != null)
                ViewBag.outletId = outlet.OutletId;

            return View();
        }

        public ActionResult OutletUsersPartialView(int? outletId)
        {
            ViewBag.outletId = outletId;
            var ru = new RepoUsers();
            var users = ru.GetOutletUsers(Convert.ToInt32(outletId));
            return PartialView(users);
        }

        public ActionResult AddOutletUsers(string mode, int outletId, string id)
        {
            SystemController systemController = new SystemController();
            ViewBag.outletId = outletId;

            if (!string.IsNullOrEmpty(mode) && mode == "edit")
            {
                var data = systemController.GetOutletsUserById(new clsDistributorUserInfo()
                {
                    OutletId = outletId,
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
        public ActionResult AddOutletUsers(clsDistributorUserInfo model)
        {
            try
            {
                SystemController systemController = new SystemController();
                ViewBag.outletId = model.OutletId;
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    ViewBag.id = model.UserId;
                    model.PhoneNumber = "123123";
                    model.Password = "123123";
                }

                // Create outlet model
                var outletModel = new clsOutlet()
                {
                    UserId = model.UserId,
                    OutletId = model.OutletId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = model.Password,
                    Address = model.Address,
                    EmployeeCode = model.EmployeeCode,
                    Designations = model.Designations
                };
                var result = systemController.OutletUserRegisterOrUpdate(outletModel);

                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("OutletUsers", "outlet", new { outletId = model.OutletId });
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

        #region Orders and their status
        public ActionResult AllOrders()
        {
            ViewBag.Controller = "Outlet";
            string loginUserId = General.GetUserId();
            var gn = new General();
            int OutletId = gn.GetOutletByUserId(loginUserId);
            var ro = new RepoOrder();
            return View(ro.GetallOrders("Outlet", OutletId));
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

        #region Customer back orders
        public ActionResult CustomerBackOrders()
        {
            string loginUserId = General.GetUserId();
            var gn = new General();
            int OutletId = gn.GetOutletByUserId(loginUserId);

            var backOrder = new BackOrder();
            var cbo = backOrder.GetAllCboByOutletId(OutletId);
            return View(cbo);
        }
        #endregion

        #region CustomerBack-Orders Export To Excel
        [HttpPost]
        public ActionResult BackOrdersExportToExcel()
        {
            try
            {
                string loginUserId = General.GetUserId();
                var gn = new General();
                int OutletId = gn.GetOutletByUserId(loginUserId);
                BackOrder cbo = new BackOrder();
                var customerBackOrders = cbo.GetCboByOutletId(OutletId);

                if (customerBackOrders.Count == 0)
                {
                    return Json(new { error = "orders not found." });
                }
                var gv = new GridView();
                gv.DataSource = customerBackOrders;
                gv.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=CustomerBackOrders.xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";

                StringWriter objStringWriter = new StringWriter();
                HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
                gv.RenderControl(objHtmlTextWriter);

                Response.Output.Write(objStringWriter.ToString());
                Response.Flush();
                Response.End();
                return Json(new { fileName = Response });
            }
            catch (Exception ex)
            {
                return Json(new { error = "some unexpected errors." });
            }
        }
        #endregion
    }
}