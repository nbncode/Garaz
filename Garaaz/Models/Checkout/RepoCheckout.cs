using System;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Garaaz.Models.Checkout
{
    public class RepoCheckout
    {
        garaazEntities db = new garaazEntities();

        public CheckoutDeliveryAddressModel GetDeliveryAddress(GetCartModel model)
        {
            if (model.UserId == null) { model.UserId = General.GetUserId(); }
            CheckoutDeliveryAddressModel cdaModel = null;
            var address = db.DeliveryAddresses.Where(x => x.UserId == model.UserId).OrderByDescending(o => o.DeliveryAddressId).FirstOrDefault();
            if (address != null)
            {
                cdaModel = new CheckoutDeliveryAddressModel
                {
                    TempOrderId = 0,
                    UserId = address.UserId,
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    MobileNumber = address.MobileNo,
                    Address = address.Address,
                    City = address.City,
                    Pincode = address.PinCode,
                    State = address.state
                };
            }
            else
            {
                int workshopID = 0;
                if (!string.IsNullOrEmpty(General.GetUserId()))
                {
                    workshopID = model.UserId.GetWorkshopId(General.GetUserRole());
                }
                var address1 = db.WorkShops.Where(x => x.WorkShopId == workshopID).FirstOrDefault();
                if (address1 != null)
                {
                    var userDetails = db.UserDetails.Where(a => a.UserId == model.UserId).FirstOrDefault();

                    cdaModel = new CheckoutDeliveryAddressModel
                    {
                        TempOrderId = 0,
                        UserId = model.UserId,
                        FirstName = userDetails.FirstName,
                        LastName = userDetails.LastName,
                        MobileNumber = userDetails.AspNetUser.UserName,
                        Address = address1.Address,
                        City = address1.City,
                        Pincode = address1.Pincode,
                        State = address1.State,
                    };
                }
            }
            return cdaModel;
        }

        public bool SaveDeliveryAddress(CheckoutDeliveryAddressModel model)
        {
            bool delAddSaved = false;
            var deliveryAddress = new DeliveryAddress
            {
                UserId = model.UserId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                MobileNo = model.MobileNumber,
                Address = model.Address,
                City = model.City,
                PinCode = model.Pincode,
                state = model.State
            };

            db.DeliveryAddresses.Add(deliveryAddress);
            delAddSaved = db.SaveChanges() > 0;

            var tempOrder = db.TempOrders.Where(o => o.TempOrderId == model.TempOrderId).FirstOrDefault();
            if (tempOrder != null)
            {
                tempOrder.DeliveryAddressId = deliveryAddress.DeliveryAddressId;
                delAddSaved = db.SaveChanges() > 0;
            }

            return delAddSaved;
        }

        public bool SavePaymentMethod(PaymentMethodModel model)
        {
            var isSaved = false;
            var tempOrder = db.TempOrders.Where(o => o.TempOrderId == model.TempOrderId).FirstOrDefault();
            if (tempOrder != null)
            {
                tempOrder.PaymentMethod = model.PaymentMethod;
                db.SaveChanges();
                return true;

            }

            return isSaved;
        }

        #region Apply PromoCode
        public bool ApplyPromoCode(PromoCodeModel model, out string Message)
        {
            Message = "";
            var isSaved = false;
            var workshopcoupon = db.WorkshopCoupons.Where(o => o.UserId == model.UserId && o.CouponNo == model.CouponNo).FirstOrDefault();
            if (workshopcoupon != null)
            {
                //o.UserId == model.UserId &&
                var tempOrder = db.TempOrders.Where(o => o.TempOrderId == model.TempOrderId).FirstOrDefault();
                if (tempOrder != null)
                {
                    var discount = (workshopcoupon.Amount != null ? workshopcoupon.Amount : 0);
                    if (discount > tempOrder.GrandTotal)
                    {
                        Message = "Coupon amount is greater then your cart amount. Please add more items in your cart to apply.";
                    }
                    else
                    {
                        tempOrder.GrandTotal = tempOrder.GrandTotal - discount;
                        tempOrder.Discount = discount;
                        tempOrder.DiscountCode = workshopcoupon.CouponNo;
                        db.SaveChanges();
                        isSaved = true;
                    }
                }
                else
                {
                    Message = "Invalid Promocode";
                }
            }
            else
            {
                Message = "Invalid Promocode";
            }
            return isSaved;
        }
        #endregion

        #region Remove PromoCode
        public bool RemovePromoCode(PromoCodeModel model)
        {
            var isSaved = false;
            //o.UserId == model.UserId &&
            var tempOrder = db.TempOrders.Where(o => o.TempOrderId == model.TempOrderId && o.DiscountCode == model.CouponNo).FirstOrDefault();
            if (tempOrder != null)
            {
                var discount = (tempOrder.Discount != null ? tempOrder.Discount : 0);
                tempOrder.GrandTotal = tempOrder.GrandTotal + discount;
                tempOrder.DiscountCode = null;
                tempOrder.Discount = null;
                db.SaveChanges();
                isSaved = true;
            }

            return isSaved;
        }
        #endregion

        public string HmacSha256Digest(string roId, string rpId)
        {
            string secret = ConfigurationManager.AppSettings["SecretKey"].ToString();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes($"{roId}|{rpId}");
            System.Security.Cryptography.HMACSHA256 cryptographer = new System.Security.Cryptography.HMACSHA256(keyBytes);

            byte[] bytes = cryptographer.ComputeHash(messageBytes);

            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

}