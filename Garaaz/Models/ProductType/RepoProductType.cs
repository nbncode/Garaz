using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RepoProductType
    {
        #region Variables  
        garaazEntities db = new garaazEntities();
        #endregion
        public List<ProductType> GetProductType()
        {
            var data = db.ProductTypes.ToList();
            return data;
        }
        public ProductType GetProductTypeById(int Id)
        {
            return db.ProductTypes.Where(x => x.Id == Id).FirstOrDefault();
        }
        public bool AddProductType(ProductType model)
        {
            if (model.Id > 0)
            {
                var data = db.ProductTypes.Where(x => x.Id == model.Id).FirstOrDefault();
                if (data != null)
                {
                    data.TypeName = model.TypeName;
                    data.TypeNameUseInFile = model.TypeNameUseInFile;
                    db.SaveChanges();
                    return true;
                }
            }
            model.CreatedDate = DateTime.Now;
            db.ProductTypes.Add(model);
            return db.SaveChanges() > 0;
        }
        public bool RemoveProductType(int Id)
        {
            var data = db.ProductTypes.Where(x => x.Id == Id).FirstOrDefault();
            if (data != null)
            {
                db.ProductTypes.Remove(data);
                return db.SaveChanges() > 0;
            }
            return false;
        }
    }
}