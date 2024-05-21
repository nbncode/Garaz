using Garaaz.Models.DistributorUsers;
using System.Collections.Generic;
using System.Linq;

namespace Garaaz.Models.Users.DistributorUsers
{
    public class RepoUserList
    {
        private readonly garaazEntities _db = new garaazEntities();

        private List<DistributorUserModel> SetOutlet(List<DistributorUserModel> userList)
        {

            // For Workshops
            var workshop = userList.Where(u => u.Role == Constants.Workshop).ToList();
            userList = userList.Where(u => u.Role != Constants.Workshop).ToList();
            if (workshop.Any())
            {
                workshop = (from a in workshop
                            join b in _db.DistributorWorkShops
                            on a.UserId equals b.UserId
                            join c in _db.WorkShops
                            on b.WorkShopId equals c.WorkShopId
                            select new DistributorUserModel
                            {
                                UserId = a.UserId,
                                FirstName = a.FirstName,
                                LastName = a.LastName,
                                UserName = a.UserName,
                                ConsPartyCode = a.ConsPartyCode,
                                IsDeleted = a.IsDeleted,
                                OTP = a.OTP,
                                Address = a.Address,
                                EmailConfirmed = a.EmailConfirmed,
                                LockoutEndDateUtc = a.LockoutEndDateUtc,
                                Role = a.Role,
                                DistributorId = a.DistributorId,
                                OutletCode = c?.Outlet != null ? c.Outlet.OutletCode : "",
                                OutletName = c?.Outlet != null ? c.Outlet.OutletName : "",
                            }).ToList();
                userList.AddRange(workshop);
            }

            // For RoIncharge
            var roIncharge = userList.Where(u => u.Role == Constants.RoIncharge).ToList();
            userList = userList.Where(u => u.Role != Constants.RoIncharge).ToList();
            if (roIncharge.Any())
            {
                roIncharge = (from ro in roIncharge
                              join d in _db.DistributorsOutlets
                                  on new { ro.UserId, ro.DistributorId } equals new { d.UserId, d.DistributorId }
                              select new DistributorUserModel
                              {
                                  UserId = ro.UserId,
                                  FirstName = ro.FirstName,
                                  LastName = ro.LastName,
                                  UserName = ro.UserName,
                                  ConsPartyCode = ro.ConsPartyCode,
                                  IsDeleted = ro.IsDeleted,
                                  OTP = ro.OTP,
                                  Address = ro.Address,
                                  EmailConfirmed = ro.EmailConfirmed,
                                  LockoutEndDateUtc = ro.LockoutEndDateUtc,
                                  Role = ro.Role,
                                  DistributorId = ro.DistributorId,
                                  OutletCode = d?.Outlet != null ? d.Outlet.OutletCode : "",
                                  OutletName = d?.Outlet != null ? d.Outlet.OutletName : "",
                              }).ToList();
                userList.AddRange(roIncharge);
            }

            // For SalesExecutives
            var salesExecutives = userList.Where(u => u.Role == Constants.SalesExecutive).ToList();
            userList = userList.Where(u => u.Role != Constants.SalesExecutive).ToList();
            if (salesExecutives.Any())
            {
                salesExecutives = (from se in salesExecutives
                                   join p in _db.RoSalesExecutives
                                       on se.UserId equals p.SeUserId
                                   join d in _db.DistributorsOutlets
                                       on p.RoUserId equals d.UserId
                                   where se.DistributorId == d.DistributorId
                                   select new DistributorUserModel
                                   {
                                       UserId = se.UserId,
                                       FirstName = se.FirstName,
                                       LastName = se.LastName,
                                       UserName = se.UserName,
                                       ConsPartyCode = se.ConsPartyCode,
                                       IsDeleted = se.IsDeleted,
                                       OTP = se.OTP,
                                       Address = se.Address,
                                       EmailConfirmed = se.EmailConfirmed,
                                       LockoutEndDateUtc = se.LockoutEndDateUtc,
                                       Role = se.Role,
                                       DistributorId = se.DistributorId,
                                       OutletCode = d?.Outlet != null ? d.Outlet.OutletCode : "",
                                       OutletName = d?.Outlet != null ? d.Outlet.OutletName : "",
                                   }).ToList();
                userList.AddRange(salesExecutives);
            }

            // For Outlet Users
            var outletUsers = userList.Where(u => u.Role == Constants.OutletUsers).ToList();
            userList = userList.Where(u => u.Role != Constants.OutletUsers).ToList();
            if (outletUsers.Any())
            {
                outletUsers = (from a in outletUsers
                               join s in _db.OutletsUsers
                                  on a.UserId equals s.UserId
                               join c in _db.Outlets
                                on s.OutletId equals c.OutletId
                               select new DistributorUserModel
                               {
                                   UserId = a.UserId,
                                   FirstName = a.FirstName,
                                   LastName = a.LastName,
                                   UserName = a.UserName,
                                   ConsPartyCode = a.ConsPartyCode,
                                   IsDeleted = a.IsDeleted,
                                   OTP = a.OTP,
                                   Address = a.Address,
                                   EmailConfirmed = a.EmailConfirmed,
                                   LockoutEndDateUtc = a.LockoutEndDateUtc,
                                   Role = a.Role,
                                   DistributorId = a.DistributorId,
                                   OutletCode = c != null ? c.OutletCode : "",
                                   OutletName = c != null ? c.OutletName : "",
                               }).ToList();
                userList.AddRange(outletUsers);
            }

            return userList;
        }

        public List<DistributorUserModel> GetDistributorUsers(int distributorId)
        {
            var UserList = (from a in _db.UserDetails
                            join d in _db.DistributorUsers on a.UserId equals d.UserId
                            where a.IsDeleted.Value == false
                            && d.DistributorId == distributorId
                            select a).Select(s => new DistributorUserModel
                            {
                                UserId = s.UserId,
                                FirstName = s.FirstName,
                                LastName = s.LastName,
                                UserName = s.AspNetUser.UserName,
                                ConsPartyCode = s.ConsPartyCode,
                                IsDeleted = s.IsDeleted,
                                OTP = s.OTP,
                                Address = s.Address,
                                EmailConfirmed = s.AspNetUser.EmailConfirmed,
                                LockoutEndDateUtc = s.AspNetUser.LockoutEndDateUtc,
                                Role = s.AspNetUser.AspNetRoles.OrderByDescending(a => a.Name).Select(r => r.Name).FirstOrDefault(),
                                DistributorId = distributorId
                            }).ToList();
            if (UserList.Any())
            {
                UserList = SetOutlet(UserList);
            }
            return UserList;
        }

        public List<DistributorUserModel> GetOutletUsers(int outletId)
        {
            var UserList = (from a in _db.UserDetails
                            join d in _db.OutletsUsers on a.UserId equals d.UserId
                            where a.IsDeleted.Value == false
                             && d.OutletId == outletId
                            select a).Select(s => new DistributorUserModel
                            {
                                UserId = s.UserId,
                                FirstName = s.FirstName,
                                LastName = s.LastName,
                                UserName = s.AspNetUser.UserName,
                                ConsPartyCode = s.ConsPartyCode,
                                IsDeleted = s.IsDeleted,
                                OTP = s.OTP,
                                Address = s.Address,
                                EmailConfirmed = s.AspNetUser.EmailConfirmed,
                                LockoutEndDateUtc = s.AspNetUser.LockoutEndDateUtc,
                                Role = s.AspNetUser.AspNetRoles.OrderByDescending(a => a.Name).Select(r => r.Name).FirstOrDefault(),
                                DistributorId = null
                            }).ToList();
            if (UserList.Any())
            {
                UserList = SetOutlet(UserList);
            }
            return UserList;
        }

        public List<DistributorUserModel> GetWorkshopUsers(int workshopId)
        {
            var UserList = (from a in _db.UserDetails
                            join w in _db.WorkshopsUsers on a.UserId equals w.UserId
                            where a.IsDeleted.Value == false
                            && w.WorkshopId == workshopId
                            select a).Select(s => new DistributorUserModel
                            {
                                UserId = s.UserId,
                                FirstName = s.FirstName,
                                LastName = s.LastName,
                                UserName = s.AspNetUser.UserName,
                                ConsPartyCode = s.ConsPartyCode,
                                IsDeleted = s.IsDeleted,
                                OTP = s.OTP,
                                Address = s.Address,
                                EmailConfirmed = s.AspNetUser.EmailConfirmed,
                                LockoutEndDateUtc = s.AspNetUser.LockoutEndDateUtc,
                                Role = s.AspNetUser.AspNetRoles.OrderByDescending(a => a.Name).Select(r => r.Name).FirstOrDefault(),
                                DistributorId = null
                            }).ToList();
            if (UserList.Any())
            {
                UserList = SetOutlet(UserList);
            }
            return UserList;
        }

        public List<DistributorOutletModel> GetDistributorOutlets(int? distributorId)
        {
            var outletList = new List<DistributorOutletModel>();

            var outlets = (from d in _db.DistributorsOutlets
                           join o in _db.Outlets on d.OutletId equals o.OutletId
                           where d.DistributorId == distributorId
                           select o).Select(s => new DistributorOutletModel
                           {
                               OutletId = s.OutletId,
                               OutletCode = s.OutletCode,
                               OutletName = s.OutletName,
                               Address = s.Address,
                           }).ToList();

            foreach (var item in outlets)
            {
                var ro = (from o in _db.DistributorsOutlets
                          join u in _db.UserDetails on o.UserId equals u.UserId
                          where o.DistributorId == distributorId.Value && o.OutletId == item.OutletId && u.AspNetUser.AspNetRoles.Any(a => a.Name.Equals(Constants.RoIncharge))
                          select u).FirstOrDefault();
                var model = new DistributorOutletModel
                {
                    OutletId = item.OutletId,
                    OutletCode = item.OutletCode,
                    OutletName = item.OutletName,
                    Address = item.Address,
                    RoIncharge = ro != null ? ro.FirstName + " " + ro.LastName : ""
                };
                outletList.Add(model);
            }
            return outletList;
        }

        public List<DistributorUserModel> GetRoInchargeUsers(int? distributorId, string role)
        {
            var userList = (from a in _db.UserDetails
                            join d in _db.DistributorsOutlets on a.UserId equals d.UserId
                            where a.IsDeleted.Value == false
                            && d.DistributorId == distributorId
                            && a.AspNetUser.AspNetRoles.Any(r => r.Name == role)
                            select a).Select(s => new DistributorUserModel
                            {
                                UserId = s.UserId,
                                FirstName = s.FirstName,
                                LastName = s.LastName,
                                UserName = s.AspNetUser.UserName,
                                ConsPartyCode = s.ConsPartyCode,
                                IsDeleted = s.IsDeleted,
                                OTP = s.OTP,
                                Address = s.Address,
                                EmailConfirmed = s.AspNetUser.EmailConfirmed,
                                LockoutEndDateUtc = s.AspNetUser.LockoutEndDateUtc,
                                Role = s.AspNetUser.AspNetRoles.OrderByDescending(a => a.Name).Select(r => r.Name).FirstOrDefault(),
                                DistributorId = distributorId
                            }).ToList();

            if (userList.Count > 0)
            {
                userList = SetOutlet(userList);
            }
            return userList;
        }

        public List<DistributorUserModel> GetSalesExecutiveUsers(int? distributorId, string role)
        {
            var userList = (from a in _db.UserDetails
                            join d in _db.DistributorUsers on a.UserId equals d.UserId
                            where a.IsDeleted.Value == false
                                  && d.DistributorId == distributorId
                                  && a.AspNetUser.AspNetRoles.Any(r => r.Name == role)
                            select a).Select(s => new DistributorUserModel
                            {
                                UserId = s.UserId,
                                FirstName = s.FirstName,
                                LastName = s.LastName,
                                UserName = s.AspNetUser.UserName,
                                ConsPartyCode = s.ConsPartyCode,
                                IsDeleted = s.IsDeleted,
                                OTP = s.OTP,
                                Address = s.Address,
                                EmailConfirmed = s.AspNetUser.EmailConfirmed,
                                LockoutEndDateUtc = s.AspNetUser.LockoutEndDateUtc,
                                Role = s.AspNetUser.AspNetRoles.OrderByDescending(a => a.Name).Select(r => r.Name).FirstOrDefault(),
                                DistributorId = distributorId
                            }).ToList();

            if (userList.Count > 0)
            {
                userList = SetOutlet(userList);
            }
            return userList;
        }

        public List<WorkshopsModel> GetWorkshop(int distributorId)
        {
            var UserList = (from a in _db.UserDetails
                            join d in _db.DistributorWorkShops on a.UserId equals d.UserId
                            join w in _db.WorkShops on d.WorkShopId equals w.WorkShopId
                            where a.IsDeleted.Value == false
                            && d.DistributorId == distributorId
                            select new WorkshopsModel
                            {
                                WorkshopId = w.WorkShopId,
                                CustomerCode = a.ConsPartyCode,
                                CustomerName = w.WorkShopName,
                                CustomerType = w.Type,
                                UserId = a.UserId,
                                UserName = w.MobileNumber!=null? w.MobileNumber:a.AspNetUser.UserName,
                                FirstName = a.FirstName,
                                LastName = a.LastName,
                                IsDelete = a.IsDeleted,
                                OutletCode = w.outletId != null ? w.Outlet.OutletCode : "",
                                OutletName = w.outletId != null ? w.Outlet.OutletName : "",
                                EmailConfirmed = a.AspNetUser.EmailConfirmed,
                                LockoutEndDateUtc = a.AspNetUser.LockoutEndDateUtc,
                                DistributorId = d.DistributorId ?? 0
                            }).ToList();
            return UserList;
        }
    }
}