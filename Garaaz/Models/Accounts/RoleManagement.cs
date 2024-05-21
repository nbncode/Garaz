using Garaaz.Models.Services;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Garaaz.Models.Accounts
{
    public class RoleManagement : IRoleManagement
    {
        #region Variables
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _applicationUserManager;
        private IUserManagement _userManager;
        private ApplicationRoleManager _roleManager;
        #endregion

        #region Construtors

        public RoleManagement(ApplicationUserManager applicationUserManager, ApplicationSignInManager signInManager, IUserManagement userManager, ApplicationRoleManager roleManager)
        {
            _applicationUserManager = applicationUserManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #endregion

        #region Is SuperAdmin

        public bool IsSuperAdmin(ApplicationUser user)
        {
            var rolesAll = _applicationUserManager.GetRolesAsync(user.Id);
            rolesAll.Wait();
            return rolesAll.Result.Where(a => a.ToString() == Constants.SuperAdmin).Count() > 0;
        }

        #endregion

        #region Is Distributor 
        public bool IsDistributor(ApplicationUser user)
        {
            var rolesAll = _applicationUserManager.GetRolesAsync(user.Id);
            rolesAll.Wait();
            return rolesAll.Result.Where(a => a.ToString() == Constants.Distributor).Count() > 0;
        }
        #endregion

        #region Add User To Role
        public async Task<bool> AddUserToRole(string userId, string[] roles)
        {
            var result = await _applicationUserManager.AddToRolesAsync(userId, roles);
            return result.Succeeded;
        }
        #endregion

        #region Get All Roles
        public List<RolesData> GetAllRoles()
        {
            return _roleManager.Roles.Select(a => new RolesData()
            {
                Id = a.Id,
                Name = a.Name
            }).ToList();
        }
        #endregion

        #region Update User Role
        public bool UpdateUserRole(string userId, string[] roles)
        {
            var currentRoles = _applicationUserManager.GetRolesAsync(userId);
            currentRoles.Wait();
            string combindedString = string.Join(",", currentRoles.Result.ToArray());
            var removeRole = _applicationUserManager.RemoveFromRolesAsync(userId, combindedString.Split(','));
            removeRole.Wait();
            var result = _applicationUserManager.AddToRolesAsync(userId, roles);
            result.Wait();
            return result.Result.Succeeded;
        }
        #endregion

        #region Get Role
        public string GetRole(string userId)
        {
            var rolesAll = _applicationUserManager.GetRolesAsync(userId);
            rolesAll.Wait();
            return rolesAll.Result.FirstOrDefault();
        }
        #endregion
    }
}