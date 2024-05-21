using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class clsOutlet
    {
        public int DistributorId { get; set; }
        public string UserId { get; set; }
        [Required]
        public int OutletId { get; set; }
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
        public string OutletName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        public bool IsApproved { get; set; }

        [Required(ErrorMessage = "Employee code is required.")]
        public string EmployeeCode { get; set; }

        [Required(ErrorMessage = "OutletCode code is required.")]
        public string OutletCode { get; set; }

        public string Designations { get; set; }

        public static SelectList GetLocations(int distributorId)
        {
            var db = new garaazEntities();

            // Create select list
            var selListItem = new SelectListItem() { Value = "", Text = "-- Select Location --" };
            var newList = new List<SelectListItem> { selListItem };
            var distLocations = db.DistributorLocations.Where(d => d.DistributorId == distributorId);
            foreach (var dl in distLocations)
            {
                newList.Add(new SelectListItem() { Value = dl.LocationId.ToString(), Text = $"{dl.Location} ({dl.LocationCode})" });
            }
            return new SelectList(newList, "Value", "Text", null);
        }
    }

}