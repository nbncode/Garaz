using Garaaz.Models;
using Garaaz.Models.Attributes;
using Garaaz.Models.Notifications;
using Garaaz.Models.Schemes;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using Constants = Garaaz.Models.Constants;

namespace Garaaz.Controllers
{
    public class HomeController : Controller
    {
        #region Home Page
        public ActionResult Index()
        {
            // used for decrypt user password
            //var pass = Utils.Decrypt("I3kvRq/lMns=");
            return View();
        }
        #endregion

        #region Privacy policy
        public ActionResult Privacy()
        {
            return View();
        }
        #endregion

        #region GetTokenByRefreshToken
        [HttpPost]
        public ActionResult GetTokenByRefreshToken()
        {
            Utils utils = new Utils();

            clsAuth auth = new clsAuth();
            auth.RefreshToken = General.GetRefreshToken();
            auth.UserId = General.GetUserId();

            SystemController systemController = new SystemController();
            var response = (TokenResponse)systemController.GetTokenByRefreshToken(auth).Data;
            if (response != null)
            {
                utils.setCurrentUserInfo(response);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Notifications
        public ActionResult AllNotifications()
        {
            return View();
        }

        public ActionResult AllNotificationsPartialView()
        {
            var nu = new NotificationUtil();
            return PartialView(nu.GetNotifications(General.GetUserId(), 0, true));
        }
        #endregion

        #region Image Upload
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase qqfile)
        {
            JsonResult json = null;

            if (qqfile != null)
            {
                // this works for IE
                string path = Guid.NewGuid() + "_" + Path.GetFileName(qqfile.FileName);
                var filename = Path.Combine(Server.MapPath("~/Content/attachment/"), path);
                string filePath = Path.Combine(("~/Content/attachment/"), path);
                qqfile.SaveAs(filename);
                List<string> lst = new List<string>();
                lst.Add(filePath);
                json = Json(new { success = true, list = lst, filename = filePath.Replace("~", "") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // this works for Firefox, Chrome
                var filename = Request["qqfile"];
                string path = Guid.NewGuid() + "_" + Path.GetFileName(qqfile.FileName);
                if (!string.IsNullOrEmpty(filename))
                {
                    filename = Path.Combine(Server.MapPath("~/Content/attachment/"), path);
                    string filePath = Path.Combine(("~/Content/attachment/"), path);
                    using (var output = System.IO.File.Create(path))
                    {
                        Request.InputStream.CopyTo(output);
                    }

                    json = Json(new { success = true, filename = filePath.Replace("~", "") }, JsonRequestBehavior.AllowGet);
                }
            }

            return json ?? Json(new { success = false });
        }
        #endregion

        #region Schemes

        public ActionResult SchemesPartialView()
        {
            var rs = new RepoSchemes();
            if (General.IsSuperAdmin())
            {
                ViewBag.Controller = "Admin";
                return PartialView(rs.GetAllScheme());
            }

            ViewBag.Controller = "Distributor";
            return PartialView(rs.GetAllScheme(General.GetCurrentDistributorId()));
        }

        [Authorize(Roles = "SuperAdmin,Distributor,DistributorUsers")]
        [CustomAuthorize("Schemes")]
        public ActionResult AddSchemes(string mode, int? id)
        {
            var repoUsers = new RepoUsers();
            var systemController = new SystemController();

            ViewBag.SchemeId = id;
            ViewBag.Role = General.GetUserRole();
            ViewBag.Distributors = repoUsers.GetAllDistributorsNew();
            ViewBag.Action = "Add";

            if (ViewBag.Role == Constants.Distributor)
            {
                ViewBag.DistributorId = General.GetCurrentDistributorId();
            }
            else if (ViewBag.Role == Constants.Users)
            {
                ViewBag.DistributorId = repoUsers.GetDistributorByUserId(General.GetUserId()).DistributorId;
            }

            if (string.IsNullOrEmpty(mode) || mode != "edit") return View();

            ViewBag.Action = "Edit";
            var schemeId = Convert.ToInt32(id);
            if (schemeId <= 0) throw new Exception("Scheme Id cannot be zero.");

            var respSchemeModel = (ResponseSchemeModel)systemController.GetSchemeBySchemeId(schemeId).Data;
            var distributorId = Convert.ToInt32(respSchemeModel.DistributorId);

            var rs = new RepoSchemes();
            var roIncharges = rs.GetRoIncharge(distributorId);
            ViewBag.SeUsers = rs.GetSalesExecutives(respSchemeModel.RoInchargeId, respSchemeModel.IsAllRoInchargeSelected, roIncharges);

            var roOutlets = new RepoOutlet();
            ViewBag.Outlets = roOutlets.GetDistributorOutlets(distributorId);

            ViewBag.customerTypes = rs.GetCustomerTypes(distributorId);

            return View(respSchemeModel);
        }

        #region Part Info

        //public ActionResult GetProdGroups(int distributorId, string partCreation, string partCategory, string fValue, string mValue, string sValue)
        //{
        //    object data;
        //    try
        //    {
        //        var rs = new RepoSchemes();

        //        data = new
        //        {
        //            Message = "Product groups found.",
        //            ResultFlag = 1,
        //            Data = rs.GetProdGroups(distributorId, partCreation, partCategory, fValue, mValue, sValue)
        //        };
        //    }
        //    catch (Exception exc)
        //    {
        //        data = new
        //        {
        //            exc.Message,
        //            ResultFlag = 0
        //        };
        //        RepoUserLogs.LogException(exc);
        //    }

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult GetProducts(int groupId)
        //{
        //    object data;
        //    try
        //    {
        //        var rs = new RepoSchemes();

        //        data = new
        //        {
        //            Message = $"Products matching to Group Id {groupId} found.",
        //            ResultFlag = 1,
        //            Data = rs.GetProducts(groupId)
        //        };
        //    }
        //    catch (Exception exc)
        //    {
        //        data = new
        //        {
        //            exc.Message,
        //            ResultFlag = 0
        //        };
        //        RepoUserLogs.LogException(exc);
        //    }

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult GetFocusPartData(int schemeId, bool emptyRow, int distributorId, string partCategory)
        //{
        //    var repoSchemes = new RepoSchemes();
        //    ViewBag.Groups = repoSchemes.GetProdGroups(distributorId, null, partCategory, null, null, null);

        //    if (emptyRow || schemeId <= 0)
        //    {
        //        return PartialView(new List<ResponseFocusPartModel>());
        //    }

        //    return PartialView(repoSchemes.GetFocusPart(schemeId));
        //}

        public ActionResult GetFmsPartData(int schemeId)
        {
            object data = null;
            try
            {
                var repoSchemes = new RepoSchemes();
                var schemeModel = repoSchemes.GetFmsPartData(schemeId);
                data = new
                {
                    Data = schemeModel,
                    Message = schemeModel != null ? "Successfully accessed FMS part data!" : "Failed to access FMS part data.",
                    ResultFlag = schemeModel != null ? 1 : 0
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    Data = data,
                    Message = exc.Message,
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UploadFocusPartsGroup(string filePath, int schemeId, int distributorId, string partCategory)
        {
            var listRfpm = new List<FocusGroupModel>();
            try
            {
                var rs = new RepoSchemes();

                var rc = new RepoCustomers();
                var absoluteFilePath = Server.MapPath(filePath);
                listRfpm = rc.GetFocusPartFromExcel(absoluteFilePath, schemeId, distributorId, out string errorMsg);
                ViewBag.ErrorMsgForNonMatchedGroups = errorMsg;

                // Delete existing focus part (if editing scheme and uploading)                
                if (schemeId > 0 && listRfpm.Count > 0)
                {
                    rs.DeleteFocusParts(schemeId);
                }

            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            return PartialView("GetFocusGroupParts", listRfpm);
            //var listRfpm = new List<ResponseFocusPartModel>();
            //try
            //{
            //    var rs = new RepoSchemes();
            //    ViewBag.Groups = rs.GetProdGroups(distributorId, null, partCategory, null, null, null);

            //    // Delete existing focus part (if editing scheme and uploading)                
            //    if (schemeId > 0 && listRfpm.Count > 0)
            //    {
            //        rs.DeleteFocusParts(schemeId);
            //    }

            //    var rc = new RepoCustomers();
            //    var absoluteFilePath = Server.MapPath(filePath);
            //    listRfpm = rc.GetFocusPartFromExcel(absoluteFilePath, out string errorMsg);
            //    ViewBag.ErrorMsgForNonMatchedGroups = errorMsg;

            //    ViewBag.Products = rs.GetProductsByGroupIds(listRfpm.Select(g => g.GroupId).ToList());

            //}
            //catch (Exception exc)
            //{
            //    TempData["error"] = exc.Message;
            //    RepoUserLogs.LogException(exc);
            //}

            //return PartialView("getFocusPartData", listRfpm);
        }

        public ActionResult GetFocusGroupParts(int schemeId, bool emptyRow)
        {
            var repoSchemes = new RepoSchemes();
            if (emptyRow && schemeId <= 0 || emptyRow)
            {
                return PartialView(new List<FocusGroupModel>());
            }
            var data = repoSchemes.GetSchemeFocusPart(schemeId);
            return PartialView(data);
        }

        public JsonResult SearchGroup(string partCategory, int distributorId, string groupIds, string query)
        {
            object data;
            try
            {
                var repoSchemes = new RepoSchemes();
                var suggestions = repoSchemes.GetAutocompleteGroups(distributorId, partCategory, groupIds, query);

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

        public ActionResult GetGroupParts(string productIds, int groupId, int distributorId)
        {
            string[] productIdArr = null;
            if (!string.IsNullOrEmpty(productIds))
                productIdArr = productIds.Split(',');

            var lstProductIds = new List<int>();
            if (productIdArr != null)
            {
                foreach (var id in productIdArr)
                {
                    lstProductIds.Add(Convert.ToInt32(id));
                }
            }
            ViewBag.ProductIds = lstProductIds;
            ViewBag.GroupId = groupId;
            var repoSchemes = new RepoSchemes();
            var parts = repoSchemes.GetSchemeFocusPart(groupId, distributorId);
            return PartialView(parts);
        }

        [HttpPost]
        public JsonResult AddFocusPart(List<FocusGroupModel> focusParts)
        {
            object data;
            try
            {
                var repoSchemes = new RepoSchemes();
                var isSaved = repoSchemes.SaveFocusPart(focusParts);

                data = new
                {
                    Message = isSaved ? "Saved focus part successfully!" : "Failed to save focus part.",
                    ResultFlag = isSaved ? 1 : 0
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                data = new
                {
                    exc.Message,
                    ResultFlag = 0
                };
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Customer Segment

        public ActionResult GetBranchCodesByDistributorId(int? DistributorId)
        {
            object data;
            try
            {
                var roOutlets = new RepoOutlet();
                data = new
                {
                    Message = "Outlet users found.",
                    ResultFlag = 1,
                    Data = roOutlets.GetOutlets(DistributorId.Value)
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
        public ActionResult GetSalesExecutiveByBranchCodes(List<int> branchCodes)
        {
            object data;
            try
            {
                var rs = new RepoSchemes();
                data = new
                {
                    Message = "Sales Executives users found.",
                    ResultFlag = 1,
                    Data = rs.GetSalesExecutives(branchCodes)
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
        public ActionResult GetCustomerTypesByDistributorId(int? DistributorId)
        {
            object data;
            try
            {
                var rs = new RepoSchemes();
                data = new
                {
                    Message = "Customer types found.",
                    ResultFlag = 1,
                    Data = rs.GetCustomerTypes(DistributorId.Value)
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
        public ActionResult GetTargetGrowth(int schemeId)
        {
            if (schemeId <= 0)
                return PartialView(new List<TargetGrowth>());

            var rs = new RepoSchemes();
            var data = rs.GetTargetGrowthsBySchemeId(schemeId);
            return PartialView(data);
        }

        public ActionResult GetCategories(int schemeId)
        {
            if (schemeId <= 0)
                return PartialView(new List<ResponseSchemeCategoryModel>());

            var rs = new RepoSchemes();
            return PartialView(rs.GetCategoryScheme(schemeId));
        }

        public ActionResult DeleteAllCategories(int schemeId)
        {
            object json;
            try
            {
                var rs = new RepoSchemes();
                var deleted = rs.DeleteCategorySchemes(schemeId);

                json = new
                {
                    ResultFlag = deleted ? 1 : 0,
                    Message = deleted ? "Deleted all categories of scheme successfully!" : "Failed to delete categories of scheme."
                };
            }
            catch (Exception exc)
            {
                json = new
                {
                    ResultFlag = 0,
                    exc.Message
                };

                RepoUserLogs.LogException(exc);
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Target Info

        public ActionResult GetTargetWorkShopData(SchemeModel schemeModel)
        {
            var repoSchemes = new RepoSchemes();
            var respSchemeModel = repoSchemes.GetSchemeBySchemeId(schemeModel.SchemeId);
            var growthPercentage = new ClsGrowthPercentage().GetGrowthPercentage(new GrowthPercentageModel { DistributorId = Convert.ToInt32(schemeModel.DistributorId) });

            ViewBag.MinValue = respSchemeModel?.GrowthCompPercentMinValue ?? growthPercentage.MinValue;
            ViewBag.BaseValue = respSchemeModel?.GrowthCompPercentBaseValue;

            if (string.IsNullOrEmpty(schemeModel.FilePath))
            {
                if (schemeModel.SchemeId <= 0)
                    return PartialView(new List<ResponseTargetWorkshopModel>());

                var targetWsList = repoSchemes.GetTargetWorkshops(schemeModel);
                return PartialView(targetWsList);
            }
            var absoluteFilePath = Server.MapPath(schemeModel.FilePath);
            var targetWorkshops = repoSchemes.GetTargetWorkshopsFromFile(absoluteFilePath, schemeModel.SchemeId, out string errorMsg);
            ViewBag.ErrorMsgForNonMatchedWorkshops = errorMsg;
            return PartialView(targetWorkshops);
        }

        public ActionResult CalculateGrowth(string workshops, int schemeId, decimal growthCompPercentMinValue, decimal growthCompPercentBaseValue)
        {
            ViewBag.MinValue = growthCompPercentMinValue;
            ViewBag.BaseValue = growthCompPercentBaseValue;

            var repoSchemes = new RepoSchemes();
            var targetWorkshops = repoSchemes.CalculateGrowth(workshops, schemeId, growthCompPercentMinValue, growthCompPercentBaseValue);
            return PartialView("getTargetWorkShopData", targetWorkshops);
        }

        public ActionResult SortByGrowth(string workshops, int distributorId, int schemeId, string sortBy)
        {
            var repoSchemes = new RepoSchemes();
            var respSchemeModel = repoSchemes.GetSchemeBySchemeId(schemeId);
            var growthPercentage = new ClsGrowthPercentage().GetGrowthPercentage(new GrowthPercentageModel { DistributorId = Convert.ToInt32(distributorId) });

            ViewBag.MinValue = respSchemeModel?.GrowthCompPercentMinValue ?? growthPercentage.MinValue;
            ViewBag.BaseValue = respSchemeModel?.GrowthCompPercentBaseValue;

            var targetWorkshops = repoSchemes.SortByGrowth(workshops, distributorId, schemeId, sortBy);
            return PartialView("getTargetWorkShopData", targetWorkshops);
        }

        #endregion

        #region Reward Info

        public async Task<ActionResult> GetTargetOverview(int schemeId)
        {
            List<TargetOverview> targetOverviews = null;
            try
            {
                var repoSchemes = new RepoSchemes();
                targetOverviews = await repoSchemes.GetTargetOverviewAsync(schemeId).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                ViewBag.ErrorMsg = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            return PartialView("_TargetOverview", targetOverviews);
        }

        public ActionResult GetCashBackRangeData(int schemeId, bool emptyRow)
        {
            ViewBag.EmptyRow = emptyRow;
            var repoSchemes = new RepoSchemes();
            return PartialView(emptyRow ? new List<CashbackRange>() : repoSchemes.GetCashBackRange(schemeId));
        }

        public ActionResult GetCashBackData(int schemeId, bool emptyRow)
        {
            var repoSchemes = new RepoSchemes();
            ViewBag.CashBackRange = repoSchemes.GetCashBackRange(schemeId);

            return PartialView(emptyRow ? new List<ResponseCashBackModel>() : repoSchemes.GetCashBack(schemeId));
        }

        public ActionResult GetAssuredGiftData(int schemeId, bool emptyRow)
        {
            var repoSchemes = new RepoSchemes();
            ViewBag.Categories = repoSchemes.GetCategoryScheme(schemeId);

            return PartialView(emptyRow ? new List<ResponseAssuredGiftModel>() : repoSchemes.GetAssuredGift(schemeId));
        }

        public ActionResult GetGiftManagementData(int schemeId, bool emptyRow)
        {
            var repoSchemes = new RepoSchemes();
            ViewBag.Categories = repoSchemes.GetCategoryScheme(schemeId);

            return PartialView(emptyRow ? new List<ResponseGiftManagementModel>() : repoSchemes.GetGiftManagement(schemeId));
        }

        public ActionResult GetQualifyingCriteriaData(int schemeId, bool emptyRow)
        {
            var productPagePermission = new ProductPagePermission();
            ViewBag.Products = productPagePermission.GetAllProduct();

            var repoSchemes = new RepoSchemes();
            return PartialView(emptyRow ? new List<ResponseQualifyCriteriaModel>() : repoSchemes.GetQualifyCriteria(schemeId));
        }

        #endregion

        public ActionResult GetLocations(int DistributorId, int? SchemeId)
        {
            RepoUsers repoUsers = new RepoUsers();
            ViewBag.Locations = repoUsers.GetDistributorLocations(DistributorId);
            RepoSchemes repoSchemes = new RepoSchemes();
            if (SchemeId.HasValue)
            {
                return PartialView(repoSchemes.GetSchemeLocations(SchemeId.Value));
            }

            return PartialView(new List<SchemeLocation>());
        }

        public ActionResult GetTickeOfJoyData(int? SchemeId, bool EmptyRow)
        {
            ProductPagePermission objProduct = new ProductPagePermission();
            ProductGroupPagePermissions objProductGroup = new ProductGroupPagePermissions();
            RepoSchemes repoSchemes = new RepoSchemes();

            ViewBag.ProductGroup = objProductGroup.GetAllProductGroup(SchemeId.Value);
            ViewBag.Product = objProduct.GetAllProduct();

            if (SchemeId.HasValue && SchemeId.Value > 0 && !EmptyRow)
            {
                return PartialView(repoSchemes.GetTicketOfJoy(SchemeId.Value));
            }

            return PartialView(new List<ResponseTicketOfJoyModel>());
        }

        public ActionResult GetLabelData(int? SchemeId, bool EmptyRow, int? distributorId, string partCreation, string partCategory, string fValue, string mValue, string sValue)
        {
            var rs = new RepoSchemes();
            ViewBag.Groups = rs.GetProdGroups(Convert.ToInt32(distributorId), partCreation, partCategory, fValue, mValue, sValue);

            if (SchemeId.HasValue && SchemeId.Value > 0 && !EmptyRow)
            {
                var rl = new RepoLabels();
                return PartialView(rl.GetLabelCriteriaBySchemesId(SchemeId.Value));
            }

            return PartialView(new List<LabelCriteria> { new LabelCriteria() });
        }

        public ActionResult GetWorkShopDataByCriteria(List<CriteriaWorkShopModel> listCws)
        {
            if (listCws == null)
            {
                listCws = new List<CriteriaWorkShopModel>();
            }
            var schemeId = listCws.Select(l => l.SchemeId).FirstOrDefault();
            var rl = new RepoLabels();

            ViewBag.SavedWorkshop = schemeId > 0 ? rl.getWorkShopSaved(schemeId.Value) : new List<WorkshopModelScheme>();
            ViewBag.schemeId = schemeId;

            return PartialView(rl.GetWorkshopsByLabelCriteria(listCws, null));
        }

        public ActionResult GetRoIncharge(int distributorId)
        {
            object data = null;
            try
            {
                var rs = new RepoSchemes();
                data = new
                {
                    Message = "RO Incharge users found.",
                    ResultFlag = 1,
                    Data = rs.GetRoIncharge(distributorId)
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

        public ActionResult GetSalesExecutive(string roUserId, int distributorId, bool? isAllSelected)
        {
            object data;
            try
            {
                var rs = new RepoSchemes();
                data = new
                {
                    Message = "Sales Executives users found.",
                    ResultFlag = 1,
                    Data = rs.GetSalesExecutives(roUserId, isAllSelected, rs.GetRoIncharge(distributorId))
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

        [HttpPost]
        public ActionResult SaveLabelCriteria(List<LabelCriteria> labelCriteria)
        {
            var repoLabels = new RepoLabels();
            var result = repoLabels.SaveLabel(labelCriteria);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveLabelWorkShops(List<WorkShopLabelScheme> workshops)
        {
            RepoLabels repoLabels = new RepoLabels();
            var result = repoLabels.saveWorkShopOnTarget(workshops);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Uploads in Scheme

        public ActionResult UploadWorkshop(string filePath, int schemeId, int distributorId)
        {
            var listWs = new List<WorkshopModelScheme>();
            try
            {
                var rl = new RepoLabels();
                ViewBag.SavedWorkshop = schemeId > 0 ? rl.getWorkShopSaved(schemeId) : new List<WorkshopModelScheme>();
                ViewBag.schemeId = schemeId;

                var ru = new RepoUsers();
                var existingWs = ru.GetWorkshopByDistId(distributorId);

                var rc = new RepoCustomers();
                var uplWs = rc.GetWorkshopFromExcel(filePath);
                var wsIDs = new HashSet<int>(uplWs.Select(s => s.WorkShopId));

                // Filter existing workshops by user' uploaded workshops.
                listWs = existingWs.Where(w => wsIDs.Contains(w.WorkShopId)).Select(w => new WorkshopModelScheme
                {
                    WorkShopId = w.WorkShopId,
                    WorkShopName = w.WorkShopName,
                    Address = w.Address
                }).ToList();
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            return PartialView("getWorkShopDataByCriteria", listWs);
        }
        #endregion


        #region Scheme Preview

        public ActionResult SchemePreview(int schemeId)
        {
            ViewBag.SchemeId = schemeId;
            return View();
        }

        public async Task<ActionResult> SchemePreviewPartial(int schemeId)
        {
            try
            {
                ViewBag.schemeId = schemeId;
                var repoSchemes = new RepoSchemes();
                var schemePreview = await repoSchemes.GetSchemePreviewAsync(schemeId).ConfigureAwait(false);
                return PartialView("_SchemePreview", schemePreview);
            }
            catch (Exception exc)
            {
                ViewBag.ErrorMsg = exc.Message;
                RepoUserLogs.LogException(exc);
            }
            return PartialView("_SchemePreview", null);
        }

        [HttpPost]
        public JsonResult UpdateAchievedTargetForWorkshop(int schemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var isSaved = repoSchemes.SaveTargetAchievedForWorkshop(schemeId);

                var result = new ResultModel
                {
                    Message = isSaved ? "Achieved target successfully update!" : "Failed to update achieved target.",
                    ResultFlag = isSaved ? 1 : 0
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return Json(new ResultModel
                {
                    Message = exc.Message,
                    ResultFlag = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Target Workshops Export To Excel
        [HttpPost]
        public ActionResult TargetWorkshopsExport(int? schemeId)
        {
            if (schemeId == null)
            {
                TempData["error"] = "workshops not found.";
                return Redirect(Request.UrlReferrer.ToString());
            }
            var repoSchemes = new RepoSchemes();
            var targetWorkshops = repoSchemes.GetTargetWorkshopsBySchemeId(schemeId.Value);

            if (targetWorkshops.Count == 0)
            {
                TempData["error"] = "workshops not found.";
                return Redirect(Request.UrlReferrer.ToString());
            }
            var gv = new GridView();
            gv.DataSource = targetWorkshops;
            gv.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=TargetWorkshops.xls");
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
        #endregion

        #endregion

        #region Show all brands
        [Authorize(Roles = "Workshop,WorkshopUsers,DistributorUsers")]
        [CustomAuthorize("Order")]
        public ActionResult Brands()
        {
            //var sc = new SystemController();
            //ResultModel rm = sc.GetAllBrand(new clsBrand { SearchString = "" });
            //return View(rm.Data);
            return View("Oriparts");
        }
        #endregion

        #region Show all vehicles
        [Authorize(Roles = "Workshop,DistributorUsers")]
        [CustomAuthorize("Order")]
        public ActionResult Vehicles(int? id)
        {
            if (id.HasValue)
            {
                var sc = new SystemController();
                ResultModel rm = sc.GetVehiclesByBrandID(new clsVehicle { BrandId = Convert.ToInt32(id) });

                if (rm.Data != null)
                    return View(rm.Data);
                else
                    return View(new List<clsVehicle>());
            }
            else
            {
                return View(new List<clsVehicle>());
            }

        }
        #endregion

        #region Show all variants
        [Authorize(Roles = "Workshop,DistributorUsers")]
        [CustomAuthorize("Order")]
        public ActionResult Variants(int? id)
        {
            if (id.HasValue)
            {
                var sc = new SystemController();
                ResultModel rm = sc.GetVariantByVehicleID(new clsVariant { VehicleId = Convert.ToInt32(id) });
                if (rm.Data != null)
                    return View(rm.Data);
                else
                    return View(new List<clsVariant>());
            }
            else
            {
                return View(new List<clsVariant>());
            }

        }
        #endregion

        #region Show all groups
        [Authorize(Roles = "Workshop,DistributorUsers")]
        [CustomAuthorize("Order")]
        public ActionResult Groups(int? id)
        {
            if (id.HasValue)
            {
                var sc = new SystemController();
                ResultModel rm = sc.GetProductGroupsByVariantID(new clsProductGroup { VariantId = Convert.ToInt32(id) });

                if (rm.Data != null)
                {
                    return View(rm.Data);
                }
                else
                    return View(new List<clsProductGroup>());
            }
            else
            {
                return View(new List<clsProductGroup>());
            }
        }

        #endregion

        #region Show all sub groups
        [Authorize(Roles = "Workshop,DistributorUsers")]
        [CustomAuthorize("Order")]
        public ActionResult SubGroups(int? id)
        {
            if (id.HasValue)
            {
                var sc = new SystemController();
                ResultModel rm = sc.GetProductGroupByGroupId(new clsProductGroup { GroupId = Convert.ToInt32(id) });

                if (rm.Data != null)
                    return View(rm.Data);
                else
                    return RedirectToAction("Products", new { id });
            }
            else
            {
                return View(new List<clsProductGroup>());
            }
        }
        #endregion

        #region Show all products
        [Authorize(Roles = "Workshop,DistributorUsers")]
        [CustomAuthorize("Order")]
        public ActionResult Products(int? id)
        {
            if (id.HasValue)
            {
                // Find products and show them
                var sc = new SystemController();
                var rm = sc.GetProductByGroupID(new clsProductGroup { GroupId = Convert.ToInt32(id) });

                if (rm.Data != null)
                    return View(rm.Data);
                else
                    return View(new List<ResponseProductModel>());
            }
            else
            {
                return View(new List<ResponseProductModel>());
            }
        }
        #endregion

        #endregion

        #region Add To cart
        [HttpPost]
        [CustomAuthorize("Order")]
        public ActionResult AddToCart(OrderDetail model)
        {
            object data = null;
            try
            {
                var utils = new Utils();
                var tempOrderId = utils.getIntCookiesValue(Constants.TempOrderId);

                var atcModel = new AddtocartModel
                {
                    TempOrderId = tempOrderId,
                    ProductId = Convert.ToInt32(model.ProductID),
                    Qty = Convert.ToInt32(model.Qty),
                    UnitPrice = model.UnitPrice,
                    UserId = General.GetUserId()
                };
                var sc = new SystemController();

                var rm = sc.AddtoCartMobile(atcModel);

                // Save tempOrderId to cookie for further pages
                if (rm.Data != null)
                {
                    tempOrderId = Convert.ToInt32(rm.Data);
                    utils.setCookiesValue(tempOrderId, Constants.TempOrderId);
                }

                data = new
                {
                    rm.Message,
                    rm.ResultFlag
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

        [HttpPost]
        [CustomAuthorize("Order")]
        public ActionResult AddProductToCart(string partNumber, string price)
        {
            object data = null;
            try
            {
                var utils = new Utils();
                var tempOrderId = utils.getIntCookiesValue(Constants.TempOrderId);

                // Get numeric value from price (INR 21.00)
                price = !string.IsNullOrEmpty(price) ? price.Remove(0, 4) : "0";

                var atcModel = new AddtocartModel
                {
                    TempOrderId = tempOrderId,
                    Qty = 1,
                    UnitPrice = 0,
                    UserId = General.GetUserId(),
                    PartNumber = partNumber
                };
                if (decimal.TryParse(price, out decimal dPrice))
                {
                    atcModel.UnitPrice = dPrice;
                }

                var sc = new SystemController();
                var rm = sc.AddtoCartMobile(atcModel);

                // Save tempOrderId to cookie for further pages
                if (rm.Data != null)
                {
                    tempOrderId = Convert.ToInt32(rm.Data);
                    utils.setCookiesValue(tempOrderId, Constants.TempOrderId);
                }

                data = new
                {
                    rm.Message,
                    rm.ResultFlag
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    Message = $"{exc.Message} \r\n {exc.StackTrace}",
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //[CustomAuthorize("Order")]
        public ActionResult AddProductToCartByApp(string partNumber, string price, string userId, int tempOrderId)
        {
            object data = null;
            try
            {
                var utils = new Utils();
                //if (tempOrderId <= 0) { 
                // tempOrderId = utils.getIntCookiesValue(Constants.TempOrderId);
                //}

                // Get numeric value from price (INR 21.00)
                price = !string.IsNullOrEmpty(price) ? price.Remove(0, 4) : "0";
                RepoUsers repoUsers = new RepoUsers();
                var roleArr = repoUsers.GetRolesByUserId(userId);
                var role = roleArr.OrderByDescending(o => o).FirstOrDefault();
                var atcModel = new AddtocartModel
                {
                    TempOrderId = tempOrderId,
                    Qty = 1,
                    UnitPrice = 0,
                    UserId = userId,//General.GetUserId(),
                    PartNumber = partNumber,
                    Role = role
                };
                if (decimal.TryParse(price, out decimal dPrice))
                {
                    atcModel.UnitPrice = dPrice;
                }

                var sc = new SystemController();
                var rm = sc.AddtoCartMobile(atcModel);

                // Save tempOrderId to cookie for further pages
                if (rm.Data != null)
                {
                    tempOrderId = Convert.ToInt32(rm.Data);
                    utils.setCookiesValue(tempOrderId, Constants.TempOrderId);
                }

                data = new
                {
                    rm.Data,
                    rm.Message,
                    rm.ResultFlag
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    Message = $"{exc.Message} \r\n {exc.StackTrace}",
                    ResultFlag = 0
                };
                RepoUserLogs.LogException(exc);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Cart Functions

        public ActionResult Cart()
        {
            var utils = new Utils();
            var sc = new SystemController();
            var rm = sc.GetCart(new GetCartModel { TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId), UserId = General.GetUserId() });

            return PartialView(rm.Data);
        }

        [CustomAuthorize("Order")]
        public ActionResult ShoppingCart()
        {
            var utils = new Utils();
            var sc = new SystemController();
            var rm = sc.GetCart(new GetCartModel { TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId), UserId = General.GetUserId() });
            ViewBag.Role = General.GetUserRole();
            return View(rm.Data);
        }

        [HttpPost]
        public ActionResult UpdateShoppingCart(List<clsCartData> model)
        {
            object data = null;

            try
            {
                var utils = new Utils();
                var tempOrderId = utils.getIntCookiesValue(Constants.TempOrderId);

                model.ForEach(a => a.TempOrderId = tempOrderId);

                var sc = new SystemController();
                var rm = sc.UpdateCart(model);

                data = new
                {
                    rm.Message,
                    rm.ResultFlag
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

        [HttpPost]
        public ActionResult DeleteFromCart(OrderDetail oDetails)
        {
            object data = null;
            var utils = new Utils();
            var tempOrderId = utils.getIntCookiesValue(Constants.TempOrderId);

            var sc = new SystemController();
            var rm = sc.RemoveCart(new RemoveCartModel { TempOrderId = tempOrderId, ProductId = oDetails.ProductID });

            data = new
            {
                rm.Message,
                rm.ResultFlag
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize("Order")]
        public ActionResult Checkout()
        {
            var utils = new Utils();
            var sc = new SystemController();
            var rm = sc.GetDeliveryAddress(new GetCartModel { TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId) });

            CheckoutDeliveryAddressModel deliveryAddress;
            if (rm.ResultFlag == 1)
            {
                deliveryAddress = rm.Data as CheckoutDeliveryAddressModel;
            }
            else
            {
                deliveryAddress = new CheckoutDeliveryAddressModel();
            }

            return View(deliveryAddress);
        }

        [HttpPost]
        public ActionResult Checkout(CheckoutDeliveryAddressModel model)
        {
            try
            {
                var utils = new Utils();

                model.UserId = General.GetUserId();
                model.TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId);

                var sc = new SystemController();
                var result = sc.SaveDeliveryAddress(model);

                if (result.ResultFlag == 1)
                {
                    TempData["success"] = result.Message;
                    return RedirectToAction("PaymentMethod");
                }
                else
                {
                    TempData["error"] = result.Message;
                }

            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [CustomAuthorize("Order")]
        public ActionResult PaymentMethod()
        {
            var utils = new Utils();
            var sc = new SystemController();
            var rm = sc.GetGrandTotal(new GetCartModel { TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId) });

            return View(rm.Data);
        }

        //[HttpPost]
        //public ActionResult PaymentMethod(string paymentMethod)
        //{
        //    if (string.IsNullOrEmpty(paymentMethod))
        //    {
        //        TempData["error"] = "Please choose payment method.";
        //    }
        //    else
        //    {
        //        var utils = new Utils();
        //        var sc = new SystemController();
        //        var rm = sc.SavePaymentMethod(new PaymentMethodModel { TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId), PaymentMethod = paymentMethod });

        //        if (rm.ResultFlag == 1)
        //        {
        //            TempData["success"] = rm.Message;
        //            return RedirectToAction("SaveOrder");
        //        }
        //        else
        //        {
        //            TempData["error"] = rm.Message;
        //        }
        //    }

        //    return View();
        //}
        [HttpPost]
        public ActionResult PaymentMethod(string paymentMethod, string razorOrderId, string razorPaymentId, string razorSignature)
        {
            object data = null;
            try
            {
                var utils = new Utils();
                var sc = new SystemController();
                data = sc.SavePaymentMethod(new PaymentMethodModel { TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId), PaymentMethod = paymentMethod, RazorOrderId = razorOrderId, RazorPaymentId = razorPaymentId, RazorSignature = razorSignature });
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreateOrderId_With_RazorPay()
        {
            object data = null;
            try
            {
                RepoOrder repoOrder = new RepoOrder();
                Utils utils = new Utils();
                var tempOrderId = utils.getStringCookiesValue(Constants.TempOrderId);
                data = repoOrder.CreateRazorPayOrderId(tempOrderId);
            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //[CustomAuthorize("Order")]
        //public ActionResult SaveOrder()
        //{
        //    SaveOrderModel soModel = null;
        //    try
        //    {
        //        var utils = new Utils();
        //        var sc = new SystemController();
        //        var rm = sc.SaveOrder(new SaveOrderModel { TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId) });

        //        if (rm.ResultFlag == 1)
        //            utils.RemoveCookiesValue(Constants.TempOrderId);

        //        soModel = rm.Data as SaveOrderModel;

        //    }
        //    catch (Exception exc)
        //    {
        //        TempData["error"] = exc.Message;
        //        RepoUserLogs.LogException(exc);
        //    }
        //    return View(soModel);
        //}
        [CustomAuthorize("Order")]
        public ActionResult SaveOrder(string data)
        {
            SaveOrderModel soModel = null;
            try
            {
                string pm = data.Split('~')[0];
                string roId = data.Split('~')[1];
                string rpId = data.Split('~')[2];
                string rs = data.Split('~')[3];
                var utils = new Utils();
                var sc = new SystemController();
                var rm = sc.SaveOrder(new SaveOrderModel
                {
                    TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId),
                    PaymentMethod = pm,
                    RazorpayOrderId = roId,
                    RazorpayPaymentId = rpId,
                    RazorpaySignature = rs
                });

                if (rm.ResultFlag == 1)
                    utils.RemoveCookiesValue(Constants.TempOrderId);

                soModel = rm.Data as SaveOrderModel;

            }
            catch (Exception exc)
            {
                TempData["error"] = exc.Message;
                RepoUserLogs.LogException(exc);
            }
            return View(soModel);
        }

        [CustomAuthorize("Order", "CurrentOrder")]
        public ActionResult OrderDetails(int orderId, string userId, string backurl)
        {
            var ro = new RepoOrder();
            ViewBag.backurl = backurl;
            ViewBag.AvailabilityTypes = ro.GetAvailabilityTypes();
            ViewBag.Outlets = ro.OutletsSelectList(orderId, userId);
            ViewBag.Role = General.GetUserRole();
            var sc = new SystemController();
            var rm = sc.GetOrderByOrderId(new GetOrderModel { OrderId = orderId, UserId = userId });

            return View(rm.Data);
        }

        #region Get Order Detail By Notification
        public ActionResult OrderDetailByOrderId(int orderId)
        {
            var sc = new SystemController();
            var rm = sc.GetOrderUserByOrderId(new OrderStatusRequest { OrderId = orderId });
            var userId = rm.Data as string;
            return RedirectToAction("OrderDetails", new { orderId, userId });
        }
        #endregion

        #region Update and Order details
        public ActionResult UpdateQuantity(List<OrderDetail> model)
        {
            object data = null;

            try
            {
                var ro = new RepoOrder();
                var updated = ro.Updateproductqty(model);

                if (updated)
                {
                    data = new
                    {
                        Message = "Item Ouantity updated successfully!",
                        ResultFlag = 1
                    };
                    TempData["success"] = "Item Ouantity updated successfully!";
                }
                else
                {
                    data = new
                    {
                        Message = "Item Ouantity not updated.",
                        ResultFlag = 0
                    };
                    TempData["error"] = "Item Ouantity not updated.";
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

        public ActionResult DeleteFromOrder(int OrderdetailId)
        {
            object data = null;

            try
            {
                var ro = new RepoOrder();
                var updated = ro.DeleteProduct(OrderdetailId);

                if (updated)
                {
                    data = new
                    {
                        Message = "Item successfully Deleted!",
                        ResultFlag = 1
                    };
                    TempData["success"] = "Item successfully Deleted!";
                }
                else
                {
                    data = new
                    {
                        Message = "Item can not be Deleted!",
                        ResultFlag = 0
                    };
                    TempData["error"] = "Item can not be Deleted!";
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

        [CustomAuthorize("Order")]
        public ActionResult ListOrder()
        {
            string loginUserId = General.GetUserId();

            var sc = new SystemController();
            var rm = sc.GetOrderByUserId(new GetUserOrderModel { UserId = loginUserId });
            return View(rm.Data);
        }
        [HttpPost]
        public ActionResult ApplyPromoCode(PromoCodeModel model)
        {
            object data = null;
            var utils = new Utils();
            var tempOrderId = utils.getIntCookiesValue(Constants.TempOrderId);
            var sc = new SystemController();
            var rm = sc.ApplyPromoCode(new PromoCodeModel { TempOrderId = tempOrderId, CouponNo = model.CouponNo, UserId = null });
            data = new
            {
                rm.Message,
                rm.ResultFlag
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RemovePromocode(PromoCodeModel model)
        {
            object data = null;
            var utils = new Utils();
            var tempOrderId = utils.getIntCookiesValue(Constants.TempOrderId);
            var sc = new SystemController();
            var rm = sc.RemovePromoCode(new PromoCodeModel { TempOrderId = tempOrderId, CouponNo = model.CouponNo, UserId = null });
            data = new
            {
                rm.Message,
                rm.ResultFlag
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Unauthorize

        public ActionResult UnAuthorize()
        {
            return View();
        }

        #endregion

        #region Change Password
        //public ActionResult ChangePassword(string Id, string FC, string FA)
        //{
        //    RepoUsers repoUsers = new RepoUsers();
        //    var data = new ChangePasswordModel() { UserId = Id, Controller = FC, Action = FA };
        //    return View(data);
        //}
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            SystemController systemController = new SystemController();
            var data = systemController.ChangePassword(new clsApproveUser()
            {
                UserId = model.UserId,
                NewPassword = model.NewPassword
            });

            return Json(data, JsonRequestBehavior.AllowGet);
            //if (data.ResultFlag == 1)
            //{
            //    //return RedirectToAction(model.Action, model.Controller);
            //}
            //return View(model);
        }
        #endregion

        #region Show all brand
        [Authorize(Roles = "Workshop,DistributorUsers")]
        [CustomAuthorize("Order")]
        public ActionResult Testcart()
        {
            var sc = new SystemController();
            ResultModel rm = sc.GetAllBrand(new clsBrand { SearchString = "" });
            return View(rm.Data);
            //return View("Oriparts");
        }
        #endregion

        #region Order summary
        public ActionResult CartPartialView()
        {
            var utils = new Utils();
            var sc = new SystemController();
            var rm = sc.GetCart(new GetCartModel { TempOrderId = utils.getIntCookiesValue(Constants.TempOrderId), UserId = General.GetUserId() });
            return PartialView(rm.Data);
        }
        #endregion

        #region Generate order per Outlet
        public ActionResult GenerateOrder(ResponseOrderModel model)
        {
            object data = null;
            try
            {
                var ro = new RepoOrder();
                ro.GenerateOrderPerOutlet(model);

                data = new
                {
                    Message = "Generated order per outlet successfully!",
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

        #region Get All Product
        [HttpPost]
        public string GetAllProduct(int DistributorID, string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {

            DataTableRequest request = new DataTableRequest();
            request.search = sSearch;
            request.length = iDisplayLength;
            request.start = iDisplayStart;
            request.column = sEcho;
            try
            {
                ProductPagePermission obj = new ProductPagePermission();
                return obj.GetAllProducts(DistributorID, sEcho, iDisplayStart, iDisplayLength, sSearch);
                //var data = obj.GetAllProduct(request);
                //return Json(new { draw = data.draw, recordsFiltered = data.recordsFiltered, recordsTotal = data.recordsTotal, data = data.data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return "";
            }
        }
        #endregion

        #region Dashboard - Sales Growth
        public ActionResult SalesGrowth()
        {
            // Get product's groups
            string userId = User.Identity.GetUserId();
            SystemController systemController = new SystemController();
            string role = systemController.GetUserRole();
            var rd = new RepoDashboard();
            ViewBag.ProdGroups = rd.ProductGroupSelectList(userId, role);

            return View();
        }
        #endregion

        #region User Profile
        public ActionResult ProfileDetail()
        {
            SystemController systemController = new SystemController();
            var data = systemController.UserProfile();
            return View(data.Data);
        }
        public new ActionResult Profile()
        {

            SystemController systemController = new SystemController();
            var data = systemController.UserProfile();
            return View(data.Data);
        }

        [HttpPost]
        public new ActionResult Profile(ResponseUserProfile model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                if (model.Role == Constants.Workshop || model.Role == Constants.WorkshopUsers)
                {
                    if (string.IsNullOrEmpty(model.WorkShopName))
                    {
                        ModelState.AddModelError("WorkShopName", "WorkShopName required.");
                        return View(model);
                    }

                }
                try
                {
                    SystemController systemController = new SystemController();
                    var result = systemController.UpdateUserProfile(model);
                    if (result.ResultFlag == 1)
                    {
                        TempData["Success"] = result.Message;
                        return RedirectToAction("ProfileDetail");
                    }
                    else
                    {
                        TempData["error"] = result.Message;
                    }
                }
                catch (Exception exc)
                {
                    TempData["error"] = "Record Not Updated";
                    RepoUserLogs.LogException(exc);
                }
                return View(model);
            }
        }
        #endregion

        #region Change Mobile Number
        [HttpPost]
        public ActionResult ChangeMobileNumber(ChangeMobileModel model)
        {
            SystemController systemController = new SystemController();
            var data = systemController.ChangePhoneNumber(new ChangePhoneNumberModel()
            {
                PhoneNumber = model.NewMobileNumber
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region DistributorOutlet
        [HttpPost]
        public ActionResult DistributorOutlet(int distributorId)
        {
            var ro = new RepoOutlet();
            var data = ro.GetOutlets(distributorId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpPost]
        public ActionResult DistributorBrands(int distributorId)
        {
            var vpp = new VehiclePagePermission();
            var data = vpp.DistributorBrands(distributorId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region Wallet 
        [HttpPost]
        public ActionResult AddMoneyInWallet(AddMoneyRequest model)
        {
            SystemController controller = new SystemController();
            return Json(controller.AddMoneyInWallet(model), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult WorkshopCoupons(WorkshopCouponRequest model)
        {
            SystemController controller = new SystemController();
            return Json(controller.GetWorkshopCoupons(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GenerateCoupon(GenerateCouponRequest model)
        {
            SystemController controller = new SystemController();
            return Json(controller.GenerateCoupon(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult WorkshopTransaction(WsTransactionRequest model)
        {
            SystemController controller = new SystemController();
            return Json(controller.GetWorkshopTransaction(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SearchCouponNumber(string couponNumber = null)
        {
            RepoWallet rw = new RepoWallet();
            return Json(rw.SearchCouponNo(couponNumber), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RedeemCouponNo(string couponNumber = null)
        {
            RepoWallet rw = new RepoWallet();
            string msg = string.Empty;
            return Json(rw.RedeemCouponNo(couponNumber, out msg), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region GetNotification Count 
        public ActionResult GetNotification(ClsNotification model)
        {
            SystemController sc = new SystemController();
            List<NotificationData> notificationData = new List<NotificationData>();
            notificationData = (List<NotificationData>)sc.GetNotifications(model).Data;
            int result = (from n in notificationData
                          where n.IsRead == false
                          select n).Count();
            return Json(result.ToString(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        [Route("widget")]
        public ActionResult OripartsWidgets(string userId = null, int? temporderid = null)
        {
            ViewBag.UserId = userId ?? string.Empty;
            ViewBag.temporderid = temporderid ?? 0;
            return View();
        }

        #region Get Distributor Lastuploadfile

        public ActionResult GetLastUploadFileDate(int distributorId, string filetype)
        {
            GeneralUse cls = new GeneralUse();
            var lastupdate = cls.GetDistributorFileUploadDate(distributorId, filetype);
            return Json(lastupdate, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Distributor GrowthPercentage

        public ActionResult GetGrowthPercentage(int distributorId)
        {
            GrowthPercentageModel model = new GrowthPercentageModel();
            model.DistributorId = distributorId;
            ClsGrowthPercentage cls = new ClsGrowthPercentage();
            var data = cls.GetGrowthPercentage(model);
            if (data != null)
            {
                model.GrowthPercentage = Convert.ToDecimal(data.GrowthPercentage1);
                model.MinValue = Convert.ToDecimal(data.MinValue);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Workshop Set Credit Limit
        [HttpPost]
        public ActionResult SetCreditLimit()
        {
            object data;
            try
            {
                var rc = new RepoCustomers();
                var isPopulated = rc.SetAllWorkshopsCreditLimit();

                var message = isPopulated ? "Credit limit was set successfully!" : "Failed to set credit limit.";
                data = new
                {
                    Message = message,
                    ResultFlag = isPopulated,
                    Data = ""
                };
            }
            catch (Exception exc)
            {
                data = new
                {
                    exc.Message,
                    ResultFlag = 0,
                    Data = ""
                };
                RepoUserLogs.LogException(exc);

            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region DistributorSchemes
        [HttpPost]
        public ActionResult DistributorSchemes(int distributorId)
        {

            RepoSchemes schemes = new RepoSchemes();
            var data = schemes.GetAllScheme(distributorId);
            return Json(new ResultModel()
            {
                Message = data != null ? "Schemes find successfully." : "Schemes not find.",
                ResultFlag = data != null ? 1 : 0,
                Data = data
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Show Daily Sales Tracker With Invoice
        public ActionResult LoadDailySales(int distributorId, string startDate, string endDate, string fromAmt, string toAmt, int workshopId, int draw, int start, int length)
        // public ActionResult LoadDailySales(int draw, int start, int length)
        {
            List<DailySalesInvoiceModel> data = null;
            var recordsTotal = 0;

            try
            {
                // Find Order Column and order direction
                var sortByColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault();
                var sortColumnDirection = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                var pageSize = Convert.ToInt32(length);
                var skip = Convert.ToInt32(start);

                var rc = new RepoCustomers();
                data = rc.GetSalesData(sortByColumn, sortColumnDirection, skip, pageSize, draw, distributorId, startDate, endDate, fromAmt, toAmt, workshopId, out recordsTotal);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Dashboard Selected Date Set In Session
        [HttpPost]
        public void SetSessionValue(string parameter, string value)
        {
            HttpContext.Session[parameter] = value;//Custom           
        }

        [HttpPost]
        public ActionResult GetSessionValue(string parameter)
        {
            var value = HttpContext.Session[parameter];
            return Json(value != null ? value : "", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region OrderDetails Export To Excel
        [HttpPost]
        public ActionResult OrderDetailsExportToExcel(int orderId, string userId)
        {
            RepoOrder repoOrder = new RepoOrder();
            var data = repoOrder.GetOrderPartsByOrderId(new GetOrderModel { OrderId = orderId, UserId = userId });
            if (data.Count == 0)
            {
                TempData["error"] = "parts not found.";
                return Redirect(Request.UrlReferrer.ToString());
            }

            var sb = new StringBuilder();

            sb.AppendFormat("{0},{1},{2},{3}", "PartNumber", "Quantity", "OrderNumber ", "CustomerName");
            sb.Append(Environment.NewLine);
            foreach (var item in data)
            {
                sb.AppendFormat("{0},{1},{2},{3}", item.PartNumber, item.Quantity, item.OrderNumber, item.CustomerName);
                sb.Append(Environment.NewLine);
            }
            //Get Current Response  
            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=OrderParts.CSV ");
            response.ContentType = "text/plain";
            response.Write(sb.ToString());
            response.End();

            return Json(new { fileName = response });
        }
        #endregion

        #region CustomerBack-Orders Export To Excel
        [HttpPost]
        public ActionResult BackOrdersExportToExcel(int? distributorId)
        {
            try
            {
                BackOrder cbo = new BackOrder();
                var customerBackOrders = cbo.GetCboByDistributor(distributorId.Value);

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