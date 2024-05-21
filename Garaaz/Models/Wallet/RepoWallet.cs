using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Garaaz.Models
{
    public class RepoWallet
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General general = new General();

        // In order - Gold, Orange, Green, Deep sky blue, hot pink, dim gray, tan, red, dark khaki and teal
        #endregion


        public List<WalletResponse> GetWorkShopWalletList(WalletRequest model)
        {
            try
            {
                SqlParameter[] param = {
                                           new SqlParameter("@QueryType","WalletDetail"),
                                           new SqlParameter("@Index",model.PageNumber),
                                           new SqlParameter("@CouponNumber",model.CouponNumber),
                                           new SqlParameter("@DistributorId",model.DistributorId),
                                       };

                DataSet ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "sp_wallet", param);
                List<WalletResponse> wrList = new List<WalletResponse>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    var datatable = ds.Tables[0];
                    foreach (DataRow item in datatable.Rows)
                    {

                        var wr = new WalletResponse();
                        wr.WorkShopId = item["WorkShopId"] != DBNull.Value ? Convert.ToInt32(item["WorkShopId"]) : 0;
                        wr.WorkShop = item["workshopName"] != DBNull.Value ? Convert.ToString(item["workshopName"]) : "";
                        wr.UserId = item["UserId"] != DBNull.Value ? Convert.ToString(item["UserId"]) : "";
                        wr.WalletAmount = item["WalletAmt"] != DBNull.Value ? Convert.ToDecimal(item["WalletAmt"]) : 0;
                        wr.Sign = Constants.RupeeSign;
                        wr.TotalCoupon = item["TotalCoupons"] != DBNull.Value ? Convert.ToInt32(item["TotalCoupons"]) : 0;
                        wrList.Add(wr);
                    }
                }
                return wrList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool AddMoneyInWallet(AddMoneyRequest model)
        {
            bool status = false;
            var userDetail = db.UserDetails.Where(x => x.UserId == model.UserId).FirstOrDefault();
            if (userDetail != null)
            {
                var curAmt = userDetail.WalletAmount != null ? userDetail.WalletAmount.Value : 0;
                if (model.Type.Equals("Cr", StringComparison.OrdinalIgnoreCase))
                {
                    userDetail.WalletAmount = curAmt + model.WalletAmount;
                    db.SaveChanges();
                }
                else
                {
                    if (model.WalletAmount <= curAmt)
                    {
                        userDetail.WalletAmount = curAmt - model.WalletAmount;
                        db.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }

                var wt = new WalletTransaction()
                {
                    UserId = model.UserId,
                    WorkshopId = model.WorkShopId,
                    Type = model.Type,
                    Amount = model.WalletAmount,
                    Description = model.Description,
                    RefId = model.RefId,
                    CreatedDate = DateTime.Now
                };
                db.WalletTransactions.Add(wt);
                status = db.SaveChanges() > 0;
            }
            return status;
        }

        public List<WorkshopCouponResponse> GetWorkshopCoupons(WorkshopCouponRequest model, out int totalRecord, out decimal WsCurAmount)
        {
            var data = db.WorkshopCoupons.Where(x => x.UserId == model.UserId).OrderByDescending(o => o.CreatedDate);
            totalRecord = data.Count();
            WsCurAmount = db.UserDetails.Where(x => x.UserId == model.UserId).Select(s => s.WalletAmount != null ? s.WalletAmount.Value : 0).FirstOrDefault();

            var couponList = new List<WorkshopCouponResponse>();
            foreach (var item in data)
            {
                var wcr = new WorkshopCouponResponse();
                wcr.CouponId = item.Id;
                wcr.WorkshopId = item.WorkshopId != null ? item.WorkshopId.Value : 0;
                wcr.UserId = item.UserId;
                wcr.CouponNo = item.CouponNo;
                wcr.Amount = item.Amount != null ? item.Amount.Value : 0;
                wcr.Sign = Constants.RupeeSign;
                wcr.Date = item.CreatedDate.Value.ToString("dd MMM, yyyy");
                couponList.Add(wcr);
            }
            if (model.PageNumber > 0)
            {
                return couponList.GetPaging<WorkshopCouponResponse>(model.PageNumber, model.PageSize);
            }
            return couponList;
        }

        public bool GenerateCoupon(GenerateCouponRequest model)
        {
            bool status = false;
            var workshopId = model.UserId.GetWorkshopId(Constants.Workshop);
            var walletCurAmt = db.UserDetails.Where(x => x.UserId == model.UserId).FirstOrDefault();
            if (walletCurAmt != null)
            {
                status = (walletCurAmt.WalletAmount != null ? walletCurAmt.WalletAmount.Value : 0) >= model.CouponAmount;
                if (status)
                {
                    Random rnd = new Random();
                NewCoupon: var couponNumber = Utils.GenerateCoupon(6, rnd);
                    var existingCoupon = db.WorkshopCoupons.Where(c => c.UserId == model.UserId && c.CouponNo == couponNumber).FirstOrDefault();
                    if (existingCoupon != null)
                    {
                        goto NewCoupon;
                    }
                    else
                    {
                        var data = new WorkshopCoupon()
                        {
                            WorkshopId = workshopId,//model.WorkShopId,
                            UserId = model.UserId,
                            Amount = model.CouponAmount,
                            CouponNo = couponNumber,
                            CreatedDate = DateTime.Now
                        };
                        db.WorkshopCoupons.Add(data);
                        status = db.SaveChanges() > 0;
                        if (status)
                        {
                            walletCurAmt.WalletAmount = (walletCurAmt.WalletAmount != null ? walletCurAmt.WalletAmount.Value : 0) - data.Amount;
                            db.SaveChanges();

                            var wt = new WalletTransaction()
                            {
                                UserId = model.UserId,
                                WorkshopId = workshopId,//model.WorkShopId,
                                Type = "Dr",
                                Amount = model.CouponAmount,
                                Description = $"New coupon generated Price: {Constants.RupeeSign} {model.CouponAmount}, Coupon: {couponNumber}.",
                                RefId = "",
                                CreatedDate = DateTime.Now
                            };
                            db.WalletTransactions.Add(wt);
                            status = db.SaveChanges() > 0;
                        }
                    }
                }
            }
            return status;
        }

        public List<WsTransactionResponse> GetWorkshopTransaction(WsTransactionRequest model, out int totalRecord, out decimal WsCurAmount)
        {
            var data = (from wt in db.WalletTransactions
                        join w in db.WorkShops on wt.WorkshopId equals w.WorkShopId
                        where wt.UserId == model.UserId
                        select wt
                        ).OrderByDescending(o => o.CreatedDate);
            totalRecord = data.Count();
            WsCurAmount = db.UserDetails.Where(x => x.UserId == model.UserId).Select(s => s.WalletAmount != null ? s.WalletAmount.Value : 0).FirstOrDefault();
            List<WsTransactionResponse> lstTransaction = new List<WsTransactionResponse>();
            foreach (var item in data)
            {
                var transaction = new WsTransactionResponse();
                transaction.TransactionId = item.Id;
                transaction.WorkShopId = item.WorkshopId != null ? item.WorkshopId.Value : 0;
                if (item.Type.Equals("Cr", StringComparison.OrdinalIgnoreCase) || item.Type.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                    transaction.Type = "Credit";
                else if (item.Type.Equals("Dr", StringComparison.OrdinalIgnoreCase) || item.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase))
                    transaction.Type = "Debit";
                else if (item.Type.Equals("Redeem", StringComparison.OrdinalIgnoreCase))
                    transaction.Type = "Redeem";
                transaction.Description = item.Description ?? "";
                transaction.Sign = Constants.RupeeSign;
                transaction.Amount = item.Amount != null ? item.Amount.Value : 0;
                transaction.Date = item.CreatedDate?.ToString("dd MMM, yyyy");
                lstTransaction.Add(transaction);
            }
            if (model.PageNumber > 0)
            {
                return lstTransaction.GetPaging<WsTransactionResponse>(model.PageNumber, model.PageSize);
            }
            return lstTransaction;
        }

        public string SearchCouponNo(string couponNo)
        {
            string msg = string.Empty;
            var data = (from wc in db.WorkshopCoupons
                        join w in db.WorkShops on wc.WorkshopId equals w.WorkShopId
                        where wc.CouponNo == couponNo
                        select w.WorkShopName
                       ).FirstOrDefault();
            if (data != null)
            {
                msg = $"This coupon number '{couponNo}' exist in workshop '{data}'.";
            }
            return msg;
        }

        public bool RedeemCouponNo(string couponNo, out string msg)
        {
            bool status = false; msg = "";
            var data = (from wc in db.WorkshopCoupons
                        join w in db.WorkShops on wc.WorkshopId equals w.WorkShopId
                        where wc.CouponNo == couponNo
                        select wc
                       ).FirstOrDefault();
            if (data != null)
            {
                msg = $"Coupon number '{data.CouponNo}' is redeemed.";
                var wt = new WalletTransaction()
                {
                    UserId = data.UserId,
                    WorkshopId = data.WorkshopId,
                    Type = "Redeem",
                    Amount = data.Amount,
                    Description = $"Coupon number '{data.CouponNo}' is redeemed.",
                    RefId = "",
                    CreatedDate = DateTime.Now
                };
                db.WorkshopCoupons.Remove(data);
                status = db.SaveChanges() > 0;
                if (status)
                {

                    db.WalletTransactions.Add(wt);
                    status = db.SaveChanges() > 0;
                }
            }
            return status;
        }
    }
}