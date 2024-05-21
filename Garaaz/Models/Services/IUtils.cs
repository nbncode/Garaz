using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models.Services
{
    public interface IUtils
    {
        string GenerateToken();
        string RandomCode(int length);
        string getTime(DateTime dt);
        void setCookiesValue(object value, string cookieName);
        string getStringCookiesValue(string cookiename);
        int getIntCookiesValue(string cookiename);
        void setCurrentUser(string UserId, string Username, string FullName, string Role, TokenResponse user);
        void RemoveCookiesValue(string cookiename);
        string GetIpUser();
        bool ContainsInt(string str, int value);
    }
}