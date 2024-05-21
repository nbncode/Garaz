using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoUsers
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region Get
        public UserDetail GetUserDetailByUserId(string userId)
        {
            return db.UserDetails.Where(a => a.IsDeleted.Value == false && a.UserId == userId).FirstOrDefault();
        }
        public List<UserDetail> getUsers()
        {
            return db.UserDetails.Where(a => a.IsDeleted.Value == false).ToList();
        }

        public List<UserDetail> getDistributor()
        {
            return db.UserDetails.Where(a => a.IsDeleted.Value == false && a.AspNetUser.AspNetRoles.Where(b => b.Name == Constants.Distributor).Count() > 0).ToList();
        }

        public List<UserDetail> getDistributorOutlets(string distributor)
        {
            return db.UserDetails.Where(a => a.IsDeleted.Value == false && a.AspNetUser.AspNetRoles.Where(b => b.Name == Constants.DistributorOutlets && a.DistributorId == distributor).Count() > 0).ToList();
        }

        public List<UserDetail> getDistributorUsers(string distributor)
        {
            return db.UserDetails.Where(a => a.IsDeleted.Value == false && a.AspNetUser.AspNetRoles.Where(b => b.Name == Constants.Users && a.DistributorId == distributor).Count() > 0).ToList();
        }
        public List<UserDetail> getDistributorUsers(int distributorId)
        {

            return (from a in db.UserDetails
                    join d in db.DistributorUsers on a.UserId equals d.UserId
                    //join info in db.DistributorUserInfoes on d.UserId equals info.UserId
                    where a.IsDeleted.Value == false
                    && d.DistributorId == distributorId
                    select a).ToList();

        }

        public List<UserDetail> GetDistributorUsers(int? distributorId, string role)
        {
            return (from a in db.UserDetails
                    join d in db.DistributorUsers on a.UserId equals d.UserId
                    where a.IsDeleted.Value == false
                    && d.DistributorId == distributorId
                    && a.AspNetUser.AspNetRoles.Where(r => r.Name == role).Count() > 0
                    select a).ToList();
        }

        /// <summary>
        /// Get outlet's users.
        /// </summary>
        /// <param name="outletId">The outlet id.</param>
        /// <returns>Return list of user detail.</returns>
        public List<UserDetail> GetOutletUsers(int outletId)
        {
            return (from a in db.UserDetails
                    join d in db.OutletsUsers on a.UserId equals d.UserId
                    where a.IsDeleted.Value == false
                    && d.OutletId == outletId
                    select a).ToList();
        }

        /// <summary>
        /// Get workshop's users.
        /// </summary>
        /// <param name="outletId">The workshop id.</param>
        /// <returns>Return list of user detail.</returns>
        public List<UserDetail> GetWorkshopUsers(int workshopId)
        {

            return (from u in db.UserDetails
                    join w in db.WorkshopsUsers on u.UserId equals w.UserId
                    where u.IsDeleted.Value == false
                    && w.WorkshopId == workshopId
                    select u).ToList();
        }

        public List<UserDetail> getWorkshop()
        {
            return db.UserDetails.Where(a => a.IsDeleted.Value == false && a.AspNetUser.AspNetRoles.Where(b => b.Name == Constants.Workshop).Count() > 0).ToList();
        }

        public WorkShop getWorkshopById(int WorkshopId)
        {
            return db.WorkShops.Where(a => a.WorkShopId == WorkshopId).FirstOrDefault();
        }


        public List<WorkShop> WorkshopList()
        {
            return db.WorkShops.ToList();
        }
        public List<WorkShopSchemes> getWorkshopAllUsers(int schemeId)
        {
            return (from w in db.WorkShops
                    join l in db.WorkShopLabelSchemes on w.WorkShopId equals l.WorkShopId
                    join s in db.Schemes on l.SchemeId equals s.SchemeId
                    where l.SchemeId == schemeId && (s.IsDeleted == null || s.IsDeleted == false)
                    select new WorkShopSchemes()
                    {
                        WorkShopId = w.WorkShopId,
                        WorkShopName = w.WorkShopName
                    }).Distinct().ToList();
        }

        public List<UserDetail> getWorkshop(string DistributorId)
        {
            return (from a in db.UserDetails
                        //join d in db.DistributorWorkShops
                        //on a.UserId equals d.WorkShopId
                        //where a.IsDeleted.Value == false && a.AspNetUser.AspNetRoles.Where(b => b.Name == Constansts.Workshop).Count() > 0
                        //&& d.DistributorId == DistributorId
                    select a).ToList();
        }
        public List<UserDetail> getWorkshop(int distributorId)
        {
            return (from a in db.UserDetails
                    join d in db.DistributorWorkShops on a.UserId equals d.UserId
                    where a.IsDeleted.Value == false
                    && d.DistributorId == distributorId
                    select a).ToList();
        }
        public List<DistributorWorkShop> getDistributorByWorkShopId(int WorkShopId)
        {
            return db.DistributorWorkShops.Where(a => a.WorkShopId == WorkShopId).ToList();
        }
        public Distributor GetDistributorByUserId(string userId)
        {
            var dist = (from d in db.Distributors
                        let du = from u in d.DistributorUsers
                                 where u.UserId == userId
                                 select u
                        where du.Any()
                        select d).FirstOrDefault();

            return dist;

            //return db.Distributors.Where(a => a.DistributorUsers.FirstOrDefault().UserId == UserId).FirstOrDefault();            
        }

        public Distributor GetDistributorByDistributorId(int distributorId)
        {
            var dist = db.Distributors.Where(d=>d.DistributorId==distributorId).FirstOrDefault();

            return dist;          
        }

        public int getDistributorIdByUserId(string userId, string role)
        {
            var aspNetUser = db.AspNetUsers.Where(a => a.Id == userId).FirstOrDefault();
            int distributorId = 0;
            if (!string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(userId))
            {
                if (role.Equals(Constants.Distributor, StringComparison.OrdinalIgnoreCase) || role.Equals(Constants.Users, StringComparison.OrdinalIgnoreCase))
                {
                    var dist = (from d in db.Distributors
                                let du = from u in d.DistributorUsers
                                         where u.UserId == userId
                                         select u
                                where du.Count() > 0
                                select d).FirstOrDefault();
                    distributorId = dist != null ? dist.DistributorId : 0;
                }
                else if (role.Equals(Constants.Workshop, StringComparison.OrdinalIgnoreCase) || role.Equals(Constants.WorkshopUsers, StringComparison.OrdinalIgnoreCase))
                {
                    int workshopId = 0;
                    workshopId = GetWorkshopIdByUserId(userId, role);
                    var distributor = (from d in db.Distributors
                                       join w in db.DistributorWorkShops on d.DistributorId equals w.DistributorId
                                       where w.WorkShopId == workshopId
                                       select d).FirstOrDefault();
                    distributorId = distributor != null ? distributor.DistributorId : 0;
                }
                else if (aspNetUser.AspNetRoles.Where(a => a.Name.Contains(Constants.SalesExecutive)).FirstOrDefault() != null)
                {
                    var distributor = db.DistributorUsers.Where(d => d.UserId == userId).FirstOrDefault();
                    distributorId = distributor != null ? Convert.ToInt32(distributor.DistributorId) : 0;
                }
                else if (aspNetUser.AspNetRoles.Where(a => a.Name.Contains(Constants.RoIncharge)).FirstOrDefault() != null)
                {
                    var distributor = db.DistributorsOutlets.Where(d => d.UserId == userId).FirstOrDefault();
                    distributorId = distributor != null ? Convert.ToInt32(distributor.DistributorId) : 0;
                }
                else if (role.Equals(Constants.SuperAdmin, StringComparison.OrdinalIgnoreCase))
                {
                    var distributor = db.Distributors.FirstOrDefault();
                    distributorId = distributor != null ? distributor.DistributorId : 0;
                }
            }
            return distributorId;
        }

        public int GetWorkshopIdByUserId(string userId, string role)
        {
            int workshopId = 0;
            if (!string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(userId))
            {
                if (role.Equals(Constants.WorkshopUsers, StringComparison.OrdinalIgnoreCase))
                {
                    var wu = db.WorkshopsUsers.Where(x => x.UserId == userId).FirstOrDefault();
                    workshopId = wu != null ? wu.WorkshopId : 0;
                }
                else if (role.Equals(Constants.Workshop, StringComparison.OrdinalIgnoreCase))
                {
                    var distWorkShop = (from w in db.DistributorWorkShops
                                        where w.UserId == userId
                                        select w).FirstOrDefault();
                    workshopId = distWorkShop != null ? (distWorkShop.WorkShopId ?? 0) : 0;
                }
                else if (role.Equals(Constants.SuperAdmin, StringComparison.OrdinalIgnoreCase))
                {
                    workshopId = 0;
                }
            }
            return workshopId;
        }

        public List<Distributor> GetAllDistributorsNew()
        {
            var ids = db.DistributorUsers.Where(a => a.AspNetUser.UserDetails.FirstOrDefault().IsDeleted == false
                                                     && a.AspNetUser.AspNetRoles.Any(b => b.Name == Constants.Distributor)).Select(a => a.DistributorId).ToList();
            return db.Distributors.Where(a => ids.Contains(a.DistributorId)).OrderBy(d => d.DistributorName).ToList();
        }


        /// <summary>
        /// Get outlet by user id.
        /// </summary>
        /// <param name="userId">The login user id of the user.</param>
        /// <returns>Return outlet object.</returns>
        public Outlet GetOutletByUserId(string userId)
        {
            // Get outlet id from DistributourOutlets
            var distributorsOutlet = (from d in db.DistributorsOutlets
                                      where d.UserId == userId
                                      select d).FirstOrDefault();

            if (distributorsOutlet != null)
            {
                var outletId = distributorsOutlet.OutletId;

                // Get outlet from Outlets based on outlet id
                return db.Outlets.Where(a => a.OutletId == outletId).FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Get workshop by user id.
        /// </summary>
        /// <param name="userId">The login user id of the user.</param>
        /// <returns>Return workshop object.</returns>
        public WorkShop GetWorkshopByUserId(string userId)
        {
            var distWorkShop = db.DistributorWorkShops.FirstOrDefault(dw => dw.UserId == userId);
            var workshopId = distWorkShop != null ? distWorkShop.WorkShopId : GetWorkshopIdByUserId(userId);

            // Get workshop from Workshops based on workshop id           
            return workshopId != null ? db.WorkShops.FirstOrDefault(w => w.WorkShopId == workshopId) : null;
        }

        /// <summary>
        /// Get distributor workshop.
        /// </summary>
        /// <param name="userId">The user id of the user.</param>
        /// <returns>Return instance of DistributorWorkshop.</returns>
        public DistributorWorkShop GetDistributorWorkShop(string userId)
        {
            return db.DistributorWorkShops.FirstOrDefault(w => w.UserId == userId);
        }

        /// <summary>
        /// Get list of Workshop Id by user id.
        /// </summary>
        /// <param name="userId">The user id of the user.</param>
        /// <returns>Return list of workshop ids.</returns>
        public List<int> GetWorkshopIdsByUserId(string userId)
        {
            return db.SalesExecutiveWorkshops.Where(se => se.UserId == userId).Select(se => se.WorkshopId).ToList();
        }

        /// <summary>
        /// Get list of workshops by user id and distributor id.
        /// </summary>
        /// <param name="userId">The user id for which workshops will be retrieved.</param>
        /// <param name="distId">The distributor id for which workshops will be retrieved.</param>
        /// <returns>Return the list of workshops.</returns>
        public List<WorkShop> GetWorkShops(string userId, int distId)
        {
            var workshops = new List<WorkShop>();
            var distWorkShop = (from w in db.DistributorWorkShops
                                where w.UserId == userId && w.DistributorId == distId
                                select w).FirstOrDefault();

            if (distWorkShop != null)
            {
                var wsId = distWorkShop.WorkShopId;
                workshops = db.WorkShops.Where(w => w.WorkShopId == wsId).ToList();
            }

            return workshops;
        }

        /// <summary>
        /// Get list of workshop by distributor Id.
        /// </summary>
        /// <param name="distributorId">The distributor Id</param>
        /// <returns>Return list of workshop.</returns>
        public List<WorkShop> GetWorkshopByDistId(int distributorId)
        {
            return (from w in db.WorkShops
                    join d in db.DistributorWorkShops on w.WorkShopId equals d.WorkShopId
                    join u in db.UserDetails on d.UserId equals u.UserId
                    where d.DistributorId == distributorId && (u.IsDeleted == null || u.IsDeleted.Value == false)
                    select w).Distinct().ToList();
        }

        /// <summary>
        /// Get list of workshop by distributor Id.
        /// </summary>
        /// <param name="distributorId">The distributor Id</param>
        /// <returns>Return list of workshop.</returns>
        public List<ResponseTargetWorkshopModel> GetWorkshopByDistIdTarget(int distributorId)
        {
            return (from w in db.WorkShops
                    join d in db.DistributorWorkShops on w.WorkShopId equals d.WorkShopId
                    join u in db.UserDetails on d.UserId equals u.UserId
                    where d.DistributorId == distributorId && (u.IsDeleted == null || u.IsDeleted.Value == false)
                    select new ResponseTargetWorkshopModel()
                    {
                        WorkShopCode = u.ConsPartyCode,
                        WorkShopId = w.WorkShopId,
                        WorkShopName = w.WorkShopName,
                        IsQualifiedAsDefault = false,
                        Max = 0,
                        Min = 0,
                    }).Distinct().ToList();
        }

        /// <summary>
        /// Get list of workshop by distributor Id.
        /// </summary>
        /// <param name="distributorId">The distributor Id</param>
        /// <returns>Return list of workshop.</returns>
        public List<WorkshopModelScheme> GetWorkshopByDistIdNew(int distributorId)
        {
            return (from w in db.WorkShops
                    join d in db.DistributorWorkShops on w.WorkShopId equals d.WorkShopId
                    join u in db.UserDetails on d.UserId equals u.UserId
                    where d.DistributorId == distributorId && (u.IsDeleted == null || u.IsDeleted.Value == false)
                    select new WorkshopModelScheme()
                    {
                        Address = w.Address,
                        ConsPartyCode = u.ConsPartyCode,
                        WorkShopId = w.WorkShopId,
                        WorkShopName = w.WorkShopName
                    }).Distinct().ToList();
        }

        /// <summary>
        /// Get list of user workshops.
        /// </summary>
        /// <param name="userId">The user id of the user.</param>
        /// <returns>Return list of UserWorkshop.</returns>
        public List<UserWorkshop> GetUserWorkshop(string userId)
        {
            return db.UserWorkshops.Where(w => w.UserId == userId).ToList();
        }

        /// <summary>
        /// Get distributor locations.
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <returns>Return list of distributor locations.</returns>
        public List<DistributorLocation> GetDistributorLocations(int distributorId)
        {
            return db.DistributorLocations.Where(l => l.DistributorId == distributorId).ToList();
        }

        /// <summary>
        /// Get distributor location by distributor Id.
        /// </summary>        
        public DistributorLocation GetDistributorLocation(int distributorId)
        {
            return db.DistributorLocations.Where(l => l.DistributorId == distributorId).FirstOrDefault();
        }

        /// <summary>
        /// Get distributor location by distributor Id and locationd Id.
        /// </summary>        
        public DistributorLocation GetDistributorLocation(int distributorId, int locationId)
        {
            return db.DistributorLocations.Where(l => l.DistributorId == distributorId && l.LocationId == locationId).FirstOrDefault();
        }

        /// <summary>
        /// Get list of User Id of sales executive for RO Incharge.
        /// </summary>        
        public List<string> GetRoSalesExecutives(string roUserId)
        {
            return db.RoSalesExecutives.Where(r => r.RoUserId == roUserId).Select(r => r.SeUserId).ToList();
        }

        /// <summary>
        /// Get workshops for Sales Executive.
        /// </summary>
        /// <param name="userId">The user id of the Sales Executive.</param>
        /// <returns>Return list of workshops.</returns>
        public List<WorkshopModelScheme> GetWorkShopsForSalesExecutive(string userId)
        {
            var seWorkshopIds = db.SalesExecutiveWorkshops.Where(se => se.UserId == userId).Select(se => se.WorkshopId);

            return (from w in db.WorkShops
                    join d in db.DistributorWorkShops on w.WorkShopId equals d.WorkShopId
                    join u in db.UserDetails on d.UserId equals u.UserId
                    where seWorkshopIds.Contains(w.WorkShopId) && (u.IsDeleted == null || u.IsDeleted.Value == false)
                    select new WorkshopModelScheme()
                    {
                        Address = w.Address,
                        ConsPartyCode = u.ConsPartyCode,
                        WorkShopId = w.WorkShopId,
                        WorkShopName = w.WorkShopName
                    }).Distinct().ToList();
        }

        /// <summary>
        /// Save user workshop details.
        /// </summary>       
        public bool SaveUserWorkshop(List<UserWorkshop> userWorkshops, string userId)
        {
            // Delete existing record for user
            var uws = db.UserWorkshops.Where(w => w.UserId == userId);
            if (uws != null && uws.Count() > 0)
            {
                foreach (var uw in uws)
                {
                    db.UserWorkshops.Remove(uw);
                }
                db.SaveChanges();
            }

            // Now save new record
            foreach (var uw in userWorkshops)
            {
                db.UserWorkshops.Add(uw);
            }
            return db.SaveChanges() > 0;
        }

        public bool SaveDistributorByWorkShopId(int WorkShopId, string Distributors)
        {
            var old = db.DistributorWorkShops.Where(a => a.WorkShopId == WorkShopId).ToList();

            if (old.Count > 0)
            {
                db.DistributorWorkShops.RemoveRange(old);
            }

            var split = Distributors.Split(',');
            foreach (var item in split)
            {
                DistributorWorkShop distributorWorkShop = new DistributorWorkShop();
                distributorWorkShop.DistributorId = Convert.ToInt32(item);
                distributorWorkShop.WorkShopId = WorkShopId;
                db.DistributorWorkShops.Add(distributorWorkShop);
                db.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Save workshop for Sales Executive user.
        /// </summary>
        /// <param name="seWorkshops">The list of sales executive workshops.</param>
        /// <param name="userId">The user id for which workshop will be saved.</param>
        /// <param name="isMappingFromWsImport">Whether mapping is done from workshop import.</param>
        /// <returns>Return true if saved else false.</returns>
        public bool SaveWorkshopsForSalesExecutive(List<SalesExecutiveWorkshop> seWorkshops, string userId, bool isMappingFromWsImport = false)
        {
            var result = false;

            // First remove existing workshops
            if (!isMappingFromWsImport)
            {
                var wsIds = seWorkshops.Select(s => s.WorkshopId).ToList();
                var existingSeWorkshops = string.IsNullOrWhiteSpace(userId) ? null : db.SalesExecutiveWorkshops.Where(se => se.UserId == userId || wsIds.Contains(se.WorkshopId));
                if (existingSeWorkshops?.Count() > 0)
                {
                    db.SalesExecutiveWorkshops.RemoveRange(existingSeWorkshops);
                    db.SaveChanges();
                    result = true;
                }
            }

            if (seWorkshops.Count > 0)
            {
                foreach (var row in seWorkshops)
                {
                    result = true;
                    var alreadyExist = db.SalesExecutiveWorkshops.AsNoTracking().FirstOrDefault(s => s.WorkshopId == row.WorkshopId && s.UserId == row.UserId);
                    if (alreadyExist == null)
                    {
                        // Then save the selected one
                        db.SalesExecutiveWorkshops.Add(row);
                        db.SaveChanges();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Save Sales executives for RO Incharge.
        /// </summary>       
        public bool SaveSalesExecutives(string roUserId, List<RoSalesExecutive> seUsers)
        {
            // Delete existing record for user
            var roSes = db.RoSalesExecutives.Where(r => r.RoUserId == roUserId);
            if (roSes.Any())
            {
                db.RoSalesExecutives.RemoveRange(roSes);
                db.SaveChanges();
            }

            // Now save new record
            if (seUsers.Any())
            {
                db.RoSalesExecutives.AddRange(seUsers);
                return db.SaveChanges() > 0;
            }
            return false;
        }

        #region Distributors
        public int SaveDistributor(Distributor model, string UserId, string BrandIds)
        {
            db.Distributors.Add(model);
            db.SaveChanges();

            DistributorUser distributorUser = new DistributorUser();
            distributorUser.DistributorId = model.DistributorId;
            distributorUser.UserId = UserId;
            db.DistributorUsers.Add(distributorUser);
            db.SaveChanges();
            if (!string.IsNullOrEmpty(BrandIds))
            {
                RegisterBrandRequest registerBrandRequest = new RegisterBrandRequest();
                registerBrandRequest.SaveRegisterBrand(new clsRegisterBrand()
                {
                    BrandIds = BrandIds,
                    DistributorId = model.DistributorId,
                    Role = Constants.Distributor
                });
            }
            return model.DistributorId;
        }

        public bool UpdateDistributor(Distributor model, string UserId, string BrandIds)
        {
            var distributorUsers = db.DistributorUsers.Where(a => a.UserId == UserId).FirstOrDefault();
            var old = db.Distributors.Where(a => a.DistributorId == distributorUsers.DistributorId).FirstOrDefault();
            old.DistributorName = model.DistributorName;
            old.Address = model.Address;
            old.Latitude = model.Latitude;
            old.Longitude = model.Longitude;
            old.City = model.City;
            old.State = model.State;
            old.Pincode = model.Pincode;
            old.Gender = model.Gender;
            old.LandlineNumber = model.LandlineNumber;
            old.Gstin = model.Gstin;
            old.FValue = model.FValue;
            old.MValue = model.MValue;
            old.SValue = model.SValue;
            old.Company = model.Company;
            old.UPIID = model.UPIID;
            old.BankName = model.BankName;
            old.IfscCode = model.IfscCode;
            old.AccountNumber = model.AccountNumber;
            //if (!string.IsNullOrEmpty(BrandIds))
            //{
            RegisterBrandRequest registerBrandRequest = new RegisterBrandRequest();
            registerBrandRequest.SaveRegisterBrand(new clsRegisterBrand()
            {
                BrandIds = BrandIds,
                DistributorId = old.DistributorId,
                Role = Constants.Distributor
            });
            //}
            return db.SaveChanges() > 0;
        }

        public bool UpdateDistributor(Distributor model, int distributorId)
        {
            var distributor = db.Distributors.Where(d => d.DistributorId == distributorId).FirstOrDefault();
            if (distributor != null)
            {
                distributor.DistributorName = model.DistributorName;
                distributor.City = model.City;
                distributor.State = model.State;
                distributor.Pincode = model.Pincode;
                distributor.Gender = model.Gender;
                distributor.LandlineNumber = model.LandlineNumber;
                distributor.Gstin = model.Gstin;
                return db.SaveChanges() > 0;
            }
            return false;
        }
        #endregion

        #region WorkShop
        public List<ResponseWorkshop> getWorkshopByText(RequestWorkshop obj)
        {
            List<ResponseWorkshop> listResponeWorkshop = new List<ResponseWorkshop>();
            var workshopList = db.WorkShops.Where(a => a.WorkShopName.Contains(obj.searchText) && a.DistributorWorkShops.FirstOrDefault() != null
                                && a.DistributorWorkShops.FirstOrDefault().AspNetUser.EmailConfirmed == true
                                && a.DistributorWorkShops.FirstOrDefault().AspNetUser.AspNetRoles.FirstOrDefault().Name == Constants.Workshop).ToList();
            foreach (var item in workshopList)
            {
                listResponeWorkshop.Add(new ResponseWorkshop()
                {
                    Name = item.WorkShopName,
                    value = item.WorkShopId
                });
            }
            return listResponeWorkshop;
        }

        public bool SaveWorkShop(WorkShop model, string UserId, int DistributorId)
        {
            //if (!model.IsMoreThanOneBranch.Value)
            //{
            //    Outlet outlet = new Outlet()
            //    {
            //        OutletName = model.WorkShopName
            //    };
            //    db.Outlets.Add(outlet);
            //    db.SaveChanges();

            //    var distOut = new DistributorsOutlet()
            //    {
            //        OutletId = outlet.OutletId,
            //        DistributorId = DistributorId,
            //        UserId = null
            //    };
            //    db.DistributorsOutlets.Add(distOut);
            //    model.outletId = outlet.OutletId;
            //}

            db.WorkShops.Add(model);
            db.SaveChanges();
            if (DistributorId > 0)
            {
                DistributorWorkShop WorkshopUser = new DistributorWorkShop();
                WorkshopUser.WorkShopId = model.WorkShopId;
                WorkshopUser.DistributorId = DistributorId;
                WorkshopUser.UserId = UserId;
                db.DistributorWorkShops.Add(WorkshopUser);
            }
            return db.SaveChanges() > 0;
        }

        public int SaveWorkShop(WorkShop model)
        {
            db.WorkShops.Add(model);
            db.SaveChanges();

            return model.WorkShopId;
        }

        public bool UpdateWorkShop(WorkShop model, string UserId, int DistributorId)
        {
            var WorkShop = db.DistributorWorkShops.Where(a => a.DistributorId == DistributorId && a.UserId == UserId).FirstOrDefault();
            var old = db.WorkShops.Where(a => a.WorkShopId == WorkShop.WorkShopId).FirstOrDefault();
            old.Address = model.Address;
            old.WorkShopName = model.WorkShopName;
            old.Latitude = model.Latitude;
            old.Longitude = model.Longitude;
            old.City = model.City;
            old.State = model.State;
            old.Pincode = model.Pincode;
            old.Gender = model.Gender;
            old.LandlineNumber = model.LandlineNumber;
            old.Gstin = model.Gstin;
            old.CriticalOutstandingDays = model.CriticalOutstandingDays;
            old.outletId = model.outletId;
            old.CategoryName = model.CategoryName;
            old.CreditLimit = model.CreditLimit;
            old.BillingName = model.BillingName;
            old.YearOfEstablishment = model.YearOfEstablishment;
            old.Type = model.Type;
            old.Make = model.Make;
            old.JobsUndertaken = model.JobsUndertaken;
            old.Premise = model.Premise;
            old.GaraazArea = model.GaraazArea;
            old.TwoPostLifts = model.TwoPostLifts;
            old.WashingBay = model.WashingBay;
            old.PaintBooth = model.PaintBooth;
            old.ScanningAndToolKit = model.ScanningAndToolKit;
            old.TotalOwners = model.TotalOwners;
            old.TotalChiefMechanics = model.TotalChiefMechanics;
            old.TotalEmployees = model.TotalEmployees;
            old.MonthlyVehiclesServiced = model.MonthlyVehiclesServiced;
            old.MonthlyPartPurchase = model.MonthlyPartPurchase;
            old.MonthlyConsumablesPurchase = model.MonthlyConsumablesPurchase;
            old.WorkingHours = model.WorkingHours;
            old.WeeklyOffDay = model.WeeklyOffDay;
            old.Website = model.Website;
            old.InsuranceCompanies = model.InsuranceCompanies;
            old.IsMoreThanOneBranch = model.IsMoreThanOneBranch;
            return db.SaveChanges() > 0;
        }

        public bool UpdateWorkShop(WorkShop model, int workshopId)
        {
            var workshop = db.WorkShops.FirstOrDefault(w => w.WorkShopId == workshopId);
            if (workshop == null) return false;

            workshop.WorkShopName = model.WorkShopName;
            workshop.City = model.City;
            workshop.State = model.State;
            workshop.Pincode = model.Pincode;
            workshop.Gender = model.Gender;
            workshop.LandlineNumber = model.LandlineNumber;
            workshop.Gstin = model.Gstin;
            workshop.CriticalOutstandingDays = model.CriticalOutstandingDays;
            workshop.outletId = model.outletId;
            workshop.CreditLimit = model.CreditLimit;
            workshop.Type = model.Type;
            db.SaveChanges();
            return true;
        }

        public bool SaveWorkShopDistributorUsers(DistributorWorkShop model)
        {
            model.UserId = db.DistributorUsers.Where(a => a.DistributorId == model.DistributorId).Where(a => a.AspNetUser.AspNetRoles.Where(b => b.Name == Constants.Distributor).Count() > 0).FirstOrDefault().UserId;
            var dataExists = db.DistributorWorkShops.Where(a => a.DistributorId == model.DistributorId && a.UserId == model.UserId).FirstOrDefault();
            if (dataExists != null)
            {
                db.DistributorWorkShops.Add(model);
                return db.SaveChanges() > 0;
            }
            return false;

        }

        public int? GetWorkshopIdByName(string workshopName)
        {
            var workshop = db.WorkShops.FirstOrDefault(w => w.WorkShopName == workshopName);
            return workshop?.WorkShopId;
        }

        public WorkShop GetWorkshopByCode(string workshopCode)
        {
            var workshop = (from u in db.UserDetails
                            join dw in db.DistributorWorkShops on u.UserId equals dw.UserId
                            join w in db.WorkShops on dw.WorkShopId equals w.WorkShopId
                            where (u.IsDeleted == null || u.IsDeleted == false) && u.ConsPartyCode == workshopCode
                            select w
                ).FirstOrDefault();

            return workshop;
        }
        #endregion

        #region Outlet
        public bool SaveOutlet(Outlet model, string UserId, int DistributorId)
        {
            db.Outlets.Add(model);
            db.SaveChanges();

            DistributorsOutlet outletUser = new DistributorsOutlet();
            outletUser.OutletId = model.OutletId;
            outletUser.DistributorId = DistributorId;
            outletUser.UserId = UserId;
            db.DistributorsOutlets.Add(outletUser);
            return db.SaveChanges() > 0;
        }

        public bool UpdateOutlet(Outlet model, string UserId, int DistributorId)
        {
            var outlet = db.DistributorsOutlets.Where(a => a.DistributorId == DistributorId && a.UserId == UserId).FirstOrDefault();
            var old = db.Outlets.Where(a => a.OutletId == outlet.OutletId).FirstOrDefault();
            old.Address = model.Address;
            old.OutletName = model.OutletName;
            old.Latitude = model.Latitude;
            old.Longitude = model.Longitude;
            old.OutletCode = model.OutletCode;
            return db.SaveChanges() > 0;
        }


        #endregion

        #region Outlet users

        /// <summary>
        /// Update outlet users.
        /// </summary>
        /// <param name="model">The DistributorUserInfo model object.</param>
        /// <param name="userId">The user id of the user.</param>
        /// <returns>Return true if changes are saved else false.</returns>
        public bool UpdateOutletUsers(DistributorUserInfo model, string userId)
        {
            var userInfo = db.DistributorUserInfoes.Where(a => a.UserId == userId).FirstOrDefault();
            userInfo.Address = model.Address;
            userInfo.Latitude = model.Latitude;
            userInfo.Longitude = model.Longitude;
            return db.SaveChanges() > 0;
        }

        /// <summary>
        /// Save outlet users.
        /// </summary>
        /// <param name="model">The DistributorUserInfo model object.</param>
        /// <param name="userId">The user id of the user.</param>
        /// <param name="outletId">The outlet id of the outlet.</param>
        /// <returns>Return true if changes are saved else false.</returns>
        public bool SaveOutletUsers(DistributorUserInfo model, string userId, int outletId)
        {
            db.DistributorUserInfoes.Add(model);
            db.SaveChanges();

            var outletsUser = new OutletsUser()
            {
                OutletId = outletId,
                UserId = userId
            };
            db.OutletsUsers.Add(outletsUser);
            return db.SaveChanges() > 0;
        }

        #endregion

        #region Workshop users

        /// <summary>
        /// Update workshop users.
        /// </summary>
        /// <param name="model">The DistributorUserInfo model object.</param>
        /// <param name="userId">The user id of the user.</param>
        /// <returns>Return true if changes are saved else false.</returns>
        public bool UpdateWorkshopUsers(DistributorUserInfo model, string userId)
        {
            var userInfo = db.DistributorUserInfoes.Where(a => a.UserId == userId).FirstOrDefault();

            if (userInfo != null)
            {
                userInfo.Address = model.Address;
                userInfo.Latitude = model.Latitude;
                userInfo.Longitude = model.Longitude;
                return db.SaveChanges() > 0;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Save workshop users.
        /// </summary>
        /// <param name="model">The DistributorUserInfo model object.</param>
        /// <param name="userId">The user id of the user.</param>
        /// <param name="workshopId">The workshop id of the workshop.</param>
        /// <returns>Return true if changes are saved else false.</returns>
        public bool SaveWorkshopUsers(DistributorUserInfo model, string userId, int workshopId)
        {
            db.DistributorUserInfoes.Add(model);
            db.SaveChanges();

            var workshopUser = new WorkshopsUser()
            {
                WorkshopId = workshopId,
                UserId = userId
            };

            db.WorkshopsUsers.Add(workshopUser);
            return db.SaveChanges() > 0;
        }

        #endregion

        #region Distributors User Info
        public bool SaveDistributorUserInfo(DistributorUserInfo model, int DistributorId)
        {
            db.DistributorUserInfoes.Add(model);
            db.SaveChanges();

            var distributorUser = new DistributorUser
            {
                DistributorId = DistributorId,
                UserId = model.UserId
            };
            db.DistributorUsers.Add(distributorUser);
            return db.SaveChanges() > 0;
        }

        public bool UpdateDistributorUserInfo(DistributorUserInfo model)
        {
            var distributorUserInfo = db.DistributorUserInfoes.FirstOrDefault(a => a.UserId == model.UserId);
            if (distributorUserInfo == null) return false;

            distributorUserInfo.Address = model.Address;
            distributorUserInfo.Latitude = model.Latitude;
            distributorUserInfo.Longitude = model.Longitude;
            db.SaveChanges();
            return true;
        }
        #endregion

        #region Distributor Location

        /// <summary>
        /// Save distributor location.
        /// </summary>        
        public bool SaveDistributorLocation(clsDistributorLocation model)
        {
            db.DistributorLocations.Add(new DistributorLocation { DistributorId = model.DistributorId, LocationCode = model.LocationCode, Location = model.Location });
            return db.SaveChanges() > 0;
        }

        /// <summary>
        /// Update distributor location.
        /// </summary>        
        public bool UpdateDistributorLocation(clsDistributorLocation model, int locationId)
        {
            var dl = db.DistributorLocations.Where(l => l.LocationId == locationId).FirstOrDefault();

            if (dl != null)
            {
                dl.LocationCode = model.LocationCode;
                dl.Location = model.Location;
                return db.SaveChanges() > 0;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Delete the distributor location.
        /// </summary>
        /// <param name="locationId">The location id of the location.</param>
        /// <returns>Return true if deleted else return false.</returns>
        public bool DeleteDistributorLocation(int locationId)
        {
            var dl = db.DistributorLocations.Where(l => l.LocationId == locationId).FirstOrDefault();

            if (dl != null)
            {
                db.DistributorLocations.Remove(dl);
                return db.SaveChanges() > 0;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public bool SaveSingleDistributorByWorkShopId(int WorkShopId, int DistributorId)
        {
            DistributorWorkShop distributorWorkShop = new DistributorWorkShop();
            distributorWorkShop.DistributorId = DistributorId;
            distributorWorkShop.WorkShopId = WorkShopId;
            db.DistributorWorkShops.Add(distributorWorkShop);
            return db.SaveChanges() > 0;
        }

        public UserDetail getUserById(string UserId)
        {
            return db.UserDetails.Where(a => a.UserId == UserId).FirstOrDefault();
        }

        public string getUserFullName(string UserId, IList<string> roles)
        {
            string UserName = "";
            if (roles.Any())
            {
                int Id = 0;
                General general = new General();
                if (roles.Contains(Constants.Distributor))
                {
                    Id = general.GetDistributorId(UserId);
                    var distributor = db.Distributors.Where(x => x.DistributorId == Id).FirstOrDefault();
                    UserName = distributor != null ? (distributor.DistributorName ?? "") : UserName;
                }
                else if (roles.Contains(Constants.Workshop))
                {
                    Id = general.GetWorkshopId(UserId);
                    var workshop = db.WorkShops.Where(x => x.WorkShopId == Id).FirstOrDefault();
                    UserName = workshop != null ? (workshop.WorkShopName ?? "") : UserName;
                }
                else
                {
                    var user = db.UserDetails.Where(a => a.UserId == UserId).FirstOrDefault();
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty;
                }
            }
            return UserName;
        }

        /// <summary>
        /// Get user by ID and role basis.
        /// </summary>
        /// <param name="userId">The user id of the user.</param>
        /// <param name="role">The role of the user.</param>
        /// <returns>Return user detail.</returns>
        public UserDetail GetUserById(string userId, string role)
        {
            return (from u in db.UserDetails
                    where u.UserId == userId
                    && u.AspNetUser.AspNetRoles.Where(r => r.Name == role).Count() > 0
                    select u).FirstOrDefault();
        }

        /// <summary>
        /// Get roles by user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Return list of roles.</returns>
        public IEnumerable<string> GetRolesByUserId(string userId)
        {
            var user = db.AspNetUsers.Where(u => u.Id == userId).FirstOrDefault();
            return user.AspNetRoles.Select(r => r.Name).ToList();
        }

        #region Get WorkshopId By Workshop_UserId
        public int? GetWorkshopIdByUserId(string userId)
        {
            int? workshopId = null;
            var workshopuser = (from w in db.WorkshopsUsers
                                where w.UserId == userId
                                select w).FirstOrDefault();
            if (workshopuser != null)
                workshopId = workshopuser.WorkshopId;
            return workshopId != null ? workshopId : null;
        }
        #endregion

        #endregion

        #region Update Insert
        public bool AddOrUpdateUsers(UserDetail model)
        {
            var old = db.UserDetails.Where(a => a.UserId == model.UserId).FirstOrDefault();
            if (old == null)
            {
                model.IsDeleted = false;
                model.CreatedDate = DateTime.Now.ToString();
                db.UserDetails.Add(model);
                return db.SaveChanges() > 0;
            }
            else
            {
                old.FirstName = model.FirstName;
                old.LastName = model.LastName;
                old.ConsPartyCode = model.ConsPartyCode;
                old.Designations = model.Designations;
                db.SaveChanges();
                return true;
            }
        }
        #endregion

        #region Delete

        public bool deleteUser(string id)
        {
            var old = db.UserDetails.Where(a => a.UserId == id).FirstOrDefault();
            if (old == null)
            {
                return false;
            }
            old.IsDeleted = true;
            return db.SaveChanges() > 0;
        }

        #endregion

        #region Get Login Details
        public List<LoginTime> getLoginTime(string userId)
        {
            return db.LoginTimes.Where(a => a.UserId == userId).ToList();
        }
        #endregion

        #region Get OTP
        public int GetOTP()
        {
            return 1234;
        }
        #endregion

        #region Update OTP Insert
        public bool addUpdateOtp(string UserId, int otp)
        {
            var old = db.UserDetails.Where(a => a.UserId == UserId).FirstOrDefault();
            if (old == null)
            {
                return db.SaveChanges() > 0;
            }
            else
            {
                old.OTP = otp;
                db.SaveChanges();
                return true;
            }
        }
        #endregion

        #region Get OTP by Username
        public int GetOTPByUserName(string UserId)
        {
            var old = db.UserDetails.Where(a => a.UserId == UserId).FirstOrDefault();
            return old.OTP.Value;
        }
        #endregion

        #region Update password
        public bool UpdatePassword(UserDetail model)
        {
            var ud = db.UserDetails.Where(a => a.UserId == model.UserId).FirstOrDefault();
            if (ud != null)
            {
                ud.Password = model.Password;
                return db.SaveChanges() > 0;
            }
            else
                return false;
        }
        #endregion

        #region Get All Users List
        public List<UsersResponse> GetAllUsers(UsersPagination model, out int totalRecords)
        {
            var usersList = new List<UsersResponse>();

            if (model.Role.Equals(Constants.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                var allUsers = db.UserDetails.Where(a => a.IsDeleted.Value == false && a.AspNetUser.AspNetRoles.Any(b => b.Name == Constants.Distributor || b.Name == Constants.Users || b.Name == Constants.SalesExecutive || b.Name == Constants.RoIncharge || b.Name == Constants.Workshop || b.Name == Constants.WorkshopUsers || b.Name == Constants.OutletUsers)).Select(s => new UsersResponse
                {
                    Id = s.UserId,
                    Name = s.FirstName + " " + s.LastName,
                    UserName = s.AspNetUser.UserName,
                    ConsPartyCode = s.ConsPartyCode,
                    Role = s.AspNetUser.AspNetRoles.OrderByDescending(a => a.Name).Select(r => r.Name).FirstOrDefault()
                }).ToList();

                usersList.AddRange(allUsers);
            }
            else if (model.Role.Equals(Constants.Distributor, StringComparison.OrdinalIgnoreCase))
            {
                var distributorId = model.UserId.GetDistributorId(model.Role);
                var distributorUsers = (from a in db.UserDetails
                                        join d in db.DistributorUsers on a.UserId equals d.UserId
                                        where a.IsDeleted.Value == false && d.DistributorId == distributorId
                                        select new UsersResponse
                                        {
                                            Id = a.UserId,
                                            Name = a.FirstName + " " + a.LastName,
                                            UserName = a.AspNetUser.UserName,
                                            ConsPartyCode = a.ConsPartyCode,
                                            Role = a.AspNetUser.AspNetRoles.OrderByDescending(b => b.Name).Select(r => r.Name).FirstOrDefault()
                                        }).ToList();

                var roUsers = (from d in db.DistributorsOutlets
                               join o in db.Outlets on d.OutletId equals o.OutletId
                               join u in db.UserDetails on d.UserId equals u.UserId
                               where d.DistributorId == distributorId && u.AspNetUser.AspNetRoles.Any(a => a.Name == Constants.RoIncharge)
                               select new UsersResponse
                               {
                                   Id = d.UserId,
                                   Name = u.FirstName + " " + u.LastName,
                                   UserName = d.AspNetUser.UserName,
                                   ConsPartyCode = u.ConsPartyCode,
                                   Role = Constants.RoIncharge,
                                   OutletCode = o.OutletCode,
                                   OutletName = o.OutletName,
                                   Location = o.Address
                               }).ToList();

                //  Remove sales executive users from distributorUsers
                var salesExecutives = distributorUsers.Where(a => a.Role == Constants.SalesExecutive).ToList();

                distributorUsers = distributorUsers.Where(a => a.Role != Constants.SalesExecutive).ToList();

                var saletsExecutiveUsers = (from s in salesExecutives
                                            join ro in db.RoSalesExecutives on s.Id equals ro.SeUserId
                                            join Do in db.DistributorsOutlets on ro.RoUserId equals Do.UserId
                                            join u in db.UserDetails on s.Id equals u.UserId
                                            where Do.DistributorId == distributorId
                                            select new UsersResponse
                                            {
                                                Id = s.Id,
                                                Name = u.FirstName + " " + u.LastName,
                                                UserName = s.UserName,
                                                ConsPartyCode = u.ConsPartyCode,
                                                Role = Constants.SalesExecutive,
                                                OutletCode = Do.Outlet != null ? Do.Outlet.OutletCode : "",
                                                OutletName = Do.Outlet != null ? Do.Outlet.OutletName : "",
                                                Location = Do.Outlet != null ? Do.Outlet.Address : ""
                                            }).ToList();


                var workshopUsers = (from a in db.UserDetails
                                     join d in db.DistributorWorkShops on a.UserId equals d.UserId
                                     join w in db.WorkShops on d.WorkShopId equals w.WorkShopId
                                     where a.IsDeleted.Value == false && d.DistributorId == distributorId
                                     select new UsersResponse
                                     {
                                         Id = a.UserId,
                                         Name = w.WorkShopName,
                                         UserName = a.AspNetUser.UserName,
                                         ConsPartyCode = a.ConsPartyCode,
                                         Role = Constants.Workshop,
                                         OutletCode = w.Outlet != null ? w.Outlet.OutletCode : "",
                                         OutletName = w.Outlet != null ? w.Outlet.OutletName : "",
                                         Location = w.Outlet != null ? w.Outlet.Address : ""
                                     }).ToList();

                usersList.AddRange(distributorUsers);
                usersList.AddRange(roUsers);
                usersList.AddRange(saletsExecutiveUsers);
                usersList.AddRange(workshopUsers);
            }
            else if (model.Role.Equals(Constants.Workshop, StringComparison.OrdinalIgnoreCase))
            {
                var workshopId = model.UserId.GetWorkshopId(model.Role);
                var workshopUsers = (from u in db.UserDetails
                                     join wu in db.WorkshopsUsers on u.UserId equals wu.UserId
                                     join w in db.WorkShops on wu.WorkshopId equals w.WorkShopId
                                     where u.IsDeleted.Value == false && wu.WorkshopId == workshopId
                                     select new UsersResponse
                                     {
                                         Id = u.UserId,
                                         Name = u.FirstName + " " + u.LastName,
                                         UserName = u.AspNetUser.UserName,
                                         ConsPartyCode = u.ConsPartyCode,
                                         Role = u.AspNetUser.AspNetRoles.OrderByDescending(a => a.Name).Select(r => r.Name).FirstOrDefault(),
                                         OutletCode = w.Outlet != null ? w.Outlet.OutletCode : "",
                                         OutletName = w.Outlet != null ? w.Outlet.OutletName : "",
                                         Location = w.Outlet != null ? w.Outlet.Address : ""
                                     }).ToList();

                usersList.AddRange(workshopUsers);
            }
            else if (model.Role.Equals(Constants.RoIncharge, StringComparison.OrdinalIgnoreCase))
            {
                var salesExecutives = (from a in db.UserDetails
                                       join s in db.RoSalesExecutives on a.UserId equals s.SeUserId
                                       where a.IsDeleted.Value == false && s.RoUserId == model.UserId
                                       select new UsersResponse
                                       {
                                           Id = a.UserId,
                                           Name = a.FirstName + " " + a.LastName,
                                           UserName = a.AspNetUser.UserName,
                                           ConsPartyCode = a.ConsPartyCode,
                                           Role = Constants.SalesExecutive
                                       }).ToList();
                usersList.AddRange(salesExecutives);

                var distributorOutlet = db.DistributorsOutlets.FirstOrDefault(a => a.UserId == model.UserId);
                if (distributorOutlet != null)
                {
                    var workshopIds = from w in db.WorkShops
                                      where w.outletId == distributorOutlet.OutletId
                                      select w.WorkShopId;

                    var workshopUsers = (from a in db.UserDetails
                                         join d in db.DistributorWorkShops on a.UserId equals d.UserId
                                         join w in db.WorkShops on d.WorkShopId equals w.WorkShopId
                                         where a.IsDeleted.Value == false && workshopIds.Contains(d.WorkShopId.Value)
                                         select new UsersResponse
                                         {
                                             Id = a.UserId,
                                             Name = w.WorkShopName,
                                             UserName = a.AspNetUser.UserName,
                                             ConsPartyCode = a.ConsPartyCode,
                                             Role = Constants.Workshop,
                                             OutletCode = w.Outlet != null ? w.Outlet.OutletCode : "",
                                             OutletName = w.Outlet != null ? w.Outlet.OutletName : "",
                                             Location = w.Outlet != null ? w.Outlet.Address : ""
                                         }).ToList();

                    usersList.AddRange(workshopUsers);
                }
            }
            else if (model.Role.Equals(Constants.SalesExecutive, StringComparison.OrdinalIgnoreCase))
            {
                var workshopIds = db.SalesExecutiveWorkshops.Where(w => w.UserId == model.UserId).Select(w => w.WorkshopId);
                var workshopUsers = (from a in db.UserDetails
                                     join d in db.DistributorWorkShops on a.UserId equals d.UserId
                                     join w in db.WorkShops on d.WorkShopId equals w.WorkShopId
                                     where a.IsDeleted.Value == false && workshopIds.Contains(d.WorkShopId.Value)
                                     select new UsersResponse
                                     {
                                         Id = a.UserId,
                                         Name = w.WorkShopName,
                                         UserName = a.AspNetUser.UserName,
                                         ConsPartyCode = a.ConsPartyCode,
                                         Role = Constants.Workshop,
                                         OutletCode = w.Outlet != null ? w.Outlet.OutletCode : "",
                                         OutletName = w.Outlet != null ? w.Outlet.OutletName : "",
                                         Location = w.Outlet != null ? w.Outlet.Address : ""
                                     }).ToList();

                usersList.AddRange(workshopUsers);
            }
            else if (model.Role.Equals(Constants.DistributorOutlets, StringComparison.OrdinalIgnoreCase))
            {
                var outlet = GetOutletByUserId(model.UserId);
                var outletId = outlet.OutletId;

                var outletUsers = (from a in db.UserDetails
                                   join d in db.OutletsUsers on a.UserId equals d.UserId
                                   where a.IsDeleted.Value == false && d.OutletId == outletId
                                   select new UsersResponse
                                   {
                                       Id = a.UserId,
                                       Name = a.FirstName + " " + a.LastName,
                                       UserName = a.AspNetUser.UserName,
                                       ConsPartyCode = a.ConsPartyCode,
                                       Role = a.AspNetUser.AspNetRoles.OrderByDescending(b => b.Name).Select(r => r.Name).FirstOrDefault(),
                                       OutletCode = d.Outlet != null ? d.Outlet.OutletCode : "",
                                       OutletName = d.Outlet != null ? d.Outlet.OutletName : "",
                                       Location = d.Outlet != null ? d.Outlet.Address : ""
                                   }).ToList();

                usersList.AddRange(outletUsers);
            }

            if (usersList.Count > 0 && model.Role == Constants.SuperAdmin)
            {
                // For Workshops
                var workshop = usersList.Where(u => u.Role == Constants.Workshop).ToList();
                usersList = usersList.Where(u => u.Role != Constants.Workshop).ToList();
                if (workshop.Any())
                {
                    workshop = (from a in workshop
                                join b in db.DistributorWorkShops on a.Id equals b.UserId
                                join c in db.WorkShops on b.WorkShopId equals c.WorkShopId
                                select new UsersResponse
                                {
                                    Id = a.Id,
                                    Name = a.Name,
                                    Role = a.Role,
                                    ConsPartyCode = a.ConsPartyCode,
                                    UserName = a.UserName,
                                    OutletCode = c.Outlet != null ? c.Outlet.OutletCode : "",
                                    OutletName = c.Outlet != null ? c.Outlet.OutletName : "",
                                    Location = c.Outlet != null ? c.Outlet.Address : "",
                                }).DistinctBy(u => u.ConsPartyCode).ToList();
                    usersList.AddRange(workshop);
                }

                // For RoIncharge
                var roIncharge = usersList.Where(u => u.Role == Constants.RoIncharge).ToList();
                usersList = usersList.Where(u => u.Role != Constants.RoIncharge).ToList();
                if (roIncharge.Any())
                {
                    roIncharge = (from a in roIncharge
                                  join c in db.DistributorsOutlets on a.Id equals c.UserId
                                  select new UsersResponse
                                  {
                                      Id = a.Id,
                                      Name = a.Name,
                                      Role = a.Role,
                                      ConsPartyCode = a.ConsPartyCode,
                                      UserName = a.UserName,
                                      OutletCode = c.Outlet != null ? c.Outlet.OutletCode : "",
                                      OutletName = c.Outlet != null ? c.Outlet.OutletName : "",
                                      Location = c.Outlet != null ? c.Outlet.Address : "",
                                  }).DistinctBy(u => u.ConsPartyCode).ToList();
                    usersList.AddRange(roIncharge);
                }

                // For SalesExecutives
                var salesExecutives = usersList.Where(u => u.Role == Constants.SalesExecutive).ToList();
                usersList = usersList.Where(u => u.Role != Constants.SalesExecutive).ToList();
                if (salesExecutives.Any())
                {
                    salesExecutives = (from a in salesExecutives
                                       join s in db.RoSalesExecutives on a.Id equals s.SeUserId
                                       join c in db.DistributorsOutlets on s.RoUserId equals c.UserId
                                       select new UsersResponse
                                       {
                                           Id = a.Id,
                                           Name = a.Name,
                                           Role = a.Role,
                                           ConsPartyCode = a.ConsPartyCode,
                                           UserName = a.UserName,
                                           OutletCode = c.Outlet != null ? c.Outlet.OutletCode : "",
                                           OutletName = c.Outlet != null ? c.Outlet.OutletName : "",
                                           Location = c.Outlet != null ? c.Outlet.Address : "",
                                       }).DistinctBy(u => u.ConsPartyCode).ToList();
                    usersList.AddRange(salesExecutives);
                }

                // For Distributor
                var distributors = usersList.Where(u => u.Role == Constants.Distributor).ToList();
                usersList = usersList.Where(u => u.Role != Constants.Distributor).ToList();
                if (distributors.Any())
                {
                    distributors = (from a in distributors
                                    join u in db.UserDetails on a.Id equals u.UserId
                                    select new UsersResponse
                                    {
                                        Id = a.Id,
                                        Name = a.Name,
                                        Role = a.Role,
                                        ConsPartyCode = a.ConsPartyCode,
                                        UserName = a.UserName,
                                        OutletCode = a.OutletCode,
                                        OutletName = a.OutletName,
                                        Location = a.Location,
                                        CompanyName = u?.AspNetUser.DistributorUsers?.FirstOrDefault()?.Distributor?.Company
                                    }).DistinctBy(u => u.ConsPartyCode).ToList();
                    usersList.AddRange(distributors);
                }
            }

            if (!string.IsNullOrEmpty(model.Type))
            {
                usersList = usersList.Where(u => u.Role == model.Type).ToList();
            }
            if (!string.IsNullOrEmpty(model.Code))
            {
                usersList = usersList.Where(u => u.ConsPartyCode.Equals(model.Code, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(model.LocationCode))
            {
                usersList = usersList.Where(u => !string.IsNullOrEmpty(u.OutletCode) && u.OutletCode.Equals(model.LocationCode, StringComparison.CurrentCultureIgnoreCase) /*|| !string.IsNullOrEmpty(u.OutletName) && u.OutletName.Equals(model.LocationCode, StringComparison.CurrentCultureIgnoreCase)*/
                ).ToList();
            }

            totalRecords = usersList.Count;
            if (model.PageNumber > 0)
            {
                usersList.OrderBy(u => u.Name).ToList();
                usersList = usersList.GetPaging(model.PageNumber, model.PageSize);
            }

            return usersList.OrderBy(u => u.Name).ToList();

        }
        #endregion

        #region CheckEmpCodeExists
        public bool CheckEmpCodeExists(string empCode, string userId, string distributorId = null)
        {
            UserDetail data;
            if (string.IsNullOrEmpty(userId))
            {
                data = distributorId != null ? db.UserDetails.FirstOrDefault(x => x.ConsPartyCode == empCode && x.DistributorId == distributorId) : db.UserDetails.FirstOrDefault(x => x.ConsPartyCode == empCode);
                return data != null;
            }

            data = distributorId != null ? db.UserDetails.FirstOrDefault(x => x.UserId != userId && x.ConsPartyCode == empCode && x.DistributorId == distributorId) : db.UserDetails.FirstOrDefault(x => x.UserId != userId && x.ConsPartyCode == empCode);
            return data != null;
        }
        #endregion

        #region RoIncharge List
        public SelectList RoInchargeSelectList(int distributorId)
        {
            // Create select list
            var selListItem = new SelectListItem() { Value = "", Text = "-- Select RoIncharge --" };
            var newList = new List<SelectListItem> { selListItem };

            var distRoIncharges = (from a in db.UserDetails
                                   join d in db.DistributorUsers on a.UserId equals d.UserId
                                   where a.IsDeleted.Value == false
                                   && d.DistributorId == distributorId
                                   && a.AspNetUser.AspNetRoles.Where(r => r.Name == Constants.RoIncharge).Count() > 0
                                   select a).ToList(); ;

            foreach (var distRo in distRoIncharges)
            {
                newList.Add(new SelectListItem() { Value = distRo.UserId.ToString(), Text = distRo.FirstName + " " + distRo.LastName });
            }
            return new SelectList(newList, "Value", "Text", null);
        }
        #endregion

        public bool SaveRoSalesExecutive(string roUserId, string salesExecutiveId)
        {
            if (string.IsNullOrEmpty(salesExecutiveId))
            {
                return false;
            }
            // update existing record for user
            var roSes = db.RoSalesExecutives.Where(r => r.SeUserId == salesExecutiveId).FirstOrDefault();
            if (roSes != null)
            {
                roSes.RoUserId = roUserId;
                return db.SaveChanges() > 0;
            }
            else
            {
                var user = new RoSalesExecutive()
                {
                    RoUserId = roUserId,
                    SeUserId = salesExecutiveId
                };
                db.RoSalesExecutives.Add(user);
                return db.SaveChanges() > 0;
            }
        }

        public string GetRoInchargeBySalesExecutive(string salesExecutiveId)
        {
            var roSes = db.RoSalesExecutives.Where(r => r.SeUserId == salesExecutiveId).FirstOrDefault();

            return roSes != null ? roSes.RoUserId : "";

        }

        public List<Roles> GetAllUsersRole(GetUserOrderModel model)
        {
            var roles = new List<Roles>();

            var aspNetUser = db.AspNetUsers.FirstOrDefault(u => u.Id == model.UserId);
            var role = aspNetUser?.AspNetRoles.FirstOrDefault()?.Name;
            if (role == null) return roles;

            if (role.Equals(Constants.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                roles.Add(new Roles { key = Constants.Distributor, value = Constants.Distributor, label = "Distributor" });
                roles.Add(new Roles { key = Constants.RoIncharge, value = Constants.RoIncharge, label = "Ro Incharge" });
                roles.Add(new Roles { key = Constants.SalesExecutive, value = Constants.SalesExecutive, label = "Sales Executive" });
                roles.Add(new Roles { key = Constants.Workshop, value = Constants.Workshop, label = "Customer" });
                roles.Add(new Roles { key = Constants.Users, value = Constants.Users, label = "Users" });
            }
            else if (role.Equals(Constants.Distributor, StringComparison.OrdinalIgnoreCase))
            {
                roles.Add(new Roles { key = Constants.RoIncharge, value = Constants.RoIncharge, label = "Ro Incharge" });
                roles.Add(new Roles { key = Constants.SalesExecutive, value = Constants.SalesExecutive, label = "Sales Executive" });
                roles.Add(new Roles { key = Constants.Workshop, value = Constants.Workshop, label = "Customer" });
                roles.Add(new Roles { key = Constants.Users, value = Constants.Users, label = "Users" });
            }
            else if (role.Equals(Constants.RoIncharge, StringComparison.OrdinalIgnoreCase))
            {
                roles.Add(new Roles { key = Constants.SalesExecutive, value = Constants.SalesExecutive, label = "Sales Executive" });
                roles.Add(new Roles { key = Constants.Workshop, value = Constants.Workshop, label = "Customer" });
            }
            else if (role.Equals(Constants.SalesExecutive, StringComparison.OrdinalIgnoreCase))
            {
                roles.Add(new Roles { key = Constants.Workshop, value = Constants.Workshop, label = "Customer" });
            }
            else if (role.Equals(Constants.Workshop, StringComparison.OrdinalIgnoreCase))
            {
                roles.Add(new Roles { key = Constants.WorkshopUsers, value = Constants.WorkshopUsers, label = "Customer Users" });
            }

            return roles;
        }

        public List<SalesExeWorkshop> GetSalesExecutiveWorkshopBySalesExecutiveId(string SeUserId)
        {

            var distributorId = db.DistributorUsers.FirstOrDefault(r => r.UserId == SeUserId)?.DistributorId;

            var workshops = (from a in db.UserDetails
                             join d in db.DistributorWorkShops on a.UserId equals d.UserId
                             join w in db.WorkShops on d.WorkShopId equals w.WorkShopId
                             where a.IsDeleted.Value == false && d.DistributorId == distributorId
                             select new SalesExeWorkshop()
                             {
                                 WorkshopId = w.WorkShopId,
                                 WorkShopCode = a.ConsPartyCode,
                                 WorkShopName = w.WorkShopName
                             }).ToList();
            return workshops;
        }

        public bool WorkshopRegisterWithDistributorId(string userName, int distributorId)
        {
            var isRegister = false;

            var user = db.AspNetUsers.FirstOrDefault(u => u.UserName == userName);

            var ws = GetWorkshopByUserId(user?.Id);

            if (ws != null)
            {
                var distWorkshop = db.DistributorWorkShops.FirstOrDefault(dw => dw.WorkShopId == ws.WorkShopId && dw.DistributorId == distributorId);
                if (distWorkshop != null) { isRegister = true; }
            }
            return isRegister;
        }

        #region Get distributors list for workshop

        public WorkShopDistributors GetDistributorsForWorkShop(WorkShopDistributorsRequest model)
        {
            if (string.IsNullOrEmpty(model.UserName))
            {
                model.UserName = General.GetUsername();
            }

            var allUser = (from d in db.UserDetails
                           join a in db.AspNetUsers on d.UserId equals a.Id
                           where d.IsDeleted.Value == false && a.UserName.Contains(model.UserName)
                           select a).ToList();

            // Create select list
            var selListItem = new List<SelectListItem>();
            foreach (var user in allUser)
            {
                var ws = GetWorkshopByUserId(user.Id);

                if (ws != null)
                {
                    var distWorkshop = db.DistributorWorkShops.FirstOrDefault(dw => dw.WorkShopId == ws.WorkShopId)?.Distributor.DistributorName;
                    if (distWorkshop != null)
                    {
                        selListItem.Add(new SelectListItem { Value = user.Id, Text = distWorkshop });
                    }
                }
            }
            var distSelectList = new SelectList(selListItem, "Value", "Text", null);
            var ds = new WorkShopDistributors
            {
                Distributors = distSelectList
            };
            return ds;
        }
        #endregion

        /// <summary>
        /// Get list of workshop's matching users distributors.
        /// </summary>
        /// <param name="username">The username of the workshop.</param>
        /// <returns>List of workshop distributor.</returns>
        public List<WorkShopDistributor> GetWorkshopDistributorsForApp(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                username = General.GetUsername();
            }

            string[] usernameStr = username.Split('_');
            username = usernameStr[0];

            var wsMatchingUsers = (from u in db.UserDetails.AsNoTracking()
                                   join a in db.AspNetUsers.AsNoTracking() on u.UserId equals a.Id
                                   where u.IsDeleted.Value == false && a.UserName.Contains(username)
                                   select a).ToList();

            var wsDistributors = new List<WorkShopDistributor>();
            foreach (var wsUser in wsMatchingUsers)
            {
                var ws = GetWorkshopByUserId(wsUser.Id);
                if (ws == null) continue;

                var distributorWorkShop = db.DistributorWorkShops.AsNoTracking().FirstOrDefault(dw => dw.WorkShopId == ws.WorkShopId);
                if (distributorWorkShop != null)
                {
                    wsDistributors.Add(new WorkShopDistributor
                    {
                        Text = distributorWorkShop.Distributor.DistributorName,
                        Value = wsUser.Id,
                        Username = distributorWorkShop.AspNetUser.UserName,
                        UPIID= distributorWorkShop.Distributor.UPIID
                    });
                }
            }

            return wsDistributors;
        }

        #region Add FCMToken
        public void AddFCMToken(string userId, string fcmToken)
        {
            var userDetails = db.UserDetails.Where(a => a.UserId == userId).FirstOrDefault();
            if (userDetails != null)
            {
                userDetails.FcmToken = fcmToken;
                db.SaveChanges();
            }
        }
        #endregion
    }
}