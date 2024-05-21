using Garaaz.Models.AppNotification;
using Garaaz.Models.Response.SchemeDescription;
using Garaaz.Models.Schemes;
using Humanizer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Garaaz.Models
{
    public class RepoSchemes
    {
        private const string Cashback = "Cashback";
        private const string LuckyDraw = "Lucky Draw";
        private const string AssuredGift = "Assured Gift";

        readonly garaazEntities db = new garaazEntities();

        #region Schemes

        #region GetAllSchemes

        public List<Scheme> GetAllScheme()
        {
            return db.Schemes.Where(x => x.IsDeleted == false || x.IsDeleted == null).OrderByDescending(s => s.CreatedDate).ToList();
        }
        public List<Scheme> GetAllScheme(int distributorId)
        {
            return db.Schemes.Where(x => (x.IsDeleted == false || x.IsDeleted == null) && x.DistributorId == distributorId).OrderByDescending(s => s.CreatedDate).ToList();
        }
        #endregion

        #region GetAllSchemesByUserId
        public List<ResponseSchemeModel> GetAllSchemeByUserId(string UserId)
        {
            var Role = db.AspNetUsers.Where(x => x.Id == UserId).FirstOrDefault().AspNetRoles.Select(a => a.Name).ToString();

            if (Role == Constants.SuperAdmin)
            {
                return db.Schemes.Where(x => x.IsDeleted == false).Select(a => new ResponseSchemeModel
                {
                    SchemeId = a.SchemeId,
                    SchemeName = a.SchemeName,
                    StartDate = a.StartDate == null ? "" : a.StartDate.Value.ToString(),
                    EndDate = a.EndDate == null ? "" : a.EndDate.Value.ToString(),
                    StartRange = a.StartRange == null ? "" : a.StartRange.Value.ToString(),
                    EndRange = a.EndRange == null ? "" : a.EndRange.Value.ToString(),
                    DispersalFrequency = a.DispersalFrequency,
                    PaybackType = a.SchemesType,
                    SchemeType = a.Type,
                    FValue = a.FValue ?? 0,
                    MValue = a.MValue ?? 0,
                    SValue = a.SValue ?? 0,
                }).ToList();
            }

            return db.Schemes.Where(x => x.UserId == UserId || x.IsDeleted == false).Select(a => new ResponseSchemeModel
            {
                SchemeId = a.SchemeId,
                SchemeName = a.SchemeName,
                StartDate = a.StartDate == null ? "" : a.StartDate.Value.ToString(),
                EndDate = a.EndDate == null ? "" : a.EndDate.Value.ToString(),
                StartRange = a.StartRange == null ? "" : a.StartRange.Value.ToString(),
                EndRange = a.EndRange == null ? "" : a.EndRange.Value.ToString(),
                DispersalFrequency = a.DispersalFrequency,
                PaybackType = a.SchemesType,
                SchemeType = a.Type,
                FValue = a.FValue ?? 0,
                MValue = a.MValue ?? 0,
                SValue = a.SValue ?? 0,
            }).ToList();
        }
        #endregion

        #region Get all schemes by UserId and Role

        public List<SchemeMain> GetAllSchemeByUserIdAndRole(string userId, string role)
        {
            var schemeMainList = new List<SchemeMain>();

            var currentSchemes = new SchemeMain
            {
                Name = "Current Schemes",
                Value = 1,
                data = new List<AppSchemeModel>()
            };
            var pastSchemes = new SchemeMain
            {
                Name = "Recently Concluded Schemes",
                Value = 2,
                data = new List<AppSchemeModel>()
            };

            var general = new General();
            IQueryable<Scheme> schemes = null;

            if (role == Constants.SuperAdmin)
            {
                schemes = db.Schemes.Where(s => s.IsActive == true && (s.IsDeleted == false || s.IsDeleted == null)).AsNoTracking();
            }
            else if (role == Constants.Distributor)
            {
                var distId = general.GetDistributorId(userId);
                schemes = db.Schemes.Where(s => s.IsActive == true && s.DistributorId == distId && (s.IsDeleted == false || s.IsDeleted == null)).AsNoTracking();
            }
            else if (role == Constants.RoIncharge)
            {
                var distributorsOutlet = db.DistributorsOutlets.Where(o => o.UserId == userId).AsNoTracking().FirstOrDefault();
                if (distributorsOutlet != null)
                {
                    schemes = db.Schemes.Where(s => (s.IsDeleted == false || s.IsDeleted == null) && (s.IsDeleted == false || s.IsDeleted == null) && s.BranchCode.Contains(distributorsOutlet.OutletId.ToString())).AsNoTracking();
                }
            }
            else if (role == Constants.SalesExecutive)
            {
                schemes = db.Schemes.Where(s => (s.IsDeleted == false || s.IsDeleted == null) && (s.IsDeleted == false || s.IsDeleted == null) && s.SalesExecutiveId.Contains(userId)).AsNoTracking();
            }
            else if (role == Constants.Workshop || role == Constants.WorkshopUsers || role == Constants.Users)
            {
                var ru = new RepoUsers();
                var wsId = general.GetWorkshopId(userId);
                var distWs = ru.getDistributorByWorkShopId(wsId).FirstOrDefault();
                schemes = from s in db.Schemes.AsNoTracking()
                          join tw in db.TargetWorkShops.AsNoTracking() on s.SchemeId equals tw.SchemeId
                          where s.IsActive == true && s.DistributorId == distWs.DistributorId && (s.IsDeleted == false || s.IsDeleted == null)
                                && tw.WorkShopId == wsId
                          select s;
            }
            else
            {
                schemes = db.Schemes.Where(x => x.IsActive == true && (x.UserId == userId || x.IsDeleted == false)).AsNoTracking();
            }

            if (schemes != null)
            {
                foreach (var scheme in schemes)
                {
                    var onwardsMessage = string.Empty;

                    var startDate = "";
                    if (scheme.StartDate.HasValue)
                    {
                        startDate = Convert.ToDateTime(scheme.StartDate).ToString("dd MMM, yyyy");
                        var schemeStartDate = Convert.ToDateTime(scheme.StartDate);
                        onwardsMessage = schemeStartDate > DateTime.Now ? $"{schemeStartDate:dd MMM, yyyy} onwards" : $"Started from {startDate}";
                    }

                    var endDate = "";
                    if (scheme.EndDate.HasValue)
                    {
                        endDate = Convert.ToDateTime(scheme.EndDate).ToString("dd MMM, yyyy");
                    }

                    if (scheme.EndDate.HasValue && scheme.EndDate < Convert.ToDateTime(DateTime.Now.ToString("dd MMM, yyyy")))
                    {
                        pastSchemes.data.Add(new AppSchemeModel
                        {
                            SchemeId = scheme.SchemeId,
                            SchemeName = scheme.SchemeName,
                            OnwardsMessage = onwardsMessage,
                            StartDate = startDate,
                            EndDate = endDate,
                            BannerImage = scheme.ThumbnailImage
                        });
                    }
                    else
                    {
                        currentSchemes.data.Add(new AppSchemeModel
                        {
                            SchemeId = scheme.SchemeId,
                            SchemeName = scheme.SchemeName,
                            OnwardsMessage = onwardsMessage,
                            StartDate = startDate,
                            EndDate = endDate,
                            BannerImage = scheme.ThumbnailImage
                        });
                    }
                }

                currentSchemes.data.ForEach(a => { a.BannerImage = general.CheckImageUrl(!string.IsNullOrEmpty(a.BannerImage) ? a.BannerImage : "/assets/images/NoPhotoAvailable.png"); });
                pastSchemes.data.ForEach(a => { a.BannerImage = general.CheckImageUrl(!string.IsNullOrEmpty(a.BannerImage) ? a.BannerImage : "/assets/images/NoPhotoAvailable.png"); });
            }

            schemeMainList.Add(currentSchemes);
            schemeMainList.Add(pastSchemes);

            return schemeMainList;
        }
        #endregion       

        #region Get Scheme By Scheme Id
        public ResponseSchemeModel GetSchemeBySchemeId(int schemeId)
        {
            var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == schemeId);
            if (scheme == null) return null;

            var rsm = new ResponseSchemeModel
            {
                // Basic info
                SchemeId = scheme.SchemeId,
                SchemeName = scheme.SchemeName,
                SchemeType = scheme.Type,
                SubSchemeType = scheme.SubSchemeType,
                AssuredGift = scheme.AssuredGift,
                CashBack = scheme.CashBack,
                LuckyDraw = scheme.LuckyDraw,
                DistributorId = scheme.DistributorId,
                StartDate = scheme.StartDate == null ? "" : scheme.StartDate.Value.ToString(),
                EndDate = scheme.EndDate == null ? "" : scheme.EndDate.Value.ToString(),
                DispersalFrequency = scheme.DispersalFrequency,
                PaybackType = scheme.SchemesType,
                ThumbnailImage = scheme.ThumbnailImage,
                BannerImage = scheme.BannerImage,

                // Part Info
                PartCategory = scheme.PartCategory,
                PartType = scheme.PartType,
                FocusPartBenifitType = scheme.FocusPartBenifitType,
                FocusPartBenifitTypeValue = scheme.FocusPartBenifitTypeValue,
                FocusPartTarget = scheme.FocusPartTarget ?? 0,
                FocusPartBenifitTypeNumber = scheme.FocusPartBenifitTypeNumber,
                StartRange = scheme.StartRange != null ? scheme.StartRange.Value.ToString() : string.Empty,
                EndRange = scheme.EndRange != null ? scheme.EndRange.Value.ToString() : string.Empty,
                FValue = scheme.FValue ?? 0,
                MValue = scheme.MValue ?? 0,
                SValue = scheme.SValue ?? 0,
                PartCreations = scheme.PartCreations,

                // Customer Segment
                BranchCode = scheme.BranchCode,
                SalesExecutiveId = scheme.SalesExecutiveId,
                PartyType = scheme.PartyType,

                // Target Info
                PrevYearFromDate = scheme.PrevYearFromDate == null ? "" : scheme.PrevYearFromDate.Value.ToString(),
                PrevYearToDate = scheme.PrevYearToDate == null ? "" : scheme.PrevYearToDate.Value.ToString(),
                PrevMonthFromDate = scheme.PrevMonthFromDate == null ? "" : scheme.PrevMonthFromDate.Value.ToString(),
                PrevMonthToDate = scheme.PrevMonthToDate == null ? "" : scheme.PrevMonthToDate.Value.ToString(),
                GrowthCompPercentMinValue = scheme.GrowthCompPercentMinValue,
                GrowthCompPercentBaseValue = scheme.GrowthCompPercentBaseValue,

                // Reward Info
                CashbackCriteria = scheme.CashbackCriteria,
                AreBothCbAgApplicable = scheme.AreBothCbAgApplicable,
                AreBothAgLdApplicable = scheme.AreBothAgLdApplicable,

                // Others
                IsFocusPartApplicable = scheme.FocusPartApplicable,
                SchemeFor = scheme.SchemeFor,
                TargetWorkshopCriteria = scheme.TargetWorkshopCriteria,
                schemeLocations = scheme.SchemeLocations.ToList(),
                TargetCriteria = scheme.TargetCriteria,
                RoInchargeId = scheme.RoInchargeId,
                IsAllRoInchargeSelected = scheme.IsAllRoInchargeSelected,
                IsAllSalesExecutiveSelected = scheme.IsAllSalesExecutiveSelected
            };

            // Warning - Do not change format of the dates. Else date picker extension will not be able to show correct date
            const string dateFormat = "MM/dd/yyyy";

            rsm.StartDate = !string.IsNullOrEmpty(rsm.StartDate) ? Convert.ToDateTime(rsm.StartDate).ToString(dateFormat) : "";
            rsm.EndDate = !string.IsNullOrEmpty(rsm.EndDate) ? Convert.ToDateTime(rsm.EndDate).ToString(dateFormat) : "";

            rsm.StartRange = !string.IsNullOrEmpty(rsm.StartRange) ? Convert.ToDateTime(rsm.StartRange).ToString(dateFormat) : "";
            rsm.EndRange = !string.IsNullOrEmpty(rsm.EndRange) ? Convert.ToDateTime(rsm.EndRange).ToString(dateFormat) : "";

            rsm.PrevYearFromDate = !string.IsNullOrEmpty(rsm.PrevYearFromDate) ? Convert.ToDateTime(rsm.PrevYearFromDate).ToString(dateFormat) : "";
            rsm.PrevYearToDate = !string.IsNullOrEmpty(rsm.PrevYearToDate) ? Convert.ToDateTime(rsm.PrevYearToDate).ToString(dateFormat) : "";
            rsm.PrevMonthFromDate = !string.IsNullOrEmpty(rsm.PrevMonthFromDate) ? Convert.ToDateTime(rsm.PrevMonthFromDate).ToString(dateFormat) : "";
            rsm.PrevMonthToDate = !string.IsNullOrEmpty(rsm.PrevMonthToDate) ? Convert.ToDateTime(rsm.PrevMonthToDate).ToString(dateFormat) : "";

            var lc = db.LabelCriterias.Where(l => l.SchemeId == schemeId).ToList();
            if (lc.Any())
            {
                rsm.LabelCriterias = lc;
            }

            return rsm;
        }

        /// <summary>
        /// Get the scheme by scheme id.
        /// </summary>
        /// <param name="schemeId">The scheme id to filter schemes.</param>
        /// <returns>Return instance of Scheme.</returns>
        public Scheme GetScheme(int schemeId)
        {
            return db.Schemes.FirstOrDefault(x => (x.IsDeleted == null || x.IsDeleted == false) && x.SchemeId == schemeId);
        }
        #endregion

        #region GetAllSchemesByUserId
        public List<SchemeLocation> GetSchemeLocations(int SchemeId)
        {
            return db.SchemeLocations.Where(a => a.SchemeId == SchemeId).ToList();
        }
        #endregion

        #region Delete Schemes
        public bool DeleteScheme(int SchemeId)
        {
            var objScheme = db.Schemes.Where(x => x.SchemeId == SchemeId).FirstOrDefault();
            if (objScheme != null)
            {
                objScheme.IsDeleted = true;
            }
            db.SaveChanges();
            return true;
        }
        #endregion

        #region Add/Update Schemes
        public object SaveOrUpdateScheme(SchemeModel model)
        {
            Scheme scheme;
            if (model.SchemeId > 0)
            {
                scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == model.SchemeId);
                if (scheme != null)
                {
                    // Basic information
                    scheme.SchemeName = model.SchemeName;
                    scheme.Type = model.Types; // Type of the scheme (Cashback,Fixed price)
                    scheme.SubSchemeType = model.SubSchemeType;
                    scheme.AssuredGift = model.IsAssuredGift;
                    scheme.CashBack = model.IsCashBack;
                    scheme.LuckyDraw = model.IsLuckyDraw;
                    scheme.DistributorId = model.DistributorId;
                    scheme.StartDate = model.StartDate;
                    scheme.EndDate = model.EndDate;
                    scheme.DispersalFrequency = model.DispersalFrequency; // Payback Period
                    scheme.SchemesType = model.SchemesType; // Payback Type
                    scheme.ThumbnailImage = model.ThumbnailImage;
                    scheme.BannerImage = model.BannerImage;

                    // Part Info
                    scheme.PartCategory = model.PartCategory;
                    scheme.PartType = model.PartType;
                    scheme.FocusPartBenifitType = model.FocusPartBenifitType;
                    scheme.FocusPartBenifitTypeValue = model.FocusPartBenifitTypeValue;
                    scheme.FocusPartTarget = model.FocusPartTarget ?? 0;
                    scheme.FocusPartBenifitTypeNumber = model.FocusPartBenifitTypeNumber;
                    scheme.StartRange = model.StartRange;
                    scheme.EndRange = model.EndRange;
                    scheme.FValue = model.FValue;
                    scheme.MValue = model.MValue;
                    scheme.SValue = model.SValue;
                    scheme.PartCreations = model.PartCreations;

                    // Customer Segment
                    scheme.BranchCode = model.BranchCode;
                    scheme.SalesExecutiveId = model.SalesExecutiveId;
                    scheme.PartyType = model.PartyType;

                    // Target Info
                    scheme.PrevYearFromDate = model.PrevYearFromDate;
                    scheme.PrevYearToDate = model.PrevYearToDate;
                    scheme.PrevMonthFromDate = model.PrevMonthFromDate;
                    scheme.PrevMonthToDate = model.PrevMonthToDate;
                    scheme.GrowthCompPercentMinValue = model.GrowthCompPercentMinValue;
                    scheme.GrowthCompPercentBaseValue = model.GrowthCompPercentBaseValue;

                    // Reward Info
                    scheme.CashbackCriteria = model.CashbackCriteria;
                    scheme.AreBothCbAgApplicable = model.AreBothCbAgApplicable;
                    scheme.AreBothAgLdApplicable = model.AreBothAgLdApplicable;
                    scheme.CanTakeMoreThanOneGift = model.CanTakeMoreThanOneGift;
                    scheme.MaxGiftsAllowed = model.MaxGiftsAllowed;

                    // Others (Probably not being used)
                    scheme.FocusPartApplicable = model.IsFocusPartApplicable;
                    scheme.TargetCriteria = model.TargetCriteria;
                    scheme.SchemeFor = model.SchemeFor;
                    scheme.TargetWorkshopCriteria = model.TargetWorkshopCriteria;

                    db.SaveChanges();
                    return scheme.SchemeId;
                }
            }

            scheme = new Scheme
            {
                UserId = model.UserId,
                IsActive = false,
                CreatedDate = DateTime.Now,

                // Basic information
                SchemeName = model.SchemeName,
                Type = model.Types, // Type of the scheme (Cashback,Fixed price)
                SubSchemeType = model.SubSchemeType,
                AssuredGift = model.IsAssuredGift,
                CashBack = model.IsCashBack,
                LuckyDraw = model.IsLuckyDraw,
                DistributorId = model.DistributorId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                DispersalFrequency = model.DispersalFrequency,
                SchemesType = model.SchemesType,
                ThumbnailImage = model.ThumbnailImage,
                BannerImage = model.BannerImage,

                // Part Info
                PartCategory = model.PartCategory,
                PartType = model.PartType,
                FocusPartBenifitType = model.FocusPartBenifitType,
                FocusPartBenifitTypeValue = model.FocusPartBenifitTypeValue,
                FocusPartTarget = model.FocusPartTarget ?? 0,
                FocusPartBenifitTypeNumber = model.FocusPartBenifitTypeNumber,
                StartRange = model.StartRange,
                EndRange = model.EndRange,
                FValue = model.FValue,
                MValue = model.MValue,
                SValue = model.SValue,
                PartCreations = model.PartCreations,

                // Customer Segment
                BranchCode = model.BranchCode,
                SalesExecutiveId = model.SalesExecutiveId,
                PartyType = model.PartyType,

                // Target Info
                PrevYearFromDate = model.PrevYearFromDate,
                PrevYearToDate = model.PrevYearToDate,
                PrevMonthFromDate = model.PrevMonthFromDate,
                PrevMonthToDate = model.PrevMonthToDate,
                GrowthCompPercentMinValue = model.GrowthCompPercentMinValue,
                GrowthCompPercentBaseValue = model.GrowthCompPercentBaseValue,

                // Reward Info
                CashbackCriteria = model.CashbackCriteria,
                AreBothCbAgApplicable = model.AreBothCbAgApplicable,
                AreBothAgLdApplicable = model.AreBothAgLdApplicable,
                CanTakeMoreThanOneGift = model.CanTakeMoreThanOneGift,
                MaxGiftsAllowed = model.MaxGiftsAllowed,

                // Others (Probably not being used)
                FocusPartApplicable = model.IsFocusPartApplicable,
                SchemeFor = model.SchemeFor,
                TargetCriteria = model.TargetCriteria,
                TargetWorkshopCriteria = model.TargetWorkshopCriteria
            };

            db.Schemes.Add(scheme);
            if (db.SaveChanges() > 0)
            {
                return scheme.SchemeId;
            }
            return null;
        }

        #endregion

        #endregion

        #region Save Locations
        public bool SaveLocationsScheme(string Locations, int SchemeId)
        {
            //Remove Locations First
            var old = db.SchemeLocations.Where(a => a.SchemeId == SchemeId).ToList();
            if (old.Count > 0)
            {
                db.SchemeLocations.RemoveRange(old);
                db.SaveChanges();
            }

            if (!string.IsNullOrEmpty(Locations))
            {
                var arrLocations = Locations.Split(',');
                foreach (var item in arrLocations)
                {
                    var location = new SchemeLocation
                    {
                        SchemeId = SchemeId,
                        LocationId = Convert.ToInt32(item)
                    };
                    db.SchemeLocations.Add(location);
                    db.SaveChanges();
                }
            }
            return true;
        }
        #endregion

        #region Category
        public List<ResponseSchemeCategoryModel> GetCategoryScheme(int schemeId)
        {
            return db.CategorySchemes.Where(x => x.SchemeId == schemeId).AsNoTracking().Select(x => new ResponseSchemeCategoryModel
            {
                CategoryId = x.CategoryId,
                SchemeId = x.SchemeId ?? 0,
                Category = x.Category,
                MinAmount = x.MinAmount,
                MaxAmount = x.MaxAmount
            }).ToList();
        }

        public bool SaveCategoryScheme(List<CategorySchemeModel> categories)
        {
            if (categories.Count <= 0) return false;

            foreach (var item in categories.Where(item => !string.IsNullOrEmpty(item.Category)))
            {
                if (item.CategoryId > 0)
                {
                    var oldCategoryScheme = db.CategorySchemes.FirstOrDefault(x => x.CategoryId == item.CategoryId);
                    if (oldCategoryScheme == null) continue;

                    oldCategoryScheme.Category = item.Category;
                    oldCategoryScheme.MinAmount = item.MinAmount;
                    oldCategoryScheme.MaxAmount = item.MaxAmount;
                    db.SaveChanges();
                }
                else
                {
                    var categoryScheme = new CategoryScheme
                    {
                        SchemeId = item.SchemeId,
                        Category = item.Category,
                        MaxAmount = item.MaxAmount,
                        MinAmount = item.MinAmount,
                        CreatedDate = DateTime.Now
                    };
                    db.CategorySchemes.Add(categoryScheme);
                    db.SaveChanges();
                }
            }

            return true;
        }

        /// <summary>
        /// Delete all categories of particular scheme.
        /// </summary>
        /// <param name="schemeId">The scheme Id.</param>
        /// <returns>True if deleted else false.</returns>
        public bool DeleteCategorySchemes(int schemeId)
        {
            db.Database.ExecuteSqlCommand("DELETE FROM CategorySchemes WHERE SchemeId={0}", schemeId);
            return true;
        }

        #endregion

        #region Target Overview

        /// <summary>
        /// Get overview for each target growth, for specific scheme, depicting number of workshops whose target fall in current target growth range along with sum of target of all workshops that fall in range, average target, maximum target and minimum target.
        /// </summary>
        /// <param name="schemeId">The scheme Id of the current scheme.</param>
        public async Task<List<TargetOverview>> GetTargetOverviewAsync(int schemeId)
        {
            var targetOverviews = new List<TargetOverview>();

            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var targetGrowths = await context.TargetGrowths.Where(t => t.SchemeId == schemeId).AsNoTracking().ToListAsync().ConfigureAwait(false);
                if (targetGrowths.Count <= 0) return targetOverviews;

                var targetWorkShops = await context.TargetWorkShops.Where(tw => tw.SchemeId == schemeId).AsNoTracking().ToListAsync().ConfigureAwait(false);
                if (targetWorkShops.Count > 0)
                {
                    foreach (var targetGrowth in targetGrowths)
                    {
                        var matchedWs = targetWorkShops.Where(w => Convert.ToDecimal(w.NewTarget) >= targetGrowth.Min &&
                                                                   Convert.ToDecimal(w.NewTarget) <= targetGrowth.Max).Select(w => new { Target = Convert.ToDouble(w.NewTarget) }).ToList();

                        double target = 0;
                        double avgTarget = 0;
                        double maxTarget = 0;
                        double minTarget = 0;

                        if (matchedWs.Count > 0)
                        {
                            target = matchedWs.Sum(w => w.Target);
                            avgTarget = Math.Round(target / matchedWs.Count, 2);
                            maxTarget = matchedWs.Max(w => w.Target);
                            minTarget = matchedWs.Min(w => w.Target);
                        }

                        targetOverviews.Add(new TargetOverview
                        {
                            TargetGrowthRange = $"{targetGrowth.Min} - {targetGrowth.Max}",
                            GrowthPercentage = targetGrowth.Growth != null ? $"{targetGrowth.Growth}%" : "NA",
                            TotalWorkshops = matchedWs.Count,
                            TotalTarget = target,
                            AverageTarget = avgTarget,
                            MaximumTarget = maxTarget,
                            MinimumTarget = minTarget
                        });
                    }
                }
            }

            return targetOverviews;
        }

        #endregion

        #region Gift Management

        #region List
        public List<ResponseGiftManagementModel> GetGiftManagement(int schemeId)
        {
            var general = new General();
            var gifts = new List<ResponseGiftManagementModel>();

            foreach (var gm in db.GiftManagements.Where(x => x.SchemeId == schemeId).AsNoTracking())
            {
                gifts.Add(new ResponseGiftManagementModel
                {
                    GiftId = gm.GiftId,
                    SchemeId = gm.SchemeId ?? 0,
                    Gift = gm.Gift,
                    Qty = gm.Qty,
                    Categories = "",
                    DrawOrder = gm.DrawOrder,
                    ImagePath = general.CheckImageUrl(gm.ImagePath),
                    ListGiftCategory = gm.GiftCategories.Select(b => new ResponseGiftCategoryModel
                    {
                        IsAll = b.IsAll ?? false,
                        CategoryId = b.CategoryId,
                        GiftCategoryId = b.GiftCategoryId,
                        GiftId = b.GiftId ?? 0
                    }).ToList()
                });
            }

            return gifts;
        }
        #endregion

        #region Save/Update 
        public bool SaveGiftManagement(List<GiftManagementModel> giftManagementModels)
        {
            if (giftManagementModels.Count <= 0) return false;

            foreach (var item in giftManagementModels.Where(item => !string.IsNullOrEmpty(item.Gift)))
            {
                GiftManagement giftManagement;
                if (item.GiftId > 0)
                {
                    giftManagement = db.GiftManagements.FirstOrDefault(a => a.GiftId == item.GiftId);
                    if (giftManagement != null)
                    {
                        giftManagement.Gift = item.Gift;
                        giftManagement.Qty = item.Qty;
                        giftManagement.DrawOrder = item.DrawOrder;
                        giftManagement.ImagePath = item.ImagePath;
                        db.SaveChanges();
                    }
                }
                else
                {
                    giftManagement = new GiftManagement
                    {
                        SchemeId = item.SchemeId,
                        Gift = item.Gift,
                        Qty = item.Qty,
                        DrawOrder = item.DrawOrder,
                        ImagePath = item.ImagePath
                    };
                    db.GiftManagements.Add(giftManagement);
                    db.SaveChanges();
                }

                if (giftManagement != null)
                    SaveGiftCategory(item.Categories, giftManagement.GiftId, item.SchemeId);
            }

            return true;

        }
        #endregion

        #region SaveGiftCategory
        public bool SaveGiftCategory(string Categories, int GiftId, int schemeId)
        {
            try
            {
                var removeGiftCategory = db.GiftCategories.Where(x => x.GiftId == GiftId);
                if (removeGiftCategory.Count() > 0)
                {
                    db.GiftCategories.RemoveRange(removeGiftCategory);
                    db.SaveChanges();
                }

                if (string.Equals(Categories, "all", StringComparison.OrdinalIgnoreCase))
                {
                    var giftCategory = new GiftCategory
                    {
                        GiftId = GiftId,
                        IsAll = true
                    };
                    db.GiftCategories.Add(giftCategory);
                    return db.SaveChanges() > 0;
                }
                if (!string.IsNullOrEmpty(Categories))
                {
                    string[] catArray = Categories.Split(',');
                    var lstCategoryScheme = db.CategorySchemes.Where(x => x.SchemeId == schemeId);

                    List<GiftCategory> lstGiftCategory = new List<GiftCategory>();
                    foreach (var item in catArray)
                    {
                        lstGiftCategory.Add(new GiftCategory
                        {
                            GiftId = GiftId,
                            CategoryId = lstCategoryScheme.Where(a => a.Category == item).FirstOrDefault().CategoryId
                        });
                    }
                    db.GiftCategories.AddRange(lstGiftCategory);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return false;
            }
        }
        #endregion
        #endregion

        #region Assured Gift

        #region Save/Update
        public bool SaveAssuredGift(List<AssuredGiftModel> assuredGiftModels)
        {
            if (assuredGiftModels.Count <= 0) return false;

            foreach (var agm in assuredGiftModels.Where(item => !string.IsNullOrEmpty(item.Target)))
            {
                AssuredGift assuredGift;
                if (agm.AssuredGiftId > 0)
                {
                    assuredGift = db.AssuredGifts.FirstOrDefault(x => x.AssuredGiftId == agm.AssuredGiftId);
                    if (assuredGift != null)
                    {
                        assuredGift.Target = agm.Target;
                        assuredGift.Point = agm.Point;
                        assuredGift.Reward = agm.Reward;
                        db.SaveChanges();
                    }
                }
                else
                {
                    assuredGift = new AssuredGift
                    {
                        SchemeId = agm.SchemeId,
                        Target = agm.Target,
                        Point = agm.Point,
                        Reward = agm.Reward
                    };
                    db.AssuredGifts.Add(assuredGift);
                    db.SaveChanges();
                }
                if (assuredGift != null)
                {
                    SaveAssuredGiftCategory(agm.Categories, assuredGift.AssuredGiftId, agm.SchemeId);
                }
            }

            var schemeId = assuredGiftModels.FirstOrDefault()?.SchemeId;
            var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == schemeId);
            if (scheme != null)
            {
                scheme.AssuredGift = true;
                db.SaveChanges();
            }

            return true;
        }

        private void SaveAssuredGiftCategory(string categories, int assuredGiftId, int schemeId)
        {
            var removeAssuredGiftCategory = db.AssuredGiftCategories.Where(x => x.AssuredGiftId == assuredGiftId);
            if (removeAssuredGiftCategory.Any())
            {
                db.AssuredGiftCategories.RemoveRange(removeAssuredGiftCategory);
                db.SaveChanges();
            }

            if (categories.ToLower() == "all")
            {
                var assuredGiftCategory = new AssuredGiftCategory
                {
                    AssuredGiftId = assuredGiftId,
                    IsAll = true
                };
                db.AssuredGiftCategories.Add(assuredGiftCategory);
                db.SaveChanges();
                return;
            }

            if (!string.IsNullOrEmpty(categories))
            {
                var catArray = categories.Split(',');
                var lstCategoryScheme = db.CategorySchemes.Where(x => x.SchemeId == schemeId);

                var anyCategoryAdded = false;
                foreach (var item in catArray)
                {
                    db.AssuredGiftCategories.Add(new AssuredGiftCategory
                    {
                        AssuredGiftId = assuredGiftId,
                        CategoryId = lstCategoryScheme.FirstOrDefault(a => a.Category == item)?.CategoryId
                    });
                    anyCategoryAdded = true;
                }
                if (anyCategoryAdded)
                {
                    db.SaveChanges();
                }
            }
        }
        #endregion

        #region List
        public List<ResponseAssuredGiftModel> GetAssuredGift(int schemeId)
        {
            return db.AssuredGifts.Where(x => x.SchemeId == schemeId).AsNoTracking().Select(a => new ResponseAssuredGiftModel
            {
                AssuredGiftId = a.AssuredGiftId,
                SchemeId = a.SchemeId ?? 0,
                Target = a.Target,
                Point = a.Point,
                Reward = a.Reward,
                Categories = "",
                ListAssuredGiftCategory = a.AssuredGiftCategories.Select(b => new ResponseAssuredGiftCategoryModel
                {
                    CategoryId = b.CategoryId,
                    AssuredGiftCategoryId = b.AssuredGiftCategoryId,
                    AssuredGiftId = b.AssuredGiftId ?? 0,
                    IsAll = b.IsAll ?? false
                }).ToList()
            }).ToList();
        }
        #endregion
        #endregion

        #region CashBack

        #region Save/Update
        public bool SaveCashBack(List<CashBackModel> cashbackModels, int schemeId)
        {
            var existingCashBacks = db.CashBacks.Where(a => a.SchemeId == schemeId);

            if (cashbackModels.Count > 0)
            {
                // Remove existing cashback
                if (existingCashBacks.Any())
                {
                    var mixCbrToRemove = db.CashbackRangeMixes.Where(a => a.SchemeId == schemeId);
                    db.CashbackRangeMixes.RemoveRange(mixCbrToRemove);
                    db.SaveChanges();

                    db.CashBacks.RemoveRange(existingCashBacks);
                    db.SaveChanges();
                }

                foreach (var cbm in cashbackModels)
                {
                    if (!cbm.FromAmount.HasValue || cbm.FromAmount <= 0) continue;

                    // Add cashback
                    var cashBack = new CashBack
                    {
                        SchemeId = cbm.SchemeId,
                        FromAmount = cbm.FromAmount,
                        ToAmount = cbm.ToAmount,
                        Benifit = cbm.Benifit
                    };
                    db.CashBacks.Add(cashBack);
                    db.SaveChanges();

                    if (cbm.lstCashbackRange.Count > 0)
                    {
                        foreach (var cashBackRange in cbm.lstCashbackRange)
                        {
                            int? cashbackRangeId = cashBackRange.CashbackRangeId;
                            if (cashbackRangeId == 0) { cashbackRangeId = null; }
                            db.CashbackRangeMixes.Add(new CashbackRangeMix
                            {
                                SchemeId = schemeId,
                                CashbackId = cashBack.CashbackId,
                                CashbackRangeId = cashbackRangeId,
                                Percentage = cashBackRange.Percentage
                            });
                        }
                        db.SaveChanges();
                    }
                }

                var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == schemeId);
                if (scheme != null)
                {
                    scheme.CashBack = true;
                    db.SaveChanges();
                }

                return true;
            }

            // If user provided no cashback, then delete all cashback for scheme
            if (existingCashBacks.Any())
            {
                var existingMixCbrs = db.CashbackRangeMixes.Where(a => a.SchemeId == schemeId);
                db.CashbackRangeMixes.RemoveRange(existingMixCbrs);
                db.CashBacks.RemoveRange(existingCashBacks);
                db.SaveChanges();
            }
            return true;
        }

        public bool SaveCashBackRange(List<CashBackRangeModel> cbrModels, int schemeId)
        {
            if (cbrModels.Count > 0)
            {
                cbrModels = cbrModels.Where(a => a.StartRange != null && a.StartRange >= 0).ToList();
                var cashbackRange = cbrModels.FirstOrDefault();
                if (cashbackRange != null)
                {
                    schemeId = cashbackRange.SchemeId;
                }

                var existingCashbackRanges = db.CashbackRanges.Where(a => a.SchemeId == schemeId).ToList();
                if (existingCashbackRanges.Count > 0)
                {
                    var cbrToRemove = new List<CashbackRange>();
                    var mixCbrToRemove = new List<CashbackRangeMix>();

                    var existingMixCashbackRanges = db.CashbackRangeMixes.Where(a => a.SchemeId == schemeId);
                    foreach (var cbr in existingCashbackRanges)
                    {
                        // If matched from passed one
                        var cbrModel = cbrModels.FirstOrDefault(a => a.CashbackRangeId == cbr.CashbackRangeId);
                        if (cbrModel != null)
                        {
                            // Update if found
                            cbr.StartRange = cbrModel.StartRange;
                            cbr.EndRange = cbrModel.EndRange;
                            db.SaveChanges();
                        }
                        else
                        {
                            // Else remove because end user removed it
                            cbrToRemove.Add(cbr);
                            mixCbrToRemove.AddRange(existingMixCashbackRanges.Where(a => a.CashbackRangeId == cbr.CashbackRangeId));
                        }
                    }

                    if (cbrToRemove.Count > 0)
                    {
                        db.CashbackRanges.RemoveRange(cbrToRemove);
                        db.CashbackRangeMixes.RemoveRange(mixCbrToRemove);
                        db.SaveChanges();
                    }

                    // Filter cashback range that not match with existing one
                    var Ids = db.CashbackRanges.Where(a => a.SchemeId == schemeId).Select(a => a.CashbackRangeId).ToList(); //1,2
                    cbrModels = (from a in cbrModels
                                 where !Ids.Contains(a.CashbackRangeId)
                                 select a).ToList();
                }

                if (cbrModels.Count > 0 && cashbackRange != null)
                {
                    foreach (var item in cbrModels)
                    {
                        db.CashbackRanges.Add(new CashbackRange
                        {
                            SchemeId = cashbackRange.SchemeId,
                            StartRange = item.StartRange,
                            EndRange = item.EndRange
                        });
                    }
                    db.SaveChanges();
                }

                return true;
            }

            // If no range was passed, then remove all existing ones for this scheme
            var oldCashbackData = db.CashbackRanges.Where(x => x.SchemeId == schemeId);
            if (oldCashbackData.Any())
            {
                var oldCashbackDataMix = db.CashbackRangeMixes.Where(x => x.SchemeId == schemeId);
                db.CashbackRangeMixes.RemoveRange(oldCashbackDataMix);
                db.CashbackRanges.RemoveRange(oldCashbackData);
                db.SaveChanges();
            }
            return true;
        }
        #endregion

        #region List
        public List<ResponseCashBackModel> GetCashBack(int schemeId)
        {
            return db.CashBacks.Where(x => x.SchemeId == schemeId).AsNoTracking().Select(x => new ResponseCashBackModel
            {
                CashbackId = x.CashbackId,
                SchemeId = x.SchemeId ?? 0,
                FromAmount = x.FromAmount,
                ToAmount = x.ToAmount,
                Benifit = x.Benifit,
                lstCashbackMix = x.CashbackRangeMixes.Select(b => new ResponseCashBackMixModel
                {
                    CashbackRangeId = b.CashbackRangeId,
                    Percentage = b.Percentage
                }).ToList()
            }).ToList();
        }
        #endregion

        #region List Range
        public List<CashbackRange> GetCashBackRange(int schemeId)
        {
            var cashbackRanges = new List<CashbackRange>();
            cashbackRanges = db.CashbackRanges.Where(x => x.SchemeId == schemeId).AsNoTracking().ToList();

            if (cashbackRanges.Count > 0)
            {
                return cashbackRanges;
            }

            // Add default cashback range
            cashbackRanges.Add(new CashbackRange
            {
                CashbackRangeId = 0,
                SchemeId = schemeId,
                StartRange = 0,
                EndRange = 100
            });
            return cashbackRanges;

        }
        #endregion
        #endregion

        #region Qualify Criteria
        #region Save/Update
        public bool SaveQualifyCriteria(List<QualifyCriteriaModel> qcModels)
        {
            if (qcModels.Count <= 0) return false;

            foreach (var item in qcModels.Where(item => !string.IsNullOrEmpty(item.Type)))
            {
                QualifyCriteria qualifyCriteria;
                if (item.QualifyCriteriaId > 0)
                {
                    qualifyCriteria = db.QualifyCriterias.FirstOrDefault(x => x.QualifyCriteriaId == item.QualifyCriteriaId);
                    if (qualifyCriteria != null)
                    {
                        qualifyCriteria.AmountUpto = item.AmountUpto;
                        qualifyCriteria.Type = item.Type;
                        qualifyCriteria.NumberOfCoupons = item.NumberOfCoupons;
                        qualifyCriteria.TypeValue = item.TypeValue;
                        qualifyCriteria.IsAll = item.Type.ToLower() == "all";
                        qualifyCriteria.AdditionalCouponAmount = item.AdditionalCouponAmount;
                        qualifyCriteria.AdditionalNumberOfCoupons = item.AdditionalNumberOfCoupons;

                        db.SaveChanges();
                    }
                }
                else
                {
                    qualifyCriteria = new QualifyCriteria
                    {
                        SchemeId = item.SchemeId,
                        AmountUpto = item.AmountUpto,
                        Type = item.Type,
                        NumberOfCoupons = item.NumberOfCoupons,
                        TypeValue = item.TypeValue,
                        IsAll = item.Type.ToLower() == "all",
                        AdditionalCouponAmount = item.AdditionalCouponAmount,
                        AdditionalNumberOfCoupons = item.AdditionalNumberOfCoupons
                    };
                    db.QualifyCriterias.Add(qualifyCriteria);
                    db.SaveChanges();
                }
            }
            return true;
        }
        #endregion

        #region List
        public List<ResponseQualifyCriteriaModel> GetQualifyCriteria(int schemeId)
        {
            return db.QualifyCriterias.Where(x => x.SchemeId == schemeId).AsNoTracking().Select(a => new ResponseQualifyCriteriaModel
            {
                QualifyCriteriaId = a.QualifyCriteriaId,
                SchemeId = a.SchemeId ?? 0,
                AmountUpto = a.AmountUpto,
                Type = a.Type,
                NumberOfCoupons = a.NumberOfCoupons,
                TypeValue = a.TypeValue,
                IsAll = a.IsAll ?? false,
                AdditionalCouponAmount = a.AdditionalCouponAmount,
                AdditionalNumberOfCoupons = a.AdditionalNumberOfCoupons
            }).ToList();
        }
        #endregion

        #endregion

        #region Target WorkShop
        #region Save/Update
        public bool SaveTargetWorkshop(List<TargetWorkshopModel> targetWorkshops, int schemeId)
        {
            var isSaved = false;
            // Delete existing ones
            db.Database.ExecuteSqlCommand("DELETE FROM TargetWorkShop WHERE SchemeId={0}", schemeId);

            if (targetWorkshops.Count > 0)
            {
                foreach (var tw in targetWorkshops)
                {
                    db.TargetWorkShops.Add(new TargetWorkShop
                    {
                        SchemeId = tw.SchemeId,
                        WorkShopId = tw.WorkShopId,
                        CustomerType = tw.CustomerType,
                        PrevYearAvgSale = tw.PrevYearAvgSale,
                        GrowthPercentage = tw.GrowthPercentage,
                        NewTarget = tw.NewTarget,
                        PrevMonthAvgSale = tw.PrevMonthAvgSale,
                        GrowthComparisonPercentage = tw.GrowthComparisonPercentage,
                        IsQualifiedAsDefault = tw.IsQualifiedAsDefault
                    });
                }
                isSaved = db.SaveChanges() > 0;

                // Push Notifications
                //RepoAppNotification appNotify = new RepoAppNotification();
                //var pushModel = new PushNotificationUserModel()
                //{
                //    SchemeId= schemeId
                //};
                //var appThread = new System.Threading.Thread(() => appNotify.SendPushNotification(pushModel)) { IsBackground = true };
                //appThread.Start();

            }

            return isSaved;
        }
        #endregion

        #region List
        public List<ResponseTargetWorkshopModel> GetTargetWorkshop(int schemeId)
        {
            return db.TargetWorkShops.Where(x => x.SchemeId == schemeId).Select(a => new ResponseTargetWorkshopModel
            {
                TargetWorkShopId = a.TargetWorkShopId,
                WorkShopId = a.WorkShopId ?? 0,
                SchemeId = a.SchemeId ?? 0,
                NewTarget = a.NewTarget,
                IsQualifiedAsDefault = a.IsQualifiedAsDefault ?? false
            }).ToList();
        }
        #endregion

        #region Target workshop list
        public List<ResponseTargetWorkshopModel> GetTargetWorkshops(SchemeModel schemeModel)
        {
            var targetWorkshops = new List<ResponseTargetWorkshopModel>();
            Scheme scheme = db.Schemes.AsNoTracking().FirstOrDefault(a => a.SchemeId == schemeModel.SchemeId);
            if (scheme == null) return targetWorkshops;

            targetWorkshops = GetWorkshopsForScheme(scheme);

            var prevYearStartDate = schemeModel.PrevYearFromDate;
            var prevYearEndDate = schemeModel.PrevYearToDate;
            var prevMonthStartDate = schemeModel.PrevMonthFromDate;
            var prevMonthEndDate = schemeModel.PrevMonthToDate;

            if (prevYearStartDate == null || prevYearEndDate == null || prevMonthStartDate == null || prevMonthEndDate == null)
            {
                return targetWorkshops;
            }

            // Get month difference for use in calculating average of sales
            var monthDiffForPrevYears = Utils.GetMonthDifference(prevYearStartDate.Value, prevYearEndDate.Value);
            var monthDiffForPrevMonths = Utils.GetMonthDifference(prevMonthStartDate.Value, prevMonthEndDate.Value);
            if (monthDiffForPrevYears == 0) monthDiffForPrevYears = 1;
            if (monthDiffForPrevMonths == 0) monthDiffForPrevMonths = 1;

            var twsIds = targetWorkshops.Select(t => t.WorkShopId).Distinct().ToList();
            var targetWsSales = GetTargetWorkshopsSales(scheme.SchemeId, twsIds, scheme.PartType, false);
            var targetGrowths = GetTargetGrowthsBySchemeId(scheme.SchemeId);

            foreach (var ws in targetWorkshops)
            {
                // Calculate new target for workshops whose target is empty or less than zero
                if (!string.IsNullOrWhiteSpace(ws.NewTarget) || decimal.TryParse(ws.NewTarget, out var decNewTarget) && decNewTarget > 0) continue;

                var prevYearAvgSale = targetWsSales.Where(x => x.WorkShopId == ws.WorkShopId && x.CreatedDate >= prevYearStartDate && x.CreatedDate <= prevYearEndDate).Sum(s => s.NetRetailSelling) / monthDiffForPrevYears;
                var prevMonthAvgSale = targetWsSales.Where(x => x.WorkShopId == ws.WorkShopId && x.CreatedDate >= prevMonthStartDate && x.CreatedDate <= prevMonthEndDate).Sum(s => s.NetRetailSelling) / monthDiffForPrevMonths;

                var growthRange = targetGrowths.FirstOrDefault(tg => prevYearAvgSale >= tg.Min && prevYearAvgSale <= tg.Max);
                var growthPercentage = growthRange?.Growth ?? 0.0M;
                var newTarget = prevYearAvgSale + prevYearAvgSale * growthPercentage / 100;

                // add by default newTarget if target is less then defaultTarget
                var minTarget = growthRange!=null?growthRange?.MinTarget: targetGrowths!=null? targetGrowths.Select(a=>a.MinTarget.Value).FirstOrDefault():0M;
                if (newTarget< minTarget)
                {
                    newTarget = minTarget.Value;
                }

                ws.PrevYearAvgSale = Math.Round(prevYearAvgSale, 2);
                ws.PrevMonthAvgSale = Math.Round(prevMonthAvgSale, 2);
                ws.GrowthComparisonPercentage = newTarget > 0 && prevMonthAvgSale > 0 ? Math.Round((newTarget - prevMonthAvgSale) * 100 / prevMonthAvgSale, 2) : 0.0M;
                ws.GrowthPercentage = growthPercentage;
                ws.NewTarget = Convert.ToString(Math.Round(newTarget, 2), CultureInfo.InvariantCulture);
            }

            return targetWorkshops;
        }
        /// <summary>
        ///  Get scheme sale when isCurrentSale is true than filter sale by scheme range time
        /// </summary>
        /// <param name="isCurrentSale"></param>
        /// <returns></returns>
        private List<DailySalesData> GetTargetWorkshopsSales(int schemeId, List<int> twsIds, string partType, bool isCurrentSale = false)
        {
            IQueryable<DailySalesTrackerWithInvoiceData> twSales;
            var dailySales = new List<DailySalesData>();

            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var scheme = context.Schemes.AsNoTracking().FirstOrDefault(f => f.SchemeId == schemeId);

                twSales = context.DailySalesTrackerWithInvoiceDatas.Where(a => a.WorkShopId != null && twsIds.Contains(a.WorkShopId.Value) && !string.IsNullOrEmpty(a.NetRetailSelling));

                if (isCurrentSale)
                {
                    twSales = twSales.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(scheme.StartDate) && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(scheme.EndDate));
                }

                if (!string.IsNullOrWhiteSpace(scheme.PartCategory))
                {
                    var partCategories = scheme.PartCategory.Split(',').ToList();
                    twSales = twSales.Where(a => partCategories.Contains(a.PartCategory));
                }

                if (!string.IsNullOrWhiteSpace(partType) && partType == "Focus Parts Group")
                {
                    twSales = FilterSalesByFocusPartGroup(schemeId, twSales);
                }

                // filter sale by BranchCode
                if (scheme.BranchCode != null)
                {
                    var branchCodes = scheme.BranchCode.Split(',').Select(int.Parse);

                    var outletcodes = context.Outlets.Where(o => branchCodes.Contains(o.OutletId)).AsNoTracking().Select(o => o.OutletCode).ToList();

                    twSales = twSales.Where(a => outletcodes.Contains(a.LocCode));
                }

                // filter sale by Sales person
                if (scheme.SalesExecutiveId != null)
                {
                    var salesExecutiveIds = scheme.SalesExecutiveId.Split(',');

                    var wsIds = context.SalesExecutiveWorkshops.Where(s => salesExecutiveIds.Contains(s.UserId)).AsNoTracking().Select(o => o.WorkshopId).ToList();

                    twSales = twSales.Where(a => wsIds.Contains(a.WorkShopId ?? 0));
                }

                // filter sale by Customer Type
                if (scheme.PartyType != null)
                {
                    var partyTypes = scheme.PartyType.Split(',');
                    twSales = twSales.Where(a => partyTypes.Contains(a.ConsPartyTypeDesc));
                }

                if (!string.IsNullOrWhiteSpace(partType) && partType == "FMS Parts Group")
                {
                    var sales = FilterSalesByFmsPartGroup(scheme, twSales);
                    dailySales = sales.Select(s => new DailySalesData
                    {
                        CreatedDate = s.CreatedDate,
                        NetRetailSelling = Convert.ToDecimal(s.NetRetailSelling),
                        WorkShopId = s.WorkShopId
                    }).ToList();
                }
                else
                {
                    var sales = twSales.Select(s => new
                    {
                        s.CreatedDate,
                        s.NetRetailSelling,
                        s.WorkShopId
                    }).AsNoTracking().ToList();

                    dailySales = sales.Select(s => new DailySalesData
                    {
                        CreatedDate = s.CreatedDate,
                        NetRetailSelling = Convert.ToDecimal(s.NetRetailSelling),
                        WorkShopId = s.WorkShopId
                    }).ToList();
                }
                return dailySales;
            }
        }

        private IQueryable<DailySalesTrackerWithInvoiceData> FilterSalesByFocusPartGroup(int schemeId, IQueryable<DailySalesTrackerWithInvoiceData> twSales)
        {
            var groupIds = new List<int?>();
            var productIds = new List<int?>();
            List<FocusPart> focusParts;
            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                focusParts = db.FocusParts.Where(f => f.SchemeId == schemeId).AsNoTracking().ToList();
            }

            foreach (var focusPart in focusParts)
            {
                if (focusPart.GroupId.HasValue && focusPart.ProductId.HasValue)
                {
                    productIds.Add(focusPart.ProductId);
                }
                else if (focusPart.GroupId.HasValue)
                {
                    groupIds.Add(focusPart.GroupId);
                }
            }

            return twSales.Where(a => groupIds.Contains(a.GroupId) || productIds.Contains(a.ProductId));
        }

        private List<DailySalesTrackerWithInvoiceData> FilterSalesByFmsPartGroup(Scheme scheme, IQueryable<DailySalesTrackerWithInvoiceData> twSales)
        {
            var filteredSales = new List<DailySalesTrackerWithInvoiceData>();

            // Filter sale by date
            // Arrange sale by descending order of quantity
            var sales = twSales.Where(a => a.CreatedDate != null && a.CreatedDate >= scheme.StartRange && a.CreatedDate <= scheme.EndRange).AsNoTracking().ToList().OrderByDescending(a => Convert.ToDecimal(a.NetRetailQty)).ToList();

            if (scheme.PartCreations == "All")
            {
                return sales;
            }
            else
            {
                // TODO: Calculate total count of all items of all sales
                decimal totalQty = sales.Sum(s => Convert.ToDecimal(s.NetRetailQty));

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

                // TODO: Make it parallel loop
                // Now loop sale record till when sum of quantity of sale not equal to 75
                var saleQtySum = 0.0M;
                foreach (var sale in sales)
                {
                    if (targetQty <= saleQtySum) break;

                    filteredSales.Add(sale);
                    saleQtySum += Convert.ToDecimal(sale.NetRetailQty);
                }
            }

            return filteredSales;
        }

        /// <summary>
        /// Get target workshops from uploaded excel file for scheme.
        /// </summary>
        /// <param name="filePath">The path of the uploaded excel file.</param>
        /// <param name="schemeId">The scheme Id for which target workshops need to be retrieved.</param>
        /// <param name="errorMsg">The error message that hold details of non-matching workshops if found otherwise null.</param>
        public List<ResponseTargetWorkshopModel> GetTargetWorkshopsFromFile(string filePath, int schemeId, out string errorMsg)
        {
            errorMsg = null;
            var fileWorkshops = new List<ResponseTargetWorkshopModel>();

            var refinedTable = General.GetTableFromExcelFile(filePath);
            if (refinedTable.Rows.Count == 0) return fileWorkshops;

            var scheme = db.Schemes.Where(s => s.SchemeId == schemeId).AsNoTracking().FirstOrDefault();

            // get workshops by sheet codes
            var workshopCodes = refinedTable.AsEnumerable().Select(r => r.Field<string>("Code"));
            var workshops = (from u in db.UserDetails.AsNoTracking()
                             join dw in db.DistributorWorkShops.AsNoTracking() on u.UserId equals dw.UserId
                             join w in db.WorkShops.AsNoTracking() on dw.WorkShopId equals w.WorkShopId
                             where (u.IsDeleted == null || u.IsDeleted == false) && workshopCodes.Contains(u.ConsPartyCode) && dw.DistributorId == scheme.DistributorId
                             select w
               ).ToList();
            if (workshops.Count == 0)
            {
                errorMsg = "Any code from file not matched with workshops.";
                return fileWorkshops;
            }
            // Process records
            var ru = new RepoUsers();
            foreach (DataRow row in refinedTable.Rows)
            {
                var code = refinedTable.ColumnValue(row, "Code");
                var prevYearAvgSale = refinedTable.DecimalColumnValue(row, "LastYearTarget");
                var target = refinedTable.DecimalColumnValue(row, "CurrentTarget");
                var prevMonthAvgSale = refinedTable.DecimalColumnValue(row, "LastMonthTarget");
                var isQualified = refinedTable.BoolColumnValue(row, "isQualified");

                var workshop = ru.GetWorkshopByCode(code);
                if (workshop == null) continue;

                else if (workshops.FirstOrDefault(w => w.WorkShopId == workshop.WorkShopId) == null)
                {
                    errorMsg += $"Customer not available with code\t• {code} in this distributor\n";
                    continue;
                }

                var rstWm = new ResponseTargetWorkshopModel
                {
                    SchemeId = schemeId,
                    WorkShopId = workshop.WorkShopId,
                    WorkShopCode = code,
                    WorkShopName = workshop.WorkShopName,
                    CustomerType = workshop.Type,
                    PrevYearAvgSale = prevYearAvgSale,
                    NewTarget = Convert.ToString(target),
                    PrevMonthAvgSale = prevMonthAvgSale,
                    IsQualifiedAsDefault = isQualified ?? false
                };
                fileWorkshops.Add(rstWm);
            }

            // filter sale by Sales person
            if (scheme.SalesExecutiveId != null && fileWorkshops.Count > 0)
            {
                var salesExecutiveIds = scheme.SalesExecutiveId.Split(',');

                var wsIds = db.SalesExecutiveWorkshops.Where(s => salesExecutiveIds.Contains(s.UserId)).AsNoTracking().Select(o => o.WorkshopId).ToList();

                fileWorkshops = fileWorkshops.Where(a => wsIds.Contains(a.WorkShopId)).ToList();
            }

            // filter sale by BranchCode
            else if (scheme.BranchCode != null && fileWorkshops.Count > 0)
            {
                var OutletIds = scheme.BranchCode.Split(',').Select(int.Parse).ToList();

                var wsIds = workshops.Where(s => OutletIds.Contains(s.outletId ?? 0)).Select(o => o.WorkShopId).ToList();

                fileWorkshops = fileWorkshops.Where(a => wsIds.Contains(a.WorkShopId)).ToList();
            }

            // filter sale by Customer Type
            if (scheme.PartyType != null && fileWorkshops.Count > 0)
            {
                var partyTypes = scheme.PartyType.Split(',');

                var wsIds = workshops.Where(s => partyTypes.Contains(s.Type)).Select(o => o.WorkShopId).ToList();

                fileWorkshops = fileWorkshops.Where(a => wsIds.Contains(a.WorkShopId)).ToList();
            }

            if (fileWorkshops.Count <= 0) return fileWorkshops;

            // Set growth percentage and growth comparison percentage for file workshops
            var targetGrowths = db.TargetGrowths.Where(t => t.SchemeId == schemeId);
            foreach (var tws in fileWorkshops)
            {
                var newTarget = Convert.ToDecimal(tws.NewTarget);
                var targetGrowth = targetGrowths.FirstOrDefault(tg => newTarget >= tg.Min && newTarget <= tg.Max);
                tws.GrowthPercentage = targetGrowth != null ? targetGrowth.Growth : 0;

                var prevMoAvgSale = Convert.ToDecimal(tws.PrevMonthAvgSale);
                tws.GrowthComparisonPercentage = newTarget > 0 && prevMoAvgSale > 0 ? Math.Round((newTarget - prevMoAvgSale) * 100 / prevMoAvgSale, 2) : 0.0M;
            }

            return fileWorkshops;


            //// Retrieve saved workshops for scheme
            //var rl = new RepoLabels();
            ////var savedWs = rl.GetSavedTargetWorkshops(schemeId);
            //var savedWs = GetWorkshopsForScheme(scheme);
            //if (savedWs.Count == 0)
            //{
            //    //savedWs = GetWorkshopsForScheme(scheme);
            //    if (savedWs.Count == 0) return new List<ResponseTargetWorkshopModel>();
            //}

            //// Get matching file workshops from saved one
            //var savedWsIds = savedWs.Select(w => w.WorkShopId);
            //var matchedFileWs = fileWorkshops.Where(w => savedWsIds.Contains(w.WorkShopId)).ToList();
            //var matchedWsIds = matchedFileWs.Select(w => w.WorkShopId);

            //// Prepare error message for not matched file workshops
            //var nonMatchedFileWs = fileWorkshops.Where(f => !matchedWsIds.Contains(f.WorkShopId)).ToList();
            //if (nonMatchedFileWs.Count > 0)
            //{
            //    errorMsg = "Following workshops from file not matched as per criteria-\n\n";
            //    foreach (var nmfWs in nonMatchedFileWs)
            //    {
            //        errorMsg += $"\t• {nmfWs.WorkShopName}\n";
            //    }
            //}

            //return matchedFileWs;
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

        #region Sort and Calculate Growth

        public List<ResponseTargetWorkshopModel> SortByGrowth(string workshops, int distributorId, int schemeId, string sortBy)
        {
            var targetWorkshops = new List<ResponseTargetWorkshopModel>();
            var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == schemeId);
            if (scheme == null) return targetWorkshops;

            targetWorkshops = JsonConvert.DeserializeObject<List<ResponseTargetWorkshopModel>>(workshops);
            if (targetWorkshops.Count == 0) return targetWorkshops;

            switch (sortBy)
            {
                case "ASC":
                    return targetWorkshops.OrderBy(a => a.GrowthComparisonPercentage).ToList();
                case "DESC":
                    return targetWorkshops.OrderByDescending(a => a.GrowthComparisonPercentage).ToList();
                default:
                    return targetWorkshops;
            }
        }

        public List<ResponseTargetWorkshopModel> CalculateGrowth(string workshops, int schemeId, decimal growthCompPercentMinValue, decimal growthCompPercentBaseValue)
        {
            var targetWorkshops = new List<ResponseTargetWorkshopModel>();
            var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == schemeId);
            if (scheme == null || string.IsNullOrEmpty(workshops)) return targetWorkshops;

            targetWorkshops = JsonConvert.DeserializeObject<List<ResponseTargetWorkshopModel>>(workshops);
            if (targetWorkshops.Count == 0) return targetWorkshops;

            foreach (var tws in targetWorkshops)
            {
                if (tws.PrevMonthAvgSale > 0 && tws.PrevYearAvgSale > 0 && tws.GrowthComparisonPercentage.HasValue && tws.GrowthComparisonPercentage < growthCompPercentMinValue)
                {
                    // Calculate new target by-
                    // 1. Find % (specified by end user) of prev month average sale
                    // 2. Now add result of step 1 to prev month average sale
                    // 3. Set result of step 2 as new target
                    // 4. Now calculate 'Growth comparison %' again and set it too.

                    var result = tws.PrevMonthAvgSale.Value * growthCompPercentBaseValue / 100;
                    var newTarget = result + tws.PrevMonthAvgSale.Value;
                    tws.NewTarget = Convert.ToString(Math.Round(newTarget, 2), CultureInfo.InvariantCulture);
                    tws.GrowthComparisonPercentage = Math.Round((newTarget - tws.PrevMonthAvgSale.Value) * 100 / tws.PrevMonthAvgSale.Value, 2);
                }
            }

            return targetWorkshops;
        }

        #endregion

        #region Focus Part
        #region Save/Update 
        public bool SaveFocusPart(List<FocusGroupModel> focusGroupParts)
        {
            if (focusGroupParts.Count <= 0) return false;

            // Remove existing focus parts
            var schemeId = focusGroupParts.FirstOrDefault()?.SchemeId;
            var query = $"DELETE FROM FocusPart WHERE SchemeId={schemeId}";
            db.Database.ExecuteSqlCommand(query);

            // Add new selected pairs
            var newFocusParts = new List<FocusPart>();
            foreach (var fgm in focusGroupParts)
            {
                if (fgm?.GroupId <= 0) continue;

                var productIds = new List<int>();
                if (!string.IsNullOrEmpty(fgm.ProductIds))
                {
                    productIds = fgm.ProductIds.Split(',').Select(int.Parse).ToList();
                }

                if (productIds.Count > 0)
                {
                    newFocusParts.AddRange(productIds.Select(p => new FocusPart
                    {
                        SchemeId = fgm.SchemeId,
                        GroupId = fgm.GroupId,
                        ProductId = p
                    }));
                }
                else
                {
                    newFocusParts.Add(new FocusPart()
                    {
                        SchemeId = fgm.SchemeId,
                        GroupId = fgm.GroupId
                    });
                }
            }

            db.FocusParts.AddRange(newFocusParts);
            db.SaveChanges();

            var scheme = db.Schemes.SingleOrDefault(x => x.SchemeId == schemeId);
            if (scheme != null)
            {
                scheme.FocusPartApplicable = true;
                db.SaveChanges();
            }
            return true;
        }

        public bool SaveFocusPart(List<FocusPartModel> listFocusParts)
        {
            if (listFocusParts.Count <= 0) return false;

            var focusParts = db.FocusParts.ToList();
            foreach (var fpm in listFocusParts)
            {
                if (fpm.GroupId == null || fpm.GroupId <= 0) continue;

                FocusPart focusPart;
                if (fpm.FocusPartId > 0)
                {
                    focusPart = focusParts.FirstOrDefault(x => x.FocusPartId == fpm.FocusPartId);
                    if (focusPart != null)
                    {
                        focusPart.GroupId = fpm.GroupId;
                        focusPart.ProductId = fpm.ProductId;
                        focusPart.Qty = fpm.Qty;
                        focusPart.Type = fpm.Type;
                        focusPart.Value = fpm.Value;
                        focusPart.Price = fpm.Price;
                        focusPart.Description = fpm.Description;
                    }
                    db.SaveChanges();
                }
                else
                {
                    focusPart = new FocusPart
                    {
                        SchemeId = fpm.SchemeId,
                        GroupId = fpm.GroupId,
                        ProductId = fpm.ProductId,
                        Qty = fpm.Qty,
                        Type = fpm.Type,
                        Value = fpm.Value,
                        Price = fpm.Price,
                        Description = fpm.Description
                    };
                    db.FocusParts.Add(focusPart);
                    db.SaveChanges();
                }
            }

            var schemeId = listFocusParts.FirstOrDefault()?.SchemeId;
            var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == schemeId);
            if (scheme != null)
            {
                scheme.FocusPartApplicable = true;
                db.SaveChanges();
            }
            return true;

        }

        /// <summary>
        /// Delete focus part for particular scheme.
        /// </summary>
        /// <param name="schemeId">The scheme id for which focus parts will be retrieved.</param>
        public void DeleteFocusParts(int schemeId)
        {
            var focusParts = db.FocusParts.Where(f => f.SchemeId == schemeId);
            if (focusParts.Any())
            {
                db.FocusParts.RemoveRange(focusParts);
                db.SaveChanges();
            }
        }
        #endregion

        #region List

        public List<ResponseFocusPartModel> GetFocusPart(int schemeId)
        {
            return db.FocusParts.Where(x => x.SchemeId == schemeId).Select(a => new ResponseFocusPartModel
            {
                FocusPartId = a.FocusPartId,
                SchemeId = a.SchemeId ?? 0,
                GroupId = a.GroupId,
                ProductId = a.ProductId,
                Qty = a.Qty,
                Type = a.Type,
                Value = a.Value,
                Price = a.Price,
                Description = a.Description

            }).ToList();
        }
        #endregion

        #region List schemes focusparts-group

        public List<FocusGroupModel> GetSchemeFocusPart(int schemeId)
        {
            var response = new List<FocusGroupModel>();
            var data = (from f in db.FocusParts
                        join g in db.ProductGroups on f.GroupId equals g.GroupId
                        where f.SchemeId == schemeId
                        select g).Distinct().Select(pg => new FocusGroupModel
                        {
                            GroupId = pg.GroupId,
                            GroupName = pg.GroupName
                        }).ToList();
            var focusParts = db.FocusParts.Where(x => x.SchemeId == schemeId).AsNoTracking().ToList();
            foreach (var g in data)
            {
                var product = focusParts.Where(p => p.GroupId == g.GroupId && p.ProductId != null).ToList();
                string[] strProductIds = product.Select(p => p.ProductId.ToString()).ToArray();

                response.Add(new FocusGroupModel
                {
                    SchemeId = schemeId,
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    ProductText = product != null ? product.Count > 0 ? product.Count() + " parts selected" : "All parts selected" : "All parts selected",
                    ProductIds = string.Join(",", strProductIds)
                });
            }
            return response;
        }
        #endregion

        #region List schemes parts based on group

        public List<FocusPartsModel> GetSchemeFocusPart(int groupId, int distributorId)
        {
            return db.Products.Where(p => p.GroupId == groupId && p.DistributorId == distributorId).AsNoTracking().Select(p => new FocusPartsModel
            {
                ProductId = p.ProductId,
                ProductName = string.IsNullOrEmpty(p.Description) ? string.IsNullOrEmpty(p.ProductName) ? p.PartNo : p.ProductName : p.Description,
                PartNumber = p.PartNo
            }).ToList();
        }
        #endregion

        public ResponseSchemeModel GetFmsPartData(int schemeId)
        {
            var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == schemeId);
            if (scheme == null) return null;

            var schemeModel = new ResponseSchemeModel
            {
                SchemeId = scheme.SchemeId,
                StartRange = scheme.StartRange != null ? scheme.StartRange.Value.ToString("MM/dd/yyyy") : string.Empty,
                EndRange = scheme.EndRange != null ? scheme.EndRange.Value.ToString("MM/dd/yyyy") : string.Empty,
                FValue = scheme.FValue ?? 0,
                MValue = scheme.MValue ?? 0,
                SValue = scheme.SValue ?? 0,
                PartCreations = scheme.PartCreations
            };

            return schemeModel;
        }

        #endregion

        #region Ticket Of Joy
        #region Save/Update
        public bool SaveTicketOfJoy(List<TicketOfJoyModel> lstModel)
        {
            if (lstModel.Count > 0)
            {
                try
                {
                    foreach (var item in lstModel)
                    {
                        if (!string.IsNullOrEmpty(item.Type))
                        {
                            TicketOfJoy objTicketOfJoy = null;
                            if (item.TicketId > 0)
                            {
                                objTicketOfJoy = db.TicketOfJoys.Where(x => x.TicketId == item.TicketId).FirstOrDefault();
                                if (objTicketOfJoy != null)
                                {
                                    objTicketOfJoy.GroupId = item.GroupId;
                                    objTicketOfJoy.ProductId = item.ProductId;
                                    objTicketOfJoy.Type = item.Type;
                                    objTicketOfJoy.Value = item.Value;
                                    objTicketOfJoy.CouponCode = item.CouponCode;
                                }
                                db.SaveChanges();
                            }
                            else
                            {
                                objTicketOfJoy = new TicketOfJoy
                                {
                                    SchemeId = item.SchemeId,
                                    GroupId = item.GroupId,
                                    ProductId = item.ProductId,
                                    Type = item.Type,
                                    Value = item.Value,
                                    CouponCode = item.CouponCode
                                };
                                db.TicketOfJoys.Add(objTicketOfJoy);
                                db.SaveChanges();
                            }
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region List
        public List<ResponseTicketOfJoyModel> GetTicketOfJoy(int schemeId)
        {
            return db.TicketOfJoys.Where(x => x.SchemeId == schemeId).Select(a => new ResponseTicketOfJoyModel
            {
                TicketId = a.TicketId,
                SchemeId = a.SchemeId ?? 0,
                GroupId = a.GroupId,
                ProductId = a.ProductId,
                Type = a.Type,
                Value = a.Value,
                CouponCode = a.CouponCode
            }).ToList();
        }
        #endregion
        #endregion

        #region Active/InActive
        public bool SchemeActive(SchemeActiveModel model)
        {
            try
            {
                if (model == null) return false;

                var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == model.SchemeId);
                if (scheme == null) return false;

                scheme.IsActive = model.IsActive;
                db.SaveChanges();

                // Push Notifications
                if (model.IsActive)
                {
                    RepoAppNotification appNotify = new RepoAppNotification();
                    var pushModel = new PushNotificationUserModel()
                    {
                        SchemeId = model.SchemeId
                    };
                    var appThread = new System.Threading.Thread(() => appNotify.SendPushNotification(pushModel)) { IsBackground = true };
                    appThread.Start();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool GetSchemeActive(SchemeActiveModel model)
        {
            var isActive = db.Schemes.AsNoTracking().FirstOrDefault(x => x.SchemeId == model.SchemeId)?.IsActive;
            return isActive ?? false;
        }
        #endregion

        #region AreBothApplicable
        public bool SaveAreBothApplicable(AreBothApplicableModel model)
        {
            var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == model.SchemeId);
            if (scheme == null) return false;

            scheme.AreBothCbAgApplicable = model.AreBothCbAgApplicable;
            db.SaveChanges();
            return true;
        }

        public bool GetAreBothApplicable(AreBothApplicableModel model)
        {
            var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == model.SchemeId);
            if (scheme == null) return false;

            return scheme.AreBothCbAgApplicable ?? false;
        }
        #endregion

        #region can take more than One Gift
        public bool SaveCanTakeMoreThanOneGift(CanTakeMoreThanOneModel model)
        {
            if (model != null)
            {
                var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == model.SchemeId);
                if (scheme != null)
                {
                    scheme.CanTakeMoreThanOneGift = model.CanTakeMoreThanOne;
                    scheme.MaxGiftsAllowed = model.MaxGiftsAllowed;
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool GetCanTakeMoreThanOneGift(CanTakeMoreThanOneModel model)
        {
            var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == model.SchemeId);
            if (scheme == null) return false;

            model.MaxGiftsAllowed = scheme.MaxGiftsAllowed;
            var canTakeMoreThanOneGift = scheme.CanTakeMoreThanOneGift;
            return canTakeMoreThanOneGift ?? false;
        }
        #endregion

        #region Get product groups based on F,M,S value

        /// <summary>
        /// Get product groups based on F, M and S values.
        /// </summary>        
        public List<ProductGroupModel> GetProdGroups(int? distributorId, string partCreation, string partCategory, string fValue, string mValue, string sValue)
        {
            var partCategoryCodes = new List<string>();
            var listGroups = new List<ProductGroupModel>();

            if (!string.IsNullOrEmpty(partCategory))
            {
                if (partCategory.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    listGroups = db.ProductGroups.AsNoTracking().Select(pg => new ProductGroupModel
                    {
                        GroupId = pg.GroupId,
                        GroupName = pg.GroupName
                    }).ToList();

                    return listGroups;
                }

                partCategoryCodes = partCategory.Split(',').ToList();

                if (partCategory.Contains("M"))
                {
                    partCategoryCodes.Add(Constants.MFull);
                }

                if (partCategory.Contains("AA"))
                {
                    partCategoryCodes.Add(Constants.AAFull);
                }
                if (partCategory.Contains("AG"))
                {
                    partCategoryCodes.Add(Constants.AGFull);
                }
                if (partCategory.Contains("T"))
                {
                    partCategoryCodes.Add(Constants.TFull);
                }

                listGroups = (from g in db.ProductGroups.AsNoTracking()
                              join p in db.Products.AsNoTracking() on g.GroupId equals p.GroupId
                              where p.PartCategoryCode != null && partCategoryCodes.Contains(p.PartCategoryCode)
                              select g).Distinct().Select(pg => new ProductGroupModel
                              {
                                  GroupId = pg.GroupId,
                                  GroupName = pg.GroupName
                              }).ToList();
            }

            // Set discount as per part creation value
            decimal discount = 0;
            if (!string.IsNullOrEmpty(partCreation))
            {
                switch (partCreation)
                {
                    case "F":
                        discount = Constants.FValue;
                        if (decimal.TryParse(fValue, out decimal fv))
                        {
                            discount = fv;
                        }
                        break;

                    case "M":
                        discount = Constants.MValue;
                        if (decimal.TryParse(mValue, out decimal mv))
                        {
                            discount = mv;
                        }
                        break;

                    case "S":
                        discount = Constants.SValue;
                        if (decimal.TryParse(sValue, out decimal sv))
                        {
                            discount = sv;
                        }
                        break;
                }
            }

            if (discount > 0)
            {
                listGroups = new List<ProductGroupModel>();
                var groups = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.GroupId != null && x.DistributorId == distributorId).AsNoTracking().ToList()
                    .GroupBy(x => x.GroupId)
                    .Select(x => new { GroupId = x.Key, Total = x.Sum(s => Convert.ToDouble(s.NetRetailSelling)) }).OrderBy(o => o.Total).ToList();

                if (!string.IsNullOrEmpty(partCategory) && !partCategory.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    groups = (from g in groups
                              join p in db.Products.AsNoTracking() on g.GroupId equals p.GroupId
                              where p.PartCategoryCode != null && partCategoryCodes.Contains(p.PartCategoryCode)
                              select g).Distinct().ToList();
                }
                var grandTotal = Convert.ToDecimal(groups.Sum(s => s.Total));

                // Prepare list of products until sum of each price doesn't be equal to target value
                decimal targetValue = discount > 0 ? grandTotal * discount / 100 : grandTotal;
                decimal saleProdTotalPrice = 0;
                var listGroupIds = new List<int>();
                foreach (var group in groups.Where(a => a.GroupId != null))
                {
                    saleProdTotalPrice += Convert.ToDecimal(group.Total);
                    if (saleProdTotalPrice > targetValue)
                    {
                        listGroupIds.Add(group.GroupId ?? 0);
                        break;
                    }

                    listGroupIds.Add(group.GroupId ?? 0);
                }

                // Retrieve groups based on product id     
                var productGroups = db.ProductGroups.Where(p => listGroupIds.Contains(p.GroupId)).AsNoTracking().ToList();
                foreach (var groupId in listGroupIds)
                {
                    var group = productGroups.FirstOrDefault(g => g.GroupId == groupId);
                    if (group == null) continue;

                    if (!listGroups.Exists(g => g.GroupId == group.GroupId))
                    {
                        listGroups.Add(new ProductGroupModel
                        {
                            GroupId = group.GroupId,
                            GroupName = group.GroupName
                        });
                    }
                }
            }

            return listGroups;
        }

        #endregion

        #region Get product groups based on partCategory and distributor

        /// <summary>
        /// Get product groups based on partCategory and distributor.
        /// </summary>        
        public List<DevbridgeData> GetAutocompleteGroups(int? distributorId, string partCategory, string groupIds, string searchText)
        {

            var groupIdsList = groupIds.Split(',').Select(int.Parse).ToList();
            var listGroups = new List<DevbridgeData>();

            if (!string.IsNullOrEmpty(partCategory))
            {
                var partCategoryCodes = partCategory.Split(',').ToList();

                if (partCategory.Contains("M"))
                {
                    partCategoryCodes.Add(Constants.MFull);
                }

                if (partCategory.Contains("AA"))
                {
                    partCategoryCodes.Add(Constants.AAFull);
                }

                if (partCategory.Contains("AG"))
                {
                    partCategoryCodes.Add(Constants.AGFull);
                }
                if (partCategory.Contains("T"))
                {
                    partCategoryCodes.Add(Constants.TFull);
                }

                listGroups = (from g in db.ProductGroups.AsNoTracking()
                              join p in db.Products.AsNoTracking() on g.GroupId equals p.GroupId
                              where p.PartCategoryCode != null && g.DistributorId == distributorId && !groupIdsList.Contains(g.GroupId) && g.GroupName.Contains(searchText) && partCategoryCodes.Contains(p.PartCategoryCode)
                              select g).Distinct().Select(pg => new DevbridgeData
                              {
                                  data = pg.GroupId.ToString(),
                                  value = pg.GroupName
                              }).ToList();
            }

            return listGroups;
        }

        #endregion

        #region Get products based on Group IDs
        /// <summary>
        /// Get list of products based on group Id.
        /// </summary>        
        public List<ProductModel> GetProductsByGroupIds(List<int?> groupId)
        {
            return db.Products.Where(p => p.GroupId != null && groupId.Contains(p.GroupId)).AsNoTracking().Select(p => new ProductModel
            {
                ProductId = p.ProductId,
                ProductName = string.IsNullOrEmpty(p.Description) ? string.IsNullOrEmpty(p.ProductName) ? p.PartNo : p.ProductName : p.Description,
                GroupId = p.GroupId.Value
            }).ToList();
        }
        #endregion

        #region TargetGrowth

        #region Save/update
        public bool SaveTargetGrowth(List<TargetGrowth> modelList)
        {
            if (modelList.Count > 0)
            {
                var targetGrowth = modelList.FirstOrDefault();
                var Existdata = db.TargetGrowths.Where(x => x.SchemeId == targetGrowth.SchemeId).ToList();
                if (Existdata.Count > 0)
                {
                    db.TargetGrowths.RemoveRange(Existdata);
                    db.SaveChanges();
                }
                foreach (var item in modelList)
                {
                    var data = new TargetGrowth
                    {
                        SchemeId = item.SchemeId,
                        Min = item.Min,
                        Max = item.Max,
                        Growth = item.Growth,
                        MinTarget=item.MinTarget
                    };
                    db.TargetGrowths.Add(data);
                    db.SaveChanges();
                }
                return true;
            }
            return false;
        }
        #endregion

        public List<TargetGrowth> GetTargetGrowthsBySchemeId(int schemeId)
        {
            using (var context = new garaazEntities())
            {
                return context.TargetGrowths.Where(x => x.SchemeId == schemeId).AsNoTracking().ToList();
            }

            //    .Select(s => new TargetGrowth
            //{
            //    TragetGrowthId = s.TragetGrowthId,
            //    Min = s.Min,
            //    Max = s.Max,
            //    Growth = s.Growth,
            //    SchemeId = s.SchemeId
            //}).ToList();
        }

        #endregion

        #region Get RO Incharge based on Dist Id

        /// <summary>
        /// Get list of RO Incharge users based on the distributor Id.
        /// </summary>        
        public List<UserModel> GetRoIncharge(int? distributorId)
        {
            var userModels = new List<UserModel>();
            var ru = new RepoUsers();
            var roUsers = ru.GetDistributorUsers(distributorId, Constants.RoIncharge);

            userModels = roUsers.Select(r => new UserModel
            {
                UserId = r.UserId,
                UserName = $"{r.FirstName} {r.LastName}"

            }).ToList();

            return userModels;
        }
        #endregion

        #region Get Sales Executives based on RO User Id

        /// <summary>
        /// Get list of Sales Executives users based on RO Incharge user id.
        /// </summary>        
        public List<UserModel> GetSalesExecutives(string roUserId, bool? isAllSelected, List<UserModel> roIncharges)
        {
            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                if (isAllSelected.HasValue && isAllSelected.Value || string.IsNullOrEmpty(roUserId))
                {
                    //var ids = string.Join(",", lstRoIncharge.Select(a => a.UserId));
                    var ids = roIncharges.Select(r => r.UserId);

                    var salesExec = (from r in context.RoSalesExecutives.AsNoTracking()
                                     where ids.Contains(r.RoUserId)
                                     join u in context.UserDetails.AsNoTracking() on r.SeUserId equals u.UserId
                                     select new
                                     {
                                         u.UserId,
                                         u.FirstName,
                                         u.LastName
                                     }).Distinct().ToList();

                    return salesExec.Select(se => new UserModel { UserId = se.UserId, UserName = $"{se.FirstName} {se.LastName}" }).ToList();
                }
                else
                {
                    var salesExec = (from r in context.RoSalesExecutives.AsNoTracking()
                                     where r.RoUserId == roUserId
                                     join u in context.UserDetails.AsNoTracking() on r.SeUserId equals u.UserId
                                     select new
                                     {
                                         u.UserId,
                                         u.FirstName,
                                         u.LastName
                                     }).Distinct().ToList();

                    return salesExec.Select(se => new UserModel { UserId = se.UserId, UserName = $"{se.FirstName} {se.LastName}" }).ToList();
                }
            }

        }
        #endregion

        #region Get Sales Executives based on Branch codes

        /// <summary>
        /// Get list of sales executives based on branch codes.
        /// </summary>
        /// <param name="branchCodes">The branch codes (Outlet Ids).</param>
        /// <returns>Return list of sales executives users.</returns>
        public List<UserModel> GetSalesExecutives(List<int> branchCodes)
        {
            if (branchCodes == null) return new List<UserModel>();

            var outletUserIds = db.DistributorsOutlets.Where(d => branchCodes.Contains(d.OutletId.Value) && d.UserId != null).Select(d => d.UserId);

            var salesExecutives = (from r in db.RoSalesExecutives
                                   where outletUserIds.Contains(r.RoUserId)
                                   join u in db.UserDetails on r.SeUserId equals u.UserId
                                   select u).Distinct().ToList();

            return salesExecutives.Select(se => new UserModel { UserId = se.UserId, UserName = $"{se.FirstName} {se.LastName}" }).ToList();

        }
        #endregion

        #region Get customer types based on distributor Id

        /// <summary>
        /// Get list of customer types.
        /// </summary>        
        public List<string> GetCustomerTypes(int distributorId)
        {
            var customerTypes = new List<string>();
            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                customerTypes = (from w in context.WorkShops.AsNoTracking()
                                 join dw in context.DistributorWorkShops.AsNoTracking() on w.WorkShopId equals dw.WorkShopId
                                 where w.Type != null && dw.DistributorId == distributorId
                                 group w by w.Type into c
                                 select c.Key).ToList();
            }
            return customerTypes;
        }
        #endregion

        #region Update Schemes Criteria
        public object UpdateSchemeCriteria(SchemesCriteria model)
        {
            Scheme objScheme = null;
            if (model.SchemeId > 0)
            {
                objScheme = db.Schemes.Where(x => x.SchemeId == model.SchemeId).FirstOrDefault();
                if (objScheme != null)
                {
                    objScheme.TargetCriteria = model.TargetCriteria;
                    objScheme.TargetWorkshopCriteria = model.TargetWorkshopCriteria;
                }
                db.SaveChanges();

                return objScheme.SchemeId;
            }

            return null;
        }
        #endregion

        #region Update Schemes Criteria For cashback
        public object UpdateCashbackCriteria(CriteriaOnCashback model)
        {
            var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == model.SchemeId);
            if (scheme == null) return null;

            scheme.CashbackCriteria = model.CashbackCriteria;
            db.SaveChanges();
            return scheme.SchemeId;
        }
        #endregion

        #region Get workshop's schemes
        /// <summary>
        /// Get all schemes in which current workshop is included as target workshop.
        /// </summary>
        /// <param name="userId">The user id of the workshop.</param>
        /// <returns>Return list of workshop schemes.</returns>
        public List<ResponseSchemeModel> GetWorkshopSchemes(string userId)
        {
            var workshopSchemes = new List<ResponseSchemeModel>();

            var ru = new RepoUsers();
            var workshop = ru.GetWorkshopByUserId(userId);

            if (workshop != null)
            {
                foreach (var scheme in db.Schemes.Where(s => s.TargetWorkShops.Any(t => t.WorkShopId == workshop.WorkShopId) && s.IsActive == true))
                {
                    var giftCount = db.GiftsCoupons.Where(g => g.GiftManagement.SchemeId == scheme.SchemeId && g.WorkshopId == workshop.WorkShopId).Count();

                    var rsModel = new ResponseSchemeModel
                    {
                        SchemeId = scheme.SchemeId,
                        SchemeName = scheme.SchemeName,
                        StartDate = scheme.StartDate != null ? scheme.StartDate.ToString() : "",
                        EndDate = scheme.EndDate != null ? Convert.ToDateTime(scheme.EndDate).ToString("MMM dd, yyyy HH:mm:ss") : DateTime.Now.AddMonths(-1).ToString("MMM dd, yyyy HH:mm:ss"),
                        PaybackType = scheme.SchemesType,
                        SchemeType = scheme.Type,
                        BannerImage = scheme.BannerImage,
                        SchemeFor = scheme.SchemeFor,
                        WorkshopId = workshop.WorkShopId,
                        IsCouponAllocated = Convert.ToBoolean(scheme.IsCouponAllocated),
                        IsGiftAllocated = giftCount > 0
                    };

                    workshopSchemes.Add(rsModel);
                }
            }
            return workshopSchemes;
        }
        #endregion

        #region Get Coupon Scheme & Workshop
        /// <summary>
        /// Get coupon scheme and it's workshops.
        /// </summary>
        /// <param name="schemeId">The scheme id.</param>
        /// <returns>Return instance of CouponSchemeModel with list of workshops.</returns>
        public CouponSchemeModel GetCouponSchemeAndWorkshops(int schemeId)
        {
            var scheme = db.Schemes.SingleOrDefault(s => s.SchemeId == schemeId);
            if (scheme == null) return null;

            var csModel = new CouponSchemeModel
            {
                Scheme = scheme,
                SchemeWorkshops = new List<SchemeWorkshop>()
            };

            foreach (var tws in scheme.TargetWorkShops)
            {
                var wsId = Convert.ToInt32(tws.WorkShopId);

                if (Convert.ToBoolean(scheme.IsCouponAllocated))
                {
                    // Calculate number of coupons for particular workshop
                    // whose coupon numbers don't match in GiftsCoupon table
                    var giftCoupons = db.GiftsCoupons.Where(g => g.WorkshopId == wsId).Select(g => g.CouponNumber);
                    var totalCoupons = db.Coupons.Count(c => c.WorkshopId == wsId && c.SchemeId == schemeId && !giftCoupons.Contains(c.CouponNumber));

                    csModel.SchemeWorkshops.Add(new SchemeWorkshop
                    {
                        SchemeId = schemeId,
                        WorkshopId = wsId,
                        WorkshopName = tws.WorkShop.WorkShopName,
                        Qualified = totalCoupons > 0,
                        NumberOfCoupon = totalCoupons
                    });
                }
                else
                {
                    // NOTE: 13-09-2019 - WE MIGHT NEED COMMENTED CODE LATER. FOR NOW NOT REQUIRED
                    //var categories = db.CategorySchemes.Where(c => c.SchemeId == schemeId);
                    //if (categories.Any())
                    //{
                    //    // If category provided, then add workshop if they match to criteria
                    //    var salesTotal = sales.Sum(s => Convert.ToDecimal(s.NetRetailSelling));

                    //    var filteredCategories = categories.Where(c => c.MaxAmount > 0 ? salesTotal >= c.MinAmount && salesTotal <= c.MaxAmount : salesTotal >= c.MinAmount).OrderByDescending(c => c.MaxAmount == null).ThenByDescending(c => c.MaxAmount.HasValue);

                    //    var category = filteredCategories.Take(1).FirstOrDefault();
                    //    if (category != null)
                    //    {
                    //        var qualified = IsWorkshopQualified(schemeId, wsId, sales, out int totalCoupons);
                    //        csModel.SchemeWorkshops.Add(new SchemeWorkshop
                    //        {
                    //            WorkshopId = wsId,
                    //            WorkshopName = tws.WorkShop.WorkShopName,
                    //            Qualified = qualified,
                    //            NumberOfCoupon = totalCoupons
                    //        });
                    //    }
                    //}
                    //else
                    //{
                    var sales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.WorkShopId == tws.WorkShopId && DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(scheme.StartDate.Value) && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(scheme.EndDate.Value)).ToList();
                    var qualified = IsWorkshopQualified(schemeId, wsId, sales, out var totalCoupons);
                    csModel.SchemeWorkshops.Add(new SchemeWorkshop
                    {
                        SchemeId = schemeId,
                        WorkshopId = wsId,
                        WorkshopName = tws.WorkShop.WorkShopName,
                        Qualified = qualified,
                        NumberOfCoupon = totalCoupons
                    });
                    //}
                }
            }

            SetReasonForNoCoupons(csModel.SchemeWorkshops);

            return csModel;
        }

        /// <summary>
        /// Get coupon scheme and it's workshops.
        /// </summary>
        /// <param name="schemeId">The scheme id.</param>
        /// <param name="couponNumber">The coupon number to search workshops.</param>
        /// <returns>Return instance of CouponSchemeModel with list of workshops.</returns>
        public CouponSchemeModel GetWorkshopByCoupon(int schemeId, string couponNumber)
        {
            var scheme = db.Schemes.SingleOrDefault(s => s.SchemeId == schemeId);
            var csModel = new CouponSchemeModel
            {
                Scheme = scheme,
                SchemeWorkshops = new List<SchemeWorkshop>()
            };
            if (scheme == null) return csModel;

            // Get workshop by coupon
            var workshopId = 0;
            var coupon = db.Coupons.FirstOrDefault(c => c.CouponNumber == couponNumber);
            if (coupon != null)
            {
                workshopId = coupon.WorkshopId;
            }
            if (workshopId <= 0) return csModel;

            var tWs = scheme.TargetWorkShops.FirstOrDefault(w => w.WorkShopId == workshopId);
            if (tWs == null) return csModel;

            if (Convert.ToBoolean(scheme.IsCouponAllocated))
            {
                var totalCoupons = db.GiftsCoupons.Count(g => g.WorkshopId == tWs.WorkShopId && g.GiftManagement.SchemeId == schemeId);

                csModel.SchemeWorkshops.Add(new SchemeWorkshop
                {
                    SchemeId = schemeId,
                    WorkshopId = Convert.ToInt32(tWs.WorkShopId),
                    WorkshopName = tWs.WorkShop.WorkShopName,
                    Qualified = totalCoupons > 0,
                    NumberOfCoupon = totalCoupons
                });
            }
            else
            {
                var sales = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.WorkShopId == tWs.WorkShopId && x.CreatedDate >= scheme.StartDate.Value && x.CreatedDate <= scheme.EndDate.Value).ToList();

                var wsId = Convert.ToInt32(tWs.WorkShopId);
                var qualified = IsWorkshopQualified(schemeId, wsId, sales, out int totalCoupons);

                csModel.SchemeWorkshops.Add(new SchemeWorkshop
                {
                    SchemeId = schemeId,
                    WorkshopId = wsId,
                    WorkshopName = tWs.WorkShop.WorkShopName,
                    Qualified = qualified,
                    NumberOfCoupon = totalCoupons
                });
            }

            SetReasonForNoCoupons(csModel.SchemeWorkshops);

            return csModel;
        }

        /// <summary>
        /// Check if particular workshop is qualified as per criteria set for scheme and provide total number of coupons.
        /// </summary>
        /// <param name="schemeId">The scheme id.</param>
        /// <param name="workshopId">The id of workshop whose qualification need to be check.</param>
        /// <param name="sales">The total sales for the workshop.</param>
        /// <param name="totalCoupons">The total number of coupons as per criteria.</param>
        /// <returns>Return true if qualified else false.</returns>
        private bool IsWorkshopQualified(int schemeId, int workshopId, List<DailySalesTrackerWithInvoiceData> sales, out int totalCoupons)
        {
            totalCoupons = 0;
            var isQualified = false;

            var scheme = db.Schemes.SingleOrDefault(s => s.SchemeId == schemeId);
            if (scheme == null) return false;

            // Check 'IsQualified' by Part Info
            var targetWs = db.TargetWorkShops.FirstOrDefault(x => x.SchemeId == schemeId && x.WorkShopId == workshopId);
            if (targetWs != null)
            {
                isQualified = Convert.ToBoolean(targetWs.IsQualifiedAsDefault);

                if (!isQualified && !string.IsNullOrEmpty(targetWs.NewTarget))
                {
                    var newTarget = Convert.ToDouble(targetWs.NewTarget);
                    if (newTarget > 0)
                    {
                        const string focusPartsGroup = "Focus Parts Group";
                        const string fmsPartsGroup = "FMS Parts Group";

                        var groups = GetGroupsByPartCategory(scheme.PartCategory, scheme.DistributorId);
                        var groupIds = groups.Select(g => g.GroupId);

                        double achievedTarget;
                        if (scheme.PartType.Equals(focusPartsGroup, StringComparison.OrdinalIgnoreCase))
                        {
                            var focusParts = db.FocusParts.Where(f => f.SchemeId == schemeId).ToList();
                            var fpGroupIds = focusParts.Where(f => f.ProductId == null && groupIds.Contains(Convert.ToInt32(f.GroupId))).Select(f => f.GroupId);
                            var fpProductIds = focusParts.Where(f => f.ProductId != null && groupIds.Contains(Convert.ToInt32(f.GroupId))).Select(f => f.ProductId);

                            achievedTarget = sales.Where(s => fpGroupIds.Contains(s.GroupId) || fpProductIds.Contains(s.ProductId))
                                .Sum(s => Convert.ToDouble(s.NetRetailSelling));
                        }
                        else if (scheme.PartType.Equals(fmsPartsGroup, StringComparison.OrdinalIgnoreCase))
                        {
                            var partCreation = scheme.PartCreations;
                            if (partCreation.Equals("All", StringComparison.OrdinalIgnoreCase))
                            {
                                achievedTarget = sales.Where(s => groupIds.Contains(Convert.ToInt32(s.GroupId))).Sum(s => Convert.ToDouble(s.NetRetailSelling));
                            }
                            else
                            {
                                // Get groups as per part creation
                                var fmsGroupIds = db.FmsGroupSales.Where(f => f.PartCreation == partCreation).Select(f => f.GroupId);

                                // Sum sales of workshop as per part creation's groups
                                achievedTarget = sales.Where(s => fmsGroupIds.Contains(Convert.ToInt32(s.GroupId))).Sum(s => Convert.ToDouble(s.NetRetailSelling));
                            }
                        }
                        else
                        {
                            achievedTarget = sales.Where(s => groupIds.Contains(Convert.ToInt32(s.GroupId))).Sum(s => Convert.ToDouble(s.NetRetailSelling));
                        }

                        isQualified = achievedTarget >= newTarget;
                    }
                }
            }

            if (!isQualified) return false;

            // Calculate coupons if qualified
            var qcList = new List<QualifyCriteriaUserLevel>();
            foreach (var qc in db.QualifyCriterias.Where(x => x.SchemeId == schemeId))
            {
                double achievedTarget = 0;
                if (qc.Type == "All" || string.IsNullOrEmpty(qc.Type))
                {
                    achievedTarget = sales.Sum(s => Convert.ToDouble(s.NetRetailSelling));
                }
                else
                {
                    if (string.IsNullOrEmpty(qc.TypeValue))
                    {
                        if (qc.Type.Equals("Parts", StringComparison.OrdinalIgnoreCase))
                        {
                            sales = (from s in sales
                                     join p in db.Products on s.ProductId equals p.ProductId
                                     where p.ProductType == "Parts"
                                     select s).ToList();
                            achievedTarget = sales.Sum(s => Convert.ToDouble(s.NetRetailSelling));
                        }
                        else if (qc.Type.Equals("Accessories", StringComparison.OrdinalIgnoreCase))
                        {
                            sales = (from s in sales
                                     join p in db.Products on s.ProductId equals p.ProductId
                                     where p.ProductType == "Accessories"
                                     select s).ToList();
                            achievedTarget = sales.Sum(s => Convert.ToDouble(s.NetRetailSelling));
                        }
                    }
                    else
                    {
                        int.TryParse(qc.TypeValue, out int productId);
                        achievedTarget = sales.Where(x => x.ProductId == productId).Sum(s => Convert.ToDouble(s.NetRetailSelling));
                    }
                }

                if (achievedTarget >= Convert.ToDouble(qc.AmountUpto))
                {
                    qcList.Add(new QualifyCriteriaUserLevel
                    {
                        AchievedTarget = achievedTarget,
                        AmountUpto = qc.AmountUpto,
                        NumberOfCoupons = qc.NumberOfCoupons,
                        AdditionalCouponAmount = qc.AdditionalCouponAmount,
                        AdditionalNumberOfCoupons = qc.AdditionalNumberOfCoupons
                    });
                }
            }

            if (qcList.Count > 0)
            {
                int coupon = 0, additionalCoupon = 0;

                // Get coupon and additional coupon
                var qcUserLevel = qcList.OrderByDescending(o => Convert.ToDouble(o.AmountUpto)).Take(1).FirstOrDefault();
                if (qcUserLevel != null)
                {
                    coupon = Convert.ToInt32(qcUserLevel.NumberOfCoupons);
                    var additionalCouponAmt = Convert.ToDouble(qcUserLevel.AdditionalCouponAmount);
                    var amount = Convert.ToDouble(qcUserLevel.AmountUpto);

                    var quotient = qcUserLevel.AchievedTarget / amount; // 890/100 = 8
                    var remainder = qcUserLevel.AchievedTarget % amount; // 890/100 = 90        

                    coupon = (int)Math.Truncate(quotient) * coupon; // 8 * 2 = 164

                    if (additionalCouponAmt > 0)
                    {
                        var quotientForAdditional = (int)Math.Truncate(remainder) / additionalCouponAmt; // 90/10 = 9
                        additionalCoupon = (int)Math.Truncate(quotientForAdditional) * Convert.ToInt32(qcUserLevel.AdditionalNumberOfCoupons); //9 *1 = 9
                    }
                }

                // Set total coupons
                totalCoupons = coupon > 0 && additionalCoupon > 0 ? coupon + additionalCoupon : coupon;
            }

            return true;
        }

        /// <summary>
        /// Set reason for zero coupons if workshop selected choice other than lucky draw.
        /// </summary>
        /// <param name="schemeWorkshops">List of workshops that will be updated.</param>
        private void SetReasonForNoCoupons(List<SchemeWorkshop> schemeWorkshops)
        {
            if (schemeWorkshops.Count <= 0) return;

            var selectedTypes = db.WorkshopSchemesSelectedTypes.ToList();
            foreach (var sw in schemeWorkshops)
            {
                var st = selectedTypes.FirstOrDefault(s => s.SchemeId == sw.SchemeId && s.WorkshopId == sw.WorkshopId);
                if (st == null) continue;
                if (st.SelectedOption.Equals(LuckyDraw, StringComparison.OrdinalIgnoreCase))
                {
                    sw.IsLuckyDrawSelected = true;
                    continue;
                }

                sw.IsLuckyDrawSelected = false;
                if (st.SelectedOption.Equals(Cashback, StringComparison.OrdinalIgnoreCase))
                {
                    sw.MsgForNoCoupons = "NA as workshop selected cashback";
                }
                else if (st.SelectedOption.Equals(AssuredGift, StringComparison.OrdinalIgnoreCase))
                {
                    sw.MsgForNoCoupons = "NA as workshop selected assured gift";
                }
            }
        }
        #endregion

        #region Get, Generate & Save coupons

        /// <summary>
        /// Get coupons for particular scheme and workshop.
        /// </summary>        
        /// <param name="workshopId">The workshop id.</param>
        /// <returns>Return list of coupons.</returns>
        public List<Coupon> GetCoupons(int schemeId, int workshopId)
        {
            var giftCoupons = db.GiftsCoupons.Where(g => g.WorkshopId == workshopId).Select(g => g.CouponNumber);
            return db.Coupons.Where(c => c.WorkshopId == workshopId && c.SchemeId == schemeId && !giftCoupons.Contains(c.CouponNumber)).OrderBy(c => c.CouponNumber).ToList();
        }

        /// <summary>
        /// Get coupons for particular scheme.
        /// </summary>
        /// <param name="schemeId">The scheme id.</param>
        /// <param name="giftId">The gift for which coupons need to be set.</param>
        /// <returns>Return instance of SchemeGift with coupons.</returns>
        public SchemeGift GetCouponsModel(int schemeId, int giftId)
        {
            var sg = new SchemeGift();
            var coupons = string.Empty;
            int giftRemainingQty = 0;

            var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == schemeId);
            if (scheme == null) return sg;

            // Get gift's remaining quantity                
            var gift = db.GiftManagements.FirstOrDefault(g => g.SchemeId == schemeId && g.GiftId == giftId);
            if (gift != null)
            {
                sg.GiftName = gift.Gift;

                // Compute gift's remaining qty
                var giftQty = Convert.ToInt32(gift.Qty);
                if (giftQty > 0)
                {
                    var gcCount = db.GiftsCoupons.Count(g => g.GiftId == gift.GiftId);
                    giftRemainingQty = giftQty - gcCount;
                }
            }

            // If gift remaining, get coupons
            if (giftRemainingQty > 0)
            {
                var canTakeMoreThanOneGift = scheme.CanTakeMoreThanOneGift ?? false;
                var allCoupons = db.Coupons.Where(c => c.SchemeId == schemeId).ToList();
                var filteredCoupons = new List<Coupon>();

                // Add default coupons first
                var defaultCouponNumbers = db.DefaultWinners.Where(d => d.SchemeId == schemeId && d.GiftId == giftId).Select(d => d.CouponNumber);
                filteredCoupons.AddRange(allCoupons.Where(c => defaultCouponNumbers.Contains(c.CouponNumber)).Take(giftRemainingQty));

                if (canTakeMoreThanOneGift)
                {
                    var maxGiftsAllowed = Convert.ToInt32(scheme.MaxGiftsAllowed);
                    if (maxGiftsAllowed > 0)
                    {
                        // Loop each workshop and add their coupons to their max gifts allowed
                        foreach (var wsId in db.Coupons.Where(g => g.SchemeId == schemeId).Select(g => g.WorkshopId).Distinct())
                        {
                            var allocatedCouponNumbers = db.GiftsCoupons.Where(g => g.WorkshopId == wsId).Select(g => g.CouponNumber);
                            var remainingGiftsAllowed = maxGiftsAllowed - allocatedCouponNumbers.Count();

                            if (remainingGiftsAllowed <= 0 || giftRemainingQty <= 0) continue;

                            var remainingCapacity = giftRemainingQty - filteredCoupons.Count;
                            if (remainingCapacity <= 0) continue;
                            filteredCoupons.AddRange(allCoupons.Where(c => c.SchemeId == schemeId && c.WorkshopId == wsId && !allocatedCouponNumbers.Contains(c.CouponNumber) && !defaultCouponNumbers.Contains(c.CouponNumber)).Take(remainingCapacity));
                        }
                    }
                    else
                    {
                        var remainingCapacity = giftRemainingQty - filteredCoupons.Count;
                        if (remainingCapacity > 0)
                        {
                            var allocatedCouponNumbers = db.GiftsCoupons.Select(g => g.CouponNumber);
                            filteredCoupons.AddRange(allCoupons.Where(c => !allocatedCouponNumbers.Contains(c.CouponNumber) && !defaultCouponNumbers.Contains(c.CouponNumber)).Take(remainingCapacity));
                        }
                    }
                }
                else
                {
                    // Add coupons that doesn't match with allocated workshop
                    // And workshop attach to coupons should not repeat for any other coupon
                    var remainingCapacity = giftRemainingQty - filteredCoupons.Count;
                    var allocatedWsIds = db.GiftsCoupons.Where(g => g.GiftManagement.SchemeId == schemeId).Select(g => g.WorkshopId).ToList();

                    if (remainingCapacity > 0)
                    {
                        // Ref - https://stackoverflow.com/a/491832/1041457
                        // Explanation - Where filter coupons. GroupBy and Select perform distinct by WorkshopId.

                        if (defaultCouponNumbers.Any())
                        {
                            var defaultWsIds = filteredCoupons.Select(d => d.WorkshopId).Distinct();
                            var uniqueCoupons = allCoupons.Where(c => !allocatedWsIds.Contains(c.WorkshopId) && !defaultWsIds.Contains(c.WorkshopId)).GroupBy(c => c.WorkshopId).Select(grp => grp.FirstOrDefault()).Take(remainingCapacity);
                            filteredCoupons.AddRange(uniqueCoupons);
                        }
                        else
                        {
                            var uniqueCoupons = allCoupons.Where(c => !allocatedWsIds.Contains(c.WorkshopId)).GroupBy(c => c.WorkshopId).Select(grp => grp.FirstOrDefault()).Take(remainingCapacity);
                            filteredCoupons.AddRange(uniqueCoupons);
                        }
                    }
                }

                // Pick random coupons                    
                if (filteredCoupons.Count > 0)
                {
                    var randomIndexes = new HashSet<int>();
                    var random = new Random();
                    do
                    {
                        randomIndexes.Add(random.Next(filteredCoupons.Count));
                    } while (randomIndexes.Count < giftRemainingQty);

                    foreach (var index in randomIndexes)
                    {
                        var coupon = filteredCoupons[index];
                        coupons = !string.IsNullOrEmpty(coupons) ? $"{coupons},{coupon.CouponNumber}" : coupon.CouponNumber;
                    }
                }
            }

            sg.GiftId = giftId;
            sg.Coupons = coupons;
            sg.GiftRemainingQty = giftRemainingQty;
            sg.SchemeName = scheme.SchemeName;

            return sg;

        }

        /// <summary>
        /// Generate and save coupons for particular workshop in scheme.
        /// </summary>
        /// <param name="schemeWorkshops">The list of scheme workshops.</param>
        public void GenerateAndSaveCoupons(List<SchemeWorkshop> schemeWorkshops)
        {
            if (schemeWorkshops.Count <= 0) return;

            var couponAllocated = false;
            var selectedTypes = db.WorkshopSchemesSelectedTypes.ToList();

            foreach (var sw in schemeWorkshops)
            {
                if (sw.Qualified && sw.NumberOfCoupon > 0)
                {
                    // Check if workshop selected option other than lucky draw
                    var st = selectedTypes.FirstOrDefault(s => s.SchemeId == sw.SchemeId && s.WorkshopId == sw.WorkshopId);
                    if (st != null && !st.SelectedOption.Equals(LuckyDraw, StringComparison.OrdinalIgnoreCase)) continue;

                    var rnd = new Random();
                    for (var i = 1; i <= sw.NumberOfCoupon; i++)
                    {
                    NewCoupon: var couponNumber = Utils.GenerateCoupon(6, rnd);
                        var existingCoupon = db.Coupons.FirstOrDefault(c => c.SchemeId == sw.SchemeId && c.CouponNumber == couponNumber);
                        if (existingCoupon != null)
                        {
                            goto NewCoupon;
                        }

                        db.Coupons.Add(new Coupon
                        {
                            SchemeId = sw.SchemeId,
                            WorkshopId = sw.WorkshopId,
                            CouponNumber = couponNumber
                        });
                        couponAllocated = db.SaveChanges() > 0;
                    }
                }
            }

            if (couponAllocated)
            {
                var schemedId = schemeWorkshops.Select(s => s.SchemeId).FirstOrDefault();
                var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == schemedId);
                if (scheme != null)
                {
                    scheme.IsCouponAllocated = true;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Generate and save coupons for particular workshop and scheme.
        /// </summary>
        /// <param name="schemeId">The scheme id.</param>
        /// <param name="workshopId">The workshop for which coupon will be generated.</param>
        /// <param name="numberOfCoupons">The number of coupons to generate and save.</param>
        internal void GenerateAndSaveCoupons(int schemeId, int workshopId, int numberOfCoupons)
        {
            bool couponAllocated = false;
            Random rnd = new Random();
            for (int i = 1; i <= numberOfCoupons; i++)
            {
            NewCoupon: var couponNumber = Utils.GenerateCoupon(6, rnd);
                var existingCoupon = db.Coupons.FirstOrDefault(c => c.SchemeId == schemeId && c.WorkshopId == workshopId && c.CouponNumber == couponNumber);
                if (existingCoupon != null)
                {
                    goto NewCoupon;
                }

                db.Coupons.Add(new Coupon
                {
                    SchemeId = schemeId,
                    WorkshopId = workshopId,
                    CouponNumber = couponNumber
                });
                couponAllocated = db.SaveChanges() > 0;
            }

            if (couponAllocated)
            {
                var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == schemeId);
                if (scheme != null)
                {
                    scheme.IsCouponAllocated = true;
                    db.SaveChanges();
                }
            }
        }

        #endregion

        #region Get Gift Data
        public SchemeGiftModel GetCategoryGifts(int schemeId)
        {
            // In order - Gold, Orange, Green, Deep sky blue, hot pink, dim gray, tan, red, dark khaki and teal
            string[] colorCodes = { "#FFD700", "#FFA500", "#008000", "#00BFFF", "#FF69B4", "#696969", "#D2B48C", "#FF0000", "#BDB76B", "#008080" };

            var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == schemeId);
            var sgModel = new SchemeGiftModel { Scheme = scheme };
            var general = new General();

            // List: Categories
            var giftManagements = db.GiftManagements.ToList();
            var categorySchemes = db.CategorySchemes.Where(c => c.SchemeId == schemeId).OrderBy(c => c.Category).ToList();
            if (categorySchemes.Count == 0)
            {
                var gm = giftManagements.Where(x => x.SchemeId == schemeId);
                if (gm.Any())
                {
                    categorySchemes.Add(new CategoryScheme
                    {
                        Category = "Default",
                        CategoryId = 0
                    });
                }
            }

            var colorIndex = 0;
            var giftsCoupons = db.GiftsCoupons.ToList();
            foreach (var cScheme in categorySchemes)
            {
                var listGifts = new List<SchemeCategoryGift>();

                // List: Gift Management
                var gifts = cScheme.CategoryId == 0 ? giftManagements.Where(g => g.SchemeId == schemeId).OrderBy(g => g.DrawOrder) : giftManagements.Where(g => g.SchemeId == schemeId && g.GiftCategories.Any(c => c.CategoryId == cScheme.CategoryId || c.IsAll == true)).OrderBy(g => g.DrawOrder);

                foreach (var gift in gifts)
                {
                    var imgPath = gift.ImagePath != null ? general.CheckImageUrl(gift.ImagePath) : general.CheckImageUrl("/assets/images/NoPhotoAvailable.png");

                    // Set quantity of gift as per allocation to coupons
                    var giftQty = Convert.ToInt32(gift.Qty);
                    var gcCount = giftsCoupons.Count(g => g.GiftId == gift.GiftId);
                    var remainingQty = giftQty >= gcCount ? giftQty - gcCount : 0;

                    listGifts.Add(new SchemeCategoryGift
                    {
                        GiftId = gift.GiftId,
                        GiftName = gift.Gift,
                        GiftImage = imgPath,
                        Qty = remainingQty,
                        CouponAllocated = gcCount > 0
                    });
                }

                sgModel.GiftDatas.Add(new GiftData
                {
                    CategoryName = cScheme.Category,
                    ColorCode = colorCodes.Length > colorIndex ? colorCodes[colorIndex] : "#8B4513",
                    data = listGifts
                });

                colorIndex++;
            }

            return sgModel;
        }
        #endregion

        #region Save coupon for Gifts
        public bool SaveCouponForGift(int giftId, string couponNumber, out string workshopName)
        {
            int? wsId;
            var defaultWinner = db.DefaultWinners.FirstOrDefault(d => d.GiftId == giftId && d.CouponNumber == couponNumber);
            if (defaultWinner != null)
            {
                wsId = defaultWinner.WorkshopId;
                workshopName = db.WorkShops.FirstOrDefault(w => w.WorkShopId == wsId)?.WorkShopName;
                db.GiftsCoupons.Add(new GiftsCoupon
                {
                    GiftId = giftId,
                    WorkshopId = Convert.ToInt32(wsId),
                    CouponNumber = couponNumber
                });
                var couponSaved = db.SaveChanges() > 0;

                if (couponSaved)
                {
                    db.DefaultWinners.Remove(defaultWinner);
                    db.SaveChanges();
                }

                return couponSaved;
            }
            else
            {
                var coupon = db.Coupons.FirstOrDefault(c => c.CouponNumber == couponNumber);
                wsId = coupon?.WorkshopId;

                workshopName = db.WorkShops.FirstOrDefault(w => w.WorkShopId == wsId)?.WorkShopName;
                db.GiftsCoupons.Add(new GiftsCoupon
                {
                    GiftId = giftId,
                    WorkshopId = Convert.ToInt32(wsId),
                    CouponNumber = couponNumber
                });
                return db.SaveChanges() > 0;
            }
        }
        #endregion

        #region Get Winners
        /// <summary>
        /// Get list of winners containing coupon and workshop for particular gift.
        /// </summary>
        /// <param name="giftId">The gift Id for which coupon and workshop will be retrieved.</param>
        /// <returns>Return list of coupon and workshop.</returns>
        public List<SchemeWorkshop> GetWinners(int giftId, ref string giftName)
        {
            var giftManagement = db.GiftManagements.Where(g => g.GiftId == giftId).FirstOrDefault();
            giftName = giftManagement?.Gift;

            var couponAndWorkshops = new List<SchemeWorkshop>();
            foreach (var coupon in db.GiftsCoupons.Where(g => g.GiftId == giftId))
            {
                var workshop = db.WorkShops.Where(w => w.WorkShopId == coupon.WorkshopId).FirstOrDefault();
                couponAndWorkshops.Add(new SchemeWorkshop
                {
                    WorkshopName = workshop?.WorkShopName,
                    CouponNumber = coupon.CouponNumber
                });
            }

            return couponAndWorkshops;
        }
        #endregion

        #region Get gift for workshop

        /// <summary>
        /// Get allocated gifts for workshop.
        /// </summary>
        /// <param name="schemeId">The scheme Id for which gift need to be retrieved.</param>
        /// <param name="workshopId">The workshop Id for which gift need to be retrieved.</param>
        /// <returns>Return list of gift's name.</returns>
        public List<string> GetGiftForWorkshop(int schemeId, int workshopId)
        {
            var gifts = new List<string>();
            var giftCoupons = db.GiftsCoupons.Where(g => g.WorkshopId == workshopId && g.GiftManagement.SchemeId == schemeId);

            if (giftCoupons.Any())
            {
                gifts = giftCoupons.Select(g => g.GiftManagement.Gift).ToList();
            }

            return gifts;

        }
        #endregion

        #region Get DecideWinners model
        public DecideWinnersModel GetDecideWinners(int schemeId, int giftId)
        {
            var dwModel = new DecideWinnersModel
            {
                SchemeId = schemeId,
                GiftId = giftId
            };

            var workshops = db.TargetWorkShops.Where(t => t.SchemeId == schemeId).Select(t => t.WorkShop);
            var giftManagement = db.GiftManagements.FirstOrDefault(g => g.SchemeId == schemeId && g.GiftId == giftId);
            var giftQty = giftManagement != null ? Convert.ToInt32(giftManagement.Qty) : 0;
            dwModel.GiftName = giftManagement?.Gift;
            if (giftQty > 0)
            {
                var gcCount = db.GiftsCoupons.Where(g => g.GiftId == giftId).Count();
                var remainingQty = giftQty >= gcCount ? giftQty - gcCount : 0;
                if (remainingQty > 0)
                {
                    dwModel.GiftRemainingQty = remainingQty;

                    foreach (var ws in workshops)
                    {
                        var giftCoupons = db.GiftsCoupons.Where(g => g.WorkshopId == ws.WorkShopId && g.GiftId == giftId).Select(g => g.CouponNumber);
                        var wc = new WorkshopCoupons
                        {
                            Workshop = ws,
                            Coupons = db.Coupons.Where(c => c.SchemeId == schemeId && c.WorkshopId == ws.WorkShopId && !giftCoupons.Contains(c.CouponNumber)).Select(c => c.CouponNumber).Take(remainingQty).ToList()
                        };
                        dwModel.WorkshopCoupons.Add(wc);
                    }
                }
            }

            // Get saved one for each workshop
            foreach (var ws in workshops)
            {
                var wc = new WorkshopCoupons
                {
                    Workshop = ws,
                    Coupons = db.DefaultWinners.Where(d => d.SchemeId == schemeId && d.WorkshopId == ws.WorkShopId && d.GiftId == giftId).Select(d => d.CouponNumber).ToList()
                };
                dwModel.DefaultWinners.Add(wc);
            }

            return dwModel;
        }
        #endregion

        #region Save DecideWinners
        /// <summary>
        /// Save the default winners for each gift.
        /// </summary>        
        /// <returns>Return true if save successfully else false.</returns>
        internal bool SaveDecideWinners(List<SchemeWorkshop> model)
        {
            // Remove existing for particular scheme and gift
            var schemeId = model.FirstOrDefault()?.SchemeId;
            var giftId = model.FirstOrDefault()?.GiftId;
            var existingDws = db.DefaultWinners.Where(d => d.SchemeId == schemeId && d.GiftId == giftId);
            db.DefaultWinners.RemoveRange(existingDws);
            db.SaveChanges();

            var defaultWinners = model.Select(df => new DefaultWinner
            {
                SchemeId = df.SchemeId,
                WorkshopId = df.WorkshopId,
                GiftId = df.GiftId,
                CouponNumber = df.CouponNumber
            });
            db.DefaultWinners.AddRange(defaultWinners);
            return db.SaveChanges() > 0;
        }
        #endregion

        public bool UpdateFocusPartBenifitType(FocusPartBenifitTypeModel model)
        {
            var scheme = db.Schemes.FirstOrDefault(x => x.SchemeId == model.SchemeId);
            if (scheme == null) return false;

            scheme.FocusPartBenifitType = model.FocusPartBenifitType;
            scheme.FocusPartBenifitTypeValue = model.FocusPartBenifitTypeValue;
            scheme.FocusPartTarget = model.FocusPartTarget ?? 0;
            scheme.FocusPartBenifitTypeNumber = model.FocusPartBenifitTypeNumber;
            db.SaveChanges();

            return true;
        }

        #endregion

        #region Scheme Preview

        /// <summary>
        /// Get preview of particular scheme.
        /// </summary>
        /// <param name="schemeId">Id of the scheme for which details need to be fetch.</param>
        public async Task<SchemePreview> GetSchemePreviewAsync(int schemeId)
        {
            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var scheme = context.Schemes.AsNoTracking().FirstOrDefault(s => s.SchemeId == schemeId);
                if (scheme == null)
                    throw new Exception($"Scheme with scheme id '{schemeId}' doesn't exist.");

                const string dateFormat = "MMM dd, yyyy";
                const string notAvailable = "NA";

                var general = new General();
                var sp = new SchemePreview();

                #region Basic Information

                var schemeType = ClsSchema.SchemeTypes.FirstOrDefault(s => s.Value == scheme.Type)?.Text;
                var distributor = context.Distributors.AsNoTracking().FirstOrDefault(d => d.DistributorId == scheme.DistributorId);

                sp.SchemeName = scheme.SchemeName;
                sp.SchemeType = schemeType ?? scheme.Type;
                sp.SchemeDateRange = $"{scheme.StartDate?.ToString(dateFormat)} - {scheme.EndDate?.ToString(dateFormat)}";
                sp.DistributorName = distributor?.DistributorName;
                sp.PaybackPeriod = scheme.DispersalFrequency;
                sp.PaybackType = scheme.SchemesType != null ? scheme.SchemesType.Equals("Percentage", StringComparison.OrdinalIgnoreCase) ? "Cashback" : scheme.SchemesType : notAvailable;
                sp.ThumbnailImgUrl = general.CheckImageUrl(scheme.ThumbnailImage);
                sp.BannerImgUrl = scheme.BannerImage;
                sp.IsActive = scheme.IsActive ?? false;

                // Set reward options
                if (scheme.Type.Equals(Cashback, StringComparison.OrdinalIgnoreCase))
                {
                    sp.SubSchemeType = scheme.SubSchemeType.Humanize();
                }
                else if (scheme.Type.Equals(LuckyDraw, StringComparison.OrdinalIgnoreCase))
                {
                    var rewardOptions = string.Empty;
                    var isCashback = Convert.ToBoolean(scheme.CashBack);
                    var isAssuredGift = Convert.ToBoolean(scheme.AssuredGift);
                    var isLuckyDraw = Convert.ToBoolean(scheme.LuckyDraw);
                    if (isCashback)
                    {
                        rewardOptions += "Cashback,";
                    }
                    if (isAssuredGift)
                    {
                        rewardOptions += " Assured gifts,";
                    }
                    if (isLuckyDraw)
                    {
                        rewardOptions += " Lucky draw";
                    }
                    sp.RewardOptions = rewardOptions.Trim(',', ' ');
                }

                #endregion

                #region Parts Information

                if (!string.IsNullOrEmpty(scheme.PartCategory))
                {
                    sp.PartCategory = scheme.PartCategory;
                }
                else
                {
                    sp.PartCategory = notAvailable;
                }

                sp.PartGroup = scheme.PartType ?? notAvailable;
                sp.FPercentage = Convert.ToString(scheme.FValue);
                sp.MPercentage = Convert.ToString(scheme.MValue);
                sp.SPercentage = Convert.ToString(scheme.SValue);
                sp.PartCreation = scheme.PartCreations;

                if (!string.IsNullOrWhiteSpace(sp.PartGroup))
                {
                    if (sp.PartGroup.Equals(sp.FmsPartsGroup, StringComparison.OrdinalIgnoreCase))
                    {
                        sp.DateRangeForFms = $"{scheme.StartRange?.ToString(dateFormat)} - {scheme.EndRange?.ToString(dateFormat)}";
                    }
                    else if (sp.PartGroup.Equals(sp.FocusPartsGroup, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var fpContext = new garaazEntities())
                        {
                            fpContext.Configuration.AutoDetectChangesEnabled = false;

                            var focusParts = await fpContext.FocusParts.Where(f => f.SchemeId == schemeId).AsNoTracking().Select(f => new
                            {
                                f.GroupId,
                                f.ProductId
                            }).ToListAsync().ConfigureAwait(false);

                            if (focusParts.Count > 0)
                            {
                                var groupIds = focusParts.Select(f => f.GroupId).Distinct();
                                var productIds = focusParts.Where(f => f.GroupId != null && f.ProductId != null).Select(f => f.ProductId.Value).Distinct();

                                var products = await fpContext.Products.Where(p => p.DistributorId == scheme.DistributorId && (groupIds.Contains(p.GroupId) || productIds.Contains(p.ProductId))).Include(p => p.ProductGroup).AsNoTracking().Select(p => new
                                {
                                    p.ProductGroup.GroupName,
                                    p.GroupId,
                                    p.PartNo,
                                    p.Description,
                                    p.Price
                                }).ToListAsync().ConfigureAwait(false);

                                var focusPartsModels = groupIds.Select(g => new SchemePreviewFocusPart()
                                {
                                    PartGroup = products.FirstOrDefault(pg => pg.GroupId == g)?.GroupName,
                                    Parts = products.Where(f => f.GroupId == g).Select(p => new SchemePreviewPart
                                    {
                                        PartNumber = p.PartNo,
                                        PartDescription = p.Description,
                                        MRP = Convert.ToString(p.Price)
                                    }).ToList()
                                });
                                focusPartsModels = focusPartsModels.Where(f => f.PartGroup != null);
                                sp.FocusParts.AddRange(focusPartsModels);
                            }
                        }
                    }
                }

                #endregion

                #region Customer Segment

                if (!string.IsNullOrEmpty(scheme.BranchCode))
                {
                    var schBranchCodes = scheme.BranchCode.Split(',');
                    var ro = new RepoOutlet();
                    var distOutlets = ro.GetDistributorOutlets(scheme.DistributorId);
                    sp.BranchCode = string.Join(", ", distOutlets.Where(d => schBranchCodes.Contains(d.OutletId.ToString())).Select(d => d.OutletName));
                }
                else
                {
                    sp.BranchCode = notAvailable;
                }

                if (!string.IsNullOrEmpty(scheme.SalesExecutiveId))
                {
                    var schSalesExecIds = scheme.SalesExecutiveId.Split(',');
                    var roIncharges = GetRoIncharge(scheme.DistributorId);
                    var salesPerson = GetSalesExecutives(null, null, roIncharges);
                    sp.SalesPerson = string.Join(", ", salesPerson.Where(s => schSalesExecIds.Contains(s.UserId)).Select(s => s.UserName));
                }
                else
                {
                    sp.SalesPerson = notAvailable;
                }

                if (!string.IsNullOrEmpty(scheme.PartyType))
                {
                    var schPartyTypes = scheme.PartyType.Split(',');
                    sp.CustomerType = string.Join(", ", schPartyTypes);
                }
                else
                {
                    sp.CustomerType = notAvailable;
                }

                sp.TargetGrowths = GetTargetGrowthsBySchemeId(schemeId);
                sp.Categories = GetCategoryScheme(schemeId);

                #endregion

                #region Target Information

                sp.PrevYearDateRange = scheme.PrevYearFromDate != null && scheme.PrevYearToDate != null ? $"{scheme.PrevYearFromDate?.ToString(dateFormat)} - {scheme.PrevYearToDate?.ToString(dateFormat)}" : notAvailable;
                sp.PrevMonthDateRange = scheme.PrevMonthFromDate != null && scheme.PrevMonthToDate != null ? $"{scheme.PrevMonthFromDate?.ToString(dateFormat)} - {scheme.PrevMonthToDate?.ToString(dateFormat)}" : notAvailable;

                if (scheme.PrevYearFromDate != null && scheme.PrevYearToDate != null && scheme.PrevMonthFromDate != null &&
                    scheme.PrevMonthToDate != null)
                {
                    var isTargetAchieved = context.TargetWorkShops.Any(w => w.SchemeId == schemeId && w.TargetAchieved != null);
                    if (isTargetAchieved)
                    {
                        var targetws = (from tw in context.TargetWorkShops
                                        join w in context.WorkShops on tw.WorkShopId equals w.WorkShopId
                                        where tw.SchemeId == schemeId
                                        select new ResponseTargetWorkshopModel
                                        {
                                            WorkShopId = w.WorkShopId,
                                            LocationCode = w.Outlet != null ? w.Outlet.OutletCode : "",
                                            RoIncharge = w.Outlet != null ? w.Outlet.OutletName : "",
                                            WorkShopName = w.WorkShopName,
                                            CustomerType = tw.CustomerType,
                                            PrevYearAvgSale = tw.PrevYearAvgSale,
                                            GrowthPercentage = tw.GrowthPercentage,
                                            NewTarget = tw.NewTarget,
                                            PrevMonthAvgSale = tw.PrevMonthAvgSale,
                                            GrowthComparisonPercentage = tw.GrowthComparisonPercentage,
                                            IsQualifiedAsDefault = tw.IsQualifiedAsDefault ?? false,
                                            TargetAchieved = tw.TargetAchieved ?? 0,
                                            AchievedPercentage = tw.TargetAchievedPercentage ?? 0
                                        }).AsNoTracking().OrderBy(w => w.WorkShopName).ToList();

                        foreach (var ws in targetws)
                        {
                            var sales = (from se in context.SalesExecutiveWorkshops
                                         join u in context.UserDetails on se.UserId equals u.UserId
                                         where se.WorkshopId == ws.WorkShopId
                                         select u).AsNoTracking().FirstOrDefault();
                            ws.SalesExecutive = string.Join(sales?.FirstName, sales?.LastName);
                        }
                        sp.TargetWorkshops.AddRange(targetws);
                    }
                    else
                    {
                        var isTargetAchievedSaved = SaveTargetAchievedForWorkshop(schemeId);
                        if (isTargetAchievedSaved)
                        {
                            var targetws = (from tw in context.TargetWorkShops
                                            join w in context.WorkShops on tw.WorkShopId equals w.WorkShopId
                                            where tw.SchemeId == schemeId
                                            select new ResponseTargetWorkshopModel
                                            {
                                                WorkShopId = w.WorkShopId,
                                                LocationCode = w.Outlet != null ? w.Outlet.OutletCode : "",
                                                RoIncharge = w.Outlet != null ? w.Outlet.OutletName : "",
                                                WorkShopName = w.WorkShopName,
                                                CustomerType = tw.CustomerType,
                                                PrevYearAvgSale = tw.PrevYearAvgSale,
                                                GrowthPercentage = tw.GrowthPercentage,
                                                NewTarget = tw.NewTarget,
                                                PrevMonthAvgSale = tw.PrevMonthAvgSale,
                                                GrowthComparisonPercentage = tw.GrowthComparisonPercentage,
                                                IsQualifiedAsDefault = tw.IsQualifiedAsDefault ?? false,
                                                TargetAchieved = tw.TargetAchieved ?? 0,
                                                AchievedPercentage = tw.TargetAchievedPercentage ?? 0
                                            }).AsNoTracking().ToList();

                            foreach (var ws in targetws)
                            {
                                var sales = (from se in context.SalesExecutiveWorkshops
                                             join u in context.UserDetails on se.UserId equals u.UserId
                                             where se.WorkshopId == ws.WorkShopId
                                             select u).AsNoTracking().FirstOrDefault();
                                ws.SalesExecutive = string.Join(sales?.FirstName, sales?.LastName);
                            }
                            sp.TargetWorkshops.AddRange(targetws);
                        }
                    }
                }

                #endregion

                #region Reward Information

                sp.TargetOverviews = await GetTargetOverviewAsync(schemeId).ConfigureAwait(true);

                if (scheme.Type.Equals(Cashback, StringComparison.OrdinalIgnoreCase))
                {
                    sp.ShowCashback = true;
                }
                else if (scheme.Type.Equals(LuckyDraw, StringComparison.OrdinalIgnoreCase))
                {
                    sp.ShowCashback = scheme.CashBack ?? false;
                    sp.ShowAssuredGift = scheme.AssuredGift ?? false;
                    sp.ShowLuckyDraw = scheme.LuckyDraw ?? false;
                    sp.ShowCouponAllocation = scheme.LuckyDraw ?? false;
                }

                if (sp.ShowCashback)
                {
                    sp.CashbackCriteria = scheme.CashbackCriteria ?? notAvailable;
                    sp.AreBothCbAgApplicable = scheme.AreBothCbAgApplicable ?? false;

                    var cashbackRanges = context.CashbackRanges.Where(c => c.SchemeId == schemeId).AsNoTracking().ToList();
                    var cashbacks = GetCashBack(schemeId);
                    sp.Cashbacks = cashbacks;

                    // Set headers as per cashback's benifit range
                    var cbBenefit = cashbacks.FirstOrDefault()?.Benifit;
                    if (!string.IsNullOrWhiteSpace(cbBenefit))
                    {
                        var jss = new JavaScriptSerializer();
                        var cashbackBenefits = jss.Deserialize<List<CashbackBenefit>>(cbBenefit);
                        sp.CbrColText = cashbackBenefits.Select(cbb => $"{cbb.FromAmount} % to {cbb.ToAmount} %").ToList();
                    }
                }

                if (sp.ShowAssuredGift)
                {
                    sp.AreBothAgLdApplicable = scheme.AreBothAgLdApplicable;

                    var assuredGifts = GetAssuredGift(schemeId);
                    foreach (var ag in assuredGifts)
                    {
                        var agCategories = ag.ListAssuredGiftCategory.Select(a => a.CategoryId);
                        ag.Categories = ag.ListAssuredGiftCategory.Any(l => l.IsAll) ? "All" : string.Join(", ", sp.Categories.Where(c => agCategories.Contains(c.CategoryId)).Select(c => c.Category));
                    }
                    sp.AssuredGifts = assuredGifts;
                }

                if (sp.ShowLuckyDraw)
                {
                    sp.MaxGiftsAllowed = scheme.MaxGiftsAllowed != null ? scheme.MaxGiftsAllowed.ToString() : notAvailable;
                    sp.CanTakeMoreThanOneGift = scheme.CanTakeMoreThanOneGift ?? false;

                    var luckyDraws = GetGiftManagement(schemeId);
                    foreach (var ld in luckyDraws)
                    {
                        var ldCategories = ld.ListGiftCategory.Select(l => l.CategoryId);
                        ld.Categories = ld.ListGiftCategory.Any(l => l.IsAll) ? "All" : string.Join(", ", sp.Categories.Where(c => ldCategories.Contains(c.CategoryId)).Select(c => c.Category));
                    }
                    sp.LuckyDraws = luckyDraws;
                }

                if (sp.ShowCouponAllocation)
                {
                    var qualifyCriterias = GetQualifyCriteria(schemeId);
                    var productIds = qualifyCriterias.Where(q => !string.IsNullOrEmpty(q.TypeValue))?.Select(q => Convert.ToInt32(q.TypeValue)).ToList();
                    var products = context.Products.Where(p => productIds.Contains(p.ProductId)).Select(p => new
                    {
                        p.ProductId,
                        p.ProductName
                    }).ToList();

                    foreach (var qc in qualifyCriterias)
                    {
                        var productId = string.IsNullOrEmpty(qc.TypeValue) ? 0 : Convert.ToInt32(qc.TypeValue);
                        sp.CouponAllocations.Add(new SchemePreviewCouponAllocation
                        {
                            Amount = qc.AmountUpto,
                            Type = qc.Type,
                            PartsOrAccessories = products.FirstOrDefault(p => p.ProductId == productId)?.ProductName,
                            NumberOfCoupons = qc.NumberOfCoupons,
                            AdditionalCouponAmount = qc.AdditionalCouponAmount,
                            AdditionalNumberOfCoupons = qc.AdditionalNumberOfCoupons
                        });
                    }
                }

                #endregion

                return sp;
            }
        }

        /// <summary>
        /// Save target achieved for target workshops.
        /// </summary>        
        public bool SaveTargetAchievedForWorkshop(int schemeId)
        {
            using (var context = new garaazEntities())
            {
                var scheme = context.Schemes.Where(s => s.SchemeId == schemeId).AsNoTracking().FirstOrDefault();
                if (scheme == null) { return false; }

                var targetWorkshops = context.TargetWorkShops.Where(w => w.SchemeId == schemeId).ToList();

                var twsIds = targetWorkshops.Select(w => w.WorkShopId.Value).ToList();

                var sales = GetTargetWorkshopsSales(scheme.SchemeId, twsIds, scheme.PartType, true);

                foreach (var ws in targetWorkshops)
                {
                    decimal targetAchieved = 0.0M, achievedPercent = 0.0M;
                    var target = Convert.ToDecimal(ws.NewTarget);
                    if (target > 0)
                    {
                        targetAchieved = sales.Where(s => s.WorkShopId == ws.WorkShopId).Sum(s => Convert.ToDecimal(s.NetRetailSelling));
                        achievedPercent = targetAchieved * 100 / target;
                    }
                    ws.TargetAchieved = targetAchieved;
                    ws.TargetAchievedPercentage = Math.Round(achievedPercent, 2);
                }
                context.SaveChanges();
                return true;
            }
        }
        #endregion

        #region Save groups by FMS

        public async Task<bool> SaveGroupsByFmsAsync(FmsPartsGroup fmsPg)
        {
            // Delete existing records per scheme Id
            await db.Database.ExecuteSqlCommandAsync("DELETE FROM FmsGroupSale WHERE SchemeId={0}", fmsPg.SchemeId).ConfigureAwait(false);

            // Retrieve groups by part category
            IEnumerable<int> groupIds = null;
            if (!string.IsNullOrEmpty(fmsPg.PartCategory))
            {
                var prodGroups = GetGroupsByPartCategory(fmsPg.PartCategory, fmsPg.DistributorId);
                groupIds = prodGroups.Select(pg => pg.GroupId);
            }
            if (groupIds == null) return false;

            // Sort groups by descending order
            var dailySales = await db.DailySalesTrackerWithInvoiceDatas.Where(s => s.DistributorId == fmsPg.DistributorId && s.GroupId != null && s.CreatedDate >= fmsPg.StartDate && s.CreatedDate <= fmsPg.EndDate).AsNoTracking().ToListAsync().ConfigureAwait(false);

            var groups = dailySales.Where(s => groupIds.Contains(Convert.ToInt32(s.GroupId))).GroupBy(s => s.GroupId).Select(g => new { GroupId = g.Key, Total = g.Sum(s => Convert.ToDecimal(s.NetRetailSelling)) }).OrderByDescending(o => o.Total).ToList();
            if (groups.Count <= 0) return false;

            // Categorize groups in F M S
            decimal totalGroups = groups.Count;

            var fValue = Convert.ToDecimal(fmsPg.FValue);
            var numberOfGroupsToMarkAsF = (int)Math.Round(totalGroups * fValue / 100, MidpointRounding.AwayFromZero);

            var mValue = Convert.ToDecimal(fmsPg.MValue);
            var numberOfGroupsToMarkAsM = (int)Math.Round(totalGroups * mValue / 100, MidpointRounding.AwayFromZero);

            var sValue = Convert.ToDecimal(fmsPg.SValue);
            var numberOfGroupsToMarkAsS = (int)Math.Round(totalGroups * sValue / 100, MidpointRounding.AwayFromZero);

            var fGroups = groups.Take(numberOfGroupsToMarkAsF).ToList();
            var mGroups = groups.Skip(numberOfGroupsToMarkAsF).Take(numberOfGroupsToMarkAsM).ToList();
            var sGroups = groups.Skip(numberOfGroupsToMarkAsF + numberOfGroupsToMarkAsM).Take(numberOfGroupsToMarkAsS).ToList();

            // Save groups and their sale's total
            var saveChanges = false;
            if (fGroups.Count > 0)
            {
                var fmsGroupSales = fGroups.Select(fg => new FmsGroupSale
                {
                    SchemeId = fmsPg.SchemeId,
                    GroupId = fg.GroupId ?? 0,
                    PartCreation = "F",
                    TotalSale = fg.Total
                });
                db.FmsGroupSales.AddRange(fmsGroupSales);
                saveChanges = true;
            }

            if (mGroups.Count > 0)
            {
                var fmsGroupSales = mGroups.Select(fg => new FmsGroupSale
                {
                    SchemeId = fmsPg.SchemeId,
                    GroupId = fg.GroupId ?? 0,
                    PartCreation = "M",
                    TotalSale = fg.Total
                });
                db.FmsGroupSales.AddRange(fmsGroupSales);
                saveChanges = true;
            }

            if (sGroups.Count > 0)
            {
                var fmsGroupSales = sGroups.Select(fg => new FmsGroupSale
                {
                    SchemeId = fmsPg.SchemeId,
                    GroupId = fg.GroupId ?? 0,
                    PartCreation = "S",
                    TotalSale = fg.Total
                });
                db.FmsGroupSales.AddRange(fmsGroupSales);
                saveChanges = true;
            }

            if (saveChanges)
            {
                var rowsAffected = await db.SaveChangesAsync().ConfigureAwait(false);
                return rowsAffected > 0;
            }

            return false;
        }

        #endregion

        private List<ProductGroupModel> GetGroupsByPartCategory(string partCategory, int? distributorId)
        {
            var groups = new List<ProductGroupModel>();
            if (string.IsNullOrEmpty(partCategory)) return groups;

            if (partCategory.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                groups = db.ProductGroups.Select(pg => new ProductGroupModel
                {
                    GroupId = pg.GroupId,
                    GroupName = pg.GroupName
                }).AsNoTracking().ToList();
            }
            else
            {
                var partCategoryCodes = partCategory.Split(',').ToList();

                if (partCategory.Contains("M"))
                {
                    partCategoryCodes.Add(Constants.MFull);
                }

                if (partCategory.Contains("AA"))
                {
                    partCategoryCodes.Add(Constants.AAFull);
                }

                if (partCategory.Contains("AG"))
                {
                    partCategoryCodes.Add(Constants.AGFull);
                }
                if (partCategory.Contains("T"))
                {
                    partCategoryCodes.Add(Constants.TFull);
                }

                groups = (from g in db.ProductGroups.AsNoTracking()
                          join p in db.Products.AsNoTracking() on g.GroupId equals p.GroupId
                          where p.DistributorId == distributorId && p.PartCategoryCode != null && p.ProductType != null && partCategoryCodes.Contains(p.PartCategoryCode)
                          select g).Distinct().Select(pg => new ProductGroupModel
                          {
                              GroupId = pg.GroupId,
                              GroupName = pg.GroupName
                          }).ToList();
            }

            return groups;
        }

        #region Get Scheme Achived Gift By WorkshopId
        public List<ResponseWorkshopScheme> GetSchemesGiftAchievedByWorkshopId(GetWorkshopSchemeModel model)
        {
            var respWsSchemes = new List<ResponseWorkshopScheme>();
            int workshopId = model.UserId.GetWorkshopId(Constants.Workshop);

            var schemes = (from s in db.Schemes
                           join tw in db.TargetWorkShops on s.SchemeId equals tw.SchemeId
                           where s.AssuredGift == true && s.LuckyDraw == true && s.IsActive == true && s.IsDeleted != true && tw.WorkShopId == workshopId
                           select s).ToList();
            if (model.SchemeId != null)
            {
                schemes = schemes.Where(s => s.SchemeId == model.SchemeId).ToList();
            }

            foreach (var s in schemes)
            {
                // Check if already exist
                var st = db.WorkshopSchemesSelectedTypes.FirstOrDefault(o => o.WorkshopId == workshopId && o.SchemeId == s.SchemeId);
                if (st != null) continue;

                // Workshop target
                var targetWs = db.TargetWorkShops.FirstOrDefault(w => w.WorkShopId == workshopId && w.SchemeId == s.SchemeId);
                if (targetWs == null) continue;
                var workshopTarget = Convert.ToDecimal(targetWs.NewTarget);

                // Workshop Sale
                var sale = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.WorkShopId == workshopId && DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(s.StartDate) && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(s.EndDate)).ToList().Sum(x => Convert.ToDecimal(x.NetRetailSelling));

                // Check target achieved
                if (sale >= workshopTarget)
                {
                    var data = new ResponseWorkshopScheme
                    {
                        SchemeId = s.SchemeId,
                        SchemeName = s.SchemeName,
                        AssuredGift = Convert.ToBoolean(s.AssuredGift),
                        LuckyDraw = Convert.ToBoolean(s.LuckyDraw),
                        CashBack = Convert.ToBoolean(s.CashBack)
                    };
                    respWsSchemes.Add(data);
                }

            }

            return respWsSchemes;
        }

        /// <summary>
        /// Decide whether to allow user selecting Cashback or LuckyDraw choice.
        /// </summary>
        public bool ShowCashbackAndLuckyDraw(GetWorkshopSchemeModel model)
        {
            const string schemeType = "Cashback";
            const string luckyDraw = "Lucky Draw";
            const string assuredGifts = "Assured gifts";
            const string cashBack = "Cashback";

            int workshopId = model.UserId.GetWorkshopId(Constants.Workshop);

            var scheme = (from s in db.Schemes.AsNoTracking()
                          join tw in db.TargetWorkShops.AsNoTracking() on s.SchemeId equals tw.SchemeId
                          where s.SchemeId == model.SchemeId && s.Type != schemeType && s.IsActive == true && s.IsDeleted != true && tw.WorkShopId == workshopId
                          select new
                          {
                              s.AssuredGift,
                              s.LuckyDraw,
                              s.CashBack,
                              tw.NewTarget,
                              tw.TargetAchieved
                          }).FirstOrDefault();

            if (scheme == null) return false;

            // Check if already exist
            var st = db.WorkshopSchemesSelectedTypes.FirstOrDefault(o => o.WorkshopId == workshopId && o.SchemeId == model.SchemeId.Value);
            if (st != null) return false;

            if (scheme.TargetAchieved >= Convert.ToDecimal(scheme.NewTarget))
            {
                var isAssuredGift = Convert.ToBoolean(scheme.AssuredGift);
                var isLuckyDraw = Convert.ToBoolean(scheme.LuckyDraw);
                var isCashback = Convert.ToBoolean(scheme.CashBack);

                if (isAssuredGift && isLuckyDraw || isAssuredGift && isLuckyDraw && isCashback)
                {
                    return true;
                }
                else if (isCashback && isAssuredGift || isCashback && isLuckyDraw)
                {
                    var isSaved = SaveWorkshopSchemeSelectedType(new WorkshopSchemeSelectType
                    {
                        UserId = model.UserId,
                        SchemeId = model.SchemeId.Value,
                        SelectedOption = isLuckyDraw ? luckyDraw : assuredGifts
                    });
                    return false;
                }
                else
                {
                    var saveOption = "";
                    if (isLuckyDraw)
                    {
                        saveOption = luckyDraw;
                    }
                    else if (isAssuredGift)
                    {
                        saveOption = assuredGifts;
                    }
                    else if (isCashback)
                    {
                        saveOption = cashBack;
                    }
                    var isSaved = SaveWorkshopSchemeSelectedType(new WorkshopSchemeSelectType
                    {
                        UserId = model.UserId,
                        SchemeId = model.SchemeId.Value,
                        SelectedOption = saveOption
                    });
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region Save Workshop Selected Gift
        public bool SaveWorkshopSchemeSelectedType(WorkshopSchemeSelectType model)
        {
            int workshopId = model.UserId.GetWorkshopId(Constants.Workshop);
            var oldData = db.WorkshopSchemesSelectedTypes.FirstOrDefault(a => a.WorkshopId == workshopId && a.SchemeId == model.SchemeId);
            if (oldData != null)
            {
                oldData.SelectedOption = model.SelectedOption;
                oldData.CreatedDate = DateTime.Now;
            }
            else
            {
                var newData = new WorkshopSchemesSelectedType()
                {
                    WorkshopId = workshopId,
                    UserId = model.UserId,
                    SchemeId = model.SchemeId,
                    SelectedOption = model.SelectedOption,
                    CreatedDate = DateTime.Now
                };
                db.WorkshopSchemesSelectedTypes.Add(newData);
            }
            return db.SaveChanges() > 0;
        }
        #endregion

        #region Export to exel Target Workshops
        public List<ResponseTargetWorkshopExport> GetTargetWorkshopsBySchemeId(int schemeId)
        {
            var targetws = (from tw in db.TargetWorkShops
                            join w in db.WorkShops on tw.WorkShopId equals w.WorkShopId
                            where tw.SchemeId == schemeId
                            select new ResponseTargetWorkshopExport
                            {
                                SrNo = w.WorkShopId,
                                LocationCode = w.Outlet != null ? w.Outlet.OutletCode : "",
                                RoIncharge = w.Outlet != null ? w.Outlet.OutletName : "",
                                WorkShopName = w.WorkShopName,
                                CustomerType = tw.CustomerType,
                                PrevYearAvgSale = tw.PrevYearAvgSale,
                                GrowthPercentage = tw.GrowthPercentage,
                                NewTarget = tw.NewTarget,
                                PrevMonthAvgSale = tw.PrevMonthAvgSale,
                                GrowthComparisonPercentage = tw.GrowthComparisonPercentage,
                                IsQualified = tw.IsQualifiedAsDefault ?? 0 == 1 ? "Yes" : "No",
                                TargetAchieved = tw.TargetAchieved ?? 0,
                                AchievedPercentage = tw.TargetAchievedPercentage ?? 0
                            }).AsNoTracking().ToList();
            int index = 1;
            foreach (var ws in targetws)
            {
                var sales = (from se in db.SalesExecutiveWorkshops
                             join u in db.UserDetails on se.UserId equals u.UserId
                             where se.WorkshopId == ws.SrNo
                             select u).AsNoTracking().FirstOrDefault();
                ws.SalesExecutive = string.Join(sales?.FirstName, sales?.LastName);
                ws.SrNo = index++;
            }
            return targetws;
        }
        #endregion

    }
}