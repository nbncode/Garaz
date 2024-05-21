namespace Garaaz.Models.DashboardOverview.Customer
{
    /// <summary>
    /// Action to be used for getting customers using stored procedure.
    /// </summary>
    public enum CustomerAction
    {
        AllCustomers,

        CustomerByRoWise,
        CustomerBySeWise,

        RoWiseCustomer,
        RoWiseBranchCustomer,
        RoWiseBilledCustomers,
        RoWiseNonBilledCustomers,

        SeWiseCustomer,
        SeWiseBranchCustomer,
        SeWiseBilledCustomers,
        SeWiseNonBilledCustomers
    }
}