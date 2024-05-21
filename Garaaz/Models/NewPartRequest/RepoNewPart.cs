using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Garaaz.Models
{
    public class RepoNewPart
    {
        private readonly garaazEntities _db = new garaazEntities();
        private const string NotAvailable = "NA";

        #region New Part Request

        public List<NewPartRequestModel> GetNewPartRequests(string role)
        {
            var newPartRequests = new List<NewPartRequestModel>();

            if (role.Equals(Constants.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                newPartRequests = (from n in _db.NewPartRequests.AsNoTracking()
                                   join dw in _db.DistributorWorkShops.AsNoTracking() on n.UserId equals dw.UserId
                                   join w in _db.WorkShops.AsNoTracking() on dw.WorkShopId equals w.WorkShopId
                                   join u in _db.UserDetails.Include(x => x.AspNetUser).AsNoTracking() on dw.UserId equals u.UserId
                                   where n.IsDeleted == false
                                   select new NewPartRequestModel
                                   {
                                       Id = n.Id,
                                       Status = ((RequestStatus)n.Status).ToString(),
                                       CarMake = n.CarMake,
                                       Model = n.Model,
                                       Modification = n.Modification,
                                       PartNumAndQty = n.PartNumAndQty,
                                       Search = n.Search,
                                       Year = n.Year,
                                       CreatedDate = n.CreatedDate,
                                       WorkshopCode = u.ConsPartyCode,
                                       WorkshopName = w.WorkShopName,
                                       Mobile = u.AspNetUser != null ? u.AspNetUser.UserName : ""
                                   }).ToList();
            }
            else if (role.Equals(Constants.Distributor, StringComparison.OrdinalIgnoreCase))
            {
                var loginUserId = General.GetUserId();
                var general = new General();
                var distributorId = general.GetDistributorId(loginUserId);

                newPartRequests = (from n in _db.NewPartRequests.AsNoTracking()
                                   join dw in _db.DistributorWorkShops.AsNoTracking() on n.UserId equals dw.UserId
                                   join w in _db.WorkShops.AsNoTracking() on dw.WorkShopId equals w.WorkShopId
                                   join u in _db.UserDetails.Include(x => x.AspNetUser).AsNoTracking() on dw.UserId equals u.UserId
                                   where n.IsDeleted == false && dw.DistributorId == distributorId
                                   select new NewPartRequestModel
                                   {
                                       Id = n.Id,
                                       Status = ((RequestStatus)n.Status).ToString(),
                                       CarMake = n.CarMake,
                                       Model = n.Model,
                                       Modification = n.Modification,
                                       PartNumAndQty = n.PartNumAndQty,
                                       Search = n.Search,
                                       Year = n.Year,
                                       CreatedDate = n.CreatedDate,
                                       WorkshopCode = u.ConsPartyCode,
                                       WorkshopName = w.WorkShopName,
                                       Mobile = u.AspNetUser != null ? u.AspNetUser.UserName : ""
                                   }).ToList();
            }
            else if (role.Equals(Constants.RoIncharge, StringComparison.OrdinalIgnoreCase) || role.Equals(Constants.DistributorOutlets, StringComparison.OrdinalIgnoreCase))
            {
                RepoUsers repoUser = new RepoUsers();
                var outletId = repoUser.GetOutletByUserId(General.GetUserId())?.OutletId ?? 0;

                newPartRequests = (from n in _db.NewPartRequests.AsNoTracking()
                                   join dw in _db.DistributorWorkShops.AsNoTracking() on n.UserId equals dw.UserId
                                   join w in _db.WorkShops.AsNoTracking() on dw.WorkShopId equals w.WorkShopId
                                   join u in _db.UserDetails.Include(x => x.AspNetUser).AsNoTracking() on dw.UserId equals u.UserId
                                   where n.IsDeleted == false && w.outletId == outletId
                                   select new NewPartRequestModel
                                   {
                                       Id = n.Id,
                                       Status = ((RequestStatus)n.Status).ToString(),
                                       CarMake = n.CarMake,
                                       Model = n.Model,
                                       Modification = n.Modification,
                                       PartNumAndQty = n.PartNumAndQty,
                                       Search = n.Search,
                                       Year = n.Year,
                                       CreatedDate = n.CreatedDate,
                                       WorkshopCode = u.ConsPartyCode,
                                       WorkshopName = w.WorkShopName,
                                       Mobile = u.AspNetUser != null ? u.AspNetUser.UserName : ""
                                   }).ToList();
            }
            else
            {
                var loginUserId = General.GetUserId();

                newPartRequests = (from n in _db.NewPartRequests.AsNoTracking()
                                   join dw in _db.DistributorWorkShops.AsNoTracking() on n.UserId equals dw.UserId
                                   join w in _db.WorkShops.AsNoTracking() on dw.WorkShopId equals w.WorkShopId
                                   join u in _db.UserDetails.Include(x => x.AspNetUser).AsNoTracking() on dw.UserId equals u.UserId
                                   where n.IsDeleted == false && dw.UserId == loginUserId
                                   select new NewPartRequestModel
                                   {
                                       Id = n.Id,
                                       Status = ((RequestStatus)n.Status).ToString(),
                                       CarMake = n.CarMake,
                                       Model = n.Model,
                                       Modification = n.Modification,
                                       PartNumAndQty = n.PartNumAndQty,
                                       Search = n.Search,
                                       Year = n.Year,
                                       CreatedDate = n.CreatedDate,
                                       WorkshopCode = u.ConsPartyCode,
                                       WorkshopName = w.WorkShopName,
                                       Mobile = u.AspNetUser != null ? u.AspNetUser.UserName : ""
                                   }).ToList();
            }
            return newPartRequests;
        }

        public bool SaveNewPartRequest(NewPartRequestModel nprModel,out string imagePath)
        {
            var userDetail = _db.UserDetails.FirstOrDefault(u => u.UserId == nprModel.UserId);
            if (userDetail == null)
            {
                throw new Exception($"No user found matching with user id {nprModel.UserId}");
            }

             imagePath = "";
            // save image if base64 data available
            if(!string.IsNullOrWhiteSpace(nprModel.Base64Image)&&nprModel.Base64Image!="")
            {
                imagePath=Utils.SaveImageUsingBase64(nprModel.Base64Image);
            }

            // If same existing request
            var npRequest = _db.NewPartRequests.FirstOrDefault(n => n.UserId == nprModel.UserId && n.CarMake == nprModel.CarMake && n.Model == nprModel.Model && n.Modification == nprModel.Modification && n.Year == nprModel.Year && n.Search == nprModel.Search && n.PartNumAndQty == nprModel.PartNumAndQty);
            if (npRequest != null)
            {
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    npRequest.ImagePath = imagePath;
                    _db.SaveChanges();
                }
                return true;
            }

            npRequest = new NewPartRequest
            {
                UserId = nprModel.UserId,
                Status = (int)RequestStatus.Pending,
                CarMake = nprModel.CarMake,
                Model = nprModel.Model,
                Modification = nprModel.Modification,
                Year = nprModel.Year,
                Search = nprModel.Search,
                PartNumAndQty = nprModel.PartNumAndQty,
                CreatedDate = DateTime.Now,
                ImagePath= imagePath
            };
            _db.NewPartRequests.Add(npRequest);
            var requestSaved = _db.SaveChanges() > 0;

            if (!requestSaved) return false;

            // Create notification
            var msg = string.IsNullOrWhiteSpace(userDetail.FirstName) ? $"New part request placed by {userDetail.ConsPartyCode}" : $"New part request placed by {userDetail.FirstName} {userDetail.LastName}";
            var notification = new Notification
            {
                UserId = nprModel.UserId,
                Type = Constants.PartRequestPlaced,
                Message = msg,
                IsRead = false,
                CreatedDate = DateTime.Now,
                RefUserId = nprModel.RefUserId ?? nprModel.UserId,
                WorkshopId = null
            };
            _db.Notifications.Add(notification);
            return _db.SaveChanges() > 0;
        }

        public bool DeleteNewPartRequest(int id)
        {
            var newPartRequest = _db.NewPartRequests.FirstOrDefault(n => n.Id == id);
            if (newPartRequest == null)
            {
                throw new Exception($"No part request found matching with id {id}");
            }

            newPartRequest.IsDeleted = true;
            _db.SaveChanges();
            return true;
        }

        #endregion

        #region Approve Part Request

        public List<DevbridgeData> SearchPart(string searchTxt, int requestId)
        {

            var distributorId = (from d in _db.DistributorWorkShops
                                 join p in _db.NewPartRequests on d.UserId equals p.UserId
                                 where p.Id == requestId
                                 select d.DistributorId
                 ).FirstOrDefault();
            if (distributorId == null) { new List<DevbridgeData>(); }

            var products = _db.Products.Where(p => (p.PartNo.Contains(searchTxt) || p.Description.Contains(searchTxt)) && p.DistributorId == distributorId).AsNoTracking().Select(p => new
            {
                p.PartNo,
                p.Description,
                Price = p.Price ?? 0
            }).ToList();

            return products.DistinctBy(p => p.PartNo).Select(p => new DevbridgeData
            {
                value = $"{p.Description} ({p.PartNo})",
                data = $"{p.PartNo}||{p.Price}"
            }).ToList();
        }

        public PartRequestModel GetApprovePartRequest(int requestId)
        {
            var npr = _db.NewPartRequests.AsNoTracking().FirstOrDefault(n => n.Id == requestId);
            if (npr == null) return null;

            var aprModel = new PartRequestModel
            {
                RequestId = requestId,
                CarMake = string.IsNullOrWhiteSpace(npr.CarMake) ? NotAvailable : npr.CarMake,
                Model = string.IsNullOrWhiteSpace(npr.Model) ? NotAvailable : npr.Model,
                Year = npr.Year,
                Modification = string.IsNullOrWhiteSpace(npr.Modification) ? NotAvailable : npr.Modification,
                Search = string.IsNullOrWhiteSpace(npr.Search) ? NotAvailable : npr.Search,
                PartNumAndQty = string.IsNullOrWhiteSpace(npr.PartNumAndQty) ? NotAvailable : npr.PartNumAndQty,
                Status = ((RequestStatus)npr.Status).ToString(),
                ImagePath=npr.ImagePath,
                ApprovedPartRequests = _db.ApprovedPartRequests.Where(r => r.RequestId == requestId).AsNoTracking().ToList()
            };

            return aprModel;
        }

        public UserRequestsAndParts GetUserRequestsAndParts(NewPartRequestModel model, string role)
        {
            var rp = new RepoUsers();
            var distributorId = rp.getDistributorIdByUserId(model.UserId, role);
            var userRp = new UserRequestsAndParts();

            var newPartRequests = _db.NewPartRequests.Where(n => n.UserId == model.UserId && n.IsDeleted == false).AsNoTracking().ToList();
            var requestIds = newPartRequests.Select(n => n.Id);

            var dbApprovedPartRequests = (from p in _db.Products.Include(x => x.Brand).AsNoTracking()
                                          join a in _db.ApprovedPartRequests on p.PartNo equals a.PartNumber
                                          where p.DistributorId == distributorId && p.GroupId != null
                                          && requestIds.Contains(a.RequestId)
                                          select new ApprovedPartRequestModel
                                          {
                                              IsOriparts = p.Brand != null ? p.Brand.IsOriparts ?? false : false,
                                              RequestId = a.RequestId,
                                              ApprovedPartRequestId = a.Id,
                                              ApproverId = a.ApproverId,
                                              PartNumber = a.PartNumber,
                                              Description = a.Description,
                                              Price = a.Price,
                                              ProductId = p.ProductId,
                                              Stock = p.CurrentStock ?? 0
                                          }).ToList();
            if (model.TempOrderId.HasValue)
            {
                var tempOrderDetails = _db.TempOrderDetails.Where(a => a.TempOrderId == model.TempOrderId.Value).AsNoTracking().ToList();

                foreach (var req in dbApprovedPartRequests)
                {
                    var cartItem = tempOrderDetails.Where(a => a.ProductId == req.ProductId).FirstOrDefault();

                    req.CartQty = cartItem != null ? cartItem.Qty : 0;
                    req.CartAvailabilityType = cartItem != null ? cartItem.AvailabilityType : "";
                    req.CartOutletId = cartItem != null ? cartItem.OutletId : null;
                }
            }
            foreach (var npr in newPartRequests)
            {
                var request = $"Request - {npr.Id}";
                var approvedPartRequests = dbApprovedPartRequests.Where(a => a.RequestId == npr.Id).ToList();
                userRp.Requests.Add(new UserRequest
                {
                    Request = request,
                    Date = npr.CreatedDate.ToString("dd MMM,yyyy"),
                    Status = ((RequestStatus)npr.Status).ToString(),
                    TotalPrice = $"₹{approvedPartRequests.Sum(a => a.Price)}"
                });

                userRp.RequestAndParts.Add(new UserRequestAndParts
                {
                    Request = request,
                    RequestId = npr.Id,
                    ApprovedPartRequests = approvedPartRequests
                });
            }

            return userRp;
        }

        public bool SavePartsForPartRequest(ApproveRequestParam arParam)
        {
            var request = _db.NewPartRequests.FirstOrDefault(n => n.Id == arParam.RequestId);
            if (request == null)
            {
                throw new Exception($"New part request doesn't exist for request Id {arParam.RequestId}");
            }

            if (arParam == null)
            {
                throw new ArgumentNullException(nameof(arParam));
            }

            _db.Database.ExecuteSqlCommand($"DELETE FROM ApprovedPartRequest WHERE RequestId={arParam.RequestId}");

            var approverId = General.GetUserId();
            if (arParam.Parts.Count <= 0) return false;

            foreach (var apr in arParam.Parts.DistinctBy(p => p.PartNumber))
            {
                _db.ApprovedPartRequests.Add(new ApprovedPartRequest
                {
                    ApproverId = approverId,
                    RequestId = arParam.RequestId,
                    PartNumber = apr.PartNumber,
                    Description = apr.Description,
                    Price = apr.Price
                });
            }
            var partsSaved = _db.SaveChanges() > 0;

            if (partsSaved)
            {
                request.Status = (int)RequestStatus.Completed;
                _db.SaveChanges();
            }

            return partsSaved;
        }

        public bool DeletePartForPartRequest(ApproveRequestParam arParam)
        {
            var apr = _db.ApprovedPartRequests.FirstOrDefault(r => r.RequestId == arParam.RequestId && r.PartNumber == arParam.PartNumber);
            if (apr == null)
            {
                throw new Exception($"No part found matching with part number {arParam.PartNumber}");
            }

            _db.ApprovedPartRequests.Remove(apr);
            return _db.SaveChanges() > 0;
        }

        #endregion
    }
}