using Garaaz.Models.Response.SchemeDescription;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Garaaz.Models
{
    public class RepoSchemesDescription
    {
        #region Variables

        readonly garaazEntities db = new garaazEntities();
        readonly General general = new General();

        // In order - Gold, Orange, Green, Deep sky blue, hot pink, dim gray, tan, red, dark khaki and teal
        private readonly string[] _colorCodes = { "#FFD700", "#FFA500", "#008000", "#00BFFF", "#FF69B4", "#696969", "#D2B48C", "#FF0000", "#BDB76B", "#008080" };

        const string dateFormat = "MMM dd, yyyy";
        #endregion

        #region Schemes Description       

        public AppSchemeDataModel GetSchemeDescription(int schemeId, string userId, string role)
        {
            var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == schemeId && (s.IsDeleted == null || s.IsDeleted == false));
            return scheme == null ? null : GetSchemeDetails(scheme, userId, role);
        }

        /// <summary>
        /// Get AppSchemeDataModel for particular scheme.
        /// </summary>        
        private AppSchemeDataModel GetSchemeDetails(Scheme scheme, string userId, string role)
        {
            const string notAvailable = "NA";


            var aspNetUser = db.AspNetUsers.FirstOrDefault(u => u.Id == userId);

            var schemeType = ClsSchema.SchemeTypes.FirstOrDefault(s => s.Value == scheme.Type)?.Text;
            var distributor = db.Distributors.FirstOrDefault(d => d.DistributorId == scheme.DistributorId);

            var asdModel = new AppSchemeDataModel
            {
                SchemeName = scheme.SchemeName,
                BannerImage = general.CheckImageUrl(scheme.BannerImage),
                AreBothCbAgApplicable = Convert.ToBoolean(scheme.AreBothCbAgApplicable),
                AreBothAgLdApplicable = scheme.AreBothAgLdApplicable,
                SchemeType = schemeType ?? scheme.Type,
                SchemeDateRange = $"{scheme.StartDate?.ToString(dateFormat)} - {scheme.EndDate?.ToString(dateFormat)}",
                DistributorName = distributor?.DistributorName,
                SchemeStartDate = scheme.StartDate != null ? scheme.StartDate?.ToString(dateFormat) : notAvailable,
                SchemeEndDate = scheme.EndDate != null ? scheme.EndDate?.ToString(dateFormat) : notAvailable,
            };

            #region Parts Information

            if (!string.IsNullOrEmpty(scheme.PartCategory))
            {
                asdModel.PartCategory = scheme.PartCategory;
            }
            else
            {
                asdModel.PartCategory = notAvailable;
            }

            asdModel.PartGroup = scheme.PartType ?? notAvailable;
            asdModel.FPercentage = Convert.ToString(scheme.FValue);
            asdModel.MPercentage = Convert.ToString(scheme.MValue);
            asdModel.SPercentage = Convert.ToString(scheme.SValue);
            asdModel.PartCreation = scheme.PartCreations;

            if (!string.IsNullOrWhiteSpace(asdModel.PartGroup))
            {
                asdModel.AllowFocusPartsDownload = true;
                //asdModel = GetFMSorFocusParts(asdModel, scheme);
                const string FmsPartsGroup = "FMS Parts Group";
                if (asdModel.PartGroup.Equals(FmsPartsGroup, StringComparison.OrdinalIgnoreCase))
                {
                    asdModel.DateRangeForFms = $"{scheme.StartRange?.ToString(dateFormat)} - {scheme.EndRange?.ToString(dateFormat)}";
                }
            }

            #endregion

            if (role == Constants.Distributor || role == Constants.SuperAdmin)
            {
                #region Customer Segment

                if (!string.IsNullOrEmpty(scheme.BranchCode))
                {
                    var schBranchCodes = scheme.BranchCode.Split(',');
                    var ro = new RepoOutlet();
                    var distOutlets = ro.GetDistributorOutlets(scheme.DistributorId);
                    asdModel.BranchCode = string.Join(", ", distOutlets.Where(d => schBranchCodes.Contains(d.OutletId.ToString())).Select(d => d.OutletName));
                }
                else
                {
                    asdModel.BranchCode = notAvailable;
                }

                if (!string.IsNullOrEmpty(scheme.SalesExecutiveId))
                {
                    RepoSchemes repoScheme = new RepoSchemes();
                    var schSalesExecIds = scheme.SalesExecutiveId.Split(',');
                    var roIncharges = repoScheme.GetRoIncharge(scheme.DistributorId);
                    var salesPerson = repoScheme.GetSalesExecutives(null, null, roIncharges);
                    asdModel.SalesPerson = string.Join(", ", salesPerson.Where(s => schSalesExecIds.Contains(s.UserId)).Select(s => s.UserName));
                }
                else
                {
                    asdModel.SalesPerson = notAvailable;
                }

                if (!string.IsNullOrEmpty(scheme.PartyType))
                {
                    var schPartyTypes = scheme.PartyType.Split(',');
                    asdModel.CustomerType = string.Join(", ", schPartyTypes);
                }
                else
                {
                    asdModel.CustomerType = notAvailable;
                }

                #endregion
            }

            #region Categories

            // List: Categories
            var categSchemes = db.CategorySchemes.Where(x => x.SchemeId == scheme.SchemeId);
            int colorIndex = 0;
            foreach (var cScheme in categSchemes)
            {
                var listGifts = new List<SchemeCategoryGift>();

                // List: Gift Management
                var gifts = from g in db.GiftManagements
                            where g.SchemeId == scheme.SchemeId
                            && g.GiftCategories.Any(c => c.CategoryId == cScheme.CategoryId || c.IsAll == true)
                            select g;
                foreach (var gift in gifts)
                {
                    var imgPath = gift.ImagePath != null ? general.CheckImageUrl(gift.ImagePath) : general.CheckImageUrl("/assets/images/NoPhotoAvailable.png");

                    listGifts.Add(new SchemeCategoryGift
                    {
                        GiftName = gift.Gift,
                        GiftImage = imgPath,
                        Qty = Convert.ToInt32(gift.Qty)
                    });
                }
                asdModel.GiftDatas.Add(new GiftData
                {
                    CategoryName = cScheme.Category,
                    ColorCode = _colorCodes.Length > colorIndex ? _colorCodes[colorIndex] : "#8B4513",
                    data = listGifts
                });

                // List: Assured Gifts
                var assGifts = from g in db.AssuredGifts
                               where g.SchemeId == scheme.SchemeId
                               && g.AssuredGiftCategories.Any(a => a.CategoryId == cScheme.CategoryId || a.IsAll == true)
                               select g;
                var listAssuredGifts = assGifts.Select(a => new SchemeAssuredGift
                {
                    SalesTarget = a.Target,
                    Point = a.Point,
                    Reward = a.Reward
                }).ToList();
                asdModel.AssuredGiftDatas.Add(new AssuredGiftData
                {
                    CategoryName = cScheme.Category,
                    ColorCode = _colorCodes.Length > colorIndex ? _colorCodes[colorIndex] : "#8B4513",
                    data = listAssuredGifts
                });

                colorIndex++;
            }

            #endregion

            #region CashBack

            // List: Cash Back
            asdModel.CashBacks = GetCashBacks(scheme.SchemeId, role);

            #endregion

            #region Qualify Criteria

            // List: Qualify Criteria
            var qcs = db.QualifyCriterias.Where(x => x.SchemeId == scheme.SchemeId);
            foreach (var qc in qcs)
            {
                var productName = "";
                if (!string.IsNullOrEmpty(qc.TypeValue))
                {
                    var prodId = Convert.ToInt32(qc.TypeValue);
                    var prod = db.Products.FirstOrDefault(p => p.ProductId == prodId);
                    if (prod != null)
                    {
                        productName = prod.ProductName;
                    }
                }

                asdModel.QualifyCriterias.Add(new SchemeQualifyCriteria
                {
                    AmountUpto = qc.AmountUpto,
                    Type = qc.Type,
                    PartsOrAccessories = productName,
                    NumberOfCoupons = qc.NumberOfCoupons,
                    AdditionalCouponAmount = qc.AdditionalCouponAmount,
                    AdditionalNumberOfCoupons = qc.AdditionalNumberOfCoupons
                });
            }

            #endregion

            #region Target workshop

            // List: Target workshop
            int distributorId;
            var targetWs = new List<TargetWorkShop>();

            if (role == Constants.SuperAdmin)
            {
                targetWs = db.TargetWorkShops.Where(x => x.SchemeId == scheme.SchemeId).AsNoTracking().ToList();
            }
            else if (role == Constants.Distributor || role == Constants.Users)
            {
                distributorId = userId.GetDistributorId(role);
                var workshops = db.DistributorWorkShops.Where(w => w.DistributorId == distributorId).AsNoTracking().ToList();
                if (workshops.Count > 0)
                {
                    var ids = workshops.Select(a => a.WorkShopId);

                    targetWs = (from a in db.TargetWorkShops
                                where a.SchemeId == scheme.SchemeId && ids.Contains(a.WorkShopId)
                                select a
                              ).AsNoTracking().ToList();
                }
            }
            else if (aspNetUser?.AspNetRoles.FirstOrDefault(a => a.Name.Contains(Constants.RoIncharge)) != null)
            {
                var distributorsOutlets = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == userId);
                if (distributorsOutlets != null)
                {
                    var workshop = (from w in db.WorkShops
                                    where w.outletId == distributorsOutlets.OutletId
                                    select w).AsNoTracking().ToList();
                    if (workshop.Count > 0)
                    {
                        var ids = workshop.Select(a => a.WorkShopId);

                        targetWs = (from a in db.TargetWorkShops
                                    where a.SchemeId == scheme.SchemeId && a.WorkShopId.HasValue && ids.Contains(a.WorkShopId.Value)
                                    select a
                                  ).AsNoTracking().ToList();
                    }
                }
            }
            else if (aspNetUser?.AspNetRoles.FirstOrDefault(a => a.Name.Contains(Constants.SalesExecutive)) != null)
            {
                var wIds = db.SalesExecutiveWorkshops.Where(se => se.UserId == userId).AsNoTracking().Select(w => w.WorkshopId).ToList();
                targetWs = (from a in db.TargetWorkShops
                            where a.SchemeId == scheme.SchemeId && a.WorkShopId.HasValue && wIds.Contains(a.WorkShopId.Value)
                            select a).AsNoTracking().ToList();
            }

            else if (role == Constants.Workshop || role == Constants.WorkshopUsers)
            {
                distributorId = userId.GetWorkshopId(role);
                targetWs = db.TargetWorkShops.Where(x => x.SchemeId == scheme.SchemeId && x.WorkShopId == distributorId).AsNoTracking().ToList();
            }

            foreach (var tws in targetWs.Where(tws => tws.WorkShop != null).OrderBy(w => w.WorkShop?.WorkShopName))
            {
                asdModel.TargetWorkshops.Add(new SchemeTargetWorkshop
                {
                    Workshop = tws.WorkShop.WorkShopName,
                    Target = tws.NewTarget,
                    TargetAchieved = Math.Round(tws.TargetAchieved ?? 0).ToString(),
                    TargetAchievedPercentage = (tws.TargetAchievedPercentage != null ? Math.Round(tws.TargetAchievedPercentage.Value).ToString() : "0") + " %"
                });
            }

            #endregion

            return asdModel;
        }

        private List<SchemeCashBack> GetCashBacks(int schemeId, string role)
        {
            var schemeCashbacks = new List<SchemeCashBack>();
            var cashbacks = db.CashBacks.Where(x => x.SchemeId == schemeId).ToList();
            if (cashbacks.Count <= 0) return schemeCashbacks;

            var jss = new JavaScriptSerializer();
            foreach (var cb in cashbacks)
            {
                var cbBenefits = new List<CashbackBenefit>();
                if (!string.IsNullOrEmpty(cb.Benifit) && cb.Benifit.IsValidJson())
                {
                    cbBenefits = jss.Deserialize<List<CashbackBenefit>>(cb.Benifit);
                }
                //if (role == Constants.Workshop || role == Constants.WorkshopUsers)
                //{
                //    schemeCashbacks.Add(new SchemeCashBack
                //    {
                //        data = cbBenefits
                //    });
                //}
                //else
                //{
                //    schemeCashbacks.Add(new SchemeCashBack
                //    {
                //        FromAmount = cb.FromAmount,
                //        ToAmount = cb.ToAmount,
                //        data = cbBenefits
                //    });
                //}
                schemeCashbacks.Add(new SchemeCashBack
                {
                    FromAmount = cb.FromAmount,
                    ToAmount = cb.ToAmount,
                    data = cbBenefits
                });

            }

            return schemeCashbacks;
        }

        //private AppSchemeDataModel GetFMSorFocusParts(AppSchemeDataModel asdModel, Scheme scheme)
        //{
        //    const string FmsPartsGroup = "FMS Parts Group";
        //    const string FocusPartsGroup = "Focus Parts Group";

        //    if (asdModel.PartGroup.Equals(FmsPartsGroup, StringComparison.OrdinalIgnoreCase))
        //    {
        //        asdModel.AllowFocusPartsDownload = true;
        //        asdModel.DateRangeForFms = $"{scheme.StartRange?.ToString(dateFormat)} - {scheme.EndRange?.ToString(dateFormat)}";
        //    }
        //    else if (asdModel.PartGroup.Equals(FocusPartsGroup, StringComparison.OrdinalIgnoreCase))
        //    {
        //        asdModel.AllowFocusPartsDownload = true;

        //        var sfParts = new List<SchemeFocusPart>();
        //        var fps = db.FocusParts.Where(x => x.SchemeId == scheme.SchemeId).AsNoTracking().ToList();

        //        var groupIds = fps.GroupBy(g => g.GroupId).Select(g => g.Key).ToList();
        //        var productIds = fps.Select(g => g.ProductId).ToList();
        //        var products = db.Products.Where(p => (groupIds.Contains(p.GroupId) || productIds.Contains(p.ProductId)) && p.DistributorId == scheme.DistributorId).Include(x => x.ProductGroup).AsNoTracking().Select(p => new
        //        {
        //            p.GroupId,
        //            p.ProductId,
        //            p.Price,
        //            p.Description,
        //            p.PartNo,
        //            p.ProductGroup
        //        }).ToList();

        //        foreach (var groupId in groupIds)
        //        {
        //            var focusParts = products.Where(x => x.GroupId == groupId).Select(s => new SchemeFocusPart
        //            {
        //                PartGroup = s.ProductGroup != null ? s.ProductGroup.GroupName : "NA",
        //                PartNumber = s.PartNo,
        //                PartDescription = s.Description,
        //                MRP = s.Price > 0 ? s.Price.ToString() : "NA",
        //            }).ToList();

        //            sfParts.AddRange(focusParts);
        //        }

        //        asdModel.FocusParts = sfParts;
        //    }
        //    return asdModel;
        //}

        #endregion

        #region Schemes User Level
        public AppLevelDataModel GetSchemeUserLevel(SchemeUserLevelModel model)
        {
            var workShopUser = db.WorkshopsUsers.AsNoTracking().FirstOrDefault(x => x.UserId == model.UserId);
            var workShopDistributor = db.DistributorWorkShops.AsNoTracking().FirstOrDefault(x => x.UserId == model.UserId);
            if (workShopUser == null && workShopDistributor == null) return null;

            if (workShopDistributor != null)
            {
                workShopUser = new WorkshopsUser
                {
                    WorkshopId = workShopDistributor.WorkShopId ?? 0
                };
            }

            return GetWorkshopLevels(model.SchemeId, workShopUser.WorkshopId);

        }
        #endregion

        #region Scheme's workshop level

        /// <summary>
        /// Get workshop levels for particular scheme.
        /// </summary>
        /// <param name="schemeId">The scheme id.</param>
        /// <param name="workshopId">The workshop id.</param>
        /// <returns>Return instance of AppLevelDataModel with levels.</returns>
        public AppLevelDataModel GetWorkshopLevels(int schemeId, int workshopId)
        {
            var scheme = db.Schemes.SingleOrDefault(s => s.SchemeId == schemeId);
            if (scheme?.StartDate == null || scheme.EndDate == null) return null;

            var levels = new List<LevelInfo>();
            var topMsg = string.Empty;
            var msg = string.Empty;
            // var points = 0;

            const string luckyDraw = "Lucky Draw";
            const string assuredGift = "Assured Gift";
            const string cashback = "Cashback";

            //var sales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.WorkShopId == workshopId && DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(scheme.StartDate) && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(scheme.EndDate)).ToList();

            // Get workshop target and achieved-target
            var targetWs = db.TargetWorkShops.AsNoTracking().FirstOrDefault(x => x.SchemeId == schemeId && x.WorkShopId == workshopId);
            var achievedTarget = Convert.ToDecimal(targetWs?.TargetAchieved);
            var achievedTargetDouble = Convert.ToDouble(targetWs?.TargetAchieved);

            var selectedOption = db.WorkshopSchemesSelectedTypes.FirstOrDefault(w => w.SchemeId == schemeId && w.WorkshopId == workshopId)?.SelectedOption ?? string.Empty;

            // Reward Info tabs options
            var cashbackVisible = Convert.ToBoolean(scheme.CashBack);
            var assuredGiftVisible = Convert.ToBoolean(scheme.AssuredGift);
            var areBothCbAgApplicable = Convert.ToBoolean(scheme.AreBothCbAgApplicable);
            var areBothAgLdApplicable = scheme.AreBothAgLdApplicable;

            #region Part 1: Categories

            // Data was saved from 'Customer Segment' tab for Scheme

            var categories = db.CategorySchemes.Where(c => c.SchemeId == schemeId);
            CategoryScheme category = null;
            if (categories.Any())
            {
                //var achievedTarget = sales.Count > 0 ? sales.Sum(s => Convert.ToDecimal(s.NetRetailSelling)) : 0;

                var filteredCat = categories.Where(c => c.MaxAmount != null && c.MaxAmount > 0 ? c.MinAmount <= achievedTarget && c.MaxAmount >= achievedTarget : c.MinAmount <= achievedTarget).OrderByDescending(o => o.MaxAmount == null).ThenByDescending(o => o.MaxAmount.HasValue);

                category = filteredCat.Take(1).FirstOrDefault();
                if (category != null)
                {
                    topMsg = $"You are {category.Category} stage now.";
                }
            }

            #endregion

            #region Part 2: Gift Management (Lucky Draw)

            // Data was saved from 'Lucky Draw' tab within Reward Info for Scheme

            if (areBothAgLdApplicable) // Are both assured gift and lucky draw applicable
            {
                var giftManagements = db.GiftManagements.Where(g => g.SchemeId == schemeId);
                if (giftManagements.Any() && category != null)
                {
                    var gifts = giftManagements.Where(m => m.GiftCategories.Any(g => g.CategoryId == category.CategoryId || g.IsAll == true)).Select(m => m.Gift);
                    if (gifts.Any())
                    {
                        msg = $"You are eligible for '{string.Join(",", gifts)}'.";

                        levels.Add(new LevelInfo
                        {
                            Message = msg,
                            LevelAchieved = true,
                            Image = general.CheckImageUrl("/assets/images/Level10.png"),
                            Date = scheme.StartDate.Value,
                            Type = Constants.GiftManagement
                        });
                    }
                }
            }

            #endregion

            #region Part 3: Cashback

            // Data was saved from 'Cashback' tab within Reward Info for Scheme

            // Show if selected option is not lucky draw, not assured gift and not areBothCbAgApplicable
            if (!selectedOption.Equals(luckyDraw, StringComparison.OrdinalIgnoreCase) && !selectedOption.Equals(assuredGift, StringComparison.OrdinalIgnoreCase) && !areBothCbAgApplicable && cashbackVisible)
            {
                var cashbackList = db.CashBacks.Where(x => x.SchemeId == schemeId).ToList();
                if (cashbackList.Count > 0)
                {
                    if (targetWs != null)
                    {
                        var wsTarget = Convert.ToDouble(targetWs.NewTarget);

                        if (scheme.CashbackCriteria.Trim() == "Monthly")
                        {
                            DateTime monthEndDate;
                            for (DateTime sDate = scheme.StartDate.Value; sDate <= scheme.EndDate; sDate = monthEndDate.AddDays(1))
                            {
                                if (sDate <= DateTime.Now)
                                {
                                    var addDays = Utils.GetWeek_MonthRemainingDays(sDate, scheme.TargetCriteria);
                                    monthEndDate = sDate.AddDays(addDays);
                                    //var achievedTarget = sales.Where(x => x.CreatedDate >= sDate && x.CreatedDate <= monthEndDate).Sum(s => Convert.ToDouble(s.NetRetailSelling));

                                    var levelInfo = GetLevelInfoForCashback(cashbackList, achievedTargetDouble, scheme.StartDate.Value, sDate,
                                        monthEndDate, wsTarget);
                                    if (levelInfo != null)
                                    {
                                        levels.Add(levelInfo);
                                    }
                                }
                                else { break; }
                            }
                        }
                        else
                        {
                            //var achievedTarget = sales.Sum(s => Convert.ToDouble(s.NetRetailSelling));
                            var levelInfo = GetLevelInfoForCashback(cashbackList, achievedTargetDouble, scheme.StartDate.Value, scheme.StartDate.Value,
                                scheme.EndDate.Value, wsTarget);
                            if (levelInfo != null)
                            {
                                levels.Add(levelInfo);
                            }
                        }
                    }

                }
            }

            #endregion

            #region Part 4: Qualify Criteria (Coupon Allocation)

            // Data was saved from 'Coupon Allocation' tab within Reward Info for Scheme

            var qualifyCriterias = db.QualifyCriterias.Where(x => x.SchemeId == schemeId).ToList();
            if (qualifyCriterias.Count > 0)
            {
                var qcList = new List<QualifyCriteriaUserLevel>();
                //foreach (var qc in qualifyCriterias)
                //{
                //    if (achievedTargetDouble >= Convert.ToDouble(qc.AmountUpto))
                //    {
                //        qcList.Add(new QualifyCriteriaUserLevel
                //        {
                //            AchievedTarget = achievedTargetDouble,
                //            AmountUpto = qc.AmountUpto,
                //            NumberOfCoupons = qc.NumberOfCoupons,
                //            AdditionalCouponAmount = qc.AdditionalCouponAmount,
                //            AdditionalNumberOfCoupons = qc.AdditionalNumberOfCoupons
                //        });
                //    }
                //}

                qcList = qualifyCriterias.Where(qc => Convert.ToDouble(qc.AmountUpto) <= achievedTargetDouble).Select(qc => new QualifyCriteriaUserLevel
                {
                    AchievedTarget = achievedTargetDouble,
                    AmountUpto = qc.AmountUpto,
                    NumberOfCoupons = qc.NumberOfCoupons,
                    AdditionalCouponAmount = qc.AdditionalCouponAmount,
                    AdditionalNumberOfCoupons = qc.AdditionalNumberOfCoupons
                }).ToList();

                if (qcList.Count > 0)
                {
                    int coupon = 0, additionalCoupon = 0;
                    var qualifyCriteria = qcList.OrderByDescending(o => Convert.ToDouble(o.AmountUpto)).Take(1).FirstOrDefault();
                    if (qualifyCriteria != null)
                    {
                        coupon = Convert.ToInt32(qualifyCriteria.NumberOfCoupons);
                        var additionalCouponAmt = Convert.ToDouble(qualifyCriteria.AdditionalCouponAmount);
                        var amount = Convert.ToDouble(qualifyCriteria.AmountUpto);

                        var quotient = qualifyCriteria.AchievedTarget / amount; // 890/100 = 8
                        var remainder = qualifyCriteria.AchievedTarget % amount; // 890/100 = 90        

                        coupon = (int)Math.Truncate(quotient) * coupon; // 8 * 2 = 16

                        if (additionalCouponAmt > 0)
                        {
                            var quotientForAdditional = (int)Math.Truncate(remainder) / additionalCouponAmt; // 90/10 = 9
                            additionalCoupon = (int)Math.Truncate(quotientForAdditional) * Convert.ToInt32(qualifyCriteria.AdditionalNumberOfCoupons); //9 *1 = 9
                        }
                    }
                    if (coupon > 0 || additionalCoupon > 0)
                    {
                        if (coupon > 0 && additionalCoupon > 0)
                            msg = "You have achieved " + coupon + " coupon and " + additionalCoupon + " additional coupon.";
                        else if (coupon > 0)
                            msg = "You have achieved " + coupon + " coupon.";
                        else if (additionalCoupon > 0)
                            msg = $"You have achieved {additionalCoupon} additional coupon.";

                        levels.Add(new LevelInfo
                        {
                            Message = msg,
                            LevelAchieved = coupon != 0,
                            Image = coupon == 0 ? general.CheckImageUrl("/assets/images/Level08.png") : general.CheckImageUrl("/assets/images/Level10.png"),
                            Date = scheme.StartDate.Value,
                            Type = Constants.QualifyingCriteria
                        });
                    }
                }
                // add new for showing message to get coupons
                else
                {
                    var qualifyCriteria = qualifyCriterias.Where(o => o.NumberOfCoupons != null).OrderBy(o => Convert.ToDouble(o.AmountUpto)).FirstOrDefault();

                    if (qualifyCriteria != null)
                    {
                        int coupon = Convert.ToInt32(qualifyCriteria.NumberOfCoupons);
                        double moreNeedSale = Convert.ToDouble(qualifyCriteria.AmountUpto) - achievedTargetDouble;

                        if (coupon > 0 && moreNeedSale > 0)
                        {
                            msg = "You have made sale of ₹ " + achievedTargetDouble + ".You need sales of ₹ " + moreNeedSale + " more to be eligible for coupon " + coupon + " .";

                            levels.Add(new LevelInfo
                            {
                                Message = msg,
                                LevelAchieved = false,
                                Image = general.CheckImageUrl("/assets/images/Level08.png"),
                                Date = scheme.StartDate.Value,
                                Type = Constants.QualifyingCriteria
                            });
                        }
                    }
                }
            }

            #endregion

            #region Part 5: Target workshop

            if (!string.IsNullOrEmpty(scheme.DispersalFrequency))
            {
                // var targetWs = db.TargetWorkShops.FirstOrDefault(x => x.SchemeId == schemeId && x.WorkShopId == workshopId);
                if (targetWs != null && !string.IsNullOrEmpty(targetWs.NewTarget))
                {
                    var targetValue = Convert.ToDecimal(targetWs.NewTarget);
                    int addDays = 0;

                    if (scheme.DispersalFrequency.Trim() == "Daily")
                    {
                        for (DateTime sDate = scheme.StartDate.Value; sDate <= scheme.EndDate; sDate = sDate.AddDays(1))
                        {
                            if (sDate <= DateTime.Now)
                            {
                                // var achievedTarget = sales.Count > 0 ? sales.Where(x => x.CreatedDate == sDate).ToList().Sum(s => Convert.ToDouble(s.NetRetailSelling)) : 0;
                                var remainingSales = targetValue - achievedTarget;
                                var date = sDate.ToString("dd-MM-yyyy");
                                if (remainingSales > 0)
                                {
                                    msg = sDate.Date == DateTime.Today ? $"On {date}, You have achieved {achievedTarget} target out of {targetValue}. Your remaining target for today is {remainingSales}." : $"On {date}, You have not achieved target of {targetValue}.";
                                }
                                else
                                    msg = $"On {date}, You have achieved target of {targetValue}.";

                                levels.Add(new LevelInfo
                                {
                                    Message = msg,
                                    LevelAchieved = remainingSales <= 0,
                                    Image = remainingSales > 0 ? general.CheckImageUrl("/assets/images/Level08.png") : general.CheckImageUrl("/assets/images/Level10.png"),
                                    Date = sDate,
                                    Type = Constants.TargetWorkShop
                                });
                            }
                            else { break; }
                        }
                    }
                    else if (scheme.DispersalFrequency.Trim() == "Weekly")
                    {
                        DateTime weekEndDate;
                        for (DateTime sDate = scheme.StartDate.Value; sDate <= scheme.EndDate; sDate = weekEndDate.AddDays(1))
                        {
                            if (sDate <= DateTime.Now)
                            {
                                addDays = Utils.GetWeek_MonthRemainingDays(sDate, scheme.DispersalFrequency.Trim());
                                weekEndDate = sDate.AddDays(addDays);
                                //var achievedTarget = sales.Count > 0 ? sales.Where(x => x.CreatedDate >= sDate && x.CreatedDate <= weekEndDate).ToList().Sum(s => Convert.ToDouble(s.NetRetailSelling)) : 0;
                                var remainingSales = targetValue - achievedTarget;
                                if (remainingSales > 0)
                                {
                                    if (DateTime.Compare(DateTime.Today, weekEndDate) == 0) // If date matches
                                        msg = "From " + sDate.ToString("dd-MM-yyyy") + " to " + weekEndDate.ToString("dd-MM-yyyy") + ", You have achieved " + achievedTarget + " target of " + targetValue + ". Your remaining target for today is " + remainingSales + ".";
                                    else
                                        msg = "From " + sDate.ToString("dd-MM-yyyy") + " to " + weekEndDate.ToString("dd-MM-yyyy") + ", You have not achieved target of " + targetValue + ".";
                                }
                                else
                                    msg = "From " + sDate.ToString("dd-MM-yyyy") + " to " + weekEndDate.ToString("dd-MM-yyyy") + ", You have achieved target of " + targetValue + ".";

                                levels.Add(new LevelInfo
                                {
                                    Message = msg,
                                    LevelAchieved = remainingSales <= 0,
                                    Image = remainingSales > 0 ? general.CheckImageUrl("/assets/images/Level08.png") : general.CheckImageUrl("/assets/images/Level10.png"),
                                    Date = sDate,
                                    Type = Constants.TargetWorkShop
                                });
                            }
                            else { break; }
                        }
                    }
                    else if (scheme.DispersalFrequency.Trim() == "Monthly")
                    {
                        DateTime monthEndDate;
                        for (DateTime sDate = scheme.StartDate.Value; sDate <= scheme.EndDate; sDate = monthEndDate.AddDays(1))
                        {
                            if (sDate <= DateTime.Now)
                            {
                                addDays = Utils.GetWeek_MonthRemainingDays(sDate, scheme.DispersalFrequency.Trim());
                                monthEndDate = sDate.AddDays(addDays);
                                // var achievedTarget = sales.Count > 0 ? sales.Where(x => x.CreatedDate >= sDate && x.CreatedDate <= monthEndDate).ToList().Sum(s => Convert.ToDouble(s.NetRetailSelling)) : 0;
                                var remainingSales = targetValue - achievedTarget;
                                if (remainingSales > 0)
                                {
                                    if (DateTime.Compare(DateTime.Today, monthEndDate) == 0)
                                        msg = "From " + sDate.ToString("dd-MM-yyyy") + " to " + monthEndDate.ToString("dd-MM-yyyy") + ", You have achieved " + achievedTarget + " target of " + targetValue + ". Your remaining target for today is " + remainingSales + ".";
                                    else
                                        msg = "From " + sDate.ToString("dd-MM-yyyy") + " to " + monthEndDate.ToString("dd-MM-yyyy") + ", You have not achieved target of " + targetValue + ".";
                                }
                                else
                                    msg = "From " + sDate.ToString("dd-MM-yyyy") + " to " + monthEndDate.ToString("dd-MM-yyyy") + ", You have achieved target of " + targetValue + ".";

                                levels.Add(new LevelInfo
                                {
                                    Message = msg,
                                    LevelAchieved = remainingSales <= 0,
                                    Image = remainingSales > 0 ? general.CheckImageUrl("/assets/images/Level08.png") : general.CheckImageUrl("/assets/images/Level10.png"),
                                    Date = sDate,
                                    Type = Constants.TargetWorkShop
                                });
                            }
                            else { break; }
                        }
                    }
                    else if (scheme.DispersalFrequency.Trim() == "One Time")
                    {
                        // var achievedTarget = sales.Count > 0 ? sales.Sum(s => Convert.ToDouble(s.NetRetailSelling)) : 0;
                        var remainingSales = targetValue - achievedTarget;
                        if (remainingSales > 0)
                        {
                            if (DateTime.Compare(DateTime.Today, scheme.EndDate.Value) == 0)
                                msg = "From " + scheme.StartDate.Value.ToString("dd-MM-yyyy") + " to " + scheme.EndDate.Value.ToString("dd-MM-yyyy") + ", You have achieved " + achievedTarget + " target of " + targetValue + " and your remaining target for today is " + remainingSales + ".";
                            else
                                msg = "From " + scheme.StartDate.Value.ToString("dd-MM-yyyy") + " to " + scheme.EndDate.Value.ToString("dd-MM-yyyy") + ", You have not achieved target of " + targetValue + ".";
                        }
                        else
                            msg = "From " + scheme.StartDate.Value.ToString("dd-MM-yyyy") + " to " + scheme.EndDate.Value.ToString("dd-MM-yyyy") + ", You have achieved target of " + targetValue + ".";

                        levels.Add(new LevelInfo
                        {
                            Message = msg,
                            LevelAchieved = remainingSales <= 0,
                            Image = remainingSales > 0 ? general.CheckImageUrl("/assets/images/Level08.png") : general.CheckImageUrl("/assets/images/Level10.png"),
                            Date = scheme.StartDate.Value,
                            Type = Constants.TargetWorkShop
                        });
                    }

                }
            }

            #endregion

            #region Part 6: Focus Part - (Obsolete)

            // NOTE: Keeping this code for future use.
            // Modified on 27-01-2020
            // Otherwise, we should have removed it because BenefitType and BenefitValue not set

            List<FocusPart> focusParts = null; // Earlier this line was here - db.FocusParts.Where(x => x.SchemeId == schemeId);
            //if (focusParts != null)
            //{
            //    var FPList = new List<FocusPartUserLevel>();
            //    if (!string.IsNullOrEmpty(scheme.FocusPartBenifitType) && scheme.FocusPartBenifitType == "BenifitByGroup")
            //    {
            //        var fpGroupTarget = scheme.FocusPartTarget ?? 0;
            //        double fpAchievedTarget = 0;
            //        foreach (var fp in focusParts)
            //        {
            //            if (fp.GroupId != null && fp.GroupId.Value > 0)
            //            {
            //                var filteredSales = fp.ProductId > 0 ? sales.Where(x => x.ProductId == fp.ProductId && x.GroupId == fp.GroupId) : sales.Where(x => x.GroupId == fp.GroupId).ToList();
            //                double totalSale = filteredSales.Sum(s => Convert.ToDouble(s.NetRetailSelling));
            //                fpAchievedTarget = totalSale + fpAchievedTarget;
            //            }
            //        }
            //        if (fpAchievedTarget >= Convert.ToDouble(fpGroupTarget))
            //        {
            //            var price = Convert.ToDecimal(fpAchievedTarget);
            //            FPList.Add(new FocusPartUserLevel
            //            {
            //                Qty = 0,
            //                BenefitType = scheme.FocusPartBenifitType,
            //                BenefitValue = scheme.FocusPartBenifitTypeNumber,
            //                TotalSale = fpAchievedTarget,
            //                Price = price
            //            });
            //        }
            //    }
            //    else
            //    {
            //        foreach (var fp in focusParts)
            //        {
            //            if (fp.GroupId != null && fp.GroupId.Value > 0)
            //            {
            //                var filteredSales = fp.ProductId > 0 ? sales.Where(x => x.ProductId == fp.ProductId && x.GroupId == fp.GroupId) : sales.Where(x => x.GroupId == fp.GroupId);
            //                double totalSale = filteredSales.Any() ? filteredSales.Sum(s => Convert.ToDouble(s.NetRetailSelling)) : 0;
            //                if (fp.Price > 0 || fp.Qty > 0)
            //                {
            //                    if (fp.Price > 0 && totalSale >= Convert.ToDouble(fp.Price))
            //                    {
            //                        FPList.Add(new FocusPartUserLevel
            //                        {
            //                            Qty = fp.Qty,
            //                            BenefitType = fp.Type,
            //                            BenefitValue = fp.Value,
            //                            TotalSale = totalSale,
            //                            Price = fp.Price
            //                        });
            //                    }

            //                    if (fp.Qty > 0 && filteredSales.Count() >= Convert.ToInt32(fp.Qty))
            //                    {
            //                        FPList.Add(new FocusPartUserLevel
            //                        {
            //                            Qty = fp.Qty,
            //                            BenefitType = fp.Type,
            //                            BenefitValue = fp.Value,
            //                            TotalSale = totalSale,
            //                            Price = fp.Price
            //                        });
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    if (FPList.Count > 0)
            //    {
            //        var firstFp = FPList.FirstOrDefault();
            //        FocusPartUserLevel focusPart = firstFp.Price > 0 ? FPList.OrderByDescending(f => Convert.ToInt32(f.Price)).Take(1).FirstOrDefault() : FPList.OrderByDescending(f => Convert.ToInt32(f.Qty)).Take(1).FirstOrDefault();
            //        if (focusPart != null && !string.IsNullOrEmpty(focusPart.BenefitType) && !string.IsNullOrEmpty(focusPart.BenefitValue))
            //        {
            //            switch (focusPart.BenefitType)
            //            {
            //                case "Coupons":
            //                    msg = $"You have received {focusPart.BenefitValue} coupons.";
            //                    break;

            //                case "Points":
            //                    msg = $"You have received {focusPart.BenefitValue} points.";
            //                    int.TryParse(focusPart.BenefitValue, out int benefitValue);
            //                    points += benefitValue;
            //                    break;

            //                case "Amount":
            //                    msg = $"You have received ₹ {focusPart.BenefitValue}.";
            //                    break;

            //                case "Gift":
            //                    msg = $"You are eligible for {focusPart.BenefitValue}.";
            //                    break;

            //                case "Percentage":
            //                case "Cashback":
            //                    var amount = focusPart.TotalSale * Convert.ToInt32(focusPart.BenefitValue) / 100;
            //                    msg = $"You have received ₹ {amount}.";
            //                    break;
            //            }

            //            levels.Add(new LevelInfo
            //            {
            //                Message = msg,
            //                LevelAchieved = focusPart != null,
            //                Image = focusPart == null ? general.CheckImageUrl("/assets/images/Level08.png") : general.CheckImageUrl("/assets/images/Level10.png"),
            //                Date = scheme.StartDate.Value,
            //                Type = Constants.FocusPart
            //            });
            //        }
            //    }
            //}

            #endregion

            #region Part 7: Assured Gift

            // Data was saved from 'Assured Gifts' tab within Reward Info for Scheme

            // Show if selected option is not either of lucky draw or cashback
            if (!selectedOption.Equals(luckyDraw, StringComparison.OrdinalIgnoreCase) && !selectedOption.Equals(cashback, StringComparison.OrdinalIgnoreCase) && (assuredGiftVisible || areBothCbAgApplicable))
            {
                var assuredGifts = db.AssuredGifts.Where(x => x.SchemeId == schemeId).ToList();
                if (assuredGifts.Count > 0)
                {
                    // var achievedTarget = sales.Count > 0 ? sales.Sum(s => Convert.ToDouble(s.NetRetailSelling)) : 0;
                    var agByTarget = assuredGifts.Where(x => Convert.ToDouble(x.Target) <= achievedTargetDouble).OrderByDescending(o => Convert.ToDouble(o.Target)).ToList();
                    var asGift = agByTarget?.Take(1).FirstOrDefault();
                    bool isReward = asGift != null;

                    //if (!isReward)
                    //{
                    //    var agByPoint = assuredGifts.Where(x => !string.IsNullOrEmpty(x.Point) && int.Parse(x.Point) <= points).OrderByDescending(o => int.Parse(o.Point)).ToList();
                    //    asGift = agByPoint?.Take(1).FirstOrDefault();
                    //    isReward = asGift != null;
                    //}

                    if (isReward)
                    {
                        msg = $"You are eligible for '{asGift.Reward}'.";
                        levels.Add(new LevelInfo
                        {
                            Message = msg,
                            LevelAchieved = asGift != null,
                            Image = asGift == null ? general.CheckImageUrl("/assets/images/Level08.png") : general.CheckImageUrl("/assets/images/Level10.png"),
                            Date = scheme.StartDate.Value,
                            Type = Constants.AssuredGift
                        });
                    }
                    // Add for show remaining target
                    else
                    {
                        asGift = assuredGifts.OrderBy(o => Convert.ToDouble(o.Target)).FirstOrDefault();
                        if (asGift != null)
                        {
                            var remainingSales = Math.Round(Convert.ToDecimal(asGift.Target) - achievedTarget);
                            msg = $"You have achieved " + Math.Round(achievedTarget) + " target of " + asGift.Target + " and your remaining target for today is " + remainingSales + "eligible for  " + asGift.Reward + ".";

                            levels.Add(new LevelInfo
                            {
                                Message = msg,
                                LevelAchieved = false,
                                Image = general.CheckImageUrl("/assets/images/Level08.png"),
                                Date = scheme.StartDate.Value,
                                Type = Constants.AssuredGift
                            });
                        }
                    }

                }
            }

            #endregion

            // Set level title
            levels = levels.OrderBy(o => o.Date).ToList();
            var levelCounter = 0;
            foreach (var level in levels)
            {
                levelCounter++;
                level.Title = $"Level {levelCounter}";
            }

            return new AppLevelDataModel
            {
                TopMessage = topMsg,
                Levels = levels,
                TargetAchieved = achievedTarget,
                TargetAchievedPercentage = $"{targetWs.TargetAchievedPercentage ?? 0}%"
            };
        }

        private LevelInfo GetLevelInfoForCashback(IEnumerable<CashBack> cashbackList, double achievedTarget, DateTime schemeStartDate, DateTime startDate, DateTime endDate, double wsTarget)
        {
            LevelInfo levelInfo = null;
            var cashbackAmt = 0.0;
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
                        cashbackAmt = achievedTarget * cashbackBenefit.Value / 100;
                    }
                    else
                    {
                        // Might extraSalePercent didn't fit in range
                        // Try setting cashback amount by using maximum value of percentage
                        cashbackBenefit = cashbackBenefits.OrderByDescending(cb => cb.ToAmount).Take(1).FirstOrDefault();
                        if (cashbackBenefit != null && extraSalePercent > cashbackBenefit.ToAmount)
                        {
                            cashbackAmt = achievedTarget * cashbackBenefit.Value / 100;
                        }
                    }
                }
                // add new for showing message to get extra benifits
                else if (cashbackBenefits.Count > 0 && extraSale < 0)
                {
                    var cashbackBenefit = cashbackBenefits.Take(1).FirstOrDefault();

                    var extraCashback = (wsTarget + cashbackBenefit.FromAmount) * cashbackBenefit.Value / 100;
                    var benifitTarget = (wsTarget + cashbackBenefit.FromAmount) - achievedTarget;
                    // set message to get cashback

                    var msg = "From " + startDate.ToString("dd-MM-yyyy") + " to " + endDate.ToString("dd-MM-yyyy") + ", You have made sale of ₹ " + achievedTarget + ".You need sales of ₹ " + benifitTarget + " more to be eligible for ₹ " + extraCashback + " cashback.";
                    levelInfo = new LevelInfo
                    {
                        Message = msg,
                        LevelAchieved = false,
                        Image = cashbackAmt == 0
                            ? general.CheckImageUrl("/assets/images/Level08.png")
                            : general.CheckImageUrl("/assets/images/Level10.png"),
                        Date = schemeStartDate,
                        Type = Constants.Cashback
                    };
                }
            }

            if (cashbackAmt > 0)
            {
                var msg = "From " + startDate.ToString("dd-MM-yyyy") + " to " + endDate.ToString("dd-MM-yyyy") + ", You have received ₹ " + cashbackAmt + " as cashback.";
                levelInfo = new LevelInfo
                {
                    Message = msg,
                    LevelAchieved = cashbackAmt > 0,
                    Image = cashbackAmt == 0
                        ? general.CheckImageUrl("/assets/images/Level08.png")
                        : general.CheckImageUrl("/assets/images/Level10.png"),
                    Date = schemeStartDate,
                    Type = Constants.Cashback
                };
            }

            return levelInfo;
        }

        #endregion

        #region Schemes Status By SchemeId
        public AppSchemeStatusDataModel GetSchemeStatus(SchemeUserLevelModel model)
        {
            var workShopUser = db.WorkshopsUsers.FirstOrDefault(x => x.UserId == model.UserId);
            var workShopDistributor = db.DistributorWorkShops.FirstOrDefault(x => x.UserId == model.UserId);
            if (workShopUser == null && workShopDistributor == null) return null;
            if (workShopDistributor != null)
            {
                workShopUser = new WorkshopsUser
                {
                    WorkshopId = workShopDistributor.WorkShopId ?? 0
                };
            }

            var aldModel = GetWorkshopLevels(model.SchemeId, workShopUser.WorkshopId);
            if (aldModel.Levels.Count == 0) return null;

            var achieveMessage = aldModel.Levels.Where(x => x.Type == Constants.TargetWorkShop).OrderByDescending(l => l.Date).Select(x => x.Message).Take(1).FirstOrDefault();
            var giftemessage = aldModel.Levels.Where(x => x.Type == Constants.AssuredGift).OrderByDescending(l => l.Date).Select(x => x.Message).Take(1).FirstOrDefault();

            return new AppSchemeStatusDataModel
            {
                SchemeMessage = achieveMessage ?? "",
                GiftMessage = giftemessage ?? ""
            };
        }
        #endregion

        #region Get Scheme Focus-Parts-Groups 
        public List<SchemeFocusPart> GetSchemeFocusPartsGroupsPagination(FocusPartGroupsRequest model, out int totalRecords)
        {
            const string focusPart = "Focus Parts Group";
            const string allParts = "All Parts";
            const string fms = "FMS Parts Group";
            int count = 0;

            var scheme = db.Schemes.Where(s => s.SchemeId == model.SchemeId).AsNoTracking().FirstOrDefault();

            var searchGroupIds = db.ProductGroups.Where(s => s.DistributorId == scheme.DistributorId && s.GroupName.StartsWith(model.GroupsSearchText)).AsNoTracking().Select(g => g.GroupId).ToList();

            var focusPartsGroups = new List<SchemeFocusPart>();
            var groupIds = new List<int>();

            // find focus-parts
            if (scheme != null && scheme.PartType.Equals(focusPart, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(model.GroupsSearchText))
                {
                    groupIds = db.FocusParts.Where(x => x.SchemeId == scheme.SchemeId).AsNoTracking().GroupBy(f => f.GroupId).Select(g => g.Key.Value).ToList();
                }
                else
                {
                    groupIds = db.FocusParts.Where(x => x.SchemeId == scheme.SchemeId && x.GroupId != null && searchGroupIds.Contains(x.GroupId.Value)).AsNoTracking().GroupBy(f => f.GroupId).Select(g => g.Key.Value).ToList();
                }


                count = groupIds.Count();

                groupIds = groupIds.Skip((model.PageNumber - 1) * model.PageSize.Value).Take(model.PageSize.Value).ToList();

                var productIds = db.FocusParts.Where(x => groupIds.Contains(x.GroupId.Value) && x.SchemeId == model.SchemeId).AsNoTracking().Select(f => f.ProductId).ToList();

                focusPartsGroups = db.Products.Where(p => (groupIds.Contains(p.GroupId.Value) || productIds.Contains(p.ProductId)) && p.DistributorId == scheme.DistributorId).Include(x => x.ProductGroup).Include(x => x.Brand).AsNoTracking().Select(s => new SchemeFocusPart
                {
                    PartGroup = s.ProductGroup != null ? s.ProductGroup.GroupName : "NA",
                    PartNumber = s.PartNo,
                    PartDescription = s.Description,
                    MRP = s.Price > 0 ? s.Price.ToString() : "NA",
                    ProductId = s.ProductId,
                    Price = s.Price ?? 0,
                    Stock = s.CurrentStock ?? 0,
                    IsOriparts = s.Brand != null ? s.Brand.IsOriparts ?? false : false
                }).ToList();

            }

            // find all parts for scheme respective distributor
            else if (scheme != null && scheme.PartType.Equals(allParts, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(model.GroupsSearchText))
                {
                    groupIds = db.ProductGroups.Where(x => x.DistributorId == scheme.DistributorId).AsNoTracking().Select(g => g.GroupId).ToList();
                }
                else
                {
                    groupIds = db.ProductGroups.Where(x => x.DistributorId == scheme.DistributorId && searchGroupIds.Contains(x.GroupId)).AsNoTracking().Select(g => g.GroupId).ToList();
                }

                count = groupIds.Count();

                groupIds = groupIds.Skip((model.PageNumber - 1) * model.PageSize.Value).Take(model.PageSize.Value).ToList();

                focusPartsGroups = db.Products.Where(p => groupIds.Contains(p.GroupId.Value) && p.DistributorId == scheme.DistributorId).Include(x => x.ProductGroup).Include(x => x.Brand).AsNoTracking().Select(s => new SchemeFocusPart
                {
                    PartGroup = s.ProductGroup != null ? s.ProductGroup.GroupName : "NA",
                    PartNumber = s.PartNo,
                    PartDescription = s.Description,
                    MRP = s.Price > 0 ? s.Price.ToString() : "NA",
                    ProductId = s.ProductId,
                    Price = s.Price ?? 0,
                    Stock = s.CurrentStock ?? 0,
                    IsOriparts = s.Brand != null ? s.Brand.IsOriparts ?? false : false
                }).ToList();
            }

            else if (scheme != null && scheme.PartType.Equals(fms, StringComparison.OrdinalIgnoreCase))
            {
                var workshops = GetWorkshopsForScheme(scheme);
                var twsIds = workshops.Select(w => w.WorkShopId);

                var saleData = db.DailySalesTrackerWithInvoiceDatas.Where(a => a.WorkShopId != null && twsIds.Contains(a.WorkShopId.Value) && searchGroupIds.Contains(a.GroupId.Value) && !string.IsNullOrEmpty(a.NetRetailQty) && a.CreatedDate != null && a.CreatedDate >= scheme.StartRange && a.CreatedDate <= scheme.EndRange && a.ProductId != null).AsNoTracking().Select(s => new
                {
                    s.ProductId,
                    s.NetRetailQty
                }).ToList();


                if (string.IsNullOrEmpty(model.GroupsSearchText))
                {
                    saleData = db.DailySalesTrackerWithInvoiceDatas.Where(a => a.WorkShopId != null && twsIds.Contains(a.WorkShopId.Value) && !string.IsNullOrEmpty(a.NetRetailQty) && a.CreatedDate != null && a.CreatedDate >= scheme.StartRange && a.CreatedDate <= scheme.EndRange && a.ProductId != null).AsNoTracking().Select(s => new
                    {
                        s.ProductId,
                        s.NetRetailQty
                    }).ToList();
                }

                var sales = saleData.OrderByDescending(a => Convert.ToDecimal(a.NetRetailQty)).GroupBy(p => p.ProductId).ToList();

                var filteredSalesProductIds = sales.Select(s => s.Key.Value).ToList();

                if (scheme.PartCreations != "All")
                {
                    // TODO: Calculate total count of all items of all sales
                    decimal totalQty = sales.Sum(s => s.Sum(x => Convert.ToDecimal(x.NetRetailQty)));

                    decimal percentage = 0.0M;
                    switch (scheme.PartCreations)
                    {
                        case "F":
                            percentage = scheme.FValue ?? 0;
                            break;
                        case "M":
                            percentage = scheme.MValue ?? 0;
                            break;
                        default: // "S"
                            percentage = scheme.SValue ?? 0;
                            break;
                    }

                    // Then calculate F, M, S % values. For e.g. 75% (F) of total count = 75
                    var targetQty = totalQty * percentage / 100;

                    filteredSalesProductIds = new List<int>();

                    // Now loop sale record till when sum of quantity of sale not equal to 75
                    var saleQtySum = 0.0M;
                    foreach (var sale in sales)
                    {
                        if (targetQty <= saleQtySum) break;
                        filteredSalesProductIds.Add(sale.Key.Value);

                        saleQtySum += sale.Sum(s => Convert.ToDecimal(s.NetRetailQty));
                    }
                }
                // add groups and products in list
                var records = db.Products.Where(p => filteredSalesProductIds.Contains(p.ProductId)).Include(x => x.ProductGroup).Include(x => x.Brand).AsNoTracking().Select(s => new
                {
                    GroupId = s.GroupId,
                    PartGroup = s.ProductGroup != null ? s.ProductGroup.GroupName : "NA",
                    PartNumber = s.PartNo,
                    PartDescription = s.Description,
                    MRP = s.Price > 0 ? s.Price.ToString() : "NA",
                    ProductId = s.ProductId,
                    Price = s.Price ?? 0,
                    Stock = s.CurrentStock ?? 0,
                    IsOriparts = s.Brand != null ? s.Brand.IsOriparts ?? false : false
                }).ToList();

                groupIds = records.GroupBy(g => g.GroupId).Select(g => g.Key.Value).ToList();

                count = groupIds.Count();

                groupIds = groupIds.Skip((model.PageNumber - 1) * model.PageSize.Value).Take(model.PageSize.Value).ToList();

                focusPartsGroups = records.Where(p => groupIds.Contains(p.GroupId.Value)).Select(s => new SchemeFocusPart
                {
                    PartGroup = s.PartGroup,
                    PartNumber = s.PartNumber,
                    PartDescription = s.PartDescription,
                    MRP = s.MRP,
                    ProductId = s.ProductId,
                    Price = s.Price,
                    Stock = s.Stock,
                    IsOriparts = s.IsOriparts
                }).ToList();
            }


            totalRecords = count;
            if (model.TempOrderId.HasValue)
            {
                var tempOrderDetails = db.TempOrderDetails.Where(a => a.TempOrderId == model.TempOrderId.Value).AsNoTracking().ToList();
                if (tempOrderDetails.Any())
                {
                    foreach (var fpg in focusPartsGroups)
                    {
                        var cartItem = tempOrderDetails.Where(a => a.ProductId == fpg.ProductId).FirstOrDefault();

                        fpg.CartQty = cartItem != null ? cartItem.Qty : 0;
                        fpg.CartAvailabilityType = cartItem != null ? cartItem.AvailabilityType : "";
                        fpg.CartOutletId = cartItem != null ? cartItem.OutletId : null;
                    }
                }
            }
            return focusPartsGroups;
        }

        /// <summary>
        /// Get all target workshops for particular scheme based on branch codes or sales executives or distributor Id. Further filtered by customer types.
        /// </summary>
        /// <param name="scheme">The scheme for which target workshops need to be retrieved.</param>
        private List<ResponseTargetWorkshopModel> GetWorkshopsForScheme(Scheme scheme)
        {
            // Get workshops by distributor
            List<ResponseTargetWorkshopModel> workshops;
            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                workshops = (from w in context.WorkShops.AsNoTracking()
                             join d in context.DistributorWorkShops.AsNoTracking() on w.WorkShopId equals d.WorkShopId
                             join u in context.UserDetails.AsNoTracking() on d.UserId equals u.UserId
                             where d.DistributorId == scheme.DistributorId && (u.IsDeleted == null || u.IsDeleted.Value == false)
                             select new ResponseTargetWorkshopModel
                             {
                                 WorkShopCode = u.ConsPartyCode,
                                 WorkShopId = w.WorkShopId,
                                 WorkShopName = w.WorkShopName,
                                 CustomerType = w.Type,
                                 OutletId = w.outletId ?? 0,
                             }).Distinct().ToList();
            }

            // Filter workshops by sales executives
            if (!string.IsNullOrEmpty(scheme.SalesExecutiveId))
            {
                var salesExecutives = scheme.SalesExecutiveId.Split(',');
                var seWsIds = new List<int>();
                using (var context = new garaazEntities())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;

                    seWsIds = context.SalesExecutiveWorkshops.Where(se => salesExecutives.Contains(se.UserId)).AsNoTracking().Select(se => se.WorkshopId).Distinct().ToList();
                }
                workshops = workshops.Where(w => seWsIds.Contains(w.WorkShopId)).ToList();
            }

            // Filter workshops by outlets or branches 
            if (!string.IsNullOrEmpty(scheme.BranchCode))
            {
                var outletIds = scheme.BranchCode.Split(',').Select(int.Parse);
                workshops = workshops.Where(w => outletIds.Contains(w.OutletId)).ToList();
            }

            // Filter workshops by customer type
            if (!string.IsNullOrEmpty(scheme.PartyType))
            {
                var customerTypes = scheme.PartyType.Split(',');
                workshops = workshops.Where(w => customerTypes.Contains(w.CustomerType, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            // Set human readable text
            workshops.ForEach(w => w.CustomerType = w.CustomerType?.Transform(To.LowerCase, To.TitleCase));

            // Get new workshops and add to saved one
            var rl = new RepoLabels();
            var savedWorkshops = rl.GetSavedTargetWorkshops(scheme.SchemeId);
            if (savedWorkshops.Count > 0 && workshops.Count > 0)
            {
                var dynamicWsIds = workshops.Select(w => w.WorkShopId).ToList();

                // Filter workshop from saved workshops that matches with dynamic one
                var matchingSavedWs = savedWorkshops.Where(w => dynamicWsIds.Contains(w.WorkShopId)).ToList();

                var matchingSavedWsIds = matchingSavedWs.Select(w => w.WorkShopId);
                var newWorkshops = workshops.Where(w => !matchingSavedWsIds.Contains(w.WorkShopId)).ToList();

                matchingSavedWs.AddRange(newWorkshops);
                return matchingSavedWs;
            }

            return workshops;
        }

        #endregion   

        #region Get Scheme Focus-Parts-Groups Html
        public string GetSchemeFocusPartsGroupsHtml(int schemeId)
        {
            var focusPartsGroups = GetSchemeFocusParts(schemeId);

            //// bind data in html table
            //var htmlTable = groups.ToHtmlTable(new { @class = "table table-bordered" });

            //// Replace double quote with single quote
            //htmlTable = htmlTable.Replace("\"", "'");

            //var html = $"<html><head><link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css' integrity='sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm' crossorigin='anonymous'></head><body>{htmlTable}</body></html>";

            // return html;           

            var groups = focusPartsGroups.GroupBy(a => a.PartGroup);
            // bind groups in html table            
            string groupTable = "<div class='d-flex flex-wrap bg-light'>";
            if (groups.Count() == 0)
            {
                groupTable += "<h5> No focus-parts-group found.</h5>";
            }

            StringBuilder sb = new StringBuilder();
            foreach (var group in groups)
            {
                groupTable += $"<div class='p-2 border'>{group.Key}</div>";


                sb.Append($"<div class='bg-info text-light pt-2 pb-2'><span class='ml-2'>Group: {group.Key}</span></div>");

                sb.Append("<TABLE class='table table-bordered'>");
                sb.Append("<TR>");
                sb.Append("<TH>");
                sb.Append("Sl.No.");
                sb.Append("</TH>");
                sb.Append("<TH>");
                sb.Append("PartNumber");
                sb.Append("</TH>");
                sb.Append("<TH>");
                sb.Append("PartDescription");
                sb.Append("</TH>");
                sb.Append("<TH>");
                sb.Append("MRP");
                sb.Append("</TH>");
                sb.Append("</TR>");
                var products = focusPartsGroups.Where(a => a.PartGroup == group.Key);
                int num = 1;
                foreach (var part in products)
                {
                    sb.Append("<TR>");
                    sb.Append("<TD>");
                    sb.Append(num++);
                    sb.Append("</TD>");
                    sb.Append("<TD>");
                    sb.Append(part.PartNumber);
                    sb.Append("</TD>");
                    sb.Append("<TD>");
                    sb.Append(part.PartDescription);
                    sb.Append("</TD>");
                    sb.Append("<TD>");
                    sb.Append(part.MRP);
                    sb.Append("</TD>");
                    sb.Append("</TR>");
                }
                sb.Append("</TABLE>");
            }
            groupTable += "</div>";

            var mainTable = groupTable + sb.ToString();

            var html2 = $"<html><head><link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css' integrity='sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm' crossorigin='anonymous'></head><body>{mainTable}</body></html>";
            return html2;
        }

        private List<SchemeFocusPart> GetSchemeFocusParts(int schemeId)
        {
            const string focusPart = "Focus Parts Group";
            const string allParts = "All Parts";
            const string fms = "FMS Parts Group";

            var scheme = db.Schemes.Where(s => s.SchemeId == schemeId).AsNoTracking().FirstOrDefault();
            var focusPartsGroups = new List<SchemeFocusPart>();
            var groupIds = new List<int?>();

            if (scheme != null && scheme.PartType.Equals(focusPart, StringComparison.OrdinalIgnoreCase))
            {
                var fps = db.FocusParts.Where(x => x.SchemeId == scheme.SchemeId).AsNoTracking().ToList();

                groupIds = fps.GroupBy(g => g.GroupId).Select(g => g.Key).ToList();
                var productIds = fps.Select(g => g.ProductId).ToList();

                focusPartsGroups = db.Products.Where(p => (groupIds.Contains(p.GroupId) || productIds.Contains(p.ProductId)) && p.DistributorId == scheme.DistributorId).Include(x => x.ProductGroup).AsNoTracking().Select(s => new SchemeFocusPart
                {
                    PartGroup = s.ProductGroup != null ? s.ProductGroup.GroupName : "NA",
                    PartNumber = s.PartNo,
                    PartDescription = s.Description,
                    MRP = s.Price > 0 ? s.Price.ToString() : "NA",
                }).ToList();

            }

            // find all parts for scheme respective distributor
            else if (scheme != null && scheme.PartType.Equals(allParts, StringComparison.OrdinalIgnoreCase))
            {
                var groupIdss = db.ProductGroups.Where(x => x.DistributorId == scheme.DistributorId).AsNoTracking().Select(g => g.GroupId).ToList();

                focusPartsGroups = db.Products.Where(p => groupIdss.Contains(p.GroupId.Value) && p.DistributorId == scheme.DistributorId).Include(x => x.ProductGroup).AsNoTracking().Select(s => new SchemeFocusPart
                {
                    PartGroup = s.ProductGroup != null ? s.ProductGroup.GroupName : "NA",
                    PartNumber = s.PartNo,
                    PartDescription = s.Description,
                    MRP = s.Price > 0 ? s.Price.ToString() : "NA",
                }).ToList();
            }

            else if (scheme != null && scheme.PartType.Equals(fms, StringComparison.OrdinalIgnoreCase))
            {
                var workshops = GetWorkshopsForScheme(scheme);
                var twsIds = workshops.Select(w => w.WorkShopId);

                var saleData = db.DailySalesTrackerWithInvoiceDatas.Where(a => a.WorkShopId != null && twsIds.Contains(a.WorkShopId.Value) && !string.IsNullOrEmpty(a.NetRetailQty) && a.CreatedDate != null && a.CreatedDate >= scheme.StartRange && a.CreatedDate <= scheme.EndRange && a.ProductId != null).AsNoTracking().Select(s => new
                {
                    s.ProductId,
                    s.NetRetailQty
                }).ToList();


                var sales = saleData.OrderByDescending(a => Convert.ToDecimal(a.NetRetailQty)).GroupBy(p => p.ProductId).ToList();

                var filteredSalesProductIds = new List<int>();

                filteredSalesProductIds = sales.Select(s => s.Key.Value).ToList();

                if (scheme.PartCreations != "All")
                {
                    // TODO: Calculate total count of all items of all sales
                    decimal totalQty = sales.Sum(s => s.Sum(x => Convert.ToDecimal(x.NetRetailQty)));

                    decimal percentage = 0.0M;
                    switch (scheme.PartCreations)
                    {
                        case "F":
                            percentage = scheme.FValue ?? 0;
                            break;
                        case "M":
                            percentage = scheme.MValue ?? 0;
                            break;
                        default: // "S"
                            percentage = scheme.SValue ?? 0;
                            break;
                    }

                    // Then calculate F, M, S % values. For e.g. 75% (F) of total count = 75
                    var targetQty = totalQty * percentage / 100;

                    filteredSalesProductIds = new List<int>();

                    // Now loop sale record till when sum of quantity of sale not equal to 75
                    var saleQtySum = 0.0M;
                    foreach (var sale in sales)
                    {
                        if (targetQty <= saleQtySum) break;
                        filteredSalesProductIds.Add(sale.Key.Value);

                        saleQtySum += sale.Sum(s => Convert.ToDecimal(s.NetRetailQty));
                    }
                }
                // add groups and products in list
                focusPartsGroups = db.Products.Where(p => filteredSalesProductIds.Contains(p.ProductId)).Include(x => x.ProductGroup).AsNoTracking().Select(s => new SchemeFocusPart
                {
                    PartGroup = s.ProductGroup != null ? s.ProductGroup.GroupName : "NA",
                    PartNumber = s.PartNo,
                    PartDescription = s.Description,
                    MRP = s.Price > 0 ? s.Price.ToString() : "NA",
                }).ToList();
            }

            return focusPartsGroups;
        }

        #endregion
    }
}