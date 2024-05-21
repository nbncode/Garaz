using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Garaaz.Models
{
    public class GrowthPercentageModel
    {
        [Required(ErrorMessage = "Please select distributor")]
        public int DistributorId { get; set; }
        [Required(ErrorMessage = "Please enter growth percentage")]
        public decimal GrowthPercentage { get; set; }
        [Required(ErrorMessage = "Please enter minimum value")]
        public decimal MinValue { get; set; }
    }
    public class ClsGrowthPercentage
    {
        garaazEntities db = new garaazEntities();
        
        #region Save GrowthPercentage
        public bool SaveGrowthPercentage(GrowthPercentageModel model)
        {
            var old = db.GrowthPercentages.FirstOrDefault(m => m.DistributorId == model.DistributorId);
            if (old != null)
            {
                old.DistributorId = model.DistributorId;
                old.GrowthPercentage1 = model.GrowthPercentage;
                old.MinValue = model.MinValue;
                db.SaveChanges();
                return true;
            }

            var growthPercentage = new GrowthPercentage
            {
                DistributorId = model.DistributorId,
                GrowthPercentage1 = model.GrowthPercentage,
                MinValue = model.MinValue,
            };
            db.GrowthPercentages.Add(growthPercentage);
            return db.SaveChanges() > 0;
        }
        #endregion

        #region Get GrowthPercentage
        public GrowthPercentage GetGrowthPercentage(GrowthPercentageModel model)
        {
            var growthPercentage = db.GrowthPercentages.FirstOrDefault(m => m.DistributorId == model.DistributorId);
            if (growthPercentage != null) return growthPercentage;

            growthPercentage = new GrowthPercentage { MinValue = 10, GrowthPercentage1 = 10 };
            return growthPercentage;
        }
        #endregion
    }
}