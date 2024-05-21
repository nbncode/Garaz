using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class ProductLocationRequest
    {
        [Required(ErrorMessage = "ProductId Required")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "DistributorId Required")]
        public int DistributorId { get; set; }
    }
}