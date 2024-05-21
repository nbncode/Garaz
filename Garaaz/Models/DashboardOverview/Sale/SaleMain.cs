using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Garaaz.Models.DashboardOverview.Sale
{
    public class SaleMain
    {
        private const string RoUspName = "dbo.usp_Dashboard_RoWiseSaleDetails";
        private const string SeUspName = "dbo.usp_Dashboard_SeWiseSaleDetails";
        private const string CsUspName = "dbo.usp_Dashboard_CsWiseSaleDetails";
        private const string PgUspName = "dbo.usp_Dashboard_PgWiseSaleDetails";

        private const string RoCaller = "RO";
        private const string SeCaller = "SE";
        private const string PgCaller = "PG";

        public SaleInfoResponse GetSalesInfo(DashboardFilter dbFilter)
        {
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            var sale = DbCommon.GetTotalSale(dbFilter);

            // Calculate sale percentage as per last year time period
            var saleGrowthPer = DbCommon.CalculatePercentage(sale.PreviousSale, sale.CurrentSale);

            // Prepare return response            
            var totalSale = sale.CurrentSale <= 0 ? "0" : $"{Math.Round(sale.CurrentSale):#,###}";
            var coDealerOrDistSale = sale.CoDealerOrDistSale <= 0 ? "0" : $"{Math.Round(sale.CoDealerOrDistSale):#,###}";
            return new SaleInfoResponse
            {
                DateInfo = dateInfo,
                TotalSale = totalSale,
                GrowthPercentage = Math.Round(saleGrowthPer, 2),
                CoDealerDistSale = $"Additional Co-Dealer/Dist: {coDealerOrDistSale}"
            };
        }

        public List<SaleInfoResponse> GetCategoryWiseSale(SaleDbFilter dbFilter)
        {
            var saleDetails = new List<SaleInfoResponse>();

            #region old code
            //var date = DbCommon.GetDatePeriod(dbFilter);
            //dbFilter.StartDate = date.StartDate;
            //dbFilter.EndDate = date.EndDate;
            //var dateInfo = date.DateInfo;

            //decimal currentSaleAll = 0, previousSaleAll = 0,
            //    currentSaleMgp = 0, previousSaleMgp = 0,
            //    currentSaleMga = 0, previousSaleMga = 0,
            //    currentSaleMgo = 0, previousSaleMgo = 0;

            //try
            //{
            //    // Set user role
            //    string role = string.Empty;
            //    if (dbFilter.Roles.Contains(Constants.SuperAdmin))
            //    { role = Constants.SuperAdmin; }
            //    if (dbFilter.Roles.Contains(Constants.Distributor))
            //    { role = Constants.Distributor; }
            //    if (dbFilter.Roles.Contains(Constants.RoIncharge))
            //    { role = Constants.RoIncharge; }
            //    if (dbFilter.Roles.Contains(Constants.SalesExecutive))
            //    { role = Constants.SalesExecutive; }

            //    SqlParameter[] parameter = {
            //                            new SqlParameter("@Action",1),
            //                            new SqlParameter("@Role",role),
            //                            new SqlParameter("@UserId",dbFilter.UserId),
            //                            new SqlParameter("@StartDate",dbFilter.StartDate),
            //                            new SqlParameter("@EndDate",dbFilter.EndDate),
            //                            new SqlParameter("@GroupName",dbFilter.Category),
            //                            new SqlParameter("@CoDealerDist",Constants.CoDealerDistributor)
            //                           };
            //    DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_Sale_CategoryWise_Temp", parameter);
            //    var table = ds.Tables[0];
            //    if (table.Rows.Count > 0)
            //    {
            //        var rows = table.Rows[0];

            //        currentSaleAll = rows["CurrentSaleAll"] != DBNull.Value ? Convert.ToDecimal(rows["CurrentSaleAll"]) : 0;
            //        previousSaleAll = rows["PreviousSaleAll"] != DBNull.Value ? Convert.ToDecimal(rows["PreviousSaleAll"]) : 0;
            //        currentSaleMgp = rows["CurrentSaleMgp"] != DBNull.Value ? Convert.ToDecimal(rows["CurrentSaleMgp"]) : 0;
            //        previousSaleMgp = rows["PreviousSaleMgp"] != DBNull.Value ? Convert.ToDecimal(rows["PreviousSaleMgp"]) : 0;
            //        currentSaleMga = rows["CurrentSaleMga"] != DBNull.Value ? Convert.ToDecimal(rows["CurrentSaleMga"]) : 0;
            //        previousSaleMga = rows["PreviousSaleMga"] != DBNull.Value ? Convert.ToDecimal(rows["PreviousSaleMga"]) : 0;
            //        currentSaleMgo = rows["CurrentSaleMgo"] != DBNull.Value ? Convert.ToDecimal(rows["CurrentSaleMgo"]) : 0;
            //        previousSaleMgo = rows["PreviousSaleMgo"] != DBNull.Value ? Convert.ToDecimal(rows["PreviousSaleMgo"]) : 0;
            //    }
            //}
            //catch (Exception exc)
            //{
            //    RepoUserLogs.LogException(exc);
            //}

            //// Calculate sale percentage as per last year time period
            //var saleGrowthAll = DbCommon.CalculatePercentage(previousSaleAll, currentSaleAll);
            //var saleGrowthMgp = DbCommon.CalculatePercentage(previousSaleMgp, currentSaleMgp);
            //var saleGrowthMga = DbCommon.CalculatePercentage(previousSaleMga, currentSaleMga);
            //var saleGrowthMgo = DbCommon.CalculatePercentage(previousSaleMgo, currentSaleMgo);

            //// Prepare return response   
            //saleDetails.Add(new SaleInfoResponse
            //{
            //    DateInfo = dateInfo,
            //    TotalSale = currentSaleAll <= 0 ? "0" : $"{Math.Round(currentSaleAll):#,###}",
            //    GrowthPercentage = Math.Round(saleGrowthAll, 2),
            //    Footer = "All Parts Sales",
            //    Category = "All"
            //});

            //saleDetails.Add(new SaleInfoResponse
            //{
            //    DateInfo = dateInfo,
            //    TotalSale = currentSaleMgp <= 0 ? "0" : $"{Math.Round(currentSaleMgp):#,###}",
            //    GrowthPercentage = Math.Round(saleGrowthMgp, 2),
            //    Footer = "MGP Sales",
            //    Category = "M"
            //});

            //saleDetails.Add(new SaleInfoResponse
            //{
            //    DateInfo = dateInfo,
            //    TotalSale = currentSaleMga <= 0 ? "0" : $"{Math.Round(currentSaleMga):#,###}",
            //    GrowthPercentage = Math.Round(saleGrowthMga, 2),
            //    Footer = "MGA Sales",
            //    Category = "AA"
            //});

            //saleDetails.Add(new SaleInfoResponse
            //{
            //    DateInfo = dateInfo,
            //    TotalSale = currentSaleMgo <= 0 ? "0" : $"{Math.Round(currentSaleMgo):#,###}",
            //    GrowthPercentage = Math.Round(saleGrowthMgo, 2),
            //    Footer = "MGO Sales",
            //    Category = "AG"
            //});
            #endregion

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            try
            {
                // Set user role
                string role = string.Empty;
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
                                        new SqlParameter("@GroupName",dbFilter.Category),
                                        new SqlParameter("@CoDealerDist",Constants.CoDealerDistributor)
                                       };
                DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_Sale_CategoryWise", parameter);

                if (ds.Tables.Count > 0)
                {

                    var currentYearSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SaleCategory
                    {
                        Category = dataRow.Field<string>("PartCategory"),
                        Sale = dataRow.Field<decimal>("NetRetailSelling")
                    }).ToList();

                    var previousYearSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SaleCategory
                    {
                        Category = dataRow.Field<string>("PartCategory"),
                        Sale = dataRow.Field<decimal>("NetRetailSelling")
                    }).ToList();

                    var previousSaleAll = previousYearSales.Sum(s => s.Sale);
                    var currentSaleAll = currentYearSales.Sum(s => s.Sale);
                    // Calculate sale percentage as per last year time period
                    var saleGrowthAll = DbCommon.CalculatePercentage(previousSaleAll, currentSaleAll);

                    // Prepare return response   
                    saleDetails.Add(new SaleInfoResponse
                    {
                        DateInfo = dateInfo,
                        TotalSale = currentSaleAll <= 0 ? "0" : $"{Math.Round(currentSaleAll):#,###}",
                        GrowthPercentage = Math.Round(saleGrowthAll, 2),
                        Footer = "All Parts Sales",
                        Category = "All"
                    });

                    if (currentYearSales.Count > 0 || previousYearSales.Count() > 0)
                    {
                        if (currentYearSales.Count > 0)
                        {
                            foreach (var currentYearSale in currentYearSales)
                            {
                                var previousYearSale = previousYearSales.FirstOrDefault(p => p.Category == currentYearSale.Category);

                                var saleGrowth = DbCommon.CalculatePercentage(previousYearSale != null ? previousYearSale.Sale : 0, currentYearSale.Sale);

                                var part = ProductPartCategory.PartCategoriesFullForm.FirstOrDefault(x => x.Value == currentYearSale.Category)?.Text;

                                var saleResponse = new SaleInfoResponse
                                {
                                    DateInfo = dateInfo,
                                    TotalSale = currentYearSale.Sale <= 0 ? "0" : $"{Math.Round(currentYearSale.Sale):#,###}",
                                    GrowthPercentage = Math.Round(saleGrowth, 2),
                                    Footer = (part != null ? part : currentYearSale.Category) + " Sales",
                                    Category = currentYearSale.Category
                                };
                                saleDetails.Add(saleResponse);
                            }
                        }
                        else if (previousYearSales.Count() > 0)
                        {
                            foreach (var previousYearSale in previousYearSales)
                            {
                                var saleGrowth = DbCommon.CalculatePercentage(previousYearSale.Sale, 0);

                                var part = ProductPartCategory.PartCategoriesFullForm.FirstOrDefault(x => x.Value == previousYearSale.Category)?.Text;

                                var saleResponse = new SaleInfoResponse
                                {
                                    DateInfo = dateInfo,
                                    TotalSale = "0",
                                    GrowthPercentage = Math.Round(saleGrowth, 2),
                                    Footer = (part != null ? part : previousYearSale.Category) + " Sales",
                                    Category = previousYearSale.Category
                                };
                                saleDetails.Add(saleResponse);
                            }
                        }
                    }

                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }
            saleDetails = saleDetails.OrderBy(s => s.Footer).ToList();
            return saleDetails;
        }

        public List<SaleInfoResponse> GetSubGroupWiseSale(SaleDbFilter dbFilter)
        {
            var saleDetails = new List<SaleInfoResponse>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            decimal roWiseSale = 0, seWiseSale = 0, customerSegmentWiseSale = 0, partGroupWiseSale = 0;
            try
            {
                // Set user role
                string role = string.Empty;
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
                                        new SqlParameter("@EndDate",dbFilter.EndDate),
                                        new SqlParameter("@GroupName",dbFilter.Category),
                                        new SqlParameter("@CoDealerDist",Constants.CoDealerDistributor)
                                       };
                DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_Sale_CategoryWise", parameter);
                var table = ds.Tables[0];
                if (table.Rows.Count > 0)
                {
                    var rows = table.Rows[0];

                    roWiseSale = rows["RoWiseCurrentSale"] != DBNull.Value ? Convert.ToDecimal(rows["RoWiseCurrentSale"]) : 0;
                    seWiseSale = rows["SalesExeWiseCurrentSale"] != DBNull.Value ? Convert.ToDecimal(rows["SalesExeWiseCurrentSale"]) : 0;
                    customerSegmentWiseSale = rows["CuSagWiseCurrentSale"] != DBNull.Value ? Convert.ToDecimal(rows["CuSagWiseCurrentSale"]) : 0;
                    partGroupWiseSale = rows["PartGroupWisePreviousSale"] != DBNull.Value ? Convert.ToDecimal(rows["PartGroupWisePreviousSale"]) : 0;
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }


            #region Comment old code
            //var sales = Session.Sales ?? GetSalesByRole(dbFilter);
            //if (sales.Count > 0)
            //{
            //    Session.Sales = sales;

            //    var customerTypes = DbCommon.CustomerTypes;

            //    var tuple = dbFilter.Category.Equals("All", StringComparison.OrdinalIgnoreCase) ? FilterSalesByCategory(dbFilter, sales, null, true, true) : FilterSalesByCategory(dbFilter, sales, dbFilter.Category, includeDistAndCoDealer: true);

            //    var roWiseTuple = GetRoWiseSales(dbFilter, tuple.Item1.ToList(), tuple.Item2.ToList());
            //    roWiseSale = roWiseTuple.Item1.Where(s => customerTypes.Contains(s.ConsPartyTypeDesc)).Sum(s => Convert.ToDecimal(s.NetRetailSelling));
            //    Session.RoWiseCurrentSales = roWiseTuple.Item1.Where(s => customerTypes.Contains(s.ConsPartyTypeDesc));
            //    Session.RoWisePrevSales = roWiseTuple.Item2.Where(s => customerTypes.Contains(s.ConsPartyTypeDesc));

            //    var seWiseTuple = GetSalesExecWiseSale(dbFilter, tuple.Item1.ToList(), tuple.Item2.ToList());
            //    seWiseSale = seWiseTuple.Item1.Where(s => customerTypes.Contains(s.ConsPartyTypeDesc)).Sum(s => Convert.ToDecimal(s.NetRetailSelling));
            //    Session.SeWiseCurrentSales = seWiseTuple.Item1.Where(s => customerTypes.Contains(s.ConsPartyTypeDesc));
            //    Session.SeWisePrevSales = seWiseTuple.Item2.Where(s => customerTypes.Contains(s.ConsPartyTypeDesc));

            //    customerSegmentWiseSale = tuple.Item1.Sum(s => Convert.ToDecimal(s.NetRetailSelling));
            //    Session.CsWiseCurrentSales = tuple.Item1;
            //    Session.CsWisePrevSales = tuple.Item2;

            //    partGroupWiseSale = tuple.Item1.Where(s => !string.IsNullOrEmpty(s.PartGroup) && customerTypes.Contains(s.ConsPartyTypeDesc)).Sum(s => Convert.ToDecimal(s.NetRetailSelling));
            //    Session.PgWiseCurrentSales = tuple.Item1.Where(s => !string.IsNullOrEmpty(s.PartGroup) && customerTypes.Contains(s.ConsPartyTypeDesc));
            //    Session.PgWisePrevSales = tuple.Item2.Where(s => !string.IsNullOrEmpty(s.PartGroup) && customerTypes.Contains(s.ConsPartyTypeDesc));
            //}
            #endregion

            saleDetails.Add(new SaleInfoResponse
            {
                DateInfo = dateInfo,
                TotalSale = roWiseSale <= 0 ? "0" : $"{Math.Round(roWiseSale):#,###}",
                Footer = "RO wise Sales",
                Category = "RO"
            });
            saleDetails.Add(new SaleInfoResponse
            {
                DateInfo = dateInfo,
                TotalSale = seWiseSale <= 0 ? "0" : $"{Math.Round(seWiseSale):#,###}",
                Footer = "Sales Executives wise Sales",
                Category = "SE"
            });
            saleDetails.Add(new SaleInfoResponse
            {
                DateInfo = dateInfo,
                TotalSale = customerSegmentWiseSale <= 0 ? "0" : $"{Math.Round(customerSegmentWiseSale):#,###}",
                Footer = "Customer Segment wise Sales",
                Category = "CS"
            });
            saleDetails.Add(new SaleInfoResponse
            {
                DateInfo = dateInfo,
                TotalSale = partGroupWiseSale <= 0 ? "0" : $"{Math.Round(partGroupWiseSale):#,###}",
                Footer = "Part Group wise Sales",
                Category = "PG"
            });

            return saleDetails;
        }

        #region RO wise sale detail

        public List<RoWiseSaleDetail> GetRoWiseSaleDetails(SaleDbFilter dbFilter)
        {
            var roWiseSaleDetails = new List<RoWiseSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            #region Comment old code
            //if (Session.RoWiseCurrentSales == null)
            //{
            //    GetSubGroupWiseSale(dbFilter);
            //}
            //if (Session.RoWiseCurrentSales == null || Session.RoWisePrevSales == null) return roWiseSaleDetails;

            //var groups = dbFilter.BranchCode != null ? Session.RoWiseCurrentSales.Where(s => s.LocCode.Equals(dbFilter.BranchCode, StringComparison.OrdinalIgnoreCase)).GroupBy(s => s.ConsPartyTypeDesc) : Session.RoWiseCurrentSales.GroupBy(s => s.ConsPartyTypeDesc);
            //var prvGroups = dbFilter.BranchCode != null ? Session.RoWisePrevSales.Where(s => s.LocCode.Equals(dbFilter.BranchCode, StringComparison.OrdinalIgnoreCase)).GroupBy(s => s.ConsPartyTypeDesc).ToList() : Session.RoWisePrevSales.GroupBy(s => s.ConsPartyTypeDesc).ToList();
            //var slNo = 1;
            //foreach (var group in groups)
            //{
            //    var threeMonthSales = group.GroupBy(s => s.CreatedDate?.Month).Take(3).SelectMany(g => g).Sum(s => Convert.ToDecimal(s.NetRetailSelling));

            //    var prvGroup = prvGroups.FirstOrDefault(p => p.Key == group.Key);

            //    roWiseSaleDetails.Add(new RoWiseSaleDetail
            //    {
            //        SlNo = slNo,
            //        CustomerType = group.Key,
            //        NumberOfCustomers = group.Count(),
            //        AverageSale = Math.Round(threeMonthSales / 3, 2),
            //        PrevAchieved = prvGroup?.Sum(s => Convert.ToDecimal(s.NetRetailSelling))
            //    });

            //    slNo++;
            //}
            #endregion

            var sqlParams = GetSqlParams(dbFilter, SaleAction.RoWiseSaleDetail, RoCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, RoUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return roWiseSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCustomers = dataRow.Field<int>("TotalCustomer"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling"),
                AvgSale = dataRow.Field<decimal>("AverageSale")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var slNo = 1;
            foreach (var cs in currentSales)
            {
                var prvSale = previousSales.FirstOrDefault(p => p.CustomerType == cs.CustomerType);

                var achievedPer = CalculateAchievedPer(cs.NetRetailSelling, prvSale?.NetRetailSelling ?? 0);

                roWiseSaleDetails.Add(new RoWiseSaleDetail
                {
                    SlNo = slNo,
                    BranchCode = cs.BranchCode,
                    CustomerType = cs.CustomerType,
                    NumberOfCustomers = cs.NumberOfCustomers,
                    AverageSale = Math.Round(cs.AvgSale, 2),
                    PrevAchieved = prvSale?.NetRetailSelling,
                    Achieved = Math.Round(cs.NetRetailSelling, 2),
                    AchievedPercentage = achievedPer
                });

                slNo++;
            }

            return roWiseSaleDetails;
        }

        public List<RoWiseBranchSaleDetail> GetRoWiseBranchSaleDetails(SaleDbFilter dbFilter)
        {
            var roWiseBranchSaleDetails = new List<RoWiseBranchSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            #region Comment old code
            //if (Session.RoWiseCurrentSales == null)
            //{
            //    GetSubGroupWiseSale(dbFilter);
            //}
            //if (Session.RoWiseCurrentSales == null || Session.RoWisePrevSales == null) return roWiseBranchSaleDetails;

            //var currentSalesWithBranches = from s in Session.RoWiseCurrentSales
            //                               join o in _db.Outlets on s.LocCode equals o.OutletName
            //                               where o.OutletName != null
            //                               select new
            //                               {
            //                                   NetRetailSelling = Convert.ToDecimal(s.NetRetailSelling),
            //                                   BranchCode = o.OutletName,
            //                                   BranchName = o.Address,
            //                                   s.CreatedDate
            //                               };
            //var prevSalesWithBranches = from s in Session.RoWisePrevSales
            //                            join o in _db.Outlets on s.LocCode equals o.OutletName
            //                            where o.OutletName != null
            //                            select new
            //                            {
            //                                NetRetailSelling = Convert.ToDecimal(s.NetRetailSelling),
            //                                BranchCode = o.OutletName,
            //                                BranchName = o.Address,
            //                                s.CreatedDate
            //                            };

            //var groups = currentSalesWithBranches.GroupBy(s => s.BranchCode);
            //var prvGroups = prevSalesWithBranches.GroupBy(s => s.BranchCode).ToList();

            //var slNo = 1;
            //foreach (var group in groups)
            //{
            //    var threeMonthSales = group.GroupBy(s => s.CreatedDate?.Month).Take(3).SelectMany(g => g).Sum(s => s.NetRetailSelling);

            //    var prvGroup = prvGroups.FirstOrDefault(p => p.Key == group.Key);

            //    roWiseBranchSaleDetails.Add(new RoWiseBranchSaleDetail
            //    {
            //        SlNo = slNo,
            //        BranchCode = group.Key,
            //        BranchName = group.Select(g => g.BranchName).FirstOrDefault(),
            //        TotalCustomers = group.Count(),
            //        AverageSale = Math.Round(threeMonthSales / 3, 2),
            //        PrevAchieved = prvGroup?.Sum(s => s.NetRetailSelling)
            //    });

            //    slNo++;
            //}

            #endregion

            var sqlParams = GetSqlParams(dbFilter, SaleAction.RoWiseBranchSaleDetail, RoCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, RoUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return roWiseBranchSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchName = dataRow.Field<string>("BranchName"),
                NumberOfCustomers = dataRow.Field<int>("TotalCustomer"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling"),
                AvgSale = dataRow.Field<decimal>("AverageSale")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var slNo = 1;
            foreach (var group in currentSales)
            {
                var prvGroup = previousSales.FirstOrDefault(p => p.BranchCode == group.BranchCode);

                var achievedPer = CalculateAchievedPer(group.NetRetailSelling, prvGroup?.NetRetailSelling ?? 0);

                roWiseBranchSaleDetails.Add(new RoWiseBranchSaleDetail
                {
                    SlNo = slNo,
                    BranchCode = group.BranchCode,
                    BranchName = group.BranchName,
                    TotalCustomers = group.NumberOfCustomers,
                    AverageSale = Math.Round(group.AvgSale, 2),
                    PrevAchieved = prvGroup?.NetRetailSelling,
                    Achieved = Math.Round(group.NetRetailSelling, 2),
                    AchievedPercentage = achievedPer
                });

                slNo++;
            }

            return roWiseBranchSaleDetails;
        }

        public List<RoWiseCustomerSaleDetail> GetRoWiseCustomerSaleDetails(SaleDbFilter dbFilter, out int totalRows)
        {
            totalRows = 0;
            var roWiseCustomerSaleDetails = new List<RoWiseCustomerSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            #region Comment old code
            //if (Session.RoWiseCurrentSales == null)
            //{
            //    GetSubGroupWiseSale(dbFilter);
            //}
            //if (Session.RoWiseCurrentSales == null || Session.RoWisePrevSales == null) return roWiseCustomerSaleDetails;

            //var currentSalesWithCustomers = Session.RoWiseCurrentSales.Select(s => new
            //{
            //    CustomerCode = s.ConsPartyCode,
            //    CustomerName = s.ConsPartyName,
            //    CustomerType = s.ConsPartyTypeDesc
            //});
            //var prevSalesWithCustomers = Session.RoWisePrevSales.Select(s => new
            //{
            //    NetRetailSelling = Convert.ToDecimal(s.NetRetailSelling),
            //    CustomerCode = s.ConsPartyCode
            //});

            //var groups = currentSalesWithCustomers.GroupBy(s => s.CustomerCode);
            //var prvGroups = prevSalesWithCustomers.GroupBy(s => s.CustomerCode).ToList();

            //var slNo = 1;
            //foreach (var group in groups)
            //{
            //    var prvGroup = prvGroups.FirstOrDefault(p => p.Key == group.Key);

            //    roWiseCustomerSaleDetails.Add(new RoWiseCustomerSaleDetail
            //    {
            //        SlNo = slNo,
            //        CustomerCode = group.Key,
            //        CustomerName = group.Select(g => g.CustomerName).FirstOrDefault(),
            //        CustomerType = group.Select(g => g.CustomerType).FirstOrDefault(),
            //        PrevAchieved = prvGroup?.Sum(s => s.NetRetailSelling)
            //    });

            //    slNo++;
            //}

            #endregion

            var action = string.IsNullOrWhiteSpace(dbFilter.SearchTxt) ? SaleAction.RoWiseCustomerSaleDetail : SaleAction.RoWiseCustomerSaleDetailWithSearch;
            var sqlParams = GetSqlParams(dbFilter, action, RoCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, RoUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return roWiseCustomerSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                TotalRows = dataRow.Field<int>("TotalRows"),
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            totalRows = currentSales.First().TotalRows;

            var slNo = dbFilter.Skip + 1;
            foreach (var group in currentSales)
            {
                var prvGroup = previousSales.FirstOrDefault(p => p.CustomerCode == group.CustomerCode);

                var achievedPer = CalculateAchievedPer(group.NetRetailSelling, prvGroup?.NetRetailSelling ?? 0);

                roWiseCustomerSaleDetails.Add(new RoWiseCustomerSaleDetail
                {
                    SlNo = slNo,
                    CustomerCode = group.CustomerCode,
                    CustomerName = group.CustomerName,
                    CustomerType = group.CustomerType,
                    PrevAchieved = prvGroup?.NetRetailSelling,
                    Achieved = Math.Round(group.NetRetailSelling, 2),
                    AchievedPercentage = achievedPer
                });

                slNo++;
            }

            return roWiseCustomerSaleDetails;
        }

        #endregion

        #region SE wise sale detail

        public List<SeWiseSaleDetail> GetSeWiseSaleDetails(SaleDbFilter dbFilter)
        {
            var seWiseSaleDetails = new List<SeWiseSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, SaleAction.SeWiseSaleDetail, SeCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, SeUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return seWiseSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SeWiseSaleDetailResponse
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCustomers = dataRow.Field<int>("TotalCustomer"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling"),
                AvgSale = dataRow.Field<decimal>("AverageSale")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SeWiseSaleDetailResponse
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var slNo = 1;
            foreach (var cs in currentSales)
            {
                var prvSale = previousSales.FirstOrDefault(p => p.CustomerType == cs.CustomerType);

                var achievedPer = CalculateAchievedPer(cs.NetRetailSelling, prvSale?.NetRetailSelling ?? 0);

                seWiseSaleDetails.Add(new SeWiseSaleDetail
                {
                    SlNo = slNo,
                    BranchCode = cs.BranchCode,
                    CustomerType = cs.CustomerType,
                    NumberOfCustomers = cs.NumberOfCustomers,
                    AverageSale = Math.Round(cs.AvgSale, 2),
                    PrevAchieved = prvSale?.NetRetailSelling,
                    Achieved = Math.Round(cs.NetRetailSelling, 2),
                    AchievedPercentage = achievedPer
                });

                slNo++;
            }

            return seWiseSaleDetails;
        }

        public List<SeWiseBranchSaleDetail> GetSeWiseBranchSaleDetails(SaleDbFilter dbFilter)
        {
            var seWiseBranchSaleDetails = new List<SeWiseBranchSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            #region Comment old code
            //if (Session.SeWiseCurrentSales == null)
            //{
            //    GetSubGroupWiseSale(dbFilter);
            //}
            //if (Session.SeWiseCurrentSales == null || Session.SeWisePrevSales == null) return seWiseSaleDetails;

            //var seCurrentSales = from s in Session.SeWiseCurrentSales
            //                     join u in _db.UserDetails on s.UserId equals u.UserId
            //                     where s.UserId != null
            //                     select new
            //                     {
            //                         BranchCode = s.LocCode,
            //                         SalesExecName = $"{u.FirstName} {u.LastName}"
            //                     };

            //var sePrvSales = from s in Session.SeWisePrevSales
            //                 join u in _db.UserDetails on s.UserId equals u.UserId
            //                 where s.UserId != null
            //                 select new
            //                 {
            //                     BranchCode = s.LocCode,
            //                     SalesExecName = $"{u.FirstName} {u.LastName}",
            //                     NetRetailSelling = Convert.ToDecimal(s.NetRetailSelling)
            //                 };

            //var groups = seCurrentSales.GroupBy(s => s.BranchCode);
            //var prvGroups = sePrvSales.GroupBy(s => s.BranchCode).ToList();

            //var slNo = 1;
            //foreach (var group in groups)
            //{
            //    var prvGroup = prvGroups.FirstOrDefault(p => p.Key == group.Key);

            //    seWiseSaleDetails.Add(new SeWiseSaleDetail
            //    {
            //        SlNo = slNo,
            //        BranchCode = group.Key,
            //        SalesExecName = group.Select(g => g.SalesExecName).FirstOrDefault(),
            //        NumberOfCustomers = group.Count(),
            //        PrevAchieved = prvGroup?.Sum(s => Convert.ToDecimal(s.NetRetailSelling))
            //    });

            //    slNo++;
            //}
            #endregion

            var sqlParams = GetSqlParams(dbFilter, SaleAction.SeWiseBranchSaleDetail, SeCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, SeUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return seWiseBranchSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SeWiseSaleDetailResponse
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                SalesExecutiveName = dataRow.Field<string>("SalesExecutiveName"),
                NumberOfCustomers = dataRow.Field<int>("TotalCustomer"),
                AvgSale = dataRow.Field<decimal>("AverageSale"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SeWiseSaleDetailResponse
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var slNo = 1;
            foreach (var cs in currentSales)
            {
                var prvSale = previousSales.FirstOrDefault(p => p.BranchCode == cs.BranchCode);

                var achievedPer = CalculateAchievedPer(cs.NetRetailSelling, prvSale?.NetRetailSelling ?? 0);

                seWiseBranchSaleDetails.Add(new SeWiseBranchSaleDetail
                {
                    SlNo = slNo,
                    BranchCode = cs.BranchCode,
                    SalesExecName = cs.SalesExecutiveName,
                    NumberOfCustomers = cs.NumberOfCustomers,
                    AverageSale = cs.AvgSale,
                    PrevAchieved = prvSale?.NetRetailSelling,
                    Achieved = Math.Round(cs.NetRetailSelling, 2),
                    AchievedPercentage = achievedPer
                });

                slNo++;
            }

            return seWiseBranchSaleDetails;
        }

        public List<CustomerSaleDetail> GetSeWiseCustomerSaleDetails(SaleDbFilter dbFilter, out int totalRows)
        {
            totalRows = 0;
            var seWiseCustomerSaleDetails = new List<CustomerSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            #region Comment old code
            //if (Session.SeWiseCurrentSales == null)
            //{
            //    GetSubGroupWiseSale(dbFilter);
            //}
            //if (Session.SeWiseCurrentSales == null || Session.SeWisePrevSales == null) return seWiseCustomerSaleDetails;

            //var currentSales = !string.IsNullOrWhiteSpace(dbFilter.CustomerType) ? Session.SeWiseCurrentSales.Where(s => s.ConsPartyTypeDesc.Equals(dbFilter.CustomerType, StringComparison.OrdinalIgnoreCase)) : Session.SeWiseCurrentSales;
            //var prevSales = !string.IsNullOrWhiteSpace(dbFilter.CustomerType) ? Session.SeWisePrevSales.Where(s => s.ConsPartyTypeDesc.Equals(dbFilter.CustomerType, StringComparison.OrdinalIgnoreCase)) : Session.SeWisePrevSales;

            //var currentSalesWithCustomers = currentSales.Select(s => new
            //{
            //    CustomerCode = s.ConsPartyCode,
            //    CustomerName = s.ConsPartyName,
            //    CustomerType = s.ConsPartyTypeDesc
            //});
            //var prevSalesWithCustomers = prevSales.Select(s => new
            //{
            //    NetRetailSelling = Convert.ToDecimal(s.NetRetailSelling),
            //    CustomerCode = s.ConsPartyCode
            //});

            //var groups = currentSalesWithCustomers.GroupBy(s => s.CustomerCode);
            //var prvGroups = prevSalesWithCustomers.GroupBy(s => s.CustomerCode).ToList();

            //var slNo = 1;
            //foreach (var group in groups)
            //{
            //    var prvGroup = prvGroups.FirstOrDefault(p => p.Key == group.Key);

            //    seWiseCustomerSaleDetails.Add(new CustomerSaleDetail
            //    {
            //        SlNo = slNo,
            //        CustomerCode = group.Key,
            //        CustomerName = group.Select(g => g.CustomerName).FirstOrDefault(),
            //        CustomerType = group.Select(g => g.CustomerType).FirstOrDefault(),
            //        PrevAchieved = prvGroup?.Sum(s => s.NetRetailSelling)
            //    });

            //    slNo++;
            //}
            #endregion

            var action = string.IsNullOrWhiteSpace(dbFilter.SearchTxt) ? SaleAction.SeWiseCustomerSaleDetail : SaleAction.SeWiseCustomerSaleDetailWithSearch;
            var sqlParams = GetSqlParams(dbFilter, action, SeCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, SeUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return seWiseCustomerSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SeWiseSaleDetailResponse
            {
                TotalRows = dataRow.Field<int>("TotalRows"),
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SeWiseSaleDetailResponse
            {
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            totalRows = currentSales.First().TotalRows;

            var slNo = dbFilter.Skip + 1;
            foreach (var cs in currentSales)
            {
                var prvSale = previousSales.FirstOrDefault(p => p.CustomerCode == cs.CustomerCode);

                var achievedPer = CalculateAchievedPer(cs.NetRetailSelling, prvSale?.NetRetailSelling ?? 0);

                seWiseCustomerSaleDetails.Add(new CustomerSaleDetail
                {
                    SlNo = slNo,
                    CustomerCode = cs.CustomerCode,
                    CustomerName = cs.CustomerName,
                    CustomerType = cs.CustomerType,
                    PrevAchieved = prvSale?.NetRetailSelling,
                    Achieved = Math.Round(cs.NetRetailSelling, 2),
                    AchievedPercentage = achievedPer
                });

                slNo++;
            }

            return seWiseCustomerSaleDetails;
        }

        #endregion

        #region CS wise sale detail
        public List<CsWiseSaleDetail> GetCsWiseSaleDetails(SaleDbFilter dbFilter)
        {
            var csWiseSaleDetails = new List<CsWiseSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            #region Comment old code
            //if (Session.CsWiseCurrentSales == null)
            //{
            //    GetSubGroupWiseSale(dbFilter);
            //}
            //if (Session.CsWiseCurrentSales == null || Session.CsWisePrevSales == null) return csWiseSaleDetails;

            //var groups = Session.CsWiseCurrentSales.GroupBy(s => s.ConsPartyTypeDesc);
            //var prvGroups = Session.CsWisePrevSales.GroupBy(s => s.ConsPartyTypeDesc).ToList();

            //var slNo = 1;
            //foreach (var group in groups)
            //{
            //    var threeMonthSales = group.GroupBy(s => s.CreatedDate?.Month).Take(3).SelectMany(g => g).Sum(s => Convert.ToDecimal(s.NetRetailSelling));

            //    var prvGroup = prvGroups.FirstOrDefault(p => p.Key == group.Key);

            //    csWiseSaleDetails.Add(new CsWiseSaleDetail
            //    {
            //        SlNo = slNo,
            //        CustomerType = group.Key,
            //        NumberOfCustomers = group.Count(),
            //        AverageSale = Math.Round(threeMonthSales / 3, 2),
            //        PrevAchieved = prvGroup?.Sum(s => Convert.ToDecimal(s.NetRetailSelling))
            //    });

            //    slNo++;
            //}
            #endregion

            var sqlParams = GetSqlParams(dbFilter, SaleAction.CsWiseSaleDetails);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, CsUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return csWiseSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCustomers = dataRow.Field<int>("TotalCustomer"),
                AvgSale = dataRow.Field<decimal>("AverageSale"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var slNo = 1;
            foreach (var cs in currentSales)
            {
                var prvSale = previousSales.FirstOrDefault(p => p.CustomerType == cs.CustomerType);

                var achievedPer = CalculateAchievedPer(cs.NetRetailSelling, prvSale?.NetRetailSelling ?? 0);

                csWiseSaleDetails.Add(new CsWiseSaleDetail
                {
                    SlNo = slNo,
                    CustomerType = cs.CustomerType,
                    NumberOfCustomers = cs.NumberOfCustomers,
                    AverageSale = Math.Round(cs.AvgSale / 3, 2),
                    PrevAchieved = prvSale?.NetRetailSelling,
                    Achieved = Math.Round(cs.NetRetailSelling, 2),
                    AchievedPercentage = achievedPer
                });

                slNo++;
            }

            return csWiseSaleDetails;
        }

        public List<CustomerSaleDetail> GetCsWiseCustomerSaleDetails(SaleDbFilter dbFilter, out int totalRows)
        {
            totalRows = 0;
            var csWiseCustomerSaleDetails = new List<CustomerSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            #region Comment old code
            //if (Session.CsWiseCurrentSales == null)
            //{
            //    GetSubGroupWiseSale(dbFilter);
            //}
            //if (Session.CsWiseCurrentSales == null || Session.CsWisePrevSales == null) return csWiseCustomerSaleDetails;

            //var currentSales = Session.CsWiseCurrentSales.Where(s => s.ConsPartyTypeDesc.Equals(dbFilter.CustomerType, StringComparison.OrdinalIgnoreCase));
            //var prevSales = Session.CsWisePrevSales.Where(s => s.ConsPartyTypeDesc.Equals(dbFilter.CustomerType, StringComparison.OrdinalIgnoreCase));

            //var currentSalesWithCustomers = currentSales.Select(s => new
            //{
            //    CustomerCode = s.ConsPartyCode,
            //    CustomerName = s.ConsPartyName,
            //    CustomerType = s.ConsPartyTypeDesc
            //});
            //var prevSalesWithCustomers = prevSales.Select(s => new
            //{
            //    NetRetailSelling = Convert.ToDecimal(s.NetRetailSelling),
            //    CustomerCode = s.ConsPartyCode
            //});

            //var groups = currentSalesWithCustomers.GroupBy(s => s.CustomerCode);
            //var prvGroups = prevSalesWithCustomers.GroupBy(s => s.CustomerCode).ToList();

            //var slNo = 1;
            //foreach (var group in groups)
            //{
            //    var prvGroup = prvGroups.FirstOrDefault(p => p.Key == group.Key);

            //    csWiseCustomerSaleDetails.Add(new CustomerSaleDetail
            //    {
            //        SlNo = slNo,
            //        CustomerCode = group.Key,
            //        CustomerName = group.Select(g => g.CustomerName).FirstOrDefault(),
            //        CustomerType = group.Select(g => g.CustomerType).FirstOrDefault(),
            //        PrevAchieved = prvGroup?.Sum(s => s.NetRetailSelling)
            //    });

            //    slNo++;
            //}
            #endregion

            var action = string.IsNullOrWhiteSpace(dbFilter.SearchTxt)
                ? SaleAction.CsWiseCustomerSaleDetails
                : SaleAction.CsWiseCustomerSaleDetailsWithSearch;
            var sqlParams = GetSqlParams(dbFilter, action);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, CsUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return csWiseCustomerSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                TotalRows = dataRow.Field<int>("TotalRows"),
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new SaleDetailResponse
            {
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            totalRows = currentSales.First().TotalRows;

            var slNo = dbFilter.Skip + 1;
            foreach (var cs in currentSales)
            {
                var prvSale = previousSales.FirstOrDefault(p => p.CustomerCode == cs.CustomerCode);

                var achievedPer = CalculateAchievedPer(cs.NetRetailSelling, prvSale?.NetRetailSelling ?? 0);

                csWiseCustomerSaleDetails.Add(new CustomerSaleDetail
                {
                    SlNo = slNo,
                    CustomerCode = cs.CustomerCode,
                    CustomerName = cs.CustomerName,
                    CustomerType = cs.CustomerType,
                    PrevAchieved = prvSale?.NetRetailSelling,
                    Achieved = Math.Round(cs.NetRetailSelling, 2),
                    AchievedPercentage = achievedPer
                });

                slNo++;
            }
            return csWiseCustomerSaleDetails;
        }
        #endregion

        #region PG wise sale detail

        public List<PgWiseSaleDetail> GetPgWiseSaleDetails(SaleDbFilter dbFilter)
        {
            var pgWiseSaleDetails = new List<PgWiseSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, SaleAction.PgWiseSaleDetail, PgCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, PgUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return pgWiseSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new PgWiseSaleDetail
            {
                GroupType = dataRow.Field<string>("GroupType"),
                NumberOfCustomer = dataRow.Field<int>("TotalCustomer"),
                AverageSale = dataRow.Field<decimal>("AverageSale"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new PgWiseSaleDetail
            {
                GroupType = dataRow.Field<string>("GroupType"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var slNo = 1;
            var totalSale = currentSales.Sum(c => c.NetRetailSelling);
            foreach (var cs in currentSales)
            {
                var contribution = totalSale > 0 ? cs.NetRetailSelling * 100 / totalSale : 0;
                var prvSale = previousSales.FirstOrDefault(p => p.GroupType == cs.GroupType);

                pgWiseSaleDetails.Add(new PgWiseSaleDetail
                {
                    SlNo = slNo,
                    GroupType = cs.GroupType,
                    NumberOfCustomer = cs.NumberOfCustomer,
                    AverageSale = Math.Round(cs.AverageSale, 2),
                    NetRetailSelling = cs.NetRetailSelling,
                    Contribution = Math.Round(contribution, 2),
                    PrevAchieved = prvSale?.NetRetailSelling
                });

                slNo++;
            }

            return pgWiseSaleDetails;
        }

        public List<PgWiseCustomerSaleDetail> GetPgWiseCustomerSaleDetail(SaleDbFilter dbFilter)
        {
            var pgWiseCustomerSaleDetails = new List<PgWiseCustomerSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, SaleAction.PgWiseCustomerSaleDetail, PgCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, PgUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return pgWiseCustomerSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new PgWiseCustomerSaleDetail
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NumberOfCustomer = dataRow.Field<int>("TotalCustomer"),
                AverageSale = dataRow.Field<decimal>("AverageSale"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new PgWiseCustomerSaleDetail
            {
                CustomerType = dataRow.Field<string>("CustomerType"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var slNo = 1;
            var totalSale = currentSales.Sum(c => c.NetRetailSelling);
            foreach (var cs in currentSales)
            {
                var contribution = totalSale > 0 ? cs.NetRetailSelling * 100 / totalSale : 0;
                var prvSale = previousSales.FirstOrDefault(p => p.CustomerType == cs.CustomerType);

                pgWiseCustomerSaleDetails.Add(new PgWiseCustomerSaleDetail
                {
                    SlNo = slNo,
                    CustomerType = cs.CustomerType,
                    NumberOfCustomer = cs.NumberOfCustomer,
                    AverageSale = Math.Round(cs.AverageSale, 2),
                    NetRetailSelling = cs.NetRetailSelling,
                    Contribution = Math.Round(contribution, 2),
                    PrevAchieved = prvSale?.NetRetailSelling,
                    PartGroup = dbFilter.PartGroupName,
                    BranchCode = dbFilter.BranchCode
                });

                slNo++;
            }
            return pgWiseCustomerSaleDetails;
        }

        public List<PgWiseBranchSaleDetail> GetPgWiseBranchSaleDetail(SaleDbFilter dbFilter)
        {
            var pgWiseBranchSaleDetails = new List<PgWiseBranchSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var sqlParams = GetSqlParams(dbFilter, SaleAction.PgWiseBranchSaleDetail, PgCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, PgUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return pgWiseBranchSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new PgWiseBranchSaleDetail
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                BranchName = dataRow.Field<string>("BranchName"),
                NumberOfCustomer = dataRow.Field<int>("TotalCustomer"),
                AverageSale = dataRow.Field<decimal>("AverageSale"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new PgWiseBranchSaleDetail
            {
                BranchCode = dataRow.Field<string>("BranchCode"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var slNo = 1;
            var totalSale = currentSales.Sum(c => c.NetRetailSelling);
            foreach (var cs in currentSales)
            {
                var contribution = totalSale > 0 ? cs.NetRetailSelling * 100 / totalSale : 0;
                var prvSale = previousSales.FirstOrDefault(p => p.BranchCode == cs.BranchCode);

                pgWiseBranchSaleDetails.Add(new PgWiseBranchSaleDetail
                {
                    SlNo = slNo,
                    BranchCode = cs.BranchCode,
                    BranchName = cs.BranchName,
                    NumberOfCustomer = cs.NumberOfCustomer,
                    AverageSale = Math.Round(cs.AverageSale, 2),
                    NetRetailSelling = cs.NetRetailSelling,
                    Contribution = Math.Round(contribution, 2),
                    PrevAchieved = prvSale?.NetRetailSelling,
                    CustomerType = dbFilter.CustomerType
                });

                slNo++;
            }
            return pgWiseBranchSaleDetails;
        }

        public List<PgCustomerSaleDetail> GetPgWiseCustomerSaleDetails(SaleDbFilter dbFilter, out int totalRows)
        {
            totalRows = 0;
            var pgWiseCustomerSaleDetails = new List<PgCustomerSaleDetail>();

            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;

            var action = string.IsNullOrWhiteSpace(dbFilter.SearchTxt)
                ? SaleAction.PgWiseCustomerSale
                : SaleAction.PgWiseCustomerSaleWithSearch;
            var sqlParams = GetSqlParams(dbFilter, action, PgCaller);
            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, PgUspName, sqlParams);
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return pgWiseCustomerSaleDetails;

            var currentSales = ds.Tables[0].AsEnumerable().Select(dataRow => new PgCustomerSaleDetail
            {
                TotalRows = dataRow.Field<int>("TotalRows"),
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                CustomerName = dataRow.Field<string>("CustomerName"),
                CustomerType = dataRow.Field<string>("CustomerType"),
                AverageSale = dataRow.Field<decimal>("AverageSale"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            var previousSales = ds.Tables[1].AsEnumerable().Select(dataRow => new PgCustomerSaleDetail
            {
                CustomerCode = dataRow.Field<string>("CustomerCode"),
                NetRetailSelling = dataRow.Field<decimal>("NetRetailSelling")
            }).ToList();

            totalRows = currentSales.First().TotalRows;

            var slNo = dbFilter.Skip + 1;
            var totalSale = currentSales.Sum(c => c.NetRetailSelling);
            foreach (var cs in currentSales)
            {
                var contribution = totalSale > 0 ? cs.NetRetailSelling * 100 / totalSale : 0;
                var prvSale = previousSales.FirstOrDefault(p => p.CustomerCode == cs.CustomerCode);

                pgWiseCustomerSaleDetails.Add(new PgCustomerSaleDetail
                {
                    SlNo = slNo,
                    CustomerCode = cs.CustomerCode,
                    CustomerName = cs.CustomerName,
                    CustomerType = cs.CustomerType,
                    AverageSale = Math.Round(cs.AverageSale, 2),
                    NetRetailSelling = cs.NetRetailSelling,
                    Contribution = Math.Round(contribution, 2),
                    ContributionTxt = $"{Math.Round(contribution, 2)} %",
                    PrevAchieved = prvSale?.NetRetailSelling
                });

                slNo++;
            }
            return pgWiseCustomerSaleDetails;
        }

        #endregion

        private static SqlParameter[] GetSqlParams(SaleDbFilter dbFilter, SaleAction action, string caller = null)
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

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@Action", action),
                new SqlParameter("@Role", role),
                new SqlParameter("@UserId", dbFilter.UserId),
                new SqlParameter("@StartDate", dbFilter.StartDate),
                new SqlParameter("@EndDate", dbFilter.EndDate),
                new SqlParameter("@GroupName", dbFilter.Category),
                new SqlParameter("@CustomerType", dbFilter.CustomerType),
                new SqlParameter("@FirstDayOfMonth", firstDayOfMonth),
                new SqlParameter("@LastDayOfPrevMonth", prevMonthLastDay),
                new SqlParameter("@SearchTxt", dbFilter.SearchTxt),
                new SqlParameter("@Skip", dbFilter.Skip),
                new SqlParameter("@Take", dbFilter.PageSize)
            };

            if (caller == RoCaller || caller == SeCaller || caller == PgCaller)
                sqlParams.Add(new SqlParameter("@BranchCode", dbFilter.BranchCode));

            if (caller == PgCaller) sqlParams.Add(new SqlParameter("@PartGroupName", dbFilter.PartGroupName));

            return sqlParams.ToArray();
        }

        private static string CalculateAchievedPer(decimal netRetailSelling, decimal prevNetRetailSelling)
        {
            string achievedPer = null;
            if (prevNetRetailSelling > 0)
            {
                achievedPer = $"{Math.Round(netRetailSelling * 100 / prevNetRetailSelling, 2)}%";
            }
            else { achievedPer = $"{netRetailSelling}%"; }

            return achievedPer;
        }
    }
}