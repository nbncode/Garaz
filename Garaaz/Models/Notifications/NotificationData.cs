namespace Garaaz.Models.Notifications
{
    public class NotificationData
    {
        public int Id { get; set; }        

        public string UserId { get; set; }

        /// <summary>
        /// The user id of the user who gets registered.
        /// </summary>
        public string RefUserId { get; set; }

        public int WorkshopId { get; set; }

        public string Type { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        public string CreatedDate { get; set; }
    }
}