using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class MgaBannerResponse
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int DistributorId { get; set; }
        public string ShortDescription { get; set; }
    }
}