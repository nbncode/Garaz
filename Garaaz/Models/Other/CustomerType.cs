namespace Garaaz.Models
{
    /// <summary>
    /// Represent customer types.
    /// </summary>
    public class CustomerType
    {
        public static string CoDealer => "CO-DEALER";
        public static string Distributor => "DISTRIBUTOR";
        public static string Mass => "MASS";
        public static string Trader => "TRADER/RETAILER";
        public static string Workshop => "INDEPENDENT WORKSHOP";
        public static string WalkInCustomer => "WALK-IN CUSTOMER";
    }
}