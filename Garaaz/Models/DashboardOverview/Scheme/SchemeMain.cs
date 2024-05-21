using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Garaaz.Models.DashboardOverview.Scheme
{
    public class SchemeMain
    {
        public SchemeInfoResponse GetSchemesInfo(DashboardFilter dbFilter)
        {
            var date = DbCommon.GetDatePeriod(dbFilter);
            dbFilter.StartDate = date.StartDate;
            dbFilter.EndDate = date.EndDate;
            var dateInfo = date.DateInfo;

            decimal currentSale = 0, newTarget = 0;
            if (dbFilter.StartDate != null && dbFilter.EndDate != null)
            {
                var role = string.Empty;
                if (dbFilter.Roles.Contains(Constants.SuperAdmin))
                {
                    role = Constants.SuperAdmin;
                }
                if (dbFilter.Roles.Contains(Constants.Distributor))
                {
                    role = Constants.Distributor;
                }
                if (dbFilter.Roles.Contains(Constants.RoIncharge))
                {
                    role = Constants.RoIncharge;
                }
                if (dbFilter.Roles.Contains(Constants.SalesExecutive))
                {
                    role = Constants.SalesExecutive;
                }

                var sqlParams = new[]{
                    new SqlParameter("@Role",role),
                    new SqlParameter("@UserId",dbFilter.UserId),
                    new SqlParameter("@StartDate",dbFilter.StartDate),
                    new SqlParameter("@EndDate",dbFilter.EndDate),
                    new SqlParameter("@CoDealerDist",Constants.CoDealerDistributor)
                };
                var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_Dashboard_TargetWorkshops", sqlParams);
                if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0) return null;

                var dataRow = ds.Tables[0].Rows[0];
                currentSale = dataRow.Field<decimal?>("CurrentSale") ?? 0;
                newTarget = dataRow.Field<decimal?>("NewTarget") ?? 0;
            }

            #region Comment entity old code
            //using (var db = new garaazEntities())
            //{
            //    var schemes = db.Schemes.Where(s =>
            //        s.StartDate != null && s.EndDate != null &&
            //        (s.StartDate >= dbFilter.StartDate && s.StartDate <= dbFilter.EndDate ||
            //         s.EndDate >= dbFilter.StartDate && s.EndDate <= dbFilter.EndDate)).ToList();

            //    if (schemes.Count > 0)
            //    {
            //        if (dbFilter.Roles.Contains(Constants.SuperAdmin))
            //        {
            //            var schemeIds = schemes.Select(s => s.SchemeId).ToList();
            //            targetWorkshops = db.TargetWorkShops.Where(w =>
            //                !string.IsNullOrEmpty(w.NewTarget) && schemeIds.Contains(w.SchemeId.Value)).ToList();
            //        }
            //        else if (dbFilter.Roles.Contains(Constants.Distributor))
            //        {
            //            var distId = _general.getDistributorId(dbFilter.UserId);
            //            schemes = schemes.Where(s => s.DistributorId == distId).ToList();
            //            if (schemes.Count > 0)
            //            {
            //                var schemeIds = schemes.Select(s => s.SchemeId).ToList();
            //                var dWsIds = db.DistributorWorkShops.Where(w => w.DistributorId == distId)
            //                    .Select(w => w.WorkShopId)
            //                    .ToList();
            //                targetWorkshops = db.TargetWorkShops.Where(w =>
            //                    !string.IsNullOrEmpty(w.NewTarget) && schemeIds.Contains(w.SchemeId.Value) &&
            //                    dWsIds.Contains(w.WorkShopId.Value)).ToList();
            //            }
            //        }
            //        else if (dbFilter.Roles.Contains(Constants.RoIncharge))
            //        {
            //            var distId = dbFilter.UserId.GetDistributorId(Constants.RoIncharge);
            //            var distOutlet = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == dbFilter.UserId);
            //            if (distOutlet != null)
            //            {
            //                var workshops = (from w in db.WorkShops
            //                                 where w.outletId == distOutlet.OutletId
            //                                 select w).ToList();
            //                schemes = schemes.Where(s => s.DistributorId == distId).ToList();
            //                if (workshops.Count > 0 && schemes.Count > 0)
            //                {
            //                    var wsIds = workshops.Select(a => a.WorkShopId).ToList();
            //                    var schemeIds = schemes.Select(s => s.SchemeId).ToList();

            //                    targetWorkshops = db.TargetWorkShops.Where(w =>
            //                        !string.IsNullOrEmpty(w.NewTarget) && schemeIds.Contains(w.SchemeId.Value) &&
            //                        wsIds.Contains(w.WorkShopId.Value)).ToList();
            //                }
            //            }
            //        }
            //        else if (dbFilter.Roles.Contains(Constants.SalesExecutive))
            //        {
            //            var seWsIds = (from w in db.SalesExecutiveWorkshops
            //                           where w.UserId == dbFilter.UserId
            //                           select w.WorkshopId).Distinct().ToList();

            //            var schemeIds = schemes.Select(s => s.SchemeId).ToList();
            //            targetWorkshops = db.TargetWorkShops.Where(w =>
            //                !string.IsNullOrEmpty(w.NewTarget) && schemeIds.Contains(w.SchemeId.Value) &&
            //                seWsIds.Contains(w.WorkShopId.Value)).ToList();
            //        }
            //    }
            //}

            // Write the enum names separated by a space.
            //var str = string.Join(" ,", targetWorkshops.Select(w => w.WorkShopId).ToList());
            //var sale = DbCommon.GetTotalSale(dbFilter, str).CurrentSale;

            //// Calculate target achieved percentage
            //var target = targetWorkshops.Sum(w => Convert.ToDecimal(w.NewTarget));
            //var targetAchievedPer = target > 0 ? Math.Round(sale * 100 / target) : 0;

            //var saleTxt = sale <= 0 ? "0" : $"{Math.Round(sale):#,###}";
            //var targetTxt = target <= 0 ? "0" : $"{Math.Round(target):#,###}";

            #endregion

            // Calculate target achieved percentage
            var targetAchievedPer = newTarget > 0 ? Math.Round(currentSale * 100 / newTarget) : 0;

            // Prepare return response
            var saleTxt = currentSale > 0 ? $"{Math.Round(currentSale):#,###}" : "0";
            var targetTxt = newTarget > 0 ? $"{Math.Round(newTarget):#,###}" : "0";

            return new SchemeInfoResponse
            {
                DateInfo = dateInfo,
                TargetAchieved = saleTxt,
                TargetAchievedPercentage = targetAchievedPer,
                TotalTarget = $"Target : {targetTxt}"
            };
        }
    }
}