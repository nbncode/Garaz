namespace Garaaz.Models.DashboardOverview.Collection
{
    /// <summary>
    /// Action to be used for getting collections using stored procedure.
    /// </summary>
    public enum CollectionAction
    {
        CollectionByRoWise,
        CollectionBySeWise,
        CollectionByCsWise,

        RoWiseCollection,
        RoWiseBranchCollection,
        RoWiseBranchCustomersByType,
        RoWiseBranchCustomers,

        SeWiseCollection,
        SeWiseBranchCollection,
        SeWiseBranchCustomersByType,
        SeWiseBranchCustomers,

        CsWiseCollection,
        CsWiseCustomersByType,

        // Common for RO, SE and CS
        CustomerDetail
    }
}