using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class SchemeMain
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public List<AppSchemeModel> data { get; set; }
    }
    public class AppSchemeModel
    {
        public int SchemeId { get; set; }
        public string SchemeName { get; set; }
        public string OnwardsMessage { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string BannerImage { get; set; }
    }
}