using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class DailySale
    {
        [Required(ErrorMessage = "Please select distributor")]
        public int DistributorId { get; set; }
        public SelectList Distributors { get; set; }
    }

    public class WorkShopDistributors
    {
        public string CurrentUserId { get; set; }
        public SelectList Distributors { get; set; }
    }
    public class WorkShopDistributorsRequest
    {
        [Required(ErrorMessage = "You must provide a phone number")]
        public string UserName { get; set; }
    }
    public class WorkShopDistributor
    {
        /// <summary>
        /// Gets or sets the name of the distributor.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the user id of the distributor.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the username of the workshop attached with distributor.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the upiid of the distributor.
        /// </summary>
        public string UPIID { get; set; }
    }
    public class DailySalesInvoiceModel
    {
        public int DailySalesTrackerId { get; set; }
        public string Region { get; set; }
        public string DealerCode { get; set; }
        public string LocCode { get; set; }
        public string LocDesc { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string RootPartNum { get; set; }
        public string PartCategory { get; set; }
        public string Day { get; set; }
        public string PartGroup { get; set; }
        public string CalMonthYear { get; set; }
        public string ConsPartyCode { get; set; }
        public string ConsPartyName { get; set; }
        public string ConsPartyTypeDesc { get; set; }
        public string DocumentNum { get; set; }
        public string Remarks { get; set; }
        public string RetailQty { get; set; }
        public string ReturnQty { get; set; }
        public string NetRetailQty { get; set; }
        public string RetailSelling { get; set; }
        public string ReturnSelling { get; set; }
        public string NetRetailSelling { get; set; }
        public string DiscountAmount { get; set; }
        public string UserId { get; set; }
        public int? DistributorId { get; set; }
        public string CreatedDate { get; set; }
        public int? WorkShopId { get; set; }
        public int? GroupId { get; set; }
        public int? ProductId { get; set; }
        public string CoNo { get; set; }
    }
}