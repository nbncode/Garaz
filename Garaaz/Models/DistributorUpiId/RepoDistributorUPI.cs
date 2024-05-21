﻿using System.Linq;

namespace Garaaz.Models.DistributorUpi
{
    public class UpiDetailsModel
    {
        public string UPIID { get; set; }
        public string DistributorName { get; set; }
        public string OutletCode { get; set; }
        public string WorkshopCode { get; set; }
    }

    public class RepoDistributorUpi
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region Get Distributor Upi Details using UserId
        public UpiDetailsModel GetDistributorUPIDetails(string userId)
        {
            var upiId = (from dw in db.DistributorWorkShops.AsNoTracking()
                         join d in db.Distributors.AsNoTracking() on dw.DistributorId equals d.DistributorId
                         join w in db.WorkShops on dw.WorkShopId equals w.WorkShopId
                         join u in db.UserDetails.AsNoTracking() on dw.UserId equals u.UserId
                         where dw.UserId == userId
                         select new UpiDetailsModel
                         {
                             UPIID = d.UPIID,
                             DistributorName=d.DistributorName,
                             OutletCode=w.Outlet!=null?w.Outlet.OutletCode:null,
                             WorkshopCode=u.ConsPartyCode
                         }).FirstOrDefault();

            return upiId;
        }
        #endregion
    }
}