namespace Garaaz.Models.Notifications
{
    /// <summary>
    /// Type of notifications.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// New distributor registered.
        /// </summary>
        NewRegisterDistributor,
        /// <summary>
        /// New workshop registered.
        /// </summary>
        NewRegisterWorkshop,
        /// <summary>
        /// Allocated gift to workshop.
        /// </summary>
        GiftAllocated
    }
}