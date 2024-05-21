using System.Collections.Generic;

namespace Garaaz.Models.AppNotification
{
    public class FCMResponse
    {
        public long multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public List<Errors> results { get; set; }
    }
    public class Errors
    {
        public string error { get; set; }
    }
    public class PushNotification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public List<string> FCMToken { get; set; }
    }
    public class PushNotificationUserModel
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public string OrderNumber { get; set; }
        public int SchemeId { get; set; }
    }
}