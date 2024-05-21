using System;

namespace Garaaz.Models.DistributorUsers
{
    public class DistributorUserModel
    {
        public string ConsPartyCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public int? DistributorId { get; set; }
        public bool? IsDeleted { get; set; }
        public int? OTP { get; set; }
        public string Address { get; set; }
        public string OutletCode { get; set; }
        public string OutletName { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
    }

    public class DistributorOutletModel
    {
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public string Address { get; set; }
        public string OutletCode { get; set; }
        public string RoIncharge { get; set; }
    }
    public class WorkshopsModel
    {
        public int WorkshopId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public int DistributorId { get; set; }
        public bool? IsDelete { get; set; }
        public string OutletCode { get; set; }
        public string OutletName { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
    }
}