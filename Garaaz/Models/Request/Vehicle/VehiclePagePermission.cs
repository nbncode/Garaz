using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class VehiclePagePermission
    {
        garaazEntities db = new garaazEntities();
        General general = new General();
        #region Get All Vehicle
        public List<ResponseVehicleModel> GetAllVehicle()
        {

            var lstVehicle = (from v in db.Vehicles
                              select new ResponseVehicleModel()
                              {
                                  VehicleId = v.VehicleId,
                                  VehicleName = v.Name,
                                  Name = v.Brand == null ? "" : v.Brand.Name,
                                  ImagePath = v.Image,
                                  BrandId = v.BrandId == null ? 0 : v.BrandId.Value,
                              }).ToList();
            lstVehicle.ForEach(a => { a.ImagePath = general.CheckImageUrl(a.ImagePath); });
            return lstVehicle;
        }
        #endregion

        #region Get Vehicle Name By Id
        public clsVehicle GetVehicleNameById(int vehicleId)
        {
            var data = db.Vehicles.Where(x => x.VehicleId == vehicleId).FirstOrDefault();
            return new clsVehicle()
            {
                VehicleId = data.VehicleId,
                VehicleName = data.Name,
                ImagePath = general.CheckImageUrl(data.Image),
                BrandId = data.BrandId == null ? 0 : data.BrandId.Value
            };
        }
        #endregion

        #region Get Vehicles By BrandID
        public List<clsVehicle> GetVehiclesByBrandID(int BrandID)
        {
            var lstVehicle = (from v in db.Vehicles
                              where v.BrandId == BrandID
                              orderby v.Name
                              select new clsVehicle()
                              {
                                  VehicleId = v.VehicleId,
                                  VehicleName = v.Name,
                                  ImagePath = v.Image
                              }).ToList();
            lstVehicle.ForEach(a => { a.ImagePath = general.CheckImageUrl(a.ImagePath); });
            return lstVehicle;
        }
        #endregion

        #region Save Vehicle
        public bool SaveVehicle(clsVehicle model)
        {
            var old = db.Vehicles.Where(m => m.VehicleId == model.VehicleId).FirstOrDefault();
            if (old != null)
            {
                old.Name = model.VehicleName;
                old.Image = model.ImagePath;
                old.BrandId = model.BrandId;
                db.SaveChanges();
                return true;
            }
            else
            {
                var vehicle = new Vehicle()
                {
                    Name = model.VehicleName,
                    Image = model.ImagePath,
                    CreatedDate = DateTime.Now,
                    BrandId = model.BrandId
                };
                db.Vehicles.Add(vehicle);
                return db.SaveChanges() > 0;
            }
        }
        #endregion]

        #region Delete Vehicle
        public bool DeleteVehicle(int Id)
        {
            var data = db.Vehicles.Where(m => m.VehicleId == Id).FirstOrDefault();
            if (data == null)
            {
                return false;
            }
            else
            {
                db.Vehicles.Remove(data);
                return db.SaveChanges() > 0;
            }
        }
        #endregion       

        #region Get Brand Name for DropDownList
        public List<clsBrand> ddlBrands()
        {
            var data = db.Brands.Select(a => new clsBrand()
            {
                BrandId = a.BrandId,
                Name = a.Name
            }).ToList();

            data.Insert(0, new clsBrand() { Name = "-- Please Select --" });
            return data;
        }
        #endregion
        #region Get Brand Name for DropDownList
        public List<clsBrand> DistributorBrands(int distributorId)
        {
            var data = (from b in db.Brands
                        join db in db.DistributorBrands on b.BrandId equals db.BrandId
                        where db.DistributorId == distributorId
                        select new clsBrand()
                        {
                            BrandId = b.BrandId,
                            Name = b.Name
                        }).ToList();
           // data.Insert(0, new clsBrand() { Name = "-- Please Select --" });
            return data;
        }
        #endregion
    }
}