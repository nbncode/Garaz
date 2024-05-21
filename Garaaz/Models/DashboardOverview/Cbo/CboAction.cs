namespace Garaaz.Models.DashboardOverview.Cbo
{
    /// <summary>
    /// Action to be used for getting customer back orders using stored procedure.
    /// </summary>
    public enum CboAction
    {
        CboByRoWise,
        CboBySeWise,
        CboSum,
        CancelledCboSum,

        RoWiseCboDetail,
        RoWiseBranchCboDetail,
        RoWiseBranchCustomerCboDetail,
        RoWiseCustomerCboDetail,
        RoWiseCustomerDetailByCustomerCode,
        RoWiseCustomerPartsDetail,

        SeWiseCboDetail,
        SeWiseBranchCboDetail,
        SeWiseBranchCustomerCboDetail,
        SeWiseCustomerCboDetail,
        SeWiseCustomerDetailByCustomerCode,
        SeWiseCustomerPartsDetail
    }
}