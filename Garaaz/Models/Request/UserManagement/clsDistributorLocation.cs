using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class clsDistributorLocation
    {
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Please enter location code")]
        public string LocationCode { get; set; }

        [Required(ErrorMessage = "Please enter location")]
        public string Location { get; set; }

        public int DistributorId { get; set; }
    }
}