using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class PaymentMethodModel
    {
        public int TempOrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string RazorOrderId { get; set; }
        public string RazorPaymentId { get; set; }
        public string RazorSignature { get; set; }
    }
    public class PromoCodeModel
    {
        [Required(ErrorMessage ="TempOrderId is required.")]
        public int TempOrderId { get; set; }        
        public string UserId { get; set; }
        [Required(ErrorMessage = "CouponNo is required.")]
        public string CouponNo { get; set; }
    }
}