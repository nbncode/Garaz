using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class FeatureRolemodel 
    {
        public int Id { get; set; }
        public int? FeatureId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsChecked { get; set; }
        public List<FeatureRolemodel> FeatureRoleList { get; set; }
    }
    public class RepoFeaturesRole
    {
        #region Variables  
        garaazEntities db = new garaazEntities();
        #endregion

        #region Save Features Role
        public bool SaveFeatures(int featureId, List<FeaturesRole> FeaturesRolesList)
        {
            // Delete existing record for Features
            var Items = db.FeaturesRoles.Where(r => r.FeatureId == featureId);
            if (Items.Any())
            {
                db.FeaturesRoles.RemoveRange(Items);
                db.SaveChanges();
            }

            // Now save new record
            if (FeaturesRolesList.Any())
            {
                db.FeaturesRoles.AddRange(FeaturesRolesList);
                return db.SaveChanges() > 0;
            }
            return false;
        }
        #endregion]

        #region Get Rolelist By FeatureId
        public List<string> GetRoleListByFeatureId(int featureId)
        {
            return db.FeaturesRoles.Where(r => r.FeatureId == featureId).Select(r => r.RoleId.ToString()).ToList();
        }
        #endregion
        public List<FeatureRolemodel> GetFeaturesRoles(int FeatureId)
        {
            var lstFeatureRoles = new List<FeatureRolemodel>();
            var Roles = db.AspNetRoles.Where(r => r.Id != "1".ToString() || r.Name!= "SuperAdmin".ToString()).ToList();
            if (Roles != null)
            {
                foreach (var item in Roles)
                {
                    FeatureRolemodel model = new FeatureRolemodel();
                    var FeatureRoles = db.FeaturesRoles.Where(x => x.RoleId ==item.Id && x.FeatureId == FeatureId).FirstOrDefault();
                    model.RoleId = item.Id;
                    model.RoleName = item.Name;
                    model.IsChecked = FeatureRoles != null ? true : false;
                    lstFeatureRoles.Add(model);
                }
                
            }
            return lstFeatureRoles;
        }
    }
}