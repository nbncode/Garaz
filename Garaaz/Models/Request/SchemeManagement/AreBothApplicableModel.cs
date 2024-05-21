using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class AreBothApplicableModel
    {
        public int SchemeId { get; set; }
        public bool AreBothCbAgApplicable { get; set; }
    }
}