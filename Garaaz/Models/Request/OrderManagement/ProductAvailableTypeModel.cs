using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class ProductAvailableTypeModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsActive { get; set; }
    }
}