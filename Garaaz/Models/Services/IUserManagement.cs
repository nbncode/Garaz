using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Garaaz.Models.Services
{
    public interface IUserManagement
    {
        string GetCurrentUserName();
        ApplicationUser GetCurrentUserById(string userId);
        Task<string> GetCurrentUserId();
        string GetUserIdByUsername(string Username);
        IdentityResult RegisterUser(string Email, string Password, string[] Roles, out string UserId);
        IdentityResult RegisterUser(string PhoneNumber, string Email, string Password, string[] Roles, bool IsApproved, out string UserId);
        IdentityResult RegisterUser(string PhoneNumber, string Email, string Password, string[] Roles, bool IsApproved, string EmployeeCode, out string UserId, string distributorId = null);
        bool LockUserAccount(string Userid);
        bool UnlockUserAccount(string Userid);
        bool SetApproved(string Userid, bool Approved);
        string GetPasswordChangeToken(string userId);
        bool ChangePassword(string userId, string newPassword);
        bool ChangePassword(string userId, string newPassword, string code);
        bool DeleteUserAccount(string userId);
        bool SetEmail(string Userid, string email);

        List<ApplicationUser> GetSuperAdminList();

        string GetSuperAdminRoleId();

        IdentityResult UpdateUserName(string userId, string userName);
    }
}