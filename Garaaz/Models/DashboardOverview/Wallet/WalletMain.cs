using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Garaaz.Models.DashboardOverview.Wallet
{
    public class WalletMain
    {
        private const string UspName = "dbo.usp_Dashboard_WalletDetail";

        public WalletInfoResponse GetWalletBalanceInfo(WalletDbFilter dbFilter)
        {
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            var walletBal = GetAllWalletsBalance(dbFilter);
            var sale = DbCommon.GetCurrentSale(dbFilter);

            // Calculate payout of sales percent
            var totalDays = DbCommon.GetDayDifference(dbFilter);
            var averageSale = totalDays > 0 ? sale / totalDays : sale;
            var payoutPercent = averageSale > 0 ? Math.Round(walletBal / averageSale, 2) : 0;

            var walletBalTxt = walletBal > 0 ? $"{Math.Round(walletBal):#,###}" : "0";
            return new WalletInfoResponse
            {
                DateInfo = dateInfo,
                TotalWalletBalance = walletBalTxt,
                PayoutOfSalesPercentage = payoutPercent
            };
        }

        public List<WalletInfoResponse> GetSubGroupWiseWallets(WalletDbFilter dbFilter)
        {
            var walletDetails = new List<WalletInfoResponse>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            var sale = DbCommon.GetCurrentSale(dbFilter);

            var roWalletInfo = GetWalletInfoResponse(dbFilter, WalletAction.WalletByRoWise, sale, dateInfo, "RO Wise", "RO");
            var seWalletInfo = GetWalletInfoResponse(dbFilter, WalletAction.WalletBySeWise, sale, dateInfo,
                "Sales Executive Wise", "SE");
            var csWalletInfo = GetWalletInfoResponse(dbFilter, WalletAction.WalletByCsWise, sale, dateInfo,
                "Customer Segment Wise", "CS");

            walletDetails.Add(roWalletInfo);
            walletDetails.Add(seWalletInfo);
            walletDetails.Add(csWalletInfo);

            return walletDetails;
        }

        #region RO wise wallet detail

        public List<WalletDetail> GetRoWiseWallets(WalletDbFilter dbFilter)
        {
            var action = string.IsNullOrWhiteSpace(dbFilter.BranchCode) ? WalletAction.RoWiseWallet : WalletAction.RoWiseBranchCustomersByType;
            return GetWalletDetails(dbFilter, action, "RO");
        }

        public List<RoWiseBranchWallet> GetRoWiseBranchWallets(WalletDbFilter dbFilter)
        {
            var rowBranchWalletDetails = new List<RoWiseBranchWallet>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, WalletAction.RoWiseBranchWallet);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName,
                sqlParams);
            if (ds.Tables.Count == 0) return rowBranchWalletDetails;

            var wallets = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchName = dataRow.Field<string>("BranchName"),
                //CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCustomers = dataRow.Field<int>("NumberOfCustomers"),
                AvgSale = dataRow.Field<decimal>("AvgSale"),
                WalletBalance = dataRow.Field<decimal>("WalletBalance")
            }).ToList();

            var slNo = 1;
            foreach (var wallet in wallets)
            {
                rowBranchWalletDetails.Add(new RoWiseBranchWallet
                {
                    SlNo = slNo,
                    BranchCode = wallet.BranchCode,
                    BranchName = wallet.BranchName,
                    NumberOfCustomers = wallet.NumberOfCustomers,
                    AverageSale = Math.Round(wallet.AvgSale, 2),
                    WalletBalance = wallet.WalletBalance,
                    PayoutOfSalesPercentage = wallet.AvgSale > 0 ? Math.Round(wallet.WalletBalance / wallet.AvgSale, 4) : 0
                });

                slNo++;
            }

            return rowBranchWalletDetails;
        }

        public List<CustomerWallet> GetRoWiseBranchCustomers(WalletDbFilter dbFilter, out int totalRows)
        {
            return GetCustomerWallets(dbFilter, WalletAction.RoWiseBranchCustomers, out totalRows);
        }

        #endregion

        #region SE wise wallet detail

        public List<WalletDetail> GetSeWiseWallets(WalletDbFilter dbFilter)
        {
            var action = string.IsNullOrWhiteSpace(dbFilter.BranchCode) ? WalletAction.SeWiseWallet : WalletAction.SeWiseBranchCustomersByType;
            return GetWalletDetails(dbFilter, action, "SE");
        }

        public List<SeWiseBranchWallet> GetSeWiseBranchWallets(WalletDbFilter dbFilter)
        {
            var sewBranchWalletDetails = new List<SeWiseBranchWallet>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, WalletAction.SeWiseBranchWallet);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName,
                sqlParams);
            if (ds.Tables.Count == 0) return sewBranchWalletDetails;

            var wallets = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                SalesExecName = dataRow.Field<string>("SalesExecName"),
                //CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCustomers = dataRow.Field<int>("NumberOfCustomers"),
                AvgSale = dataRow.Field<decimal>("AvgSale"),
                WalletBalance = dataRow.Field<decimal>("WalletBalance")
            }).ToList();

            var slNo = 1;
            foreach (var wallet in wallets)
            {
                sewBranchWalletDetails.Add(new SeWiseBranchWallet
                {
                    SlNo = slNo,
                    BranchCode = wallet.BranchCode,
                    SalesExecName = wallet.SalesExecName,
                    NumberOfCustomers = wallet.NumberOfCustomers,
                    AverageSale = Math.Round(wallet.AvgSale, 2),
                    WalletBalance = wallet.WalletBalance,
                    PayoutOfSalesPercentage = wallet.AvgSale > 0 ? Math.Round(wallet.WalletBalance / wallet.AvgSale, 4) : 0
                });

                slNo++;
            }

            return sewBranchWalletDetails;
        }

        public List<CustomerWallet> GetSeWiseBranchCustomers(WalletDbFilter dbFilter, out int totalRows)
        {
            return GetCustomerWallets(dbFilter, WalletAction.SeWiseBranchCustomers, out totalRows);
        }

        #endregion

        #region CS wise wallet detail

        public List<WalletDetail> GetCsWiseWallets(WalletDbFilter dbFilter)
        {
            return GetWalletDetails(dbFilter, WalletAction.CsWiseWallet, "CS");
        }

        public List<CustomerWallet> GetCsWiseCustomers(WalletDbFilter dbFilter, out int totalRows)
        {
            return GetCustomerWallets(dbFilter, WalletAction.CsWiseCustomersByType, out totalRows);
        }

        #endregion

        public List<WalCustomerDetail> GetCustomerDetails(WalletDbFilter dbFilter, out int totalRows)
        {
            totalRows = 0;

            var customerDetails = new List<WalCustomerDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, WalletAction.CustomerDetail);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return customerDetails;

            var pageIndex = dbFilter.Skip + 1;
            customerDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new WalCustomerDetail
            {
                SlNo = index + pageIndex,
                DateOfTransaction = dataRow.Field<DateTime>("DateOfTransaction").ToString("d-MMM-yyyy"),
                TransactionDetails = dataRow.Field<string>("TransactionDetails"),
                Amount = dataRow.Field<decimal>("Amount"),
                TotalRows = dataRow.Field<int>("TotalRows")
            }).ToList();

            totalRows = customerDetails.First().TotalRows;

            return customerDetails;
        }

        #region Helper Methods

        private static decimal GetAllWalletsBalance(WalletDbFilter dbFilter)
        {
            var sqlParams = GetSqlParams(dbFilter, WalletAction.TotalWalletBalance);

            var objWalletSum = SqlHelper.ExecuteScalar(Connection.ConnectionString, CommandType.StoredProcedure,
                UspName, sqlParams);

            return Convert.ToDecimal(objWalletSum);
        }

        private static SqlParameter[] GetSqlParams(WalletDbFilter dbFilter, WalletAction action)
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

            // Calculate first day of month and last day of previous month
            var endDate = dbFilter.EndDate ?? DateTime.Now;
            var month = new DateTime(endDate.Year, endDate.Month, 1);
            var firstDayOfMonth = new DateTime(endDate.Year, endDate.Month, 1);
            var prevMonthLastDay = month.AddDays(-1);

            // Set search value with sql LIKE operator (for performance)
            var searchTxt = string.IsNullOrWhiteSpace(dbFilter.SearchTxt) ? null : $"%{dbFilter.SearchTxt}%";

            var sortBy = GetSortColumn(dbFilter, action);

            var sqlParams = new[]
            {
                new SqlParameter("@Action", action),
                new SqlParameter("@Role", role),
                new SqlParameter("@UserId", dbFilter.UserId),
                new SqlParameter("@StartDate", dbFilter.StartDate),
                new SqlParameter("@EndDate", dbFilter.EndDate),
                new SqlParameter("@CustomerType", dbFilter.CustomerType),
                new SqlParameter("@BranchCode", dbFilter.BranchCode),
                new SqlParameter("@Skip", dbFilter.Skip),
                new SqlParameter("@Take", dbFilter.PageSize),
                new SqlParameter("@SearchTxt",searchTxt),
                new SqlParameter("@CustomerCode", dbFilter.CustomerCode),
                new SqlParameter("@FirstDayOfMonth", firstDayOfMonth),
                new SqlParameter("@LastDayOfPrevMonth", prevMonthLastDay),
                new SqlParameter("@SortBy",sortBy),
                new SqlParameter("@SortOrder",dbFilter.SortOrder)
            };

            return sqlParams;
        }

        private static WalletInfoResponse GetWalletInfoResponse(WalletDbFilter dbFilter, WalletAction action,
            decimal sale, string dateInfo, string footer, string category)
        {
            var sqlParams = GetSqlParams(dbFilter, action);
            var objWalletSum = SqlHelper.ExecuteScalar(Connection.ConnectionString, CommandType.StoredProcedure,
                UspName, sqlParams);
            var walletBal = objWalletSum is DBNull ? 0 : Convert.ToDecimal(objWalletSum);

            // Calculate 'payout of sales' percentage
            var totalDays = DbCommon.GetDayDifference(dbFilter);
            var averageSale = totalDays > 0 ? sale / totalDays : sale;
            var payoutPercent = averageSale != 0 ? Math.Round(walletBal / averageSale, 2) : 0;

            var walletBalTxt = walletBal <= 0 ? "0" : $"{Math.Round(walletBal):#,###}";
            return new WalletInfoResponse
            {
                DateInfo = dateInfo,
                TotalWalletBalance = walletBalTxt,
                PayoutOfSalesPercentage = payoutPercent,
                Footer = footer,
                Category = category
            };
        }

        private static List<WalletDetail> GetWalletDetails(WalletDbFilter dbFilter, WalletAction action, string subGroup)
        {
            var walletDetails = new List<WalletDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName,
                sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return walletDetails;

            var wallets = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCustomers = dataRow.Field<int>("NumberOfCustomers"),
                AvgSale = dataRow.Field<decimal>("AvgSale"),
                WalletBalance = dataRow.Field<decimal>("WalletBalance"),
                dbFilter.BranchCode
            }).ToList();

            var slNo = 1;
            foreach (var wallet in wallets)
            {
                walletDetails.Add(new WalletDetail
                {
                    SlNo = slNo,
                    CustomerType = wallet.CustomerType,
                    NumberOfCustomers = wallet.NumberOfCustomers,
                    AverageSale = Math.Round(wallet.AvgSale, 2),
                    WalletBalance = wallet.WalletBalance,
                    PayoutOfSalesPercentage = wallet.AvgSale > 0 ? Math.Round(wallet.WalletBalance / wallet.AvgSale, 4) : 0,
                    BranchCode = wallet.BranchCode,
                    SubGroup = subGroup
                });

                slNo++;
            }

            return walletDetails;
        }

        private static List<CustomerWallet> GetCustomerWallets(WalletDbFilter dbFilter, WalletAction action, out int totalRows)
        {
            totalRows = 0;
            var custWallets = new List<CustomerWallet>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return custWallets;

            var pageIndex = dbFilter.Skip + 1;
            custWallets = ds.Tables[0].AsEnumerable().Select(dataRow => new CustomerWallet
            {
                SlNo = pageIndex,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                AverageSale = Math.Round(dataRow.Field<decimal?>("AverageSale") ?? 0, 2),
                WalletBalance = dataRow.Field<decimal?>("WalletBalance") ?? 0,
                PayoutOfSalesPercentage = dataRow.Field<decimal?>("AverageSale") > 0 ? Math.Round(dataRow.Field<decimal?>("WalletBalance") ?? 0 / dataRow.Field<decimal?>("AverageSale") ?? 0, 4) : 0,
                PayoutOfSalesPercentageTxt = (dataRow.Field<decimal?>("AverageSale") > 0 ? Math.Round(dataRow.Field<decimal?>("WalletBalance") ?? 0 / dataRow.Field<decimal?>("AverageSale") ?? 0, 4) : 0).ToString("P")
            }).ToList();

            totalRows = ds.Tables[0].Rows[0] != null ? ds.Tables[0].Rows[0].Field<int>("TotalRows") : 0;

            //var wallets = ds.Tables[0].AsEnumerable().Select(dataRow => new
            //{
            //    CustomerCode = dataRow.Field<string>("CustomerCode"),
            //    CustomerName = dataRow.Field<string>("CustomerName"),
            //    CustomerType = dataRow.Field<string>("CustomerType"),
            //    AvgSale = dataRow.Field<decimal?>("AvgSale") ?? 0,
            //    WalletBalance = dataRow.Field<decimal?>("WalletBalance") ?? 0,
            //    TotalRows = dataRow.Field<int>("TotalRows")
            //}).ToList();

            //totalRows = wallets.First().TotalRows;

            //var slNo = 1;
            //foreach (var wallet in wallets)
            //{
            //    var posPercent = wallet.AvgSale > 0 ? Math.Round(wallet.WalletBalance / wallet.AvgSale, 4) : 0;
            //    custWallets.Add(new CustomerWallet
            //    {
            //        SlNo = slNo,
            //        CustomerCode = wallet.CustomerCode,
            //        CustomerName = wallet.CustomerName,
            //        CustomerType = wallet.CustomerType,
            //        AverageSale = Math.Round(wallet.AvgSale, 2),
            //        WalletBalance = wallet.WalletBalance,
            //        PayoutOfSalesPercentage = posPercent,
            //        PayoutOfSalesPercentageTxt = posPercent.ToString("P")
            //    });

            //    slNo++;
            //}

            return custWallets;
        }

        private static string GetSortColumn(DashboardFilter dbFilter, WalletAction action)
        {
            string sortColumn = null;
            if (string.IsNullOrWhiteSpace(dbFilter.SortBy)) return sortColumn;

            if (action == WalletAction.CustomerDetail)
            {
                var isColumnValid = new WalCustomerDetail().GetType().GetProperty(dbFilter.SortBy) != null ? true : false;

                if (isColumnValid)
                {
                    if (dbFilter.SortBy != "SlNo")
                    {
                        sortColumn = dbFilter.SortBy;
                    }
                }
            }
            else if (action == WalletAction.RoWiseBranchCustomers || action == WalletAction.SeWiseBranchCustomers|| action == WalletAction.CsWiseCustomersByType)
            {
                var isColumnValid = new CustomerWallet().GetType().GetProperty(dbFilter.SortBy) != null ? true : false;

                if (isColumnValid)
                {
                    switch (dbFilter.SortBy)
                    {
                        case "SlNo":
                            sortColumn = null;
                            break;

                        case "PayoutOfSalesPercentage":
                            sortColumn = "WalletBalance";
                            break;

                        case "PayoutOfSalesPercentageTxt":
                            sortColumn = "WalletBalance";
                            break;

                        default:
                            sortColumn = dbFilter.SortBy;
                            break;
                    }
                }
            }
            return sortColumn;
        }
        #endregion
    }
}

