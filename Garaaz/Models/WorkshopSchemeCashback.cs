//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Garaaz.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WorkshopSchemeCashback
    {
        public int Id { get; set; }
        public Nullable<int> SchemeId { get; set; }
        public string SchemeName { get; set; }
        public Nullable<int> WorkshopId { get; set; }
        public string WorkshopName { get; set; }
        public string CustomerType { get; set; }
        public Nullable<int> DistributorId { get; set; }
        public string DistributorName { get; set; }
        public string RoInchargeId { get; set; }
        public string SalesExecutiveId { get; set; }
        public Nullable<int> OutletId { get; set; }
        public Nullable<decimal> Cashback { get; set; }
        public string CreatedDate { get; set; }
        public Nullable<bool> IsPaid { get; set; }
    }
}
