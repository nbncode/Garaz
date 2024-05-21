namespace Garaaz.Models.DashboardOverview.Outstanding
{
    /// <summary>
    /// Action to be used for getting outstanding details using stored procedure.
    /// </summary>

    public enum OsAction
    {
        RoWiseTotalOs,
        RoWiseOsDetails,
        RoWiseBranchOs,
        RoWiseOsByBranch,
        RoWiseCustomerOs,
        RoWiseCustomerOsWithSearch,

        SeWiseTotalOs,
        SeWiseOsDetails,
        SeWiseBranchOs,
        SeWiseOsByBranch,
        SeWiseCustomerOs,
        SeWiseCustomerOsWithSearch,

        CsWiseTotalOs,
        CsWiseOsDetails,
        CsWiseCustomerOs,
        CsWiseCustomerOsWithSearch
    }
}