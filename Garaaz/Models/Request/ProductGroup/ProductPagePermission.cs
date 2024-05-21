using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class ProductPagePermission
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General Img = new General();
        #endregion

        #region Get All Product
        public List<ResponseProductModel> GetAllProduct()
        {
            using (var context = new garaazEntities())
            {
                var result = (from p in context.Products.AsNoTracking()
                              select new ResponseProductModel()
                              {
                                  ProductId = p.ProductId,
                                  ProductName = string.IsNullOrEmpty(p.ProductName) ? p.Description : p.ProductName,
                                  GroupName = p.ProductGroup == null ? "" : p.ProductGroup.GroupName,
                                  PartNumber = p.PartNo,
                                  Description = p.Description,
                                  Remark = p.Remarks,
                                  ImagePath = p.ImagePath,
                                  PriceDecimal = p.Price.HasValue ? p.Price.Value : 0
                              }).ToList();

                result.ForEach(a => { a.ImagePath = Img.CheckImageUrl(!string.IsNullOrEmpty(a.ImagePath) ? a.ImagePath : "/assets/images/NoPhotoAvailable.png"); a.Price = string.Format("{0:#,###}", a.PriceDecimal); });
                return result;
            }
           
        }
        #endregion

        #region GetAllProduct with Datatable pagination
        public DataTableResponse GetAllProduct(DataTableRequest request)
        {
            DataTableResponse dataTable = new DataTableResponse();
            //Get All data
            var result = GetAllProduct();

            //Get Search
            if (!string.IsNullOrEmpty(request.search))
            {
                result = result.Where(a => a.ProductName.ToLower().Contains(request.search.ToLower()) || a.PartNumber.ToLower().Contains(request.search.ToLower())).ToList();
            }

            //Get Order
            if (request.start != -1)
            {
                if (request.dir == "asc")
                {
                    if (request.column == "ProductName")
                    {
                        result = result.OrderBy(a => a.ProductName).ToList();
                    }
                    else if (request.column == "PartNumber")
                    {
                        result = result.OrderBy(a => a.PartNumber).ToList();
                    }
                    else
                    {
                        result = result.OrderBy(a => a.ProductName).ToList();
                    }
                }
                else
                {
                    if (request.column == "ProductName")
                    {
                        result = result.OrderByDescending(a => a.ProductName).ToList();
                    }
                    else if (request.column == "PartNumber")
                    {
                        result = result.OrderByDescending(a => a.PartNumber).ToList();
                    }
                    else
                    {
                        result = result.OrderByDescending(a => a.ProductName).ToList();
                    }
                }
            }
            else
            {
                result = result.OrderBy(a => a.ProductName).ToList();
            }

            //set Total Numbers Of Records
            dataTable.recordsFiltered = result.Count;
            dataTable.recordsTotal = result.Count;

            if (request.start != -1)
            {
                //Get Pagation
                result = result.Skip(request.start).Take(request.length).ToList();
            }
            dataTable.draw = request.draw;
            dataTable.data = result;
            return dataTable;
        }

        public string GetAllProducts(int DistributorID, string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            var result = new List<ResponseProductModel>();
            if (DistributorID > 0)
            {
                result = GetProductbyDistributorId(DistributorID);
            }
            else { result = GetAllProduct(); }
            sSearch = sSearch.ToLower();
            int totalRecord = result.Count();
            if (!string.IsNullOrEmpty(sSearch))
                result = result.Where(a => (a.ProductName != null && a.ProductName.ToLower().Contains(sSearch.ToLower())) || (a.PartNumber != null && a.PartNumber.ToLower().Contains(sSearch.ToLower())) || (a.GroupName != null && a.GroupName.ToLower().Contains(sSearch.ToLower()))).ToList();
            else
                result = result.OrderBy(a => a.ProductId).Skip(iDisplayStart).Take(iDisplayLength).ToList();
            //result.ForEach(a => a.ImagePath = "<img src="+ a.ImagePath + " width='50' height='50'>") ;
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("{");
            sb.Append("\"sEcho\": ");
            sb.Append(sEcho);
            sb.Append(",");
            sb.Append("\"iTotalRecords\": ");
            sb.Append(totalRecord);
            sb.Append(",");
            sb.Append("\"iTotalDisplayRecords\": ");
            sb.Append(totalRecord);
            sb.Append(",");
            sb.Append("\"aaData\": ");
            sb.Append(JsonConvert.SerializeObject(result));
            sb.Append("}");
            return sb.ToString();
            //Get All data
        }
        #endregion

        #region Get Product Name By Id
        public clsProduct GetProductNameById(int ProductId)
        {
            var result = db.Products.Where(x => x.ProductId == ProductId).Select(data => new clsProduct()
            {
                ProductId = data.ProductId,
                GroupId = data.GroupId == null ? 0 : data.GroupId.Value,
                ProductName = data.ProductName,
                PartNumber = data.PartNo,
                Description = data.Description,
                Remark = data.Remarks,
                ImagePath = data.ImagePath,
                Price = data.Price,
                VariantId = data.ProductGroup.VariantId,
                VehicleId = data.ProductGroup.Variant.VehicleId,
                BrandId = data.BrandId,
                ProductType = data.ProductType,
                DistributorId = data.DistributorId != null ? data.DistributorId : 0,
                TaxValue = data.TaxValue,
                PackQuantity = data.PackQuantity!=null?data.PackQuantity:0,
                RootPartNo= data.RootPartNum,
                CurrentStock=data.CurrentStock??0
            }).FirstOrDefault();

            if (result != null)
            {
                result.ImagePath = Img.CheckImageUrl(result.ImagePath);
            }
            return result;

        }
        #endregion

        #region Get Product Name By Id
        public ProductWithBreadcrumb GetProductNameByIdMobile(int ProductId)
        {
            var result = db.Products.Where(x => x.ProductId == ProductId).Select(data => new clsProduct()
            {
                ProductId = data.ProductId,
                GroupId = data.GroupId == null ? 0 : data.GroupId.Value,
                ProductName = data.ProductName,
                PartNumber = data.PartNo,
                Description = data.Description,
                Remark = data.Remarks,
                ImagePath = data.ImagePath,
                Price = data.Price,
                VariantId = data.ProductGroup.VariantId,
                VehicleId = data.ProductGroup.Variant.VehicleId,
                BrandId = data.ProductGroup.Variant.Vehicle.BrandId,
            }).FirstOrDefault();

            if (result != null)
            {
                result.ImagePath = Img.CheckImageUrl(result.ImagePath);
            }
            var product = db.Products.Where(x => x.ProductId == ProductId).FirstOrDefault();
            List<string> lstbreadCrumbModels = new List<string>();
            lstbreadCrumbModels.Add(product.ProductGroup.Variant.Vehicle.Brand.Name);
            lstbreadCrumbModels.Add(product.ProductGroup.Variant.Vehicle.Name);
            lstbreadCrumbModels.Add(product.ProductGroup.Variant.Name);
            lstbreadCrumbModels.Add(product.ProductGroup.GroupName);
            lstbreadCrumbModels.Add(product.ProductName);
            return new ProductWithBreadcrumb()
            {
                Product = result,
                lstBreadCrumb = lstbreadCrumbModels
            };
        }
        #endregion

        #region Save Product
        public bool SaveProduct(clsProduct model)
        {
            var old = db.Products.Where(m => m.ProductId == model.ProductId).FirstOrDefault();
            string partCategoryCode = null;
            switch (model.ProductType)
            {
                case "Parts":
                    partCategoryCode= Constants.MFull;
                    break;

                case "Accessories":
                    partCategoryCode = Constants.AAFull;
                    break;

                case "Oil":
                    partCategoryCode = Constants.AGFull;
                    break;
            }
            if (old != null)
            {
                old.GroupId = model.GroupId;
                old.ProductName = model.ProductName;
                old.PartNo = model.PartNumber;
                old.Description = model.Description;
                old.Remarks = model.Remark;
                old.ImagePath = model.ImagePath;
                old.Price = model.Price;
                old.ProductType = model.ProductType;
                old.BrandId = model.BrandId;
                old.DistributorId = model.DistributorId;
                old.TaxValue = model.TaxValue;
                old.PackQuantity = model.PackQuantity;
                old.RootPartNum = model.RootPartNo;
                old.CurrentStock = model.CurrentStock;
                old.PartCategoryCode = partCategoryCode;
                db.SaveChanges();
                return true;
            }
            else
            {
                var product = new Product()
                {
                    GroupId = model.GroupId,
                    ProductName = model.ProductName,
                    PartNo = model.PartNumber,
                    Description = model.Description,
                    Remarks = model.Remark,
                    ImagePath = model.ImagePath,
                    CreatedDate = DateTime.Now,
                    Price = model.Price,
                    ProductType = model.ProductType,
                    BrandId = model.BrandId,
                    DistributorId = model.DistributorId,
                    TaxValue = model.TaxValue,
                    PackQuantity = model.PackQuantity,
                    RootPartNum = model.RootPartNo,
                    CurrentStock=model.CurrentStock,
                    PartCategoryCode= partCategoryCode
                };
                db.Products.Add(product);
                return db.SaveChanges() > 0;
            }
        }
        #endregion]

        #region Delete Product
        public bool DeleteProduct(int Id)
        {
            var data = db.Products.Where(m => m.ProductId == Id).FirstOrDefault();
            if (data == null)
            {
                return false;
            }
            else
            {
                db.Products.Remove(data);
                return db.SaveChanges() > 0;
            }
        }
        #endregion

        #region Get Product Name for DropDownList
        public List<ProductGroup> ddlProductGroupName()
        {
            var data = db.ProductGroups.ToList();
            data.Insert(0, new ProductGroup() { GroupName = "-- Please Select --" });
            return data;
        }
        #endregion

        #region Get Products by GroupID
        public List<ResponseProductModel> GetProductsByGroupID(int GroupID)
        {
            var result = (from p in db.Products
                          where p.GroupId == GroupID
                          select new ResponseProductModel()
                          {
                              ProductId = p.ProductId,
                              ProductName = p.ProductName,
                              GroupName = p.ProductGroup == null ? "" : p.ProductGroup.GroupName,
                              PartNumber = p.PartNo,
                              Description = p.Description,
                              Remark = p.Remarks,
                              ImagePath = p.ImagePath,
                              Price = !p.Price.HasValue ? "0" : String.Format("{0:#,###}", p.Price)
                          }).ToList();
            result.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));
            return result;
        }
        #endregion

        #region FastMoving Products
        public List<ResponseProductModel> GetFastMovingProducts(FastMovingProductRequest model, out int totalRecord)
        {
            int DistributorId = 0;
            if (model.Role != Constants.SuperAdmin)
            {
                DistributorId = model.UserId.GetDistributorId(model.Role);
            }
            IQueryable<TempOrderDetail> tempOrderDetails = Enumerable.Empty<TempOrderDetail>().AsQueryable();
            if (model.TempOrderId.HasValue)
            {
                tempOrderDetails = db.TempOrderDetails.Where(a => a.TempOrderId == model.TempOrderId.Value);
            }

            totalRecord = 0;
            var result = (from p in db.Products
                          where p.GroupId == model.GroupId && p.DistributorId == (DistributorId == 0 ? p.DistributorId : DistributorId)
                          select p).ToList()
            .Select(p=>new ResponseProductModel()
                          {
                              ProductId = p.ProductId,
                              ProductName = string.IsNullOrEmpty(p.ProductName) ? p.Description : p.ProductName,
                              GroupName = p.ProductGroup == null ? "" : p.ProductGroup.GroupName,
                              PartNumber = p.PartNo,
                              Description = p.Description,
                              Remark = p.Remarks,
                              ImagePath = p.ImagePath,
                              Price =!p.Price.HasValue ? "0" : String.Format("{0:#,###}", p.Price),
                              BrandId = p.BrandId
                          }).ToList();
            result.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));
            totalRecord = result.Count();
            if (model.PageNumber > 0)
            {
                var d = result.GetPaging<ResponseProductModel>(model.PageNumber, model.PageSize);
                foreach(var productModel in d)
                {
                    var isOriparts = db.Brands.Where(b => b.BrandId == (productModel.BrandId != null ? productModel.BrandId.Value : 0)).FirstOrDefault()?.IsOriparts; //p.Brand?.IsOriparts;
                    productModel.IsOriparts = isOriparts != null ? isOriparts.Value : false;

                    var cartItem = tempOrderDetails.Where(a => a.ProductId == productModel.ProductId).FirstOrDefault();
                    if (cartItem != null)
                    {
                        productModel.CartQty = cartItem.Qty;
                        productModel.CartAvailabilityType = cartItem.AvailabilityType;
                        productModel.CartOutletId = cartItem.OutletId;
                    }
                }
                return d;
            }
            return result;
        }
        #endregion

        #region Product Type DropDownList

        /// <summary>
        /// The select list of product types.
        /// </summary>
        /// <returns></returns>
        public SelectList ProductTypes()
        {
            // Create select list            
            var prodTypes = new List<SelectListItem>
            {
                new SelectListItem() { Value = "", Text = "-- Please Select --" },
                new SelectListItem() { Value = "Accessories", Text = "Accessories" },
                new SelectListItem() { Value = "Parts", Text = "Parts" },
                new SelectListItem() { Value = "Oil", Text = "Oil" }
            };

            return new SelectList(prodTypes, "Value", "Text", null);
        }

        public SelectList ProductTypesList()
        {
            // Create select list        
            var data = db.ProductTypes.ToList();
            var prodTypes = new  List<SelectListItem>();
            prodTypes.Add(new SelectListItem() { Value = "", Text = "-- Please Select --" });
            foreach (var item in data)
            {
                prodTypes.Add(new SelectListItem() { Value = item.TypeName, Text = item.TypeName });
            }
            return new SelectList(prodTypes, "Value", "Text", null);
        }
        #endregion

        #region Get Product By DistributorID
        public List<ResponseProductModel> GetProductbyDistributorId(int Distributorid)
        {
            var result = (from p in db.Products
                          where p.DistributorId == Distributorid
                          select new ResponseProductModel()
                          {
                              ProductId = p.ProductId,
                              //ProductName = p.ProductName,
                              //GroupName = p.ProductGroup == null ? "" : p.ProductGroup.GroupName,
                              ProductName = string.IsNullOrEmpty(p.ProductName) ? p.Description : p.ProductName,
                              GroupName = p.ProductGroup == null ? "" : p.ProductGroup.GroupName,
                              PartNumber = p.PartNo,
                              Description = p.Description,
                              Remark = p.Remarks,
                              ImagePath = p.ImagePath,
                              Price = p.Price.HasValue ? p.Price.ToString() : "0"
                          }).ToList();
            result.ForEach(a => a.ImagePath = Img.CheckImageUrl(a.ImagePath));
            return result;
        }
        #endregion
    }
}