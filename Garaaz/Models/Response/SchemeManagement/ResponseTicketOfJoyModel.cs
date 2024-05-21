using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseTicketOfJoyModel
    {
        public int TicketId { get; set; }
        public int SchemeId { get; set; }
        public int? GroupId { get; set; }
        public int? ProductId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string CouponCode { get; set; }
    }
}