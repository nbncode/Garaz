using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Garaaz.Models.DashboardOverview.Customer
{
    public class CustomerMain
    {
        private const string UspName = "dbo.usp_Dashboard_CustomerDetail";

        public CustomerInfoResponse GetCustomerInfo(CustomerDbFilter dbFilter)
        {
            return GetCustomerInfoResponse(dbFilter, CustomerAction.AllCustomers);
        }

        public List<CustomerInfoResponse> GetSubGroupWiseCustomers(CustomerDbFilter dbFilter)
        {
            var cusInfos = new List<CustomerInfoResponse>();

            var roCusInfo = GetCustomerInfoResponse(dbFilter, CustomerAction.CustomerByRoWise, "RO Wise", "RO");
            var seCusInfo = GetCustomerInfoResponse(dbFilter, CustomerAction.CustomerBySeWise, "Sales Executive Wise", "SE");

            cusInfos.Add(roCusInfo);
            cusInfos.Add(seCusInfo);

            return cusInfos;
        }

        #region RO wise customer detail

        public List<CustomerDetail> GetRoWiseCustomers(CustomerDbFilter dbFilter)
        {
            return GetCustomers(dbFilter, CustomerAction.RoWiseCustomer);
        }

        public List<RoWiseBranchCustomer> GetRoWiseBranchCustomers(CustomerDbFilter dbFilter)
        {
            var rowBranchCustomers = new List<RoWiseBranchCustomer>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, CustomerAction.RoWiseBranchCustomer);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return rowBranchCustomers;

            var customers = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchName = dataRow.Field<string>("BranchName"),
                TotalCustomers = (decimal)dataRow.Field<int>("TotalCustomers"),
                BilledCustomers = (decimal)dataRow.Field<int>("BilledCustomers"),
                NonBilledCustomers = (decimal)dataRow.Field<int>("NonBilledCustomers")
            }).ToList();

            var slNo = 1;
            foreach (var customer in customers)
            {
                rowBranchCustomers.Add(new RoWiseBranchCustomer
                {
                    SlNo = slNo,
                    BranchCode = customer.BranchCode,
                    BranchName = customer.BranchName,
                    TotalCustomers = customer.TotalCustomers,
                    BilledCustomers = customer.BilledCustomers,
                    BilledCustomersRatio = customer.BilledCustomers / customer.TotalCustomers,
                    NonBilledCustomers = customer.NonBilledCustomers,
                    NonBilledCustomersRatio = customer.NonBilledCustomers / customer.TotalCustomers
                });

                slNo++;
            }

            return rowBranchCustomers;
        }

        public List<BranchCustomerDetail> GetRoWiseBilledCustomers(CustomerDbFilter dbFilter, out int totalRows)
        {
            return GetBranchCustomerDetails(dbFilter, CustomerAction.RoWiseBilledCustomers, out totalRows);
        }

        public List<BranchCustomerDetail> GetRoWiseNonBilledCustomers(CustomerDbFilter dbFilter, out int totalRows)
        {
            return GetBranchCustomerDetails(dbFilter, CustomerAction.RoWiseNonBilledCustomers, out totalRows);
        }

        #endregion

        #region SE wise customer detail

        public List<CustomerDetail> GetSeWiseCustomers(CustomerDbFilter dbFilter)
        {
            return GetCustomers(dbFilter, CustomerAction.SeWiseCustomer);
        }

        public List<SeWiseBranchCustomer> GetSeWiseBranchCustomers(CustomerDbFilter dbFilter)
        {
            var sewBranchCustomers = new List<SeWiseBranchCustomer>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, CustomerAction.SeWiseBranchCustomer);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return sewBranchCustomers;

            var customers = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                SalesExecName = dataRow.Field<string>("SalesExecName"),
                SalesExecUserId = dataRow.Field<string>("SalesExecUserId"),
                TotalCustomers = (decimal)dataRow.Field<int>("TotalCustomers"),
                BilledCustomers = (decimal)dataRow.Field<int>("BilledCustomers"),
                NonBilledCustomers = (decimal)dataRow.Field<int>("NonBilledCustomers")
            }).ToList();

            var slNo = 1;
            foreach (var customer in customers)
            {
                sewBranchCustomers.Add(new SeWiseBranchCustomer
                {
                    SlNo = slNo,
                    BranchCode = customer.BranchCode,
                    SalesExecName = customer.SalesExecName,
                    SalesExecUserId = customer.SalesExecUserId,
                    TotalCustomers = customer.TotalCustomers,
                    BilledCustomers = customer.BilledCustomers,
                    BilledCustomersRatio = customer.BilledCustomers / customer.TotalCustomers,
                    NonBilledCustomers = customer.NonBilledCustomers,
                    NonBilledCustomersRatio = customer.NonBilledCustomers / customer.TotalCustomers
                });

                slNo++;
            }

            return sewBranchCustomers;
        }

        public List<BranchCustomerDetail> GetSeWiseBilledCustomers(CustomerDbFilter dbFilter, out int totalRows)
        {
            return GetBranchCustomerDetails(dbFilter, CustomerAction.SeWiseBilledCustomers, out totalRows);
        }

        public List<BranchCustomerDetail> GetSeWiseNonBilledCustomers(CustomerDbFilter dbFilter, out int totalRows)
        {
            return GetBranchCustomerDetails(dbFilter, CustomerAction.SeWiseNonBilledCustomers, out totalRows);
        }

        #endregion

        private static CustomerInfoResponse GetCustomerInfoResponse(CustomerDbFilter dbFilter, CustomerAction action, string footer = null, string category = null)
        {
            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);

            decimal totalCustomers = 0, billedCustomers = 0, nonBilledCustomers = 0;
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var dataRow = ds.Tables[0].Rows[0];
                totalCustomers = dataRow.Field<int>("TotalCustomers");
                billedCustomers = dataRow.Field<int>("BilledCustomers");
                nonBilledCustomers = dataRow.Field<int>("NonBilledCustomers");
            }

            decimal nonBilledCustomersRatio = 0, billedCustomersRatio = 0;
            if (totalCustomers > 0)
            {
                nonBilledCustomersRatio = nonBilledCustomers / totalCustomers;
                billedCustomersRatio = billedCustomers / totalCustomers;
            }

            return new CustomerInfoResponse
            {
                DateInfo = dateInfo,
                TotalCustomers = totalCustomers,
                NonBilledCustomers = nonBilledCustomers,
                BilledCustomers = billedCustomers,
                NonBilledCustomersRatio = nonBilledCustomersRatio,
                BilledCustomersRatio = billedCustomersRatio,
                Footer = footer,
                Category = category
            };
        }

        private static SqlParameter[] GetSqlParams(CustomerDbFilter dbFilter, CustomerAction action)
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

            // Calculate first day of month and last day of previous month
            var endDate = dbFilter.EndDate ?? DateTime.Now;
            var month = new DateTime(endDate.Year, endDate.Month, 1);
            var firstDayOfMonth = new DateTime(endDate.Year, endDate.Month, 1);
            var prevMonthLastDay = month.AddDays(-1);

            // Calculate previous year start date and end date
            var startDate = dbFilter.StartDate ?? DateTime.Now;
            var prvYrStartDate = startDate.AddYears(-1);
            var prvYrEndDate = endDate.AddYears(-1);

            var sortBy = GetSortColumn(dbFilter,action);

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
                 new SqlParameter("@CurrentDate",DateTime.Now),
                 new SqlParameter("@FirstDayOfMonth",firstDayOfMonth),
                 new SqlParameter("@LastDayOfPrevMonth",prevMonthLastDay),
                 new SqlParameter("@PrvYrStartDate",prvYrStartDate),
                 new SqlParameter("@PrvYrEndDate",prvYrEndDate),
                 new SqlParameter("@SalesExecUserId",dbFilter.SalesExecUserId),
                 new SqlParameter("@SortBy",sortBy),
                 new SqlParameter("@SortOrder",dbFilter.SortOrder)
             };

            return sqlParams;
        }


        private static string GetSortColumn(DashboardFilter dbFilter, CustomerAction action)
        {
            string sortColumn = null;
            if (string.IsNullOrWhiteSpace(dbFilter.SortBy)) return sortColumn;

            if (action == CustomerAction.RoWiseBilledCustomers|| action == CustomerAction.RoWiseNonBilledCustomers|| action == CustomerAction.SeWiseBilledCustomers || action == CustomerAction.SeWiseNonBilledCustomers)
            {
                
                var isColumnValid = new BranchCustomerDetail().GetType().GetProperty(dbFilter.SortBy) != null ? true : false;
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

        private static List<CustomerDetail> GetCustomers(CustomerDbFilter dbFilter, CustomerAction action)
        {
            var customerDetails = new List<CustomerDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return customerDetails;

            var customers = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                TotalCustomers = (decimal)dataRow.Field<int>("TotalCustomers"),
                BilledCustomers = (decimal)dataRow.Field<int>("BilledCustomers"),
                NonBilledCustomers = (decimal)dataRow.Field<int>("NonBilledCustomers")
            }).ToList();

            var slNo = 1;
            foreach (var customer in customers)
            {
                customerDetails.Add(new CustomerDetail
                {
                    SlNo = slNo,
                    CustomerType = customer.CustomerType,
                    TotalCustomers = customer.TotalCustomers,
                    BilledCustomers = customer.BilledCustomers,
                    BilledCustomersRatio = customer.BilledCustomers / customer.TotalCustomers,
                    NonBilledCustomers = customer.NonBilledCustomers,
                    NonBilledCustomersRatio = customer.NonBilledCustomers / customer.TotalCustomers
                });

                slNo++;
            }

            return customerDetails;
        }

        private static List<BranchCustomerDetail> GetBranchCustomerDetails(CustomerDbFilter dbFilter, CustomerAction action, out int totalRows)
        {
            totalRows = 0;
            var branchCustomerDetails = new List<BranchCustomerDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return branchCustomerDetails;

            branchCustomerDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new BranchCustomerDetail
            {
                SlNo = index + 1,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                AvgSale = Math.Round(dataRow.Field<decimal>("AvgSale"), 2),
                CurrentMonthSale = dataRow.Table.Columns.Contains("CurrentMonthSale") ? Math.Round(dataRow.Field<decimal>("CurrentMonthSale"), 2) : 0,
                NonBilledFromDays = dataRow.Table.Columns.Contains("NonBilledFromDays") ? dataRow.Field<int>("NonBilledFromDays") : 0,
                PrvYrSale = Math.Round(dataRow.Field<decimal>("PrvYrSale"), 2),
                TotalRows = dataRow.Field<int>("TotalRows")
            }).ToList();

            totalRows = branchCustomerDetails.First().TotalRows;

            return branchCustomerDetails;
        }
    }
}