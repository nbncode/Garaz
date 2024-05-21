using Garaaz.Controllers;
using Garaaz.Models.AppNotification;
using Garaaz.Models.Checkout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoOrder
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region Order

        #region Get Cart
        public Cart GetCart(GetCartModel model)
        {
            Cart cart = null;
            var cartMainList = new List<CartMain>();

            var cartMain1 = new CartMain()
            {
                Name = "Available at outlet",
                Value = 1,
                data = new List<clsCartData>()
            };
            var cartMain2 = new CartMain()
            {
                Name = string.Empty,//"Available at other outlet",
                Value = 2,
                data = new List<clsCartData>()
            };
            var cartMain3 = new CartMain()
            {
                Name = "Currently not available",
                Value = 3,
                data = new List<clsCartData>()
            };


            var tempOrder = db.TempOrders.Where(x => x.TempOrderId == model.TempOrderId).FirstOrDefault();
            if (tempOrder != null)
            {
                var ru = new RepoUsers();
                var ws = ru.GetDistributorWorkShop(model.UserId);
                var outletId = ws?.WorkShop.outletId;

                var general = new General();
                foreach (var od in tempOrder.TempOrderDetails)
                {
                    var cartItem = new clsCartData()
                    {
                        ProductId = od.ProductId,
                        //ProductName = od.Product.ProductName,
                        ProductName = string.IsNullOrEmpty(od.Product.ProductName) ? od.Product.Description : od.Product.ProductName,
                        PartNumber = od.Product.PartNo,
                        Brand = od.Product.Brand?.Name, //od.Product?.ProductGroup?.Variant?.Vehicle?.Brand.Name,
                        Description = od.Product.Description,
                        UnitPrice = od.UnitPrice,
                        TotalPrice = Convert.ToDecimal(od.TotalPrice),
                        Quantity = Convert.ToInt32(od.Qty),
                        ImagePath = od.Product.ImagePath,
                        TempOrderId = Convert.ToInt32(od.TempOrderId),
                        ProductAvailablityType = od.AvailabilityType,
                        OutletId = od.OutletId,
                        OutletData = new List<OutletData>()
                    };
                    cartItem.ImagePath = general.CheckImageUrl(!string.IsNullOrEmpty(cartItem.ImagePath) ? general.CheckImageUrl(cartItem.ImagePath) : "/assets/images/NoPhotoAvailable.png");
                    if (outletId.HasValue)
                    {
                        var dailyStock = db.DailyStocks.Where(s => s.OutletId == outletId.Value && s.PartNum == od.Product.PartNo).FirstOrDefault();
                        if (dailyStock != null && int.TryParse(dailyStock.StockQuantity, out int stockQty))
                        {
                            cartItem.Available = stockQty > 0;

                            if (cartItem.Available && cartItem.OutletId == null)
                            {
                                cartItem.OutletId = outletId;
                            }

                            if (string.IsNullOrEmpty(cartItem.ProductAvailablityType))
                            {
                                cartItem.ProductAvailablityType = Constants.SelfPickup;
                            }
                        }
                    }

                    //Case 1 : When Default Outlet Has Data
                    if (ws != null)
                    {
                        if (cartItem.Available)
                        {
                            var OutletDatas = new List<OutletData>();

                            //Add Default Outlet
                            var prod = new List<string>();
                            prod.Add(Constants.SelfPickup);
                            prod.Add(Constants.ExpressDelivery);


                            OutletDatas.Add(new OutletData()
                            {
                                OutletId = outletId.Value,
                                OutletName = ws.WorkShop.Outlet.OutletName,
                                lstProductAvailablity = prod,
                                IsDefaultOutlet = true
                            });
                            cartItem.OutletData = OutletDatas;

                            cartMain1.data.Add(cartItem);
                            //cartMainList.Add(cartMain1);
                        }
                        else
                        {

                            var dailyStock = (from d in db.DailyStocks
                                              join o in db.Outlets
                                              on d.OutletId equals o.OutletId
                                              join t in db.DistributorsOutlets
                                              on o.OutletId equals t.OutletId
                                              where t.DistributorId == ws.DistributorId
                                              && d.PartNum == od.Product.PartNo &&
                                              !string.IsNullOrEmpty(d.StockQuantity)
                                              select d).ToList();

                            //when not Available to first but avilable to another one
                            if (dailyStock.Count > 0)
                            {
                                var OutletDatas = new List<OutletData>();

                                //Add Default Outlet
                                var prod = new List<string>();
                                prod.Add(Constants.ArrageItem);

                                if (outletId != null)
                                {
                                    OutletDatas.Add(new OutletData()
                                    {
                                        OutletId = outletId.Value,
                                        OutletName = ws.WorkShop.Outlet?.OutletName,
                                        lstProductAvailablity = prod
                                    });
                                }

                                //Now Other Outlets
                                prod = new List<string>();
                                prod.Add(Constants.SelfPickup);
                                prod.Add(Constants.ExpressDelivery);

                                foreach (var item in dailyStock)
                                {
                                    OutletDatas.Add(new OutletData()
                                    {
                                        OutletId = item.OutletId.Value,
                                        OutletName = item.Outlet.OutletName,
                                        lstProductAvailablity = prod
                                    });
                                }

                                cartItem.OutletData = OutletDatas;
                                cartMain2.data.Add(cartItem);

                            }

                            //cartMainList.Add(cartMain2);
                        }

                    }

                    //When not Available on any outlet
                    if (cartItem.OutletData.Count == 0)
                    {
                        cartItem.ProductAvailablityNotFoundData = new List<string>();
                        cartItem.ProductAvailablityNotFoundData.Add(Constants.OrderToMSIL);
                        cartItem.ProductAvailablityType = Constants.OrderToMSIL;

                        cartMain3.data.Add(cartItem);
                        //cartMainList.Add(cartMain3);
                    }
                }

                if (cartMain1.data.Count > 0)
                    cartMainList.Add(cartMain1);
                if (cartMain2.data.Count > 0)
                    cartMainList.Add(cartMain2);
                if (cartMain3.data.Count > 0)
                    cartMainList.Add(cartMain3);

                cart = new Cart
                {
                    CartMain = cartMainList,
                    SoldBy = ws?.Distributor?.DistributorName ?? string.Empty, //"Maruti",
                    TotalItems = cartMainList.Sum(a => a.data.Sum(b => b.Quantity)),
                    SubTotal = tempOrder.SubTotal ?? 0,
                    Discount = tempOrder.Discount ?? 0,
                    DiscountCode = tempOrder.DiscountCode,
                    DeliveryCharge = tempOrder.DeliveryCharge ?? 0,
                    PackingCharge = tempOrder.PackingCharge ?? 0,
                    GrandTotal = tempOrder.GrandTotal ?? 0,
                    TempOrderId = model.TempOrderId
                };
            }
            return cart;
        }
        #endregion

        #region Get Cart counts
        public int GetCartcount(int temporderid)
        {
            var count = 0;
            var tempOrder = db.TempOrders.Where(x => x.TempOrderId == temporderid).FirstOrDefault();
            if (tempOrder != null)
            {
                var temporderDetail = db.TempOrderDetails.Where(x => x.TempOrderId == temporderid);
                count = Convert.ToInt32(temporderDetail != null ? temporderDetail.Sum(x => x.Qty) : 0);
            }
            return count;
        }
        #endregion

        #region Remove Cart
        public bool RemoveCart(RemoveCartModel model, out string Message)
        {
            Message = "Product cart Removed";

            var removedata = db.TempOrderDetails.Where(x => x.ProductId == model.ProductId && x.TempOrderId == model.TempOrderId).FirstOrDefault();
            if (removedata != null)
            {
                db.TempOrderDetails.Remove(removedata);
                db.SaveChanges();


                var oldOrderData = db.TempOrders.Where(x => x.TempOrderId == model.TempOrderId).FirstOrDefault();

                if (oldOrderData.Discount.HasValue)
                {
                    var oldCartDetail = db.TempOrderDetails.Where(x => x.TempOrderId == model.TempOrderId);

                    if (oldCartDetail.Count() == 0)
                    {
                        oldOrderData.Discount = 0;
                        oldOrderData.DiscountCode = "";
                        db.SaveChanges();
                    }
                    else if (oldOrderData.Discount.Value > oldCartDetail.Sum(x => x.TotalPrice))
                    {
                        oldOrderData.Discount = 0;
                        oldOrderData.DiscountCode = "";
                        db.SaveChanges();
                        Message = "Coupon amount is greater then your cart amount so your coupon is removed from your cart";
                    }
                }
                return SetGrandTotal(model.TempOrderId);
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Add Cart
        public bool AddToCart(AddtocartModel model, out int tempOrderId)
        {
            tempOrderId = 0;

            if (model.TempOrderId == 0)
            {
                TempOrder orderData = new TempOrder();

                RepoUsers repoUsers = new RepoUsers();
                var workshop = repoUsers.GetWorkshopByUserId(model.UserId);
                if (workshop != null)
                {
                    orderData.WorkShopId = workshop.WorkShopId;
                    var distWs = repoUsers.getDistributorByWorkShopId(workshop.WorkShopId);
                    orderData.DistributorId = distWs.FirstOrDefault()?.DistributorId;
                }
                orderData.UserId = model.UserId;
                orderData.OrderDate = DateTime.Now;

                db.TempOrders.Add(orderData);
                db.SaveChanges();
                model.TempOrderId = orderData.TempOrderId;
            }
            var oldOrderDetail = db.TempOrderDetails.Where(x => x.ProductId == model.ProductId && x.TempOrderId == model.TempOrderId).FirstOrDefault();
            if (oldOrderDetail != null)
            {
                oldOrderDetail.Qty += model.Qty;
                oldOrderDetail.TotalPrice = oldOrderDetail.Qty * oldOrderDetail.UnitPrice;
                db.SaveChanges();
            }
            else
            {
                TempOrderDetail tempOrderDetail = new TempOrderDetail()
                {
                    TempOrderId = model.TempOrderId,
                    ProductId = model.ProductId,
                    Qty = model.Qty,
                    UnitPrice = model.UnitPrice,
                    TotalPrice = model.Qty * (model.UnitPrice ?? 0),
                    OutletId = model.OutletId
                };
                db.TempOrderDetails.Add(tempOrderDetail);
                db.SaveChanges();
            }
            tempOrderId = model.TempOrderId;
            return SetGrandTotal(model.TempOrderId);

        }
        #endregion
        public int GetProductId(string productNumber, out decimal price)
        {
            var product = db.Products.Where(p => p.PartNo == productNumber).FirstOrDefault();
            price = product?.Price ?? 0;
            return product?.ProductId ?? 0;
        }
        #region Update Cart
        public bool UpdateCart(List<clsCartData> model)
        {
            var tempOrderId = model.FirstOrDefault().TempOrderId;

            foreach (var item in model)
            {
                var oldOrderDetail = db.TempOrderDetails.Where(x => x.ProductId == item.ProductId && x.TempOrderId == item.TempOrderId).FirstOrDefault();
                if (oldOrderDetail != null)
                {
                    oldOrderDetail.Qty = item.Quantity;
                    oldOrderDetail.TotalPrice = item.Quantity * item.UnitPrice;
                    oldOrderDetail.UnitPrice = item.UnitPrice;
                    oldOrderDetail.OutletId = item.OutletId != null ? item.OutletId.Value : item.OutletId;
                    oldOrderDetail.AvailabilityType = item.ProductAvailablityType != null ? item.ProductAvailablityType : item.OutletId != null ? "Self Pickup" : "";
                    db.SaveChanges();
                }
            }

            return SetGrandTotal(tempOrderId);

        }
        #endregion

        public string GetGrandTotal(GetCartModel model)
        {
            decimal? grandTotal = 0;
            var tempOrder = db.TempOrders.Where(x => x.TempOrderId == model.TempOrderId).FirstOrDefault();
            if (tempOrder != null)
            {
                grandTotal = tempOrder.GrandTotal;
            }
            return Convert.ToString(grandTotal);
        }

        public bool SetGrandTotal(int tempOrderId)
        {
            var oldCartDetail = db.TempOrderDetails.Where(x => x.TempOrderId == tempOrderId);
            var oldOrderData = db.TempOrders.Where(x => x.TempOrderId == tempOrderId).FirstOrDefault();
            if (oldOrderData != null)
            {
                var deliverycharge = oldOrderData.DeliveryCharge != null ? oldOrderData.DeliveryCharge : 0;
                var discount = (oldOrderData.Discount != null ? oldOrderData.Discount : 0);
                var PackingCharge = oldOrderData.PackingCharge != null ? oldOrderData.PackingCharge : 0;
                oldOrderData.SubTotal = oldCartDetail != null ? oldCartDetail.Sum(x => x.TotalPrice) : 0;
                oldOrderData.GrandTotal = (oldOrderData.SubTotal + deliverycharge + PackingCharge) - discount;
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Save Order

        public bool ProcessCheckout(ref SaveOrderModel model, string userId, string role, List<ApplicationUser> superAdmins)
        {
            SaveOrderModel soModel = model;
            if (model.IsFromMobile)
            {
                var tempOrder = db.TempOrders.FirstOrDefault(t => t.TempOrderId == soModel.TempOrderId);
                if (tempOrder != null)
                {
                    var repoUser = new RepoUsers();
                    int workshopId = repoUser.GetWorkshopIdByUserId(model.UserId, role);
                    int distributorId = repoUser.getDistributorIdByUserId(model.UserId, role);

                    tempOrder.UserId = model.UserId;
                    tempOrder.WorkShopId = workshopId;
                    tempOrder.DistributorId = distributorId;

                    db.SaveChanges();
                }
            }

            bool orderSaved = SaveOrder(model.TempOrderId, model.PaymentMethod, model.RazorpayOrderId, model.RazorpayPaymentId, model.RazorpaySignature, out SaveOrderModel saveOrderModel);

            if (orderSaved)
            {
                // delete tempOrderId from UserDetails
                RemoveTempOrderId(model.TempOrderId);

                // Push Notifications
                RepoAppNotification appNotify = new RepoAppNotification();
                var pushModel = new PushNotificationUserModel()
                {
                    UserId = userId,
                    Role = role,
                    OrderNumber = saveOrderModel.OrderNumber
                };
                var appThread = new System.Threading.Thread(() => appNotify.SendPushNotification(pushModel)) { IsBackground = true };
                appThread.Start();

                // Save Notifications Start
                var notify = new CustomerBackOrderModel { UserId = userId, Role = role };
                var rd = new RepoDashboard();

                var thread = new System.Threading.Thread(() => rd.SaveOrderNotification(saveOrderModel.OrderId, notify.UserId, notify.Role, superAdmins, saveOrderModel.OrderNumber))
                { IsBackground = true };
                thread.Start();
            }

            model = saveOrderModel;
            return orderSaved;
        }

        public bool SaveOrder(int tempOrderId, string paymentMethod, string razorPayOrderId, string razorPayPaymentId, string razorPaySignature, out SaveOrderModel soModel)
        {
            soModel = new SaveOrderModel();
            if (tempOrderId <= 0) return false;

            if (!string.IsNullOrEmpty(paymentMethod))
            {
                var repoCheckout = new RepoCheckout();
                repoCheckout.SavePaymentMethod(new PaymentMethodModel { TempOrderId = tempOrderId, PaymentMethod = paymentMethod });
            }

            var tempOrder = db.TempOrders.FirstOrDefault(x => x.TempOrderId == tempOrderId);
            if (tempOrder == null) return false;

            int? outletId = null, workshopId = null;
            if (!string.IsNullOrEmpty(tempOrder.UserId))
            {
                workshopId = tempOrder.UserId.GetWorkshopId("Workshop");
                if (workshopId > 0)
                {
                    var workshop = db.WorkShops.FirstOrDefault(a => a.WorkShopId == workshopId);
                    outletId = workshop?.outletId;
                }
            }

            var cartData = new OrderTable
            {
                OrderNo = Utils.Randomint(),
                WorkshopID = workshopId != null ? workshopId : tempOrder.WorkShopId,
                DeliveryAddressId = tempOrder.DeliveryAddressId,
                PaymentMethod = tempOrder.PaymentMethod,
                SubTotal = tempOrder.SubTotal,
                Discount = tempOrder.Discount,
                DiscountCode = tempOrder.DiscountCode,
                DeliveryCharge = tempOrder.DeliveryCharge,
                PackingCharge = tempOrder.PackingCharge,
                UserID = tempOrder.UserId,
                OrderTotal = tempOrder.GrandTotal,
                OrderDate = DateTime.Now,
                OrderStatus = OrderStatus.Pending.ToString(),
                DistributorId = tempOrder.DistributorId,
                OutletId = outletId,
                RazorpayOrderId = razorPayOrderId,
                RazorpayPaymentId = razorPayPaymentId,
                RazorpaySignature = razorPaySignature
            };

            db.OrderTables.Add(cartData);
            var status = db.SaveChanges() > 0;
            if (!status) return false;

            soModel.OrderId = cartData.OrderID;
            soModel.OrderNumber = cartData.OrderNo;
            soModel.UserId = cartData.UserID;

            var tempOrderDetails = db.TempOrderDetails.Where(x => x.TempOrderId == tempOrderId);
            var listOrderDetails = new List<OrderDetail>();

            var productIds = tempOrderDetails.Select(t => t.ProductId).ToList();
            var productDetails = db.Products.Where(p => productIds.Contains(p.ProductId)).Select(p => new
            {
                ProductID = p.ProductId,
                ProductName = string.IsNullOrEmpty(p.Description) ? string.IsNullOrEmpty(p.ProductName) ? p.PartNo : p.ProductName : p.Description,
                ImagePath = p.ImagePath,
                PartNumber = p.PartNo,
                RootPartNumber = p.RootPartNum,
                BrandName = p.BrandId != null ? p.Brand.Name : "",
                GroupName = p.GroupId != null ? p.ProductGroup.GroupName : ""
            }).ToList();

            foreach (var tod in tempOrderDetails)
            {
                if (tod.OutletId == null || string.IsNullOrEmpty(tod.AvailabilityType))
                {
                    if (outletId.HasValue)
                    {
                        var dailyStock = db.DailyStocks.FirstOrDefault(s => s.OutletId == outletId.Value && s.PartNum == tod.Product.PartNo);
                        if (dailyStock != null && int.TryParse(dailyStock.StockQuantity, out var stockQty))
                        {
                            if (!tod.OutletId.HasValue)
                            {
                                tod.OutletId = stockQty > 0 ? outletId.Value : tod.OutletId;
                            }
                            if (string.IsNullOrEmpty(tod.AvailabilityType))
                            {
                                tod.AvailabilityType = stockQty > 0 ? Constants.SelfPickup : tod.AvailabilityType;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tod.AvailabilityType))
                            {
                                tod.AvailabilityType = Constants.OrderToMSIL;
                            }
                        }
                    }
                    else
                    {
                        tod.AvailabilityType = Constants.OrderToMSIL;
                    }
                }
                var product = productDetails.FirstOrDefault(p => p.ProductID == tod.ProductId);

                listOrderDetails.Add(new OrderDetail
                {
                    OrderID = cartData.OrderID,
                    ProductID = tod.ProductId,
                    ProductName = product.ProductName,
                    UnitPrice = tod.UnitPrice,
                    Qty = tod.Qty,
                    TotalPrice = tod.TotalPrice,
                    ImagePath = product.ImagePath,
                    OutletId = tod.OutletId,
                    AvailabilityType = tod.AvailabilityType,
                    PartNumber = product.PartNumber,
                    RootPartNumber = product.RootPartNumber,
                    BrandName = product.BrandName,
                    GroupName = product.GroupName
                });
            }

            if (listOrderDetails.Count > 0)
            {
                db.OrderDetails.AddRange(listOrderDetails);
                status = db.SaveChanges() > 0;
            }
            if (!status) return false;

            // Remove applied coupon
            var wsCoupon = db.WorkshopCoupons.FirstOrDefault(x => x.CouponNo == tempOrder.DiscountCode);
            if (wsCoupon != null)
            {
                db.WorkshopCoupons.Remove(wsCoupon);
                db.SaveChanges();

                // ASK: Why this code was written?
                //if (db.SaveChanges() > 0)
                //{
                //    var wt = new WalletTransaction
                //    {
                //        UserId = tempOrder.UserId,
                //        WorkshopId = wsCoupon.WorkshopId,
                //        Type = "Redeem",
                //        Amount = tempOrder.Discount,
                //        Description = $"The coupon number {tempOrder.DiscountCode} redeemed.",
                //        RefId = Convert.ToString(cartData.OrderID),
                //        CreatedDate = DateTime.Now
                //    };
                //}
            }

            var removeTempCartDetails = db.TempOrderDetails.Where(x => x.TempOrderId == tempOrderId).ToList();
            if (removeTempCartDetails.Count > 0)
            {
                db.TempOrderDetails.RemoveRange(removeTempCartDetails);
                db.SaveChanges();
            }

            var removeTempCart = db.TempOrders.FirstOrDefault(x => x.TempOrderId == tempOrderId);
            if (removeTempCart != null)
            {
                db.TempOrders.Remove(removeTempCart);
                db.SaveChanges();
            }

            return status;

        }
        #endregion

        #region Get User's All Order
        public List<ResponseOrderModel> GetOrderByUserId(string userId)
        {
            List<ResponseOrderModel> lstResponseOrderModels = new List<ResponseOrderModel>();
            var orderData = db.OrderTables.Where(x => x.UserID == userId).ToList();
            if (orderData.Count > 0)
            {
                foreach (var item in orderData)
                {
                    ResponseOrderModel responseOrderModel = new ResponseOrderModel
                    {
                        OrderID = item.OrderID,
                        OrderNo = item.OrderNo,
                        WorkshopID = item.WorkshopID,
                        DeliveryAddressId = item.DeliveryAddressId,
                        PaymentMethod = item.PaymentMethod,
                        SubTotal = item.SubTotal,
                        Discount = item.Discount,
                        DiscountCode = item.DiscountCode,
                        DeliveryCharge = item.DeliveryCharge,
                        PackingCharge = item.PackingCharge,
                        UserID = item.UserID,
                        OrderTotal = item.OrderTotal,
                        OrderDate = item.OrderDate,
                        OrderDateStr = item.OrderDate.Value.ToString("dd MMM, yyyy"),
                        OrderStatus = item.OrderStatus
                    };

                    if (item.OrderDetails.Count > 0)
                    {
                        var general = new General();
                        var lstResponseOrderDetailModel = new List<ResponseOrderDetailModel>();
                        foreach (var od in item.OrderDetails)
                        {
                            var responseOrderDetailModel = new ResponseOrderDetailModel()
                            {
                                OrderDetailID = od.OrderDetailID,
                                OrderID = od.OrderID,
                                ProductID = od.ProductID,
                                ProductName = od.ProductName,
                                PartNumber = od.PartNumber,
                                Brand = od.BrandName,
                                ImagePath = general.CheckImageUrl(od.ImagePath),
                                Qty = od.Qty,
                                UnitPrice = od.UnitPrice,
                                TotalPrice = od.TotalPrice,
                            };

                            lstResponseOrderDetailModel.Add(responseOrderDetailModel);
                        }
                        responseOrderModel.OrderDetails = lstResponseOrderDetailModel;
                    }
                    responseOrderModel.Count = responseOrderModel.OrderDetails?.Count ?? 0;
                    lstResponseOrderModels.Add(responseOrderModel);
                }
            }
            return lstResponseOrderModels;
        }
        #endregion

        #region Get User's All Order Mobile
        public ResponseOrderModelMobile GetOrderByUserIdMobile(string userId, string role)
        {
            var aspNetUser = db.AspNetUsers.Where(u => u.Id == userId).FirstOrDefault();
            ResponseOrderModelMobile responseOrderModel = new ResponseOrderModelMobile();
            string[] statusArr = new[] { OrderStatus.BackOrder.ToString(), OrderStatus.Cancelled.ToString(), OrderStatus.Completed.ToString() };

            if (role.Equals(Constants.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                var previousOrder = db.OrderTables.Where(x => statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.PreviousOrders = GetOrderbyUserIdMobile(previousOrder);

                var currentOrder = db.OrderTables.Where(x => !statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.CurrentOrders = GetOrderbyUserIdMobile(currentOrder);

            }
            else if (role.Equals(Constants.Distributor, StringComparison.OrdinalIgnoreCase))
            {
                int distributorId = userId.GetDistributorId(role);

                var previousOrder = db.OrderTables.Where(x => x.DistributorId == distributorId && statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.PreviousOrders = GetOrderbyUserIdMobile(previousOrder);

                var currentOrder = db.OrderTables.Where(x => x.DistributorId == distributorId && !statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.CurrentOrders = GetOrderbyUserIdMobile(currentOrder);

            }
            else if (aspNetUser.AspNetRoles.Where(a => a.Name.Contains(Constants.RoIncharge)).FirstOrDefault() != null)
            {
                var DistributorsOutlets = db.DistributorsOutlets.Where(a => a.UserId == userId).FirstOrDefault();
                if (DistributorsOutlets != null)
                {
                    var workshop = (from w in db.WorkShops
                                    where w.outletId == DistributorsOutlets.OutletId
                                    select w).ToList();
                    if (workshop.Count > 0)
                    {
                        var ids = workshop.Select(a => a.WorkShopId).ToList();

                        var previousOrder = db.OrderTables.Where(x => ids.Contains(x.WorkshopID ?? 0) && statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                        responseOrderModel.PreviousOrders = GetOrderbyUserIdMobile(previousOrder);

                        var currentOrder = db.OrderTables.Where(x => ids.Contains(x.WorkshopID ?? 0) && !statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                        responseOrderModel.CurrentOrders = GetOrderbyUserIdMobile(currentOrder);
                    }
                }
            }
            else if (aspNetUser.AspNetRoles.Where(a => a.Name.Contains(Constants.SalesExecutive)).FirstOrDefault() != null)
            {
                var wIds = db.SalesExecutiveWorkshops.Where(se => se.UserId == userId).Select(w => w.WorkshopId).ToList();

                var previousOrder = db.OrderTables.Where(x => wIds.Contains(x.WorkshopID ?? 0) && statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.PreviousOrders = GetOrderbyUserIdMobile(previousOrder);

                var currentOrder = db.OrderTables.Where(x => wIds.Contains(x.WorkshopID ?? 0) && !statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.CurrentOrders = GetOrderbyUserIdMobile(currentOrder);
            }
            else if (role.Equals(Constants.Workshop, StringComparison.OrdinalIgnoreCase))
            {
                int workshopId = userId.GetWorkshopId(role);
                var previousOrder = db.OrderTables.Where(x => x.WorkshopID == workshopId && statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.PreviousOrders = GetOrderbyUserIdMobile(previousOrder);

                var currentOrder = db.OrderTables.Where(x => x.WorkshopID == workshopId && !statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.CurrentOrders = GetOrderbyUserIdMobile(currentOrder);
            }
            else
            {
                var previousOrder = db.OrderTables.Where(x => x.UserID == userId && statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.PreviousOrders = GetOrderbyUserIdMobile(previousOrder);

                var currentOrder = db.OrderTables.Where(x => x.UserID == userId && !statusArr.Contains(x.OrderStatus)).OrderByDescending(o => o.OrderDate).ToList();
                responseOrderModel.CurrentOrders = GetOrderbyUserIdMobile(currentOrder);
            }

            return responseOrderModel;
        }
        private List<ResponseOrderModel> GetOrderbyUserIdMobile(List<OrderTable> orderData)
        {
            List<ResponseOrderModel> listOrderModels = new List<ResponseOrderModel>();
            if (orderData.Count > 0)
            {
                foreach (var item in orderData)
                {
                    ResponseOrderModel responseOrderModel = new ResponseOrderModel
                    {
                        OrderID = item.OrderID,
                        OrderNo = item.OrderNo,
                        WorkshopID = item.WorkshopID,
                        DeliveryAddressId = item.DeliveryAddressId,
                        PaymentMethod = item.PaymentMethod,
                        SubTotal = item.SubTotal,
                        Discount = item.Discount,
                        DiscountCode = item.DiscountCode,
                        DeliveryCharge = item.DeliveryCharge,
                        PackingCharge = item.PackingCharge,
                        UserID = item.UserID,
                        OrderTotal = item.OrderTotal,
                        OrderDate = item.OrderDate,
                        OrderDateStr = item.OrderDate.Value.ToString("dd MMM, yyyy"),
                        OrderStatus = item.OrderStatus
                    };

                    if (item.OrderDetails.Count > 0)
                    {
                        var general = new General();
                        var lstResponseOrderDetailModel = new List<ResponseOrderDetailModel>();
                        foreach (var od in item.OrderDetails)
                        {
                            var responseOrderDetailModel = new ResponseOrderDetailModel()
                            {
                                OrderDetailID = od.OrderDetailID,
                                OrderID = od.OrderID,
                                ProductID = od.ProductID,
                                ProductName = od.ProductName,
                                PartNumber = od.PartNumber,
                                Brand = od.BrandName,
                                ImagePath = general.CheckImageUrl(od.ImagePath),
                                Qty = od.Qty,
                                UnitPrice = od.UnitPrice,
                                TotalPrice = od.TotalPrice,
                            };
                            lstResponseOrderDetailModel.Add(responseOrderDetailModel);
                        }
                        responseOrderModel.OrderDetails = lstResponseOrderDetailModel;
                    }
                    responseOrderModel.Count = responseOrderModel.OrderDetails?.Count ?? 0;
                    listOrderModels.Add(responseOrderModel);
                }
            }
            return listOrderModels;
        }
        #endregion

        #region Get all orders
        public List<ResponseOrderModel> GetAllOrders()
        {
            var lstResponseOrderModels = new List<ResponseOrderModel>();
            foreach (var item in db.OrderTables)
            {
                var responseOrderModel = new ResponseOrderModel
                {
                    OrderID = item.OrderID,
                    OrderNo = item.OrderNo,
                    WorkshopID = item.WorkshopID,
                    DeliveryAddressId = item.DeliveryAddressId,
                    PaymentMethod = item.PaymentMethod,
                    SubTotal = item.SubTotal,
                    Discount = item.Discount,
                    DiscountCode = item.DiscountCode,
                    DeliveryCharge = item.DeliveryCharge,
                    PackingCharge = item.PackingCharge,
                    UserID = item.UserID,
                    OrderTotal = item.OrderTotal,
                    OrderDate = item.OrderDate,
                    OrderStatus = item.OrderStatus,
                    // We have intentionally omit setting order details because
                    // that is not required and to improve performance
                    OrderDetails = null
                };
                lstResponseOrderModels.Add(responseOrderModel);
            }
            return lstResponseOrderModels;
        }
        #endregion

        #region Get Order by orderId
        public ResponseOrderModel GetOrderByOrderId(GetOrderModel model)
        {
            ResponseOrderModel responseOrderModel = null;
            var orderTable = db.OrderTables.Where(x => x.UserID == model.UserId && x.OrderID == model.OrderId).FirstOrDefault();
            if (orderTable != null)
            {
                var outlet = db.Outlets.Where(x => x.OutletId == orderTable.OutletId).FirstOrDefault();
                var workshopUser = (from a in db.UserDetails
                                    join d in db.DistributorWorkShops on a.UserId equals d.UserId
                                    where d.WorkShopId == orderTable.WorkshopID
                                    select a).FirstOrDefault();
                responseOrderModel = new ResponseOrderModel
                {
                    OrderID = orderTable.OrderID,
                    OrderNo = orderTable.OrderNo,
                    WorkshopID = orderTable.WorkshopID,
                    DeliveryAddressId = orderTable.DeliveryAddressId,
                    PaymentMethod = orderTable.PaymentMethod,
                    SubTotal = orderTable.SubTotal,
                    Discount = orderTable.Discount,
                    DiscountCode = orderTable.DiscountCode,
                    DeliveryCharge = orderTable.DeliveryCharge,
                    PackingCharge = orderTable.PackingCharge,
                    UserID = orderTable.UserID,
                    OrderTotal = orderTable.OrderTotal,
                    OrderDate = orderTable.OrderDate,
                    OrderDateStr = orderTable.OrderDate.Value.ToString("dd MMM, yyyy"),
                    OutletAddress = outlet != null ? outlet.Address : "",
                    OrderStatus = orderTable != null ? orderTable.OrderStatus : "",
                    WorkshopName = orderTable.WorkshopID != null ? orderTable.WorkShop.WorkShopName : "",
                    WorkshopCode = workshopUser != null ? workshopUser.ConsPartyCode : ""
                };

                responseOrderModel.Count = orderTable.OrderDetails.Count;

                if (orderTable.OrderDetails.Count > 0)
                {
                    var general = new General();
                    var rodModels = new List<ResponseOrderDetailModel>();
                    foreach (var od in orderTable.OrderDetails)
                    {
                        var responseOrderDetailModel = new ResponseOrderDetailModel
                        {
                            OrderDetailID = od.OrderDetailID,
                            OrderID = od.OrderID,
                            ProductID = od.ProductID,
                            ProductName = od.ProductName,
                            ImagePath = general.CheckImageUrl(od.ImagePath ?? ""),
                            Qty = od.Qty,
                            Quantity = od.Qty,
                            UnitPrice = od.UnitPrice,
                            TotalPrice = od.TotalPrice,
                            AvailabilityType = od.AvailabilityType,
                            OutletId = od.OutletId,
                            PartNumber = od.PartNumber,
                            Brand = od.BrandName,
                            OutletName = od.Outlet != null ? od.Outlet.OutletName : ""

                        };

                        rodModels.Add(responseOrderDetailModel);
                    }
                    responseOrderModel.OrderDetails = rodModels;
                }

                if (orderTable.DeliveryAddressId != null)
                {
                    var da = db.DeliveryAddresses.Where(d => d.DeliveryAddressId == orderTable.DeliveryAddressId).FirstOrDefault();
                    if (da != null)
                    {
                        responseOrderModel.DelAddress = new DeliveryAddress
                        {
                            FirstName = da.FirstName,
                            LastName = da.LastName,
                            MobileNo = da.MobileNo,
                            Address = da.Address,
                            City = da.City,
                            PinCode = da.PinCode
                        };
                    }
                }
            }
            return responseOrderModel;
        }
        #endregion

        #region Get Order status by orderId
        public ResponseOrderStatus GetOrderStatusByOrderId(int orderId)
        {
            ResponseOrderStatus responseOrderModel = null;
            var orderData = db.OrderTables.Where(x => x.OrderID == orderId).FirstOrDefault();
            if (orderData != null)
            {
                responseOrderModel = new ResponseOrderStatus();
                responseOrderModel.Status = orderData.OrderStatus;
            }
            return responseOrderModel;
        }
        #endregion

        #region Update order status
        public bool UpdateOrderStatus(List<OrderTable> orders)
        {
            List<ResponseOrderModel> lstorderstatus = new List<ResponseOrderModel>();
            foreach (var item in orders)
            {
                var order = db.OrderTables.Where(o => o.OrderID == item.OrderID).FirstOrDefault();
                if (order == null) continue;
                if (order.OrderStatus != item.OrderStatus)
                {
                    ResponseOrderModel responseOrderModel = new ResponseOrderModel
                    {
                        OrderID = order.OrderID,
                        OrderNo = order.OrderNo,
                        UserID = order.UserID,
                        OrderStatus = item.OrderStatus
                    };
                    lstorderstatus.Add(responseOrderModel);
                }
                order.OrderStatus = item.OrderStatus;

            }
            if (lstorderstatus.Any())
            {
                SystemController sc = new SystemController();
                sc.SentmailonStatus(lstorderstatus);
            }
            return db.SaveChanges() > 0;
        }
        #endregion

        #region Get Order details by orderId
        public List<OrderPartsDetailModel> GetOrderPartsByOrderId(GetOrderModel model)
        {
            var orderParts = new List<OrderPartsDetailModel>();
            var orderTable = db.OrderTables.Where(x => x.UserID == model.UserId && x.OrderID == model.OrderId).FirstOrDefault();
            if (orderTable != null)
            {
                if (orderTable.OrderDetails.Count > 0)
                {
                    var workshopId = db.DistributorWorkShops.AsNoTracking().FirstOrDefault(dw => dw.UserId == orderTable.UserID)?.WorkShopId;
                    if (workshopId == null)
                    {
                        workshopId = orderTable.WorkshopID;
                    }
                    var workshopName = db.WorkShops.AsNoTracking().FirstOrDefault(w => w.WorkShopId == workshopId)?.WorkShopName;

                    foreach (var od in orderTable.OrderDetails)
                    {
                        var responseOrderDetailModel = new OrderPartsDetailModel
                        {
                            OrderNumber = orderTable.OrderNo,
                            Quantity = od.Qty,
                            PartNumber = od.PartNumber,
                            CustomerName = workshopName != null ? workshopName : ""
                        };
                        orderParts.Add(responseOrderDetailModel);
                    }
                }
            }
            return orderParts;
        }
        #endregion

        #endregion

        /// <summary>
        /// Get product based on part number.
        /// </summary>
        /// <param name="distributorId">The distributor id of the distributor.</param>
        /// <param name="partNumber">The part number of the product.</param>
        /// <returns>Return product.</returns>
        public Product GetProduct(int? distributorId, string partNumber, string Role)
        {
            return db.Products.Where(p => p.DistributorId == (Role == Constants.SuperAdmin ? p.DistributorId : distributorId) && p.PartNo == partNumber).FirstOrDefault();
        }

        #region Get and Save Stock Color
        /// <summary>
        /// Get stock color model.
        /// </summary>
        /// <returns>Return instance of StockColorModel.</returns>
        public StockColorModel GetStockColorModel()
        {
            var scm = new StockColorModel();
            if (db.StockColors.Any())
            {
                var lowStock = db.StockColors.Where(s => s.StockType == "Low").FirstOrDefault();
                if (lowStock != null)
                {
                    scm.LowStockQty = lowStock.Qty;
                    scm.LowStockColor = lowStock.Color;
                    scm.LowStockTag = lowStock.Tag;
                }

                var mediumStock = db.StockColors.Where(s => s.StockType == "Medium").FirstOrDefault();
                if (mediumStock != null)
                {
                    scm.MediumStockQty = mediumStock.Qty;
                    scm.MediumStockColor = mediumStock.Color;
                    scm.MediumStockTag = mediumStock.Tag;
                }

                var highStock = db.StockColors.Where(s => s.StockType == "High").FirstOrDefault();
                if (highStock != null)
                {
                    scm.HighStockColor = highStock.Color;
                    scm.HighStockTag = highStock.Tag;
                }
            }

            return scm;
        }

        /// <summary>
        /// Save or update the stock color.
        /// </summary>
        /// <param name="scm">The object of StockColorModel.</param>
        /// <returns>Return true if save or updated else false.</returns>
        public bool SaveOrUpdateStockColor(StockColorModel scm)
        {
            if (db.StockColors.Any())
            {
                var lowStock = db.StockColors.Where(s => s.StockType == "Low").FirstOrDefault();
                if (lowStock != null)
                {
                    lowStock.Qty = scm.LowStockQty;
                    lowStock.Color = scm.LowStockColor;
                    lowStock.Tag = scm.LowStockTag;
                }

                var mediumStock = db.StockColors.Where(s => s.StockType == "Medium").FirstOrDefault();
                if (mediumStock != null)
                {
                    mediumStock.Qty = scm.MediumStockQty;
                    mediumStock.Color = scm.MediumStockColor;
                    mediumStock.Tag = scm.MediumStockTag;
                }

                var highStock = db.StockColors.Where(s => s.StockType == "High").FirstOrDefault();
                if (highStock != null)
                {
                    highStock.Color = scm.HighStockColor;
                    highStock.Tag = scm.HighStockTag;
                }
            }
            else
            {
                var scList = new List<StockColor>();
                var lowSc = new StockColor
                {
                    StockType = "Low",
                    Qty = scm.LowStockQty,
                    Color = scm.LowStockColor,
                    Tag = scm.LowStockTag
                };
                var medSc = new StockColor
                {
                    StockType = "Medium",
                    Qty = scm.MediumStockQty,
                    Color = scm.MediumStockColor,
                    Tag = scm.MediumStockTag
                };
                var highSc = new StockColor
                {
                    StockType = "High",
                    Color = scm.HighStockColor,
                    Tag = scm.HighStockTag
                };

                scList.Add(lowSc);
                scList.Add(medSc);
                scList.Add(highSc);

                db.StockColors.AddRange(scList);
            }

            return db.SaveChanges() > 0;
        }
        #endregion

        #region ProductAvailablityType
        public List<ProductAvailableTypeModel> ProductAvailablityType()
        {
            var data = db.ProductAvailabilityTypes.Where(x => x.IsActive == true).Select(s => new ProductAvailableTypeModel()
            {
                Id = s.Id,
                IsActive = s.IsActive,
                Text = s.Text
            }).ToList();
            return data;
        }
        #endregion

        #region SaveProductAvailablityType
        public bool SaveProductAvailablityType(List<ProductAvailablityRequest> lstModel)
        {
            foreach (var item in lstModel)
            {
                var data = db.TempOrderDetails.Where(x => x.TempOrderId == item.TempOrderId && x.ProductId == item.ProductId).FirstOrDefault();
                if (data != null)
                {
                    data.AvailabilityType = item.AvailablityTypeId;
                    db.SaveChanges();
                }
            }
            return true;
        }
        #endregion

        #region Get All Order's
        public List<ResponseOrderModel> GetallOrders(string UserType, int Id)
        {
            List<ResponseOrderModel> lstResponseOrderModels = new List<ResponseOrderModel>();
            var orderData = new List<OrderTable>();
            if (!string.IsNullOrEmpty(UserType))
            {
                if (string.Equals(UserType, "distributor", StringComparison.OrdinalIgnoreCase))
                {
                    orderData = db.OrderTables.Where(x => x.DistributorId == Id).OrderByDescending(o => o.OrderDate
                    ).ToList();
                }
                else if (string.Equals(UserType, "workshop", StringComparison.OrdinalIgnoreCase))
                {
                    orderData = db.OrderTables.Where(x => x.WorkshopID == Id).OrderByDescending(o => o.OrderDate
                    ).ToList();
                }
                else if (string.Equals(UserType, "outlet", StringComparison.OrdinalIgnoreCase))
                {
                    orderData = db.OrderTables.Where(x => x.OutletId == Id).OrderByDescending(o => o.OrderDate
                    ).ToList();
                }
                else
                {
                    orderData = db.OrderTables.OrderByDescending(o => o.OrderDate
                    ).ToList();
                }
            }
            //orderData = db.OrderTables.ToList();
            if (orderData.Count > 0)
            {
                foreach (var item in orderData)
                {
                    var salesExecutiveName = "";
                    var workshopCode = "";
                    if (item.WorkshopID != null)
                    {
                        var user = (from u in db.UserDetails
                                    join w in db.SalesExecutiveWorkshops on u.UserId equals w.UserId
                                    where w.WorkshopId == item.WorkshopID
                                    select u).FirstOrDefault();
                        salesExecutiveName = user != null ? user.FirstName + " " + user.LastName : "";

                        var workshopUser = (from a in db.UserDetails
                                            join d in db.DistributorWorkShops on a.UserId equals d.UserId
                                            where d.WorkShopId == item.WorkshopID
                                            select a).FirstOrDefault();
                        workshopCode = workshopUser != null ? workshopUser.ConsPartyCode : "";
                    }
                    ResponseOrderModel responseOrderModel = new ResponseOrderModel
                    {
                        OrderID = item.OrderID,
                        OrderNo = item.OrderNo,
                        WorkshopID = item.WorkshopID,
                        DeliveryAddressId = item.DeliveryAddressId,
                        PaymentMethod = item.PaymentMethod,
                        SubTotal = item.SubTotal,
                        Discount = item.Discount,
                        DiscountCode = item.DiscountCode,
                        DeliveryCharge = item.DeliveryCharge,
                        PackingCharge = item.PackingCharge,
                        UserID = item.UserID,
                        OrderTotal = item.OrderTotal,
                        OrderDate = item.OrderDate,
                        OrderStatus = item.OrderStatus,
                        OutletName = item.OutletId != null ? item.Outlet.OutletName : "",
                        OutletCode = item.OutletId != null ? item.Outlet.OutletCode : "",
                        SalesExecutiveName = salesExecutiveName,
                        WorkshopName = item.WorkshopID != null ? item.WorkShop.WorkShopName : "",
                        WorkshopCode = workshopCode
                    };
                    var t = responseOrderModel;
                    if (item.OrderDetails.Count > 0)
                    {
                        var general = new General();
                        var lstResponseOrderDetailModel = new List<ResponseOrderDetailModel>();
                        foreach (var od in item.OrderDetails)
                        {
                            var responseOrderDetailModel = new ResponseOrderDetailModel()
                            {
                                OrderDetailID = od.OrderDetailID,
                                OrderID = od.OrderID,
                                ProductID = od.ProductID,
                                ProductName = od.ProductName,
                                PartNumber = od.PartNumber,
                                Brand = od.BrandName,
                                ImagePath = general.CheckImageUrl(od.ImagePath ?? ""),
                                Qty = od.Qty,
                                UnitPrice = od.UnitPrice,
                                TotalPrice = od.TotalPrice,
                            };

                            lstResponseOrderDetailModel.Add(responseOrderDetailModel);
                        }
                        responseOrderModel.OrderDetails = lstResponseOrderDetailModel;
                    }
                    responseOrderModel.Count = (responseOrderModel.OrderDetails != null ? responseOrderModel.OrderDetails.Count : 0);
                    lstResponseOrderModels.Add(responseOrderModel);
                }
            }
            return lstResponseOrderModels;
        }
        #endregion

        #region Get GetCoupon's 
        public List<ResponseCoupons> GetCoupons(CouponRequest model, out int totalRecord)
        {
            int workshopID = 0;
            if (!string.IsNullOrEmpty(model.UserId))
            {
                workshopID = model.UserId.GetWorkshopId(model.Role);
            }
            totalRecord = 0;

            List<ResponseCoupons> coupons = new List<ResponseCoupons>();
            if (workshopID > 0 || model.Role == Constants.SuperAdmin || model.Role == Constants.Distributor)
            {
                var data = db.Coupons.Where(x => x.SchemeId == model.SchemeId && x.WorkshopId == (workshopID > 0 ? workshopID : x.WorkshopId)).ToList();
                foreach (var item in data)
                {
                    var coupon = new ResponseCoupons();
                    coupon.CouponId = item.CouponId;
                    coupon.CouponNumber = item.CouponNumber;
                    coupon.SchemeId = item.SchemeId;
                    coupon.WorkshopId = item.WorkshopId;
                    var couponGift = (from g in db.GiftManagements
                                      join gc in db.GiftsCoupons on g.GiftId equals gc.GiftId
                                      where gc.CouponNumber.Trim().ToLower() == item.CouponNumber.Trim().ToLower()
                                      select g
                                     ).FirstOrDefault();
                    if (couponGift != null)
                    {
                        coupon.IsGifted = true;
                        coupon.GiftName = couponGift.Gift;
                    }
                    coupons.Add(coupon);
                }
                totalRecord = coupons.Any() ? coupons.Count() : 0;
                if (model.PageNumber > 0)
                {
                    return coupons.GetPaging<ResponseCoupons>(model.PageNumber, model.PageSize);
                }
            }
            return coupons;
        }
        #endregion

        #region Update Product Qty
        public bool SetTotalPrice(int OrderId)
        {
            var orderDetail = db.OrderDetails.Where(x => x.OrderID == OrderId);
            var OrderData = db.OrderTables.Where(x => x.OrderID == OrderId).FirstOrDefault();
            if (OrderData != null)
            {
                var deliverycharge = OrderData.DeliveryCharge != null ? OrderData.DeliveryCharge : 0;
                var discount = (OrderData.Discount != null ? OrderData.Discount : 0);
                var PackingCharge = OrderData.PackingCharge != null ? OrderData.PackingCharge : 0;
                OrderData.SubTotal = orderDetail != null ? orderDetail.Sum(x => x.TotalPrice) : 0;
                OrderData.OrderTotal = (OrderData.SubTotal + deliverycharge + PackingCharge) - discount;
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Updateproductqty(List<OrderDetail> orderdetails)
        {
            var Orderid = 0;
            foreach (var item in orderdetails)
            {
                var order = db.OrderDetails.Where(o => o.OrderDetailID == item.OrderDetailID).FirstOrDefault();
                if (order == null) continue;
                Orderid = order.OrderID;
                var unitprice = (order.UnitPrice != null ? order.UnitPrice : 0);
                order.Qty = item.Qty;
                order.TotalPrice = item.Qty * unitprice;
            }
            db.SaveChanges();
            return SetTotalPrice(Orderid);
        }
        #endregion

        #region Delete Product from order
        public bool DeleteProduct(int OrderDetailID)
        {
            var removedata = db.OrderDetails.Where(x => x.OrderDetailID == OrderDetailID).FirstOrDefault();
            if (removedata != null)
            {
                var orderid = removedata.OrderID;
                db.OrderDetails.Remove(removedata);
                db.SaveChanges();
                var remaindata = db.OrderDetails.Where(x => x.OrderID == orderid).FirstOrDefault();
                if (remaindata != null)
                {
                    return SetTotalPrice(removedata.OrderID);
                }
                else
                {
                    var orderdata = db.OrderTables.Where(x => x.OrderID == orderid).FirstOrDefault();
                    db.OrderTables.Remove(orderdata);
                    return true;
                }

            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Get availability types
        /// <summary>
        /// Get list of product availability types that are active.
        /// </summary>        
        public List<string> GetAvailabilityTypes()
        {
            return db.ProductAvailabilityTypes.Where(p => p.IsActive).Select(s => s.Text).ToList();
        }
        #endregion

        #region Get outlets as per order       

        /// <summary>
        /// Get all outlets based on distributor id from particular order.
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>Return select list of outlets.</returns>
        public SelectList OutletsSelectList(int orderId, string userId)
        {
            var distOutlets = new List<Outlet>();
            var orderTable = db.OrderTables.Where(x => x.UserID == userId && x.OrderID == orderId).FirstOrDefault();
            if (orderTable != null)
            {
                distOutlets = db.DistributorsOutlets.Where(d => d.DistributorId == orderTable.DistributorId).Select(o => o.Outlet).ToList();
            }

            // Create select list
            var selListItem = new SelectListItem() { Value = "", Text = "-- Select Outlet --" };
            var newList = new List<SelectListItem> { selListItem };

            foreach (var distO in distOutlets)
            {
                newList.Add(new SelectListItem() { Value = distO.OutletId.ToString(), Text = distO.OutletName });
            }
            return new SelectList(newList, "Value", "Text", null);
        }

        #endregion

        #region Generate orders per outlet
        public void GenerateOrderPerOutlet(ResponseOrderModel model)
        {
            var order = db.OrderTables.Where(o => o.OrderID == model.OrderID).FirstOrDefault();

            foreach (var odGroup in model.OrderDetails.GroupBy(o => o.OutletId))
            {
                var outletId = odGroup.Key;
                var newOrder = new OrderTable
                {
                    OrderNo = Utils.Randomint(),
                    WorkshopID = order.WorkshopID,
                    DeliveryAddressId = order.DeliveryAddressId,
                    PaymentMethod = order.PaymentMethod,
                    Discount = order.Discount,
                    DiscountCode = order.DiscountCode,
                    DeliveryCharge = order.DeliveryCharge,
                    PackingCharge = order.PackingCharge,
                    UserID = order.UserID,
                    OrderDate = DateTime.Now,
                    OrderStatus = OrderStatus.Pending.ToString(),
                    DistributorId = order.DistributorId,
                    OutletId = outletId
                };

                var orderDetailList = new List<OrderDetail>();
                foreach (ResponseOrderDetailModel rodModel in odGroup)
                {
                    if (rodModel.OutletId == null)
                    {
                        var orderDetail = db.OrderDetails.Where(o => o.OrderDetailID == rodModel.OrderDetailID).FirstOrDefault();
                        if (orderDetail != null)
                        {
                            db.OrderDetails.Remove(orderDetail);

                            var consPartyCode = db.UserDetails.Where(u => u.UserId == order.UserID).FirstOrDefault()?.ConsPartyCode;
                            var product = db.Products.Where(p => p.ProductId == orderDetail.ProductID).FirstOrDefault();

                            // Adjust existing order's SubTotal and OrderTotal
                            var oDeliveryCharge = order.DeliveryCharge ?? 0;
                            var oPackingCharge = order.PackingCharge ?? 0;
                            var oDiscount = order.Discount ?? 0;
                            order.SubTotal -= orderDetail.TotalPrice;
                            order.OrderTotal = order.SubTotal + oDeliveryCharge + oPackingCharge - oDiscount;

                            var cbo = new CustomerBackOrder
                            {
                                CONo = order.OrderNo,
                                CODate = order.OrderDate,
                                PartyType = string.Empty,
                                PartyCode = consPartyCode,
                                PartyName = string.Empty,
                                CustomerOrderType = string.Empty,
                                PartStatus = order.OrderStatus,
                                PartNum = orderDetail.PartNumber,
                                PartDesc = string.Empty,
                                BinLoc = string.Empty,
                                OrderedQty = orderDetail.Qty?.ToString(),
                                ProcessedQty = string.Empty,
                                PendingOrCancelledQty = string.Empty,
                                StockQty = product?.CurrentStock?.ToString(),
                                SellingPrice = orderDetail.UnitPrice != null ? Convert.ToString(orderDetail.UnitPrice) : string.Empty,
                                DistributorId = order.DistributorId,
                                WorkshopId = order.WorkshopID
                            };
                            db.CustomerBackOrders.Add(cbo);

                            db.SaveChanges();
                        }

                    }
                    else if (rodModel.OutletId != order.OutletId)
                    {
                        var orderDetails = db.OrderDetails.Where(o => o.OrderDetailID == rodModel.OrderDetailID && o.OrderID == order.OrderID);
                        db.OrderDetails.RemoveRange(orderDetails);
                        db.SaveChanges();

                        orderDetailList.Add(new OrderDetail
                        {
                            ProductID = rodModel.ProductID,
                            ProductName = rodModel.ProductName,
                            UnitPrice = rodModel.UnitPrice,
                            Qty = rodModel.Qty,
                            TotalPrice = rodModel.TotalPrice,
                            ImagePath = rodModel.ImagePath,
                            OutletId = outletId,
                            AvailabilityType = rodModel.AvailabilityType
                        });
                    }
                    else if (rodModel.OutletId == order.OutletId)
                    {
                        // First OrderDetail's OutletId was different
                        // Now changed to same of Order's OutletId
                        var orderDetail = db.OrderDetails.Where(o => o.OrderDetailID == rodModel.OrderDetailID).FirstOrDefault();
                        if (orderDetail != null)
                        {
                            orderDetail.OutletId = order.OutletId;
                            orderDetail.AvailabilityType = rodModel.AvailabilityType;
                            db.SaveChanges();
                        }
                    }
                }

                if (orderDetailList.Count > 0)
                {
                    // Calculate SubTotal and OrderTotal
                    var deliverycharge = newOrder.DeliveryCharge ?? 0;
                    var packingCharge = newOrder.PackingCharge ?? 0;
                    var discount = newOrder.Discount ?? 0;
                    newOrder.SubTotal = orderDetailList.Sum(x => x.TotalPrice);
                    newOrder.OrderTotal = newOrder.SubTotal + deliverycharge + packingCharge - discount;

                    // Adjust existing order's SubTotal and OrderTotal
                    var oDeliveryCharge = order.DeliveryCharge ?? 0;
                    var oPackingCharge = order.PackingCharge ?? 0;
                    var oDiscount = order.Discount ?? 0;
                    order.SubTotal -= newOrder.SubTotal;
                    order.OrderTotal = order.SubTotal + oDeliveryCharge + oPackingCharge - oDiscount;

                    db.OrderTables.Add(newOrder);
                    db.SaveChanges();

                    orderDetailList.ForEach(o => o.OrderID = newOrder.OrderID);
                    db.OrderDetails.AddRange(orderDetailList);
                    db.SaveChanges();
                }

                // Remove order if no order detail left for current order
                if (order.OrderDetails.Count == 0)
                {
                    db.OrderTables.Remove(order);
                    db.SaveChanges();
                }
            }
        }
        #endregion

        #region Cancel Order By OrderId
        public bool CancelOrderById(int OrderId, out string OrderNo)
        {
            var Ordertable = db.OrderTables.Where(x => x.OrderID == OrderId && x.OrderStatus != OrderStatus.Cancelled.ToString()).FirstOrDefault();
            if (Ordertable != null)
            {
                OrderNo = Ordertable.OrderNo;
                Ordertable.OrderStatus = OrderStatus.Cancelled.ToString();
                Ordertable.CancelDate = DateTime.Now;
                db.SaveChanges();
                return true;
            }
            else
            {
                OrderNo = "";
                return false;
            }
        }

        #endregion

        #region Get OrderUserId by orderId
        public string GetOrderUserByOrderId(int orderId)
        {
            string UserId = db.OrderTables.Where(x => x.OrderID == orderId).Select(x => x.UserID).FirstOrDefault();
            return UserId;
        }
        #endregion

        #region Get GetStockColor
        public List<StockColorResponse> GetStockColor()
        {
            return db.StockColors.Select(s => new StockColorResponse()
            {
                StockId = s.StockId,
                color = s.Color,
                Tag = s.Tag
            }).ToList();
        }
        #endregion

        public RazorPayResponse CreateRazorPayOrderId(string tempOrderId)
        {
            string key = ConfigurationManager.AppSettings["Key"].ToString(); //"rzp_test_0WA175Nk0AoV5M";
            string secret = ConfigurationManager.AppSettings["SecretKey"].ToString(); //"SS2f7fqFOOKwPhnDIJqNkWA5";
            int.TryParse(tempOrderId, out int toid);
            int orderId = toid;
            var tempOrder = db.TempOrders.Where(x => x.TempOrderId == orderId).FirstOrDefault();
            if (tempOrder != null)
            {
                Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(key, secret);
                Dictionary<string, object> input = new Dictionary<string, object>();
                var grandTotal = tempOrder?.GrandTotal != null ? (tempOrder?.GrandTotal.Value * 100) : 0;
                input.Add("amount", grandTotal); // this amount should be same as transaction amount
                input.Add("currency", "INR");
                input.Add("receipt", "12121");
                input.Add("payment_capture", 1);
                Razorpay.Api.Order order = client.Order.Create(input);

                var rp = new RazorPayResponse();
                rp.Key = key;
                rp.SecreyKey = secret;
                rp.Currency = "INR";
                rp.OrderId = order["id"].ToString();
                rp.Amount = tempOrder.GrandTotal != null ? tempOrder.GrandTotal.Value : 0;
                rp.Image = "https://garaaz.com/assets/images/logo.png";
                var userDetail = db.UserDetails.Where(x => x.UserId == tempOrder.UserId).FirstOrDefault();
                if (userDetail != null)
                {
                    rp.Name = "Garaaz";
                    rp.Prefill_Name = $"{userDetail.FirstName} {userDetail.LastName}";
                    rp.Prefill_Email = userDetail.AspNetUser.Email;
                    rp.Prefill_Contact = userDetail.AspNetUser.UserName;
                    rp.Address = userDetail.Address;
                    rp.Description = "";
                }
                return rp;
            }
            else
            {
                return new RazorPayResponse();
            }
        }

        public RazorPayResponse CreateRazorPayOrderId(decimal amount, string userId)
        {
            string key = ConfigurationManager.AppSettings["Key"].ToString(); //"rzp_test_0WA175Nk0AoV5M";
            string secret = ConfigurationManager.AppSettings["SecretKey"].ToString(); //"SS2f7fqFOOKwPhnDIJqNkWA5";
            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(key, secret);
            Dictionary<string, object> input = new Dictionary<string, object>();
            var grandTotal = amount * 100;
            input.Add("amount", grandTotal); // this amount should be same as transaction amount
            input.Add("currency", "INR");
            input.Add("receipt", "12121");
            input.Add("payment_capture", 1);
            Razorpay.Api.Order order = client.Order.Create(input);

            var rp = new RazorPayResponse();
            rp.Key = key;
            rp.SecreyKey = secret;
            rp.Currency = "INR";
            rp.OrderId = order["id"].ToString();
            rp.Amount = amount;
            var userDetail = db.UserDetails.Where(x => x.UserId == userId).FirstOrDefault();
            if (userDetail != null)
            {
                rp.Name = "Garaaz";
                rp.Prefill_Name = $"{userDetail.FirstName} {userDetail.LastName}";
                rp.Prefill_Email = userDetail.AspNetUser.Email;
                rp.Prefill_Contact = userDetail.AspNetUser.UserName;
                rp.Address = userDetail.Address;
                rp.Description = "";
                rp.Image = "http://garaaz.com/assets/images/logo.png";
            }
            return rp;

        }

        public bool OutstandingPayment(decimal amount, string userId, string paymentId, string role, List<ApplicationUser> superAdmins)
        {
            int wsid = userId.GetWorkshopId(role);
            var ws = db.WorkShops.Where(x => x.WorkShopId == wsid).FirstOrDefault();
            if (ws != null)
            {
                ws.TotalOutstanding = ws.TotalOutstanding != null ? ws.TotalOutstanding.Value : 0;
                if (ws.TotalOutstanding.Value >= amount)
                {
                    ws.TotalOutstanding = ws.TotalOutstanding.Value - amount;
                    db.SaveChanges();

                    Log log = new Log();
                    log.CreatedDate = DateTime.Now;
                    log.Status = string.Empty;
                    log.UserId = userId;
                    log.Details = string.Empty;
                    log.OutstandingAmount = amount;
                    log.PaymentId = paymentId;
                    db.Logs.Add(log);
                    bool status = db.SaveChanges() > 0;

                    // Save Notifications for Super Admin
                    if (superAdmins.Any())
                    {
                        foreach (var sa in superAdmins)
                        {
                            var notification = new Notification()
                            {
                                UserId = sa.Id.ToString(),
                                Type = Constants.OutstandingPayment,
                                Message = $"{ws.WorkShopName} has clear Rs{amount} outstanding payment from App.",
                                IsRead = false,
                                CreatedDate = DateTime.Now,
                                RefUserId = userId,//userId,
                                WorkshopId = ws.WorkShopId,
                            };
                            db.Notifications.Add(notification);
                            db.SaveChanges();
                        }
                    }
                    return status;
                }
            }
            return false;
        }

        public bool AddTempOrderId(int temporderid, string userid)
        {
            bool result = false;
            var userdetail = db.UserDetails.Where(u => u.UserId == userid).FirstOrDefault();
            if (userdetail != null)
            {
                userdetail.TempOrderId = temporderid;
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public bool RemoveTempOrderId(int temporderid)
        {
            var userdetail = db.UserDetails.FirstOrDefault(u => u.TempOrderId == temporderid);
            if (userdetail == null) return false;

            userdetail.TempOrderId = null;
            db.SaveChanges();
            return true;
        }

        #region Get Confirm Or Cancelled BackOrder
        public List<CustomerBackOrder> GetConfirmOrCancelBackOrderByRole(string userId, string Role)
        {
            string cancel = "Cancel"; string confirm = "Accept";
            var orders = new List<CustomerBackOrder>();
            if (Role == Constants.SuperAdmin)
            {
                orders = db.CustomerBackOrders.Where(o => o.PartStatus == cancel || o.PartStatus == confirm).OrderByDescending(d => d.CODate).ToList();
            }
            else
            {
                var user = new RepoUsers();
                int distributorId = user.getDistributorIdByUserId(userId, Role);
                orders = db.CustomerBackOrders.Where(o => (o.PartStatus == cancel || o.PartStatus == confirm) && o.DistributorId == distributorId).OrderByDescending(d => d.CODate).ToList();
            }
            return orders;
        }
        #endregion
    }

    /// <summary>
    /// The values for order status.
    /// </summary>
    public enum OrderStatus
    {
        [Description("Pending")]
        Pending,
        [Description("In Review")]
        InReview,
        [Description("Confirmed")]
        Confirmed,
        [Description("In process")]
        InProcess,
        [Description("Out for delivery")]
        OutForDelivery,
        [Description("Delivered")]
        Delivered,
        [Description("Back Order")]
        BackOrder,
        [Description("Cancelled")]
        Cancelled,
        [Description("Completed")]
        Completed
    }



}