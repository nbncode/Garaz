using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class ProductPartCategory
    {
        public static List<SelectListItem> PartCategoriesFullForm => new List<SelectListItem>
            {
            new SelectListItem { Text = "MGP", Value = "M",Group=new SelectListGroup { Name="Maruti Suzuki Genuine Parts" } },
                 new SelectListItem { Text = "MGA", Value = "AA",Group=new SelectListGroup { Name="Maruti Suzuki Genuine Accessories" } },
                  new SelectListItem { Text = "MGO", Value = "AG",Group=new SelectListGroup { Name="Maruti Suzuki Genuine Oil" } },
                  new SelectListItem { Text = "Tools", Value = "T",Group=new SelectListGroup { Name="" } },

            };

        public static List<SelectListItem> GetPartCategories()
        {
            var PartCategories = new List<SelectListItem>();
            using (var context = new garaazEntities())
            {
                // context.Configuration.AutoDetectChangesEnabled = false;

                var dbCategories = context.DailySalesTrackerWithInvoiceDatas.AsNoTracking().GroupBy(s => s.PartCategory).Select(s => new SelectListItem
                {
                    Text = s.Key,
                    Value = s.Key
                }).ToList();

                if (dbCategories.Count > 0)
                {
                    var matched = (from d in dbCategories
                                   join s in PartCategoriesFullForm on d.Value equals s.Value
                                   select new SelectListItem
                                   {
                                       Text = s.Text,
                                       Value = d.Value
                                   }).ToList();
                    var matchedVaues = matched.Select(m => m.Value);

                    dbCategories = dbCategories.Where(d => !matchedVaues.Contains(d.Value)).ToList();

                    PartCategories.AddRange(dbCategories);
                    PartCategories.AddRange(matched);
                }
            }

            return PartCategories;
        }
    }
}