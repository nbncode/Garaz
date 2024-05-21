using System.Collections.Generic;

namespace Garaaz.Models
{
    public class clsCartData
    {
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public int TempOrderId { get; set; }
        public bool Available { get; set; }
        public string ProductAvailablityType { get; set; }
        public int? OutletId { get; set; }
        public List<OutletData> OutletData { get; set; }
        public List<string> ProductAvailablityNotFoundData { get; set; }
    }

    public class OutletData
    {
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public bool IsDefaultOutlet { get; set; }
        public List<string> lstProductAvailablity { get; set; }
    }
}