using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;

namespace Garaaz.Models.DashboardOverview.Cbo
{
    public class CboMain
    {
        private const string UspName = "dbo.usp_Dashboard_CboDetail";

        public CboInfoResponse GetCboInfo(CboDbFilter dbFilter)
        {
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            decimal cboSum = GetCboSum(dbFilter, CboAction.CboSum);
            decimal cancelledCboSum = GetCboSum(dbFilter, CboAction.CancelledCboSum);

            // Prepare return response
            var cboTxt = cboSum <= 0 ? "0" : $"{Math.Round(Convert.ToDecimal(cboSum)):#,###}";
            var cancelledCboTxt = cancelledCboSum <= 0 ? "0" : $"{Math.Round(Convert.ToDecimal(cancelledCboSum)):#,###}";

            return new CboInfoResponse
            {
                DateInfo = dateInfo,
                TotalCbo = cboTxt,
                CancelledCbo = $"Cancelled CBO : ₹ {cancelledCboTxt}"
            };
        }

        public List<CboInfoResponse> GetSubGroupWiseCbo(CboDbFilter dbFilter)
        {
            var cboDetails = new List<CboInfoResponse>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            var roCboInfo = GetCboInfoResponse(dbFilter, CboAction.CboByRoWise, dateInfo, "RO Wise", "RO");
            var seCboInfo = GetCboInfoResponse(dbFilter, CboAction.CboBySeWise, dateInfo, "Sales Executive Wise", "SE");

            // Filter by Cancelled
            var cancelledCboSum = GetCboSum(dbFilter, CboAction.CancelledCboSum);
            var cancelledCboTxt = cancelledCboSum <= 0 ? "0" : $"{Math.Round(Convert.ToDecimal(cancelledCboSum)):#,###}";
            var cancelledCboInfo = new CboInfoResponse
            {
                DateInfo = dateInfo,
                TotalCbo = cancelledCboTxt,
                Footer = "Cancelled CBO"
            };

            cboDetails.Add(roCboInfo);
            cboDetails.Add(seCboInfo);
            cboDetails.Add(cancelledCboInfo);

            return cboDetails;
        }

        #region RO wise Cbo detail

        public List<CategoryWiseCboDetail> GetRoWiseCboDetails(CboDbFilter dbFilter)
        {
            return GetCboDetails(dbFilter, CboAction.RoWiseCboDetail);
        }

        public List<CategoryWiseBranchCboDetail> GetRoWiseBranchCboDetails(CboDbFilter dbFilter)
        {
            return GetBranchCboDetails(dbFilter, CboAction.RoWiseBranchCboDetail);
        }

        public List<CategoryWiseCboDetail> GetRoWiseBranchCustomerCboDetail(CboDbFilter dbFilter)
        {
            return GetBranchCustomerCboDetails(dbFilter, CboAction.RoWiseBranchCustomerCboDetail);
        }

        public List<CategoryWiseCustomerCboDetail> GetRoWiseCustomerCboDetail(CboDbFilter dbFilter, out int totalRows)
        {
            return GetCustomerCboDetails(dbFilter, CboAction.RoWiseCustomerCboDetail, out totalRows);
        }

        public List<CboCustomerDetail> GetRoWiseCustomerDetail(CboDbFilter dbFilter, out int totalRows)
        {
            return GetCustomerDetails(dbFilter, CboAction.RoWiseCustomerDetailByCustomerCode, out totalRows);
        }

        public List<CustomerPartDetail> GetRoWiseCustomerPartsDetails(CboDbFilter dbFilter)
        {
            return GetCustomerPartsDetails(dbFilter, CboAction.RoWiseCustomerPartsDetail);
        }

        #endregion

        #region SE wise Cbo detail

        public List<CategoryWiseCboDetail> GetSeWiseCboDetails(CboDbFilter dbFilter)
        {
            return GetCboDetails(dbFilter, CboAction.SeWiseCboDetail);
        }

        public List<CategoryWiseBranchCboDetail> GetSeWiseBranchCboDetails(CboDbFilter dbFilter)
        {
            return GetBranchCboDetails(dbFilter, CboAction.SeWiseBranchCboDetail);
        }

        public List<CategoryWiseCboDetail> GetSeWiseBranchCustomerCboDetail(CboDbFilter dbFilter)
        {
            return GetBranchCustomerCboDetails(dbFilter, CboAction.SeWiseBranchCustomerCboDetail);
        }

        public List<CategoryWiseCustomerCboDetail> GetSeWiseCustomerCboDetail(CboDbFilter dbFilter, out int totalRows)
        {
            return GetCustomerCboDetails(dbFilter, CboAction.SeWiseCustomerCboDetail, out totalRows);
        }

        public List<CboCustomerDetail> GetSeWiseCustomerDetail(CboDbFilter dbFilter, out int totalRows)
        {
            return GetCustomerDetails(dbFilter, CboAction.SeWiseCustomerDetailByCustomerCode, out totalRows);
        }

        public List<CustomerPartDetail> GetSeWiseCustomerPartsDetails(CboDbFilter dbFilter)
        {
            return GetCustomerPartsDetails(dbFilter, CboAction.SeWiseCustomerPartsDetail);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get sum of price of parts in customer back order as per current user's role.
        /// </summary>
        public static int GetCboSum(CboDbFilter dbFilter, CboAction action)
        {
            var sqlParams = GetSqlParams(dbFilter, action);

            var objCboSum = SqlHelper.ExecuteScalar(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);

            return objCboSum is DBNull ? 0 : Convert.ToInt32(objCboSum);
        }

        private static CboInfoResponse GetCboInfoResponse(CboDbFilter dbFilter, CboAction action, string dateInfo, string footer, string category)
        {
            var sqlParams = GetSqlParams(dbFilter, action);

            decimal? cboSum = 0, cancelledCboSum = 0;
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var cbosWithPrice = ds.Tables[0].AsEnumerable().Select(dataRow => new
                {
                    PartNum = dataRow.Field<string>("PartNum"),
                    PartStatus = dataRow.Field<string>("PartStatus"),
                    Price = dataRow.Field<decimal?>("Price")
                }).ToList();
                cboSum = cbosWithPrice.Where(c => c.Price != null && c.PartStatus != "Cancel").Sum(c => c.Price);
                cancelledCboSum = cbosWithPrice.Where(c => c.Price != null && c.PartStatus == "Cancel").Sum(c => c.Price);
            }

            var cboTxt = cboSum <= 0 ? "0" : $"{Math.Round(Convert.ToDecimal(cboSum)):#,###}";
            var cancelledCboTxt = cancelledCboSum <= 0 ? "0" : $"{Math.Round(Convert.ToDecimal(cancelledCboSum)):#,###}";

            return new CboInfoResponse
            {
                DateInfo = dateInfo,
                TotalCbo = cboTxt,
                CancelledCbo = $"Cancelled CBO : ₹ {cancelledCboTxt}",
                Footer = footer,
                Category = category
            };
        }

        private static SqlParameter[] GetSqlParams(CboDbFilter dbFilter, CboAction action)
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

            // Set sortBy value with table column name
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
                new SqlParameter("@CustomerCode",dbFilter.CustomerCode),
                new SqlParameter("CoNumber",dbFilter.CoNumber),
                new SqlParameter("@FirstDayOfMonth", firstDayOfMonth),
                new SqlParameter("@LastDayOfPrevMonth", prevMonthLastDay),
                new SqlParameter("@SortBy",sortBy),
                new SqlParameter("@SortOrder",dbFilter.SortOrder)
            };

            return sqlParams;
        }

        private static List<CategoryWiseCboDetail> GetCboDetails(CboDbFilter dbFilter, CboAction cboAction)
        {
            var cboDetails = new List<CategoryWiseCboDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, cboAction);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return cboDetails;

            var cbos = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCboCustomers = dataRow.Field<int>("NumberOfCboCustomers"),
                NumberOfCboOrders = dataRow.Field<int>("NumberOfCboOrders"),
                CboPrice = dataRow.Field<decimal>("CboPrice"),
                CboPrice0To7Days = dataRow.Field<decimal>("CboPrice0To7Days"),
                CboPrice7To15Days = dataRow.Field<decimal>("CboPrice7To15Days"),
                CboPriceMoreThan15Days = dataRow.Field<decimal>("CboPriceMoreThan15Days"),
                AvgSale = dataRow.Field<decimal>("AvgSale")
            }).ToList();

            var slNo = 1;
            foreach (var cbo in cbos)
            {
                cboDetails.Add(new CategoryWiseCboDetail
                {
                    SlNo = slNo,
                    CustomerType = cbo.CustomerType,
                    NumberOfCboCustomers = cbo.NumberOfCboCustomers,
                    NumberOfCboOrders = cbo.NumberOfCboOrders,
                    CboPrice = Math.Round(cbo.CboPrice, 2),
                    CboPrice0To7Days = Math.Round(cbo.CboPrice0To7Days, 2),
                    CboPrice7To15Days = Math.Round(cbo.CboPrice7To15Days, 2),
                    CboPriceMoreThan15Days = Math.Round(cbo.CboPriceMoreThan15Days, 2),
                    AverageSale = Math.Round(cbo.AvgSale, 2),
                    CboPercentage = cbo.AvgSale > 0 ? Math.Round(cbo.CboPrice / cbo.AvgSale, 2) : 0
                });

                slNo++;
            }

            return cboDetails;
        }

        private static List<CategoryWiseBranchCboDetail> GetBranchCboDetails(CboDbFilter dbFilter, CboAction cboAction)
        {
            var cboDetails = new List<CategoryWiseBranchCboDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, cboAction);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return cboDetails;

            var cbosWithBranches = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchName = dataRow.Field<string>("BranchName"),
                NumberOfCboCustomers = dataRow.Field<int>("NumberOfCboCustomers"),
                NumberOfCboOrders = dataRow.Field<int>("NumberOfCboOrders"),
                CboPrice = dataRow.Field<decimal>("CboPrice"),
                CboPrice0To7Days = dataRow.Field<decimal>("CboPrice0To7Days"),
                CboPrice7To15Days = dataRow.Field<decimal>("CboPrice7To15Days"),
                CboPriceMoreThan15Days = dataRow.Field<decimal>("CboPriceMoreThan15Days"),
                AvgSale = dataRow.Field<decimal>("AvgSale")
            }).ToList();

            if (cbosWithBranches.Count == 0) return cboDetails;

            var slNo = 1;
            foreach (var cbo in cbosWithBranches)
            {
                cboDetails.Add(new CategoryWiseBranchCboDetail
                {
                    SlNo = slNo,
                    BranchCode = cbo.BranchCode,
                    BranchName = cbo.BranchName,
                    NumberOfCboCustomers = cbo.NumberOfCboCustomers,
                    NumberOfCboOrders = cbo.NumberOfCboOrders,
                    CboPrice = Math.Round(cbo.CboPrice, 2),
                    CboPrice0To7Days = Math.Round(cbo.CboPrice0To7Days, 2),
                    CboPrice7To15Days = Math.Round(cbo.CboPrice7To15Days, 2),
                    CboPriceMoreThan15Days = Math.Round(cbo.CboPriceMoreThan15Days, 2),
                    AvgSale = Math.Round(cbo.AvgSale, 2),
                    CboPercentage = cbo.AvgSale > 0 ? Math.Round(cbo.CboPrice / cbo.AvgSale, 2) : 0
                });

                slNo++;
            }

            return cboDetails;
        }

        private static List<CategoryWiseCboDetail> GetBranchCustomerCboDetails(CboDbFilter dbFilter, CboAction cboAction)
        {
            var bcCboDetails = new List<CategoryWiseCboDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, cboAction);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return bcCboDetails;

            var cbos = ds.Tables[0].AsEnumerable().Select(dataRow => new
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCboCustomers = dataRow.Field<int>("NumberOfCboCustomers"),
                NumberOfCboOrders = dataRow.Field<int>("NumberOfCboOrders"),
                CboPrice = dataRow.Field<decimal>("CboPrice"),
                CboPrice0To7Days = dataRow.Field<decimal>("CboPrice0To7Days"),
                CboPrice7To15Days = dataRow.Field<decimal>("CboPrice7To15Days"),
                CboPriceMoreThan15Days = dataRow.Field<decimal>("CboPriceMoreThan15Days"),
                AvgSale = dataRow.Field<decimal>("AvgSale")
            }).ToList();

            var slNo = 1;
            foreach (var cbo in cbos)
            {
                bcCboDetails.Add(new CategoryWiseCboDetail
                {
                    SlNo = slNo,
                    BranchCode = dbFilter.BranchCode,
                    CustomerType = cbo.CustomerType,
                    NumberOfCboCustomers = cbo.NumberOfCboCustomers,
                    NumberOfCboOrders = cbo.NumberOfCboOrders,
                    CboPrice = Math.Round(cbo.CboPrice, 2),
                    CboPrice0To7Days = Math.Round(cbo.CboPrice0To7Days, 2),
                    CboPrice7To15Days = Math.Round(cbo.CboPrice7To15Days, 2),
                    CboPriceMoreThan15Days = Math.Round(cbo.CboPriceMoreThan15Days, 2),
                    AvgSale = Math.Round(cbo.AvgSale, 2),
                    CboPercentage = cbo.AvgSale > 0 ? Math.Round(cbo.CboPrice / cbo.AvgSale, 2) : 0
                });

                slNo++;
            }

            return bcCboDetails;
        }

        private static List<CategoryWiseCustomerCboDetail> GetCustomerCboDetails(CboDbFilter dbFilter, CboAction cboAction, out int totalRows)
        {
            totalRows = 0;
            var custCboDetails = new List<CategoryWiseCustomerCboDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, cboAction);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return custCboDetails;

            var slNo = dbFilter.Skip + 1;
            custCboDetails = ds.Tables[0].AsEnumerable().Select(dataRow => new CategoryWiseCustomerCboDetail
            {
                SlNo = slNo,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCboCustomers = dataRow.Field<int>("NumberOfCboCustomers"),
                NumberOfCboOrders = dataRow.Field<int>("NumberOfCboOrders"),
                CboPrice = Math.Round(dataRow.Field<decimal>("CboPrice"), 2),
                CboPrice0To7Days = Math.Round(dataRow.Field<decimal>("CboPrice0To7Days"), 2),
                CboPrice7To15Days = Math.Round(dataRow.Field<decimal>("CboPrice7To15Days"), 2),
                CboPriceMoreThan15Days = Math.Round(dataRow.Field<decimal>("CboPriceMoreThan15Days"), 2),
                AvgSale = Math.Round(dataRow.Field<decimal>("AvgSale"), 2),
                CboPercentageTxt = $"{(dataRow.Field<decimal>("AvgSale") > 0 ? Math.Round(dataRow.Field<decimal>("CboPrice") / dataRow.Field<decimal>("AvgSale"), 2) : 0)}%"
            }).ToList();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var rowDs = ds.Tables[0].Rows[0];
                totalRows = rowDs.Field<int>("TotalRows");
            }
            return custCboDetails;
        }

        private static List<CboCustomerDetail> GetCustomerDetails(CboDbFilter dbFilter, CboAction cboAction, out int totalRows)
        {
            totalRows = 0;
            var customerDetails = new List<CboCustomerDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, cboAction);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return customerDetails;

            var pageIndex = dbFilter.Skip + 1;
            customerDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new CboCustomerDetail
            {
                SlNo = index + pageIndex,
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CoNumber = dataRow.Field<string>("CoNumber"),
                NumberOfParts = dataRow.Field<int>("NumberOfParts"),
                CboPrice = dataRow.Field<decimal>("CboPrice"),
                NumberOfDaysSinceOrder = dataRow.Field<int>("NumberOfDaysSinceOrder"),
                TotalRows = dataRow.Field<int>("TotalRows")
            }).ToList();

            totalRows = customerDetails.First().TotalRows;

            return customerDetails;
        }

        public static List<CustomerPartDetail> GetCustomerPartsDetails(CboDbFilter dbFilter, CboAction cboAction)
        {
            var customerPartDetails = new List<CustomerPartDetail>();

            // Set dates as per date filter
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, cboAction);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, UspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return customerPartDetails;

            customerPartDetails = ds.Tables[0].AsEnumerable().Select((dataRow, index) => new CustomerPartDetail
            {
                SlNo = index + 1,
                CoDate = dataRow.Field<DateTime>("CODate"),
                LocCode = dataRow.Field<string>("LocCode"),
                PartNum = dataRow.Field<string>("PartNum"),
                PartDesc = dataRow.Field<string>("PartDesc"),
                Order = dataRow.Field<string>("Order"),
                UnitPrice = dataRow.Field<decimal>("UnitPrice"),
                OrderValue = dataRow.Field<decimal>("OrderValue"),
                Cbo = dataRow.Field<string>("CBO"),
                StkMw = dataRow.Field<string>("StkMW"),
                Eta = dataRow.Field<string>("ETA"),
                Inv = dataRow.Field<string>("Inv"),
                Pick = dataRow.Field<string>("Pick"),
                Alloc = dataRow.Field<string>("Alloc"),
                Bo = dataRow.Field<string>("BO"),
                Ao = dataRow.Field<string>("AO"),
                Action = dataRow.Field<string>("Action"),
                Pd = dataRow.Field<string>("PD"),
            }).ToList();

            return customerPartDetails;
        }

        private static string GetSortColumn(DashboardFilter dbFilter, CboAction action)
        {
            string sortColumn = null;
            if (string.IsNullOrWhiteSpace(dbFilter.SortBy)) return sortColumn;
            // Check Orderby column name exist in the CategoryWiseCustomerCboDetail model if yes then set Orderby column name
            if (action == CboAction.SeWiseCustomerDetailByCustomerCode || action == CboAction.RoWiseCustomerDetailByCustomerCode)
            {
                var isColumnValid = new CboCustomerDetail().GetType().GetProperty(dbFilter.SortBy) != null ? true : false;

                if (isColumnValid)
                {
                    if (dbFilter.SortBy != "SlNo")
                    {
                        sortColumn = dbFilter.SortBy;
                    }
                }
            }
            else if (action == CboAction.RoWiseCustomerCboDetail || action == CboAction.SeWiseCustomerCboDetail)
            {
                var isColumnValid = new CategoryWiseCustomerCboDetail().GetType().GetProperty(dbFilter.SortBy) != null ? true : false;

                if (isColumnValid)
                {
                    if (dbFilter.SortBy != "SlNo")
                    {
                        if (dbFilter.SortBy == "CboPercentageTxt")
                        {
                            sortColumn = "CboPrice";
                        }
                        else { sortColumn = dbFilter.SortBy; }
                    }
                }
            }
            return sortColumn;
        }

        #endregion
    }
}