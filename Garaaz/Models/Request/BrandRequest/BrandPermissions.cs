using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class BrandPermissions
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region Brand
        #region Get All Brand
        public List<clsBrand> GetAllBrand()
        {
            return (from B in db.Brands
                    select new clsBrand()
                    {
                        BrandId = B.BrandId,
                        Name = B.Name,
                        ImagePath = B.Image,
                        IsOriparts = B.IsOriparts != null ? B.IsOriparts.Value : false
                    }).ToList();
        }

        public List<clsBrand> GetAllBrand(string userId, string role, string SearchString)
        {
            var listClsBrands = new List<clsBrand>();
            var general = new General();

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
            {
                switch (role)
                {
                    case "SuperAdmin":
                        listClsBrands = (from b in db.Brands                                        
                                         select new clsBrand()
                                         {
                                             BrandId = b.BrandId,
                                             Name = b.Name,
                                             ImagePath = b.Image,
                                             IsOriparts = b.IsOriparts != null ? b.IsOriparts.Value : false
                                         }).ToList();
                        break;
                    case "Distributor":
                        var distId = general.GetDistributorId(userId);
                        var dBrandIds = db.DistributorBrands.Where(b => b.DistributorId == distId).Select(b => b.BrandId);
                        listClsBrands = (from b in db.Brands
                                         where dBrandIds.Contains(b.BrandId)
                                         select new clsBrand()
                                         {
                                             BrandId = b.BrandId,
                                             Name = b.Name,
                                             ImagePath = b.Image,
                                             IsOriparts = b.IsOriparts != null ? b.IsOriparts.Value : false
                                         }).ToList();
                        break;

                    case "Workshop":
                        var wsId = general.GetWorkshopId(userId);
                        var wBrandIds = db.WorkshopBrands.Where(b => b.WorkshopId == wsId).Select(b => b.BrandId);
                        listClsBrands = (from b in db.Brands
                                         where wBrandIds.Contains(b.BrandId)
                                         select new clsBrand()
                                         {
                                             BrandId = b.BrandId,
                                             Name = b.Name,
                                             ImagePath = b.Image,
                                             IsOriparts = b.IsOriparts != null ? b.IsOriparts.Value : false
                                         }).ToList();

                        break;

                    case "DistributorUsers":
                        var distributorId = general.GetDistributorId(userId);
                        if (distributorId > 0)
                        {
                            var distBrandIds = db.DistributorBrands.Where(b => b.DistributorId == distributorId).Select(b => b.BrandId);
                            listClsBrands = (from b in db.Brands
                                             where distBrandIds.Contains(b.BrandId)
                                             select new clsBrand()
                                             {
                                                 BrandId = b.BrandId,
                                                 Name = b.Name,
                                                 ImagePath = b.Image,
                                                 IsOriparts = b.IsOriparts != null ? b.IsOriparts.Value : false
                                             }).ToList();
                        }
                        else
                        {
                            var wshopId = general.GetWorkshopId(userId);
                            if (wshopId > 0)
                            {
                                var wsBrandIds = db.WorkshopBrands.Where(b => b.WorkshopId == wshopId).Select(b => b.BrandId);
                                listClsBrands = (from b in db.Brands
                                                 where wsBrandIds.Contains(b.BrandId)
                                                 select new clsBrand()
                                                 {
                                                     BrandId = b.BrandId,
                                                     Name = b.Name,
                                                     ImagePath = b.Image,
                                                     IsOriparts = b.IsOriparts != null ? b.IsOriparts.Value : false
                                                 }).ToList();
                            }
                        }

                        break;
                }
            }
            else
            {
                listClsBrands = (from B in db.Brands
                                 select new clsBrand()
                                 {
                                     BrandId = B.BrandId,
                                     Name = B.Name,
                                     ImagePath = B.Image,
                                     IsOriparts = B.IsOriparts != null ? B.IsOriparts.Value : false
                                 }).ToList();
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                listClsBrands = listClsBrands.Where(x => x.Name.Contains(SearchString.ToLower())).ToList();
            }

            listClsBrands.ForEach(a => { a.ImagePath = general.CheckImageUrl(a.ImagePath); });
            return listClsBrands;
        }
        #endregion

        #region Get Product Name By Id
        public clsBrand GetBrandNameById(int BrandId)
        {
            var data = db.Brands.Where(x => x.BrandId == BrandId).FirstOrDefault();
            return new clsBrand()
            {
                BrandId = data.BrandId,
                Name = data.Name,
                ImagePath = data.Image,
                IsOriparts = data.IsOriparts != null ? data.IsOriparts.Value : false
            };
        }
        #endregion

        #region Save Product
        public bool SaveBrand(clsBrand model)
        {

            var old = db.Brands.Where(m => m.BrandId == model.BrandId||m.Name==model.Name).FirstOrDefault();
            if (old != null)
            {
                old.BrandId = old.BrandId;
                old.Name = model.Name;
                old.Image = model.ImagePath;
                old.IsOriparts = model.IsOriparts;
                db.SaveChanges();
                return true;
            }
            else
            {
                var Brand = new Brand()
                {
                    BrandId = model.BrandId.Value,
                    Name = model.Name,
                    Image = model.ImagePath,
                    IsOriparts = model.IsOriparts,
                CreatedDate = DateTime.Now
                };
                db.Brands.Add(Brand);
                return db.SaveChanges() > 0;
            }
        }
        #endregion]

        #region Delete Product
        public bool DeleteBrand(int Id)
        {
            var data = db.Brands.Where(m => m.BrandId == Id).FirstOrDefault();
            if (data == null)
            {
                return false;
            }
            else
            {
                db.Brands.Remove(data);
                return db.SaveChanges() > 0;
            }
        }
        #endregion
        #endregion
    }
}