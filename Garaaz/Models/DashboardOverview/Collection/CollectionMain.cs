using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;

namespace Garaaz.Models.DashboardOverview.Collection
{
    public class CollectionMain
    {
        private const string UspName = "dbo.usp_Dashboard_CollectionDetail";

        public ColInfoResponse GetCollectionInfo(DashboardFilter dbFilter)
        {
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            var totalCollection = DbCommon.GetTotalCollectionByCurrentUserRole(dbFilter);

            var sale = DbCommon.GetCurrentSale(dbFilter);

            // Calculate collectionPercentage
            var collectionPer = sale > 0 ? totalCollection * 100 / sale : 0;

            // Prepare return response
            var totalCollectionTxt = totalCollection <= 0 ? "0" : $"{Math.Round(totalCollection):#,###}";

            return new ColInfoResponse
            {
                DateInfo = dateInfo,
                TotalCollection = totalCollectionTxt,
                CollectionPercentage = Math.Round(collectionPer, 2)
            };
        }

        public List<ColInfoResponse> GetSubGroupWiseCollection(ColDbFilter dbFilter)
        {
            var colInfos = new List<ColInfoResponse>();

            var sale = DbCommon.GetCurrentSale(dbFilter);

            var roColInfo = GetColInfoResponse(dbFilter, CollectionAction.CollectionByRoWise, sale, "RO Wise", "RO");
            var seColInfo = GetColInfoResponse(dbFilter, CollectionAction.CollectionBySeWise, sale, "Sales Executive Wise", "SE");
            var csColInfo = GetColInfoResponse(dbFilter, CollectionAction.CollectionByCsWise, sale, "Customer Segment Wise", "CS");

            colInfos.Add(roColInfo);
            colInfos.Add(seColInfo);
            colInfos.Add(csColInfo);

            return colInfos;
        }

        #region RO wise collection

        public List<ColDetail> GetRoWiseCollections(ColDbFilter dbFilter)
        {
            var action = string.IsNullOrWhiteSpace(dbFilter.BranchCode) ? CollectionAction.RoWiseCollection : CollectionAction.RoWiseBranchCustomersByType;
            return GetCollections(dbFilter, action, "RO");
        }

        public List<RoWiseBranchCol> GetRoWiseBranchCollections(ColDbFilter dbFilter)
        {
            var rowBranchCols = new List<RoWiseBranchCol>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, CollectionAction.RoWiseBranchCollection);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return rowBranchCols;

            rowBranchCols = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new RoWiseBranchCol
            {
                SlNo = index + 1,
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchName = dataRow.Field<string>("BranchName"),
                TotalCustomers = dataRow.Field<int>("NumberOfCustomers")
            }).ToList();

            return rowBranchCols;
        }

        public List<Customer> GetRoWiseBranchCustomers(ColDbFilter dbFilter, out int totalRows)
        {
            return GetCustomers(dbFilter, CollectionAction.RoWiseBranchCustomers, out totalRows);
        }

        #endregion

        #region SE wise collection

        public List<ColDetail> GetSeWiseCollections(ColDbFilter dbFilter)
        {
            var action = string.IsNullOrWhiteSpace(dbFilter.BranchCode) ? CollectionAction.SeWiseCollection : CollectionAction.SeWiseBranchCustomersByType;
            return GetCollections(dbFilter, action, "SE");
        }

        public List<SeWiseBranchCol> GetSeWiseBranchCollections(ColDbFilter dbFilter)
        {
            var sewBranchCols = new List<SeWiseBranchCol>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, CollectionAction.SeWiseBranchCollection);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return sewBranchCols;

            sewBranchCols = ds.Tables[0].AsEnumerable().Select((dataSew, index) => new SeWiseBranchCol
            {
                SlNo = index + 1,
                BranchCode = dataSew.Field<string>("BranchCode"),
                SalesExecName = dataSew.Field<string>("SalesExecName"),
                TotalCustomers = dataSew.Field<int>("NumberOfCustomers")
            }).ToList();

            return sewBranchCols;
        }

        public List<Customer> GetSeWiseBranchCustomers(ColDbFilter dbFilter, out int totalRows)
        {
            return GetCustomers(dbFilter, CollectionAction.SeWiseBranchCustomers, out totalRows);
        }

        #endregion

        #region CS wise collection

        public List<ColDetail> GetCsWiseCollections(ColDbFilter dbFilter)
        {
            return GetCollections(dbFilter, CollectionAction.CsWiseCollection, "CS");
        }

        public List<Customer> GetCsWiseCustomers(ColDbFilter dbFilter, out int totalRows)
        {
            return GetCustomers(dbFilter, CollectionAction.CsWiseCustomersByType, out totalRows);
        }

        #endregion

        public List<ColCustomerDetail> GetCustomerDetails(ColDbFilter dbFilter, out int totalRows)
        {
            totalRows = 0;

            var customerDetails = new List<ColCustomerDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, CollectionAction.CustomerDetail);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return customerDetails;

            var pageIndex = dbFilter.Skip + 1;
            customerDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new ColCustomerDetail
            {
                SlNo = index + pageIndex,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                PaymentDate = dataRow.Field<DateTime>("PaymentDate").ToString("d-MMM-yyyy"),
                Particulars = dataRow.Field<string>("Particulars"),
                PaymentAmount = dataRow.Field<decimal>("PaymentAmount"),
                VoucherType = dataRow.Field<string>("VchType"),
                TotalRows = dataRow.Field<int>("TotalRows")
            }).ToList();

            totalRows = customerDetails.First().TotalRows;

            return customerDetails;
        }

        #region Helper Methods

        private static ColInfoResponse GetColInfoResponse(ColDbFilter dbFilter, CollectionAction action, decimal sale, string footer, string category)
        {
            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            var sqlParams = GetSqlParams(dbFilter, action);
            var objCreditSum = SqlHelper.ExecuteScalar(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            var totalCollection = Convert.ToDecimal(objCreditSum);

            var collectionPercentage = sale > 0 ? totalCollection * 100 / sale : 0;
            var totalCollectionTxt = totalCollection <= 0 ? "0" : $"{Math.Round(totalCollection):#,###}";

            return new ColInfoResponse
            {
                DateInfo = dateInfo,
                TotalCollection = totalCollectionTxt,
                CollectionPercentage = Math.Round(collectionPercentage, 2),
                Footer = footer,
                Category = category
            };
        }

        private static SqlParameter[] GetSqlParams(ColDbFilter dbFilter, CollectionAction action)
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
                new SqlParameter("@CustomerCode",dbFilter.CustomerCode),
                new SqlParameter("@ConstantsClosingBalance",Constants.ClosingBalance),
                new SqlParameter("@ConstantsOpeningBalance",Constants.OpeningBalance),
                new SqlParameter("@SortBy",dbFilter.SortBy),
                new SqlParameter("@SortOrder",dbFilter.SortOrder)
            };

            return sqlParams;
        }

        private static List<ColDetail> GetCollections(ColDbFilter dbFilter, CollectionAction action, string subGroup)
        {
            var colDetails = new List<ColDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return colDetails;

            colDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new ColDetail
            {
                SlNo = index + 1,
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCustomers = dataRow.Field<int>("NoOfCustomers"),
                BranchCode = dbFilter.BranchCode,
                SubGroup = subGroup
            }).ToList();

            return colDetails;
        }

        private static List<Customer> GetCustomers(ColDbFilter dbFilter, CollectionAction action, out int totalRows)
        {
            totalRows = 0;
            var customers = new List<Customer>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return customers;

            var pageIndex = dbFilter.Skip + 1;
            customers = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new Customer
            {
                SlNo = index + pageIndex,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                TotalRows = dataRow.Field<int>("TotalRows")
            }).ToList();

            totalRows = customers.First().TotalRows;
            return customers;
        }

        #endregion
    }
}