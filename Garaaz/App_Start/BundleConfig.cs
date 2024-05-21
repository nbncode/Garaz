using System.Web.Optimization;

namespace Garaaz
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;

            #region For _LayoutFront.cshtml

            bundles.Add(new StyleBundle("~/Content/dependencyCss").Include(
                "~/Content/plugins/SpartanMB-Bold/styles.css",
                "~/Content/plugins/SpartanMB-Thin/styles.css",
                "~/Content/css/bootstrap.min.css",
                "~/Content/css/animate.css",
                "~/Content/css/magnific-popup.css",
                "~/Content/css/font-awesome.min.css",
                "~/Content/css/jquery.bxslider.min.css",
                "~/Content/css/hover-min.css",
                "~/Content/plugins/bands-icon/style.css",
                "~/Content/plugins/carevan-icon/style.css",
                "~/Content/css/owl.carousel.css",
                "~/Content/css/owl.theme.default.min.css",
                "~/Content/css/bootstrap-select.min.css"
            ));

            bundles.Add(new StyleBundle("~/Content/mainCss").Include(
                "~/Content/css/style.css",
                "~/Content/css/updated.css",
                "~/Content/css/responsive.css",
                "~/Content/css/updated-responsive.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/LayoutFront/jquery.js",
                //"~/Scripts/LayoutFront/gmaps.js",
                "~/Scripts/LayoutFront/map-helper.js",
                "~/Scripts/LayoutFront/bootstrap.bundle.min.js",
                "~/Scripts/LayoutFront/jquery.magnific-popup.min.js",
                "~/Scripts/LayoutFront/owl.carousel.min.js",
                "~/Scripts/LayoutFront/isotope.js",
                "~/Scripts/LayoutFront/bootstrap-select.min.js",
                "~/Scripts/LayoutFront/waypoints.min.js",
                "~/Scripts/LayoutFront/jquery.bxslider.min.js",
                "~/Scripts/LayoutFront/jquery.counterup.min.js",
                "~/Scripts/LayoutFront/wow.min.js",
                "~/Scripts/LayoutFront/theme.js"
            ));

            #endregion

            bundles.Add(new ScriptBundle("~/bundles/approveRequest-js").Include(
                "~/Scripts/jquery.autocomplete.js",
                "~/js/approveRequest.js"
            ));
        }
    }
}
