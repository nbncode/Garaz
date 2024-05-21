using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models.DashboardOverview.LoserAndGainer
{
    /// <summary>
    /// Action to be used for getting customers using stored procedure.
    /// </summary>
    public enum LGAction
    {
        // Used for mainDashboard
        LooserGainerInfo,

        // Used for Sub Group by
        LooserGainerByRoWise,
        LooserGainerBySeWise,
        LooserGainerByCsWise,

        // Used for Ro by
        RoWiseLGDetails,
        RoWiseBranchDetails,
        ROGainerCustomerDetail,
        ROLooserCustomerDetail,

        // Used for Se by
        SeWiseLGDetails,
        SeWiseBranchDetails,
        SeGainerCustomerDetail,
        SeLooserCustomerDetail,

        // Used for Cs by
        CsWiseLGDetails,
        CsGainerCustomerDetail,
        CsLooserCustomerDetail
    }
}