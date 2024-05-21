using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class VariantPagePermission
    {
        garaazEntities db = new garaazEntities();

        #region Get All Variant
        public List<ResponseVariantModel> GetAllVariant()
        {
            return (from v in db.Variants
                    select new ResponseVariantModel()
                    {
                        VariantId = v.VariantId,
                        VariantName = v.Name,
                        ProductionYear = v.ProductionYear,
                        Description = v.Description,
                        ParentId = v.ParentId == null ? 0 : v.ParentId.Value,
                        VehicleId = v.VehicleId == null ? 0 : v.VehicleId.Value,
                        Region = v.Region,
                        Engine = v.Engine,
                        ChassisType = v.ChassisType,
                        VNo = v.VNo

                    }).ToList();
        }
        #endregion

        #region Get Variant by VehicleID
        public List<clsVariant> GetVariantByVehicleID(int VehicleID)
        {
            return (from v in db.Variants
                              where v.VehicleId == VehicleID
                              select new clsVariant()
                              {
                                  VariantId = v.VariantId,
                                  VariantName = v.Name,
                                  ProductionYear = v.ProductionYear,
                                  Description = v.Description,
                                  ParentId = v.ParentId == null ? 0 : v.ParentId.Value,
                                  Region = v.Region,
                                  Engine = v.Engine,
                                  ChassisType = v.ChassisType,
                                  VNo = v.VNo
                              }).ToList();
        }
        #endregion

        #region Get Variant by VehicleID for Mobile
        public VariantWithBreadcrumb GetVariantByVehicleIDMobile(int VehicleID)
        {
            var lstVariant = (from v in db.Variants
                              where v.VehicleId == VehicleID
                              select new clsVariant()
                              {
                                  VariantId = v.VariantId,
                                  VariantName = v.Name,
                                  ProductionYear = v.ProductionYear,
                                  Description = v.Description,
                                  ParentId = v.ParentId == null ? 0 : v.ParentId.Value,
                                  Region = v.Region,
                                  Engine = v.Engine,
                                  ChassisType = v.ChassisType,
                                  VNo = v.VNo
                              }).ToList();
            var vehicle = db.Vehicles.Where(x => x.VehicleId == VehicleID).FirstOrDefault();
            List<string> lstbreadCrumbModels = new List<string>();
            lstbreadCrumbModels.Add(vehicle.Brand.Name);
            lstbreadCrumbModels.Add(vehicle.Name);
            return new VariantWithBreadcrumb()
            {
                lstBreadCrumb = lstbreadCrumbModels,
                lstVarient = lstVariant
            };

        }
        #endregion

        #region Get Variant Name By Id
        public clsVariant GetVariantNameById(int VariantId)
        {
            var data = db.Variants.Where(x => x.VariantId == VariantId).FirstOrDefault();
            return new clsVariant()
            {
                VariantId = data.VariantId,
                VariantName = data.Name,
                ProductionYear = data.ProductionYear,
                Description = data.Description,
                ParentId = data.ParentId == null ? 0 : data.ParentId.Value,
                VehicleId = data.VehicleId == null ? 0 : data.VehicleId.Value,
                BrandId = data.Vehicle.BrandId.Value,
                Region = data.Region,
                Engine = data.Engine,
                ChassisType = data.ChassisType,
                VNo = data.VNo
            };
        }
        #endregion

        #region Save Variant
        public bool SaveVariant(clsVariant model)
        {
            var old = db.Variants.Where(m => m.VariantId == model.VariantId).FirstOrDefault();
            if (old != null)
            {
                old.Name = model.VariantName;
                old.ProductionYear = model.ProductionYear;
                old.Description = model.Description;
                old.ParentId = model.ParentId;
                old.VehicleId = model.VehicleId;
                old.Region = model.Region;
                old.Engine = model.Engine;
                old.ChassisType = model.ChassisType;
                old.VNo = model.VNo;
                return db.SaveChanges() > 0;
            }
            else
            {
                var Variant = new Variant()
                {
                    Name = model.VariantName,
                    ProductionYear = model.ProductionYear,
                    Description = model.Description,
                    Region = model.Region,
                    Engine = model.Engine,
                    ChassisType = model.ChassisType,
                    VNo = model.VNo,
                    ParentId = model.ParentId,
                    VehicleId = model.VehicleId,
                    CreatedDate = DateTime.Now
                };
                db.Variants.Add(Variant);
                return db.SaveChanges() > 0;
            }
        }
        #endregion]

        #region Delete Variant
        public bool DeleteVariant(int Id)
        {
            var data = db.Variants.Where(m => m.VariantId == Id).FirstOrDefault();
            if (data == null)
            {
                return false;
            }
            else
            {
                db.Variants.Remove(data);
                return db.SaveChanges() > 0;
            }
        }
        #endregion       

        #region Get Vehicle Name for DropDownList
        public List<Vehicle> ddlVehicles()
        {
            var data = db.Vehicles.ToList();
            data.Insert(0, new Vehicle() { Name = "-- Please Select --" });
            return data;
        }
        #endregion

        #region Get Variant parent name for Dropdownlist
        public List<Variant> ddlVariants()
        {
            var data = db.Variants.ToList();
            data.Insert(0, new Variant() { Name = "-- Please Select --" });
            return data;
        }
        #endregion
    }
}