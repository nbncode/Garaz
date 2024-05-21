using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RepoUserFeaturePermission
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region User Feature

        #region GetAllFeatures
        public List<Feature> GetAllFeatures()
        {
            return db.Features.ToList();
        }
        #endregion

        #region GetAllFeaturebyuserId
        public List<UserFeature> getUserFeatures(string userId)
        {
            return db.UserFeatures.Where(x => x.UserId == userId && x.Feature.HasValue && x.Feature.Value).ToList();
        }
        #endregion

        #region SaveUserFeatures
        public bool SaveUserFeatures(List<UserFeature> modelList, string userId)
        {
            var ExistsData = db.UserFeatures.Where(x => x.UserId == userId).ToList();
            if (ExistsData.Count > 0)
            {
                ExistsData.ForEach(a => a.Feature = false);
                db.SaveChanges();
            }
            if (modelList.Count > 0)
            {
                foreach (var item in modelList)
                {

                    if (ExistsData.Where(a => a.FeatureId == item.FeatureId && a.UserId == item.UserId).Count() > 0)
                    {
                        var cur = ExistsData.Where(a => a.FeatureId == item.FeatureId && a.UserId == item.UserId).FirstOrDefault();
                        cur.Feature = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        item.Feature = true;
                        db.UserFeatures.Add(item);
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        #endregion

        #endregion

        #region Features

        #region SaveFeatures
        public bool SaveFeatures(Feature model)
        {

            var old = db.Features.Where(m => m.FeatureId == model.FeatureId).FirstOrDefault();
            if (old != null)
            {
                old.FeatureName = model.FeatureName;
                old.FeatureValue = model.FeatureValue;
                old.IsDefault = model.IsDefault;
                return db.SaveChanges() > 0;
            }
            else
            {
                db.Features.Add(model);
                return db.SaveChanges() > 0;
            }
        }
        #endregion

        #region Get Features by Id
        public Feature getFeaturesNamebyId(int FeatureId)
        {

            return db.Features.Where(x => x.FeatureId == FeatureId).FirstOrDefault();

        }
        #endregion

        #region Get features by role
        /// <summary>
        /// Get features filtered by role.
        /// </summary>        
        public List<Feature> GetFeatureByRole(IEnumerable<string> roles, List<Feature> allFeatures)
        {
            var aspNetRoleIds = db.AspNetRoles.Where(r => roles.Contains(r.Name)).Select(r => r.Id);
            if (aspNetRoleIds.Any())
            {
                var featureIds = db.FeaturesRoles.Where(f => aspNetRoleIds.Contains(f.RoleId)).Select(r => r.FeatureId).ToList();
                return allFeatures.Where(af => featureIds.Contains(af.FeatureId)).ToList();
            }
            return new List<Feature>();
        }
        #endregion

        #region Delete

        public bool DeleteFeatures(int id)
        {
            var FeatureRoles = db.FeaturesRoles.Where(x => x.FeatureId == id).ToList();
            if (FeatureRoles == null)
            {
                db.FeaturesRoles.RemoveRange(FeatureRoles);
            }
            var data = db.Features.Where(a => a.FeatureId == id).FirstOrDefault();
            if (data == null)
            {
                return false;
            }
            else
            {
                db.Features.Remove(data);
                return db.SaveChanges() > 0;
            }
        }

        #endregion

        #endregion

    }
}