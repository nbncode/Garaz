using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Garaaz.Models
{
    public class ResponseUserProfile
    {
        public string code { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        [EmailAddress]
        public string Emailaddress { get; set; }
        [Required]
        public string Address { get; set; }
        public string UserImage { get; set; }


        public string Role { get; set; }
        public int WorkshopId { get; set; }
        public string WorkShopName { get; set; }
        public string Gstin { get; set; }
        public string BillingName { get; set; }
        public int? YearOfEstablishment { get; set; }
        public string WorkshopType { get; set; }
        public string Make { get; set; }


        public string JobsUndertaken { get; set; }
        public string Premise { get; set; }
        public string GaraazArea { get; set; }
        public int? TwoPostLifts { get; set; }
        public bool WashingBay { get; set; }
        public bool PaintBooth { get; set; }
        public bool ScanningAndToolKit { get; set; }
        public int? TotalOwners { get; set; }
        public int? TotalChiefMechanics { get; set; }
        public int? TotalEmployees { get; set; }
        public int? MonthlyVehiclesServiced { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Field should be decimal number")]
        public decimal MonthlyPartPurchase { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Field should be decimal number")]
        public decimal MonthlyConsumablesPurchase { get; set; }
        public string WorkingHours { get; set; }
        public string WeeklyOffDay { get; set; }
        public string Website { get; set; }
        public string InsuranceCompanies { get; set; }
        public int CompleteProfile { get; set; }
    }
    public class ResponseUserImageModel
    {
        public string UserId { get; set; }
        public string UserImage64 { get; set; }
    }
    public class ResponseImage
    {
        public string Image { get; set; }
    }
    public class UserProfiles
    {
        #region Variables
        garaazEntities db = new garaazEntities();

        #endregion

        #region Get User Profile
        public ResponseUserProfile GetUserProfile(string UserId, string Role)
        {
            int completefield = 0;
            int totalfield = 0;
            var userdetails = new ResponseUserProfile();
            if (!string.IsNullOrEmpty(UserId))
            {
                dynamic workshop = null;
                var data = (from a in db.AspNetUsers
                            join u in db.UserDetails on a.Id equals u.UserId
                            where a.Id == UserId
                            select new
                            {
                                Email = a.Email,
                                Phone = a.UserName,
                                ConsPartyCode = u.ConsPartyCode,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                Address = u.Address,
                                UserImage = u.UserImage
                            }).FirstOrDefault();

                if (Role != null && Role == Constants.Workshop || Role == Constants.WorkshopUsers)
                {
                    workshop = (from d in db.DistributorWorkShops
                                join u in db.WorkShops on d.WorkShopId equals u.WorkShopId
                                where d.UserId == UserId
                                select u).FirstOrDefault();
                }
                if (data != null)
                {
                    userdetails.code = data.ConsPartyCode;
                    userdetails.FirstName = data.FirstName;
                    userdetails.LastName = data.LastName;
                    userdetails.PhoneNumber = data.Phone;
                    userdetails.Emailaddress = data.Email;
                    userdetails.Address = data.Address;
                    userdetails.Role = Role;
                    userdetails.UserImage = data.UserImage;
                    if (workshop != null)
                    {
                        userdetails.WorkshopId = workshop.WorkShopId != null ? workshop.WorkShopId : 0;
                        userdetails.WorkShopName = workshop.WorkShopName != null ? workshop.WorkShopName : "";
                        userdetails.Gstin = workshop != null ? workshop.Gstin : "";
                        userdetails.BillingName = workshop.BillingName != null ? workshop.BillingName : "";
                        userdetails.YearOfEstablishment = workshop.YearOfEstablishment != null ? Convert.ToInt32(workshop.YearOfEstablishment) : 0;
                        userdetails.WorkshopType = workshop.Type != null ? workshop.Type : "";
                        userdetails.Make = workshop.Make != null ? workshop.Make : "";
                        userdetails.JobsUndertaken = workshop.JobsUndertaken != null ? workshop.JobsUndertaken : "";
                        userdetails.Premise = workshop.Premise != null ? workshop.Premise : "";
                        userdetails.GaraazArea = workshop.GaraazArea != null ? workshop.GaraazArea : "";
                        userdetails.TwoPostLifts = workshop.TwoPostLifts != null ? Convert.ToInt32(workshop.TwoPostLifts) : 0;
                        userdetails.WashingBay = workshop.WashingBay != null ? Convert.ToBoolean(workshop.WashingBay) : false;
                        userdetails.PaintBooth = workshop.PaintBooth != null ? Convert.ToBoolean(workshop.PaintBooth) : false;
                        userdetails.ScanningAndToolKit = workshop.ScanningAndToolKit != null ? Convert.ToBoolean(workshop.ScanningAndToolKit) : false;
                        userdetails.TotalOwners = workshop.TotalOwners != null ? Convert.ToInt32(workshop.TotalOwners) : 0;
                        userdetails.TotalChiefMechanics = workshop.TotalChiefMechanics != null ? Convert.ToInt32(workshop.TotalChiefMechanics) : 0;
                        userdetails.TotalEmployees = workshop.TotalEmployees != null ? Convert.ToInt32(workshop.TotalEmployees) : 0;
                        userdetails.MonthlyVehiclesServiced = workshop.MonthlyVehiclesServiced != null ? Convert.ToInt32(workshop.MonthlyVehiclesServiced) : 0;
                        userdetails.MonthlyPartPurchase = workshop.MonthlyPartPurchase != null ? Convert.ToDecimal(workshop.MonthlyPartPurchase) : 0;
                        userdetails.MonthlyConsumablesPurchase = workshop.MonthlyConsumablesPurchase != null ? Convert.ToDecimal(workshop.MonthlyConsumablesPurchase) : 0;
                        userdetails.WorkingHours = workshop.WorkingHours != null ? workshop.WorkingHours : "";
                        userdetails.WeeklyOffDay = workshop.WeeklyOffDay != null ? workshop.WeeklyOffDay : "";
                        userdetails.Website = workshop.Website != null ? workshop.Website : "";
                        userdetails.InsuranceCompanies = workshop.InsuranceCompanies != null ? workshop.InsuranceCompanies : "";
                    }
                }
                // calculate profile complete
                totalfield = totalfield + 1;
                if (!string.IsNullOrEmpty(userdetails.code)) { completefield = completefield + 1; }
                totalfield = totalfield + 1;
                if (!string.IsNullOrEmpty(userdetails.FirstName)) { completefield = completefield + 1; }
                totalfield = totalfield + 1;
                if (!string.IsNullOrEmpty(userdetails.LastName)) { completefield = completefield + 1; }
                totalfield = totalfield + 1;
                if (!string.IsNullOrEmpty(userdetails.PhoneNumber)) { completefield = completefield + 1; }
                totalfield = totalfield + 1;
                if (!string.IsNullOrEmpty(userdetails.Emailaddress)) { completefield = completefield + 1; }
                totalfield = totalfield + 1;
                if (!string.IsNullOrEmpty(userdetails.Address)) { completefield = completefield + 1; }
                totalfield = totalfield + 1;
                if (!string.IsNullOrEmpty(userdetails.UserImage)) { completefield = completefield + 1; }
                if (workshop != null)
                {
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.WorkShopName)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.Gstin)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.BillingName)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.YearOfEstablishment > 0) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.Type)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.Make)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.JobsUndertaken)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.Premise)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.GaraazArea)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.TwoPostLifts != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.WashingBay != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.PaintBooth != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.ScanningAndToolKit != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.TotalOwners != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.TotalChiefMechanics != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.TotalEmployees != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.MonthlyVehiclesServiced != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.MonthlyPartPurchase != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (workshop.MonthlyConsumablesPurchase != null) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.WorkingHours)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.WeeklyOffDay)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.Website)) { completefield = completefield + 1; }
                    totalfield = totalfield + 1;
                    if (!string.IsNullOrEmpty(workshop.InsuranceCompanies)) { completefield = completefield + 1; }
                }
                userdetails.CompleteProfile = completefield * 100 / totalfield;
            }
            return userdetails;
        }
        #endregion

        #region Update User Profile
        public bool UpdateUserProfile(ResponseUserProfile model, string UserId)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(UserId) && model != null)
            {
                var detail = db.UserDetails.Where(u => u.UserId == UserId).FirstOrDefault();
                var user = db.AspNetUsers.Where(a => a.Id == UserId).FirstOrDefault();
                if (detail != null && user != null)
                {
                    detail.FirstName = model.FirstName;
                    detail.LastName = model.LastName;
                    user.Email = model.Emailaddress;
                    detail.Address = model.Address;
                    detail.UserImage = model.UserImage;
                    db.SaveChanges();
                    result = true;
                }
                if (model.WorkshopId > 0)
                {
                    var workshop = db.WorkShops.Where(w => w.WorkShopId == model.WorkshopId).FirstOrDefault();
                    if (workshop != null)
                    {
                        workshop.WorkShopName = model.WorkShopName;
                        workshop.Gstin = model.Gstin;
                        workshop.BillingName = model.BillingName;
                        workshop.YearOfEstablishment = model.YearOfEstablishment;
                        workshop.Type = model.WorkshopType;
                        workshop.Make = model.Make;
                        workshop.JobsUndertaken = model.JobsUndertaken;
                        workshop.Premise = model.Premise;
                        workshop.GaraazArea = model.GaraazArea;

                        workshop.TwoPostLifts = model.TwoPostLifts;
                        workshop.WashingBay = model.WashingBay;
                        workshop.PaintBooth = model.PaintBooth;
                        workshop.ScanningAndToolKit = model.ScanningAndToolKit;
                        workshop.TotalOwners = model.TotalOwners;

                        workshop.TotalChiefMechanics = model.TotalChiefMechanics;
                        workshop.TotalEmployees = model.TotalEmployees;
                        workshop.MonthlyVehiclesServiced = model.MonthlyVehiclesServiced;
                        workshop.MonthlyPartPurchase = model.MonthlyPartPurchase;
                        workshop.MonthlyConsumablesPurchase = model.MonthlyConsumablesPurchase;
                        workshop.WorkingHours = model.WorkingHours;
                        workshop.WeeklyOffDay = model.WeeklyOffDay;
                        workshop.Website = model.Website;
                        workshop.InsuranceCompanies = model.InsuranceCompanies;


                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Save User Image and Update
        public bool SaveUserImage(ResponseUserImageModel model, out string Imagepath)
        {
            bool result = false;

            // use common function for save image
            Imagepath = Utils.SaveImageUsingBase64(model.UserImage64);
            result = UpdateUserImage(model, Imagepath);

            return result;
        }

        public bool UpdateUserImage(ResponseUserImageModel model, string Imagepath)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(model.UserId) && model != null)
            {
                var detail = db.UserDetails.Where(u => u.UserId == model.UserId).FirstOrDefault();
                if (detail != null && Imagepath != null)
                {
                    detail.UserImage = Imagepath;
                    db.SaveChanges();
                    result = true;
                }
            }
            return result;
        }

        #endregion

    }


}