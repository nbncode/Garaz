namespace Garaaz.Models
{
    public class StockColorModel
    {
        public int? LowStockQty { get; set; }
        public int? MediumStockQty { get; set; }
        public string LowStockColor { get; set; }
        public string MediumStockColor { get; set; }
        public string HighStockColor { get; set; }        
        public string LowStockTag { get; set; }       
        public string MediumStockTag { get; set; }
        public string HighStockTag { get; set; }
    }
}