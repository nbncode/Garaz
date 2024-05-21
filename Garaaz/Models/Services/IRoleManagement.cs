using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Garaaz.Models.Services
{
    public interface IRoleManagement
    {
        bool IsSuperAdmin(ApplicationUser user);
        bool IsDistributor(ApplicationUser user);
        Task<bool> AddUserToRole(string userId, string[] roles);
        List<RolesData> GetAllRoles();
        bool UpdateUserRole(string userId, string[] roles);
        string GetRole(string userId);
    }
}