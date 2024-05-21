using System.Collections.Generic;

namespace Garaaz.Models
{
    public class CouponSchemeModel
    {
        public Scheme Scheme { get; set; }
        public List<SchemeWorkshop> SchemeWorkshops { get; set; }
    }
}