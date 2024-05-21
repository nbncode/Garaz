using Garaaz.Models.DashboardOverview.Sale;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Garaaz.Models.DashboardOverview
{
    /// <summary>
    /// Contains common helper methods for Dashboard.
    /// </summary>
    public class DbCommon
    {
        /// <summary>
        /// Gets customer types excluding co-dealer.
        /// </summary>
        public static string[] CustomerTypes => new[] { CustomerType.Mass, CustomerType.Trader, CustomerType.Workshop, CustomerType.WalkInCustomer };

        /// <summary>
        /// Gets customer types including co-dealer.
        /// </summary>
        public static string[] CoCustomerTypes => new[] { CustomerType.CoDealer, CustomerType.Mass, CustomerType.Trader, CustomerType.Workshop, CustomerType.WalkInCustomer };

        public static void SetDashboardCookies(string userId, string role)
        {
            var utils = new Utils();
            utils.setCookiesValue(userId, Constants.UserIdSession);
            utils.setCookiesValue(role, Constants.UserRoleSession);
        }

        /// <summary>
        /// Get date info as per date type.
        /// </summary>
        public static DateInfoResponse GetDatePeriod(DashboardFilter dbFilter)
        {
            var diResp = new DateInfoResponse();

            var now = DateTime.Now;
            var mfi = new DateTimeFormatInfo();
            switch (dbFilter.DateType)
            {
                case "MTD":
                    diResp.StartDate = new DateTime(now.Year, now.Month, 1);
                    diResp.EndDate = diResp.StartDate.AddDays(now.Day - 1);
                    diResp.DateInfo = mfi.GetMonthName(Convert.ToInt32(diResp.StartDate.Month)) + " - " + diResp.StartDate.Year;
                    break;

                case "YTD":
                    diResp.StartDate = new DateTime(now.Year, 1, 1);
                    diResp.EndDate = new DateTime(now.Year, now.Month, now.Day);
                    diResp.DateInfo = diResp.StartDate.Year.ToString();
                    break;

                case "Q1":
                    diResp.StartDate = new DateTime(now.Year, 4, 1);
                    diResp.EndDate = now.Month > 6 ? diResp.StartDate.AddMonths(3).AddDays(-1) : diResp.StartDate.AddMonths(now.Month - 1).AddDays(now.Day - 1);
                    diResp.DateInfo = "Quarter : 1";
                    break;

                case "Q2":
                    diResp.StartDate = new DateTime(now.Year, 7, 1);
                    diResp.EndDate = now.Month > 9 ? diResp.StartDate.AddMonths(3).AddDays(-1) : diResp.StartDate.AddMonths(now.Month - 1).AddDays(now.Day - 1);
                    diResp.DateInfo = "Quarter : 2";
                    break;

                case "Q3":
                    diResp.StartDate = new DateTime(now.Year, 10, 1);
                    diResp.EndDate = now.Month > 12 ? diResp.StartDate.AddMonths(3).AddDays(-1) : diResp.StartDate.AddMonths(now.Month - 1).AddDays(now.Day - 1);
                    diResp.DateInfo = "Quarter : 3";
                    break;

                case "Q4":
                    diResp.StartDate = new DateTime(now.Year, 1, 1);
                    diResp.EndDate = now.Month > 3 ? diResp.StartDate.AddMonths(3).AddDays(-1) : diResp.StartDate.AddMonths(now.Month - 1).AddDays(now.Day - 1);
                    diResp.DateInfo = "Quarter : 4";
                    break;

                case "CustomDate":
                    diResp.StartDate = dbFilter.StartDate ?? DateTime.Now;
                    diResp.EndDate = dbFilter.EndDate ?? DateTime.Now;
                    diResp.DateInfo = $"{diResp.StartDate:dd-MM-yyyy} to {diResp.EndDate:dd-MM-yyyy}";
                    break;
            }

            return diResp;
        }

        public static decimal CalculatePercentage(decimal previousSale, decimal currentSale)
        {
            decimal percentage;
            if (previousSale == 0)
            {
                //percentage = currentSale * 100 / 1;
                percentage = currentSale;
            }
            else { percentage = (currentSale - previousSale) * 100 / previousSale; }
            return percentage;
        }

        public static decimal CalculateGrowthPercentage(int previousSale, int currentSale)
        {
            decimal percentage;
            if (previousSale == 0)
            {
                //percentage = currentSale * 100 / 1;
                percentage = currentSale;
            }
            else { percentage = (currentSale - previousSale) * 100 / previousSale; }
            return percentage;
        }

        /// <summary>
        /// Get sales by current logged in user's role and further filter by date range.
        /// </summary>
        public static List<DailySalesTrackerWithInvoiceData> GetSalesByCurrentUserRole(DashboardFilter dbFilter)
        {
            var sales = new List<DailySalesTrackerWithInvoiceData>();

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

            var sqlParams = new[] {
                new SqlParameter("@Action",1),
                new SqlParameter("@Role",role),
                new SqlParameter("@UserId",dbFilter.UserId),
                new SqlParameter("@StartDate",dbFilter.StartDate),
                new SqlParameter("@EndDate",dbFilter.EndDate),
                new SqlParameter("@BranchCode",dbFilter.BranchCode)

            };

            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_CurrentSaleDetails", sqlParams);
            if (ds.Tables.Count > 0)
            {
                // TODO: Reduce number of columns retrieved from DailySalesTrackerWithInvoiceData

                sales = ds.Tables[0].AsEnumerable().Select(dataRow => new DailySalesTrackerWithInvoiceData
                {
                    DailySalesTrackerId = dataRow.Field<int>("DailySalesTrackerId"),
                    LocCode = dataRow.Field<string>("LocCode"),
                    PartNum = dataRow.Field<string>("PartNum"),
                    NetRetailSelling = dataRow.Field<string>("NetRetailSelling"),
                    WorkShopId = dataRow.Field<int>("WorkShopId")
                }).ToList();
            }

            return sales;
        }

        /// <summary>
        /// Get workshops by current logged in user's role.
        /// </summary>
        public static List<WorkShop> GetWorkshopsByCurrentUserRole(DashboardFilter dbFilter)
        {
            var workshops = new List<WorkShop>();
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

            var sqlParams = new[] {
                new SqlParameter("@Action",1),
                new SqlParameter("@Role",role),
                new SqlParameter("@UserId",dbFilter.UserId)
            };

            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_CurrentUserWorkshops", sqlParams);
            if (ds.Tables.Count > 0)
            {
                // TODO: Reduce number of columns retrieved from workshop

                workshops = ds.Tables[0].AsEnumerable().Select(dataRow => new WorkShop
                {
                    WorkShopId = dataRow.Field<int>("WorkShopId"),
                    WorkShopName = dataRow.Field<string>("WorkShopName"),
                    Address = dataRow.Field<string>("Address"),
                    Latitude = dataRow.Field<string>("Latitude"),
                    Longitude = dataRow.Field<string>("Longitude"),
                    Gstin = dataRow.Field<string>("Gstin"),
                    Pincode = dataRow.Field<string>("Pincode"),
                    State = dataRow.Field<string>("State"),
                    City = dataRow.Field<string>("City"),
                    Gender = dataRow.Field<string>("Gender"),
                    LandlineNumber = dataRow.Field<string>("LandlineNumber"),
                    CriticalOutstandingDays = dataRow.Field<int?>("CriticalOutstandingDays"),
                    OutstandingAmount = dataRow.Field<decimal?>("OutstandingAmount"),
                    outletId = dataRow.Field<int?>("outletId"),
                    CategoryName = dataRow.Field<string>("CategoryName"),
                    CreditLimit = dataRow.Field<decimal?>("CreditLimit"),
                    TotalOutstanding = dataRow.Field<decimal?>("TotalOutstanding"),
                    BillingName = dataRow.Field<string>("BillingName"),
                    YearOfEstablishment = dataRow.Field<int?>("YearOfEstablishment"),
                    Type = dataRow.Field<string>("Type"),
                    Make = dataRow.Field<string>("Make"),
                    JobsUndertaken = dataRow.Field<string>("JobsUndertaken"),
                    Premise = dataRow.Field<string>("Premise"),
                    GaraazArea = dataRow.Field<string>("GaraazArea"),
                    TwoPostLifts = dataRow.Field<int?>("TwoPostLifts"),
                    WashingBay = dataRow.Field<bool?>("WashingBay"),
                    PaintBooth = dataRow.Field<bool?>("PaintBooth"),
                    ScanningAndToolKit = dataRow.Field<bool?>("ScanningAndToolKit"),
                    TotalOwners = dataRow.Field<int?>("TotalOwners"),
                    TotalChiefMechanics = dataRow.Field<int?>("TotalChiefMechanics"),
                    TotalEmployees = dataRow.Field<int?>("TotalEmployees"),
                    MonthlyVehiclesServiced = dataRow.Field<int?>("MonthlyVehiclesServiced"),
                    MonthlyPartPurchase = dataRow.Field<decimal?>("MonthlyPartPurchase"),
                    MonthlyConsumablesPurchase = dataRow.Field<decimal?>("MonthlyConsumablesPurchase"),
                    WorkingHours = dataRow.Field<string>("WorkingHours"),
                    WeeklyOffDay = dataRow.Field<string>("WeeklyOffDay"),
                    Website = dataRow.Field<string>("Website"),
                    InsuranceCompanies = dataRow.Field<string>("InsuranceCompanies"),
                    IsMoreThanOneBranch = dataRow.Field<bool?>("IsMoreThanOneBranch")
                }).ToList();
            }

            #region Comment Entity code
            //using (var db = new garaazEntities())
            //{
            //    if (dbFilter.Roles.Contains(Constants.SuperAdmin))
            //    {
            //        workshops = db.WorkShops.Select(w => w).ToList();
            //    }
            //    else if (dbFilter.Roles.Contains(Constants.Distributor))
            //    {
            //        var distId = General.getDistributorId(dbFilter.UserId);
            //        workshops = (from w in db.WorkShops
            //                     join dw in db.DistributorWorkShops on w.WorkShopId equals dw.WorkShopId
            //                     where dw.DistributorId == distId
            //                     select w).ToList();
            //    }
            //    else if (dbFilter.Roles.Contains(Constants.RoIncharge))
            //    {
            //        var distributorsOutlet = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == dbFilter.UserId);
            //        if (distributorsOutlet != null)
            //        {
            //            workshops = db.WorkShops.Where(w => w.outletId == distributorsOutlet.OutletId)
            //                .Select(w => w).ToList();
            //        }
            //    }
            //    else if (dbFilter.Roles.Contains(Constants.SalesExecutive))
            //    {
            //        workshops = (from w in db.WorkShops
            //                     join sew in db.SalesExecutiveWorkshops on w.WorkShopId equals sew.WorkshopId
            //                     where sew.UserId == dbFilter.UserId
            //                     select w).ToList();
            //    }
            //}
            #endregion

            return workshops;
        }

        /// <summary>
        /// Get accounts ledgers by current logged in user's role.
        /// </summary>
        public static decimal GetTotalCollectionByCurrentUserRole(DashboardFilter dbFilter)
        {
            decimal totalCollection = 0;

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

            var sqlParams = new[] {
                new SqlParameter("@Action",1),
                new SqlParameter("@Role",role),
                new SqlParameter("@UserId",dbFilter.UserId),
                new SqlParameter("@StartDate",dbFilter.StartDate),
                new SqlParameter("@EndDate",dbFilter.EndDate),
                new SqlParameter("@ConstantsClosingBalance",Constants.ClosingBalance),
                new SqlParameter("@ConstantsOpeningBalance",Constants.OpeningBalance)
            };

            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_CurrentUserCollection", sqlParams);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var dataRow = ds.Tables[0].Rows[0];
                var payment = dataRow["Payment"] != DBNull.Value ? Convert.ToDecimal(dataRow["Payment"]) : 0;
                var credit = dataRow["Credit"] != DBNull.Value ? Convert.ToDecimal(dataRow["Credit"]) : 0;
                totalCollection = credit - payment;
            }
            #region Comment old entity code
            //using (var db = new garaazEntities())
            //{
            //    if (dbFilter.Roles.Contains(Constants.SuperAdmin))
            //    {
            //        accounts = (from a in db.AccountLedgers
            //                    where a.Particulars != Constants.ClosingBalance && a.Particulars != Constants.OpeningBalance &&
            //                          a.Date != null && DbFunctions.TruncateTime(a.Date) >= dbFilter.StartDate &&
            //                          DbFunctions.TruncateTime(a.Date) <= dbFilter.EndDate
            //                    select a).ToList();
            //    }
            //    else if (dbFilter.Roles.Contains(Constants.Distributor))
            //    {
            //        var distId = General.getDistributorId(dbFilter.UserId);
            //        accounts = (from a in db.AccountLedgers
            //                    where a.Particulars != Constants.ClosingBalance && a.Particulars != Constants.OpeningBalance &&
            //                          a.Date != null && DbFunctions.TruncateTime(a.Date) >= dbFilter.StartDate &&
            //                          DbFunctions.TruncateTime(a.Date) <= dbFilter.EndDate && a.DistributorId == distId
            //                    select a).ToList();
            //    }
            //    else if (dbFilter.Roles.Contains(Constants.RoIncharge))
            //    {
            //        var distributorsOutlets = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == dbFilter.UserId);
            //        if (distributorsOutlets != null)
            //        {
            //            var workshop = (from w in db.WorkShops
            //                            where w.outletId == distributorsOutlets.OutletId
            //                            select w).ToList();
            //            if (workshop.Count > 0)
            //            {
            //                var wIds = workshop.Select(w => w.WorkShopId).ToList();
            //                accounts = (from a in db.AccountLedgers
            //                            where a.Particulars != Constants.ClosingBalance &&
            //                                  a.Particulars != Constants.OpeningBalance && a.Date != null &&
            //                                  DbFunctions.TruncateTime(a.Date) >= dbFilter.StartDate &&
            //                                  DbFunctions.TruncateTime(a.Date) <= dbFilter.EndDate &&
            //                                  wIds.Contains(a.WorkshopId.Value)
            //                            select a).ToList();
            //            }
            //        }
            //    }
            //    else if (dbFilter.Roles.Contains(Constants.SalesExecutive))
            //    {
            //        var seWsIds = db.SalesExecutiveWorkshops.Where(w => w.UserId == dbFilter.UserId)
            //            .Select(w => w.WorkshopId).ToList();
            //        if (seWsIds.Count > 0)
            //        {
            //            accounts = (from a in db.AccountLedgers
            //                        where a.Particulars != Constants.ClosingBalance &&
            //                              a.Particulars != Constants.OpeningBalance && a.Date != null &&
            //                              DbFunctions.TruncateTime(a.Date) >= dbFilter.StartDate &&
            //                              DbFunctions.TruncateTime(a.Date) <= dbFilter.EndDate &&
            //                              seWsIds.Contains(a.WorkshopId.Value)
            //                        select a).ToList();
            //        }
            //    }
            //}
            #endregion

            return totalCollection;
        }

        /// <summary>
        /// Get daily stocks by current logged in user's role.
        /// </summary>
        /// <param name="dbFilter"></param>
        /// <returns></returns>
        public static decimal GetStockPriceByCurrentUserRole(DashboardFilter dbFilter)
        {
            decimal stockPrice = 0;

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

            var sqlParams = new[] {
                new SqlParameter("@Action",1),
                new SqlParameter("@Role",role),
                new SqlParameter("@UserId",dbFilter.UserId)
            };

            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_CurrentUserStocksPrice", sqlParams);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count>0 )
            {
                stockPrice = ds.Tables[0].Rows[0].Field<decimal>("StockPrice");
            }
            #region Comment old entity code
            //using (var db = new garaazEntities())
            //{
            //    if (dbFilter.Roles.Contains(Constants.SuperAdmin))
            //    {
            //        dailyStocks = db.DailyStocks.Where(s => s.OutletId != null).ToList();

            //    }
            //    else if (dbFilter.Roles.Contains(Constants.Distributor))
            //    {
            //        var distId = General.getDistributorId(dbFilter.UserId);
            //        dailyStocks = db.DailyStocks.Where(s => s.OutletId != null && s.DistributorId == distId).ToList();

            //    }
            //    else if (dbFilter.Roles.Contains(Constants.RoIncharge))
            //    {
            //        var distOutlets = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == dbFilter.UserId);
            //        if (distOutlets != null)
            //        {
            //            dailyStocks = db.DailyStocks.Where(s => s.OutletId == distOutlets.OutletId).ToList();
            //        }
            //    }
            //    else if (dbFilter.Roles.Contains(Constants.SalesExecutive))
            //    {
            //        var outletUser = (from a in db.OutletsUsers
            //                          join r in db.RoSalesExecutives on a.UserId equals r.RoUserId
            //                          where r.SeUserId == dbFilter.UserId
            //                          select a).FirstOrDefault();
            //        if (outletUser != null)
            //        {
            //            dailyStocks = db.DailyStocks.Where(s => s.OutletId == outletUser.OutletId).ToList();
            //        }
            //    }
            //}
            #endregion

            return stockPrice;
        }

        public static SaleDetail GetTotalSale(DashboardFilter dbFilter)
        {
            decimal currentSale = 0, previousSale = 0, coDealerOrDistSale = 0;
            if (dbFilter.StartDate == null || dbFilter.EndDate == null)
            {
                return new SaleDetail
                {
                    CurrentSale = currentSale,
                    PreviousSale = previousSale,
                    CoDealerOrDistSale = coDealerOrDistSale
                };
            }

            try
            {
                // Set user role
                var role = string.Empty;
                if (dbFilter.Roles.Contains(Constants.SuperAdmin))
                { role = Constants.SuperAdmin; }
                if (dbFilter.Roles.Contains(Constants.Distributor))
                { role = Constants.Distributor; }
                if (dbFilter.Roles.Contains(Constants.RoIncharge))
                { role = Constants.RoIncharge; }
                if (dbFilter.Roles.Contains(Constants.SalesExecutive))
                { role = Constants.SalesExecutive; }

                SqlParameter[] parameter = {
                    new SqlParameter("@Action",1),
                    new SqlParameter("@Role",role),
                    new SqlParameter("@UserId",dbFilter.UserId),
                    new SqlParameter("@StartDate",dbFilter.StartDate),
                    new SqlParameter("@EndDate",dbFilter.EndDate),
                    new SqlParameter("@CoDealerDist",Constants.CoDealerDistributor)
                };

                DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_TotalSaleDetail", parameter);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var rows = ds.Tables[0].Rows[0];
                    currentSale = rows["CurrentSale"] != DBNull.Value ? Convert.ToDecimal(rows["CurrentSale"]) : 0;
                    previousSale = rows["PreviousSale"] != DBNull.Value ? Convert.ToDecimal(rows["PreviousSale"]) : 0;
                    coDealerOrDistSale = rows["CoDealerOrDistSale"] != DBNull.Value ? Convert.ToDecimal(rows["CoDealerOrDistSale"]) : 0;
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            #region Comment Entity Code
            //if (dbFilter.StartDate != null && dbFilter.EndDate != null)
            //{
            //    DateTime? prevYrStartDate = dbFilter.StartDate?.AddYears(-1),
            //        prvYrEndDate = dbFilter.EndDate?.AddYears(-1);

            //    var sales = new List<DailySalesTrackerWithInvoiceData>();
            //    using (var db = new garaazEntities())
            //    {
            //        if (dbFilter.Roles.Contains(Constants.SuperAdmin))
            //        {
            //            sales = (from s in db.DailySalesTrackerWithInvoiceDatas
            //                     where !string.IsNullOrEmpty(s.NetRetailSelling)
            //                     select s).ToList();
            //        }
            //        else if (dbFilter.Roles.Contains(Constants.Distributor))
            //        {
            //            var distId = General.getDistributorId(dbFilter.UserId);
            //            sales = (from s in db.DailySalesTrackerWithInvoiceDatas
            //                     where !string.IsNullOrEmpty(s.NetRetailSelling) && s.DistributorId == distId
            //                     select s).ToList();
            //        }
            //        else if (dbFilter.Roles.Contains(Constants.RoIncharge))
            //        {
            //            var distributorsOutlet =
            //                db.DistributorsOutlets.FirstOrDefault(a => a.UserId == dbFilter.UserId);
            //            if (distributorsOutlet != null)
            //            {
            //                var workshop = (from w in db.WorkShops
            //                                where w.outletId == distributorsOutlet.OutletId
            //                                select w).ToList();
            //                var wsIds = workshop.Select(a => a.WorkShopId);

            //                sales = (from d in db.DailySalesTrackerWithInvoiceDatas
            //                         where !string.IsNullOrEmpty(d.NetRetailSelling) && d.WorkShopId.HasValue &&
            //                               wsIds.Contains(d.WorkShopId.Value)
            //                         select d).ToList();
            //            }
            //        }
            //        else if (dbFilter.Roles.Contains(Constants.SalesExecutive))
            //        {
            //            var seWsIds = db.SalesExecutiveWorkshops.Where(w => w.UserId == dbFilter.UserId)
            //                .Select(w => w.WorkshopId).ToList();
            //            sales = (from d in db.DailySalesTrackerWithInvoiceDatas
            //                     where !string.IsNullOrEmpty(d.NetRetailSelling) && d.WorkShopId.HasValue &&
            //                           seWsIds.Contains(d.WorkShopId.Value)
            //                     select d).ToList();
            //        }

            //        if (targetWorkShops != null)
            //        {
            //            // Get sales for target workshops
            //            var twsIds = targetWorkShops.Select(t => t.WorkShopId);
            //            sales = sales.Where(s => twsIds.Contains(s.WorkShopId)).ToList();
            //        }

            //        if (sales.Count > 0)
            //        {
            //            // Filter current sale
            //            currentSale = sales
            //                .Where(d => d.CreatedDate?.Date >= dbFilter.StartDate &&
            //                            d.CreatedDate?.Date <= dbFilter.EndDate &&
            //                            d.ConsPartyTypeDesc != CustomerType.CoDealer &&
            //                            d.ConsPartyTypeDesc != CustomerType.Distributor)
            //                .Sum(s => Convert.ToDecimal(s.NetRetailSelling));

            //            // Filter co-dealer or distributor current sale
            //            coDealerOrDistSale = sales
            //                .Where(d => d.CreatedDate?.Date >= dbFilter.StartDate &&
            //                            d.CreatedDate?.Date <= dbFilter.EndDate &&
            //                            (d.ConsPartyTypeDesc == CustomerType.CoDealer ||
            //                             d.ConsPartyTypeDesc == CustomerType.Distributor))
            //                .Sum(s => Convert.ToDecimal(s.NetRetailSelling));

            //            // Filter last year sale
            //            previousSale = sales
            //                .Where(d => d.CreatedDate?.Date >= prevYrStartDate && d.CreatedDate?.Date <= prvYrEndDate &&
            //                            d.ConsPartyTypeDesc != CustomerType.CoDealer &&
            //                            d.ConsPartyTypeDesc != CustomerType.Distributor)
            //                .Sum(s => Convert.ToDecimal(s.NetRetailSelling));
            //        }
            //    }
            //}
            #endregion

            return new SaleDetail
            {
                CurrentSale = currentSale,
                PreviousSale = previousSale,
                CoDealerOrDistSale = coDealerOrDistSale
            };
        }

        public static int GetDayDifference(DashboardFilter dbFilter)
        {
            double totalDays = 0;
            if (dbFilter.StartDate != null && dbFilter.EndDate != null)
            {
                TimeSpan t = dbFilter.EndDate.Value - dbFilter.StartDate.Value;
                totalDays = t.TotalDays;
            }
            return Convert.ToInt32(totalDays);
        }

        public static decimal GetCurrentSale(DashboardFilter dbFilter)
        {
            decimal currentSale = 0;
            if (dbFilter.StartDate == null || dbFilter.EndDate == null)
            {
                return currentSale;
            }

            try
            {
                // Set user role
                var role = string.Empty;
                if (dbFilter.Roles.Contains(Constants.SuperAdmin))
                { role = Constants.SuperAdmin; }
                if (dbFilter.Roles.Contains(Constants.Distributor))
                { role = Constants.Distributor; }
                if (dbFilter.Roles.Contains(Constants.RoIncharge))
                { role = Constants.RoIncharge; }
                if (dbFilter.Roles.Contains(Constants.SalesExecutive))
                { role = Constants.SalesExecutive; }

                SqlParameter[] parameter = {
                    new SqlParameter("@Action",2),
                    new SqlParameter("@Role",role),
                    new SqlParameter("@UserId",dbFilter.UserId),
                    new SqlParameter("@StartDate",dbFilter.StartDate),
                    new SqlParameter("@EndDate",dbFilter.EndDate)
                };

                DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_TotalSaleDetail", parameter);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var rows = ds.Tables[0].Rows[0];
                    currentSale = rows["CurrentSale"] != DBNull.Value ? Convert.ToDecimal(rows["CurrentSale"]) : 0;
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return currentSale;
        }
    }
}