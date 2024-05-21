using System;

namespace Garaaz.Models.DashboardOverview.Cbo
{
    public class CustomerPartDetail
    {
        public int SlNo { get; set; }
        public DateTime CoDate { get; set; }
        public string LocCode { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string Order { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OrderValue { get; set; }
        public string Cbo { get; set; }
        public string StkMw { get; set; }
        public string Eta { get; set; }
        public string Inv { get; set; }
        public string Pick { get; set; }
        public string Alloc { get; set; }
        public string Bo { get; set; }
        public string Ao { get; set; }
        public string Action { get; set; }
        public string Pd { get; set; }
    }
}