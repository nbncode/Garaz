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
    
    public partial class Log
    {
        public int LogsId { get; set; }
        public string UserId { get; set; }
        public string Details { get; set; }
        public string Link { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string Status { get; set; }
        public Nullable<decimal> OutstandingAmount { get; set; }
        public string PaymentId { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
