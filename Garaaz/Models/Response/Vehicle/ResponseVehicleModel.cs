namespace Garaaz.Models
{
    public class ResponseVehicleModel
    {
        public int VehicleId { get; set; }
        public int BrandId { get; set; }
        /// <summary>
        /// The brand name.
        /// </summary>
        public string Name { get; set; }
        public string VehicleName { get; set; }
        public string ImagePath { get; set; }
    }
}