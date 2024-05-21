using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class clsVehicle
    {
        public int VehicleId { get; set; }
        [Required(ErrorMessage = "Please select brand id")]
        public int BrandId { get; set; }
        [Required(ErrorMessage = "Vehicle name required")]
        public string VehicleName { get; set; }
        [Required(ErrorMessage = "Please select vehicle image")]
        public string ImagePath { get; set; }
        
    }
}