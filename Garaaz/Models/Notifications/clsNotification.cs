using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models.Notifications
{
    public class ClsNotification
    {
        /// <summary>
        /// Get or set the logged in user id.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Get or set the number of notification required.
        /// </summary>
        public int NumberOfNotification { get; set; }

        /// <summary>
        /// Get or set whether to include all notifications.
        /// </summary>
        public bool GetAllNotification { get; set; }        
    }
    public class NotificationPagination
    {
        public string SearchText { get; set; }
        public int PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string UserId { get; set; }
    }
}