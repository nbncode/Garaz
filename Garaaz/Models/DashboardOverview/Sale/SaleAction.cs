namespace Garaaz.Models.DashboardOverview.Sale
{
    /// <summary>
    /// Action to be used for getting sale details using stored procedure.
    /// </summary>
    public enum SaleAction
    {
        RoWiseSaleDetail,
        RoWiseBranchSaleDetail,
        RoWiseCustomerSaleDetail,
        RoWiseCustomerSaleDetailWithSearch,

        SeWiseSaleDetail,
        SeWiseBranchSaleDetail,
        SeWiseCustomerSaleDetail,
        SeWiseCustomerSaleDetailWithSearch,

        CsWiseSaleDetails,
        CsWiseCustomerSaleDetails,
        CsWiseCustomerSaleDetailsWithSearch,

        PgWiseSaleDetail,
        PgWiseCustomerSaleDetail,
        PgWiseBranchSaleDetail,
        PgWiseCustomerSale,
        PgWiseCustomerSaleWithSearch
    }
}