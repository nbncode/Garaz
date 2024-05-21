using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoBestSeller
    {
        private readonly garaazEntities _db = new garaazEntities();

        /// <summary>
        /// Gets or sets the distributor Id selected.
        /// </summary>
        [Required(ErrorMessage = "Please select distributor")]
        public int DistributorId { get; set; }

        /// <summary>
        /// Gets select list of distributors.
        /// </summary>
        public SelectList Distributors => DistributorSelect(DistributorId);

        /// <summary>
        /// Gets list of all sellers.
        /// </summary>
        public List<BestSellerModel> AllSellers => GetAllSeller(DistributorId);

        public RepoBestSeller(int distributorId)
        {
            DistributorId = distributorId;
        }

        private List<BestSellerModel> GetAllSeller(int distributorId)
        {
            var data = (from pg in _db.ProductGroups.AsNoTracking()
                        join bs in _db.BestSellers.AsNoTracking() on pg.GroupId equals bs.GroupId.Value into ps
                        from p in ps.DefaultIfEmpty()
                        where pg.DistributorId==distributorId
                        select new BestSellerModel
                        {
                            GroupId = pg.GroupId,
                            Group = pg.GroupName,
                            IsBestSeller = p != null && p.GroupId.Value > 0 && p.DistributorId == distributorId
                        }).OrderBy(s => s.Group).ToList();

            return data;
        }

        /// <summary>
        /// Save best sellers for distributor.
        /// </summary>
        /// <param name="bestSellers">The list of selected best seller.</param>
        /// <param name="distributorId">The distributor for which best seller need to be saved.</param>
        /// <returns>Return true if saved else false.</returns>
        public bool SaveBestSellers(List<BestSeller> bestSellers, int distributorId)
        {
            var existingBestSellers = _db.BestSellers.Where(b => b.DistributorId == distributorId);
            if (existingBestSellers.Any())
            {
                _db.BestSellers.RemoveRange(existingBestSellers);
                _db.SaveChanges();
            }
            
            _db.BestSellers.AddRange(bestSellers);
            return _db.SaveChanges() > 0;
        }

        /// <summary>
        /// Get the list of select type of distributors.
        /// </summary>
        /// <param name="distributorId">The distributor Id to mark selected distributor.</param>
        /// <returns>Return SelectList.</returns>
        private SelectList DistributorSelect(int distributorId)
        {
            // Create select list
            var ru = new RepoUsers();
            var selects = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select Distributor --" } };
            selects.AddRange(ru.GetAllDistributorsNew().Select(dist =>
                new SelectListItem { Value = dist.DistributorId.ToString(), Text = dist.DistributorName }
            ));

            var distSelectList = new SelectList(selects, "Value", "Text", null);

            // So as to keep the item in select list selected (for DistributorController case)
            if (distributorId > 0)
            {
                DistributorId = distributorId;
            }

            return distSelectList;

        }
    }
}