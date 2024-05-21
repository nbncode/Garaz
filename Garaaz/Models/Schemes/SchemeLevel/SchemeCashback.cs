using System.Collections.Generic;

namespace Garaaz.Models
{
    public class SchemeCashback
    {
        public int WorkShopId { get; set; }
        public string WorkShopName { get; set; }
    }

    public class SchemeLevelModel
    {
        public bool IsCashbackVisible { get; set; }
        public List<ResponseSchemeLevel> Details { get; set; }
    }
    public class ResponseSchemeLevel
    {
        public int WorkShopId { get; set; }
        public string WorkShopName { get; set; }
        public decimal? CashBack { get; set; }
        public bool IsDistribute { get; set; }
    }
}