using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RegisterBrandRequest
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region Save Register Brand
        public bool SaveRegisterBrand(clsRegisterBrand model)
        {
            var  brandIds = model.BrandIds!= null ?  model.BrandIds.Split(',') : null;
            if (model.Role == Constants.Distributor)
            {
                var listDistBrand = new List<DistributorBrand>();
                var distributorBrand = db.DistributorBrands.Where(x => x.DistributorId == model.DistributorId).ToList();
                if (distributorBrand.Count > 0)
                {
                    db.DistributorBrands.RemoveRange(distributorBrand);
                }
                if(brandIds != null) { 
                    foreach (var brandId in brandIds)
                    {
                        listDistBrand.Add(new DistributorBrand()
                        {
                            DistributorId = model.DistributorId,
                            BrandId = Convert.ToInt32(brandId)
                        });
                    }
                }
                db.DistributorBrands.AddRange(listDistBrand);
                return db.SaveChanges() > 0;
            }
            else if (model.Role == Constants.Workshop)
            {
                var listWsBrand = new List<WorkshopBrand>();
                foreach (var brandId in brandIds)
                {
                    listWsBrand.Add(new WorkshopBrand()
                    {
                        WorkshopId = model.WorkshopId,
                        BrandId = Convert.ToInt32(brandId)
                    });
                }
                db.WorkshopBrands.AddRange(listWsBrand);
                return db.SaveChanges() > 0;
            }
            else
            {
                return false;
            }
        }

        public List<DistributorBrand> GetDistributorBrand(string role, string userId)
        {
            List<DistributorBrand> listDistributorBrand = new List<DistributorBrand>();
            if (role == Constants.Distributor)
            {
                var distributorUsers = db.DistributorUsers.Where(a => a.UserId == userId).FirstOrDefault();
                if (distributorUsers != null)
                {
                    listDistributorBrand = db.DistributorBrands.Where(x => x.DistributorId == distributorUsers.DistributorId).ToList();
                }
                //else
                //{
                //    listDistributorBrand = db.DistributorBrands.ToList();
                //}
            }
            return listDistributorBrand;
        }
        #endregion
    }
}