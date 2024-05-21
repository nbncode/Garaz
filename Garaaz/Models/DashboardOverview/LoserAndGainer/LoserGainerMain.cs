using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Garaaz.Models.DashboardOverview.LoserAndGainer
{
    public class LoserGainerMain
    {
        private const string UspName = "dbo.usp_Dashboard_LooserAndGainersDetails";

        public LoserAndGainerInfoResponse GetLoserAndGainerInfo(LooserGainerFilter dbFilter)
        {
            var lgInfo = GetLooserGainerInfoResponse(dbFilter, LGAction.LooserGainerInfo, "", "");

            return lgInfo;
        }

        public List<LoserAndGainerInfoResponse> GetSubGroupWiseLooserAndGainers(LooserGainerFilter dbFilter)
        {
            var lgDetails = new List<LoserAndGainerInfoResponse>();

            var roWalletInfo = GetLooserGainerInfoResponse(dbFilter, LGAction.LooserGainerByRoWise, "RO Wise", "RO");
            var seWalletInfo = GetLooserGainerInfoResponse(dbFilter, LGAction.LooserGainerBySeWise, "Sales Executive Wise", "SE");
            var csWalletInfo = GetLooserGainerInfoResponse(dbFilter, LGAction.LooserGainerByCsWise, "Customer Segment Wise", "CS");

            lgDetails.Add(roWalletInfo);
            lgDetails.Add(seWalletInfo);
            lgDetails.Add(csWalletInfo);

            return lgDetails;
        }

        #region RO wise Loosers & Gainers detail

        public List<LooserAndGainersDetails> RoWiseLGDetailS(LooserGainerFilter dbFilter)
        {
            return GetLGDetails(dbFilter, LGAction.RoWiseLGDetails, "RO");
        }

        public List<BranchWiseLGInfo> GetRoWiseBranchDetails(LooserGainerFilter dbFilter)
        {
            return GetLGBranchDetails(dbFilter, LGAction.RoWiseBranchDetails, "RO");
        }

        public List<CustomerWiseLGInfo> GetRoGainerCustomerDetails(LooserGainerFilter dbFilter, out int totalRows)
        {
            var action = LGAction.ROGainerCustomerDetail;
            return GetCustomerDetails(dbFilter, action, out totalRows);
        }

        public List<CustomerWiseLGInfo> GetROLooserCustomerDetails(LooserGainerFilter dbFilter, out int totalRows)
        {
            var action = LGAction.ROLooserCustomerDetail;
            return GetCustomerDetails(dbFilter, action, out totalRows);
        }

        #endregion

        #region SE wise Loosers & Gainers detail

        public List<LooserAndGainersDetails> SEWiseLGDetailS(LooserGainerFilter dbFilter)
        {
            return GetLGDetails(dbFilter, LGAction.SeWiseLGDetails, "SE");
        }

        public List<BranchWiseLGInfo> GetSeWiseBranchDetails(LooserGainerFilter dbFilter)
        {
            return GetLGSalesExeDetails(dbFilter, LGAction.SeWiseBranchDetails, "SE");
        }

        public List<CustomerWiseLGInfo> GetSeGainerCustomerDetails(LooserGainerFilter dbFilter, out int totalRows)
        {
            var action = LGAction.SeGainerCustomerDetail;
            return GetCustomerDetails(dbFilter, action, out totalRows);
        }

        public List<CustomerWiseLGInfo> GetSeLooserCustomerDetails(LooserGainerFilter dbFilter, out int totalRows)
        {
            var action = LGAction.SeLooserCustomerDetail;
            return GetCustomerDetails(dbFilter, action,out totalRows);
        }

        #endregion

        #region CS wise Loosers & Gainers detail

        public List<LooserAndGainersDetails> CSWiseLGDetailS(LooserGainerFilter dbFilter)
        {
            return GetLGDetails(dbFilter, LGAction.CsWiseLGDetails, "CS");
        }

        public List<CustomerWiseLGInfo> GetCSGainerCustomerDetails(LooserGainerFilter dbFilter, out int totalRows)
        {
            var action = LGAction.CsGainerCustomerDetail;
            return GetCustomerDetails(dbFilter, action, out totalRows);
        }

        public List<CustomerWiseLGInfo> GetCsLooserCustomerDetails(LooserGainerFilter dbFilter, out int totalRows)
        {
            var action = LGAction.CsLooserCustomerDetail;
            return GetCustomerDetails(dbFilter, action, out totalRows);
        }

        #endregion

        #region Helper Methods
        private static SqlParameter[] GetSqlParams(LooserGainerFilter dbFilter, LGAction action)
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

            // Set search value with sql LIKE operator (for performance)
            var searchTxt = string.IsNullOrWhiteSpace(dbFilter.SearchTxt) ? null : $"%{dbFilter.SearchTxt}%";

            var sortBy = GetSortColumn(dbFilter, action);

            var sqlParams = new[] {
                 new SqlParameter("@Action",action),
                 new SqlParameter("@Role",role),
                 new SqlParameter("@UserId",dbFilter.UserId),
                 new SqlParameter("@StartDate",dbFilter.StartDate),
                 new SqlParameter("@EndDate",dbFilter.EndDate),
                 new SqlParameter("@CustomerType",dbFilter.CustomerType),
                 new SqlParameter("@BranchCode",dbFilter.BranchCode),
                 new SqlParameter("@Skip",dbFilter.Skip),
                 new SqlParameter("@Take",dbFilter.PageSize),
                 new SqlParameter("@SearchTxt",searchTxt),
                 new SqlParameter("@SalesExecUserId",dbFilter.SalesExecUserId),
                 new SqlParameter("@SortBy",sortBy),
                 new SqlParameter("@SortOrder",dbFilter.SortOrder)
             };

            return sqlParams;
        }

        private static string GetSortColumn(LooserGainerFilter dbFilter, LGAction action)
        {
            string sortColumn = null;
            if (string.IsNullOrWhiteSpace(dbFilter.SortBy)) return sortColumn;

            if (action == LGAction.ROGainerCustomerDetail || action == LGAction.ROLooserCustomerDetail || action == LGAction.SeGainerCustomerDetail || action == LGAction.SeLooserCustomerDetail || action == LGAction.CsGainerCustomerDetail || action == LGAction.CsLooserCustomerDetail)
            {
                var isColumnValid = new CustomerWiseLGInfo().GetType().GetProperty(dbFilter.SortBy) != null ? true : false;
                if (isColumnValid)
                {
                    if (dbFilter.SortBy != "SlNo")
                    {
                        sortColumn = dbFilter.SortBy;
                    }
                }
            }
            return sortColumn;
        }

        private static LoserAndGainerInfoResponse GetLooserGainerInfoResponse(LooserGainerFilter dbFilter, LGAction action, string footer, string category)
        {
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            int loosers = 0; int gainers = 0;
            decimal totalLooserSale = 0; decimal totalLooserPrevSale = 0; decimal totalLoosercurrentSale = 0;
            decimal totalGainerSale = 0; decimal totalGainerPrevSale = 0; decimal totalGainercurrentSale = 0;
            decimal looserPercent = 0; decimal gainerPercent = 0;

            var sqlParams = GetSqlParams(dbFilter, action);

            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count > 0)
            {
                var customerData = ds.Tables[0].AsEnumerable().Select(dataRow => new
                {
                    WorkShopId = dataRow.Field<int>("WorkShopId"),
                    CurrentSale = dataRow.Field<decimal>("CurrentSale"),
                    PreviousSale = dataRow.Field<decimal>("PreviousSale")
                }).ToList();

                foreach (var data in customerData)
                {
                    // add gainers data
                    if (data.CurrentSale > data.PreviousSale)
                    {
                        gainers++;
                        totalGainerPrevSale += data.PreviousSale;
                        totalGainercurrentSale += data.CurrentSale;
                    }
                    // add gainers data
                    if (data.CurrentSale <= data.PreviousSale)
                    {
                        loosers++;
                        totalLooserPrevSale += data.PreviousSale;
                        totalLoosercurrentSale += data.CurrentSale;
                    }
                }
                totalGainerSale = totalGainercurrentSale - totalGainerPrevSale;
                totalLooserSale = totalLooserPrevSale - totalLoosercurrentSale;

                //looserPercent = (totalLooserPrevSale - totalLoosercurrentSale)*100 / (totalLoosercurrentSale > 0 ? totalLoosercurrentSale : 1);

                looserPercent = DbCommon.CalculatePercentage(totalLoosercurrentSale, totalLooserPrevSale);

                gainerPercent = DbCommon.CalculatePercentage(totalGainerPrevSale, totalGainercurrentSale);

                // convert in lack
                totalLooserSale = Math.Round(totalLooserSale > 0 ? totalLooserSale / 100000 : totalLooserSale, 2);
                totalGainerSale = Math.Round(totalGainerSale > 0 ? totalGainerSale / 100000 : totalGainerSale, 2);
            }

            return new LoserAndGainerInfoResponse
            {
                DateInfo = dateInfo,
                Loosers = $"Loosers: {loosers} ({totalLooserSale} L - {Math.Round(looserPercent, 2)}%)",
                Gainers = $"Gainers: {gainers} ({totalGainerSale} L - {Math.Round(gainerPercent, 2)}%)",
                Footer = footer,
                Category = category
            };
        }

        #region Get Loosers and Gainers Details

        private static List<LooserAndGainersDetails> GetLGDetails(LooserGainerFilter dbFilter, LGAction action, string subGroup)
        {
            var lGDetails = new List<LooserAndGainersDetails>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure,UspName,
                sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return lGDetails;

            var data = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                WorkShopId = dataRow.Field<int>("WorkShopId"),
                CustomerType = dataRow.Field<string>("CustomerType")?.ToUpper(),
                CurrentOrderDays = dataRow.Field<int>("CurrentOrderDays"),
                PreviousOrderDays = dataRow.Field<int>("PreviousOrderDays"),
                CurrentSale = dataRow.Field<decimal>("CurrentSale"),
                PreviousSale = dataRow.Field<decimal>("PreviousSale"),
                IsGainer = dataRow.Field<bool>("IsGainer")
            }).OrderBy(a => a.CustomerType).ToList();

            var groupByCustomers = data.GroupBy(a => a.CustomerType).ToList();

            var slNo = 1;
            foreach (var cust in groupByCustomers)
            {
                var customers = data.Where(a => a.CustomerType == cust.Key);

                var gainers = customers.Where(a => a.IsGainer == true);
                var loosers = customers.Where(a => a.IsGainer == false);

                var gainedRetailValue = Math.Round(gainers.Sum(a => a.CurrentSale) - gainers.Sum(a => a.PreviousSale), 2);

                var lostRetailValue = Math.Round(loosers.Sum(a => a.PreviousSale) - loosers.Sum(a => a.CurrentSale), 2);

                var currentOrderDays = cust.Sum(a => a.CurrentOrderDays);
                var previousOrderDays = cust.Sum(a => a.PreviousOrderDays);

                var growthDays = DbCommon.CalculateGrowthPercentage(previousOrderDays, currentOrderDays);

                var currentOrderValue = cust.Sum(a => a.CurrentSale);
                var previousOrderValue = cust.Sum(a => a.PreviousSale);

                var growthValue = DbCommon.CalculatePercentage(previousOrderValue, currentOrderValue);

                lGDetails.Add(new LooserAndGainersDetails
                {
                    SlNo = slNo,
                    CustomerType = cust.Key,
                    NumberOfCustomers = customers.Count(),
                    Loosers = loosers.Count(),
                    LostRetailValue = lostRetailValue,
                    Gainers = gainers.Count(),
                    GainedRetailValue = gainedRetailValue,
                    CurrentOrderDays = currentOrderDays,
                    PreviousOrderDays = previousOrderDays,
                    GrowthDays = Math.Round(growthDays, 2),
                    CurrentOrderValue = Math.Round(cust.Sum(a => a.CurrentSale), 2),
                    PreviousOrderValue = Math.Round(cust.Sum(a => a.PreviousSale), 2),
                    GrowthValue = Math.Round(growthValue, 2),
                    SubGroup = subGroup
                });

                slNo++;
            }

            return lGDetails;
        }
        #endregion

        #region Get Loosers and Gainers Branch Details

        private static List<BranchWiseLGInfo> GetLGBranchDetails(LooserGainerFilter dbFilter, LGAction action, string subGroup)
        {
            var lGDetails = new List<BranchWiseLGInfo>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName,
                sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return lGDetails;

            var data = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                WorkShopId = dataRow.Field<int>("WorkShopId"),
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchOrSalesName = dataRow.Field<string>("BranchOrSalesName"),
                CurrentOrderDays = dataRow.Field<int>("CurrentOrderDays"),
                PreviousOrderDays = dataRow.Field<int>("PreviousOrderDays"),
                CurrentSale = dataRow.Field<decimal>("CurrentSale"),
                PreviousSale = dataRow.Field<decimal>("PreviousSale"),
                IsGainer = dataRow.Field<bool>("IsGainer")
            }).OrderBy(a => a.BranchCode).ToList();

            var groupByBranch = data.GroupBy(a => a.BranchCode).ToList();

            var slNo = 1;
            foreach (var cust in groupByBranch)
            {
                var customers = data.Where(a => a.BranchCode == cust.Key);

                var gainers = customers.Where(a => a.IsGainer == true);
                var loosers = customers.Where(a => a.IsGainer == false);

                var gainedRetailValue = Math.Round(gainers.Sum(a => a.CurrentSale) - gainers.Sum(a => a.PreviousSale), 2);

                var lostRetailValue = Math.Round(loosers.Sum(a => a.PreviousSale) - loosers.Sum(a => a.CurrentSale), 2);

                var currentOrderDays = cust.Sum(a => a.CurrentOrderDays);
                var previousOrderDays = cust.Sum(a => a.PreviousOrderDays);

                var growthDays = DbCommon.CalculateGrowthPercentage(previousOrderDays, currentOrderDays);

                var currentOrderValue = cust.Sum(a => a.CurrentSale);
                var previousOrderValue = cust.Sum(a => a.PreviousSale);

                var growthValue = DbCommon.CalculatePercentage(previousOrderValue, currentOrderValue);

                lGDetails.Add(new BranchWiseLGInfo
                {
                    SlNo = slNo,
                    BranchCode = cust.Key,
                    BranchOrSalesName = cust.Select(a => a.BranchOrSalesName).FirstOrDefault(),
                    NumberOfCustomers = customers.Count(),
                    Loosers = loosers.Count(),
                    LostRetailValue = lostRetailValue,
                    Gainers = gainers.Count(),
                    GainedRetailValue = gainedRetailValue,
                    CurrentOrderDays = currentOrderDays,
                    PreviousOrderDays = previousOrderDays,
                    GrowthDays = Math.Round(growthDays, 2),
                    CurrentOrderValue = Math.Round(cust.Sum(a => a.CurrentSale), 2),
                    PreviousOrderValue = Math.Round(cust.Sum(a => a.PreviousSale), 2),
                    GrowthValue = Math.Round(growthValue, 2),
                    SubGroup = subGroup
                });

                slNo++;
            }

            return lGDetails;
        }

        private static List<BranchWiseLGInfo> GetLGSalesExeDetails(LooserGainerFilter dbFilter, LGAction action, string subGroup)
        {
            var lGDetails = new List<BranchWiseLGInfo>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName,
                sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return lGDetails;

            var data = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                WorkShopId = dataRow.Field<int>("WorkShopId"),
                SeUserId = dataRow.Field<string>("SeUserId"),
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchOrSalesName = dataRow.Field<string>("BranchOrSalesName"),
                CurrentOrderDays = dataRow.Field<int>("CurrentOrderDays"),
                PreviousOrderDays = dataRow.Field<int>("PreviousOrderDays"),
                CurrentSale = dataRow.Field<decimal>("CurrentSale"),
                PreviousSale = dataRow.Field<decimal>("PreviousSale"),
                IsGainer = dataRow.Field<bool>("IsGainer")
            }).OrderBy(a => a.BranchOrSalesName).ToList();

            var groupBySalesExe = data.GroupBy(a => a.SeUserId).ToList();

            var slNo = 1;
            foreach (var cust in groupBySalesExe)
            {
                var customers = data.Where(a => a.SeUserId == cust.Key);

                var gainers = customers.Where(a => a.IsGainer == true);
                var loosers = customers.Where(a => a.IsGainer == false);

                var gainedRetailValue = Math.Round(gainers.Sum(a => a.CurrentSale) - gainers.Sum(a => a.PreviousSale), 2);

                var lostRetailValue = Math.Round(loosers.Sum(a => a.PreviousSale) - loosers.Sum(a => a.CurrentSale), 2);

                var currentOrderDays = cust.Sum(a => a.CurrentOrderDays);
                var previousOrderDays = cust.Sum(a => a.PreviousOrderDays);

                var growthDays = DbCommon.CalculateGrowthPercentage(previousOrderDays, currentOrderDays);

                var currentOrderValue = cust.Sum(a => a.CurrentSale);
                var previousOrderValue = cust.Sum(a => a.PreviousSale);

                var growthValue = DbCommon.CalculatePercentage(previousOrderValue, currentOrderValue);

                lGDetails.Add(new BranchWiseLGInfo
                {
                    SlNo = slNo,
                    BranchCode = cust.Select(a => a.BranchCode).FirstOrDefault(),
                    SeUserId= cust.Key,
                    BranchOrSalesName = cust.Select(a => a.BranchOrSalesName).FirstOrDefault(),
                    NumberOfCustomers = customers.Count(),
                    Loosers = loosers.Count(),
                    LostRetailValue = lostRetailValue,
                    Gainers = gainers.Count(),
                    GainedRetailValue = gainedRetailValue,
                    CurrentOrderDays = currentOrderDays,
                    PreviousOrderDays = previousOrderDays,
                    GrowthDays = Math.Round(growthDays, 2),
                    CurrentOrderValue = Math.Round(cust.Sum(a => a.CurrentSale), 2),
                    PreviousOrderValue = Math.Round(cust.Sum(a => a.PreviousSale), 2),
                    GrowthValue = Math.Round(growthValue, 2),
                    SubGroup = subGroup
                });

                slNo++;
            }

            return lGDetails;
        }
        #endregion

        #region Get Loosers and Gainers Customers Details

        private static List<CustomerWiseLGInfo> GetCustomerDetails(LooserGainerFilter dbFilter, LGAction action,out int totalRows)
        {
            var lGDetails = new List<CustomerWiseLGInfo>();
            totalRows = 0;
            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName,
                sqlParams);

            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return lGDetails;

            lGDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) =>
            new CustomerWiseLGInfo
            {
                SlNo = index + 1,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                CurrentOrderDays = dataRow.Field<int>("CurrentOrderDays"),
                PreviousOrderDays = dataRow.Field<int>("PreviousOrderDays"),
                GrowthDays = dataRow.Field<decimal>("GrowthDays").ToString()+"%",
                CurrentOrderValue = dataRow.Field<decimal>("CurrentOrderValue"),
                PreviousOrderValue = dataRow.Field<decimal>("PreviousOrderValue"),
                GrowthValue = dataRow.Field<decimal>("GrowthValue").ToString() + "%"
            }).ToList();
            totalRows = ds.Tables[0].Rows[0].Field<int>("TotalRows");

            return lGDetails;
        }
        #endregion

        #endregion
    }
}