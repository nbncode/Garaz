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
    
    public partial class OutletsUser
    {
        public int Id { get; set; }
        public int OutletId { get; set; }
        public string UserId { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Outlet Outlet { get; set; }
    }
}
