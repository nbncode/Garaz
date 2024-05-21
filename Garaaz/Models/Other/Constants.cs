using System.Collections.Generic;
using System.Configuration;

namespace Garaaz.Models
{
    public class Constants
    {
        #region Roles
        public static string SuperAdmin = "SuperAdmin";
        public static string Distributor = "Distributor";
        public static string DistributorOutlets = "DistributorOutlets";
        public static string Users = "DistributorUsers";
        public static string Workshop = "Workshop";
        public static string SalesExecutive = "SalesExecutive";
        public static string RoIncharge = "RoIncharge";
        public static string OutletUsers = "OutletUsers";
        public static string WorkshopUsers = "WorkshopUsers";
        public static string Outlet = "Outlet";

        #endregion

        #region Cookies Id
        public const string UsernameSession = "UsernameSession";
        public const string UserFullnameSession = "UserFullnameSession";
        public const string UserIdSession = "UserIdSession";
        public const string UserRoleSession = "UserRoleSession";
        public const string UserRefreshTokenSession = "UserRefreshTokenSession";
        public const string UserTokenSession = "UserTokenSession";
        public const string TempOrderId = "TempOrderId";
        #endregion

        #region Other
        public static string TokenApi = ConfigurationManager.AppSettings["WebUrl"] + "/token";
        public static string Sign = "₹";
        public static decimal FValue = 75;
        public static decimal MValue = 15;
        public static decimal SValue = 10;
        public static string UPIID = "UPI121";

        public static string SelfPickup = "Self Pickup";
        public static string ExpressDelivery = "Express Delivery";
        public static string ArrageItem = "Arrage Item";
        public static string OrderToMSIL = "Order To MSIL";

        public static string StockTypeLow = "Low";
        public static string StockTypeMediumn = "Medium";
        public static string StockTypeHigh = "High";
        public static int StockTypeLowQty = 5;
        public static int StockTypeMediumnQty = 10;

        public static string DashboardHeading = "Clear Outstanding Amount";
        public static string DashboardDescription = "Lorem Ipsum is simply dummy";
        public static string RupeeSign = "₨";
        #endregion

        #region Scheme Levels

        public static string Categories = "Categories";
        public static string GiftManagement = "Gift Management";
        public static string AssuredGift = "Assured Gift";
        public static string Cashback = "Cashback";
        public static string QualifyingCriteria = "Qualifying Criteria";
        public static string TargetWorkShop = "TargetWorkShop";
        public static string FocusPart = "Focus Part";
        public static string Setleveltitle = "Set level title";

        #endregion

        #region MailTemplate
        public static string PlaceOrder = "PlaceOrder";
        public static string PlaceFailure = "PlaceFailure";
        public static string OrderCancel = "OrderCancel";
        public static string OrderConfirmed = "OrderConfirmed";
        public static string OrderOntheWay = "OrderOntheWay";
        public static string OrderDelivered = "OrderDelivered";
        public static string OrderCompleted = "OrderCompleted";
        public static string StatusChange = "StatusChange";
        public static string BackOrderCancel = "BackOrderCancel";
        public static string OtpSent = "OtpSent";
        #endregion

        #region ImportHeading
        public static string WorkshopImport = "Workshop file upload";
        public static string DailyStockImport = "Daily stocks file upload ";
        public static string DailySalesTrackerImport = "Daily sales tracker file upload ";
        public static string BackOrderImport = "Customer backorder file upload ";
        public static string OutstandingImport = "Outstanding file upload ";
        public static string ProductImport = "Product file upload ";
        public static string AccountLedgerImport = "Account ledger file upload ";
        public static string OutletImport = "Outlet file upload";
        public static string RoInchargeImport = "RO Incharge file upload";
        public static string SalesExecutiveImport = "Sales Executive file upload";
        public static string RequestPartfilterImport = "Request Part Filter file upload";

        #endregion

        #region Import Part Category Name
        public static string M = "Parts";
        public static string AA = "Accessories";
        public static string AG = "Oil";
        public static string T = "Tools";

        public static string MFull = "Maruti Suzuki Genuine Parts";
        public static string AAFull = "Maruti Suzuki Genuine Accessories";
        public static string AGFull = "Maruti Suzuki Genuine Oil";
        public static string TFull = "Tools";

        #endregion

        #region Account Ledger Filter Particulars
        public static string ClosingBalance = "Closing Balance";
        public static string OpeningBalance = "Opening Balance";
        #endregion

        #region Notification Type

        public static string RejectBackOrder = "RejectBackOrder";
        public static string AcceptBackOrder = "AcceptBackOrder";
        public static string RejectMainOrder = "RejectMainOrder";
        public static string NewOrderPlaced = "NewOrderPlaced";
        public static string SupportType = "Support";
        public static string OutstandingPayment = "OutstandingPayment";
        public static string ItemAddtocart = "ItemAddtoCart";
        public static string PartRequestPlaced = "PartRequestPlaced";

        #endregion

        public const string ImportStatus = "All records were imported successfully!";

        #region Dashboard sale filter CO-DEALER/CO-DISTRIBUTOR
        public static string CoDealerDistributor = "CO-DEALER,DISTRIBUTOR,CO-DISTRIBUTOR";
        
        #endregion
    }

    public class DistributerUserMenues
    {
        #region Distributer Users Menue's

        public static string Outlets = "Outlets";
        public static string Users = "Users";
        public static string SalesExecutives = "SalesExecutives";
        public static string ROIncharge = "ROIncharge";
        public static string Workshop = "Workshop";
        public static string Schemes = "Schemes";
        public static string LocationCode = "LocationCode";
        public static string ShowSales = "ShowSales";
        public static string CurrentOrder = "CurrentOrder";
        public static string Updates = "MgaBanner";
        public static string ProductGroup = "ProductGroup";
        public static string Product = "Product";
        public static string BackOrders = "CustomerBackOrders";
        public static string NewPartRequests = "NewPartRequest";

        #endregion
    }
}