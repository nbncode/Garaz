using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RepoMgaCatalougeBanner
    {
        #region Variables  
        garaazEntities db = new garaazEntities();
        General general = new General();
        #endregion
        #region Save Banner
        public ResultModel SaveBanner(MgaBanner model)
        {

            var old = db.MgaCatalougeBanners.Where(m => m.Id == model.Id).FirstOrDefault();
            if (old != null)
            {
                old.Id = model.Id;
                old.ImagePath = model.ImagePath;
                old.DistributorId = model.DistributorId;
                old.ShortDescription = model.ShortDescription;
                db.SaveChanges();
                return new ResultModel()
                {
                    Message = "Banner Update Successfully",
                    ResultFlag = 1
                };
            }
            else
            {
                var Bannerdata = new MgaCatalougeBanner()
                {
                    Id = model.Id,
                    ImagePath = model.ImagePath,
                    DistributorId = model.DistributorId,
                    ShortDescription = model.ShortDescription,
                    CreateDate = DateTime.Now
                };
                db.MgaCatalougeBanners.Add(Bannerdata);
                db.SaveChanges();
                return new ResultModel()
                {
                    Message = "Banner Save Successfully",
                    ResultFlag = 1
                };
            }
        }
        #endregion]

        #region Get All Banner
        public List<MgaBannerResponse> GetAllBanner()
        {
            var mgaBanner = new List<MgaBannerResponse>();
            mgaBanner = db.MgaCatalougeBanners.Select(s => new MgaBannerResponse()
            {
                Id = s.Id,
                DistributorId = s.DistributorId != null ? s.DistributorId.Value : 0,
                ImagePath = s.ImagePath,
                ShortDescription = s.ShortDescription
            }).ToList();
            mgaBanner.ForEach(a => { a.ImagePath = general.CheckImageUrl(!string.IsNullOrEmpty(a.ImagePath) ? a.ImagePath : "/assets/images/NoPhotoAvailable.png"); });
            return mgaBanner;
        }
        #endregion

        #region Get All Banner Distributor wise 
        public List<MgaBannerResponse> GetDistributorBanner(int DistributorId)
        {
            var mgaBanner = new List<MgaBannerResponse>();
            if (DistributorId > 0)
            {
                mgaBanner = db.MgaCatalougeBanners.Where(x => x.DistributorId != null && x.DistributorId == DistributorId).Select(s => new MgaBannerResponse()
                {
                    Id = s.Id,
                    DistributorId = s.DistributorId != null ? s.DistributorId.Value : 0,
                    ImagePath = s.ImagePath,
                    ShortDescription = s.ShortDescription
                }).ToList();
                mgaBanner.ForEach(a => { a.ImagePath = general.CheckImageUrl(!string.IsNullOrEmpty(a.ImagePath) ? a.ImagePath : "/assets/images/NoPhotoAvailable.png"); });
            }
            return mgaBanner;
        }
        #endregion

        #region Get Banner By Id
        public MgaBanner GetBannerById(int Id)
        {
            var data = db.MgaCatalougeBanners.Where(x => x.Id == Id).FirstOrDefault();
            return new MgaBanner()
            {
                Id = data.Id,
                ImagePath = data.ImagePath,
                DistributorId = Convert.ToInt32(data.DistributorId),
                ShortDescription = data.ShortDescription
            };
        }
        #endregion

        #region Delete Banner By Id
        public ResultModel DeleteBannerById(int Id)
        {
            var data = db.MgaCatalougeBanners.Where(x => x.Id == Id).FirstOrDefault();
            if (data != null)
            {
                var bannerProducts = db.MgaCatalougeProducts.Where(x => x.BannerId == data.Id);
                if (bannerProducts.Any())
                {
                    db.MgaCatalougeProducts.RemoveRange(bannerProducts);
                    db.SaveChanges();
                }
                db.MgaCatalougeBanners.Remove(data);
                db.SaveChanges();
                return new ResultModel()
                {
                    Message = "Banner delete Successfully",
                    ResultFlag = 1
                };
            }
            else
            {
                return new ResultModel()
                {
                    Message = "Banner Not Deleted",
                    ResultFlag = 0
                };
            }
        }

        #endregion

        #region GetAllMgaBannerByUserId
        public List<MgaBannerResponse> AllMgaBannerByUserId(MgaBannerRequest model, out int totalRecord)
        {
            totalRecord = 0; int distributorId = 0;
            var mgaBanner = new List<MgaBannerResponse>();
            distributorId = model.UserId.GetDistributorId(model.Role);

            mgaBanner = db.MgaCatalougeBanners.Where(x => x.DistributorId != null && x.DistributorId == (model.Role == Constants.SuperAdmin ? x.DistributorId : distributorId)).Select(s => new MgaBannerResponse()
            {
                Id = s.Id,
                DistributorId = s.DistributorId != null ? s.DistributorId.Value : 0,
                ImagePath = s.ImagePath,
                ShortDescription = s.ShortDescription
            }).ToList();
            totalRecord = mgaBanner.Count();
            mgaBanner.ForEach(a => { a.ImagePath = general.CheckImageUrl(!string.IsNullOrEmpty(a.ImagePath) ? a.ImagePath : "/assets/images/NoPhotoAvailable.png"); });
            mgaBanner = model.PageNumber > 0 ? mgaBanner.GetPaging(model.PageNumber, model.PageSize) : mgaBanner;
            return mgaBanner;
        }
        #endregion

        #region GetAllMgaBannerByUserId
        public List<ProductModel> AllProductByMgaBannerId(MgaBannerProductRequest model, out int totalRecord)
        {
            totalRecord = 0;
            IQueryable<TempOrderDetail> tempOrderDetails = Enumerable.Empty<TempOrderDetail>().AsQueryable();
            if (model.TempOrderId.HasValue)
            {
                tempOrderDetails = db.TempOrderDetails.Where(a => a.TempOrderId == model.TempOrderId.Value);
            }
            var bannerProduct = new List<ProductModel>();
            if (model.BannerId > 0)
            {
                bannerProduct = (from d in db.MgaCatalougeProducts
                                 join p in db.Products on d.ProductId equals p.ProductId
                                 where d.ProductId != null && d.BannerId == model.BannerId
                                 select p).ToList()
                                 .Select(p => new ProductModel()
                                 {
                                     ProductId = p.ProductId,
                                     ProductName = string.IsNullOrEmpty(p.ProductName) ? p.Description : p.ProductName,
                                     PartNumber = p.PartNo,
                                     Description = p.Description,
                                     Price = !p.Price.HasValue ? "0" : String.Format("{0:#,###}", p.Price),
                                     ImagePath = p.ImagePath,
                                     Stock = 0,
                                     Color = "",
                                     DistributorName = string.Empty,
                                     //BrandId = p.BrandId
                                 }
                                 ).ToList();
                totalRecord = bannerProduct.Count();
                bannerProduct = model.PageNumber > 0 ? bannerProduct.GetPaging(model.PageNumber, model.PageSize) : bannerProduct;

                foreach (var productModel in bannerProduct)
                {
                    //var isOriparts = db.Brands.Where(b => b.BrandId == (productModel.BrandId != null ? productModel.BrandId.Value : 0)).FirstOrDefault()?.IsOriparts;
                    //productModel.IsOriparts = isOriparts != null ? isOriparts.Value : false;

                    var cartItem = tempOrderDetails.Where(a => a.ProductId == productModel.ProductId).FirstOrDefault();
                    if (cartItem != null)
                    {
                        productModel.CartQty = cartItem.Qty;
                        productModel.CartAvailabilityType = cartItem.AvailabilityType;
                        productModel.CartOutletId = cartItem.OutletId;
                    }
                    productModel.ImagePath = general.CheckImageUrl(productModel.ImagePath);
                }

            }
            return bannerProduct;
        }
        #endregion
    }
}