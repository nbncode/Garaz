using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models.Notifications
{
    /// <summary>
    /// This class handle notifications related tasks.
    /// </summary>
    public class NotificationUtil
    {
        private readonly garaazEntities _db = new garaazEntities();

        /// <summary>
        /// Save notification message for Super admins in 'Notifications' table.
        /// </summary>
        /// <param name="role">The role of the user.</param>
        /// <param name="refUserId">The reference user id of the user who is registering.</param>
        /// <param name="workshopId">The workshop id of the workshop.</param>
        /// <param name="superAdmins">The list of super admin users.</param>
        public void SaveNotification(string role, string refUserId, int? workshopId, List<ApplicationUser> superAdmins)
        {
            // Set type and message for registration
            var type = string.Empty;
            var message = string.Empty;
            switch (role)
            {
                case "Distributor":
                    type = NotificationType.NewRegisterDistributor.ToString();
                    message = "New distributor registered.";
                    break;

                case "Workshop":
                    type = NotificationType.NewRegisterWorkshop.ToString();
                    message = "New workshop registered.";
                    break;
            }

            // Save notification for each super admin
            foreach (var sa in superAdmins)
            {
                var notification = new Notification
                {
                    UserId = sa.Id,
                    Type = type,
                    Message = message,
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    RefUserId = refUserId,
                    WorkshopId = workshopId
                };
                _db.Notifications.Add(notification);
            }

            _db.SaveChanges();
        }

        /// <summary>
        /// Get total count of unread notifications.
        /// </summary>
        /// <returns>Return unread notification count.</returns>
        public int TotalUnreadNotifications()
        {
            var unreadCount = (from n in _db.Notifications
                               where n.IsRead == false
                               select n).Count();

            return unreadCount;
        }

        /// <summary>
        /// Get notifications based on parameter value passed.
        /// </summary>
        /// <param name="userId">The reference user id of the logged in user.</param>
        /// <param name="numberOfNotifications">The top number of notifications to be returned.</param>
        /// <param name="getAll">Specify whethe to return all notifications.</param>
        /// <returns>Return list of notifications.</returns>
        public List<NotificationData> GetNotifications(string userId, int numberOfNotifications, bool getAll)
        {
            List<Notification> notifications;

            if (getAll)
            {
                notifications = _db.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedDate).ToList();
                foreach (var no in notifications)
                {
                    MarkNotificationRead(no.Id, userId);
                }
            }
            else
            {
                notifications = (from n in _db.Notifications
                                 where n.UserId == userId
                                 orderby n.CreatedDate descending
                                 select n).Take(numberOfNotifications).ToList();
            }

            // Convert to list of other type so as to fix the issue- Self referencing loop detected for property 'AspNetUser'
            var utils = new Utils();
            return notifications.Select(n => new NotificationData
            {
                Id = n.Id,
                RefUserId = n.RefUserId,
                WorkshopId = Convert.ToInt32(n.WorkshopId),
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedDate = utils.getTime(n.CreatedDate)
            }).ToList();
        }

        /// <summary>
        /// Mark notification for particular user as read.
        /// </summary>
        /// <param name="notificationId">The notification id of the notification.</param>
        /// <param name="userId">The user id of the user.</param>
        /// <returns>Return true if notification is marked as read.</returns>
        public bool MarkNotificationRead(int notificationId, string userId)
        {
            var isMarkRead = false;

            var notification = (from n in _db.Notifications
                                where n.Id == notificationId && n.UserId == userId
                                select n).FirstOrDefault();

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                isMarkRead = _db.SaveChanges() > 0;
            }

            return isMarkRead;
        }

        /// <summary>
        /// Get workshop model including user details and list of distributors.
        /// </summary>
        /// <param name="refUserId">The reference user id.</param>
        /// <param name="workshopId">The workshop id.</param>
        /// <returns>Return ClsNewWorkshop object.</returns>
        public ClsNewWorkshop GetWorkshopModel(string refUserId, int workshopId)
        {
            RepoUsers repoUsers = new RepoUsers();

            // Get user detail
            var userDetail = repoUsers.getUserById(refUserId);

            // Get list of distributor Ids for particular workshop and user
            var distributors = repoUsers.GetAllDistributorsNew();
            var distIds = (from d in _db.DistributorWorkShops
                           where d.WorkShopId == workshopId && d.UserId == refUserId
                           select d.DistributorId).ToList();


            // Get list of distributors    
            var selectList = new List<SelectListItem>();
            foreach (var item in distributors)
            {
                if (distIds.Contains(item.DistributorId))
                {
                    // Selected is read only property and therefore should be set during initialization
                    selectList.Add(new SelectListItem
                    {
                        Text = item.DistributorName,
                        Value = item.DistributorId.ToString(),
                        Selected = true
                    });
                }
                else
                {
                    selectList.Add(new SelectListItem
                    {
                        Text = item.DistributorName,
                        Value = item.DistributorId.ToString()
                    });
                }
            }

            var clsNewWorkshop = new ClsNewWorkshop
            {
                RefUserId = refUserId,
                WorkshopId = workshopId,
                UserDetail = userDetail,
                Distributors = selectList
            };

            return clsNewWorkshop;
        }

        #region Get All Notifications paging
        public List<NotificationData> GetAllNotifications(NotificationPagination model, out int totalRecords)
        {
            var notifications = model.UserId != null ? _db.Notifications.Where(n => n.UserId == model.UserId).OrderByDescending(n => n.CreatedDate).ToList() : _db.Notifications.OrderByDescending(n => n.CreatedDate).ToList();
            totalRecords = notifications.Count;
            if (model.PageNumber > 0)
            {
                notifications = notifications.GetPaging(model.PageNumber, model.PageSize);
            }
            var utils = new Utils();
            return notifications.Select(n => new NotificationData
            {
                Id = n.Id,
                RefUserId = n.RefUserId,
                WorkshopId = Convert.ToInt32(n.WorkshopId),
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedDate = utils.getTime(n.CreatedDate)
            }).ToList();
        }
        #endregion

        /// <summary>
        /// Save notification for gift allocated to workshop.
        /// </summary>
        /// <param name="couponNumber">The coupon number that will be used to get workshop.</param>
        /// <param name="schemeName">Name of the scheme.</param>
        /// <param name="giftName">Name of the gift.</param>
        internal void SaveNotification(string couponNumber, string schemeName, string giftName)
        {
            var coupon = _db.Coupons.FirstOrDefault(c => c.CouponNumber == couponNumber);
            if (coupon != null)
            {
                var wsId = coupon.WorkshopId;
                var user = (from u in _db.UserDetails
                            join d in _db.DistributorWorkShops on u.UserId equals d.UserId
                            where d.WorkShopId == wsId
                            select u).FirstOrDefault();

                if (user != null)
                {
                    var notification = new Notification
                    {
                        UserId = user.UserId,
                        Type = NotificationType.GiftAllocated.ToString(),
                        Message = $"You have won {giftName} in {schemeName} scheme.",
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        WorkshopId = wsId
                    };
                    _db.Notifications.Add(notification);
                    _db.SaveChanges();
                }
            }
        }
    }
}