using System;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public abstract class BaseRequestModel
    {
        public string CarMake { get; set; }
        public string Model { get; set; }
        public int? Year { get; set; }
        public string Modification { get; set; }
        public string Search { get; set; }

        [Required(ErrorMessage = "Part number and quantity is required.")]
        public string PartNumAndQty { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }
    }
}