using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;

namespace Garaaz.Models.DashboardOverview.Inventory
{
    public class InventoryMain
    {
        private const string UspName = "dbo.usp_Dashboard_InventoryDetails";

        public InvInfoResponse GetInventoryInfo(DashboardFilter dbFilter)
        {
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            decimal stockPrice = DbCommon.GetStockPriceByCurrentUserRole(dbFilter);

            var sale = DbCommon.GetCurrentSale(dbFilter);
            var totalDays = DbCommon.GetDayDifference(dbFilter);

            // Calculate  inventory days
            var dailySale = totalDays > 0 ? sale / totalDays : sale;
            var inventoryDays = Math.Round(stockPrice / (dailySale != 0 ? dailySale : 1), 2);

            // Prepare return response
            var totalInventory = string.IsNullOrEmpty($"{stockPrice:#,###}") ? "0" : $"{Math.Round(stockPrice):#,###}";

            return new InvInfoResponse
            {
                DateInfo = dateInfo,
                TotalInventory = totalInventory,
                InventoryDays = $"Inventory days: {inventoryDays}"
            };
        }

        public List<InvDetail> GetInvDetails(DashboardFilter dbFilter)
        {
            var invDetails = new List<InvDetail>();

            var diResp = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = diResp.StartDate;
            dbFilter.EndDate = diResp.EndDate;

            var sqlParams = GetSqlParams(dbFilter, InventoryAction.InvDetails);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return invDetails;

            var pageIndex = dbFilter.Skip + 1;
            invDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new InvDetail
            {
                SlNo = index + pageIndex,
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchName = dataRow.Field<string>("BranchName"),
                StockDays = Math.Round(dataRow.Field<decimal>("StockDays"),2),
                AverageSales = Math.Round(dataRow.Field<decimal>("AverageSales"),2),
                StockPrice = dataRow.Field<decimal>("StockPrice"),
                PartLinesInStock =dataRow.Field<int>("PartLinesInStock")
            }).ToList();

            // StockDays and AverageSales calculate formula used in db

            // var sales=Select 3 months sale before selected end date by user
            //    var branchSales = sales.Where(s => s.LocCode.Equals(inv.BranchCode, StringComparison.OrdinalIgnoreCase)).ToList();
            //    var AverageSales = branchSales.Sum(s => Convert.ToDecimal(s.NetRetailSelling)) / 3;

            //    var dailySale = branchAvgSales / 30;
            //    var stockDays = dailySale > 0 ? inv.StockPrice / dailySale : 0;

            return invDetails;
        }

        public List<InvDetailForBranch> GetBranchWiseInvDetails(DashboardFilter dbFilter, out int totalRows)
        {
            totalRows = 0;
            var bwInvDetails = new List<InvDetailForBranch>();

            var diResp = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = diResp.StartDate;
            dbFilter.EndDate = diResp.EndDate;

            var action = InventoryAction.BranchWiseInvDetails;

            dbFilter.SortBy = GetSortColumn(dbFilter);

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return bwInvDetails;

            var pageIndex = dbFilter.Skip + 1;
            bwInvDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new InvDetailForBranch
            {
                SlNo = index + pageIndex,
                PartGroup = dataRow.Field<string>("PartGroup"),
                PartCategory = dataRow.Field<string>("PartCategory"),
                RootPartNumber = dataRow.Field<string>("RootPartNumber"),
                PartNumber = dataRow.Field<string>("PartNumber"),
                PartDescription = dataRow.Field<string>("PartDescription"),
                Mrp = dataRow.Field<decimal>("MRP"),
                NumberOfStock = dataRow.Field<int>("NumberOfStock"),
                AvgConsumption = dataRow.Field<int>("AvgConsumption")
            }).ToList();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var dataRow = ds.Tables[0].Rows[0];
                totalRows = dataRow.Field<int?>("TotalRows") != null ? dataRow.Field<int?>("TotalRows").Value : 0;
            }
            
            return bwInvDetails;
        }

        #region Comment old code
        //public List<InvDetail> GetInvDetails(DashboardFilter dbFilter)
        //{
        //    var invDetails = new List<InvDetail>();

        //    var diResp = DbCommon.GetDatePeriod(dbFilter);
        //    dbFilter.StartDate = diResp.StartDate;
        //    dbFilter.EndDate = diResp.EndDate;

        //    var dailyStocks = DbCommon.GetStocksByCurrentUserRole(dbFilter);
        //    Session.DailyStocks = dailyStocks;
        //    var stocksWithBranch = from s in dailyStocks
        //                           join o in _db.Outlets on s.OutletId equals o.OutletId
        //                           select new
        //                           {
        //                               o.OutletId,
        //                               BranchCode = o.OutletName,
        //                               BranchName = o.Address,
        //                               StockQty = Convert.ToDecimal(s.StockQuantity),
        //                               StockMrp = Convert.ToDecimal(s.Mrp)
        //                           };
        //    var groups = stocksWithBranch.GroupBy(s => s.BranchCode).ToList();
        //    if (groups.Count == 0) return invDetails;

        //    var sales = DbCommon.GetSalesByCurrentUserRole(dbFilter);
        //    Session.Sales = sales;
        //    var totalDays = DbCommon.GetDayDifference(dbFilter);

        //    var slNo = 1;
        //    foreach (var grp in groups)
        //    {
        //        var branchStockQty = grp.Sum(s => s.StockQty);
        //        var branchSales = sales.Where(s => s.LocCode.Equals(grp.Key, StringComparison.OrdinalIgnoreCase)).ToList();
        //        var branchSaleSum = branchSales.Sum(s => Convert.ToDecimal(s.NetRetailSelling));

        //        decimal stockPrice = 0.0M, stockDays = 0.0M;
        //        if (totalDays > 0)
        //        {
        //            stockPrice = grp.Sum(s => s.StockQty * s.StockMrp);
        //            var dailySale = branchSaleSum / totalDays;
        //            stockDays = dailySale > 0 ? Math.Round(stockPrice / dailySale) : 0;
        //        }

        //        invDetails.Add(new InvDetail
        //        {
        //            SlNo = slNo,
        //            BranchCode = grp.Key,
        //            BranchName = grp.Select(g => g.BranchName).FirstOrDefault(),
        //            StockDays = stockDays,
        //            AverageSales = Math.Round(branchSales.Count > 0 ? branchSaleSum / branchSales.Count : 0, 2),
        //            StockPrice = stockPrice,
        //            PartLinesInStock = Math.Round(branchStockQty, 2)
        //        });

        //        slNo++;
        //    }

        //    return invDetails;
        //}

        //public List<InvDetailForBranch> GetBranchWiseInvDetails(DashboardFilter dbFilter)
        //{
        //    var bwInvDetails = new List<InvDetailForBranch>();

        //    var outlet = _db.Outlets.FirstOrDefault(o => o.OutletName.Equals(dbFilter.BranchCode, StringComparison.OrdinalIgnoreCase));
        //    if (outlet == null) return bwInvDetails;

        //    var dailyStocks = Session.DailyStocks;
        //    var stocks = dailyStocks.Where(s => s.OutletId == outlet.OutletId).ToList();
        //    if (stocks.Count == 0) return bwInvDetails;

        //    var stockDetails = from s in stocks
        //                       join p in _db.Products on s.PartNum equals p.PartNo
        //                       join pg in _db.ProductGroups on p.GroupId equals pg.GroupId
        //                       select new
        //                       {
        //                           pg.GroupName,
        //                           s.PartCategoryCode,
        //                           s.RootPartNum,
        //                           s.PartNum,
        //                           p.Description,
        //                           s.Mrp,
        //                           s.StockQuantity
        //                       };

        //    // Get days for calculating average consumption
        //    var diResp = DbCommon.GetDatePeriod(dbFilter);
        //    dbFilter.StartDate = diResp.StartDate;
        //    dbFilter.EndDate = diResp.EndDate;
        //    var days = DbCommon.GetDayDifference(dbFilter);

        //    var sales = Session.Sales;
        //    var branchSales = sales.Where(s => s.LocCode.Equals(dbFilter.BranchCode, StringComparison.OrdinalIgnoreCase)).ToList();

        //    var slNo = 1;
        //    foreach (var sd in stockDetails)
        //    {
        //        var bwSale = branchSales.Where(s => s.PartNum.Equals(sd.PartNum, StringComparison.OrdinalIgnoreCase)).Sum(s => Convert.ToDecimal(s.NetRetailSelling));
        //        bwInvDetails.Add(new InvDetailForBranch
        //        {
        //            SlNo = slNo,
        //            PartGroup = sd.GroupName,
        //            PartCategory = sd.PartCategoryCode,
        //            RootPartNumber = sd.RootPartNum,
        //            PartNumber = sd.PartNum,
        //            PartDescription = sd.Description,
        //            Mrp = sd.Mrp,
        //            AvgConsumption = Math.Round(days > 0 ? bwSale / days : bwSale, 2),
        //            NumberOfStock = sd.StockQuantity
        //        });

        //        slNo++;
        //    }

        //    return bwInvDetails;
        //}
        #endregion

        /// <summary>
        /// Get last three months sales excluding current month.
        /// </summary>
        private static List<DailySalesTrackerWithInvoiceData> GetLastThreeMonthsSales(DashboardFilter dbFilter, bool getFromSession = false)
        {
            if (getFromSession)
            {
                if (HttpContext.Current.Session["Sales"] is List<DailySalesTrackerWithInvoiceData> sessionSales)
                {
                    return sessionSales;
                }
            }

            // Calculate first day of three month back and last day of previous month
            var endDate = dbFilter.EndDate ?? DateTime.Now;
            var month = new DateTime(endDate.Year, endDate.Month, 1);
            var threeMonthBackFirstDay = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-3);
            var prevMonthLastDay = month.AddDays(-1);

            // Modify filter to get sales
            dbFilter.StartDate = threeMonthBackFirstDay;
            dbFilter.EndDate = prevMonthLastDay;

            var sales = DbCommon.GetSalesByCurrentUserRole(dbFilter);
            HttpContext.Current.Session["Sales"] = sales;

            return sales;
        }

        private static SqlParameter[] GetSqlParams(DashboardFilter dbFilter, InventoryAction action)
        {
            string role = null;
            if (dbFilter.Roles.Contains(Constants.SuperAdmin))
            {
                role = Constants.SuperAdmin;
            }
            else if (dbFilter.Roles.Contains(Constants.Distributor))
            {
                role = Constants.Distributor;
            }
            else if (dbFilter.Roles.Contains(Constants.RoIncharge))
            {
                role = Constants.RoIncharge;
            }
            else if (dbFilter.Roles.Contains(Constants.SalesExecutive))
            {
                role = Constants.SalesExecutive;
            }

            var endDate = dbFilter.EndDate ?? DateTime.Now;
            var avgThreeMonthEndDate = new DateTime(endDate.Year, endDate.Month, 1).AddDays(-1);
            var avgThreeMonthStartDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-3);
            var totalDays = DbCommon.GetDayDifference(dbFilter);

            var sqlParams = new[] {
                new SqlParameter("@Action",action),
                new SqlParameter("@Role",role),
                new SqlParameter("@UserId",dbFilter.UserId),
                new SqlParameter("@BranchCode",dbFilter.BranchCode),
                new SqlParameter("@Skip",dbFilter.Skip),
                new SqlParameter("@Take",dbFilter.PageSize),
                new SqlParameter("@SearchTxt",dbFilter.SearchTxt),
                new SqlParameter("@SortBy",dbFilter.SortBy),
                new SqlParameter("@SortOrder",dbFilter.SortOrder),
                new SqlParameter("@AvgThreeMonthStartDate",avgThreeMonthStartDate),
                new SqlParameter("@AvgThreeMonthEndDate",avgThreeMonthEndDate),
                new SqlParameter("@DaysDiff",totalDays)
            };

            return sqlParams;
        }

        private static string GetSortColumn(DashboardFilter dbFilter)
        {
            string sortColumn = null;
            if (string.IsNullOrWhiteSpace(dbFilter.SortBy)) return sortColumn;
            // Check Orderby column name exist in the InvDetailForBranch model if yes then set Orderby column name
            var isColumnValid = new InvDetailForBranch().GetType().GetProperty(dbFilter.SortBy) != null ? true : false;

            if (isColumnValid)
            {
                if (dbFilter.SortBy != "SlNo")
                {
                    sortColumn = dbFilter.SortBy;
                }
            }
            return sortColumn;
        }
    }
}