using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class MgaCatalougeProductmodel
    {
        public int Id { get; set; }
        public Nullable<int> BannerId { get; set; }
        public int? ProductId { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public virtual MgaCatalougeBanner MgaCatalougeBanner { get; set; }
        public virtual Product Product { get; set; }
        public List<string> ProductIds { get; set; }
    }
    public class MgaProductGroup
    {
        public int GroupId { get; set; }
        public string Group { get; set; }
        public int GroupPageNo { get; set; }
        public List<MgaProductModel> Products { get; set; }
    }

    public class MgaProductModel
    {
        public int ProductId { get; set; }
        public string Product { get; set; }
        public string Description { get; set; }
        public bool IsChecked { get; set; }
    }

    public class RepoMgaCatalougeProduct
    {
        #region Variables  
        garaazEntities db = new garaazEntities();
        #endregion
        #region Save Product
        public bool SaveProducts(int bannerid, List<MgaCatalougeProduct> Productlist)
        {
            // Delete existing record for Banner
            var Items = db.MgaCatalougeProducts.Where(r => r.BannerId == bannerid);
            if (Items.Any())
            {
                db.MgaCatalougeProducts.RemoveRange(Items);
                db.SaveChanges();
            }

            // Now save new record
            if (Productlist.Any())
            {
                db.MgaCatalougeProducts.AddRange(Productlist);
                return db.SaveChanges() > 0;
            }
            return false;
        }
        #endregion]
        #region Get Productlist By Id
        public List<string> GetProductListByIdd(int BannerId)
        {
            return db.MgaCatalougeProducts.Where(r => r.BannerId == BannerId).Select(r => r.ProductId.ToString()).ToList();
        }
        #endregion

        public List<MgaProductGroup> GetBannerProducts(int distributorId, int bannerId)
        {
            var lstMgaProductGroup = new List<MgaProductGroup>();
            var data = (
                        from p in db.Products
                        join pg in db.ProductGroups on p.GroupId equals pg.GroupId
                        where p.DistributorId == distributorId
                        select new { pg.GroupId, pg.GroupName, p.ProductId, p.ProductName, p.Description }
                      ).ToList();

            lstMgaProductGroup = (from k in data
                                  group new { k } by new { k.GroupId, k.GroupName } into g
                                  select new MgaProductGroup()
                                  {
                                      Group = g.Key.GroupName,
                                      GroupId = g.Key.GroupId,
                                      Products = data.Where(a => a.GroupId == g.Key.GroupId).Select(s => new MgaProductModel() { ProductId = s.ProductId, Product = !string.IsNullOrEmpty(s.ProductName) ? s.ProductName : s.Description }).ToList()
                                  }).ToList();

            var MgaCatalougeProducts = db.MgaCatalougeProducts.Where(x => x.BannerId == bannerId).ToList();
            foreach (var item in lstMgaProductGroup)
            {
                item.GroupPageNo = 1;
                item.Products = item.Products.GetPaging<MgaProductModel>(1, 50);
                item.Products = (from p in item.Products
                                 join m in MgaCatalougeProducts on p.ProductId equals m.ProductId into ps
                                 from d in ps.DefaultIfEmpty()
                                 select new MgaProductModel()
                                 {
                                     ProductId = p.ProductId,
                                     Product = p.Product,
                                     IsChecked = d != null ? true : false
                                 }).ToList();
            }
            return lstMgaProductGroup;
        }

        public List<MgaProductModel> GetBannerProductsOnLoadMore(ProductLoadMore model)
        {
            var lstMgaProduct = new List<MgaProductModel>();
            var MgaCatalougeProducts = db.MgaCatalougeProducts.Where(x => x.BannerId == model.BannerId).ToList();
            var products = db.Products.Where(x => x.GroupId == model.GroupId && x.DistributorId == model.DistributorId).ToList();
            products = products.GetPaging<Product>(model.PageNumber, 50);
            lstMgaProduct = ( from p in products
                              join m in MgaCatalougeProducts on p.ProductId equals m.ProductId into ps
                             from d in ps.DefaultIfEmpty()
                             select new MgaProductModel()
                             {
                                 ProductId = p.ProductId,
                                 Product = p.ProductName ?? p.Description,
                                 IsChecked = d != null ? true : false
                             }).ToList();
            return lstMgaProduct;
        }
    }
}

