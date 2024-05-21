using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class RazorPayResponse
    {
        public string Key { get; set; }
        public string SecreyKey { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string OrderId { get; set; }
        public string Prefill_Name { get; set; }
        public string Prefill_Email { get; set; }
        public string Prefill_Contact { get; set; }
        public string Address { get; set; }
        public decimal Amount { get; set; }
    }

    public class RazorPayRequest
    {
        [Required(ErrorMessage = "TempOrderId orderId required.")]
        public string TempOrderId { get; set; }
    }

    public class RazorPayOutstandingRequest
    {
        [Required(ErrorMessage = "Outstanding Amount required.")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "UserId required.")]
        public string UserId { get; set; }
    }
    public class RazorPayOutstandingPayment
    {
        [Required(ErrorMessage = "Outstanding Amount required.")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "UserId required.")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Role required.")]
        public string Role { get; set; }
        [Required(ErrorMessage = "Razorpay PaymentId required.")]
        public string RazorpayPaymentId { get; set; }
    }
    public class RazorPaymentRequest
    {
        [Required(ErrorMessage = "Payment mode required.")]
        public string PaymentMethod { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
    }
}