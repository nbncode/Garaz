using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoCBO
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
        public List<BackOrderModel> CustomerBackOrders => GetAllCbo(DistributorId);

        public RepoCBO(int distributorId)
        {
            DistributorId = distributorId;
        }

        private List<BackOrderModel> GetAllCbo(int distributorId)
        {
            var data = _db.CustomerBackOrders.AsNoTracking().Where(o => o.DistributorId == distributorId).Select(o => new BackOrderModel()
            {
                CustomerOrderId = o.CustomerOrderId,
                DistributorId = o.DistributorId,
                CONo = o.CONo,
                CODate = o.CODate,
                PartyType = o.PartyType,
                PartyCode = o.PartyCode,
                PartyName = o.PartyName,
                PartStatus = o.PartStatus,
                PartNum = o.PartNum,
                PartDesc = o.PartDesc,
                LocCode = o.LocCode,
                Order = o.Order
            }).ToList();

            return data;
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
    public class BackOrder
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region Get CustomerBack_Order by distributor
        public List<BackOrderExelModel> GetCboByDistributor(int distributorId)
        {
            int index = 1;
            var data = db.CustomerBackOrders.Where(o => o.DistributorId == distributorId).Include(d => d.Distributor).AsNoTracking().ToList();

            return data.Select(o => new BackOrderExelModel()
            {
                SrNo = index++,
                Distributor = o.Distributor != null ? o.Distributor.DistributorName : "",
                CONo = o.CONo,
                OrderDate = o.CODate != null ? o.CODate.Value.ToShortDateString() : "",
                PartyCode = o.PartyCode,
                PartyName = o.PartyName,
                PartStatus = o.PartStatus != null ? o.PartStatus : "Pending",
                PartNum = o.PartNum,
                PartDesc = o.PartDesc,
                LocCode = o.LocCode,
                Order = o.Order
            }).ToList();
        }
        #endregion

        #region Get CustomerBack_Order by outletId
        public List<BackOrderModel> GetAllCboByOutletId(int outletId)
        {
            var data = (from b in db.CustomerBackOrders.AsNoTracking()
                        join w in db.WorkShops.AsNoTracking() on b.WorkshopId equals w.WorkShopId
                        where w.outletId == outletId
                        select new BackOrderModel()
                        {
                            CustomerOrderId = b.CustomerOrderId,
                            DistributorId = b.DistributorId,
                            CONo = b.CONo,
                            CODate = b.CODate,
                            PartyType = b.PartyType,
                            PartyCode = b.PartyCode,
                            PartyName = b.PartyName,
                            PartStatus = b.PartStatus,
                            PartNum = b.PartNum,
                            PartDesc = b.PartDesc,
                            LocCode = b.LocCode,
                            Order = b.Order
                        }).ToList();

            return data;
        }
        #endregion

        #region Get CustomerBack_Order by outletId
        public List<BackOrderExelModel> GetCboByOutletId(int outletId)
        {
            int index = 1;
            var data = (from b in db.CustomerBackOrders.Include(d => d.Distributor).AsNoTracking()
                        join w in db.WorkShops.AsNoTracking() on b.WorkshopId equals w.WorkShopId
                        where w.outletId == outletId
                        select b
                        ).ToList();

            return data.Select(o => new BackOrderExelModel()
            {
                SrNo = index++,
                Distributor = o.Distributor != null ? o.Distributor.DistributorName : "",
                CONo = o.CONo,
                OrderDate = o.CODate != null ? o.CODate.Value.ToShortDateString() : "",
                PartyCode = o.PartyCode,
                PartyName = o.PartyName,
                PartStatus = o.PartStatus != null ? o.PartStatus : "Pending",
                PartNum = o.PartNum,
                PartDesc = o.PartDesc,
                LocCode = o.LocCode,
                Order = o.Order
            }).ToList();
        }
        #endregion
    }
}