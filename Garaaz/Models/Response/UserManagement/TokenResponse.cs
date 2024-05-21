using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class TokenResponse
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public IList<string> Roles { get; set; }
        public string AccessToken { get; set; }
        public string ExpiresIn { get; set; }
        public string Error { get; set; }
        public string RefreshToken { get; set; }
        public string Profile { get; set; }
        public int? TempOrderId { get; set; }
    }
}