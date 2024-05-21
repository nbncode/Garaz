using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class CartItemCountRequest
    {
        [Required(ErrorMessage ="TempOrderId Required")]
        public int TempOrderId { get; set; }
    }
}