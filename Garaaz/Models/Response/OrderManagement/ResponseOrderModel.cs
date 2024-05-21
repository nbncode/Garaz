using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class ResponseOrderModel
    {
        public int OrderID { get; set; }
        public string OrderNo { get; set; }
        public Nullable<int> WorkshopID { get; set; }
        public Nullable<int> DeliveryAddressId { get; set; }
        public string PaymentMethod { get; set; }
        public Nullable<decimal> SubTotal { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public string DiscountCode { get; set; }
        public Nullable<decimal> DeliveryCharge { get; set; }
        public Nullable<decimal> PackingCharge { get; set; }
        public string UserID { get; set; }
        public Nullable<decimal> OrderTotal { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public string OrderDateStr { get; set; }
        public List<ResponseOrderDetailModel> OrderDetails { get; set; }
        public DeliveryAddress DelAddress { get; set; }
        public string OrderStatus { get; set; }
        public int Count { get; set; }
        public string OutletAddress { get; set; }
        public string WorkshopName { get; set; }
        public string WorkshopCode { get; set; }
        public string OutletName { get; set; }
        public string OutletCode { get; set; }
        public string SalesExecutiveName { get; set; }
    }
}