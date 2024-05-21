using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RepoProductAvailabilityType
    {
        #region Variables  
        garaazEntities db = new garaazEntities();
        #endregion
        #region Save ProductAvailabilityType
        public ResultModel SaveProductAvailabilityType(ProductAvailabilityType model)
        {

            var old = db.ProductAvailabilityTypes.Where(m => m.Id == model.Id).FirstOrDefault();
            if (old != null)
            {
                old.Id = model.Id;
                old.Text = model.Text;
                old.IsActive = model.IsActive;
                db.SaveChanges();
                return new ResultModel()
                {
                    Message = "Product Availability Type Update Successfully",
                    ResultFlag = 1
                };
            }
            else
            {
                var ProductType = new ProductAvailabilityType()
                {
                    Id = model.Id,
                    Text = model.Text,
                    IsActive = model.IsActive
                };
                db.ProductAvailabilityTypes.Add(ProductType);
                db.SaveChanges();
                return new ResultModel()
                {
                    Message = "Product Availability Type Save Successfully",
                    ResultFlag = 1
                };
            }
        }
        #endregion]

        #region Get All Product Availability Type
        public List<ProductAvailabilityType> GetProductAvailabilities()
        {
            return (db.ProductAvailabilityTypes.ToList());
        }

        #endregion

        #region Get GetProductAvailability By Id
        public ProductAvailabilityType GetProductAvailabilityById(int Id)
        {
            var data = db.ProductAvailabilityTypes.Where(x => x.Id == Id).FirstOrDefault();
            return new ProductAvailabilityType()
            {
                Id = data.Id,
                Text = data.Text,
                IsActive = data.IsActive,
            };
        }
        #endregion

        #region Delete GetProductAvailability By Id
        public ResultModel DeleteGetProductAvailability(int Id)
        {
            var data = db.ProductAvailabilityTypes.Where(x => x.Id == Id).FirstOrDefault();
            if (data != null)
            {
                db.ProductAvailabilityTypes.Remove(data);
                db.SaveChanges();
                return new ResultModel()
                {
                    Message = "ProductAvailability delete Successfully",
                    ResultFlag = 1
                };
            }
            else
            {
                return new ResultModel()
                {
                    Message = "ProductAvailability Not Deleted",
                    ResultFlag = 0
                };
            }
        }

        #endregion
    }
}