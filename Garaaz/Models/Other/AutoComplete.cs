using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models.Other
{
    #region Response

    public class AutoCompleteModel
    {
        [Required(ErrorMessage = "Search Text required")]
        public string SearchText { get; set; }
    }

    public class AutoCompleteWorkshopCode
    {
        public string WorkshopCode { get; set; }
    }

    public class LocationCodeModel
    {
        public string label { get; set; }
        public string value { get; set; }
        public string key { get; set; }
    }

    #endregion

    public class AutoComplete
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General general = new General();
        #endregion

        #region Workshop Code AutoComplete
        public List<AutoCompleteWorkshopCode> GetAutoCompleteWorkshopCode(string searchText, string userId, string role)
        {
            int? distributorId = null;
            distributorId = userId.GetDistributorId(role);
            var data = new List<AutoCompleteWorkshopCode>();
            if (role==Constants.SuperAdmin)
            {
                data = (from a in db.UserDetails
                        join dw in db.DistributorWorkShops.AsNoTracking() on a.UserId equals dw.UserId
                        join w in db.WorkShops.AsNoTracking() on dw.WorkShopId equals w.WorkShopId
                        where a.IsDeleted.Value == false
                        && a.AspNetUser.AspNetRoles.Any(r => r.Name == Constants.Workshop)
                        && (a.ConsPartyCode.StartsWith(searchText)|| w.WorkShopName.StartsWith(searchText))
                        select new AutoCompleteWorkshopCode() { WorkshopCode = a.ConsPartyCode }).Take(20).ToList();
            }
            else
            {
                data = (from a in db.UserDetails
                        join dw in db.DistributorWorkShops on a.UserId equals dw.UserId
                        join w in db.WorkShops.AsNoTracking() on dw.WorkShopId equals w.WorkShopId
                        where a.IsDeleted.Value == false
                        && a.AspNetUser.AspNetRoles.Any(r => r.Name == Constants.Workshop)
                        && dw.DistributorId== distributorId && (a.ConsPartyCode.StartsWith(searchText) || w.WorkShopName.StartsWith(searchText))
                        select new AutoCompleteWorkshopCode() { WorkshopCode = a.ConsPartyCode }).Take(20).ToList();
            }
            return data;
        }
        #endregion

        #region Location Code Access By Role
        public List<LocationCodeModel> GetLocationCodeByRole(string userId, string role)
        {
            int? distributorId = null;
            distributorId = userId.GetDistributorId(role);
            var data = new List<LocationCodeModel>();
            if (role == Constants.SuperAdmin)
            {
                data = (from o in db.Outlets.AsNoTracking().OrderBy(o => o.OutletCode)
                        where o.OutletCode!=null && !o.OutletCode.Equals(string.Empty)
                        select new LocationCodeModel(){label = o.OutletCode.ToUpper(), value = o.OutletCode,key= o.OutletCode, }).ToList();
            }
            else
            {
                data = (from o in db.Outlets.AsNoTracking().OrderBy(o => o.OutletCode)
                        join od in db.DistributorsOutlets.AsNoTracking() on o.OutletId equals od.OutletId
                        where od.DistributorId == distributorId
                        && o.OutletCode != null && !o.OutletCode.Equals(string.Empty)
                        select new LocationCodeModel() { label = o.OutletCode.ToUpper(), value = o.OutletCode, key = o.OutletCode, }).ToList();
            }
            return data;
        }
        #endregion
    }
}