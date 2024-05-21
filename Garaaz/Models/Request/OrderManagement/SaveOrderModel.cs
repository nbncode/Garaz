namespace Garaaz.Models
{
    public class SaveOrderModel
    {
        public int TempOrderId { get; set; }
        public string PaymentMethod { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }

        /// <summary>
        /// Gets or sets whether request is from mobile.
        /// </summary>
        public bool IsFromMobile { get; set; }
    }
}