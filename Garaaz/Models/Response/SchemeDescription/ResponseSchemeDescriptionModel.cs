using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseSchemeDescriptionModel
    {
        public string ImagePath  { get; set; }
        public List<SchemeDescriptionCatetgory> lstCategory { get; set; }
        public List<SchemeDescriptionFocuspart> lstFocusPart { get; set; }
    }

    public class SchemeDescriptionCatetgory
    {
        public string Name { get; set; }
        public string ColorCode { get; set; }
        public List<SchemeDescriptionCatetgoryItem> lstCategoryItem { get; set; }
    }

    public class SchemeDescriptionCatetgoryItem
    {
        public string Image { get; set; }
        public string Brand { get; set; }
        public string vehicle { get; set; }
    }

    public class SchemeDescriptionFocuspart
    {
        public string Product { get; set; }
        public int Qty { get; set; }
        public int Value { get; set; }
    }

}