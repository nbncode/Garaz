using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoDashboard
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General general = new General();
        private const string CloseBal = "Closing Balance";
        // In order - Gold, Orange, Green, Deep sky blue, hot pink, dim gray, tan, red, dark khaki and teal
        #endregion

        #region Dashboard Detail
        public DashboardResponse GetDashboard(DashboardRequest model)
        {
            //string upiid = "";
            decimal saleValue = 0, cl = 0, tos = 0, cos = 0;
            DateTime? closingDate = null;
            List<Banner> bannerList = new List<Banner>();
            var general = new General();

            //var aspNetUser = db.AspNetUsers.Where(u => u.Id == model.UserId).FirstOrDefault();

            var distributorID = 0;

            if (model.Roles.Any())
            {
                if (model.StartDate == null && model.EndDate == null)
                {
                    DateTime now = DateTime.Now;
                    model.StartDate = new DateTime(now.Year, now.Month, 1);
                    model.EndDate = model.StartDate?.AddMonths(1).AddDays(-1);

                }
                int id = 0;

                if (model.Roles.Contains(Constants.Workshop) || model.Roles.Contains(Constants.WorkshopUsers))
                {
                    id = model.UserId.GetWorkshopId(Constants.Workshop);
                    var workshop = db.WorkShops.Where(x => x.WorkShopId == id).FirstOrDefault();
                    if (workshop != null)
                    {
                        cos = workshop.OutstandingAmount != null ? workshop.OutstandingAmount.Value : cos;
                        tos = workshop.TotalOutstanding != null ? workshop.TotalOutstanding.Value : tos;
                        cl = workshop.CreditLimit != null ? workshop.CreditLimit.Value : cl;
                        closingDate = workshop.AccountLedgers?.Where(a => a.Particulars == CloseBal).OrderByDescending(a => a.Date).Select(a => a.Date).FirstOrDefault();
                    }
                    var distributor = (from d in db.Distributors
                                       join w in db.DistributorWorkShops on d.DistributorId equals w.DistributorId
                                       where w.WorkShopId == id && w.UserId == model.UserId
                                       select d).FirstOrDefault();

                    distributorID = distributor != null ? distributor.DistributorId : 0;
                    saleValue = (from d in db.DailySalesTrackerWithInvoiceDatas
                                     //join p in db.Products on d.ProductId equals p.ProductId
                                 where !string.IsNullOrEmpty(d.NetRetailSelling) && d.WorkShopId == id
                                 && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
                                 select new { saleValue = d.NetRetailSelling }).ToList()
                                 .Sum(s => Convert.ToDecimal(s.saleValue));

                }
                else if (model.Roles.Contains(Constants.SuperAdmin))
                {
                    var workshop = db.WorkShops.ToList();
                    if (workshop.Count > 0)
                    {
                        cos = workshop.Where(w => w.OutstandingAmount != null).ToList().Sum(s => Convert.ToDecimal(s.OutstandingAmount));
                        tos = workshop.Where(w => w.TotalOutstanding != null).ToList().Sum(s => Convert.ToDecimal(s.TotalOutstanding));
                        cl = workshop.Where(w => w.CreditLimit != null).ToList().Sum(s => Convert.ToDecimal(s.CreditLimit));
                        closingDate = db.AccountLedgers.Where(a => a.Particulars == CloseBal).OrderByDescending(a => a.Date).Select(a => a.Date).FirstOrDefault();
                    }
                    saleValue = (from d in db.DailySalesTrackerWithInvoiceDatas
                                     //join p in db.Products on d.ProductId equals p.ProductId
                                 where !string.IsNullOrEmpty(d.NetRetailSelling)
                                 && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
                                 select new { saleValue = d.NetRetailSelling, d.DistributorId, d.WorkShopId }).ToList()
                                 .Sum(s => Convert.ToDecimal(s.saleValue));
                }
                else if (model.Roles.Contains(Constants.RoIncharge))
                {
                    var DistributorsOutlets = db.DistributorsOutlets.Where(a => a.UserId == model.UserId).FirstOrDefault();
                    if (DistributorsOutlets != null)
                    {
                        var workshop = (from w in db.WorkShops
                                        where w.outletId == DistributorsOutlets.OutletId
                                        select w).ToList();
                        if (workshop.Count > 0)
                        {
                            var wsIds = workshop.Select(w => w.WorkShopId);
                            cos = workshop.Where(w => w.OutstandingAmount != null).ToList().Sum(s => Convert.ToDecimal(s.OutstandingAmount));
                            tos = workshop.Where(w => w.TotalOutstanding != null).ToList().Sum(s => Convert.ToDecimal(s.TotalOutstanding));
                            cl = workshop.Where(w => w.CreditLimit != null).ToList().Sum(s => Convert.ToDecimal(s.CreditLimit));
                            closingDate = db.AccountLedgers.Where(a => a.Particulars == CloseBal && a.WorkshopId.HasValue && wsIds.Contains(a.WorkshopId.Value)).OrderByDescending(a => a.Date).Select(a => a.Date).FirstOrDefault();
                        }

                        var ids = workshop.Select(a => a.WorkShopId);

                        saleValue = (from d in db.DailySalesTrackerWithInvoiceDatas
                                         //join p in db.Products on d.ProductId equals p.ProductId
                                     where !string.IsNullOrEmpty(d.NetRetailSelling) && d.WorkShopId.HasValue && ids.Contains(d.WorkShopId.Value)
                                     && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
                                     select new { saleValue = d.NetRetailSelling }).ToList()
                                 .Sum(s => Convert.ToDecimal(s.saleValue));

                        distributorID = DistributorsOutlets.DistributorId.Value;
                    }
                }
                else if (model.Roles.Contains(Constants.SalesExecutive))
                {

                    var idsAll = db.SalesExecutiveWorkshops.Where(w => w.UserId == model.UserId).Select(w => w.WorkshopId).ToList();
                    var workshops = (from a in db.UserDetails
                                     join w in db.DistributorWorkShops on a.UserId equals w.UserId
                                     join ww in db.WorkShops on w.WorkShopId equals ww.WorkShopId
                                     where a.IsDeleted.Value == false && idsAll.Contains(w.WorkShopId.Value)
                                     select ww).ToList();

                    if (workshops.Count > 0)
                    {
                        var wsIds = workshops.Select(w => w.WorkShopId);
                        cos = workshops.Where(w => w.OutstandingAmount != null).ToList().Sum(s => Convert.ToDecimal(s.OutstandingAmount));
                        tos = workshops.Where(w => w.TotalOutstanding != null).ToList().Sum(s => Convert.ToDecimal(s.TotalOutstanding));
                        cl = workshops.Where(w => w.CreditLimit != null).ToList().Sum(s => Convert.ToDecimal(s.CreditLimit));
                        closingDate = db.AccountLedgers.Where(a => a.Particulars == CloseBal && a.WorkshopId.HasValue && wsIds.Contains(a.WorkshopId.Value)).OrderByDescending(a => a.Date).Select(a => a.Date).FirstOrDefault();
                    }

                    var ids = workshops.Select(a => a.WorkShopId);

                    saleValue = (from d in db.DailySalesTrackerWithInvoiceDatas
                                 where !string.IsNullOrEmpty(d.NetRetailSelling) && d.WorkShopId.HasValue && ids.Contains(d.WorkShopId.Value)
                                 && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
                                 select new { saleValue = d.NetRetailSelling }).ToList()
                             .Sum(s => Convert.ToDecimal(s.saleValue));

                    var dd = db.DistributorUsers.Where(a => a.UserId == model.UserId).FirstOrDefault();
                    if (dd != null)
                    {
                        distributorID = dd.DistributorId.Value;
                    }
                }
                else if (model.Roles.Contains(Constants.Distributor) || model.Roles.Contains(Constants.Users))
                {
                    id = general.GetDistributorId(model.UserId);
                    distributorID = id;
                    var workshop = (from w in db.WorkShops
                                    join d in db.DistributorWorkShops on w.WorkShopId equals d.WorkShopId
                                    where d.DistributorId == id
                                    select w).ToList();
                    if (workshop.Count > 0)
                    {
                        cos = workshop.Where(w => w.OutstandingAmount != null).ToList().Sum(s => Convert.ToDecimal(s.OutstandingAmount));
                        tos = workshop.Where(w => w.TotalOutstanding != null).ToList().Sum(s => Convert.ToDecimal(s.TotalOutstanding));
                        cl = workshop.Where(w => w.CreditLimit != null).ToList().Sum(s => Convert.ToDecimal(s.CreditLimit));
                        closingDate = db.AccountLedgers.Where(a => a.Particulars == CloseBal && a.DistributorId == id).OrderByDescending(a => a.Date).Select(a => a.Date).FirstOrDefault();
                    }

                    //var distributor = db.Distributors.Where(x => x.DistributorId == id).FirstOrDefault();
                    //upiid = distributor != null ? (distributor.UPIID ?? "") : "";

                    saleValue = (from d in db.DailySalesTrackerWithInvoiceDatas
                                     //join p in db.Products on d.ProductId equals p.ProductId
                                 where !string.IsNullOrEmpty(d.NetRetailSelling) && d.DistributorId == id
                                 && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
                                 select new { saleValue = d.NetRetailSelling }).ToList()
                                 .Sum(s => Convert.ToDecimal(s.saleValue));
                }

                var bannerDetail = db.BannerMobiles.Where(a => a.DistributorId == (model.Roles.Contains(Constants.SuperAdmin) ? a.DistributorId : distributorID));

                if (bannerDetail.Any())
                {
                    foreach (var item in bannerDetail)
                    {
                        if (item.SchemeId.HasValue)
                        {
                            if (db.TargetWorkShops.Where(a => a.SchemeId == item.SchemeId && a.WorkShopId == id).FirstOrDefault() != null && item.Scheme != null && item.Scheme.IsActive.HasValue && item.Scheme.IsActive == true && (item.Scheme.IsDeleted == null || item.Scheme.IsDeleted == false))
                            {
                                bannerList.Add(new Banner()
                                {
                                    Heading = item.BannerName ?? "",
                                    ImagePath = general.CheckImageUrl(item.BannerImage),
                                    Type = item.Type ?? "",
                                    Data = item.Data ?? "",
                                    SchemeId = item.SchemeId.Value
                                });
                            }
                        }
                        else
                        {
                            bannerList.Add(new Banner()
                            {
                                Heading = item.BannerName ?? "",
                                ImagePath = general.CheckImageUrl(item.BannerImage),
                                Type = item.Type ?? "",
                                Data = item.Data ?? "",
                                SchemeId = 0
                            });
                        }
                    }
                }

            }
            return new DashboardResponse()
            {
                UPID = "", //upiid,
                Heading = Constants.DashboardHeading,
                Description = Constants.DashboardDescription,
                SaleHeading = model != null ? $"Sale ({model.StartDate.Value.ToString("MMM yyyy")})" : "",
                SaleSubHeading = "Sale",
                SaleValue = string.IsNullOrEmpty(String.Format("{0:#,###}", saleValue)) ? "0" : String.Format("{0:#,###}", saleValue),
                CreditLimit = string.IsNullOrEmpty(String.Format("{0:#,###}", cl)) ? "0" : String.Format("{0:#,###}", cl),
                TotalOutstanding = string.IsNullOrEmpty(String.Format("{0:#,###}", tos)) ? "0" : String.Format("{0:#,###}", tos),
                CriticalOutstanding = string.IsNullOrEmpty(String.Format("{0:#,###}", cos)) ? "0" : String.Format("{0:#,###}", cos),
                ClosingDate = closingDate != null ? closingDate.Value.ToString("dd/MMM/yyyy") : "",
                Banners = bannerList
            };
        }

        #endregion

        #region Dashboard ProductType Details Detail
        public DashboardProductTypeDetail DashboardProductTypeDetails(DashboardRequest model, out string UPIID, bool IsCallFromDashboard)
        {
            decimal outStantdingTotal = 0, MGA = 0, MGP = 0, MGO = 0;
            dynamic data = null; UPIID = "";
            var general = new General();
            if (model.Roles.Any())
            {
                if (model.StartDate == null && model.EndDate == null)
                {
                    DateTime now = DateTime.Now;
                    model.StartDate = new DateTime(now.Year, now.Month, 1);
                    model.EndDate = model.StartDate?.AddMonths(1).AddDays(-1);
                    if (IsCallFromDashboard)
                    {
                        model.StartDate = model.StartDate.Value.AddMonths(-1);
                        model.EndDate = model.EndDate.Value.AddMonths(-1);
                    }
                }

                int id = 0;
                if (model.Roles.Contains(Constants.Distributor) || model.Roles.Contains(Constants.Users))
                {
                    id = general.GetDistributorId(model.UserId);
                    if (IsCallFromDashboard)
                    {
                        var distributor = db.Distributors.Where(x => x.DistributorId == id).FirstOrDefault();
                        UPIID = distributor != null ? (distributor.UPIID ?? UPIID) : UPIID;
                    }

                    data = (from d in db.DailySalesTrackerWithInvoiceDatas
                            join p in db.Products on d.ProductId equals p.ProductId
                            where d.DistributorId == id && d.NetRetailSelling != null && d.NetRetailSelling != string.Empty && p.ProductType != null
                            && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
                            select new { totalSales = d.NetRetailSelling, p.ProductType } into g
                            group new { g } by new { g.ProductType } into k
                            select k).ToList().Select(a => new { ProductType = a.First().g.ProductType, totalSales = a.Sum(b => float.Parse(b.g.totalSales)) });
                }
                else if (model.Roles.Contains(Constants.Workshop) || model.Roles.Contains(Constants.WorkshopUsers))
                {
                    id = model.UserId.GetWorkshopId(Constants.Workshop);
                    if (IsCallFromDashboard)
                    {
                        var distributor = (from d in db.Distributors
                                           join w in db.DistributorWorkShops on d.DistributorId equals w.DistributorId
                                           where w.WorkShopId == id && w.UserId == model.UserId
                                           select d).FirstOrDefault();
                        UPIID = distributor != null ? (distributor.UPIID ?? UPIID) : UPIID;
                    }

                    data = (from d in db.DailySalesTrackerWithInvoiceDatas
                            join p in db.Products on d.ProductId equals p.ProductId
                            where d.WorkShopId == id && d.NetRetailSelling != null && d.NetRetailSelling != string.Empty && p.ProductType != null
                            && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
                            select new { totalSales = d.NetRetailSelling, p.ProductType } into g
                            group new { g } by new { g.ProductType } into k
                            select k).ToList().Select(a => new { ProductType = a.First().g.ProductType, totalSales = a.Sum(b => float.Parse(b.g.totalSales)) });


                    var workshop = db.WorkShops.Where(x => x.WorkShopId == id).FirstOrDefault();
                    outStantdingTotal = workshop != null ? Convert.ToDecimal(workshop.OutstandingAmount) : outStantdingTotal;
                }
                else if (model.Roles.Contains(Constants.SuperAdmin))
                {
                    if (IsCallFromDashboard)
                    {
                        var distributor = db.Distributors.FirstOrDefault();
                        UPIID = distributor != null ? (distributor.UPIID ?? Constants.UPIID) : Constants.UPIID;
                    }

                    data = (from d in db.DailySalesTrackerWithInvoiceDatas
                            join p in db.Products on d.ProductId equals p.ProductId
                            where d.NetRetailSelling != null && d.NetRetailSelling != string.Empty && p.ProductType != null
                            && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
                            select new { totalSales = d.NetRetailSelling, p.ProductType } into g
                            group new { g } by new { g.ProductType } into k
                            select k).ToList().Select(a => new { ProductType = a.First().g.ProductType, totalSales = a.Sum(b => float.Parse(b.g.totalSales)) });
                }

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        if (item.ProductType.Equals("Oil", StringComparison.OrdinalIgnoreCase))
                            MGO = Convert.ToDecimal(item.totalSales);
                        else if (item.ProductType.Equals("Accessories", StringComparison.OrdinalIgnoreCase))
                            MGA = Convert.ToDecimal(item.totalSales);
                        else
                            MGP = Convert.ToDecimal(item.totalSales);
                    }
                }
            }
            var dashboardProductTypeDetail = new DashboardProductTypeDetail()
            {
                MGA = MGA,
                MGP = MGP,
                MGO = MGO,
                TotalSales = MGA + MGP + MGO,
                Total = outStantdingTotal
            };
            return dashboardProductTypeDetail;
        }
        #endregion

        #region Get Available Stock
        //public List<ProductModel> GetAvailableStock(StockModel model, out int totalRecord)
        //{
        //    totalRecord = 0;
        //    int distId = model.UserId.GetDistributorId(model.Role);
        //    var products = db.Products.Where(p => p.DistributorId == (model.Role != Constants.SuperAdmin ? distId : p.DistributorId)).ToList();
        //    if (!string.IsNullOrEmpty(model.PartNumber))
        //    {
        //        products = products.Where(x => !string.IsNullOrEmpty(x.PartNo) && x.PartNo.Contains(model.PartNumber)).ToList();
        //    }
        //    if (model.OutletId != null && model.OutletId > 0)
        //    {
        //        var dailyStock = db.DailyStocks.Where(x => x.OutletId == model.OutletId);
        //        products = (from p in products
        //                    join d in dailyStock on p.PartNo equals d.PartNum
        //                    select p
        //                   ).ToList();
        //    }
        //    if (model.StockColorId != null && model.StockColorId > 0)
        //    {
        //        var stockColor = db.StockColors.Where(x => x.StockId == model.StockColorId.Value).FirstOrDefault();
        //        if (stockColor != null)
        //        {
        //            if (stockColor.StockType == Constants.StockTypeLow.ToLower() || stockColor.StockType == Constants.StockTypeMediumn.ToLower())
        //            {
        //                products = (from p in products
        //                            join d in db.DailyStocks on p.PartNo equals d.PartNum
        //                            where !string.IsNullOrEmpty(d.StockQuantity) && Convert.ToInt32(d.StockQuantity) <= stockColor.Qty
        //                            select p
        //                   ).ToList();

        //            }
        //            else
        //            {
        //                var MediumQty = db.StockColors.Where(a => a.StockType == Constants.StockTypeMediumn.ToString()).FirstOrDefault();

        //                products = (from p in products
        //                            join d in db.DailyStocks on p.PartNo equals d.PartNum
        //                            where !string.IsNullOrEmpty(d.StockQuantity) && Convert.ToInt32(d.StockQuantity) >= stockColor.Qty
        //                            select p
        //                   ).ToList();
        //            }
        //        }
        //    }

        //    totalRecord = products.Count();
        //    return GetProdModel(products, distId, model.PageNumber);
        //}

        public List<ProductModel> GetAvailableStock(StockModel model, out int totalRecord)
        {
            totalRecord = 0;
            SqlParameter[] param = {
                                           new SqlParameter("@Role",model.Role),
                                           new SqlParameter("@UserId",model.UserId),
                                           new SqlParameter("@Index",model.PageNumber),
                                           new SqlParameter("@PartNumber",model.PartNumber),
                                           new SqlParameter("@OutletId",model.OutletId),
                                           new SqlParameter("@StockColorId",model.StockColorId),
                                       };

            DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "Sp_AvailableStock", param);
            List<ProductModel> productModelList = new List<ProductModel>();
            IQueryable<TempOrderDetail> tempOrderDetails = Enumerable.Empty<TempOrderDetail>().AsQueryable();
            if (model.TempOrderId.HasValue)
            {
                tempOrderDetails = db.TempOrderDetails.Where(a => a.TempOrderId == model.TempOrderId.Value);
            }

            if (ds.Tables.Count == 0) return productModelList;
            if (ds.Tables[0].Rows.Count <= 0) return productModelList;

            foreach (DataRow item in ds.Tables[0].Rows)
            {

                var productModel = new ProductModel();
                productModel.ProductId = item["ProductId"] != DBNull.Value ? Convert.ToInt32(item["ProductId"]) : 0;
                productModel.GroupId = item["GroupId"] != DBNull.Value ? Convert.ToInt32(item["GroupId"]) : 0;
                productModel.ProductName = item["ProductName"] != DBNull.Value ? Convert.ToString(item["ProductName"]) : Convert.ToString(item["Description"]);
                productModel.PartNumber = item["PartNo"] != DBNull.Value ? Convert.ToString(item["PartNo"]) : "";
                productModel.Description = item["Description"] != DBNull.Value ? Convert.ToString(item["Description"]) : "";

                var price = item["Price"] != DBNull.Value ? Convert.ToDecimal(item["Price"]) : 0;
                productModel.Price = price > 0 ? string.Format("{0:#,###}", price) : "0";

                string imgPath = item["ImagePath"] != DBNull.Value ? Convert.ToString(item["ImagePath"]) : "";
                productModel.ImagePath = general.CheckImageUrl(!string.IsNullOrEmpty(imgPath) ? imgPath : "/assets/images/NoPhotoAvailable.png");

                // Set color and color tag
                if (model.OutletId.HasValue && model.OutletId.Value > 0)
                {
                    productModel.Stock = item["StockQuantity"] != DBNull.Value ? Convert.ToInt32(item["StockQuantity"]) : 0;
                }
                else
                {
                    productModel.Stock = item["CurrentStock"] != DBNull.Value ? Convert.ToInt32(item["CurrentStock"]) : 0;
                }
                var stockColor = GetStockColor(productModel.Stock);
                productModel.Color = stockColor?.Color;
                productModel.ColorTag = stockColor?.Tag;

                productModel.DistributorId = item["DistributorId"] != DBNull.Value ? Convert.ToInt32(item["DistributorId"]) : 0;
                totalRecord = item["Total"] != DBNull.Value ? Convert.ToInt32(item["Total"]) : 0;

                var cartItem = tempOrderDetails.FirstOrDefault(a => a.ProductId == productModel.ProductId);
                if (cartItem != null)
                {
                    productModel.CartQty = cartItem.Qty;
                    productModel.CartAvailabilityType = cartItem.AvailabilityType;
                    productModel.CartOutletId = cartItem.OutletId;
                }
                productModelList.Add(productModel);
            }
            return productModelList;
        }

        private List<ProductModel> GetProdModel(List<Product> products, int distId, int pageNumber)
        {
            List<ProductModel> listPsm = null;
            var filteredProducts = products.GetPaging<Product>(pageNumber, null);
            if (filteredProducts.Any())
            {
                listPsm = new List<ProductModel>();
                foreach (var prod in filteredProducts)
                {
                    var colorData = GetStockColor(prod.CurrentStock);
                    listPsm.Add(new ProductModel
                    {
                        ProductId = prod.ProductId,
                        ProductName = prod.ProductName,
                        PartNumber = prod.PartNo,
                        Description = prod.Description,
                        Price = !prod.Price.HasValue ? "0" : String.Format("{0:#,###}", prod.Price),
                        ImagePath = prod.ImagePath,
                        Color = colorData?.Color,
                        ColorTag = colorData?.Tag,
                        DistributorId = distId
                    });
                }
                listPsm.ForEach(a =>
                {
                    a.ImagePath = general.CheckImageUrl(!string.IsNullOrEmpty(a.ImagePath) ? a.ImagePath : "/assets/images/NoPhotoAvailable.png");
                });
            }
            return listPsm;
        }


        /// <summary>
        /// Get color for product based on stock value.
        /// </summary>
        /// <param name="currentStock">The quantity of current stock.</param>
        /// <returns>Return color object.</returns>
        public StockColor GetStockColor(int? currentStock)
        {
            StockColor stockColor = null;
            if (currentStock.HasValue)
            {
                var lowStockQty = db.StockColors.Where(s => s.StockType == "Low").Select(s => s.Qty).FirstOrDefault();
                var medStockQty = db.StockColors.Where(s => s.StockType == "Medium").Select(s => s.Qty).FirstOrDefault();

                if (currentStock.Value <= lowStockQty)
                {
                    stockColor = db.StockColors.Where(s => s.StockType == "Low").FirstOrDefault();
                }
                else if (currentStock.Value > lowStockQty && currentStock.Value <= medStockQty)
                {
                    stockColor = db.StockColors.Where(s => s.StockType == "Medium").FirstOrDefault();
                }
                else if (currentStock.Value > medStockQty)
                {
                    stockColor = db.StockColors.Where(s => s.StockType == "High").FirstOrDefault();
                }
            }
            return stockColor;
        }
        #endregion

        #region Get Customer Back Order
        public List<CustomerBackOrderModel> GetCustomerBackOrders(string userId, string role, DateTime startDate, DateTime endDate, int pageNumber, out int totalRecord)
        {
            var aspNetUser = db.AspNetUsers.FirstOrDefault(a => a.Id == userId);
            var cbOrders = new List<CustomerBackOrder>();
            totalRecord = 0;

            if (role == Constants.Distributor || role == Constants.Users)
            {
                var distId = userId.GetDistributorId(role);
                cbOrders = db.CustomerBackOrders.Where(c => c.DistributorId == distId && c.CODate >= startDate && c.CODate <= endDate).ToList();
            }
            else if (role == Constants.Workshop || role == Constants.WorkshopUsers)
            {
                var wsId = userId.GetWorkshopId(role);
                cbOrders = db.CustomerBackOrders.Where(c => c.WorkshopId == wsId && c.CODate >= startDate && c.CODate <= endDate).ToList();
            }
            else if (role == Constants.SuperAdmin)
            {
                cbOrders = db.CustomerBackOrders.Where(c => c.CODate >= startDate && c.CODate <= endDate).ToList();
            }
            else if (aspNetUser?.AspNetRoles.FirstOrDefault(a => a.Name.Contains(Constants.SalesExecutive)) != null)
            {
                var wIds = db.SalesExecutiveWorkshops.Where(se => se.UserId == userId).Select(w => w.WorkshopId).ToList();
                cbOrders = db.CustomerBackOrders.Where(c => c.CODate >= startDate && c.CODate <= endDate && wIds.Contains(c.WorkshopId ?? 0)).ToList();
            }
            else if (aspNetUser?.AspNetRoles.FirstOrDefault(a => a.Name.Contains(Constants.RoIncharge)) != null)
            {
                var distOutlet = db.DistributorsOutlets.FirstOrDefault(o => o.UserId == userId);
                var outletId = distOutlet != null ? Convert.ToInt32(distOutlet.OutletId) : 0;
                var wIds = (from w in db.WorkShops
                            where w.outletId == outletId
                            select w.WorkShopId).Distinct().ToList();
                cbOrders = db.CustomerBackOrders.Where(c => c.CODate >= startDate && c.CODate <= endDate && wIds.Contains(c.WorkshopId ?? 0)).ToList();
            }

            if (cbOrders.Count == 0) return new List<CustomerBackOrderModel>();

            cbOrders=cbOrders.OrderBy(o => o.PartyName).ToList();

            var coNumbers = cbOrders.GroupBy(a => a.CONo).Select(a => a.FirstOrDefault()?.CONo).ToList();

            totalRecord = coNumbers.Count;

            // Skip and Take 
            const int pageSize = 10;
            var skip = (pageNumber - 1) * pageSize;
            coNumbers = coNumbers.Skip(skip).Take(pageSize).ToList();

            var cboModels = new List<CustomerBackOrderModel>();
            foreach (var coCbOrders in coNumbers.Select(coNo => cbOrders.Where(c => c.CONo == coNo).ToList()))
            {
                var status = string.Empty;
                if (coCbOrders.Count(a => a.PartStatus == "Accept") == coCbOrders.Count)
                {
                    status = "Accept";
                }
                else if (coCbOrders.Count(a => a.PartStatus == "Cancel") == coCbOrders.Count)
                {
                    status = "Cancel";
                }
                else if (coCbOrders.Any(a => a.PartStatus == "Accept") && coCbOrders.Any(c => c.PartStatus == "Cancel"))
                {
                    // Orders contain both 'Accept' and 'Cancel'
                    status = "Partial";
                }

                var cboModel = new CustomerBackOrderModel
                {
                    Products = new List<ProductModel>(),
                    Qty = coCbOrders.Count,
                    OrderNo = coCbOrders.FirstOrDefault()?.CONo,
                    OrderDate = coCbOrders.FirstOrDefault()?.CODate?.ToString("dd MMM, yyyy"),
                    Status = status,
                    PartyName= coCbOrders.FirstOrDefault()?.PartyName

                };
                cboModels.Add(cboModel);
            }
            return cboModels;
        }

        #endregion

        #region Fast Moving

        #region Comment old code
        //public List<FastMovingResponse> FastMoving(FastMovingRequest model, out int TotalRecord)
        //{
        //    int distributorId = 0; decimal discount = Constants.FValue; TotalRecord = 0;
        //    var fastMovingList = new List<FastMovingResponse>();
        //    if (model.Role != Constants.SuperAdmin)
        //    {
        //        distributorId = model.UserId.GetDistributorId(model.Role);
        //    }
        //    var distributor = db.Distributors.Where(x => x.DistributorId == distributorId).FirstOrDefault();
        //    if (distributor != null)
        //    {
        //        discount = distributor.FValue != null ? (distributor.FValue.Value == 0 ? Constants.FValue : distributor.FValue.Value) : Constants.FValue;
        //    }

        //    var groupsSales = (from d in db.DailySalesTrackerWithInvoiceDatas
        //                       where d.GroupId != null && d.DistributorId != null && d.DistributorId == (distributorId != 0 ? distributorId : d.DistributorId)
        //                       && d.CreatedDate != null && d.CreatedDate >= model.StartDate && d.CreatedDate <= model.EndDate
        //                       select new { totalSales = d.NetRetailSelling, month = d.CreatedDate.HasValue ? d.CreatedDate.Value.Month : 0, d.GroupId } into g
        //                       group new { g } by new { g.GroupId, g.month } into k
        //                       select k).ToList().Select(a => new { GroupId = a.First().g.GroupId, Month = a.First().g.month, Total = a.Sum(b => Convert.ToDouble(b.g.totalSales)) }).ToList();

        //    var grandTotal = Convert.ToDecimal(groupsSales.Sum(s => s.Total));

        //    var groups = (from g in groupsSales
        //                  join pg in db.ProductGroups on g.GroupId equals pg.GroupId
        //                  where g.GroupId != null
        //                  select new { pg.GroupId, pg.GroupName, g.Month, g.Total }
        //                 );
        //    // Prepare list of products until sum of each price doesn't be equal to target value
        //    decimal targetValue = discount > 0 ? grandTotal * discount / 100 : grandTotal;
        //    decimal saleProdTotalPrice = 0;

        //    foreach (var group in groups)
        //    {
        //        var fmResponse = new FastMovingResponse();
        //        if (!fastMovingList.Where(x => x.GroupId == group.GroupId).Any())
        //        {
        //            saleProdTotalPrice += Convert.ToDecimal(group.Total);
        //            if (saleProdTotalPrice > targetValue)
        //            {
        //                fmResponse.GroupId = group.GroupId;
        //                fmResponse.Group = group.GroupName;
        //                var monthWiseSale = groups.Where(x => x.GroupId == group.GroupId);
        //                fmResponse.data = new List<MonthWiseSale>();
        //                foreach (var val in monthWiseSale)
        //                {
        //                    var data = new MonthWiseSale();
        //                    data.Name = val.Month > 0 ? new DateTimeFormatInfo().GetMonthName(val.Month).Substring(0, 3) : "";
        //                    data.Value = Math.Round(val.Total);
        //                    fmResponse.data.Add(data);
        //                }
        //                fastMovingList.Add(fmResponse);
        //                break;
        //            }
        //            else
        //            {
        //                fmResponse.GroupId = group.GroupId;
        //                fmResponse.Group = group.GroupName;
        //                var monthWiseSale = groups.Where(x => x.GroupId == group.GroupId);
        //                fmResponse.data = new List<MonthWiseSale>();
        //                foreach (var val in monthWiseSale)
        //                {
        //                    var data = new MonthWiseSale();
        //                    data.Name = val.Month > 0 ? new DateTimeFormatInfo().GetMonthName(val.Month).Substring(0, 3) : "";//val.Month.ToString();
        //                    data.Value = Math.Round(val.Total);
        //                    fmResponse.data.Add(data);
        //                }
        //                fastMovingList.Add(fmResponse);
        //            }
        //        }
        //    }
        //    fastMovingList = (from pg in db.ProductGroups.AsEnumerable()
        //                      join f in fastMovingList on pg.GroupId equals f.GroupId into ps
        //                      from p in ps.DefaultIfEmpty()
        //                      select new FastMovingResponse()
        //                      {
        //                          GroupId = pg.GroupId,
        //                          Group = pg.GroupName,
        //                          data = p != null ? p.data : new List<MonthWiseSale>()
        //                      }).ToList();

        //    TotalRecord = fastMovingList.Count;
        //    if (model.PageNumber > 0)
        //    {
        //        return fastMovingList.GetPaging<FastMovingResponse>(model.PageNumber, model.PageSize);
        //    }
        //    return fastMovingList;
        //}
        #endregion

        public List<FastMovingResponse> FastMoving(FastMovingRequest model, out int totalRecord)
        {
            try
            {
                totalRecord = 0;
                SqlParameter[] param = {
                                           new SqlParameter("@Role",model.Role),
                                           new SqlParameter("@UserId",model.UserId),
                                           new SqlParameter("@Index",model.PageNumber),
                                           new SqlParameter("@StartDate",model.StartDate),
                                           new SqlParameter("@EndDate",model.EndDate),
                                       };

                DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "Sp_DailySalesData", param);
                List<FastMovingResponse> fmList = new List<FastMovingResponse>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    var datatable = ds.Tables[0];
                    foreach (DataRow item in datatable.Rows)
                    {

                        var fm = new FastMovingResponse();
                        fm.GroupId = item["GroupId"] != DBNull.Value ? Convert.ToInt32(item["GroupId"]) : 0;
                        fm.Group = item["GroupName"] != DBNull.Value ? Convert.ToString(item["GroupName"]) : "";
                        totalRecord = item["Total"] != DBNull.Value ? Convert.ToInt32(item["Total"]) : 0;

                        var monthName = item["MonthName"] != DBNull.Value ? Convert.ToString(item["MonthName"]) : "";
                        var Year = item["Year"] != DBNull.Value ? Convert.ToString(item["Year"]) : "";
                        var Qty = string.IsNullOrEmpty(item["TotalQty"].ToString()) ? "0" : item["TotalQty"].ToString();
                        var d = fmList.Where(a => a.GroupId == fm.GroupId);
                        if (d.Count() > 0)
                        {
                            d.FirstOrDefault().data.Add(new MonthWiseSale()
                            {
                                Name = monthName + " " + Year,
                                Qty = Qty
                            });
                        }
                        else
                        {
                            fm.data = new List<MonthWiseSale>();
                            if (!string.IsNullOrEmpty(monthName))
                            {
                                fm.data.Add(new MonthWiseSale()
                                {
                                    Name = monthName + " " + Year,
                                    Qty = Qty
                                });
                            }
                            fmList.Add(fm);
                        }
                    }
                }
                return fmList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //public List<FastMovingResponse> FastMoving(FastMovingRequest model, out int totalRecord)
        //{
        //    try
        //    {
        //        SqlParameter[] param = {
        //                                   new SqlParameter("@QryType","FastMoving")
        //                               };

        //        DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "Sp_DailySales", param);
        //        List<FastMovingResponse> fastMovingList = new List<FastMovingResponse>();
        //        totalRecord = 0;
        //        if (ds.Tables[0].Rows.Count > 0)
        //        {
        //            foreach (DataRow item in ds.Tables[0].Rows)
        //            {
        //                var fastMoving = new FastMovingResponse();
        //                fastMoving.GroupId = item["GroupId"] != DBNull.Value ? Convert.ToInt32(item["GroupId"]) : 0;
        //                fastMoving.Group = item["Group"] != DBNull.Value ? Convert.ToString(item["Group"]) : "";
        //                fastMoving.data = JsonConvert.DeserializeObject<List<MonthWiseSale>>(item["BarometerJson"].ToString()).ToList();
        //                fastMovingList.Add(fastMoving);
        //            }
        //            totalRecord = fastMovingList.Count();
        //            fastMovingList = fastMovingList.GetPaging<FastMovingResponse>(model.PageNumber, model.PageSize);
        //        }
        //        return fastMovingList;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        #endregion

        #region Sales Return and Invoice
        public List<SalesReturnResponse> SalesReturn(SalesReturnRequest model, bool isInvoice, out int totalRecord)
        {
            totalRecord = 0;
            var lstSalesReturn = new List<SalesReturnResponse>();
            IOrderedQueryable<DailySalesTrackerWithInvoiceData> salesReturn = null;

            if (model.Roles.Contains(Constants.SuperAdmin))
            {
                salesReturn = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId);
            }
            else if (model.Roles.Contains(Constants.Distributor))
            {
                var distributorId = model.UserId.GetDistributorId(Constants.Distributor);
                salesReturn = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId
                                && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId);
            }
            else if (model.Roles.Contains(Constants.Workshop) || model.Roles.Contains(Constants.WorkshopUsers))
            {
                var workshopId = model.UserId.GetWorkshopId(Constants.Workshop);
                salesReturn = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.WorkShopId != null && x.WorkShopId == workshopId
                                                                              && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId);
            }
            else if (model.Roles.Contains(Constants.RoIncharge))
            {
                var distributorId = model.UserId.GetDistributorId(Constants.RoIncharge);
                var distributorsOutlet = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == model.UserId);
                if (distributorsOutlet != null)
                {
                    var workshop = (from w in db.WorkShops
                                    where w.outletId == distributorsOutlet.OutletId
                                    select w).ToList();
                    if (workshop.Count > 0)
                    {
                        var ids = workshop.Select(a => a.WorkShopId).ToList();

                        salesReturn = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId && x.WorkShopId.HasValue && ids.Contains(x.WorkShopId ?? 0)
                                 && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId);
                    }
                }
            }
            else if (model.Roles.Contains(Constants.SalesExecutive))
            {
                var wIds = db.SalesExecutiveWorkshops.Where(se => se.UserId == model.UserId).Select(w => w.WorkshopId).ToList();
                var distributorId = model.UserId.GetDistributorId(Constants.SalesExecutive);
                salesReturn = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId && x.WorkShopId.HasValue && wIds.Contains(x.WorkShopId ?? 0)
                                 && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId);
            }
            else if (model.Roles.Contains(Constants.Users))
            {
                var distributorId = model.UserId.GetDistributorId(Constants.Users);
                salesReturn = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId
                                                                              && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId);
            }

            salesReturn = isInvoice ? salesReturn?.Where(x => string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0").OrderBy(o => o.DailySalesTrackerId) : salesReturn?.Where(x => !string.IsNullOrEmpty(x.ReturnQty) && x.ReturnQty != "0").OrderBy(o => o.DailySalesTrackerId);

            if (salesReturn != null && salesReturn.Any())
            {
                totalRecord = salesReturn.Count();
                var data = model.PageNumber > 0 ? salesReturn.PagedData(model.PageNumber, model.PageSize) : salesReturn;
                var products = (from g in data
                                join pg in db.Products on g.ProductId equals pg.ProductId
                                where g.ProductId != null
                                select new
                                {
                                    pg.GroupId,
                                    pg.ProductId,
                                    pg.ProductName,
                                    pg.Description,
                                    pg.PartNo,
                                    pg.Price,
                                    pg.ImagePath,
                                    g.CreatedDate
                                }).ToList()
                     .Select(pg => new SalesReturnResponse
                     {
                         GroupId = pg.GroupId ?? 0,
                         ProductId = pg.ProductId,
                         ProductName = string.IsNullOrEmpty(pg.ProductName) ? pg.Description : pg.ProductName,
                         Description = pg.Description ?? "",
                         PartNumber = pg.PartNo ?? "",
                         Price = !pg.Price.HasValue ? "0" : $"{pg.Price:#,###}",
                         ImagePath = pg.ImagePath,
                         Date = pg.CreatedDate
                     }).ToList();

                var tempOrderDetails = Enumerable.Empty<TempOrderDetail>().AsQueryable();
                if (model.TempOrderId.HasValue)
                {
                    tempOrderDetails = db.TempOrderDetails.Where(a => a.TempOrderId == model.TempOrderId.Value);
                }

                products.ForEach(a =>
                {
                    a.ImagePath = general.CheckImageUrl(a.ImagePath);
                    a.ReturnDate = a.Date?.ToString("dd MMM, yyyy");
                    var cartItem = tempOrderDetails.FirstOrDefault(b => b.ProductId == a.ProductId);
                    if (cartItem != null)
                    {
                        a.CartQty = cartItem.Qty;
                        a.CartAvailabilityType = cartItem.AvailabilityType;
                        a.CartOutletId = cartItem.OutletId;
                    }
                });
                lstSalesReturn = products;
            }
            return lstSalesReturn;
        }
        #endregion

        #region Invoice
        public List<InvoiceResponse> GetInvoice(SalesReturnRequest model, out int totalRecord)
        {
            totalRecord = 0;
            var invoices = new List<InvoiceResponse>();

            if (model.Roles.Contains(Constants.SuperAdmin))
            {
                invoices = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.CreatedDate != null
                                                                                               && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo)
                                                                                               && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList()
                    .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse { CoNo = s.Key, Date = s.FirstOrDefault().CreatedDate.Value, TotalDec = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)), QtyCount = s.Sum(su => Convert.ToInt32(su.NetRetailQty)), Count = s.Count(), CustomerCode = s.FirstOrDefault().ConsPartyCode, CustomerName = s.FirstOrDefault().ConsPartyName }).ToList();
            }
            else if (model.Roles.Contains(Constants.Distributor))
            {
                var distributorId = model.UserId.GetDistributorId(Constants.Distributor);
                invoices = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId
                                && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo)
                                && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).ToList()
                                .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse { CoNo = s.Key, Date = s.FirstOrDefault().CreatedDate.Value, TotalDec = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)), QtyCount = s.Sum(su => Convert.ToInt32(su.NetRetailQty)), Count = s.Count(), CustomerCode = s.FirstOrDefault().ConsPartyCode, CustomerName = s.FirstOrDefault().ConsPartyName }).ToList();
            }
            else if (model.Roles.Contains(Constants.Workshop) || model.Roles.Contains(Constants.WorkshopUsers))
            {
                var workshopId = model.UserId.GetWorkshopId(Constants.Workshop);
                invoices = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.WorkShopId != null && x.WorkShopId == workshopId
                                                                           && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo)
                                                                           && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList()
                    .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse { CoNo = s.Key, Date = s.FirstOrDefault().CreatedDate.Value, TotalDec = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)), QtyCount = s.Sum(su => Convert.ToInt32(su.NetRetailQty)), Count = s.Count(), CustomerCode = s.FirstOrDefault().ConsPartyCode, CustomerName = s.FirstOrDefault().ConsPartyName }).ToList();
            }
            else if (model.Roles.Contains(Constants.RoIncharge))
            {
                var distributorsOutlet = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == model.UserId);
                if (distributorsOutlet != null)
                {
                    var workshop = (from w in db.WorkShops
                                    where w.outletId == distributorsOutlet.OutletId
                                    select w).ToList();
                    if (workshop.Count > 0)
                    {
                        var ids = workshop.Select(a => a.WorkShopId).ToList();

                        invoices = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.WorkShopId != null && ids.Contains(x.WorkShopId ?? 0)
                                && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo)
                                && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList()
                                .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse { CoNo = s.Key, Date = s.FirstOrDefault().CreatedDate.Value, TotalDec = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)), QtyCount = s.Sum(su => Convert.ToInt32(su.NetRetailQty)), Count = s.Count(), CustomerCode = s.FirstOrDefault().ConsPartyCode, CustomerName = s.FirstOrDefault().ConsPartyName }).ToList();
                    }
                }
            }
            else if (model.Roles.Contains(Constants.SalesExecutive))
            {
                var wIds = db.SalesExecutiveWorkshops.Where(se => se.UserId == model.UserId).Select(w => w.WorkshopId).ToList();

                invoices = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.WorkShopId != null && wIds.Contains(x.WorkShopId ?? 0)
                                && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo)
                                && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList()
                                .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse { CoNo = s.Key, Date = s.FirstOrDefault().CreatedDate.Value, TotalDec = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)), QtyCount = s.Sum(su => Convert.ToInt32(su.NetRetailQty)), Count = s.Count(), CustomerCode = s.FirstOrDefault().ConsPartyCode, CustomerName = s.FirstOrDefault().ConsPartyName }).ToList();
            }
            else if (model.Roles.Contains(Constants.Users))
            {
                var distributorId = model.UserId.GetDistributorId(Constants.Users);
                invoices = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId
                                                                           && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo)
                                                                           && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).ToList()
                    .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse { CoNo = s.Key, Date = s.FirstOrDefault().CreatedDate.Value, TotalDec = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)), QtyCount = s.Sum(su => Convert.ToInt32(su.NetRetailQty)), Count = s.Count(), CustomerCode = s.FirstOrDefault().ConsPartyCode, CustomerName = s.FirstOrDefault().ConsPartyName }).ToList();
            }

            if (invoices.Count > 0)
            {
                invoices.ForEach(x => { x.Total = $"{Math.Round(x.TotalDec, 2):#,###}"; x.OrderDate = x.Date.ToString("dd MMM, yyyy"); });
                totalRecord = invoices.Count;
                if (model.PageNumber > 0)
                {
                    return invoices.GetPaging(model.PageNumber, model.PageSize);
                }
            }

            return invoices;
        }
        #endregion

        #region Invoice Detail
        public List<InvoiceDetailResponse> GetInvoiceDetail(InvoiceDetailRequest model, out int totalRecord)
        {
            totalRecord = 0;
            var invoiceDetails = new List<InvoiceDetailResponse>();
            List<DailySalesTrackerWithInvoiceData> invoiceSales = null;

            if (model.Roles.Contains(Constants.SuperAdmin))
            {
                invoiceSales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.CreatedDate != null
                                                                               && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo) && x.CoNo == model.CoNo
                                                                               && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList();
            }
            else if (model.Roles.Contains(Constants.Distributor))
            {
                var distributorId = model.UserId.GetDistributorId(Constants.Distributor);
                invoiceSales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId
                                && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo) && x.CoNo == model.CoNo
                                && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList();
            }
            else if (model.Roles.Contains(Constants.Workshop) || model.Roles.Contains(Constants.WorkshopUsers))
            {
                var workshopId = model.UserId.GetWorkshopId(Constants.Workshop);
                invoiceSales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.WorkShopId != null && x.WorkShopId == workshopId
                                                                               && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo) && x.CoNo == model.CoNo
                                                                               && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList();

            }
            else if (model.Roles.Contains(Constants.RoIncharge))
            {
                var distributorsOutlet = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == model.UserId);
                if (distributorsOutlet != null)
                {
                    var workshop = (from w in db.WorkShops
                                    where w.outletId == distributorsOutlet.OutletId
                                    select w).ToList();
                    if (workshop.Count > 0)
                    {
                        var ids = workshop.Select(a => a.WorkShopId).ToList();

                        invoiceSales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.WorkShopId != null && ids.Contains(x.WorkShopId ?? 0)
                                  && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo) && x.CoNo == model.CoNo
                                  && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList();
                    }
                }
            }
            else if (model.Roles.Contains(Constants.SalesExecutive))
            {
                var wIds = db.SalesExecutiveWorkshops.Where(se => se.UserId == model.UserId).Select(w => w.WorkshopId).ToList();

                invoiceSales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.WorkShopId != null && wIds.Contains(x.WorkShopId ?? 0)
                                 && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo) && x.CoNo == model.CoNo
                                 && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList();
            }
            else if (model.Roles.Contains(Constants.Users))
            {
                var distributorId = model.UserId.GetDistributorId(Constants.Users);
                invoiceSales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId
                                                                                                       && (string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0") && !string.IsNullOrEmpty(x.CoNo) && x.CoNo == model.CoNo
                                                                                                       && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList();
            }

            if (invoiceSales?.Count > 0)
            {
                totalRecord = invoiceSales.Count;
                invoiceDetails.AddRange(invoiceSales.Select(item => new InvoiceDetailResponse
                {
                    CoNo = item.CoNo,
                    PartNumber = item.PartNum,
                    Date = item.CreatedDate != null ? item.CreatedDate.Value.ToString("dd MMM, yyyy") : "",
                    Price = Math.Round(Convert.ToDecimal(item.NetRetailSelling), 2),
                    Qty = item.NetRetailQty != null ? Convert.ToInt32(item.NetRetailQty) : 0,
                    OutletName = item.LocDesc != null ? item.LocDesc : ""
                }));

                if (model.PageNumber > 0)
                {
                    return invoiceDetails.GetPaging(model.PageNumber, model.PageSize);
                }
            }
            return invoiceDetails;
        }
        #endregion

        #region Reject Customer Back Order
        public bool RejectCustomerBackOrder(string coNumber, string partNumber, string userId, string role, List<ApplicationUser> superAdmins, out string message)
        {
            bool status = false;

            if (!string.IsNullOrEmpty(partNumber))
            {
                var cbOrder = db.CustomerBackOrders.Where(c => c.CONo == coNumber && c.PartNum == partNumber).FirstOrDefault();
                if (cbOrder != null)
                {
                    cbOrder.PartStatus = "Cancel";
                    status = db.SaveChanges() > 0;
                    message = status ? "Customer back orders were rejected successfully!" : "Failed to reject customer back orders.";
                }
                else
                {
                    //throw new Exception($"Customer back order with order number {coNumber} and part number {partNumber} not found.");
                    message = $"Customer back order with order number {coNumber} and part number {partNumber} not found.";
                }
            }
            else
            {
                var cbOrders = db.CustomerBackOrders.Where(c => c.CONo == coNumber && c.PartStatus != "Accept" && c.PartStatus != "Cancel").ToList();

                cbOrders.ForEach(c => c.PartStatus = "Cancel");
                status = db.SaveChanges() > 0;
                message = status ? "Customer back orders were rejected successfully!" : "Failed to reject customer back orders.";
            }
            if (status)
            {
                var t = new System.Threading.Thread(() => SaveRejectOrderNotification(coNumber, partNumber, userId, role, superAdmins, Constants.RejectBackOrder))
                { IsBackground = true };
                t.Start();
            }
            return status;
        }
        #endregion

        #region Accept Customer Back Order
        public bool AcceptCustomerBackOrder(string coNumber, string partNumber, string userId, string role, List<ApplicationUser> superAdmins, out string message)
        {
            bool status = false;
            if (!string.IsNullOrEmpty(partNumber))
            {
                var cbOrder = db.CustomerBackOrders.Where(c => c.CONo == coNumber && c.PartNum == partNumber).FirstOrDefault();
                if (cbOrder != null)
                {
                    cbOrder.PartStatus = "Accept";
                    status = db.SaveChanges() > 0;
                    message = status ? "Customer back orders were accepted successfully!" : "Failed to accept customer back orders.";
                }
                else
                {
                    message = $"Customer back order with order number {coNumber} and part number {partNumber} not found.";
                    //throw new Exception($"Customer back order with order number {coNumber} and part number {partNumber} not found.");
                }
            }
            else
            {
                var cbOrders = db.CustomerBackOrders.Where(c => c.CONo == coNumber && c.PartStatus != "Accept" && c.PartStatus != "Cancel").ToList();
                cbOrders.ForEach(c => c.PartStatus = "Accept");
                status = db.SaveChanges() > 0;
                message = status ? "Customer back orders were accepted successfully!" : "Failed to accept customer back orders.";
            }
            if (status)
            {
                var t = new System.Threading.Thread(() => SaveAcceptOrderNotification(coNumber, partNumber, userId, role, superAdmins, Constants.AcceptBackOrder))
                { IsBackground = true };
                t.Start();
            }
            return status;
        }
        #endregion

        #region Save Notification On Accept Order
        public bool SaveAcceptOrderNotification(string coNumber, string partNumber, string userId, string role, List<ApplicationUser> superAdmins, string OrderType)
        {
            try
            {


                //var mail = db.MailTemplates.Where(x => x.Type == Constants.AcceptBackOrder).FirstOrDefault();
                var backorderData = db.CustomerBackOrders.Where(x => x.PartNum == partNumber).ToList();
                // Msg For Order Type
                string msg = $"Customer order with order Id {coNumber} accepted.";
                if (OrderType == Constants.AcceptBackOrder)
                {
                    msg = $"Order Accepted. Order Number : {coNumber} and part number : {partNumber}";
                    if (string.IsNullOrEmpty(partNumber))
                    {
                        msg = $"Order Accepted. Order Number : {coNumber}";
                        backorderData = db.CustomerBackOrders.Where(x => x.CONo == coNumber).ToList();
                    }
                    //mail = db.MailTemplates.Where(x => x.Type == Constants.AcceptBackOrder).FirstOrDefault();
                }
                clsEmailMgt clsEmailMgt = new clsEmailMgt();
                int? workshopId = null, distributorId = null;
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
                {
                    distributorId = userId.GetDistributorId(role);
                    workshopId = db.DistributorWorkShops.Where(x => x.DistributorId == distributorId && x.UserId == userId).Select(s => s.WorkShopId).FirstOrDefault();
                    // Save Notifications for User
                    if (role != Constants.RoIncharge && role != Constants.Distributor && role != Constants.SuperAdmin)
                    {
                        var user = db.UserDetails.Where(u => u.UserId == userId).FirstOrDefault();
                        if (user != null)
                        {
                            var notification = new Notification()
                            {
                                UserId = userId.ToString(),
                                Type = OrderType,
                                Message = msg,
                                IsRead = false,
                                CreatedDate = DateTime.Now,
                                RefUserId = partNumber.ToString(),
                                WorkshopId = workshopId,
                            };
                            db.Notifications.Add(notification);
                            db.SaveChanges();

                            //if (mail != null)
                            //{
                            //    if (OrderType == Constants.AcceptBackOrder)
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailBackorder(mail.MailHeading, mail.MailHtml, $"{user.FirstName + " " + user.LastName}", user.AspNetUser.Email, backorderData, Constants.RupeeSign, true);
                            //    }
                            //}
                        }
                    }
                    var roUserDetail = (from w in db.WorkShops
                                        join d in db.DistributorsOutlets on w.outletId equals d.OutletId
                                        join u in db.UserDetails on d.UserId equals u.UserId
                                        where w.WorkShopId == workshopId && d.DistributorId == distributorId && w.outletId != null && w.outletId != 0
                                        select u
                                 ).ToList();

                    // Save Notifications for Ro Incharge
                    if (roUserDetail.Any())
                    {
                        foreach (var roUser in roUserDetail)
                        {
                            var notification = new Notification()
                            {
                                UserId = roUser.UserId.ToString(),
                                Type = OrderType,
                                Message = msg,
                                IsRead = false,
                                CreatedDate = DateTime.Now,
                                RefUserId = partNumber,//userId,
                                WorkshopId = workshopId,
                            };
                            db.Notifications.Add(notification);
                            db.SaveChanges();

                            //if (mail != null)
                            //{
                            //    if (OrderType == Constants.AcceptBackOrder)
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailBackorder(mail.MailHeading, mail.MailHtml, $"{roUser.FirstName + " " + roUser.LastName}", roUser.AspNetUser.Email, backorderData, Constants.RupeeSign, true);
                            //    }
                            //}
                        }
                    }
                    // Save Notifications for Distributors
                    var DistributorUserId = db.DistributorUsers.Where(x => x.DistributorId == distributorId).Select(u => u.UserId).FirstOrDefault();
                    if (DistributorUserId.Any())
                    {
                        var notification = new Notification()
                        {
                            UserId = DistributorUserId.ToString(),
                            Type = OrderType,
                            Message = msg,
                            IsRead = false,
                            CreatedDate = DateTime.Now,
                            RefUserId = partNumber,//userId,
                            WorkshopId = workshopId
                        };
                        db.Notifications.Add(notification);
                        db.SaveChanges();

                        //if (mail != null)
                        //{
                        //    var distUser = db.UserDetails.Where(x => x.UserId == DistributorUserId).FirstOrDefault();
                        //    if (OrderType == Constants.AcceptBackOrder)
                        //    {
                        //        var resultNew = clsEmailMgt.SendMailBackorder(mail.MailHeading, mail.MailHtml, $"{distUser.FirstName + " " + distUser.LastName}", distUser.AspNetUser.Email, backorderData, Constants.RupeeSign, true);
                        //    }
                        //}

                    }
                    // Save Notifications for Super Admin
                    if (superAdmins.Any())
                    {
                        foreach (var sa in superAdmins)
                        {
                            var notification = new Notification()
                            {
                                UserId = sa.Id.ToString(),
                                Type = OrderType,
                                Message = msg,
                                IsRead = false,
                                CreatedDate = DateTime.Now,
                                RefUserId = partNumber,//userId,
                                WorkshopId = workshopId,
                            };
                            db.Notifications.Add(notification);
                            db.SaveChanges();

                            //if (mail != null)
                            //{
                            //    var superAdmin = db.UserDetails.Where(x => x.UserId == sa.Id).FirstOrDefault();
                            //    if (OrderType == Constants.AcceptBackOrder)
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailBackorder(mail.MailHeading, mail.MailHtml, $"{superAdmin.FirstName + " " + superAdmin.LastName}", superAdmin.AspNetUser.Email, backorderData, Constants.RupeeSign, true);
                            //    }
                            //}
                        }
                    }
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                RepoUserLogs.SendExceptionMail("Exception From SaveAcceptOrderNotification", ex.Message, ex.StackTrace);
                return false;
            }

        }
        #endregion

        #region Save Notification On Reject Order
        public bool SaveRejectOrderNotification(string coNumber, string partNumber, string userId, string role, List<ApplicationUser> superAdmins, string OrderType)
        {
            try
            {

                //var mail = db.MailTemplates.Where(x => x.Type == Constants.OrderCancel).FirstOrDefault();
                var orderData = db.OrderTables.Where(x => x.OrderNo == coNumber).FirstOrDefault();
                var backorderData = db.CustomerBackOrders.Where(x => x.PartNum == partNumber).ToList();
                // Msg For Order Type
                string msg = $"Customer order with order Id {coNumber} rejected.";
                if (OrderType == Constants.RejectBackOrder)
                {
                    msg = $"Order Rejected. Order Number : {coNumber} and part number : {partNumber}";
                    if (string.IsNullOrEmpty(partNumber))
                    {
                        msg = $"Order Rejected. Order Number : {coNumber}";
                        backorderData = db.CustomerBackOrders.Where(x => x.CONo == coNumber).ToList();
                    }
                    //mail = db.MailTemplates.Where(x => x.Type == Constants.BackOrderCancel).FirstOrDefault();
                }


                clsEmailMgt clsEmailMgt = new clsEmailMgt();



                int? workshopId = null, distributorId = null;
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
                {
                    distributorId = userId.GetDistributorId(role);
                    workshopId = db.DistributorWorkShops.Where(x => x.DistributorId == distributorId && x.UserId == userId).Select(s => s.WorkShopId).FirstOrDefault();


                    // Save Notifications for User
                    if (role != Constants.RoIncharge && role != Constants.Distributor && role != Constants.SuperAdmin)
                    {
                        var user = db.UserDetails.Where(u => u.UserId == userId).FirstOrDefault();
                        if (user != null)
                        {
                            var notification = new Notification()
                            {
                                UserId = userId.ToString(),
                                Type = OrderType,
                                Message = msg,
                                IsRead = false,
                                CreatedDate = DateTime.Now,
                                RefUserId = partNumber.ToString(),
                                WorkshopId = workshopId,
                            };
                            db.Notifications.Add(notification);
                            db.SaveChanges();

                            //if (mail != null)
                            //{
                            //    if (OrderType == Constants.RejectMainOrder)
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{user.FirstName + " " + user.LastName}", user.AspNetUser.Email, user.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                            //    }
                            //    else
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailrajectBackorder(mail.MailHeading, mail.MailHtml, $"{user.FirstName + " " + user.LastName}", user.AspNetUser.Email, backorderData, Constants.RupeeSign, true);
                            //    }
                            //}
                        }
                    }
                    var roUserDetail = (from w in db.WorkShops
                                        join d in db.DistributorsOutlets on w.outletId equals d.OutletId
                                        join u in db.UserDetails on d.UserId equals u.UserId
                                        where w.WorkShopId == workshopId && d.DistributorId == distributorId && w.outletId != null && w.outletId != 0
                                        select u
                                 ).ToList();

                    // Save Notifications for Ro Incharge
                    if (roUserDetail.Any())
                    {
                        foreach (var roUser in roUserDetail)
                        {
                            var notification = new Notification()
                            {
                                UserId = roUser.UserId.ToString(),
                                Type = OrderType,
                                Message = msg,
                                IsRead = false,
                                CreatedDate = DateTime.Now,
                                RefUserId = partNumber,//userId,
                                WorkshopId = workshopId,
                            };
                            db.Notifications.Add(notification);
                            db.SaveChanges();

                            //if (mail != null)
                            //{
                            //    if (OrderType == Constants.RejectMainOrder)
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{roUser.FirstName + " " + roUser.LastName}", roUser.AspNetUser.Email, roUser.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                            //    }
                            //    else
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailrajectBackorder(mail.MailHeading, mail.MailHtml, $"{roUser.FirstName + " " + roUser.LastName}", roUser.AspNetUser.Email, backorderData, Constants.RupeeSign, true);
                            //    }
                            //}
                        }
                    }
                    // Save Notifications for Distributors
                    var DistributorUserId = db.DistributorUsers.Where(x => x.DistributorId == distributorId).Select(u => u.UserId).FirstOrDefault();
                    if (DistributorUserId.Any())
                    {
                        var notification = new Notification()
                        {
                            UserId = DistributorUserId.ToString(),
                            Type = OrderType,
                            Message = msg,
                            IsRead = false,
                            CreatedDate = DateTime.Now,
                            RefUserId = partNumber,//userId,
                            WorkshopId = workshopId
                        };
                        db.Notifications.Add(notification);
                        db.SaveChanges();

                        //if (mail != null)
                        //{
                        //    var distUser = db.UserDetails.Where(x => x.UserId == DistributorUserId).FirstOrDefault();
                        //    if (OrderType == Constants.RejectMainOrder)
                        //    {
                        //        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{distUser.FirstName + " " + distUser.LastName}", distUser.AspNetUser.Email, distUser.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                        //    }
                        //    else
                        //    {
                        //        var resultNew = clsEmailMgt.SendMailrajectBackorder(mail.MailHeading, mail.MailHtml, $"{distUser.FirstName + " " + distUser.LastName}", distUser.AspNetUser.Email, backorderData, Constants.RupeeSign, true);
                        //    }
                        //}

                    }
                    // Save Notifications for Super Admin
                    if (superAdmins.Any())
                    {
                        foreach (var sa in superAdmins)
                        {
                            var notification = new Notification()
                            {
                                UserId = sa.Id.ToString(),
                                Type = OrderType,
                                Message = msg,
                                IsRead = false,
                                CreatedDate = DateTime.Now,
                                RefUserId = partNumber,//userId,
                                WorkshopId = workshopId,
                            };
                            db.Notifications.Add(notification);
                            db.SaveChanges();

                            //if (mail != null)
                            //{
                            //    var superAdmin = db.UserDetails.Where(x => x.UserId == sa.Id).FirstOrDefault();
                            //    if (OrderType == Constants.RejectMainOrder)
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{superAdmin.FirstName + " " + superAdmin.LastName}", superAdmin.AspNetUser.Email, superAdmin.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                            //    }
                            //    else
                            //    {
                            //        var resultNew = clsEmailMgt.SendMailrajectBackorder(mail.MailHeading, mail.MailHtml, $"{superAdmin.FirstName + " " + superAdmin.LastName}", superAdmin.AspNetUser.Email, backorderData, Constants.RupeeSign, true);
                            //    }
                            //}
                        }
                    }
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                RepoUserLogs.SendExceptionMail("Exception From SaveRejectOrderNotification", ex.Message, ex.StackTrace);
                return false;
            }
        }
        #endregion

        #region Invoice
        //public List<InvoiceResponse> GetAccountLedger(clsAccountLedger model, out int totalRecord)
        //{
        //    totalRecord = 0;
        //    List<InvoiceResponse> lstInvoice = new List<InvoiceResponse>();
        //    if (model.Role.Equals(Constants.Distributor, StringComparison.OrdinalIgnoreCase) || model.Role.Equals(Constants.Users, StringComparison.OrdinalIgnoreCase))
        //    {
        //        int distributorId = model.UserId.GetDistributorId(model.Role);
        //        lstInvoice = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.DistributorId != null && x.DistributorId == distributorId
        //                        && string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0" && !string.IsNullOrEmpty(x.CoNo)
        //                        && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).ToList()
        //                        .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse() { CoNo = s.Key, Total = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)) }).ToList();
        //    }
        //    else if (model.Role.Equals(Constants.Workshop, StringComparison.OrdinalIgnoreCase) || model.Role.Equals(Constants.WorkshopUsers, StringComparison.OrdinalIgnoreCase))
        //    {
        //        int workshopId = model.UserId.GetWorkshopId(model.Role);
        //        lstInvoice = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.WorkShopId != null && x.WorkShopId == workshopId
        //                        && string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0" && !string.IsNullOrEmpty(x.CoNo)
        //                        && x.CreatedDate != null && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList()
        //                        .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse() { CoNo = s.Key, Total = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)) }).ToList();
        //    }
        //    else if (model.Role.Equals(Constants.SuperAdmin, StringComparison.OrdinalIgnoreCase))
        //    {
        //        lstInvoice = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.ProductId != null && x.CreatedDate != null
        //                        && string.IsNullOrEmpty(x.ReturnQty) || x.ReturnQty == "0" && !string.IsNullOrEmpty(x.CoNo)
        //                        && x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate).OrderBy(o => o.DailySalesTrackerId).ToList()
        //                        .GroupBy(g => g.CoNo).Select(s => new InvoiceResponse() { CoNo = s.Key, Total = s.Sum(su => Convert.ToDecimal(su.NetRetailSelling)) }).ToList();

        //    }

        //    if (lstInvoice.Any())
        //    {
        //        lstInvoice.ForEach(x => x.Total = Math.Round(x.Total, 2));
        //        totalRecord = lstInvoice.Count();
        //        if (model.PageNumber > 0)
        //        {
        //            return lstInvoice.GetPaging<InvoiceResponse>(model.PageNumber, model.PageSize);
        //        }
        //    }
        //    return lstInvoice;
        //}
        #endregion

        #region Save Customer new Order notification
        public bool SaveOrderNotification(int OrderId, string userId, string role, List<ApplicationUser> superAdmins, string OrderNo)
        {
            string OrderType = Constants.NewOrderPlaced;
            int? workshopId = null, distributorId = null;
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
            {
                distributorId = userId.GetDistributorId(role);
                workshopId = db.DistributorWorkShops.Where(x => x.DistributorId == distributorId && x.UserId == userId).Select(s => s.WorkShopId).FirstOrDefault();
            }
            var roUserDetail = (from w in db.WorkShops
                                join d in db.DistributorsOutlets on w.outletId equals d.OutletId
                                join u in db.UserDetails on d.UserId equals u.UserId
                                where w.WorkShopId == workshopId && d.DistributorId == distributorId && w.outletId != null && w.outletId != 0
                                select u
                           ).ToList();
            // Msg For Order Type
            string msg = $"New Order Placed. Order Number : {OrderNo}";

            var mail = db.MailTemplates.Where(x => x.Type == Constants.PlaceOrder).FirstOrDefault();
            clsEmailMgt clsEmailMgt = new clsEmailMgt();
            var orderData = db.OrderTables.Where(x => x.OrderID == OrderId).FirstOrDefault();
            if (role != Constants.RoIncharge && role != Constants.Distributor && role != Constants.SuperAdmin)
            {
                // Send Mail for user
                var user = db.UserDetails.Where(u => u.UserId == userId).FirstOrDefault();
                var notification = new Notification()
                {
                    UserId = userId.ToString(),
                    Type = OrderType,
                    Message = msg,
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    RefUserId = OrderId.ToString(),//userId,
                    WorkshopId = workshopId,
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                if (mail != null)
                {
                    var sendmail = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{user.FirstName + " " + user.LastName}", user.AspNetUser.Email, user.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                }
            }
            // Save Notifications for Ro Incharge
            if (roUserDetail.Any())
            {
                foreach (var roUser in roUserDetail)
                {
                    var notification = new Notification()
                    {
                        UserId = roUser.UserId.ToString(),
                        Type = OrderType,
                        Message = msg,
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        RefUserId = OrderId.ToString(),//userId,
                        WorkshopId = workshopId,
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();
                    if (mail != null)
                    {
                        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{roUser.FirstName + " " + roUser.LastName}", roUser.AspNetUser.Email, roUser.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                    }
                }
            }
            // Save Notifications for Distributors
            var DistributorUserId = db.DistributorUsers.Where(x => x.DistributorId == distributorId).Select(u => u.UserId).FirstOrDefault();
            if (DistributorUserId != null)
            {
                var notification = new Notification()
                {
                    UserId = DistributorUserId.ToString(),
                    Type = OrderType,
                    Message = msg,
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    RefUserId = OrderId.ToString(),//userId,
                    WorkshopId = workshopId
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                var distUser = db.UserDetails.Where(x => x.UserId == DistributorUserId).FirstOrDefault();
                if (mail != null)
                {
                    var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{distUser.FirstName + " " + distUser.LastName}", distUser.AspNetUser.Email, distUser.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                }
            }
            // Save Notifications for Super Admin
            if (superAdmins.Any())
            {
                foreach (var sa in superAdmins)
                {
                    var notification = new Notification()
                    {
                        UserId = sa.Id.ToString(),
                        Type = OrderType,
                        Message = msg,
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        RefUserId = OrderId.ToString(),//userId,
                        WorkshopId = workshopId,
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();

                    var superAdmin = db.UserDetails.Where(x => x.UserId == sa.Id).FirstOrDefault();
                    if (mail != null)
                    {
                        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{superAdmin.FirstName + " " + superAdmin.LastName}", superAdmin.AspNetUser.Email, superAdmin.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                    }
                }
            }
            return true;
        }
        #endregion

        #region Get Outstanding Detail
        public OutstandingResponse GetOutstandingDetail(string userId, string role)
        {
            DateTime? closingDate = null;
            decimal creditLimit = 0, totalOs = 0, criticalOs = 0, cosLessThan = 0, cosGreaterThan = 0;
            int criticalOsDays = 0, cosGreaterThanDays = 0;

            decimal outstanding0To7Days = 0, outstanding7To14Days = 0, outstanding14To21Days = 0, outstanding21To28Days = 0,
                outstanding28To35Days = 0, outstanding35To50Days = 0, outstanding50To70Days = 0, outstandingAbove70Days = 0;

            var workshops = new List<WorkShop>();

            if (role == Constants.Workshop || role == Constants.WorkshopUsers)
            {
                var workshopId = userId.GetWorkshopId(role);
                var workshop = db.WorkShops.AsNoTracking().FirstOrDefault(x => x.WorkShopId == workshopId);

                if (workshop != null)
                {
                    criticalOs = workshop.OutstandingAmount ?? criticalOs;
                    totalOs = workshop.TotalOutstanding ?? totalOs;
                    creditLimit = workshop.CreditLimit ?? creditLimit;

                    closingDate = db.AccountLedgers.Where(a => a.Particulars == CloseBal && a.WorkshopId.HasValue && a.WorkshopId == workshop.WorkShopId).OrderByDescending(o => o.Date).AsNoTracking().Select(a => a.Date).FirstOrDefault();

                    var outstanding = db.Outstandings.Where(x => x.WorkshopId == workshop.WorkShopId).OrderByDescending(o => o.CreatedDate).Take(1).AsNoTracking().FirstOrDefault();
                    if (outstanding != null)
                    {
                        criticalOsDays = workshop.CriticalOutstandingDays ?? 0;
                        cosLessThan = outstanding.LessThanCriticalDay;
                        cosGreaterThan = outstanding.MoreThanCriticalDay;

                        outstanding0To7Days = !string.IsNullOrEmpty(outstanding.LessThan7Days) ? Convert.ToDecimal(outstanding.LessThan7Days) : 0;
                        outstanding7To14Days = !string.IsNullOrEmpty(outstanding.C7To14Days) ? Convert.ToDecimal(outstanding.C7To14Days) : 0;
                        outstanding14To21Days = !string.IsNullOrEmpty(outstanding.C14To21Days) ? Convert.ToDecimal(outstanding.C14To21Days) : 0;
                        outstanding21To28Days = !string.IsNullOrEmpty(outstanding.C21To28Days) ? Convert.ToDecimal(outstanding.C21To28Days) : 0;
                        outstanding28To35Days = !string.IsNullOrEmpty(outstanding.C28To35Days) ? Convert.ToDecimal(outstanding.C28To35Days) : 0;
                        outstanding35To50Days = !string.IsNullOrEmpty(outstanding.C35To50Days) ? Convert.ToDecimal(outstanding.C35To50Days) : 0;
                        outstanding50To70Days = !string.IsNullOrEmpty(outstanding.C50To70Days) ? Convert.ToDecimal(outstanding.C50To70Days) : 0;
                        outstandingAbove70Days = !string.IsNullOrEmpty(outstanding.MoreThan70Days) ? Convert.ToDecimal(outstanding.MoreThan70Days) : 0;
                    }
                }
            }

            else if (role == Constants.SalesExecutive)
            {

                var idsAll = db.SalesExecutiveWorkshops.Where(w => w.UserId == userId).Select(w => w.WorkshopId).ToList();
                workshops = (from a in db.UserDetails
                                 join w in db.DistributorWorkShops on a.UserId equals w.UserId
                                 join ww in db.WorkShops on w.WorkShopId equals ww.WorkShopId
                                 where a.IsDeleted.Value == false && idsAll.Contains(w.WorkShopId.Value)
                                 select ww).ToList();
            }
            else if (role == Constants.RoIncharge)
            {
                var distributorsOutlet = db.DistributorsOutlets.AsNoTracking().FirstOrDefault(a => a.UserId == userId);
                if (distributorsOutlet != null)
                {
                    workshops = (from w in db.WorkShops.AsNoTracking()
                                 where w.outletId == distributorsOutlet.OutletId
                                 select w).ToList();
                }
            }
            else if (role == Constants.SuperAdmin)
            {
                workshops = db.WorkShops.AsNoTracking().ToList();

            }
            else if (role == Constants.Distributor || role == Constants.Users)
            {
                var distributorId = userId.GetDistributorId(role);
                workshops = (from w in db.WorkShops.AsNoTracking()
                             join d in db.DistributorWorkShops.AsNoTracking() on w.WorkShopId equals d.WorkShopId
                             where d.DistributorId == distributorId
                             select w).ToList();
            }

            // Set outstanding values
            if (workshops.Count > 0)
            {
                criticalOs = workshops.Where(w => w.OutstandingAmount != null).ToList().Sum(s => Convert.ToDecimal(s.OutstandingAmount));
                totalOs = workshops.Where(w => w.TotalOutstanding != null).ToList().Sum(s => Convert.ToDecimal(s.TotalOutstanding));
                creditLimit = workshops.Where(w => w.CreditLimit != null).ToList().Sum(s => Convert.ToDecimal(s.CreditLimit));

                var wsIds = workshops.Select(w => w.WorkShopId).ToList();

                closingDate = db.AccountLedgers.Where(a => a.Particulars == CloseBal && a.WorkshopId.HasValue && wsIds.Contains(a.WorkshopId.Value)).OrderByDescending(o => o.Date).AsNoTracking().Select(a => a.Date).FirstOrDefault();

                var outstandings = db.Outstandings.Where(x => wsIds.Contains(x.WorkshopId ?? 0)).OrderByDescending(o => o.CreatedDate).AsNoTracking().ToList();
                if (outstandings.Count > 0)
                {
                    outstanding0To7Days = outstandings.Where(o => !string.IsNullOrEmpty(o.LessThan7Days)).Sum(o => Convert.ToDecimal(o.LessThan7Days));
                    outstanding7To14Days = outstandings.Where(o => !string.IsNullOrEmpty(o.C7To14Days)).Sum(o => Convert.ToDecimal(o.C7To14Days));
                    outstanding14To21Days = outstandings.Where(o => !string.IsNullOrEmpty(o.C14To21Days)).Sum(o => Convert.ToDecimal(o.C14To21Days));
                    outstanding21To28Days = outstandings.Where(o => !string.IsNullOrEmpty(o.C21To28Days)).Sum(o => Convert.ToDecimal(o.C21To28Days));
                    outstanding28To35Days = outstandings.Where(o => !string.IsNullOrEmpty(o.C28To35Days)).Sum(o => Convert.ToDecimal(o.C28To35Days));
                    outstanding35To50Days = outstandings.Where(o => !string.IsNullOrEmpty(o.C35To50Days)).Sum(o => Convert.ToDecimal(o.C35To50Days));
                    outstanding50To70Days = outstandings.Where(o => !string.IsNullOrEmpty(o.C50To70Days)).Sum(o => Convert.ToDecimal(o.C50To70Days));
                    outstandingAbove70Days = outstandings.Where(o => !string.IsNullOrEmpty(o.MoreThan70Days)).Sum(o => Convert.ToDecimal(o.MoreThan70Days));
                }
            }

            return new OutstandingResponse
            {
                UPID = "",// upiid,
                Heading = Constants.DashboardHeading,
                Description = Constants.DashboardDescription,
                CreditLimit = string.Format("{0:#,###}", creditLimit),
                TotalOutstanding = string.Format("{0:#,###}", totalOs),
                CriticalOutstanding = string.Format("{0:#,###}", criticalOs),
                CriticalOsLessThan = string.Format("{0:#,###}", cosLessThan),
                CriticalOsLessThanDays = criticalOsDays,
                CriticalOsGraterThan = string.Format("{0:#,###}", cosGreaterThan),
                CriticalOsGraterThanDays = cosGreaterThanDays,
                ClosingDate = closingDate != null ? closingDate.Value.ToString("dd/MMM/yyyy") : "",
                Outstanding0To7Days = string.Format("{0:#,###}", outstanding0To7Days),
                Outstanding7To14Days = string.Format("{0:#,###}", outstanding7To14Days),
                Outstanding14To21Days = string.Format("{0:#,###}", outstanding14To21Days),
                Outstanding21To28Days = string.Format("{0:#,###}", outstanding21To28Days),
                Outstanding28To35Days = string.Format("{0:#,###}", outstanding28To35Days),
                Outstanding35To50Days = string.Format("{0:#,###}", outstanding35To50Days),
                Outstanding50To70Days = string.Format("{0:#,###}", outstanding50To70Days),
                OutstandingAbove70Days = string.Format("{0:#,###}", outstandingAbove70Days)
            };
        }

        #endregion

        #region GetCartItemCount
        public int GetCartItemCount(int tempOrderId)
        {
            var data = db.TempOrderDetails.Where(x => x.TempOrderId == tempOrderId).ToList().Select(s => s.Qty != null ? s.Qty.Value : 0).Sum();
            return data;
        }
        #endregion

        #region Get total sales growth
        public List<SaleGrowth> GetTotalSalesGrowth(FilterModel model, ref string errorMsg)
        {
            var sales = new List<SaleGrowth>();
            if (model.IsFromMobile)
            {
                sales.Add(new SaleGrowth
                {
                    FrequencyCol = model.Frequency.ToString(),
                    Sale = "Current",
                    PrevSale = "Previous",
                    Growth = "Growth"
                });
            }

            var runCode = true;

            if (model.Frequency == Frequency.Yearly && model.Growth == Growth.LastYearly)
            {
                int days = Convert.ToInt32((model.EndDate - model.StartDate).TotalDays);
                if (days > 365)
                {
                    runCode = false;
                    DateTime endDate = new DateTime(model.StartDate.Year, model.EndDate.Month, model.EndDate.Day);
                    DateTime startDate = model.StartDate.AddYears(-1);
                    int years = Convert.ToInt32(days / 365);
                    int rem = days % 365;
                    if (rem > 0) { years = years + 1; }

                    for (int i = 1; i <= years; i++)
                    {
                        DateTime nextEndDate = endDate.AddYears(i);
                        DateTime nextStartDate = startDate.AddYears(i);


                        // call sp
                        SqlParameter[] sqlParams = {
                                       new SqlParameter("@StartDate",nextStartDate),
                                       new SqlParameter("@EndDate",nextEndDate),
                                       new SqlParameter("@GroupId",model.GroupId),
                                       new SqlParameter("@Frequency",model.Frequency.ToString()),
                                       new SqlParameter("@Growth",model.Growth.ToString()),
                                       new SqlParameter("@Role",model.Role),
                                       new SqlParameter("@UserId",model.UserId)
                                   };
                        DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_SalesAndGrowth", sqlParams);

                        // Handle not allowed growth for frequency
                        bool allowedGrowth = true;
                        switch (model.Frequency)
                        {
                            case Frequency.Weekly:
                                allowedGrowth = model.Growth != Growth.LastDay;
                                break;

                            case Frequency.Monthly:
                                allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek;
                                break;

                            case Frequency.Quarterly:
                                allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth;
                                break;

                            case Frequency.Yearly:
                                allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth && model.Growth != Growth.LastQuarter;
                                break;
                        }
                        if (!allowedGrowth)
                        {
                            errorMsg = $"Growth {model.Growth} isn't allowed for frequency {model.Frequency}.";
                            return null;
                        }

                        if (ds.Tables.Count == 0) return null;

                        var table = ds.Tables[0];
                        if (table.Rows.Count == 0) return null;



                        foreach (DataRow row in table.Rows)
                        {
                            var sg = new SaleGrowth();

                            switch (model.Frequency)
                            {
                                case Frequency.Daily:
                                    var day = row["Day"] != DBNull.Value ? Convert.ToString(row["Day"]) : string.Empty;
                                    var month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                                    var year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    sg.FrequencyCol = $"{day}-{month}-{year}";
                                    break;

                                case Frequency.Weekly:
                                    var weekNum = row["Week"] != DBNull.Value ? Convert.ToString(row["Week"]) : string.Empty;
                                    month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                                    year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    sg.FrequencyCol = $"Week-{weekNum} ({month}-{year})";
                                    break;

                                case Frequency.Monthly:
                                    month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                                    year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    sg.FrequencyCol = $"{month}({year})";
                                    break;

                                case Frequency.Quarterly:
                                    var quarter = row["Quarter"] != DBNull.Value ? Convert.ToString(row["Quarter"]) : string.Empty;
                                    year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    sg.FrequencyCol = $"Q{quarter} ({year})";
                                    break;

                                case Frequency.Yearly:
                                    sg.FrequencyCol = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    break;
                            }

                            sg.Sale = row["CurrentSale"] != DBNull.Value ? model.IsFromMobile ? $"{Constants.Sign} {row["CurrentSale"]:#,###}" :
                                $"{row["CurrentSale"]:#,###}" : "0";
                            sg.PrevSale = row["PreviousSale"] != DBNull.Value ? model.IsFromMobile ? $"{Constants.Sign} {row["PreviousSale"]:#,###}" :
                                $"{row["PreviousSale"]:#,###}" : "0";

                            sg.Growth = row["Growth"] != DBNull.Value ? Convert.ToString(row["Growth"]) : string.Empty;
                            sales.Add(sg);
                        }

                    }
                }
            }


            if (runCode)
            {
                SqlParameter[] sqlParams = {
                                           new SqlParameter("@StartDate",model.StartDate),
                                           new SqlParameter("@EndDate",model.EndDate),
                                           new SqlParameter("@GroupId",model.GroupId),
                                           new SqlParameter("@Frequency",model.Frequency.ToString()),
                                           new SqlParameter("@Growth",model.Growth.ToString()),
                                           new SqlParameter("@Role",model.Role),
                                           new SqlParameter("@UserId",model.UserId)
                                       };

                DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_SalesAndGrowth", sqlParams);

                // Handle not allowed growth for frequency
                bool allowedGrowth = true;
                switch (model.Frequency)
                {
                    case Frequency.Weekly:
                        allowedGrowth = model.Growth != Growth.LastDay;
                        break;

                    case Frequency.Monthly:
                        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek;
                        break;

                    case Frequency.Quarterly:
                        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth;
                        break;

                    case Frequency.Yearly:
                        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth && model.Growth != Growth.LastQuarter;
                        break;
                }
                if (!allowedGrowth)
                {
                    errorMsg = $"Growth {model.Growth} isn't allowed for frequency {model.Frequency}.";
                    return null;
                }

                if (ds.Tables.Count == 0) return null;

                var table = ds.Tables[0];
                if (table.Rows.Count == 0) return null;



                foreach (DataRow row in table.Rows)
                {
                    var sg = new SaleGrowth();

                    switch (model.Frequency)
                    {
                        case Frequency.Daily:
                            var day = row["Day"] != DBNull.Value ? Convert.ToString(row["Day"]) : string.Empty;
                            var month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                            var year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            sg.FrequencyCol = $"{day}-{month}-{year}";
                            break;

                        case Frequency.Weekly:
                            var weekNum = row["Week"] != DBNull.Value ? Convert.ToString(row["Week"]) : string.Empty;
                            month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            sg.FrequencyCol = $"Week-{weekNum} ({month}-{year})";
                            break;

                        case Frequency.Monthly:
                            month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            sg.FrequencyCol = $"{month}({year})";
                            break;

                        case Frequency.Quarterly:
                            var quarter = row["Quarter"] != DBNull.Value ? Convert.ToString(row["Quarter"]) : string.Empty;
                            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            sg.FrequencyCol = $"Q{quarter} ({year})";
                            break;

                        case Frequency.Yearly:
                            sg.FrequencyCol = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            break;
                    }

                    sg.Sale = row["CurrentSale"] != DBNull.Value ? model.IsFromMobile ? $"{Constants.Sign} {row["CurrentSale"]:#,###}" :
                        $"{row["CurrentSale"]:#,###}" : "0";
                    sg.PrevSale = row["PreviousSale"] != DBNull.Value ? model.IsFromMobile ? $"{Constants.Sign} {row["PreviousSale"]:#,###}" :
                        $"{row["PreviousSale"]:#,###}" : "0";

                    sg.Growth = row["Growth"] != DBNull.Value ? Convert.ToString(row["Growth"]) : string.Empty;
                    sales.Add(sg);
                }
            }

            #region Comment old Logic

            //SqlParameter[] sqlParams = {
            //                               new SqlParameter("@StartDate",model.StartDate),
            //                               new SqlParameter("@EndDate",model.EndDate),
            //                               new SqlParameter("@GroupId",model.GroupId),
            //                               new SqlParameter("@Frequency",model.Frequency.ToString()),
            //                               new SqlParameter("@Growth",model.Growth.ToString()),
            //                               new SqlParameter("@Role",model.Role),
            //                               new SqlParameter("@UserId",model.UserId)
            //                           };

            //DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_SalesAndGrowth", sqlParams);

            //// Handle not allowed growth for frequency
            //bool allowedGrowth = true;
            //switch (model.Frequency)
            //{
            //    case Frequency.Weekly:
            //        allowedGrowth = model.Growth != Growth.LastDay;
            //        break;

            //    case Frequency.Monthly:
            //        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek;
            //        break;

            //    case Frequency.Quarterly:
            //        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth;
            //        break;

            //    case Frequency.Yearly:
            //        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth && model.Growth != Growth.LastQuarter;
            //        break;
            //}
            //if (!allowedGrowth)
            //{
            //    errorMsg = $"Growth {model.Growth} isn't allowed for frequency {model.Frequency}.";
            //    return null;
            //}

            //if (ds.Tables.Count == 0) return null;

            //var table = ds.Tables[0];
            //if (table.Rows.Count == 0) return null;

            //var sales = new List<SaleGrowth>();
            //if (model.IsFromMobile)
            //{
            //    sales.Add(new SaleGrowth
            //    {
            //        FrequencyCol = model.Frequency.ToString(),
            //        Sale = "Current",
            //        PrevSale = "Previous",
            //        Growth = "Growth"
            //    });
            //}

            //foreach (DataRow row in table.Rows)
            //{
            //    var sg = new SaleGrowth();

            //    switch (model.Frequency)
            //    {
            //        case Frequency.Daily:
            //            var day = row["Day"] != DBNull.Value ? Convert.ToString(row["Day"]) : string.Empty;
            //            var month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
            //            var year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
            //            sg.FrequencyCol = $"{day}-{month}-{year}";
            //            break;

            //        case Frequency.Weekly:
            //            var weekNum = row["Week"] != DBNull.Value ? Convert.ToString(row["Week"]) : string.Empty;
            //            month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
            //            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
            //            sg.FrequencyCol = $"Week-{weekNum} ({month}-{year})";
            //            break;

            //        case Frequency.Monthly:
            //            month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
            //            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
            //            sg.FrequencyCol = $"{month}({year})";
            //            break;

            //        case Frequency.Quarterly:
            //            var quarter = row["Quarter"] != DBNull.Value ? Convert.ToString(row["Quarter"]) : string.Empty;
            //            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
            //            sg.FrequencyCol = $"Q{quarter} ({year})";
            //            break;

            //        case Frequency.Yearly:
            //            sg.FrequencyCol = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
            //            break;
            //    }

            //    sg.Sale = row["CurrentSale"] != DBNull.Value ? model.IsFromMobile ? $"{Constants.Sign} {row["CurrentSale"]:#,###}" :
            //        $"{row["CurrentSale"]:#,###}" : "0";
            //    sg.PrevSale = row["PreviousSale"] != DBNull.Value ? model.IsFromMobile ? $"{Constants.Sign} {row["PreviousSale"]:#,###}" :
            //        $"{row["PreviousSale"]:#,###}" : "0";

            //    sg.Growth = row["Growth"] != DBNull.Value ? Convert.ToString(row["Growth"]) : string.Empty;
            //    sales.Add(sg);
            //}
            #endregion

            return sales;
        }
        #endregion

        #region Sales Chart
        public SelectList ProductGroupSelectList(string userId, string role)
        {
            // Create select list
            var selListItem = new SelectListItem() { Value = "", Text = "-- Select Group --" };
            var newList = new List<SelectListItem> { selListItem };

            var pgpp = new ProductGroupPagePermissions();
            var prodGroups = pgpp.GetProductGroupByDistributorId(userId, role);
            newList.AddRange(prodGroups.Select(p => new SelectListItem
            {
                Text = p.GroupName,
                Value = p.GroupId.ToString()
            }));

            return new SelectList(newList, "Value", "Text", null);
        }

        /// <summary>
        /// Get sales chart data that will be used for drawing chart.
        /// </summary>
        /// <param name="model">The instance of FilterModel.</param>
        /// <returns>Return instance of ChartModel.</returns>
        public ChartModel GetSalesChartData(FilterModel model, ref string errorMsg)
        {
            var salesGrowthList = GetTotalSalesGrowth(model, ref errorMsg);
            if (salesGrowthList == null || salesGrowthList.Count == 0)
                return null;

            var cm = new ChartModel
            {
                Title = $"Sales by {model.Frequency} frequency"
            };

            foreach (var sg in salesGrowthList)
            {
                switch (model.Frequency)
                {
                    case Frequency.Daily:
                        cm.XAxisLabel = "Days";
                        break;

                    case Frequency.Weekly:
                        cm.XAxisLabel = "Weeks";
                        break;

                    case Frequency.Monthly:
                        cm.XAxisLabel = "Months";
                        break;

                    case Frequency.Quarterly:
                        cm.XAxisLabel = "Quarters";
                        break;

                    case Frequency.Yearly:
                        cm.XAxisLabel = "Years";
                        break;
                }

                cm.Labels.Add(sg.FrequencyCol);
                cm.Sales.Add(!string.IsNullOrEmpty(sg.Sale) ? Convert.ToDecimal(sg.Sale) : 0);
            }

            return cm;
        }
        #endregion

        #region Sent mail on change Customer Order status
        public bool SentMailOnStatus(int OrderId, string userId, string OrderNo, string status, string role, List<ApplicationUser> superAdmins)
        {
            bool result = false;

            string OrderType = $"Order {status}";
            // Msg For Order Type
            string msg = $"Order {status}. Order Number : {OrderNo}";
            var orderData = db.OrderTables.Where(x => x.OrderID == OrderId).FirstOrDefault();
            var mail = db.MailTemplates.Where(x => x.Type == status).FirstOrDefault();

            int? workshopId = null, distributorId = null;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
            {
                distributorId = userId.GetDistributorId(role);
                workshopId = db.DistributorWorkShops.Where(x => x.DistributorId == distributorId && x.UserId == userId).Select(s => s.WorkShopId).FirstOrDefault();
            }
            clsEmailMgt clsEmailMgt = new clsEmailMgt();

            if (role != Constants.RoIncharge && role != Constants.Distributor && role != Constants.SuperAdmin)
            {
                // Send Mail & notification for user
                var user = db.UserDetails.Where(u => u.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    var notification = new Notification()
                    {
                        UserId = userId.ToString(),
                        Type = OrderType,
                        Message = msg,
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        RefUserId = OrderId.ToString(),
                        WorkshopId = workshopId,
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();
                    if (mail != null)
                    {
                        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{user.FirstName + " " + user.LastName}", user.AspNetUser.Email, user.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                    }
                }
            }


            var roUserDetail = (from w in db.WorkShops
                                join d in db.DistributorsOutlets on w.outletId equals d.OutletId
                                join u in db.UserDetails on d.UserId equals u.UserId
                                where w.WorkShopId == workshopId && d.DistributorId == distributorId && w.outletId != null && w.outletId != 0
                                select u
                           ).ToList();

            // Save Notifications for Ro Incharge
            if (roUserDetail.Any())
            {
                foreach (var roUser in roUserDetail)
                {
                    var notification = new Notification()
                    {
                        UserId = roUser.UserId.ToString(),
                        Type = OrderType,
                        Message = msg,
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        RefUserId = OrderId.ToString(),
                        WorkshopId = workshopId,
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();

                    if (mail != null)
                    {
                        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{roUser.FirstName + " " + roUser.LastName}", roUser.AspNetUser.Email, roUser.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                    }
                }
            }
            // Save Notifications for Distributors
            var DistributorUserId = db.DistributorUsers.Where(x => x.DistributorId == distributorId).Select(u => u.UserId).FirstOrDefault();
            if (DistributorUserId != null)
            {
                var notification = new Notification()
                {
                    UserId = DistributorUserId.ToString(),
                    Type = OrderType,
                    Message = msg,
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    RefUserId = OrderId.ToString(),//userId,
                    WorkshopId = workshopId
                };
                db.Notifications.Add(notification);
                db.SaveChanges();

                if (mail != null)
                {
                    var distUser = db.UserDetails.Where(x => x.UserId == DistributorUserId).FirstOrDefault();
                    var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{distUser.FirstName + " " + distUser.LastName}", distUser.AspNetUser.Email, distUser.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                }
            }
            // Save Notifications for Super Admin
            if (superAdmins.Any())
            {
                foreach (var sa in superAdmins)
                {
                    var notification = new Notification()
                    {
                        UserId = sa.Id.ToString(),
                        Type = OrderType,
                        Message = msg,
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        RefUserId = OrderId.ToString(),//userId,
                        WorkshopId = workshopId,
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();

                    if (mail != null)
                    {
                        var superAdmin = db.UserDetails.Where(x => x.UserId == sa.Id).FirstOrDefault();
                        var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{superAdmin.FirstName + " " + superAdmin.LastName}", superAdmin.AspNetUser.Email, superAdmin.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                    }
                }
            }
            result = true;

            return result;
        }
        #endregion

        #region Save Notification On Item Add to Cart
        public bool SaveNotificationAddToCart(int temporderId, string userId, string role)
        {
            var TemporderData = db.TempOrders.Where(x => x.TempOrderId == temporderId).FirstOrDefault();
            string partNumber = "";
            var TemporderdetailData = db.TempOrderDetails.Where(x => x.TempOrderId == temporderId).ToList();
            foreach (var detail in TemporderdetailData)
            {
                if (partNumber == "") { partNumber = detail.Product.PartNo; }
                else { partNumber = ", " + detail.Product.PartNo; }
            }
            // Msg For Order Type
            string msg = $"Customer new items add in cart with part no. {partNumber} ";

            int? workshopId = null, distributorId = null;
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
            {
                distributorId = userId.GetDistributorId(role);
                workshopId = db.DistributorWorkShops.Where(x => x.DistributorId == distributorId && x.UserId == userId).Select(s => s.WorkShopId).FirstOrDefault();


                // Save Notifications for User
                var user = db.UserDetails.Where(u => u.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    var notification = new Notification()
                    {
                        UserId = userId.ToString(),
                        Type = Constants.ItemAddtocart,
                        Message = msg,
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        RefUserId = partNumber.ToString(),
                        WorkshopId = workshopId,
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();
                }


                return true;
            }
            else { return false; }

        }
        #endregion

        #region Customer Back Order Details by OrderNumber
        public List<CustomerBackOrderModel> GetCustomerBackOrderDetails(CustomerBackOrderModel model)
        {
            return db.CustomerBackOrders.AsNoTracking().Where(c => c.CONo == model.OrderNo).ToList().Select(cbo => new CustomerBackOrderModel
            {
                Products = new List<ProductModel>(),
                OrderNo = cbo.CONo,
                OrderDate = cbo.CODate?.ToString("dd MMM, yyyy"),
                Status = cbo.PartStatus,
                PartNumber = cbo.PartNum,
                Qty = Convert.ToInt32(cbo.Order),
                PartyName=cbo.PartyName
            }).ToList();

            // NOTE: Use loop if need to show product for each cboModel
            //var product = db.Products.Where(p => p.PartNo == cbo.PartNum).FirstOrDefault();
            //if (product != null)
            //{
            //    cboModel.Products.Add(new ProductModel
            //    {
            //        ProductId = product.ProductId,
            //        ProductName = product.ProductName,
            //        PartNumber = product.PartNo,
            //        Description = product.Description,
            //        Price = !product.Price.HasValue ? "0" : String.Format("{0:#,###}", product.Price),
            //        ImagePath = general.CheckImageurl(product.ImagePath)

            //    });
            //}
        }
        #endregion

        #region Get total sales growth For App
        public List<SaleGrowth> GetTotalSalesGrowthApp(SaleGrowthFilter model, ref string errorMsg)
        {
            var sales = new List<SaleGrowth>();
            sales.Add(new SaleGrowth
            {
                FrequencyCol = model.Frequency.ToString(),
                Sale = "Current",
                PrevSale = "Previous",
                Growth = "Growth"
            });

            var runCode = true;

            if (model.Frequency == Frequency.Yearly && model.Growth == Growth.LastYearly)
            {
                int days = Convert.ToInt32((model.EndDate - model.StartDate).TotalDays);
                if (days > 365)
                {
                    runCode = false;
                    DateTime endDate = new DateTime(model.StartDate.Year, model.EndDate.Month, model.EndDate.Day);
                    DateTime startDate = model.StartDate.AddYears(-1);
                    int years = Convert.ToInt32(days / 365);
                    int rem = days % 365;
                    if (rem > 0) { years = years + 1; }

                    for (int i = 1; i <= years; i++)
                    {
                        DateTime nextEndDate = endDate.AddYears(i);
                        DateTime nextStartDate = startDate.AddYears(i);


                        // call sp
                        SqlParameter[] sqlParams = {
                                       new SqlParameter("@StartDate",nextStartDate),
                                       new SqlParameter("@EndDate",nextEndDate),
                                       new SqlParameter("@PartCategory",model.PartCategory),
                                       new SqlParameter("@Frequency",model.Frequency.ToString()),
                                       new SqlParameter("@Growth",model.Growth.ToString()),
                                       new SqlParameter("@Role",model.Role),
                                       new SqlParameter("@UserId",model.UserId)
                                   };
                        DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_SalesAndGrowth_App", sqlParams);

                        // Handle not allowed growth for frequency
                        bool allowedGrowth = true;
                        switch (model.Frequency)
                        {
                            case Frequency.Weekly:
                                allowedGrowth = model.Growth != Growth.LastDay;
                                break;

                            case Frequency.Monthly:
                                allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek;
                                break;

                            case Frequency.Quarterly:
                                allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth;
                                break;

                            case Frequency.Yearly:
                                allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth && model.Growth != Growth.LastQuarter;
                                break;
                        }
                        if (!allowedGrowth)
                        {
                            errorMsg = $"Growth {model.Growth} isn't allowed for frequency {model.Frequency}.";
                            return null;
                        }

                        if (ds.Tables.Count == 0) return null;

                        var table = ds.Tables[0];
                        if (table.Rows.Count == 0) return null;



                        foreach (DataRow row in table.Rows)
                        {
                            var sg = new SaleGrowth();

                            switch (model.Frequency)
                            {
                                case Frequency.Daily:
                                    var day = row["Day"] != DBNull.Value ? Convert.ToString(row["Day"]) : string.Empty;
                                    var month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                                    var year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    sg.FrequencyCol = $"{day}-{month}-{year}";
                                    break;

                                case Frequency.Weekly:
                                    var weekNum = row["Week"] != DBNull.Value ? Convert.ToString(row["Week"]) : string.Empty;
                                    month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                                    year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    sg.FrequencyCol = $"Week-{weekNum} ({month}-{year})";
                                    break;

                                case Frequency.Monthly:
                                    month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                                    year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    sg.FrequencyCol = $"{month}({year})";
                                    break;

                                case Frequency.Quarterly:
                                    var quarter = row["Quarter"] != DBNull.Value ? Convert.ToString(row["Quarter"]) : string.Empty;
                                    year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    sg.FrequencyCol = $"Q{quarter} ({year})";
                                    break;

                                case Frequency.Yearly:
                                    sg.FrequencyCol = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                                    break;
                            }

                            sg.Sale = row["CurrentSale"] != DBNull.Value ? $"{Constants.Sign} {row["CurrentSale"]:#,###}" : "0";
                            sg.PrevSale = row["PreviousSale"] != DBNull.Value ? $"{Constants.Sign} {row["PreviousSale"]:#,###}" : "0";
                            sg.Growth = row["Growth"] != DBNull.Value ? Convert.ToString(row["Growth"]) : string.Empty;
                            sales.Add(sg);
                        }

                    }
                }
            }


            if (runCode)
            {
                SqlParameter[] sqlParams = {
                                           new SqlParameter("@StartDate",model.StartDate),
                                           new SqlParameter("@EndDate",model.EndDate),
                                           new SqlParameter("@PartCategory",model.PartCategory),
                                           new SqlParameter("@Frequency",model.Frequency.ToString()),
                                           new SqlParameter("@Growth",model.Growth.ToString()),
                                           new SqlParameter("@Role",model.Role),
                                           new SqlParameter("@UserId",model.UserId)
                                       };

                DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_SalesAndGrowth_App", sqlParams);

                // Handle not allowed growth for frequency
                bool allowedGrowth = true;
                switch (model.Frequency)
                {
                    case Frequency.Weekly:
                        allowedGrowth = model.Growth != Growth.LastDay;
                        break;

                    case Frequency.Monthly:
                        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek;
                        break;

                    case Frequency.Quarterly:
                        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth;
                        break;

                    case Frequency.Yearly:
                        allowedGrowth = model.Growth != Growth.LastDay && model.Growth != Growth.LastWeek && model.Growth != Growth.LastMonth && model.Growth != Growth.LastQuarter;
                        break;
                }
                if (!allowedGrowth)
                {
                    errorMsg = $"Growth {model.Growth} isn't allowed for frequency {model.Frequency}.";
                    return null;
                }

                if (ds.Tables.Count == 0) return null;

                var table = ds.Tables[0];
                if (table.Rows.Count == 0) return null;



                foreach (DataRow row in table.Rows)
                {
                    var sg = new SaleGrowth();

                    switch (model.Frequency)
                    {
                        case Frequency.Daily:
                            var day = row["Day"] != DBNull.Value ? Convert.ToString(row["Day"]) : string.Empty;
                            var month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                            var year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            sg.FrequencyCol = $"{day}-{month}-{year}";
                            break;

                        case Frequency.Weekly:
                            var weekNum = row["Week"] != DBNull.Value ? Convert.ToString(row["Week"]) : string.Empty;
                            month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            sg.FrequencyCol = $"Week-{weekNum} ({month}-{year})";
                            break;

                        case Frequency.Monthly:
                            month = row["Month"] != DBNull.Value ? Convert.ToString(row["Month"]) : string.Empty;
                            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            sg.FrequencyCol = $"{month}({year})";
                            break;

                        case Frequency.Quarterly:
                            var quarter = row["Quarter"] != DBNull.Value ? Convert.ToString(row["Quarter"]) : string.Empty;
                            year = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            sg.FrequencyCol = $"Q{quarter} ({year})";
                            break;

                        case Frequency.Yearly:
                            sg.FrequencyCol = row["Year"] != DBNull.Value ? Convert.ToString(row["Year"]) : string.Empty;
                            break;
                    }

                    sg.Sale = row["CurrentSale"] != DBNull.Value ? $"{Constants.Sign} {row["CurrentSale"]:#,###}" : "0";
                    sg.PrevSale = row["PreviousSale"] != DBNull.Value ? $"{Constants.Sign} {row["PreviousSale"]:#,###}" : "0";

                    sg.Growth = row["Growth"] != DBNull.Value ? Convert.ToString(row["Growth"]) : string.Empty;
                    sales.Add(sg);
                }
            }

            return sales;
        }
        #endregion
    }
}