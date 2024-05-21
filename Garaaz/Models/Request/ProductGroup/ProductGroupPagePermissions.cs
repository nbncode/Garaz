using System;
using System.Collections.Generic;
using System.Linq;

namespace Garaaz.Models
{
    public class ProductGroupPagePermissions
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General Img = new General();
        #endregion

        #region Get All Product Group
        public List<clsProductGroup> GetAllProductGroup(int schemeId)
        {
            var scheme = db.Schemes.Where(x => x.SchemeId == schemeId).FirstOrDefault();
            var productGroups = db.ProductGroups;
            General Img = new General();
            var data = (from p in productGroups
                        select new clsProductGroup()
                        {
                            GroupId = p.GroupId,
                            GroupName = p.GroupName,
                            ParentName = p.ParentId.HasValue ? productGroups.Where(a => a.GroupId == p.ParentId).FirstOrDefault().GroupName : "",
                            ImagePath = p.ImagePath
                        }).ToList();

            data.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));
            if (scheme != null)
            {
                if (!string.IsNullOrEmpty(scheme.PartCategory) && !scheme.PartCategory.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    var partCategoryCodes = scheme.PartCategory.Split(',').ToList();

                    if (scheme.PartCategory.Contains("M"))
                    {
                        partCategoryCodes.Add(Constants.MFull);
                    }

                    if (scheme.PartCategory.Contains("AA"))
                    {
                        partCategoryCodes.Add(Constants.AAFull);
                    }

                    if (scheme.PartCategory.Contains("AG"))
                    {
                        partCategoryCodes.Add(Constants.AGFull);
                    }
                    if (scheme.PartCategory.Contains("T"))
                    {
                        partCategoryCodes.Add(Constants.TFull);
                    }

                    data = (from g in data
                            join p in db.Products on g.GroupId equals p.GroupId
                            where p.DistributorId == scheme.DistributorId && p.PartCategoryCode != null && partCategoryCodes.Contains(p.PartCategoryCode)
                            select g).Distinct().ToList();
                }
            }
            return data;
        }
        #endregion

        #region Get Product Group Distributor wise
        public List<clsProductGroup> GetProductGroupByDistributorId(string userId, string role)
        {
            int? distributorId = null;
            distributorId = userId.GetDistributorId(role);
            var data = (from p in db.ProductGroups
                        where p.DistributorId == (role != Constants.SuperAdmin ? (distributorId == 0 ? -1 : (distributorId ?? -1)) : p.DistributorId)
                        select new clsProductGroup()
                        {
                            GroupId = p.GroupId,
                            GroupName = p.GroupName
                        }).ToList();
            return data;
        }
        #endregion

        #region Get all product group by Group id
        public List<clsProductGroup> GetAllProductGroupByGroupId(int groupId)
        {
            var productGroups = db.ProductGroups;
            var productGroup = from p in productGroups
                               where p.ParentId == groupId
                               select new clsProductGroup()
                               {
                                   GroupId = p.GroupId,
                                   GroupName = p.GroupName,
                                   ParentName = p.ParentId.HasValue ? productGroups.Where(a => a.GroupId == p.ParentId).FirstOrDefault().GroupName : "",
                                   ImagePath = p.ImagePath
                               };

            General Img = new General();
            var lstProductGroup = new List<clsProductGroup>();
            foreach (var item in productGroup)
            {
                //item.ImagePath = Img.CheckImageurl(item.ImagePath);
                item.childs = GetProductGroupsByGroupID(item.GroupId);
                lstProductGroup.Add(item);
            }
            lstProductGroup.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));
            return lstProductGroup;
        }
        #endregion

        #region Get Product Group Name By Id
        public clsProductGroup GetProductGroupNameById(int GroupId)
        {
            var result = db.ProductGroups.Where(x => x.GroupId == GroupId).Select(a => new clsProductGroup()
            {
                GroupId = a.GroupId,
                GroupName = a.GroupName,
                ParentId = a.ParentId.Value,
                ImagePath = a.ImagePath,
                //VariantId = a.VariantId.Value,
                //VehicleId = a.Variant.VehicleId.Value,
                BrandId = a.BrandId.Value,
                DistributorId = a.DistributorId.Value
            }).FirstOrDefault();
            if (result != null)
            {
                result.ImagePath = Img.CheckImageUrl(result.ImagePath);
            }
            return result;
        }
        #endregion

        #region Save Product Group
        public bool SaveProductGroup(ProductGroup model)
        {

            var old = db.ProductGroups.Where(m => m.GroupId == model.GroupId).FirstOrDefault();
            if (old != null)
            {
                old.GroupName = model.GroupName;
                old.ParentId = model.ParentId;
                old.ImagePath = model.ImagePath;
                old.BrandId = model.BrandId;
                old.DistributorId = model.DistributorId;
                return db.SaveChanges() > 0;
            }
            else
            {
                db.ProductGroups.Add(model);
                return db.SaveChanges() > 0;
            }
        }
        #endregion]

        #region Delete Product Group
        public bool DeleteProductGroup(int Id)
        {
            var data = db.ProductGroups.Where(m => m.GroupId == Id).FirstOrDefault();
            if (data == null)
            {
                return false;
            }
            else
            {
                db.ProductGroups.Remove(data);
                return db.SaveChanges() > 0;
            }
        }
        #endregion

        #region Get Product Name for DropDownList
        public List<ProductGroup> ddlProductGroups()
        {
            var data = db.ProductGroups.ToList();
            data.Insert(0, new ProductGroup() { GroupName = "-- Please Select --" });
            return data;
        }
        #endregion

        #region Get Product Name for DropDownList
        public List<ProductGroupComboList> ComboProductGroups()
        {
            var data = db.ProductGroups.ToList();
            var productGroupList = new List<ProductGroupComboList>();
            productGroupList.Add(new ProductGroupComboList() { GroupId = null, GroupName = "-- Please Select --" });
            foreach (var item in data)
            {
                productGroupList.Add(new ProductGroupComboList() { GroupId = item.GroupId, GroupName = item.GroupName });
            }
            return productGroupList;
        }
        #endregion

        #region Get Products Group By Variant ID
        public List<clsProductGroup> GetProductGroupsByVariantID(int VariantID)
        {
            var productGroup = (from p in db.ProductGroups
                                where p.VariantId == VariantID
                                select new clsProductGroup()
                                {
                                    GroupId = p.GroupId,
                                    GroupName = p.GroupName,
                                    ParentId = p.ParentId,
                                    ImagePath = p.ImagePath
                                }).ToList();

            List<clsProductGroup> lstProductGroup = new List<clsProductGroup>();
            foreach (var item in productGroup)
            {
                item.childs = GetProductGroupsByGroupID(item.GroupId);
                lstProductGroup.Add(item);
            }
            lstProductGroup.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));

            return lstProductGroup;
        }

        public List<clsProductGroup> GetProductGroupsByGroupID(int GroupId)
        {
            var productGroup = (from p in db.ProductGroups
                                where p.ParentId == GroupId
                                select new clsProductGroup()
                                {
                                    GroupId = p.GroupId,
                                    GroupName = p.GroupName,
                                    ParentId = p.ParentId
                                }).ToList();

            List<clsProductGroup> lstProductGroup = new List<clsProductGroup>();
            foreach (var item in productGroup)
            {
                item.childs = GetProductGroupsByGroupID(item.GroupId);
                lstProductGroup.Add(item);
            }
            lstProductGroup.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));
            return lstProductGroup;
        }

        public ProductGroupWithBreadcrumb GetProductGroupsByVariantIDMobile(int VariantID)
        {
            var allProductGroup = db.ProductGroups;

            var productGroup = (from p in allProductGroup
                                where p.VariantId == VariantID
                                select new clsProductGroup()
                                {
                                    GroupId = p.GroupId,
                                    GroupName = p.GroupName,
                                    ParentId = p.ParentId,
                                    ImagePath = p.ImagePath,
                                    ChildCount = (allProductGroup.Where(a => a.ParentId == p.GroupId).Count())
                                }).ToList();

            productGroup.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));

            var Variant = db.Variants.Where(x => x.VariantId == VariantID).FirstOrDefault();
            List<string> lstbreadCrumbModels = new List<string>();
            lstbreadCrumbModels.Add(Variant.Vehicle.Brand.Name);
            lstbreadCrumbModels.Add(Variant.Vehicle.Name);
            lstbreadCrumbModels.Add(Variant.Name);
            return new ProductGroupWithBreadcrumb()
            {
                lstBreadCrumb = lstbreadCrumbModels,
                lstProductGroup = productGroup
            };
        }

        public ProductGroupWithBreadcrumb GetAllProductGroupByGroupIdMobile(int groupId)
        {
            var productGroups = db.ProductGroups;
            var productGroup = (from p in productGroups
                                where p.ParentId == groupId
                                select new clsProductGroup()
                                {
                                    GroupId = p.GroupId,
                                    GroupName = p.GroupName,
                                    ParentName = p.ParentId.HasValue ? productGroups.Where(a => a.GroupId == p.ParentId).FirstOrDefault().GroupName : "",
                                    ImagePath = p.ImagePath,
                                    ChildCount = (productGroups.Where(a => a.ParentId == p.GroupId).Count())
                                }).ToList();


            productGroup.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));

            var ProductGroups = db.ProductGroups.Where(x => x.GroupId == groupId).FirstOrDefault();
            List<string> lstbreadCrumbModels = new List<string>();
            lstbreadCrumbModels.Add(ProductGroups.Variant.Vehicle.Brand.Name);
            lstbreadCrumbModels.Add(ProductGroups.Variant.Vehicle.Name);
            lstbreadCrumbModels.Add(ProductGroups.Variant.Name);
            lstbreadCrumbModels.Add(ProductGroups.GroupName);
            return new ProductGroupWithBreadcrumb()
            {
                lstBreadCrumb = lstbreadCrumbModels,
                lstProductGroup = productGroup
            };
        }

        #endregion

        #region OrderPartAutoComplete
        public List<ProductModel> OrderPartAutoComplete(string searchText, string userId, string role)
        {
            int? distributorId = null;
            distributorId = userId.GetDistributorId(role);
            var data = (from p in db.Products
                        where p.DistributorId == (role != Constants.SuperAdmin ? (distributorId == 0 ? -1 : (distributorId ?? -1)) : p.DistributorId)
                        && p.PartNo.StartsWith(searchText)
                        select new ProductModel()
                        {
                            ProductId = p.ProductId,
                            PartNumber = p.PartNo
                        }).Take(20).ToList();
            return data;
        }
        public List<ProductModel> OrderPartProductDetail(OrderPartAutoComplete model, string userId, string role, out int totalRecords)
        {
            int? distributorId = null; totalRecords = 0;
            distributorId = userId.GetDistributorId(role);
            List<ProductModel> lstProduct = new List<ProductModel>();
            var data = (from p in db.Products
                        where p.DistributorId == (role != Constants.SuperAdmin ? (distributorId == 0 ? -1 : (distributorId ?? -1)) : p.DistributorId)
                        && p.PartNo.Contains(model.SearchText)
                        select p).ToList();
            totalRecords = data.Count();
            if (model.PageNumber > 0)
            {
                data = data.GetPaging<Product>(model.PageNumber, model.PageSize);
            }
            IQueryable<TempOrderDetail> tempOrderDetails = Enumerable.Empty<TempOrderDetail>().AsQueryable();

            if (model.TempOrderId.HasValue)
            {
                tempOrderDetails = db.TempOrderDetails.Where(a => a.TempOrderId == model.TempOrderId.Value);
            }
            foreach (var p in data)
            {
                var productModel = new ProductModel();
                productModel.ProductId = p.ProductId;
                productModel.PartNumber = p.PartNo;
                productModel.ProductName = string.IsNullOrEmpty(p.ProductName) ? p.Description : p.ProductName;
                productModel.Description = p.Description;
                productModel.Price = !p.Price.HasValue ? "0" : String.Format("{0:#,###}", p.Price);
                productModel.ImagePath = Img.CheckImageUrl(p.ImagePath);
                productModel.Stock = p.CurrentStock ?? 0;
                var cartItem = tempOrderDetails.Where(a => a.ProductId == productModel.ProductId).FirstOrDefault();
                if (cartItem != null)
                {
                    productModel.CartQty = cartItem.Qty;
                    productModel.CartAvailabilityType = cartItem.AvailabilityType;
                    productModel.CartOutletId = cartItem.OutletId;
                };
                var isOriparts = db.Brands.Where(b => b.BrandId == (p.BrandId != null ? p.BrandId.Value : 0)).FirstOrDefault()?.IsOriparts; //p.Brand?.IsOriparts;
                productModel.IsOriparts = isOriparts != null ? isOriparts.Value : false;
                lstProduct.Add(productModel);
            }
            return lstProduct;
        }
        #endregion

        #region Get Product GroupList Distributor wise
        public List<clsProductGroup> GetProductListGroupByDistributorId(string userId, string role)
        {
            int? distributorId = null;
            distributorId = userId.GetDistributorId(role);
            var productGroups = db.ProductGroups;
            var data = (from p in db.ProductGroups
                        where p.DistributorId == (role != Constants.SuperAdmin ? (distributorId == 0 ? -1 : (distributorId ?? -1)) : p.DistributorId)
                        select new clsProductGroup()
                        {
                            GroupId = p.GroupId,
                            GroupName = p.GroupName,
                            ParentName = p.ParentId.HasValue ? productGroups.Where(a => a.GroupId == p.ParentId).FirstOrDefault().GroupName : "",
                            ImagePath = p.ImagePath
                        }).ToList();

            data.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));
            return data;
        }
        #endregion

        #region Get Product Name for DropDownList
        public List<ProductGroup> ddlProductGroupsDistributor(int DistributorId)
        {
            var data = db.ProductGroups.Where(a => a.DistributorId == DistributorId).ToList();
            data.Insert(0, new ProductGroup() { GroupName = "-- Please Select --" });
            return data;
        }
        #endregion

        #region Get Product Name for DropDownList
        public List<ProductGroupComboList> ComboProductGroupsDistributor(int DistributorId)
        {
            var data = db.ProductGroups.Where(a => a.DistributorId == DistributorId).ToList();
            var productGroupList = new List<ProductGroupComboList>();
            productGroupList.Add(new ProductGroupComboList() { GroupId = null, GroupName = "-- Please Select --" });
            foreach (var item in data)
            {
                productGroupList.Add(new ProductGroupComboList() { GroupId = item.GroupId, GroupName = item.GroupName });
            }
            return productGroupList;
        }
        #endregion

        #region Get Product Part Category
        public List<PartCategory> GetProductPartCategory()
        {
            List<PartCategory> category = new List<PartCategory>
            {
             new PartCategory{Text="All", Value="All"},
             new PartCategory{Text="MGP", Value="M"},
             new PartCategory{Text="MGA", Value="AA"},
             new PartCategory{Text="MGO", Value="AG"},
             new PartCategory{Text="MGT", Value="T"},
            };

            return category;
        }
        #endregion
    }
}