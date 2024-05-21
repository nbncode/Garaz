namespace Garaaz.Models.DashboardOverview.Wallet
{
    /// <summary>
    /// Action to be used for getting wallet details using stored procedure.
    /// </summary>
    public enum WalletAction
    {
        TotalWalletBalance,
        WalletByRoWise,
        WalletBySeWise,
        WalletByCsWise,

        RoWiseWallet,
        RoWiseBranchWallet,
        RoWiseBranchCustomersByType,
        RoWiseBranchCustomers,

        SeWiseWallet,
        SeWiseBranchWallet,
        SeWiseBranchCustomersByType,
        SeWiseBranchCustomers,

        CsWiseWallet,
        CsWiseCustomersByType,

        // Common for RO, SE and CS
        CustomerDetail
    }
}