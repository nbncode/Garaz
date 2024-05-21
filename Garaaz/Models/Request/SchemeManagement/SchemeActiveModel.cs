using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class SchemeActiveModel
    {
        public int SchemeId { get; set; }
        public bool IsActive { get; set; }
    }
}