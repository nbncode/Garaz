using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public partial class ResponseBannerMobile
    {
        public int BannerId { get; set; }
        [Required(ErrorMessage = "Please select BannerImage")]
        public string BannerImage { get; set; }
        [Required(ErrorMessage = "Please enter bannername")]
        public string BannerName { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public int? SchemeId { get; set; }
        public DateTime? CreateDate { get; set; }
        [Required(ErrorMessage = "Please select distributor")]
        public int DistributorId { get; set; }
        public SelectList Distributors { get; set; }

        public virtual Scheme Scheme { get; set; }
    }
    public class RepoBanner
    {
        #region Variables  
        garaazEntities db = new garaazEntities();
        #endregion
        #region Save Banner
        public ResultModel SaveBanner(ResponseBannerMobile model)
        {

            var old = db.BannerMobiles.Where(m => m.BannerId == model.BannerId).FirstOrDefault();
            if (old != null)
            {
                old.BannerId = model.BannerId;
                old.BannerImage = model.BannerImage;
                old.BannerName = model.BannerName;
                old.Type = model.Type;
                old.Data = model.Data;
                old.SchemeId = model.SchemeId;
                old.DistributorId = model.DistributorId;
                db.SaveChanges();
                return new ResultModel()
                {
                    Message = "Banner Update Successfully",
                    ResultFlag = 1                   
                };
            }
            else
            {
                var Bannerdata = new BannerMobile()
                {
                    BannerId = model.BannerId,
                    BannerImage = model.BannerImage,
                    BannerName = model.BannerName,
                    Type = model.Type,
                    Data = model.Data,
                    SchemeId = model.SchemeId,
                    DistributorId=model.DistributorId,
                    CreateDate = DateTime.Now
                };
                db.BannerMobiles.Add(Bannerdata);
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
    public List<BannerMobile> GetAllBanner()
    {
            return (db.BannerMobiles.ToList());
                  
    }

        #endregion
        #region Get Banner By Id
        public ResponseBannerMobile GetBannerById(int BannerId)
        {
            var data = db.BannerMobiles.Where(x => x.BannerId == BannerId).FirstOrDefault();
            return new ResponseBannerMobile()
            {
                BannerId = data.BannerId,
                BannerImage = data.BannerImage,
                BannerName = data.BannerName,
                Type = data.Type,
                Data = data.Data,
                SchemeId = data.SchemeId,
                DistributorId=data.DistributorId??0,
            };
        }
        #endregion
        #region Delete Banner By Id
        public ResultModel DeleteBannerById(int BannerId)
        {
            var data = db.BannerMobiles.Where(x => x.BannerId == BannerId).FirstOrDefault();
            if (data != null)
            {
                db.BannerMobiles.Remove(data);
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
        #region Get All Banner By Distributor
        public List<BannerMobile> GetAllBanner(int distributorId)
        {
            return (db.BannerMobiles.Where(b=>b.DistributorId==distributorId).ToList());

        }

        #endregion
    }
}