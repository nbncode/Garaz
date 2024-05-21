using System.Web.Http.ModelBinding;

namespace Garaaz.Models.DashboardOverview.DataTables
{
    [ModelBinder(typeof(DataTableModelBinder))]
    public class JqDataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }

        public DataTableOrder[] Order { get; set; }
        public DataTableColumn[] Columns { get; set; }
        public DataTableSearch Search { get; set; }
    }
}