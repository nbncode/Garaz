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
    
    public partial class AssuredGiftCategory
    {
        public int AssuredGiftCategoryId { get; set; }
        public Nullable<int> AssuredGiftId { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public Nullable<bool> IsAll { get; set; }
    
        public virtual AssuredGift AssuredGift { get; set; }
        public virtual CategoryScheme CategoryScheme { get; set; }
    }
}
