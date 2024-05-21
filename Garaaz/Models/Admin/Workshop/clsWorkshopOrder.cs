using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class OrderData
    {
        public string UserID { get; set; }
        public int WorkshopID { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }

    public class OrderResponseModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? OrderTotal { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }

    public class clsWorkshopOrder
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region Add New Order
        public int SaveOrder(OrderTable model)
        {
            db.OrderTables.Add(model);
            db.SaveChanges();
            return model.OrderID;
        }
        #endregion

        #region Save Order Details
        public bool SaveOrderDetails(OrderDetail model)
        {
            db.OrderDetails.Add(model);
            return db.SaveChanges() > 0;
        }
        public bool SaveOrderDetails(List<OrderDetail> model)
        {
            db.OrderDetails.AddRange(model);
            return db.SaveChanges() > 0;
        }
        #endregion

        #region Delete Order
        public bool DeleteOrder(int OrderID)
        {
            //Get all the order details record & remove them first
            List<OrderDetail> lstOrderDetails = db.OrderDetails.Where(x => x.OrderID == OrderID).ToList();
            db.OrderDetails.RemoveRange(lstOrderDetails);

            // then remove the order table single record
            OrderTable OrderEntity = db.OrderTables.FirstOrDefault(x => x.OrderID == OrderID);
            db.OrderTables.Remove(OrderEntity);

            return db.SaveChanges() > 0;
        }
        #endregion

        #region Get Order History for workshop
        public List<OrderResponseModel> GetOrderHistoryForWorkshop(int WorkshopID)
        {
            List<OrderResponseModel> response = new List<OrderResponseModel>();

            List<OrderTable> lstOrders = db.OrderTables.Where(x => x.WorkshopID == WorkshopID).OrderByDescending(x => x.OrderDate).ToList();
            foreach(OrderTable ot in lstOrders)
            {
                OrderResponseModel orderRecord = new OrderResponseModel();
                orderRecord.OrderID = ot.OrderID;
                orderRecord.OrderDate = Convert.ToDateTime(ot.OrderDate);
                orderRecord.OrderTotal = Convert.ToDecimal(ot.OrderTotal);

                List<OrderDetail> lstDetails = db.OrderDetails.Where(x => x.OrderID == ot.OrderID).ToList();
                orderRecord.OrderDetails = lstDetails;
                response.Add(orderRecord);
            }
            return response;
        }
        #endregion
    }
}