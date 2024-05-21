using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Garaaz.Models
{
    public class RepoRequestPartFilter
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General Img = new General();
        #endregion

        #region Get All RequestPartFilter
        public List<RequestPartViewModel> GetAllRequestPartFilter(ClsPartfilterRequest model)
        {
            var data = db.RequestPartFilters.Where(a => a.DistributorId == model.DistributorId).Select(s => new RequestPartViewModel
            {
                Id = s.Id,
                DistributorId = s.DistributorId,
                CarMake = s.CarMake,
                ModelNumber = s.Model,
                Year = s.Year,
                Modification = s.Modification,
                PartGroup = s.PartGroup,
                PartNumber = s.PartNum,
                RootPartNumber = s.RootPartNum,
                PartDescription = s.PartDesc,
            }).ToList();
            return data;
        }
        #endregion

        #region Get RequestPartFilter By Id
        public clsPartfilter GetRequestPartFilterById(int Id)
        {
            var data = (from a in db.RequestPartFilters
                        where a.Id == Id
                        select a).Select(s => new clsPartfilter
                        {
                            Id = s.Id,
                            DistributorId = s.DistributorId,
                            CarMake = s.CarMake,
                            ModelNumber = s.Model,
                            Year = s.Year,
                            Modification = s.Modification,
                            PartGroup = s.PartGroup,
                            PartNumber = s.PartNum,
                            RootPartNumber = s.RootPartNum,
                            PartDescription = s.PartDesc,
                        }).FirstOrDefault();
            return data;
        }
        #endregion

        #region Add Or Update RequestPartFilter
        public bool AddOrUpdateRequestPartFilter(clsPartfilter model)
        {
            if (model.Id > 0)
            {
                var data = db.RequestPartFilters.Where(x => x.Id == model.Id).FirstOrDefault();
                if (data != null)
                {
                    data.DistributorId = model.DistributorId;
                    data.CarMake = model.CarMake;
                    data.Model = model.ModelNumber;
                    data.Year = model.Year;
                    data.Modification = model.Modification;
                    data.PartGroup = model.PartGroup;
                    data.PartNum = model.PartNumber;
                    data.RootPartNum = model.RootPartNumber;
                    data.PartDesc = model.PartDescription;
                    db.SaveChanges();
                    return true;
                }
            }
            db.RequestPartFilters.Add(new RequestPartFilter()
            {
                DistributorId = model.DistributorId,
                CarMake = model.CarMake,
                Model = model.ModelNumber,
                Year = model.Year,
                Modification = model.Modification,
                PartGroup = model.PartGroup,
                PartNum = model.PartNumber,
                RootPartNum = model.RootPartNumber,
                PartDesc = model.PartDescription
            });
            return db.SaveChanges() > 0;
        }
        #endregion

        #region Delete RequestPartFilter
        public bool DeleteRequestPartFilter(int Id)
        {
            var data = db.RequestPartFilters.Where(x => x.Id == Id).FirstOrDefault();
            if (data != null)
            {
                db.RequestPartFilters.Remove(data);
                return db.SaveChanges() > 0;
            }
            return false;
        }
        #endregion

        #region Get PartRequest Filters
        public PartRequestFilters GetPartRequestFilters(ClsPartfilterRequest model)
        {
            bool isSuperAdmin = false;
            var filters = new PartRequestFilters();
            if (model.DistributorId == 0)
            {
                RepoUsers rp = new RepoUsers();
                var roles = rp.GetRolesByUserId(model.UserId);
                var role = roles?.FirstOrDefault();
                if (roles != null && roles.Contains(Constants.SuperAdmin))
                {
                    isSuperAdmin = true;
                }
                else if (roles != null && roles.Contains(Constants.Workshop))
                {
                    role = Constants.Workshop;
                }
                model.DistributorId = rp.getDistributorIdByUserId(model.UserId, role);
                if (model.DistributorId == 0) { throw new Exception($"Distributor can't find using UserId {model.UserId}"); }
            }
            var data = new List<RequestPartFilter>();
            if (isSuperAdmin)
            {
                data = db.RequestPartFilters.AsNoTracking().ToList();
            }
            else
            {
                data = db.RequestPartFilters.AsNoTracking().Where(a => a.DistributorId == model.DistributorId).ToList();
            }

            if (data.Any())
            {
                filters.CarMake = new List<RequestPartFilterValue>();
                filters.Model = new List<RequestPartFilterValue>();
                filters.Year = new List<RequestPartFilterValue>();
                filters.Modification = new List<RequestPartFilterValue>();
                // filter carMake data
                var carmakeList = (from a in data
                                   where !string.IsNullOrEmpty(a.CarMake)
                                   group a by a.CarMake into g
                                   select new RequestPartFilterValue
                                   {
                                       Value = g.Key,
                                       Text = g.Key
                                   }).OrderBy(a => a.Text).ToList();
                filters.CarMake.AddRange(carmakeList);

                // filter year data
                var years = new List<string>();
                var yearStrList = (from a in data where !string.IsNullOrEmpty(a.Year) group a by a.Year into g select g.Key).ToList();
                foreach (var item in yearStrList)
                {
                    string[] str = item.Split(',');

                    foreach (var year in str)
                    {
                        years.Add(year);
                    }
                }

                var yearList = (from a in years
                                group a by a into g
                                select new RequestPartFilterValue
                                {
                                    Value = g.Key.ToString(),
                                    Text = g.Key.ToString()
                                }).OrderBy(a => a.Text).ToList();

                filters.Year.AddRange(yearList);

                if (!string.IsNullOrEmpty(model.Year))
                {
                    data = data.Where(a => model.Year.Contains(a.Year)).ToList();
                }

                // filter modification data
                var modificationList = (from a in data
                                        group a by a.Modification into g
                                        select new RequestPartFilterValue
                                        {
                                            Value = g.Key,
                                            Text = g.Key
                                        }).OrderBy(a => a.Text).ToList();
                filters.Modification.AddRange(modificationList);

                // filter other ddls based on selected CarMake
                if (!string.IsNullOrEmpty(model.CarMake))
                {
                    data = data.Where(a => a.CarMake == model.CarMake).ToList();
                }

                // filter model data
                var modelList = (from a in data
                                 where !string.IsNullOrEmpty(a.Model)
                                 group a by a.Model into g
                                 select new RequestPartFilterValue
                                 {
                                     Value = g.Key,
                                     Text = g.Key
                                 }).OrderBy(a => a.Text).ToList();
                filters.Model.AddRange(modelList);

                if (!string.IsNullOrEmpty(model.ModelNumber))
                {
                    data = data.Where(a => a.Model == model.ModelNumber).ToList();
                }
            }
            return filters;
        }
        #endregion

        #region Get RequestPartFilters
        public List<ProductModel> GetRequestPartFilters(RequestNewPart model, out int totalRecords)
        {
            var datalist = new List<ProductModel>();

            RepoUsers rp = new RepoUsers();
            var roles = rp.GetRolesByUserId(model.UserId);
            var role = roles?.FirstOrDefault();
            if (roles != null && roles.Contains(Constants.SuperAdmin))
            {
                role = Constants.SuperAdmin;
            }
            else if (roles != null && roles.Contains(Constants.Distributor))
            {
                role = Constants.Distributor;
            }
            else if (roles != null && roles.Contains(Constants.Workshop))
            {
                role = Constants.Workshop;
            }
            else if (roles != null && roles.Contains(Constants.Users))
            {
                role = Constants.Users;
            }
            if (role != Constants.SuperAdmin)
            {
                model.DistributorId = rp.getDistributorIdByUserId(model.UserId, role);
                if (model.DistributorId == 0) { throw new Exception($"Distributor can't find using UserId {model.UserId}"); }
            }
            var requestPartFilters = new List<RequestPartFilter>();

            IQueryable<RequestPartFilter> query;
            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                if (model.DistributorId > 0)
                {
                    query = db.RequestPartFilters.Where(a => a.DistributorId == model.DistributorId);
                }
                else { query = db.RequestPartFilters; }

                if (!string.IsNullOrEmpty(model.CarMake))
                {
                    query = query.Where(a => a.CarMake == model.CarMake);
                }
                if (!string.IsNullOrEmpty(model.ModelNumber))
                {
                    query = query.Where(a => a.Model == model.ModelNumber);
                }
                if (!string.IsNullOrEmpty(model.Year))
                {
                    query = query.Where(a => a.Year.Contains(model.Year));
                }
                if (!string.IsNullOrEmpty(model.Modification))
                {
                    query = query.Where(a => a.Modification == model.Modification);
                }
                if (!string.IsNullOrEmpty(model.PartName))
                {
                    query = query.Where(a => a.PartGroup.Contains(model.PartName) || a.PartDesc.Contains(model.PartName));
                }

                totalRecords = query.GroupBy(g => new { g.PartNum, g.RootPartNum }).Count();
                int take = model.PageSize.Value;
                int skip = 0;

                if (model.PageNumber > 0)
                {
                    skip = model.PageNumber * model.PageSize.Value;
                }

                var data = query.GroupBy(g => new { g.PartNum, g.RootPartNum }).Select(r => new
                {
                    PartNum = r.Key.PartNum,
                    RootPartNum = r.Key.RootPartNum,
                    PartDesc = r.Select(p => p.PartDesc).FirstOrDefault()
                }).OrderBy(p => p.PartNum).Skip(skip).Take(take).AsNoTracking().ToList();

                requestPartFilters = data.Select(d => new RequestPartFilter
                {
                    PartNum = d.PartNum,
                    RootPartNum = d.RootPartNum,
                    PartDesc = d.PartDesc
                }).ToList();
            }

            var partNumbers = requestPartFilters.Select(n => n.PartNum);
            var products = db.Products.Where(p => partNumbers.Contains(p.PartNo) && p.DistributorId == (model.DistributorId != 0 ? model.DistributorId : p.DistributorId)).Select(p => new
            {
                p.ProductId,
                p.GroupId,
                p.ProductName,
                p.Description,
                p.Price,
                p.CurrentStock,
                p.ImagePath,
                p.PartNo,
                p.RootPartNum,
                p.BrandId
            }).AsNoTracking().ToList();

            IQueryable<TempOrderDetail> tempOrderDetails = Enumerable.Empty<TempOrderDetail>().AsQueryable();

            if (model.TempOrderId.HasValue)
            {
                tempOrderDetails = db.TempOrderDetails.Where(a => a.TempOrderId == model.TempOrderId.Value);
            }
            foreach (var item in requestPartFilters)
            {
                var product = products?.Where(p => p.PartNo == item.PartNum && p.RootPartNum == item.RootPartNum).FirstOrDefault();
                if (product == null) { continue; }

                var productId = product != null ? product.ProductId : 0;
                var cartItem = tempOrderDetails.Where(a => a.ProductId == productId).FirstOrDefault();

                var brandId = product != null ? product.BrandId ?? 0 : 0;
                var isOriparts = db.Brands.Where(b => b.BrandId == brandId).FirstOrDefault()?.IsOriparts;

                var productModel = new ProductModel()
                {
                    ProductId = productId,
                    GroupId = product != null ? product.GroupId ?? 0 : 0,
                    ProductName = product != null ? (string.IsNullOrEmpty(product.ProductName) ? product.Description : product.ProductName) : "",
                    PartNumber = item.PartNum,
                    Description = item.PartDesc,
                    Price = product != null ? (!product.Price.HasValue ? "0" : String.Format("{0:#,###}", product.Price)) : "0",
                    ImagePath = Img.CheckImageUrl(product?.ImagePath),
                    Stock = product != null ? product.CurrentStock ?? 0 : 0,
                    CartQty = cartItem?.Qty,
                    CartAvailabilityType = cartItem?.AvailabilityType,
                    CartOutletId = cartItem?.OutletId,
                    IsOriparts = isOriparts != null ? isOriparts.Value : false,
                };

                datalist.Add(productModel);
            }

            return datalist;
        }
        #endregion

        #region Group Auto Complete
        public List<string> GroupAutoComplete(RequestNewPart model)
        {

            var groups = new List<string>();
            RepoUsers rp = new RepoUsers();
            var roles = rp.GetRolesByUserId(model.UserId);
            var role = roles?.FirstOrDefault();
            if (roles != null && roles.Contains(Constants.SuperAdmin))
            {
                role = Constants.SuperAdmin;
            }
            else if (roles != null && roles.Contains(Constants.Distributor))
            {
                role = Constants.Distributor;
            }
            else if (roles != null && roles.Contains(Constants.Workshop))
            {
                role = Constants.Workshop;
            }
            else if (roles != null && roles.Contains(Constants.Users))
            {
                role = Constants.Users;
            }
            if (role != Constants.SuperAdmin)
            {
                model.DistributorId = rp.getDistributorIdByUserId(model.UserId, role);
                if (model.DistributorId == 0) { throw new Exception($"Distributor can't find using UserId {model.UserId}"); }
            }
            IQueryable<RequestPartFilter> query;
            using (var context = new garaazEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                if (model.DistributorId > 0)
                {
                    query = db.RequestPartFilters.Where(a => a.DistributorId == model.DistributorId);
                }
                else { query = db.RequestPartFilters; }

                if (!string.IsNullOrEmpty(model.CarMake))
                {
                    query = query.Where(a => a.CarMake == model.CarMake);
                }
                if (!string.IsNullOrEmpty(model.ModelNumber))
                {
                    query = query.Where(a => a.Model == model.ModelNumber);
                }
                if (!string.IsNullOrEmpty(model.Year))
                {
                    query = query.Where(a => a.Year.Contains(model.Year));
                }
                if (!string.IsNullOrEmpty(model.Modification))
                {
                    query = query.Where(a => a.Modification == model.Modification);
                }

                var groupQuery = query.Where(a => a.PartGroup.StartsWith(model.PartName));

                groups = groupQuery.OrderBy(a => a.PartGroup).GroupBy(g => g.PartGroup).Select(r => r.Key).AsNoTracking().ToList();
                // group by partDesc if group not matched
                if (groups.Count == 0)
                {
                    query = query.Where(a => a.PartDesc.StartsWith(model.PartName));

                    groups = query.OrderBy(a => a.PartDesc).GroupBy(g => g.PartDesc).Select(r => r.Key).AsNoTracking().ToList();
                }
                return groups;
            }
        }
        #endregion
    }
}