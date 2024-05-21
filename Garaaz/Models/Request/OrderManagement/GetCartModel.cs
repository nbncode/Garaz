using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class GetCartModel
    {
        [Required]
        public int TempOrderId { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}