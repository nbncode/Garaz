using Garaaz.Models.Response.SchemeDescription;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Script.Serialization;

namespace Garaaz.Models.Schemes.SchemeLevel
{
    public class RepoSchemeCashback
    {
        readonly garaazEntities db = new garaazEntities();
        const string luckyDraw = "Lucky Draw";
        const string assuredGift = "Assured Gift";

        #region Get Scheme's workshops
        /// <summary>
        /// Get all workshops for particular scheme.
        /// </summary>
        /// <param name="schemeId">The scheme id to use for getting workshops.</param>
        /// <returns>Return list of WorkShop.</returns>
        public SchemeLevelModel GetSchemeWorkshops(int schemeId)
        {
            var schemeLevel = new SchemeLevelModel();

            var scheme = db.Schemes.Where(s => s.SchemeId == schemeId).AsNoTracking().FirstOrDefault();
            if (scheme == null) return schemeLevel;

            var cashbackVisible = Convert.ToBoolean(scheme.CashBack);
            var areBothCbAgApplicable = Convert.ToBoolean(scheme.AreBothCbAgApplicable);

            if (scheme.EndDate < DateTime.Now && cashbackVisible && !areBothCbAgApplicable)
            {
                schemeLevel = new SchemeLevelModel()
                {
                    IsCashbackVisible = true,
                    Details = calculateCashbackAmountforScheme(schemeId)
                };
            }
            else
            {
                var levelDetails = db.TargetWorkShops.Where(t => t.SchemeId == schemeId).Include(t => t.WorkShop).Select(w => new ResponseSchemeLevel()
                {
                    WorkShopId = w.WorkShopId ?? 0,
                    WorkShopName = w.WorkShop != null ? w.WorkShop.WorkShopName : "",
                    CashBack = null,
                    IsDistribute = false
                }).AsNoTracking().ToList();

                schemeLevel = new SchemeLevelModel()
                {
                    IsCashbackVisible = false,
                    Details = levelDetails
                };
            }
            return schemeLevel;
        }

        private List<ResponseSchemeLevel> calculateCashbackAmountforScheme(int schemeId)
        {
            var customers = new List<ResponseSchemeLevel>();

            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var targetWorkshops = context.TargetWorkShops.Where(t => t.SchemeId == schemeId).Include(t => t.WorkShop).AsNoTracking().ToList();

                var wsIds = targetWorkshops.Select(w => w.WorkShopId).ToList();

                var selectedOptions = context.WorkshopSchemesSelectedTypes.Where(w => w.SchemeId == schemeId && wsIds.Contains(w.WorkshopId)).AsNoTracking().ToList();

                var cashbackList = context.CashBacks.Where(x => x.SchemeId == schemeId).AsNoTracking().ToList();
                if (cashbackList.Count == 0) return customers;

                var scheme = context.Schemes.Where(s => s.SchemeId == schemeId).AsNoTracking().FirstOrDefault();

                var targetCriteria = string.IsNullOrEmpty(scheme.TargetCriteria) ? null : scheme.TargetCriteria.Trim();

                foreach (var ws in targetWorkshops)
                {
                    var targetWs = targetWorkshops.FirstOrDefault(w => w.WorkShopId == ws.WorkShopId);
                    if (targetWs == null) { continue; }

                    var selectedOption = selectedOptions.FirstOrDefault(w => w.WorkshopId == ws.WorkShopId)?.SelectedOption ?? string.Empty; ;

                    decimal cashback = 0;

                    if (!selectedOption.Equals(luckyDraw, StringComparison.OrdinalIgnoreCase) && !selectedOption.Equals(assuredGift, StringComparison.OrdinalIgnoreCase))
                    {
                        var wsTarget = Convert.ToDouble(targetWs.NewTarget);
                        var achievedTarget = Convert.ToDouble(targetWs.TargetAchieved);

                        if (!string.IsNullOrEmpty(scheme.CashbackCriteria) && scheme.CashbackCriteria.Trim() == "Monthly")
                        {
                            DateTime monthEndDate;
                            for (DateTime sDate = scheme.StartDate.Value; sDate <= scheme.EndDate; sDate = monthEndDate.AddDays(1))
                            {
                                var addDays = Utils.GetWeek_MonthRemainingDays(sDate, targetCriteria);
                                monthEndDate = sDate.AddDays(addDays);

                                cashback += GetCashbackForWorkshop(cashbackList, achievedTarget, wsTarget);

                            }
                        }
                        else
                        {
                            cashback = GetCashbackForWorkshop(cashbackList, achievedTarget, wsTarget);
                        }
                    }

                    //need to check distribute
                    var isDistribute = context.WorkshopSchemeCashbacks.Where(c => c.SchemeId == schemeId && c.WorkshopId == ws.WorkShopId).AsNoTracking().Count() > 0;

                    customers.Add(new ResponseSchemeLevel()
                    {
                        WorkShopId = ws.WorkShopId ?? 0,
                        WorkShopName = ws.WorkShop != null ? ws.WorkShop.WorkShopName : "",
                        CashBack = cashback,
                        IsDistribute = isDistribute
                    });

                }
            }
            return customers;
        }
        #endregion

        private decimal GetCashbackForWorkshop(IEnumerable<CashBack> cashbackList, double achievedTarget, double wsTarget)
        {
            var cashbackAmt = 0.0M;

            var cashbacks = cashbackList.Where(x => Convert.ToDouble(x.ToAmount) > 0 ? achievedTarget >= Convert.ToDouble(x.FromAmount) && achievedTarget <= Convert.ToDouble(x.ToAmount) : achievedTarget >= Convert.ToDouble(x.FromAmount)).OrderByDescending(o => Convert.ToDouble(o.FromAmount)).ToList();

            // Might cashbacks didn't fit in range
            // Try setting cashback amount by using maximum value of ToAmount
            if (cashbacks.Count() == 0)
            {
                cashbacks = cashbackList.Where(x => Convert.ToDouble(x.ToAmount) > 0 ? achievedTarget >= Convert.ToDouble(x.ToAmount) : achievedTarget >= Convert.ToDouble(x.FromAmount)).OrderByDescending(o => Convert.ToDouble(o.FromAmount)).ToList();
            }

            var cashback = cashbacks.Take(1).FirstOrDefault();
            if (cashback != null && !string.IsNullOrEmpty(cashback.Benifit) && cashback.Benifit.IsValidJson())
            {
                // Calculate cashback amount from benefit
                var jss = new JavaScriptSerializer();
                var cashbackBenefits = jss.Deserialize<List<CashbackBenefit>>(cashback.Benifit);

                var extraSale = achievedTarget - wsTarget;
                if (cashbackBenefits.Count > 0 && extraSale > 0)
                {
                    var extraSalePercent = extraSale * 100 / wsTarget;
                    var cashbackBenefit = cashbackBenefits.FirstOrDefault(cb => extraSalePercent >= cb.FromAmount && extraSalePercent <= cb.ToAmount);

                    if (cashbackBenefit != null)
                    {
                        cashbackAmt = Convert.ToDecimal(achievedTarget * cashbackBenefit.Value / 100);
                    }
                    else
                    {
                        // Might extraSalePercent didn't fit in range
                        // Try setting cashback amount by using maximum value of percentage
                        cashbackBenefit = cashbackBenefits.OrderByDescending(cb => cb.ToAmount).Take(1).FirstOrDefault();
                        if (cashbackBenefit != null && extraSalePercent > cashbackBenefit.ToAmount)
                        {
                            cashbackAmt = Convert.ToDecimal(achievedTarget * cashbackBenefit.Value / 100);
                        }
                    }
                }
            }
            return cashbackAmt;
        }

        #region Distribute scheme cashback
        /// <summary>
        /// Transfer workshops cashback in wallet
        /// </summary>
        /// <param name="workshopId">for single workshop</param>
        /// <param name="isAll"> for all workshop for this scheme</param>
        public bool DistributeSchemeCashback(int? workshopId, int schemeId, bool isAll = false)
        {
            var workShopCashbackList = new List<WorkshopSchemeCashback>();

            try
            {
                using (var context = new garaazEntities())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;

                    var distributedWorkshopIds = context.WorkshopSchemeCashbacks.Where(c => c.SchemeId == schemeId).Select(w => w.WorkshopId).ToList();

                    var targetWorkshops = new List<TargetWorkShop>();
                    if (isAll)
                    {
                        targetWorkshops = context.TargetWorkShops.Where(t => t.SchemeId == schemeId && !distributedWorkshopIds.Contains(t.WorkShopId)).Include(t => t.WorkShop).AsNoTracking().ToList();
                    }
                    else
                    {
                        targetWorkshops = context.TargetWorkShops.Where(t => t.SchemeId == schemeId && t.WorkShopId == workshopId).Include(t => t.WorkShop).AsNoTracking().ToList();
                    }

                    var wsIds = targetWorkshops.Select(w => w.WorkShopId).ToList();

                    var selectedOptions = context.WorkshopSchemesSelectedTypes.Where(w => w.SchemeId == schemeId && wsIds.Contains(w.WorkshopId)).AsNoTracking().ToList();

                    var cashbackList = context.CashBacks.Where(x => x.SchemeId == schemeId).AsNoTracking().ToList();
                    if (cashbackList.Count == 0) return false;

                    var scheme = context.Schemes.Where(s => s.SchemeId == schemeId).AsNoTracking().FirstOrDefault();
                    if (scheme == null) return false;

                    var targetCriteria = string.IsNullOrEmpty(scheme.TargetCriteria) ? null : scheme.TargetCriteria.Trim();

                    var salesExecutives = context.SalesExecutiveWorkshops.Where(x => wsIds.Contains(x.WorkshopId)).AsNoTracking();

                    var roUsers = (from t in targetWorkshops
                                   join d in context.DistributorsOutlets.AsNoTracking() on t.WorkShop?.outletId equals d.OutletId
                                   select new
                                   {
                                       d.UserId,
                                       t.WorkShopId
                                   }).ToList();

                    foreach (var ws in targetWorkshops)
                    {
                        var targetWs = targetWorkshops.FirstOrDefault(w => w.WorkShopId == ws.WorkShopId);
                        if (targetWs == null) { continue; }

                        var selectedOption = selectedOptions.FirstOrDefault(w => w.WorkshopId == ws.WorkShopId)?.SelectedOption ?? string.Empty; ;

                        decimal cashback = 0;

                        if (!selectedOption.Equals(luckyDraw, StringComparison.OrdinalIgnoreCase) && !selectedOption.Equals(assuredGift, StringComparison.OrdinalIgnoreCase))
                        {
                            var wsTarget = Convert.ToDouble(targetWs.NewTarget);
                            var achievedTarget = Convert.ToDouble(targetWs.TargetAchieved);

                            if (!string.IsNullOrEmpty(scheme.CashbackCriteria) && scheme.CashbackCriteria.Trim() == "Monthly")
                            {
                                DateTime monthEndDate;
                                for (DateTime sDate = scheme.StartDate.Value; sDate <= scheme.EndDate; sDate = monthEndDate.AddDays(1))
                                {
                                    var addDays = Utils.GetWeek_MonthRemainingDays(sDate, targetCriteria);
                                    monthEndDate = sDate.AddDays(addDays);

                                    cashback += GetCashbackForWorkshop(cashbackList, achievedTarget, wsTarget);

                                }
                            }
                            else
                            {
                                cashback = GetCashbackForWorkshop(cashbackList, achievedTarget, wsTarget);
                            }
                        }

                        if (cashback == 0) continue;

                        var salesExecutiveId = salesExecutives.FirstOrDefault(s => s.WorkshopId == ws.WorkShopId)?.UserId;

                        var roUserId = roUsers.FirstOrDefault(r => r.WorkShopId == ws.WorkShopId)?.UserId;

                        workShopCashbackList.Add(new WorkshopSchemeCashback()
                        {
                            SchemeId = schemeId,
                            SchemeName = scheme.SchemeName,
                            DistributorId = scheme.DistributorId,
                            DistributorName = scheme.Distributor != null ? scheme.Distributor.DistributorName : "",
                            WorkshopId = ws.WorkShopId ?? 0,
                            WorkshopName = ws.WorkShop != null ? ws.WorkShop.WorkShopName : "",
                            CustomerType = ws.WorkShop != null ? ws.WorkShop.Type : "",
                            OutletId = ws.WorkShop != null ? ws.WorkShop.outletId : null,
                            RoInchargeId = roUserId,
                            SalesExecutiveId = salesExecutiveId,
                            Cashback = cashback,
                            CreatedDate = DateTime.Now.ToString()
                        });
                    }

                    if (workShopCashbackList.Count > 0)
                    {
                        return SaveWorkshopCashback(workShopCashbackList);
                    }
                }

            }
            catch (Exception ex)
            {
                RepoUserLogs.SaveUserLogs(ex.Message, null, ex.StackTrace);
            }
            return false;
        }
        #endregion

        private bool SaveWorkshopCashback(List<WorkshopSchemeCashback> cashbackList)
        {
            bool isAnySaved = false;
            var currentUserId = General.GetUserId();

            using (var context = new garaazEntities())
            {
                var wsIds = cashbackList.Select(w => w.WorkshopId);

                var workshopUsers = context.DistributorWorkShops.Where(d => wsIds.Contains(d.WorkShopId)).AsNoTracking();

                for (int i = 0; i < cashbackList.Count(); i++)
                {
                    var wsCashback = cashbackList[i];

                    var workshopUserId = workshopUsers.FirstOrDefault(dw => dw.WorkShopId == wsCashback.WorkshopId)?.UserId;

                    if (workshopUserId == null) continue;

                    // check if already entry then ignore
                    var isExist = context.WorkshopSchemeCashbacks.FirstOrDefault(c => c.SchemeId == wsCashback.SchemeId && c.WorkshopId == wsCashback.WorkshopId);

                    if (isExist == null)
                    {
                        var walletTransaction = new WalletTransaction()
                        {
                            UserId = workshopUserId,
                            WorkshopId = wsCashback.WorkshopId,
                            Type = "Cr",
                            Amount = wsCashback.Cashback,
                            Description = $"Won cashback Price: ₨ {wsCashback.Cashback} from scheme {wsCashback.SchemeName}.",
                            RefId = currentUserId,
                            CreatedDate = DateTime.Now
                        };
                        if (SaveWalletBalance(walletTransaction))
                        {
                            wsCashback.IsPaid = true;
                            context.WorkshopSchemeCashbacks.Add(wsCashback);
                            var isSaved = context.SaveChanges() > 0;
                            if (isSaved) { isAnySaved = isSaved; }
                        }
                    }
                }
            }
            return isAnySaved;
        }

        private bool SaveWalletBalance(WalletTransaction walletTransaction)
        {
            bool isSaved = false;
            using (var context = new garaazEntities())
            {
                var userDetails = context.UserDetails.FirstOrDefault(u => u.UserId == walletTransaction.UserId);
                if (userDetails != null)
                {
                    context.WalletTransactions.Add(walletTransaction);

                    var walletBalance = userDetails.WalletAmount ?? 0;

                    userDetails.WalletAmount = walletBalance + walletTransaction.Amount;

                    isSaved = context.SaveChanges() > 0;
                }
            }
            return isSaved;
        }
    }
}