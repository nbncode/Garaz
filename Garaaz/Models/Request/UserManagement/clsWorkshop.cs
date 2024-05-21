using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class clsWorkshop
    {
        public int DistributorId { get; set; }
        public string UserId { get; set; }
        public int WorkshopId { get; set; }
        [Required(ErrorMessage = "You must provide a phone number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string WorkShopName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        public bool IsApproved { get; set; }
        public string Role { get; set; }
        public bool IsFromDistributorUser { get; set; }
        public string Pincode { get; set; }
        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }
        public string Gender { get; set; }
        public string LandlineNumber { get; set; }
        public string Gstin { get; set; }

        [Required(ErrorMessage = "Employee code is required.")]
        public string EmployeeCode { get; set; }
        public string Designations { get; set; }
        [Required(ErrorMessage = "Critical Outstanding Days is required.")]
        public int CriticalOutstandingDays { get; set; }
        [Required(ErrorMessage = "OutletId required.")]
        public int OutletId { get; set; }
        public string CategoryName { get; set; }
        public decimal? CreditLimit { get; set; }
        [Required(ErrorMessage = "BillingName is required.")]
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
        public decimal? MonthlyPartPurchase { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Field should be decimal number")]
        public decimal? MonthlyConsumablesPurchase { get; set; }
      
        public string WorkingHours { get; set; }
        public string WeeklyOffDay { get; set; }
        public string Website { get; set; }
        public string InsuranceCompanies { get; set; }
        public bool IsMorethanOneBranch { get; set; }


        public static SelectList GetDistributorOutlets(int distributorId)
        {
            var db = new garaazEntities();
            // Create select list
            var selListItem = new SelectListItem() { Value = "", Text = "-- Select Outlet --" };
            var newList = new List<SelectListItem>{};
            var ro = new RepoOutlet();
            var distOutlets= ro.GetDistributorOutlets(distributorId);
            foreach (var dl in distOutlets)
            {
                newList.Add(new SelectListItem() { Value = dl.OutletId.ToString(), Text = dl.OutletName });
            }
            return new SelectList(newList, "Value", "Text", null);
        }
       
    }
    public class workshop
    {
        public List<SelectListItem> GetWorkshopCategory()
        {
            try
            {
                List<SelectListItem> categories = new List<SelectListItem>() {
                        new SelectListItem(){Text = "--Select--",Value=""},
                        new SelectListItem() { Text = "Premium garage", Value = "PremiumGarage" },
                        new SelectListItem() { Text = "Classic garage", Value = "ClassicGarage" },
                        new SelectListItem() { Text = "Economy garage", Value = "EconomyGarage" },
                        new SelectListItem() { Text = "Authorized service", Value = "AuthorizedService" },
                        new SelectListItem() { Text = "Station/Dealer/Detailing studio", Value = "Station/Dealer/DetailingStudio" },
                };

                return categories;
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }
        public List<SelectListItem> GetInsuranceCompanies()
        {
            try
            {
                List<SelectListItem> ICompanies = new List<SelectListItem>() {
                        new SelectListItem() { Text = "HDFC Ergo", Value = "HDFCErgo" },
                        new SelectListItem() { Text = "ICICI Lomabrd", Value = "ICICILomabrd" },
                        new SelectListItem() { Text = "TATA AIG", Value = "TATAAIG" },
                        new SelectListItem() { Text = "GoDigit", Value = "GoDigit" },
                };

                return ICompanies;
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }
       
    }
}