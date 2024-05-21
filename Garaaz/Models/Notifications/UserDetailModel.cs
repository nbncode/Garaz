using System;

namespace Garaaz.Models
{
    public class UserDetailModel
    {
        public int UserDetailId { get; set; }
        public string ConsPartyCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public string DistributorId { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<int> OTP { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Password { get; set; }

        public string DistributorName { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

    }
}