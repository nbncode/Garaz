using System.Collections.Generic;

namespace Garaaz.Models.DashboardOverview.DataTables
{
    public class JqDataTableResponse<T> where T: class
    {
        public int draw { get; set; }
        public long recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; }
        public string error { get; set; }
    }
}