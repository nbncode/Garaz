using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Garaaz.Models.DashboardOverview.Outstanding
{
    public class OutstandingMain
    {
        private const string RoUspName = "dbo.usp_Dashboard_RoWiseOutstandingDetails";
        private const string SeUspName = "dbo.usp_Dashboard_SeWiseOutstandingDetails";
        private const string CsUspName = "dbo.usp_Dashboard_CsWiseOutstandingDetails";

        public OsInfoResponse GetOutstandingInfo(DashboardFilter dbFilter)
        {
            var dateInfoResponse = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = dateInfoResponse.StartDate;
            dbFilter.EndDate = dateInfoResponse.EndDate;
            var dateInfo = dateInfoResponse.DateInfo;

            decimal totalOutstanding = 0, criticalOutstanding = 0;

            var workshops = DbCommon.GetWorkshopsByCurrentUserRole(dbFilter);
            if (workshops.Count > 0)
            {
                totalOutstanding = workshops.Where(w => w.TotalOutstanding != null).Sum(s => Convert.ToDecimal(s.TotalOutstanding));
                criticalOutstanding = workshops.Where(w => w.OutstandingAmount != null).Sum(s => Convert.ToDecimal(s.OutstandingAmount));
            }

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            var totalDays = DbCommon.GetDayDifference(dbFilter);

            // Calculate outstanding days
            var perDaySale = totalDays > 0 ? currentSale / totalDays : currentSale;
            var outstandingDays = perDaySale > 0 ? Math.Round(totalOutstanding / perDaySale, 2) : 0;

            var totalOutstandingTxt = totalOutstanding <= 0 ? "0" : $"{Math.Round(totalOutstanding):#,###}";
            var criticalOutstandingTxt = criticalOutstanding <= 0 ? "0" : $"{Math.Round(criticalOutstanding):#,###}";

            return new OsInfoResponse
            {
                DateInfo = dateInfo,
                TotalOutstanding = totalOutstandingTxt,
                OutstandingDays = $"Outstanding days: {outstandingDays}",
                CriticalPayment = $"Critical Payment: {criticalOutstandingTxt}"
            };
        }

        public List<OsInfoResponse> GetSubGroupWiseOutstanding(DashboardFilter dbFilter)
        {
            var osInfos = new List<OsInfoResponse>();

            var dateInfoResponse = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = dateInfoResponse.StartDate;
            dbFilter.EndDate = dateInfoResponse.EndDate;
            var dateInfo = dateInfoResponse.DateInfo;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);

            var roOsInfoResp = GetOsInfoResponse(dbFilter, OsAction.RoWiseTotalOs, currentSale, dateInfo, "RO Wise", "RO");
            osInfos.Add(roOsInfoResp);

            var seOsInfoResp = GetOsInfoResponse(dbFilter, OsAction.SeWiseTotalOs, currentSale, dateInfo, "Sales Executive Wise", "SE");
            osInfos.Add(seOsInfoResp);

            var csOsInfoResp = GetOsInfoResponse(dbFilter, OsAction.CsWiseTotalOs, currentSale, dateInfo, "Customer Segment Wise", "CS");
            osInfos.Add(csOsInfoResp);

            return osInfos;
        }

        #region RO wise outstanding detail

        public List<OsCustDetail> GetRoWiseOsDetails(DashboardFilter dbFilter)
        {
            var roWiseOsDetails = new List<OsCustDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var action = string.IsNullOrWhiteSpace(dbFilter.BranchCode) ? OsAction.RoWiseOsDetails : OsAction.RoWiseOsByBranch;
            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, RoUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return roWiseOsDetails;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            roWiseOsDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new OsCustDetail
            {
                SlNo = index + 1,
                CustomerType = dataRow.Field<string>("CustomerType"),
                NoOfCustomers = dataRow.Field<int>("TotalCustomer"),
                OutstandingDays = GetOutstandingDays(dbFilter, dataRow.Field<decimal>("TotalOutstanding"), currentSale),
                Outstanding = dataRow.Field<decimal>("TotalOutstanding"),
                CreditLimit = dataRow.Field<decimal>("CreditLimit"),
                CriticalPayment = dataRow.Field<decimal>("CriticalPayment"),
                ZeroToFourteenDays = dataRow.Field<decimal>("0To14Days"),
                FourteenToTwentyEightDays = dataRow.Field<decimal>("14To28Days"),
                TwentyEightToFiftyDays = dataRow.Field<decimal>("28To50Days"),
                FiftyToSeventyDays = dataRow.Field<decimal>("50To70Days"),
                MoreThanSeventyDays = dataRow.Field<decimal>("MoreThan70Days"),
                BranchCode = dbFilter.BranchCode
            }).ToList();

            return roWiseOsDetails;
        }

        public List<BranchOsDetail> GetRoWiseOsBranchDetails(DashboardFilter dbFilter)
        {
            var roWiseOsDetails = new List<BranchOsDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, OsAction.RoWiseBranchOs);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, RoUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return roWiseOsDetails;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            roWiseOsDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new BranchOsDetail
            {
                SlNo = index + 1,
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchName = dataRow.Field<string>("BranchName"),
                NoOfCustomers = dataRow.Field<int>("TotalCustomer"),
                OutstandingDays = GetOutstandingDays(dbFilter, dataRow.Field<decimal>("TotalOutstanding"), currentSale),
                Outstanding = dataRow.Field<decimal>("TotalOutstanding"),
                CreditLimit = dataRow.Field<decimal>("CreditLimit"),
                CriticalPayment = dataRow.Field<decimal>("CriticalPayment"),
                ZeroToFourteenDays = dataRow.Field<decimal>("0To14Days"),
                FourteenToTwentyEightDays = dataRow.Field<decimal>("14To28Days"),
                TwentyEightToFiftyDays = dataRow.Field<decimal>("28To50Days"),
                FiftyToSeventyDays = dataRow.Field<decimal>("50To70Days"),
                MoreThanSeventyDays = dataRow.Field<decimal>("MoreThan70Days")
            }).ToList();

            return roWiseOsDetails;
        }

        public List<CustomerOsDetail> GetRoWiseCustomerOsDetails(DashboardFilter dbFilter, out int totalRows)
        {
            totalRows = 0;
            var roWiseOsDetails = new List<CustomerOsDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var action = string.IsNullOrWhiteSpace(dbFilter.SearchTxt)
                ? OsAction.RoWiseCustomerOs
                : OsAction.RoWiseCustomerOsWithSearch;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, RoUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return roWiseOsDetails;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            var pageIndex = dbFilter.Skip + 1;
            roWiseOsDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new CustomerOsDetail
            {
                SlNo = index + pageIndex,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                OutstandingDays = GetOutstandingDays(dbFilter, dataRow.Field<decimal>("TotalOutstanding"), currentSale),
                Outstanding = dataRow.Field<decimal>("TotalOutstanding"),
                CreditLimit = dataRow.Field<decimal>("CreditLimit"),
                CriticalPayment = dataRow.Field<decimal>("CriticalPayment"),
                ZeroToFourteenDays = dataRow.Field<decimal>("0To14Days"),
                FourteenToTwentyEightDays = dataRow.Field<decimal>("14To28Days"),
                TwentyEightToFiftyDays = dataRow.Field<decimal>("28To50Days"),
                FiftyToSeventyDays = dataRow.Field<decimal>("50To70Days"),
                MoreThanSeventyDays = dataRow.Field<decimal>("MoreThan70Days"),
                TotalRows = dataRow.Field<int>("TotalRows")
            }).ToList();

            totalRows = roWiseOsDetails.First()?.TotalRows ?? 0;

            return roWiseOsDetails;
        }

        #endregion

        #region SE wise outstanding detail

        public List<OsCustDetail> GetSeWiseOsDetails(DashboardFilter dbFilter)
        {
            var seWiseOsDetails = new List<OsCustDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var action = string.IsNullOrWhiteSpace(dbFilter.BranchCode) ? OsAction.SeWiseOsDetails : OsAction.SeWiseOsByBranch;
            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, SeUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return seWiseOsDetails;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            seWiseOsDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new OsCustDetail
            {
                SlNo = index + 1,
                CustomerType = dataRow.Field<string>("CustomerType"),
                NoOfCustomers = dataRow.Field<int>("TotalCustomer"),
                OutstandingDays = GetOutstandingDays(dbFilter, dataRow.Field<decimal>("TotalOutstanding"), currentSale),
                Outstanding = dataRow.Field<decimal>("TotalOutstanding"),
                CreditLimit = dataRow.Field<decimal>("CreditLimit"),
                CriticalPayment = dataRow.Field<decimal>("CriticalPayment"),
                ZeroToFourteenDays = dataRow.Field<decimal>("0To14Days"),
                FourteenToTwentyEightDays = dataRow.Field<decimal>("14To28Days"),
                TwentyEightToFiftyDays = dataRow.Field<decimal>("28To50Days"),
                FiftyToSeventyDays = dataRow.Field<decimal>("50To70Days"),
                MoreThanSeventyDays = dataRow.Field<decimal>("MoreThan70Days"),
                BranchCode = dbFilter.BranchCode
            }).ToList();

            return seWiseOsDetails;
        }

        public List<SeWiseBranchOsDetail> GetSeWiseOsBranchDetails(DashboardFilter dbFilter)
        {
            var seWiseOsDetails = new List<SeWiseBranchOsDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, OsAction.SeWiseBranchOs);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, SeUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return seWiseOsDetails;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            seWiseOsDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new SeWiseBranchOsDetail
            {
                SlNo = index + 1,
                BranchCode = dataRow.Field<string>("BranchCode"),
                SalesExecutiveName = dataRow.Field<string>("SalesExName"),
                NoOfCustomers = dataRow.Field<int>("TotalCustomer"),
                OutstandingDays = GetOutstandingDays(dbFilter, dataRow.Field<decimal>("TotalOutstanding"), currentSale),
                Outstanding = dataRow.Field<decimal>("TotalOutstanding"),
                CreditLimit = dataRow.Field<decimal>("CreditLimit"),
                CriticalPayment = dataRow.Field<decimal>("CriticalPayment"),
                ZeroToFourteenDays = dataRow.Field<decimal>("0To14Days"),
                FourteenToTwentyEightDays = dataRow.Field<decimal>("14To28Days"),
                TwentyEightToFiftyDays = dataRow.Field<decimal>("28To50Days"),
                FiftyToSeventyDays = dataRow.Field<decimal>("50To70Days"),
                MoreThanSeventyDays = dataRow.Field<decimal>("MoreThan70Days")
            }).ToList();

            return seWiseOsDetails;
        }

        public List<CustomerOsDetail> GetSeWiseCustomerOsDetails(DashboardFilter dbFilter, out int totalRows)
        {
            totalRows = 0;
            var seWiseOsDetails = new List<CustomerOsDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var action = string.IsNullOrWhiteSpace(dbFilter.SearchTxt)
                ? OsAction.SeWiseCustomerOs
                : OsAction.SeWiseCustomerOsWithSearch;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, SeUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return seWiseOsDetails;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            var pageIndex = dbFilter.Skip + 1;
            seWiseOsDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new CustomerOsDetail
            {
                SlNo = index + pageIndex,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                OutstandingDays = GetOutstandingDays(dbFilter, dataRow.Field<decimal>("TotalOutstanding"), currentSale),
                Outstanding = dataRow.Field<decimal>("TotalOutstanding"),
                CreditLimit = dataRow.Field<decimal>("CreditLimit"),
                CriticalPayment = dataRow.Field<decimal>("CriticalPayment"),
                ZeroToFourteenDays = dataRow.Field<decimal>("0To14Days"),
                FourteenToTwentyEightDays = dataRow.Field<decimal>("14To28Days"),
                TwentyEightToFiftyDays = dataRow.Field<decimal>("28To50Days"),
                FiftyToSeventyDays = dataRow.Field<decimal>("50To70Days"),
                MoreThanSeventyDays = dataRow.Field<decimal>("MoreThan70Days"),
                TotalRows = dataRow.Field<int>("TotalRows")
            }).ToList();

            totalRows = seWiseOsDetails.First().TotalRows;

            return seWiseOsDetails;
        }

        #endregion

        #region CS wise outstanding detail

        public List<OsCustDetail> GetCsWiseOsDetails(DashboardFilter dbFilter)
        {
            var seWiseOsDetails = new List<OsCustDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, OsAction.CsWiseOsDetails);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, CsUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return seWiseOsDetails;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            seWiseOsDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new OsCustDetail
            {
                SlNo = index + 1,
                CustomerType = dataRow.Field<string>("CustomerType"),
                NoOfCustomers = dataRow.Field<int>("TotalCustomer"),
                OutstandingDays = GetOutstandingDays(dbFilter, dataRow.Field<decimal>("TotalOutstanding"), currentSale),
                Outstanding = dataRow.Field<decimal>("TotalOutstanding"),
                CreditLimit = dataRow.Field<decimal>("CreditLimit"),
                CriticalPayment = dataRow.Field<decimal>("CriticalPayment"),
                ZeroToFourteenDays = dataRow.Field<decimal>("0To14Days"),
                FourteenToTwentyEightDays = dataRow.Field<decimal>("14To28Days"),
                TwentyEightToFiftyDays = dataRow.Field<decimal>("28To50Days"),
                FiftyToSeventyDays = dataRow.Field<decimal>("50To70Days"),
                MoreThanSeventyDays = dataRow.Field<decimal>("MoreThan70Days")
            }).ToList();

            return seWiseOsDetails;
        }

        public List<CsWiseCustomerOsDetail> GetCsWiseCustomerOsDetails(DashboardFilter dbFilter, out int totalRows)
        {
            totalRows = 0;
            var csWiseOsDetails = new List<CsWiseCustomerOsDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var action = string.IsNullOrWhiteSpace(dbFilter.SearchTxt) ? OsAction.CsWiseCustomerOs : OsAction.CsWiseCustomerOsWithSearch;
            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, CsUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return csWiseOsDetails;

            var currentSale = DbCommon.GetCurrentSale(dbFilter);
            var pageIndex = dbFilter.Skip + 1;
            csWiseOsDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new CsWiseCustomerOsDetail
            {
                SlNo = index + pageIndex,
                BranchCode = dataRow.Field<string>("BranchCode"),
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                OutstandingDays = GetOutstandingDays(dbFilter, dataRow.Field<decimal>("TotalOutstanding"), currentSale),
                Outstanding = dataRow.Field<decimal>("TotalOutstanding"),
                CreditLimit = dataRow.Field<decimal>("CreditLimit"),
                CriticalPayment = dataRow.Field<decimal>("CriticalPayment"),
                ZeroToFourteenDays = dataRow.Field<decimal>("0To14Days"),
                FourteenToTwentyEightDays = dataRow.Field<decimal>("14To28Days"),
                TwentyEightToFiftyDays = dataRow.Field<decimal>("28To50Days"),
                FiftyToSeventyDays = dataRow.Field<decimal>("50To70Days"),
                MoreThanSeventyDays = dataRow.Field<decimal>("MoreThan70Days"),
                TotalRows = dataRow.Field<int>("TotalRows")
            }).ToList();

            totalRows = csWiseOsDetails.First().TotalRows;

            return csWiseOsDetails;
        }

        #endregion

        public OsCustomer GetOsSpecificCustomer(string customerCode)
        {
            using (var db = new garaazEntities())
            {
                var os = db.Outstandings.AsNoTracking().FirstOrDefault(o => o.PartyCode == customerCode);
                if (os == null) return null;

                var osCustomer = new OsCustomer
                {
                    PartyCode = os.PartyCode,
                    PartyName = os.PartyName,
                    CustomerType = os.CustomerType,
                    SalesExecutiveOrBranchCode = os.SalesExecutiveOrBranchCode,
                    PendingBills = os.PendingBills,
                    LessThan7Days = os.LessThan7Days,
                    C7To14Days = os.C7To14Days,
                    C14To21Days = os.C14To21Days,
                    C21To28Days = os.C21To28Days,
                    C28To35Days = os.C28To35Days,
                    C35To50Days = os.C35To50Days,
                    C50To70Days = os.C50To70Days,
                    MoreThan70Days = os.MoreThan70Days
                };

                return osCustomer;
            }
        }

        private static OsInfoResponse GetOsInfoResponse(DashboardFilter dbFilter, OsAction action, decimal currentSale, string dateInfo, string footer, string category)
        {
            decimal totalOs = 0.0M, criticalPayment = 0.0M;
            var sqlParams = GetSqlParams(dbFilter, action);

            string sp;
            if (action == OsAction.RoWiseTotalOs)
                sp = RoUspName;
            else if (action == OsAction.SeWiseTotalOs)
                sp = SeUspName;
            else if (action == OsAction.CsWiseTotalOs)
                sp = CsUspName;
            else
                throw new Exception("Outstanding action not supported.");

            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, sp, sqlParams);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var dataRow = ds.Tables[0].Rows[0];
                totalOs = dataRow.Field<decimal?>("TotalOs") ?? 0;
                criticalPayment = dataRow.Field<decimal?>("TotalOsAmount") ?? 0;
            }

            var criticalPaymentTxt = criticalPayment <= 0 ? "0" : $"{Math.Round(criticalPayment):#,###}";
            var osDays = GetOutstandingDays(dbFilter, totalOs, currentSale);

            return new OsInfoResponse
            {
                DateInfo = dateInfo,
                TotalOutstanding = totalOs <= 0 ? "0" : $"{Math.Round(totalOs):#,###}",
                CriticalPayment = $"Critical Payment: {criticalPaymentTxt}",
                OutstandingDays = $"Outstanding days: {osDays}",
                Footer = footer,
                Category = category
            };
        }

        private static SqlParameter[] GetSqlParams(DashboardFilter dbFilter, OsAction action)
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

            // Check Orderby column name exist in the CustomerOsDetail model if yes then set Orderby column name
            dbFilter.SortBy = GetSortColumn(dbFilter);

            var sqlParams = new[] {
                new SqlParameter("@Action",action),
                new SqlParameter("@Role",role),
                new SqlParameter("@UserId",dbFilter.UserId),
                new SqlParameter("@StartDate",dbFilter.StartDate),
                new SqlParameter("@EndDate",dbFilter.EndDate),
                new SqlParameter("@BranchCode",dbFilter.BranchCode),
                new SqlParameter("@CustomerType",dbFilter.CustomerType),
                new SqlParameter("@Skip",dbFilter.Skip),
                new SqlParameter("@Take",dbFilter.PageSize),
                new SqlParameter("@SearchTxt",dbFilter.SearchTxt),
                new SqlParameter("@SortBy",dbFilter.SortBy),
                new SqlParameter("@SortOrder",dbFilter.SortOrder)
            };

            return sqlParams;
        }

        private static decimal GetOutstandingDays(DashboardFilter dbFilter, decimal totalOutstanding, decimal currentSale)
        {
            var totalDays = DbCommon.GetDayDifference(dbFilter);

            // Calculate outstanding days
            var perDaySale = totalDays > 0 ? currentSale / totalDays : currentSale;
            var outstandingDays = perDaySale > 0 ? Math.Round(totalOutstanding / perDaySale, 2) : 0;

            return outstandingDays;
        }

        private static string GetSortColumn(DashboardFilter dbFilter)
        {
            string sortColumn = null;
            if (string.IsNullOrWhiteSpace(dbFilter.SortBy)) return sortColumn;
            // Check Orderby column name exist in the CustomerOsDetail model if yes then set Orderby column name
            var isColumnValid = new CustomerOsDetail().GetType().GetProperty(dbFilter.SortBy) != null ? true : false;

            if (isColumnValid)
            {
                switch (dbFilter.SortBy)
                {
                    case "SlNo":
                        sortColumn = null;
                        break;

                    case "OutstandingDays":
                        sortColumn = "TotalOutstanding";
                        break;

                    case "Outstanding":
                        sortColumn = "TotalOutstanding";
                        break;

                    case "ZeroToFourteenDays":
                        sortColumn = "[0To14Days]";
                        break;

                    case "FourteenToTwentyEightDays":
                        sortColumn = "[14To28Days]";
                        break;

                    case "TwentyEightToFiftyDays":
                        dbFilter.SortBy = "[28To50Days]";
                        break;

                    case "FiftyToSeventyDays":
                        sortColumn = "[50To70Days]";
                        break;

                    case "MoreThanSeventyDays":
                        sortColumn = "[MoreThan70Days]";
                        break;
                    default: sortColumn =dbFilter.SortBy;
                        break;
                }
            }
            return sortColumn;
        }
    }
}