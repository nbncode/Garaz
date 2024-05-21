using Garaaz.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoOutlet
    {
        garaazEntities db = new garaazEntities();

        /// <summary>
        /// Get distributor's outlets.
        /// </summary>
        /// <param name="distributorId">The id of the distributor whose outlets need to be located.</param>
        /// <returns>Return list of outlets.</returns>
        public List<Outlet> GetDistributorOutlets(int? distributorId)
        {
            return (from d in db.DistributorsOutlets.AsNoTracking()
                    join o in db.Outlets.AsNoTracking() on d.OutletId equals o.OutletId
                    where d.DistributorId == distributorId
                    select o).Distinct().ToList();
        }

        public List<OutletListResponse> GetOutlets(int distributorId)
        {
            return (from d in db.DistributorsOutlets
                    join o in db.Outlets on d.OutletId equals o.OutletId
                    where d.DistributorId == distributorId
                    select new OutletListResponse()
                    {
                        OutletId = o.OutletId,
                        OutletName = o.OutletName
                    }).ToList();
        }

        public List<DailyStockResponse> GetDailyStockQty(int ProductId)
        {
            return (from d in db.Products
                    join o in db.DailyStocks on d.PartNo equals o.PartNum
                    where d.ProductId == ProductId && o.OutletId.HasValue
                    select new DailyStockResponse()
                    {
                        OutletId = o.OutletId.Value,
                        Qty = !string.IsNullOrEmpty(o.StockQuantity) ? o.StockQuantity : "0"
                    }).ToList();
        }

        /// <summary>
        /// Save or update outlet. Tuple with Item1 indicate saved or updated. Item2 provide respective message.
        /// </summary>        
        public Tuple<bool, string> SaveOrUpdateOutlet(OutletModel model)
        {
            try
            {
                var outlet = new Outlet
                {
                    Address = model.Address,
                    OutletName = model.OutletName,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    OutletCode = model.OutletCode
                };

                if (model.OutletId > 0)
                {
                    var oUpdated = UpdateOutlet(outlet, model.OutletId);
                    return new Tuple<bool, string>(oUpdated, "Outlet updated successfully!");
                }
                else
                {
                    var oSaved = SaveOutlet(outlet, model.DistributorId);
                    return new Tuple<bool, string>(oSaved, "Outlet registered successfully!");
                }
            }
            catch (Exception exc)
            {
                return new Tuple<bool, string>(false, exc.Message);
            }
        }


        /// <summary>
        /// Get outlet based on Outlet Id.
        /// </summary>        
        public Outlet GetOutlet(int outletId)
        {
            return db.Outlets.Where(o => o.OutletId == outletId).FirstOrDefault();
        }

        /// <summary>
        /// Save outlet for distributor.
        /// </summary>        
        public bool SaveOutlet(Outlet model, int distributorId)
        {
            var outlet = (from o in db.Outlets
                          join d in db.DistributorsOutlets on o.OutletId equals d.OutletId
                          where o.OutletCode == model.OutletCode
                          select o).FirstOrDefault();

            if (outlet == null)
            {
                db.Outlets.Add(model);

                var distOut = new DistributorsOutlet()
                {
                    OutletId = model.OutletId,
                    DistributorId = distributorId,
                    UserId = null
                };
                db.DistributorsOutlets.Add(distOut);
                return db.SaveChanges() > 0;
            }
            else
            {
                var distOutlet = outlet.DistributorsOutlets.FirstOrDefault(d => d.DistributorId == distributorId);
                if (distOutlet != null)
                {
                    throw new Exception($"Outlet with outlet code '{model.OutletCode}' already exists for this distributor.");
                }
                else
                {
                    throw new Exception($"Outlet with outlet code '{model.OutletCode}' already exists for another distributor.");
                }
            }
        }

        /// <summary>
        /// Save outlet in database.
        /// </summary>        
        public bool SaveDailyStock(List<DailyStock> lstModel, int distributoId, string partNo)
        {
            var data = db.DailyStocks.Where(x => x.DistributorId == distributoId && x.PartNum == partNo).ToList();
            if (data.Any())
            {
                db.DailyStocks.RemoveRange(data);
                db.SaveChanges();
            }
            if (lstModel.Any())
            {
                lstModel.ForEach(a =>
                {
                    a.StockQuantity = string.IsNullOrEmpty(a.StockQuantity) ? "0" : a.StockQuantity;
                });
                db.DailyStocks.AddRange(lstModel);
                return db.SaveChanges() > 0;
            }
            return false;
        }

        /// <summary>
        /// Update outlet based on outlet Id.
        /// </summary>        
        public bool UpdateOutlet(Outlet model, int outletId)
        {
            var outlet = db.Outlets.Where(a => a.OutletId == outletId).FirstOrDefault();
            if (outlet == null) return false;

            outlet.Address = model.Address;
            outlet.OutletName = model.OutletName;
            outlet.Latitude = model.Latitude;
            outlet.Longitude = model.Longitude;
            outlet.OutletCode = model.OutletCode;
            db.SaveChanges();
            return true;
        }

        /// <summary>
        /// Delete outlet based on Outlet Id.
        /// </summary>
        /// <param name="outletId">The Outlet Id of the outlet to be deleted.</param>
        /// <returns>Return true if deleted else false.</returns>
        public bool DeleteOutlet(int outletId)
        {
            var outlet = db.Outlets.Where(o => o.OutletId == outletId).FirstOrDefault();
            if (outlet == null) return false;

            db.Outlets.Remove(outlet);
            return db.SaveChanges() > 0;
        }

        /// <summary>
        /// Save user for specific outlet.
        /// </summary>
        /// <param name="userId">The user id of the user to be saved.</param>
        /// <param name="distributorId">The distributor Id of which outlets are used.</param>
        /// <param name="outletId">The outlet Id of the outlet selected by end user.</param>
        public bool SaveUserForOutlet(string userId, int distributorId, int outletId)
        {
            // TODO: Check if it work for other save cases
            // Check if record matching with User Id and Dist Id exists
            //var distOWithUser = db.DistributorsOutlets.FirstOrDefault(d => d.DistributorId == distributorId && d.UserId == userId);

            //if (distOWithUser != null)
            //{
            //    // User is trying to update his outlet
            //    distOWithUser.UserId = null;
            //    var distO = db.DistributorsOutlets.FirstOrDefault(d => d.DistributorId == distributorId && d.OutletId == outletId && d.UserId == null);
            //    if (distO != null)
            //    {
            //        distO.UserId = userId;
            //        db.SaveChanges();
            //    }
            //}
            //else
            //{

            // User is registering first time
            var distOWithoutUser = db.DistributorsOutlets.FirstOrDefault(d => d.DistributorId == distributorId && d.OutletId == outletId && d.UserId == null);

            var distRoIncharge = db.DistributorsOutlets.FirstOrDefault(d => d.DistributorId == distributorId && d.UserId == userId);
            if (distOWithoutUser != null && distRoIncharge == null)
            {
                distOWithoutUser.UserId = userId;
                db.SaveChanges();
                return true;
            }
            return false;
            // }
        }

        /// <summary>
        /// Get outlet Id.
        /// </summary>
        /// <param name="userId">The user id of the user that will be used as filter.</param>
        /// <param name="distributorId">The distributor id that will be used as filter.</param>
        /// <returns>Return outlet Id if found else zero.</returns>
        public int GetOutletId(string userId, int distributorId)
        {
            var outlet = db.DistributorsOutlets.Where(d => d.UserId == userId && d.DistributorId == distributorId).FirstOrDefault();
            return outlet != null ? Convert.ToInt32(outlet.OutletId) : 0;
        }

        /// <summary>
        /// The select list of Outlets.
        /// </summary>
        /// <param name="distributorId">The distributor Id for which to filter outlets.</param>
        /// <param name="userId">The user id of the user whose outlet should be included.</param>
        /// <returns>Return SelectList of Outlets.</returns>
        public SelectList OutletsSelectList(int distributorId, string userId)
        {
            // Create select list
            var selListItem = new SelectListItem() { Value = "0", Text = "-- Select Outlet --" };
            var newList = new List<SelectListItem> { selListItem };

            var distOutlets = (from d in db.DistributorsOutlets
                               join o in db.Outlets on d.OutletId equals o.OutletId
                               where d.DistributorId == distributorId && (d.UserId == null || d.UserId == userId)
                               select o).ToList();

            foreach (var distO in distOutlets)
            {
                newList.Add(new SelectListItem() { Value = distO.OutletId.ToString(), Text = distO.OutletName });
            }
            return new SelectList(newList, "Value", "Text", null);
        }

        public List<OutletListResponse> GetOutletList(string userId, string role)
        {
            var distributorId = userId.GetDistributorId(role);
            var defaultOutlet = 0;
            var aspNetUser = db.AspNetUsers.FirstOrDefault(u => u.Id == userId);

            if (role == Constants.Workshop || role == Constants.WorkshopUsers)
            {
                var workshopId = userId.GetWorkshopId(role);
                var workshop = db.WorkShops.FirstOrDefault(a => a.WorkShopId == workshopId);
                if (workshop != null)
                {
                    defaultOutlet = workshop.outletId ?? 0;
                }
            }
            else if (aspNetUser?.AspNetRoles.FirstOrDefault(a => a.Name.Contains(Constants.RoIncharge)) != null)
            {
                var distributorsOutlet = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == userId);
                if (distributorsOutlet != null)
                {
                    defaultOutlet = distributorsOutlet.OutletId ?? 0;
                }
            }
            else if (aspNetUser?.AspNetRoles.FirstOrDefault(a => a.Name.Contains(Constants.SalesExecutive)) != null)
            {
                var roUserId = db.RoSalesExecutives.Where(w => w.SeUserId == userId).Select(w => w.RoUserId).FirstOrDefault();
                var distributorsOutlets = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == roUserId);
                if (distributorsOutlets != null)
                {
                    defaultOutlet = distributorsOutlets.OutletId ?? 0;
                }
            }

            var data = (from d in db.DistributorsOutlets
                        join o in db.Outlets on d.OutletId equals o.OutletId
                        where d.DistributorId == (role == Constants.SuperAdmin ? d.DistributorId : distributorId)
                        select new OutletListResponse
                        {
                            OutletId = o.OutletId,
                            OutletName = o.OutletName,
                            IsDefault = o.OutletId == defaultOutlet
                        }).Distinct().ToList();
            return data;
        }

        public List<ProductLocationResponse> GetProductLocation(int productId, int distributorId)
        {
            var product = db.Products.FirstOrDefault(x => x.ProductId == productId);
            var dailyStock = db.DailyStocks.Where(x => x.DistributorId == distributorId && !string.IsNullOrEmpty(x.PartNum) && x.PartNum == product.PartNo);

            var data = (from o in db.Outlets
                        join dos in db.DistributorsOutlets on o.OutletId equals dos.OutletId
                        where dos.DistributorId == distributorId
                        join d in dailyStock on o.OutletId equals d.OutletId into ps
                        from p in ps.DefaultIfEmpty()
                        select new { o.OutletName, p.OutletId, p.StockQuantity }
                       );
            data = data.Where(x => !string.IsNullOrEmpty(x.StockQuantity) && x.StockQuantity != "0");
            List<ProductLocationResponse> lstProductLocation = new List<ProductLocationResponse>();
            foreach (var item in data)
            {
                var obj = new ProductLocationResponse();
                obj.Outlet = item.OutletName;
                obj.Qty = 0;
                obj.ProductName = string.IsNullOrEmpty(product?.ProductName) ? product?.Description : product.ProductName;
                obj.GroupName = product?.ProductGroup == null ? "" : product.ProductGroup.GroupName;
                if (item.OutletId != null && item.OutletId.Value > 0)
                {
                    if (!string.IsNullOrEmpty(item.StockQuantity))
                    {
                        int.TryParse(item.StockQuantity, out int qty);
                        obj.Qty = qty;
                    }
                    RepoDashboard repoDashboard = new RepoDashboard();
                    var colorData = repoDashboard.GetStockColor(obj.Qty);
                    obj.Color = colorData?.Color;
                    obj.ColorTag = colorData?.Tag;
                }
                lstProductLocation.Add(obj);
            }
            return lstProductLocation.OrderByDescending(o => o.Qty).ToList();
        }

        #region Register dummy workshop for RoIncharge
        public bool RegisterWalkInCustomer(clsDistributorUserInfo model)
        {
            bool isSaved = false;

            var outlet = db.Outlets.Where(o => o.OutletId == model.OutletId).AsNoTracking().FirstOrDefault();
            if (outlet == null) return isSaved;

            var consPartyCode = string.Concat(outlet.OutletCode + "WI");
            var user = db.UserDetails.Where(u => u.ConsPartyCode == consPartyCode).AsNoTracking().FirstOrDefault();

            if (user == null)
            {
                var clsWs = new clsWorkshop
                {
                    DistributorId = model.DistributorId,
                    PhoneNumber = string.Concat(model.PhoneNumber + "_1"),
                    Password = "Abc123$",
                    EmployeeCode = consPartyCode,
                    WorkShopName = CustomerType.WalkInCustomer,
                    OutletId = model.OutletId,
                    WorkshopType = CustomerType.WalkInCustomer,
                    IsApproved = true
                };

                var sc = new SystemController();
                isSaved =sc.WorkshopRegisterOrUpdate(clsWs).ResultFlag==1?true:false;
            }
            return isSaved;
        }
        #endregion
    }
}