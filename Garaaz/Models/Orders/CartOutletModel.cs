using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class CartOutletModel
    {
        public int OutletId { get; set; }
        public string Outlet { get; set; }
        public bool IsDefault { get; set; }
    }
}