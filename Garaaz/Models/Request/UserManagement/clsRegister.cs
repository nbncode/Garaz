using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class clsRegister
    {
        public string UserId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Role { get; set; }
        public bool IsFromDistributorPage { get; set; }
        public bool IsFromDistributorOutletsPage { get; set; }
        public bool IsFromDistributorUsersPage { get; set; }
        public bool IsFromWorkshopPage { get; set; }
        public string DistributorId { get; set; }
    }
}