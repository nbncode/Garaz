using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class MgaBanner
    {
        public int Id { get; set; } 
        [Required(ErrorMessage = "Please Upload Banner.")]
        public string ImagePath { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        [Required(ErrorMessage = "Please select distributor.")]
        public int DistributorId { get; set; }
        public string ShortDescription { get; set; }
        public int Value { get; set; }
        public SelectList Distributors { get; set; }
    }
}