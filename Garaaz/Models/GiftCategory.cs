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
    
    public partial class GiftCategory
    {
        public int GiftCategoryId { get; set; }
        public Nullable<int> GiftId { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public Nullable<bool> IsAll { get; set; }
    
        public virtual CategoryScheme CategoryScheme { get; set; }
        public virtual GiftManagement GiftManagement { get; set; }
    }
}
