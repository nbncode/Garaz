using System;
using System.Web.Script.Serialization;

namespace Garaaz.Models
{
    public class AuthManagement : IAuthManagement
    {
        public TokenResponse GetToken(string username, string password)
        {
            // So that we can allow special character such as &,? etc in password and avoid BadRequest error
            var escapePassword = Uri.EscapeDataString(password);

            var jss = new JavaScriptSerializer();
            TokenResult tokenResult = jss.Deserialize<TokenResult>(General.GetToken(Constants.TokenApi, $"grant_type=password&username={username}&password={escapePassword}"));

            if (string.IsNullOrWhiteSpace(tokenResult.error))
            {
                return new TokenResponse
                {
                    AccessToken = tokenResult.access_token,
                    ExpiresIn = tokenResult.expires_in,
                    RefreshToken = tokenResult.refresh_token,
                    Username = tokenResult.userName
                };
            }

            return new TokenResponse
            {
                Error = !string.IsNullOrWhiteSpace(tokenResult.error) ? tokenResult.error : "Username password not match"
            };
        }

        public TokenResponse GetTokenFromRefreshToken(string refreshToken)
        {
            var jss = new JavaScriptSerializer();
            TokenResult tokenResult = jss.Deserialize<TokenResult>(General.GetToken(Constants.TokenApi, "grant_type=refresh_token&refresh_token=" + refreshToken));

            if (!string.IsNullOrEmpty(tokenResult.error) || tokenResult.error == null)
            {
                return new TokenResponse
                {
                    AccessToken = tokenResult.access_token,
                    ExpiresIn = tokenResult.expires_in,
                    RefreshToken = tokenResult.refresh_token,
                    Username = tokenResult.userName
                };
            }

            return new TokenResponse
            {
                Error = "refresh token is expire"
            };
        }
    }
}