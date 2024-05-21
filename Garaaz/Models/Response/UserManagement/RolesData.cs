using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RolesData
    {
        public string Id { get; set; }
        public string Name { get; set; }        
    }
    public class UsersResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string OutletCode { get; set; }
        public string ConsPartyCode { get; set; }
        public string OutletName { get; set; }
        public string Location { get; set; }
        public string CompanyName { get; set; }
    }
    public class UsersPagination 
    {
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string LocationCode { get; set; }
    }
    public class Roles
    {
        public string key { get; set; }
        public string value { get; set; }
        public string label { get; set; }
    }

}