using Garaaz.Models;
using Garaaz.Models.DashboardOverview;
using Garaaz.Models.DashboardOverview.Cbo;
using Garaaz.Models.DashboardOverview.Collection;
using Garaaz.Models.DashboardOverview.Customer;
using Garaaz.Models.DashboardOverview.DataTables;
using Garaaz.Models.DashboardOverview.Inventory;
using Garaaz.Models.DashboardOverview.LoserAndGainer;
using Garaaz.Models.DashboardOverview.Outstanding;
using Garaaz.Models.DashboardOverview.Sale;
using Garaaz.Models.DashboardOverview.Scheme;
using Garaaz.Models.DashboardOverview.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Garaaz.Controllers.Dashboard
{
    public class BaseDbController : Controller
    {
        private static bool _isFromMobile;

        public BaseDbController(bool isFromMobile)
        {
            _isFromMobile = isFromMobile;
        }

        #region Dashboard : Main
        public ActionResult FetchSaleInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            SaleInfoResponse saleInfo = null;
            try
            {
                var dbFilter = new DashboardFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var saleMain = new SaleMain();
                saleInfo = saleMain.GetSalesInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_SalesInfo.cshtml", saleInfo);
        }

        public ActionResult FetchOutstandingInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            OsInfoResponse osInfo = null;
            try
            {
                var dbFilter = new DashboardFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var osMain = new OutstandingMain();
                osInfo = osMain.GetOutstandingInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Outstanding/_OutstandingInfo.cshtml", osInfo);
        }

        public ActionResult FetchCollectionInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            ColInfoResponse colInfo = null;
            try
            {
                var dbFilter = new DashboardFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var colMain = new CollectionMain();
                colInfo = colMain.GetCollectionInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Collection/_CollectionInfo.cshtml", colInfo);
        }

        public ActionResult FetchInventoryInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            InvInfoResponse inventoryInfo = null;
            try
            {
                var dbFilter = new DashboardFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var invMain = new InventoryMain();
                inventoryInfo = invMain.GetInventoryInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Inventory/_InventoryInfo.cshtml", inventoryInfo);
        }

        public ActionResult FetchCboInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            CboInfoResponse cboInfo = null;
            try
            {
                var dbFilter = new CboDbFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var cboMain = new CboMain();
                cboInfo = cboMain.GetCboInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_CboInfo.cshtml", cboInfo);
        }

        public ActionResult FetchSchemeInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            SchemeInfoResponse schemeInfo = null;
            try
            {
                var dbFilter = new DashboardFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var schemeMain = new Models.DashboardOverview.Scheme.SchemeMain();
                schemeInfo = schemeMain.GetSchemesInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Scheme/_SchemeInfo.cshtml", schemeInfo);
        }

        public ActionResult FetchWalletBalanceInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            WalletInfoResponse walletInfo = null;
            try
            {
                var dbFilter = new WalletDbFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var walMain = new WalletMain();
                walletInfo = walMain.GetWalletBalanceInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Wallet/_WalletBalanceInfo.cshtml", walletInfo);
        }

        public ActionResult FetchCustomerInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            CustomerInfoResponse customerInfo = null;
            try
            {
                var dbFilter = new CustomerDbFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var cusMain = new CustomerMain();
                customerInfo = cusMain.GetCustomerInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Customer/_CustomerInfo.cshtml", customerInfo);
        }

        public ActionResult FetchLoserAndGainerInfo(string dateType, DateTime? startDate, DateTime? endDate)
        {
            LoserAndGainerInfoResponse lgInfo = null;
            try
            {
                var dbFilter = new LooserGainerFilter
                {
                    UserId = General.GetUserId(),
                    Roles = new List<string> { General.GetUserRole(_isFromMobile) },
                    DateType = dateType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var lgMain = new LoserGainerMain();
                lgInfo = lgMain.GetLoserAndGainerInfo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/LoserAndGainer/_LoserAndGainerInfo.cshtml", lgInfo);
        }

        #endregion

        #region Sales detail

        public ActionResult SalesDetail()
        {
            return View("~/Views/Dashboard/Sale/SalesDetail.cshtml");
        }

        public ActionResult FetchSalesByCategory(SaleDbFilter dbFilter)
        {
            List<SaleInfoResponse> sales = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                sales = saleMain.GetCategoryWiseSale(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_SalesByCategory.cshtml", sales);
        }

        public ActionResult FetchSalesBySubGroup(SaleDbFilter dbFilter)
        {
            List<SaleInfoResponse> sales = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                sales = saleMain.GetSubGroupWiseSale(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_SalesBySubGroup.cshtml", sales);
        }

        #region RO wise sale detail

        public ActionResult FetchRoWiseSaleDetail(SaleDbFilter dbFilter)
        {
            List<RoWiseSaleDetail> rowSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                rowSaleDetails = saleMain.GetRoWiseSaleDetails(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_RoWiseSalesDetail.cshtml", rowSaleDetails);
        }

        public ActionResult FetchRoWiseBranchSaleDetail(SaleDbFilter dbFilter)
        {
            List<RoWiseBranchSaleDetail> rowBranchSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                rowBranchSaleDetails = saleMain.GetRoWiseBranchSaleDetails(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_RoWiseBranchSalesDetail.cshtml", rowBranchSaleDetails);
        }

        public ActionResult FetchRoWiseBranchCustomerSaleDetail(SaleDbFilter dbFilter)
        {
            List<RoWiseSaleDetail> rowSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                rowSaleDetails = saleMain.GetRoWiseSaleDetails(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }

            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_RoWiseBranchCustomerSalesDetail.cshtml", rowSaleDetails);
        }

        [HttpPost]
        public ActionResult FetchRoWiseCustomerSaleDetail(SaleDbFilter dbFilter, JqDataTableRequest request)
        {
            List<RoWiseCustomerSaleDetail> rowCustomerSaleDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var saleMain = new SaleMain();
                rowCustomerSaleDetails = saleMain.GetRoWiseCustomerSaleDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                rowCustomerSaleDetails = new List<RoWiseCustomerSaleDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<RoWiseCustomerSaleDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = rowCustomerSaleDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SE (Sales Executive) wise sale detail

        public ActionResult FetchSeWiseSaleDetail(SaleDbFilter dbFilter)
        {
            List<SeWiseSaleDetail> sewSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                sewSaleDetails = saleMain.GetSeWiseSaleDetails(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_SeWiseSalesDetail.cshtml", sewSaleDetails);
        }

        public ActionResult FetchSeWiseBranchSaleDetail(SaleDbFilter dbFilter)
        {
            List<SeWiseBranchSaleDetail> sewBranchSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                sewBranchSaleDetails = saleMain.GetSeWiseBranchSaleDetails(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_SeWiseBranchSalesDetail.cshtml", sewBranchSaleDetails);
        }

        public ActionResult FetchSeWiseBranchCustomerSaleDetail(SaleDbFilter dbFilter)
        {
            List<SeWiseSaleDetail> sewSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                sewSaleDetails = saleMain.GetSeWiseSaleDetails(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }

            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_SeWiseBranchCustomerSalesDetail.cshtml", sewSaleDetails);
        }

        [HttpPost]
        public ActionResult FetchSeWiseCustomerSaleDetail(SaleDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerSaleDetail> sewCustomerSaleDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var saleMain = new SaleMain();
                sewCustomerSaleDetails = saleMain.GetSeWiseCustomerSaleDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                sewCustomerSaleDetails = new List<CustomerSaleDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerSaleDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = sewCustomerSaleDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region CS (Customer Segment) wise sale detail

        public ActionResult FetchCsWiseSaleDetail(SaleDbFilter dbFilter)
        {
            List<CsWiseSaleDetail> cswSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                cswSaleDetails = saleMain.GetCsWiseSaleDetails(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_CsWiseSalesDetail.cshtml", cswSaleDetails);
        }

        [HttpPost]
        public ActionResult FetchCsWiseCustomerSaleDetail(SaleDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerSaleDetail> cswCustomerSaleDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var saleMain = new SaleMain();
                cswCustomerSaleDetails = saleMain.GetCsWiseCustomerSaleDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                cswCustomerSaleDetails = new List<CustomerSaleDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerSaleDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = cswCustomerSaleDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region PG (Part Group) wise sale detail

        public ActionResult FetchPgWiseSaleDetail(SaleDbFilter dbFilter)
        {
            List<PgWiseSaleDetail> pgwSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                pgwSaleDetails = saleMain.GetPgWiseSaleDetails(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_PgWiseSalesDetail.cshtml", pgwSaleDetails);
        }

        public ActionResult FetchPgWisePartGroupSaleDetail(SaleDbFilter dbFilter)
        {
            List<PgWiseCustomerSaleDetail> pgwCustomerSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                pgwCustomerSaleDetails = saleMain.GetPgWiseCustomerSaleDetail(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_PgWisePartGroupSalesDetail.cshtml", pgwCustomerSaleDetails);
        }

        public ActionResult FetchPgWiseBranchSaleDetail(SaleDbFilter dbFilter)
        {
            List<PgWiseBranchSaleDetail> pgwCustomerSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                pgwCustomerSaleDetails = saleMain.GetPgWiseBranchSaleDetail(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_PgWiseBranchSalesDetail.cshtml", pgwCustomerSaleDetails);
        }

        public ActionResult FetchPgWiseBranchCustomerSaleDetail(SaleDbFilter dbFilter)
        {
            List<PgWiseCustomerSaleDetail> pgwCustomerSaleDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var saleMain = new SaleMain();
                pgwCustomerSaleDetails = saleMain.GetPgWiseCustomerSaleDetail(dbFilter);

                ViewBag.PrvYrHeader = $"PY {dbFilter.DateType}";
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Sale/_PgWiseBranchCustomerSalesDetail.cshtml", pgwCustomerSaleDetails);
        }

        [HttpPost]
        public ActionResult FetchPgWiseCustomerSaleDetail(SaleDbFilter dbFilter, JqDataTableRequest request)
        {
            List<PgCustomerSaleDetail> pgwCustomerSaleDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var saleMain = new SaleMain();
                pgwCustomerSaleDetails = saleMain.GetPgWiseCustomerSaleDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                pgwCustomerSaleDetails = new List<PgCustomerSaleDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<PgCustomerSaleDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = pgwCustomerSaleDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region Outstanding detail

        public ActionResult OutstandingDetail()
        {
            return View("~/Views/Dashboard/Outstanding/OutstandingDetail.cshtml");
        }

        public ActionResult FetchOsBySubGroup(DashboardFilter dbFilter)
        {
            List<OsInfoResponse> outstandings = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var osMain = new OutstandingMain();
                outstandings = osMain.GetSubGroupWiseOutstanding(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Outstanding/_OsBySubGroup.cshtml", outstandings);
        }

        #region RO wise outstanding detail

        public ActionResult FetchRoWiseOsDetail(DashboardFilter dbFilter)
        {
            List<OsCustDetail> osCustDetails = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var osMain = new OutstandingMain();
                osCustDetails = osMain.GetRoWiseOsDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Outstanding/_RoWiseOsDetail.cshtml", osCustDetails);
        }

        public ActionResult FetchRoWiseOsBranchDetail(DashboardFilter dbFilter)
        {
            List<BranchOsDetail> bOsDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var osMain = new OutstandingMain();
                bOsDetails = osMain.GetRoWiseOsBranchDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("/Views/Dashboard/Outstanding/_RoWiseOsBranchDetail.cshtml", bOsDetails);
        }

        public ActionResult FetchRoWiseOsDetailByBranch(DashboardFilter dbFilter)
        {
            List<OsCustDetail> osCustDetails = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var osMain = new OutstandingMain();
                osCustDetails = osMain.GetRoWiseOsDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Outstanding/_RoWiseOsDetailByBranch.cshtml", osCustDetails);
        }

        [HttpPost]
        public ActionResult FetchRoWiseCustomerOsDetail(DashboardFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerOsDetail> outstandingDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var osMain = new OutstandingMain();
                outstandingDetails = osMain.GetRoWiseCustomerOsDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                outstandingDetails = new List<CustomerOsDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerOsDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = outstandingDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SE wise outstanding detail

        public ActionResult FetchSeWiseOsDetail(DashboardFilter dbFilter)
        {
            List<OsCustDetail> osCustDetails = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var osMain = new OutstandingMain();
                osCustDetails = osMain.GetSeWiseOsDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Outstanding/_SeWiseOsDetail.cshtml", osCustDetails);
        }

        public ActionResult FetchSeWiseOsBranchDetail(DashboardFilter dbFilter)
        {
            List<SeWiseBranchOsDetail> bOsDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var osMain = new OutstandingMain();
                bOsDetails = osMain.GetSeWiseOsBranchDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("/Views/Dashboard/Outstanding/_SeWiseOsBranchDetail.cshtml", bOsDetails);
        }

        public ActionResult FetchSeWiseOsDetailByBranch(DashboardFilter dbFilter)
        {
            List<OsCustDetail> osCustDetails = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var osMain = new OutstandingMain();
                osCustDetails = osMain.GetSeWiseOsDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Outstanding/_SeWiseOsDetailByBranch.cshtml", osCustDetails);
        }

        [HttpPost]
        public ActionResult FetchSeWiseCustomerOsDetail(DashboardFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerOsDetail> outstandingDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var osMain = new OutstandingMain();
                outstandingDetails = osMain.GetSeWiseCustomerOsDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                outstandingDetails = new List<CustomerOsDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerOsDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = outstandingDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult FetchOsSpecificCustomer(string customerCode)
        {
            OsCustomer osCustomer = null;
            try
            {
                var osMain = new OutstandingMain();
                osCustomer = osMain.GetOsSpecificCustomer(customerCode);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Outstanding/_OsSpecificCustomer.cshtml", osCustomer);
        }

        #region CS wise outstanding detail

        public ActionResult FetchCsWiseOsDetail(DashboardFilter dbFilter)
        {
            List<OsCustDetail> osCustDeails = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var osMain = new OutstandingMain();
                osCustDeails = osMain.GetCsWiseOsDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Outstanding/_CsWiseOsDetail.cshtml", osCustDeails);
        }

        [HttpPost]
        public ActionResult FetchCsWiseCustomerOsDetail(DashboardFilter dbFilter, JqDataTableRequest request)
        {
            List<CsWiseCustomerOsDetail> outstandingDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var osMain = new OutstandingMain();
                outstandingDetails = osMain.GetCsWiseCustomerOsDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                outstandingDetails = new List<CsWiseCustomerOsDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CsWiseCustomerOsDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = outstandingDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region Collection detail

        public ActionResult CollectionDetail()
        {
            return View("~/Views/Dashboard/Collection/CollectionDetail.cshtml");
        }

        public ActionResult FetchColBySubGroup(ColDbFilter dbFilter)
        {
            List<ColInfoResponse> collections = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var colMain = new CollectionMain();
                collections = colMain.GetSubGroupWiseCollection(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Collection/_ColBySubGroup.cshtml", collections);
        }

        #region RO wise collection detail

        public ActionResult FetchRoWiseColDetail(ColDbFilter dbFilter)
        {
            List<ColDetail> rowColDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var colMain = new CollectionMain();
                rowColDetails = colMain.GetRoWiseCollections(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            var viewName = string.IsNullOrWhiteSpace(dbFilter.BranchCode)
                ? "~/Views/Dashboard/Collection/_ColDetail.cshtml"
                : "~/Views/Dashboard/Collection/_BranchColByType.cshtml";
            return PartialView(viewName, rowColDetails);
        }

        public ActionResult FetchRoWiseBranchColDetail(ColDbFilter dbFilter)
        {
            List<RoWiseBranchCol> rowBranchColDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var colMain = new CollectionMain();
                rowBranchColDetails = colMain.GetRoWiseBranchCollections(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Collection/_RoWiseBranchColDetail.cshtml", rowBranchColDetails);
        }

        [HttpPost]
        public ActionResult FetchRoWiseBranchCustomersCol(ColDbFilter dbFilter, JqDataTableRequest request)
        {
            List<Customer> rowBranchCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var colMain = new CollectionMain();
                rowBranchCustomers = colMain.GetRoWiseBranchCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                rowBranchCustomers = new List<Customer>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<Customer>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = rowBranchCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SE wise collection detail

        public ActionResult FetchSeWiseColDetail(ColDbFilter dbFilter)
        {
            List<ColDetail> sewColDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var colMain = new CollectionMain();
                sewColDetails = colMain.GetSeWiseCollections(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            var viewName = string.IsNullOrWhiteSpace(dbFilter.BranchCode)
                ? "~/Views/Dashboard/Collection/_ColDetail.cshtml"
                : "~/Views/Dashboard/Collection/_BranchColByType.cshtml";
            return PartialView(viewName, sewColDetails);
        }

        public ActionResult FetchSeWiseBranchColDetail(ColDbFilter dbFilter)
        {
            List<SeWiseBranchCol> sewBranchColDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var colMain = new CollectionMain();
                sewBranchColDetails = colMain.GetSeWiseBranchCollections(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Collection/_SeWiseBranchColDetail.cshtml", sewBranchColDetails);
        }

        [HttpPost]
        public ActionResult FetchSeWiseBranchCustomersCol(ColDbFilter dbFilter, JqDataTableRequest request)
        {
            List<Customer> sewBranchCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var colMain = new CollectionMain();
                sewBranchCustomers = colMain.GetSeWiseBranchCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                sewBranchCustomers = new List<Customer>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<Customer>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = sewBranchCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpPost]
        public ActionResult FetchColCustomerDetails(ColDbFilter dbFilter, JqDataTableRequest request)
        {
            List<ColCustomerDetail> customers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var colMain = new CollectionMain();
                customers = colMain.GetCustomerDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                customers = new List<ColCustomerDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<ColCustomerDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = customers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #region CS wise collection detail

        public ActionResult FetchCsWiseColDetail(ColDbFilter dbFilter)
        {
            List<ColDetail> cswColDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var colMain = new CollectionMain();
                cswColDetails = colMain.GetCsWiseCollections(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Collection/_ColDetail.cshtml", cswColDetails);
        }

        [HttpPost]
        public ActionResult FetchCsWiseCustomersCol(ColDbFilter dbFilter, JqDataTableRequest request)
        {
            List<Customer> cswCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var colMain = new CollectionMain();
                cswCustomers = colMain.GetCsWiseCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                cswCustomers = new List<Customer>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<Customer>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = cswCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region Inventory detail

        public ActionResult InventoryDetail()
        {
            return View("~/Views/Dashboard/Inventory/InventoryDetail.cshtml");
        }

        public ActionResult FetchInvByBranch(DashboardFilter dbFilter)
        {
            List<InvDetail> inventories = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var invMain = new InventoryMain();
                inventories = invMain.GetInvDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("/Views/Dashboard/Inventory/_InvByBranch.cshtml", inventories);
        }

        [HttpPost]
        public ActionResult FetchInvForBranch(DashboardFilter dbFilter, JqDataTableRequest request)
        {
            List<InvDetailForBranch> bpgInventories;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var invMain = new InventoryMain();
                bpgInventories = invMain.GetBranchWiseInvDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                bpgInventories = new List<InvDetailForBranch>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<InvDetailForBranch>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = bpgInventories,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Cbo detail

        public ActionResult CboDetail()
        {
            return View("~/Views/Dashboard/Cbo/CboDetail.cshtml");
        }

        public ActionResult FetchCboBySubGroup(CboDbFilter dbFilter)
        {
            List<CboInfoResponse> cbos = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                cbos = cboMain.GetSubGroupWiseCbo(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_CboBySubGroup.cshtml", cbos);
        }

        #region RO wise CBO detail

        public ActionResult FetchRoWiseCboDetail(CboDbFilter dbFilter)
        {
            List<CategoryWiseCboDetail> rowCboDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                rowCboDetails = cboMain.GetRoWiseCboDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_RoWiseCboDetail.cshtml", rowCboDetails);
        }

        public ActionResult FetchRoWiseBranchCboDetail(CboDbFilter dbFilter)
        {
            List<CategoryWiseBranchCboDetail> rowBranchCboDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                rowBranchCboDetails = cboMain.GetRoWiseBranchCboDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_RoWiseBranchCboDetail.cshtml", rowBranchCboDetails);
        }

        public ActionResult FetchRoWiseBranchCustomerCboDetail(CboDbFilter dbFilter)
        {
            List<CategoryWiseCboDetail> rowBcCboDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                rowBcCboDetails = cboMain.GetRoWiseBranchCustomerCboDetail(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_RoWiseBranchCustomerCboDetail.cshtml", rowBcCboDetails);
        }

        [HttpPost]
        public ActionResult FetchRoWiseCustomerCboDetail(CboDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CategoryWiseCustomerCboDetail> rowCustCboDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var cboMain = new CboMain();
                rowCustCboDetails = cboMain.GetRoWiseCustomerCboDetail(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                rowCustCboDetails = new List<CategoryWiseCustomerCboDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CategoryWiseCustomerCboDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = rowCustCboDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchRoWiseCustomerDetail(CboDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CboCustomerDetail> customerDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var cboMain = new CboMain();
                customerDetails = cboMain.GetRoWiseCustomerDetail(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                customerDetails = new List<CboCustomerDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CboCustomerDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = customerDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FetchRoWiseCustomerPartsDetail(CboDbFilter dbFilter)
        {
            List<CustomerPartDetail> customerPartsDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                customerPartsDetails = cboMain.GetRoWiseCustomerPartsDetails(dbFilter);

            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_RoWiseCustomerPartsDetail.cshtml", customerPartsDetails);
        }

        #endregion

        #region SE wise CBO detail

        public ActionResult FetchSeWiseCboDetail(CboDbFilter dbFilter)
        {
            List<CategoryWiseCboDetail> sewCboDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                sewCboDetails = cboMain.GetSeWiseCboDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_SeWiseCboDetail.cshtml", sewCboDetails);
        }

        public ActionResult FetchSeWiseBranchCboDetail(CboDbFilter dbFilter)
        {
            List<CategoryWiseBranchCboDetail> sewBranchCboDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                sewBranchCboDetails = cboMain.GetSeWiseBranchCboDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_SeWiseBranchCboDetail.cshtml", sewBranchCboDetails);
        }

        public ActionResult FetchSeWiseBranchCustomerCboDetail(CboDbFilter dbFilter)
        {
            List<CategoryWiseCboDetail> sewBcCboDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                sewBcCboDetails = cboMain.GetSeWiseBranchCustomerCboDetail(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_SeWiseBranchCustomerCboDetail.cshtml", sewBcCboDetails);
        }

        [HttpPost]
        public ActionResult FetchSeWiseCustomerCboDetail(CboDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CategoryWiseCustomerCboDetail> sewCustCboDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var cboMain = new CboMain();
                sewCustCboDetails = cboMain.GetSeWiseCustomerCboDetail(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                sewCustCboDetails = new List<CategoryWiseCustomerCboDetail>();
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CategoryWiseCustomerCboDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = sewCustCboDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchSeWiseCustomerDetail(CboDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CboCustomerDetail> customerDetails;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var cboMain = new CboMain();
                customerDetails = cboMain.GetSeWiseCustomerDetail(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                customerDetails = new List<CboCustomerDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CboCustomerDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = customerDetails,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FetchSeWiseCustomerPartsDetail(CboDbFilter dbFilter)
        {
            List<CustomerPartDetail> customerPartsDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cboMain = new CboMain();
                customerPartsDetails = cboMain.GetSeWiseCustomerPartsDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Cbo/_SeWiseCustomerPartsDetail.cshtml", customerPartsDetails);
        }

        #endregion

        #region CBO Export To Excel
        [HttpPost]
        public ActionResult CBOExportToExcel()
        {
            var UserId = General.GetUserId();
            var Role = General.GetUserRole(_isFromMobile);
            RepoOrder repoOrder = new RepoOrder();
            var orders = repoOrder.GetConfirmOrCancelBackOrderByRole(UserId, Role);
            if (orders.Count == 0)
            {
                TempData["error"] = "Orders not found.";
                return RedirectToAction("CboDetail", "Dashboard");
            }
            var gv = new GridView();
            gv.DataSource = orders;
            gv.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=CboOrders.xls");
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

        #region Scheme detail

        public ActionResult SchemeDetail()
        {
            return RedirectToAction("Schemes", General.IsSuperAdmin() ? "Admin" : "Distributor");
        }

        #endregion

        #region Wallet detail

        public ActionResult WalletDetail()
        {
            return View("~/Views/Dashboard/Wallet/WalletDetail.cshtml");
        }

        public ActionResult FetchWalletBySubGroup(WalletDbFilter dbFilter)
        {
            List<WalletInfoResponse> wallets = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var walletMain = new WalletMain();
                wallets = walletMain.GetSubGroupWiseWallets(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Wallet/_WalletBySubGroup.cshtml", wallets);
        }

        #region RO wise Wallet detail

        public ActionResult FetchRoWiseWalletDetail(WalletDbFilter dbFilter)
        {
            List<WalletDetail> walletDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var walletMain = new WalletMain();
                walletDetails = walletMain.GetRoWiseWallets(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            var viewName = string.IsNullOrWhiteSpace(dbFilter.BranchCode)
                ? "~/Views/Dashboard/Wallet/_WalletDetail.cshtml"
                : "~/Views/Dashboard/Wallet/_BranchWalletByType.cshtml";

            return PartialView(viewName, walletDetails);
        }

        public ActionResult FetchRoWiseBranchWalletDetail(WalletDbFilter dbFilter)
        {
            List<RoWiseBranchWallet> rowBranchWalletDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var walletMain = new WalletMain();
                rowBranchWalletDetails = walletMain.GetRoWiseBranchWallets(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Wallet/_RoWiseBranchWalletDetail.cshtml", rowBranchWalletDetails);
        }

        [HttpPost]
        public ActionResult FetchRoWiseBranchCustomersWallet(WalletDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWallet> customerWallets;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var walletMain = new WalletMain();
                customerWallets = walletMain.GetRoWiseBranchCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                customerWallets = new List<CustomerWallet>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWallet>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = customerWallets,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SE wise Wallet detail

        public ActionResult FetchSeWiseWalletDetail(WalletDbFilter dbFilter)
        {
            List<WalletDetail> walletDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var walletMain = new WalletMain();
                walletDetails = walletMain.GetSeWiseWallets(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            var viewName = string.IsNullOrWhiteSpace(dbFilter.BranchCode)
                ? "~/Views/Dashboard/Wallet/_WalletDetail.cshtml"
                : "~/Views/Dashboard/Wallet/_BranchWalletByType.cshtml";

            return PartialView(viewName, walletDetails);
        }

        public ActionResult FetchSeWiseBranchWalletDetail(WalletDbFilter dbFilter)
        {
            List<SeWiseBranchWallet> sewBranchWalletDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var walletMain = new WalletMain();
                sewBranchWalletDetails = walletMain.GetSeWiseBranchWallets(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Wallet/_SeWiseBranchWalletDetail.cshtml", sewBranchWalletDetails);
        }

        [HttpPost]
        public ActionResult FetchSeWiseBranchCustomersWallet(WalletDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWallet> customerWallets;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var walletMain = new WalletMain();
                customerWallets = walletMain.GetSeWiseBranchCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                customerWallets = new List<CustomerWallet>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWallet>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = customerWallets,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region CS wise Wallet detail

        public ActionResult FetchCsWiseWalletDetail(WalletDbFilter dbFilter)
        {
            List<WalletDetail> walletDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var walletMain = new WalletMain();
                walletDetails = walletMain.GetCsWiseWallets(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Wallet/_WalletDetail.cshtml", walletDetails);
        }

        [HttpPost]
        public ActionResult FetchCsWiseCustomersWallet(WalletDbFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWallet> customerWallets;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var walletMain = new WalletMain();
                customerWallets = walletMain.GetCsWiseCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                customerWallets = new List<CustomerWallet>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWallet>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = customerWallets,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpPost]
        public ActionResult FetchWalCustomerDetails(WalletDbFilter dbFilter, JqDataTableRequest request)
        {
            List<WalCustomerDetail> walCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var walMain = new WalletMain();
                walCustomers = walMain.GetCustomerDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                walCustomers = new List<WalCustomerDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<WalCustomerDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = walCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Customer detail

        public ActionResult CustomerDetail()
        {
            return View("~/Views/Dashboard/Customer/CustomerDetail.cshtml");
        }

        public ActionResult FetchCustomerBySubGroup(CustomerDbFilter dbFilter)
        {
            List<CustomerInfoResponse> customers = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cusMain = new CustomerMain();
                customers = cusMain.GetSubGroupWiseCustomers(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Customer/_CustomerBySubGroup.cshtml", customers);
        }

        #region RO wise customer detail

        public ActionResult FetchRoWiseCustomerDetail(CustomerDbFilter dbFilter)
        {
            List<CustomerDetail> customerDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cusMain = new CustomerMain();
                customerDetails = cusMain.GetRoWiseCustomers(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Customer/_CustomerDetail.cshtml", customerDetails);
        }

        public ActionResult FetchRoWiseBranchCustomerDetail(CustomerDbFilter dbFilter)
        {
            List<RoWiseBranchCustomer> rowBranchCustomers = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cusMain = new CustomerMain();
                rowBranchCustomers = cusMain.GetRoWiseBranchCustomers(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Customer/_RoWiseBranchCustomerDetail.cshtml", rowBranchCustomers);
        }

        [HttpPost]
        public ActionResult FetchRoWiseBilledCustomers(CustomerDbFilter dbFilter, JqDataTableRequest request)
        {
            List<BranchCustomerDetail> billedCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var cusMain = new CustomerMain();
                billedCustomers = cusMain.GetRoWiseBilledCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                billedCustomers = new List<BranchCustomerDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<BranchCustomerDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = billedCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchRoWiseNonBilledCustomers(CustomerDbFilter dbFilter, JqDataTableRequest request)
        {
            List<BranchCustomerDetail> nonBilledCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var cusMain = new CustomerMain();
                nonBilledCustomers = cusMain.GetRoWiseNonBilledCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                nonBilledCustomers = new List<BranchCustomerDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<BranchCustomerDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = nonBilledCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SE wise customer detail

        public ActionResult FetchSeWiseCustomerDetail(CustomerDbFilter dbFilter)
        {
            List<CustomerDetail> customerDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cusMain = new CustomerMain();
                customerDetails = cusMain.GetSeWiseCustomers(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Customer/_CustomerDetail.cshtml", customerDetails);
        }

        public ActionResult FetchSeWiseBranchCustomerDetail(CustomerDbFilter dbFilter)
        {
            List<SeWiseBranchCustomer> sewBranchCustomers = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var cusMain = new CustomerMain();
                sewBranchCustomers = cusMain.GetSeWiseBranchCustomers(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/Customer/_SeWiseBranchCustomerDetail.cshtml", sewBranchCustomers);
        }

        [HttpPost]
        public ActionResult FetchSeWiseBilledCustomers(CustomerDbFilter dbFilter, JqDataTableRequest request)
        {
            List<BranchCustomerDetail> billedCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var cusMain = new CustomerMain();
                billedCustomers = cusMain.GetSeWiseBilledCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                billedCustomers = new List<BranchCustomerDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<BranchCustomerDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = billedCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchSeWiseNonBilledCustomers(CustomerDbFilter dbFilter, JqDataTableRequest request)
        {
            List<BranchCustomerDetail> nonBilledCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var cusMain = new CustomerMain();
                nonBilledCustomers = cusMain.GetSeWiseNonBilledCustomers(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                nonBilledCustomers = new List<BranchCustomerDetail>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<BranchCustomerDetail>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = nonBilledCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region Loosers & Gainers detail

        public ActionResult LooserAndGainersDetail()
        {
            return View("~/Views/Dashboard/LoserAndGainer/LooserAndGainersDetail.cshtml");
        }

        public ActionResult FetchLooserAndGainersBySubGroup(LooserGainerFilter dbFilter)
        {
            List<LoserAndGainerInfoResponse> looserGainers = null;
            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var lgMain = new LoserGainerMain();
                looserGainers = lgMain.GetSubGroupWiseLooserAndGainers(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/LoserAndGainer/_LooserAndGainersBySubGroup.cshtml", looserGainers);
        }

        #region RO wise Loosers & Gainers detail

        public ActionResult FetchRoWiseLGDetail(LooserGainerFilter dbFilter)
        {
            List<LooserAndGainersDetails> lGDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var lgMain = new LoserGainerMain();
                lGDetails = lgMain.RoWiseLGDetailS(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }
            return PartialView("~/Views/Dashboard/LoserAndGainer/_LGCustomerTypeDetails.cshtml", lGDetails);
        }

        public ActionResult FetchRoWiseBranchLGDetail(LooserGainerFilter dbFilter)
        {
            List<BranchWiseLGInfo> rowBranchLGDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var lgMain = new LoserGainerMain();
                rowBranchLGDetails = lgMain.GetRoWiseBranchDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/LoserAndGainer/_LGBranchDetails.cshtml", rowBranchLGDetails);
        }

        [HttpPost]
        public ActionResult FetchRoGainersCustomerDetails(LooserGainerFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWiseLGInfo> lgCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var lgMain = new LoserGainerMain();
                lgCustomers = lgMain.GetRoGainerCustomerDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                lgCustomers = new List<CustomerWiseLGInfo>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWiseLGInfo>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = lgCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchRoLoosersCustomerDetails(LooserGainerFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWiseLGInfo> lgCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var lgMain = new LoserGainerMain();
                lgCustomers = lgMain.GetROLooserCustomerDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                lgCustomers = new List<CustomerWiseLGInfo>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWiseLGInfo>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = lgCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SE wise Loosers & Gainers detail

        public ActionResult FetchSeWiseLGDetail(LooserGainerFilter dbFilter)
        {
            List<LooserAndGainersDetails> lGDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var lgMain = new LoserGainerMain();
                lGDetails = lgMain.SEWiseLGDetailS(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }
            return PartialView("~/Views/Dashboard/LoserAndGainer/_LGCustomerTypeDetails.cshtml", lGDetails);
        }

        public ActionResult FetchSeWiseBranchLGDetail(LooserGainerFilter dbFilter)
        {
            List<BranchWiseLGInfo> seBranchLGDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var lgMain = new LoserGainerMain();
                seBranchLGDetails = lgMain.GetSeWiseBranchDetails(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }

            return PartialView("~/Views/Dashboard/LoserAndGainer/_LGBranchDetails.cshtml", seBranchLGDetails);
        }

        [HttpPost]
        public ActionResult FetchSEGainersCustomerDetails(LooserGainerFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWiseLGInfo> lgCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var lgMain = new LoserGainerMain();
                lgCustomers = lgMain.GetSeGainerCustomerDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                lgCustomers = new List<CustomerWiseLGInfo>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWiseLGInfo>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = lgCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchSELoosersCustomerDetails(LooserGainerFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWiseLGInfo> lgCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var lgMain = new LoserGainerMain();
                lgCustomers = lgMain.GetSeLooserCustomerDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                lgCustomers = new List<CustomerWiseLGInfo>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWiseLGInfo>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = lgCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region CS wise Loosers & Gainers detail

        public ActionResult FetchCsWiseLGDetail(LooserGainerFilter dbFilter)
        {
            List<LooserAndGainersDetails> lGDetails = null;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };

                var lgMain = new LoserGainerMain();
                lGDetails = lgMain.CSWiseLGDetailS(dbFilter);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                ViewBag.ErrorMsg = exc.Message;
            }
            return PartialView("~/Views/Dashboard/LoserAndGainer/_LGCustomerTypeDetails.cshtml", lGDetails);
        }

        [HttpPost]
        public ActionResult FetchCsGainersCustomerDetails(LooserGainerFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWiseLGInfo> lgCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var lgMain = new LoserGainerMain();
                lgCustomers = lgMain.GetCSGainerCustomerDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                lgCustomers = new List<CustomerWiseLGInfo>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWiseLGInfo>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = lgCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchCsLoosersCustomerDetails(LooserGainerFilter dbFilter, JqDataTableRequest request)
        {
            List<CustomerWiseLGInfo> lgCustomers;
            var errorMsg = string.Empty;
            var totalRows = 0;

            try
            {
                dbFilter.UserId = General.GetUserId();
                dbFilter.Roles = new List<string> { General.GetUserRole(_isFromMobile) };
                dbFilter.SetDataTableFilter(request);

                var lgMain = new LoserGainerMain();
                lgCustomers = lgMain.GetCsLooserCustomerDetails(dbFilter, out totalRows);
            }
            catch (Exception exc)
            {
                lgCustomers = new List<CustomerWiseLGInfo>();
                RepoUserLogs.LogException(exc);
                errorMsg = exc.Message;
            }

            return Json(new JqDataTableResponse<CustomerWiseLGInfo>
            {
                draw = request.Draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRows,
                data = lgCustomers,
                error = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion
    }
}