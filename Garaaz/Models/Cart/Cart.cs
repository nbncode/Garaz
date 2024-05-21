using System.Collections.Generic;

namespace Garaaz.Models
{
   
    public class Cart
    {
        public List<CartMain> CartMain { get; set; }
        public string SoldBy { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? Discount { get; set; }
        public string DiscountCode { get; set; }
        public decimal? DeliveryCharge { get; set; }
        public decimal? PackingCharge { get; set; }
        public decimal? GrandTotal { get; set; }
        public int TotalItems { get; set; }
        public int TempOrderId { get; set; }
    }
    public class CartMain
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public List<clsCartData> data { get; set; }
    }

  //public  enum CartEnum
  //  {
  //      "Available At Default Outlet" = 1,
  //      "Available on other Outlet" =2,
  //      "Not Available"=3
  //  }
}