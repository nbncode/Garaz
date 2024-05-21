using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Garaaz.Models.Import
{
    public class ImportWorkshopModel
    {
        [Required(ErrorMessage = "Please select distributor")]
        public int DistributorId { get; set; }
        public SelectList Distributors { get; set; }
        public List<ImportStatus> ImportStatus { get; set; }
    }
}