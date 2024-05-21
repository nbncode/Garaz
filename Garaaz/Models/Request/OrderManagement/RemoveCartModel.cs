using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RemoveCartModel
    {
        public int TempOrderId { get; set; }
        public int? ProductId { get; set; }
    }
}