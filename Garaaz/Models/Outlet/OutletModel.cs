using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class OutletModel
    {
        public int DistributorId { get; set; }     
        public int OutletId { get; set; }        
        [Required]
        public string OutletName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        [Required]
        public string OutletCode { get; set; }


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

        public static SelectList GetWorkshop(int distributorId)
        {
            var db = new garaazEntities();
            // Create select list
            var selListItem = new SelectListItem() { Value = "", Text = "-- Select Workshop --" };
            var newList = new List<SelectListItem> { selListItem };
            RepoUsers repoUsers = new RepoUsers();
            var distWorkshop = repoUsers.GetWorkshopByDistId(distributorId);
            foreach (var dl in distWorkshop)
            {
                newList.Add(new SelectListItem() { Value = dl.WorkShopId.ToString(), Text = dl.WorkShopName });
            }
            return new SelectList(newList, "Value", "Text", null);
        }
    }
}