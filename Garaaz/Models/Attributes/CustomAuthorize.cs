using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Garaaz.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedFeatures;
        public CustomAuthorizeAttribute(params string[] features)
        {
            allowedFeatures = features;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;

            if (httpContext.User.Identity.IsAuthenticated)
            {
                var isWorkshop = httpContext.User.IsInRole(Constants.Workshop);
                var isWorkshopUser = httpContext.User.IsInRole(Constants.WorkshopUsers);

                if (isWorkshop || isWorkshopUser) return true;

                var isDistUser = httpContext.User.IsInRole(Constants.Users);
                if (isDistUser)
                {
                    var db = new garaazEntities();
                    var user = db.AspNetUsers.FirstOrDefault(u => u.UserName == httpContext.User.Identity.Name);

                    if (user != null)
                    {
                        foreach (var af in allowedFeatures)
                        {
                            var feature = db.Features.FirstOrDefault(f => f.FeatureValue.Equals(af));
                            if (feature == null) continue;

                            var userFeature = db.UserFeatures.FirstOrDefault(u => u.UserId == user.Id && u.FeatureId == feature.FeatureId && u.Feature == true); 
                            authorize = userFeature != null ? true : false;
                        }
                    }
                }
                else
                {
                    // For other roles, by pass and authorize them
                    authorize = true;
                }
            }

            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //filterContext.Result = new HttpUnauthorizedResult();

            filterContext.Result = new RedirectToRouteResult(
                                  new RouteValueDictionary
                                  {
                                       { "action", "UnAuthorize" },
                                       { "controller", "Home" }
                                  });
        }
    }
}