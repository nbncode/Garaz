using Garaaz.Models.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Garaaz.Models.Accounts
{
    public class UserManagement : IUserManagement
    {
        #region Variables
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        //private IRoleManagement _roleManagement;
        private ApplicationRoleManager _roleManager;
        #endregion

        #region Construtors

        public UserManagement(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

        }

        #endregion

        #region Get Current UserName

        public string GetCurrentUserName()
        {
            return HttpContext.Current.User.Identity.Name;
        }

        #endregion

        #region Get Current User By UserId

        public ApplicationUser GetCurrentUserById(string userId)
        {
            var Name = GetCurrentUserName();
            var user = _userManager.FindById(userId);
            return user;
        }

        #endregion

        #region Get Current UserId

        public async Task<string> GetCurrentUserId()
        {
            var Name = GetCurrentUserName();
            var user = await _userManager.FindByNameAsync(Name);
            return user.Id;
        }

        #endregion

        #region Get UserId By Username

        public string GetUserIdByUsername(string Username)
        {
            var user = _userManager.FindByName(Username);
            if (user != null)
                return user.Id;
            else
                return null;
        }

        public string GetUsernameByUserId(string UserId)
        {
            var user = _userManager.FindById(UserId);
            return user.UserName;
        }

        #endregion

        #region Register User
        public IdentityResult RegisterUser(string Email, string Password, string[] Roles, out string UserId)
        {
            UserId = "";
            var user = new ApplicationUser { UserName = Email, Email = Email };
            var result = _userManager.Create(user, Password);
            if (result.Succeeded)
            {
                UserId = user.Id;
                var r = _userManager.AddToRolesAsync(UserId, Roles);
                r.Wait();
            }
            return result;
        }

        public IdentityResult RegisterUser(string PhoneNumber, string Email, string Password, string[] Roles, bool IsApproved, out string UserId)
        {
            UserId = "";
            var user = new ApplicationUser { UserName = PhoneNumber, Email = Email, EmailConfirmed = IsApproved };
            var result = _userManager.Create(user, Password);
            if (result.Succeeded)
            {
                UserId = user.Id;
                if (Roles != null)
                {
                    var task = _userManager.AddToRolesAsync(UserId, Roles);
                    task.Wait();

                    // Save all permissions for user with DistributorUsers role only
                    var isDistUser = Array.Find(Roles, n => n == Constants.Users);
                    if (!string.IsNullOrEmpty(isDistUser))
                    {
                        var thread = new System.Threading.Thread(() => SavePermissionsForUser(user.Id, Roles))
                        { IsBackground = true };
                        thread.Start();
                    }
                }

            }
            return result;
        }

        public IdentityResult RegisterUser(string PhoneNumber, string Email, string Password, string[] Roles, bool IsApproved, string empCode, out string UserId, string distributorId = null)
        {
            UserId = "";
            RepoUsers repoUsers = new RepoUsers();
            var isExists = repoUsers.CheckEmpCodeExists(empCode, string.Empty,distributorId);
            if (isExists)
            {
                string[] errors = new[] { $"{empCode} employee code already exists." };
                var identityResult = new IdentityResult(errors);
                return identityResult;
            }
            var user = new ApplicationUser { UserName = PhoneNumber, Email = Email, EmailConfirmed = IsApproved };
            var result = _userManager.Create(user, Password);
            if (result.Succeeded)
            {
                UserId = user.Id;
                if (Roles != null)
                {
                    var task = _userManager.AddToRolesAsync(UserId, Roles);
                    task.Wait();

                    // Save all permissions for user with DistributorUsers role only
                    var isDistUser = Array.Find(Roles, n => n == Constants.Users);
                    if (!string.IsNullOrEmpty(isDistUser))
                    {
                        var thread = new System.Threading.Thread(() => SavePermissionsForUser(user.Id, Roles))
                        { IsBackground = true };
                        thread.Start();
                    }
                }

            }
            return result;
        }
        #endregion

        #region Lock/Unlock User

        public bool LockUserAccount(string userId)
        {
            var result = _userManager.SetLockoutEnabled(userId, true);
            if (result.Succeeded)
            {
                result = _userManager.SetLockoutEndDate(userId, DateTimeOffset.MaxValue);
            }
            return result.Succeeded;
        }

        public bool UnlockUserAccount(string userId)
        {
            var result = _userManager.SetLockoutEnabled(userId, false);
            if (result.Succeeded)
            {
                result = _userManager.ResetAccessFailedCount(userId);
            }
            return result.Succeeded;
        }

        #endregion

        #region Delete User

        public bool DeleteUserAccount(string userId)
        {
            var asp = GetCurrentUserById(userId);
            var result = _userManager.Delete(asp);
            return result.Succeeded;
        }

        #endregion

        #region Approve/DisApprove User

        public bool SetApproved(string Userid, bool Approved)
        {
            var user = GetCurrentUserById(Userid);
            user.EmailConfirmed = Approved;
            var result = _userManager.Update(user);
            return result.Succeeded;
        }

        #endregion

        #region Get Password Change Token
        public string GetPasswordChangeToken(string userId)
        {
            return _userManager.GeneratePasswordResetToken(userId);
        }
        #endregion

        #region Change Password
        public bool ChangePassword(string userId, string newPassword)
        {
            string code = GetPasswordChangeToken(userId);
            return ChangePassword(userId, newPassword, code);
        }

        public bool ChangePassword(string userId, string newPassword, string code)
        {
            var result = _userManager.ResetPassword(userId, code, newPassword);
            return result.Succeeded;
        }
        #endregion

        #region Update User Email Id
        public bool SetEmail(string Userid, string email)
        {
            var user = GetCurrentUserById(Userid);
            user.Email = email;
            var result = _userManager.Update(user);
            return result.Succeeded;
        }
        #endregion

        #region Get SuperAdmin Id
        public string GetSuperAdminRoleId()
        {
            return _roleManager.Roles.Where(a => a.Name == Constants.SuperAdmin).FirstOrDefault().Id;

        }
        #endregion

        #region Get Super Admin List
        public List<ApplicationUser> GetSuperAdminList()
        {
            var SuperAdminId = GetSuperAdminRoleId();

            return _userManager.Users.Where(a => a.Roles.Where(b => b.RoleId == SuperAdminId).Count() > 0).ToList();
        }
        #endregion

        /// <summary>
        /// Save all default permissions, by role, for newly created user.
        /// </summary>        
        /// <param name="userId">The user Id of the user.</param>
        public static void SavePermissionsForUser(string userId, string[] roles)
        {
            var db = new garaazEntities();

            // Get all 'default' features set for passed roles            
            var featureIdsByRole = db.FeaturesRoles.Where(f => roles.Contains(f.AspNetRole.Name)).Select(r => r.FeatureId);
            var features = db.Features.Where(f => featureIdsByRole.Contains(f.FeatureId) && f.IsDefault).ToList();

            if (features.Any())
            {
                var userFeatures = features.Select(f => new UserFeature
                {
                    FeatureId = f.FeatureId,
                    UserId = userId,
                    Feature = true
                }).ToList();
                db.UserFeatures.AddRange(userFeatures);
                db.SaveChanges();
            }
        }

        #region update user name
        public IdentityResult UpdateUserName(string userId, string userName)
        {
            var user = GetCurrentUserById(userId);
            user.UserName = userName;
            var result = _userManager.Update(user);
            return result;
        }
        #endregion
    }
}