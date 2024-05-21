using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoLabels
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region GetLabelCriteriaByLabelId
        public List<LabelCriteria> GetLabelCriteriaBySchemesId(int SchemesId)
        {
            return db.LabelCriterias.Where(x => x.SchemeId == SchemesId).ToList();
        }
        #endregion

        #region Add/Update Labels
        public string SaveLabel(List<LabelCriteria> model)
        {
            var criteriaId = string.Empty;
            if (model == null) return criteriaId;

            foreach (var item in model)
            {
                if (item.CriteriaId > 0)
                {
                    var lc = db.LabelCriterias.Where(x => x.CriteriaId == item.CriteriaId).FirstOrDefault();
                    if (lc != null)
                    {
                        lc.Condition = item.Condition;
                        lc.GroupId = item.GroupId;
                        lc.ProductId = item.ProductId;
                        lc.SaleAmount = item.SaleAmount;
                        lc.SaleCondition = item.SaleCondition;
                        lc.TypeOfCriteria = item.TypeOfCriteria;
                        lc.Value = item.Value;
                        lc.Qty = item.Qty;
                        lc.Operator = item.Operator;
                    }
                    db.SaveChanges();
                    criteriaId += $"{item.CriteriaId},";
                }
                else
                {
                    // Only save if any of value is not null
                    if (item.GroupId != null || item.ProductId != null || !string.IsNullOrEmpty(item.Condition) || !string.IsNullOrEmpty(item.Value) || item.Qty != null || !string.IsNullOrEmpty(item.Operator))
                    {
                        db.LabelCriterias.Add(item);
                        db.SaveChanges();
                        criteriaId += $"{item.CriteriaId},";
                    }
                }
            }
            return criteriaId;
        }

        #endregion

        #region Get Workshops      

        public List<WorkshopModelScheme> GetWorkshopsByLabelCriteria(List<CriteriaWorkShopModel> listCwsm, Scheme scheme)
        {
            var workshops = new List<WorkshopModelScheme>();
            var defaultWs = new List<WorkshopModelScheme>();
            var filteredWs = new List<WorkshopModelScheme>();
            bool firstOpIgnore = false;

            foreach (var cwsModel in listCwsm)
            {
                if (cwsModel.DistributorId > 0)
                {
                    var ru = new RepoUsers();
                    if (scheme != null)
                    {
                        if (scheme.SalesExecutiveId == null || (scheme.IsAllSalesExecutiveSelected != null && Convert.ToBoolean(scheme.IsAllSalesExecutiveSelected)))
                        {
                            workshops = ru.GetWorkshopByDistIdNew(cwsModel.DistributorId);
                        }
                        else
                        {
                            workshops = ru.GetWorkShopsForSalesExecutive(scheme.SalesExecutiveId);
                        }
                    }
                    else if (!string.IsNullOrEmpty(cwsModel.SalesExecutiveId))
                    {
                        workshops = cwsModel.SalesExecutiveId == "All" ? ru.GetWorkshopByDistIdNew(cwsModel.DistributorId) : ru.GetWorkShopsForSalesExecutive(cwsModel.SalesExecutiveId);
                    }
                    else
                    {
                        workshops = ru.GetWorkshopByDistIdNew(cwsModel.DistributorId);
                    }
                }

                if (workshops.Count == 0) continue;

                if (firstOpIgnore)
                {
                    filteredWs = GetWorkShops(cwsModel, workshops); // 50

                    switch (cwsModel.Operator)
                    {
                        case "OR":
                            // Compare two lists and get not matching one
                            var wsIDs = new HashSet<int>(defaultWs.Select(s => s.WorkShopId));
                            var results = filteredWs.Where(f => !wsIDs.Contains(f.WorkShopId)).ToList();

                            // Add unique items to existing list
                            defaultWs.AddRange(results);
                            break;

                        case "AND":
                            // Compare two lists and get not matching one
                            wsIDs = new HashSet<int>(defaultWs.Select(s => s.WorkShopId));
                            results = filteredWs.Where(f => !wsIDs.Contains(f.WorkShopId)).ToList();

                            // Assign unique results to default ws
                            defaultWs = results;
                            break;
                    }

                    workshops = defaultWs;
                }
                else
                {
                    defaultWs = GetWorkShops(cwsModel, workshops); // 100
                    firstOpIgnore = true;
                    workshops = defaultWs;
                }
            }

            return workshops;
        }

        private List<WorkshopModelScheme> GetWorkShops(CriteriaWorkShopModel cwsModel, List<WorkshopModelScheme> workshops)
        {
            var wsList = workshops;

            if (cwsModel != null && !string.IsNullOrEmpty(cwsModel.Condition))
            {
                wsList = new List<WorkshopModelScheme>();
                // Get sales based on GroupId or ProductId
                var dailysalesList = db.DailySalesTrackerWithInvoiceDatas.Where(x => x.GroupId == (cwsModel.GroupId == null ? x.GroupId : cwsModel.GroupId)
                || x.ProductId == (cwsModel.ProductId == null ? x.ProductId : cwsModel.ProductId));

                var listTgWs = new List<TargetgroupWorkshop>();
                foreach (var item in workshops)
                {
                    var dailySales = dailysalesList.Where(x => x.WorkShopId == item.WorkShopId).ToList();
                    listTgWs.Add(new TargetgroupWorkshop()
                    {
                        workshopId = item.WorkShopId,
                        Total = dailySales.Count > 0 ? dailySales.Sum(s => Convert.ToDecimal(s.NetRetailSelling)) : 0,
                        Qty = dailySales.Count > 0 ? dailySales.Where(q => q.GroupId != null && q.ProductId != null).GroupBy(qg => new { qg.GroupId, qg.ProductId }).ToList().Count() : 0
                    });
                }

                if (cwsModel.Condition == "Equal")
                    listTgWs = listTgWs.Where(d => (!string.IsNullOrEmpty(cwsModel.Value) ? (d.Total == Convert.ToDecimal(cwsModel.Value)) : (cwsModel.Qty != null ? d.Qty == cwsModel.Qty : d.Qty == d.Qty))).ToList();
                else if (cwsModel.Condition == "Greaterthan")
                    listTgWs = listTgWs.Where(d => (!string.IsNullOrEmpty(cwsModel.Value) ? (d.Total > Convert.ToDecimal(cwsModel.Value)) : (cwsModel.Qty != null ? d.Qty > cwsModel.Qty : d.Qty == d.Qty))).ToList();
                else if (cwsModel.Condition == "lower than")
                    listTgWs = listTgWs.Where(d => (!string.IsNullOrEmpty(cwsModel.Value) ? (d.Total < Convert.ToDecimal(cwsModel.Value)) : (cwsModel.Qty != null ? d.Qty < cwsModel.Qty : d.Qty == d.Qty))).ToList();

                if (listTgWs.Count > 0)
                {
                    var workshopIdArr = listTgWs.Select(s => s.workshopId).ToList();
                    wsList = workshops.Where(x => workshopIdArr.Contains(x.WorkShopId)).ToList();
                }
            }

            return wsList;
        }
        #endregion

        #region Get WorkShop Saved
        public List<WorkshopModelScheme> getWorkShopSaved(int SchemesId)
        {
            return db.TargetWorkShops.Where(a => a.SchemeId == SchemesId).Select(a => new WorkshopModelScheme() { WorkShopId = a.WorkShopId.Value }).ToList();
        }
        #endregion

        #region Get WorkShop Saved New
        public List<ResponseTargetWorkshopModel> getWorkShopSavedNew(int SchemesId)
        {
            return (from t in db.WorkShopLabelSchemes
                    join d in db.DistributorWorkShops on t.WorkShopId equals d.WorkShopId
                    join u in db.UserDetails on d.UserId equals u.UserId
                    where t.SchemeId == SchemesId
                    select new ResponseTargetWorkshopModel()
                    {
                        //Growth = t.Growth,
                        IsQualifiedAsDefault = false,
                        Max = 0,
                        Min = 0,
                        SchemeId = t.SchemeId,
                        //TagetWorkShopId = t.TagetWorkShopId,
                        //Target = t.Target,
                        //TargetWithoutGrowth = t.TargetWithoutGrowth,
                        WorkShopCode = u.ConsPartyCode,
                        WorkShopId = t.WorkShopId,
                        WorkShopName = t.WorkShop.WorkShopName
                    }).ToList();

        }
        #endregion

        #region Get WorkShop Saved Already
        public List<ResponseTargetWorkshopModel> GetSavedTargetWorkshops(int schemeId)
        {
            var savedWorkshops = new List<ResponseTargetWorkshopModel>();

            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var targetWs = context.TargetWorkShops.Where(w => w.SchemeId == schemeId).Include(w => w.WorkShop).AsNoTracking().ToList();
                var targetWsIds = targetWs.Select(t => t.WorkShopId);

                var users = (from d in context.DistributorWorkShops.AsNoTracking()
                             join u in context.UserDetails.AsNoTracking() on d.UserId equals u.UserId
                             where targetWsIds.Contains(d.WorkShopId)
                             select new
                             {
                                 d.WorkShopId,
                                 u.ConsPartyCode
                             }).ToList();

                foreach (var tw in targetWs)
                {
                    var user = users.FirstOrDefault(u => u.WorkShopId == tw.WorkShopId);

                    savedWorkshops.Add(new ResponseTargetWorkshopModel
                    {
                        TargetWorkShopId = tw.TargetWorkShopId,
                        WorkShopCode = user != null ? user.ConsPartyCode : "",
                        WorkShopId = tw.WorkShopId.Value,
                        WorkShopName = tw.WorkShop.WorkShopName,
                        SchemeId = tw.SchemeId.Value,
                        Max = 0,
                        Min = 0,
                        CustomerType = tw.CustomerType,
                        PrevYearAvgSale = tw.PrevYearAvgSale,
                        GrowthPercentage = tw.GrowthPercentage,
                        NewTarget = tw.NewTarget,
                        PrevMonthAvgSale = tw.PrevMonthAvgSale,
                        GrowthComparisonPercentage = tw.GrowthComparisonPercentage,
                        IsQualifiedAsDefault = tw.IsQualifiedAsDefault.Value
                    });
                }
            }

            return savedWorkshops;
        }
        #endregion

        #region Save WorkShop Label
        public bool saveWorkShopLabels(List<WorkShopLabelScheme> data)
        {
            data = data == null ? new List<WorkShopLabelScheme>() : data;
            var schemeId = data.Any() ? data.FirstOrDefault().SchemeId : 0;

            var objLabel = db.WorkShopLabelSchemes.Where(x => x.SchemeId == schemeId).ToList();
            if (objLabel.Count > 0)
            {
                db.WorkShopLabelSchemes.RemoveRange(objLabel);
                db.SaveChanges();
            }
            if (data.Any())
            {
                db.WorkShopLabelSchemes.AddRange(data);
                db.SaveChanges();
            }
            return true;
        }
        #endregion

        #region Save WorkShop Label
        public bool saveWorkShopOnTarget(List<WorkShopLabelScheme> data)
        {
            //1,3
            data = data == null ? new List<WorkShopLabelScheme>() : data;
            var schemeId = data.Any() ? data.FirstOrDefault().SchemeId : 0;

            if (schemeId > 0)
            {
                // Get workshop id
                var WorkshopsIDs = data.Select(a => a.WorkShopId).ToList();

                //Get Data which we need to remove
                var remeveData = (from a in db.TargetWorkShops
                                  where a.SchemeId == schemeId && !WorkshopsIDs.Contains(a.WorkShopId.Value)
                                  select a
                                  ).ToList(); // 2

                if (remeveData.Count > 0)
                {
                    db.TargetWorkShops.RemoveRange(remeveData);
                    db.SaveChanges();
                }


                var Ids = db.TargetWorkShops.Where(a => a.SchemeId == schemeId).Select(a => a.WorkShopId).ToList(); //1,2

                var addData = (from a in data
                               where !Ids.Contains(a.WorkShopId)
                               select a
                                  ).ToList(); // 3


                var workshopDatas = new List<TargetWorkShop>();

                if (addData.Count > 0)
                {
                    foreach (var item in addData)
                    {
                        TargetWorkShop w = new TargetWorkShop();
                        w.WorkShopId = item.WorkShopId;
                        w.SchemeId = schemeId;
                        w.IsQualifiedAsDefault = false;
                        workshopDatas.Add(w);
                    }
                    db.TargetWorkShops.AddRange(workshopDatas);
                    db.SaveChanges();
                }
            }

            return true;
        }
        #endregion

        #region Get Condition SelectList
        public List<SelectListItem> GetConditionSelect()
        {
            var lst = new List<SelectListItem>
            {
                new SelectListItem() {Text = "Equal",Value = "Equal"},
                new SelectListItem() {Text = "Greater than",Value = "Greaterthan"},
                new SelectListItem() {Text = "Less than",Value = "lower than"}
            };

            return lst;
        }
        #endregion

        #region Get Operator SelectList
        public List<SelectListItem> GetOperatorSelect()
        {
            var lst = new List<SelectListItem>
            {
                new SelectListItem() {Text = "AND",Value = "AND"},
                new SelectListItem() {Text = "OR",Value = "OR"}
            };

            return lst;
        }
        #endregion
    }
}