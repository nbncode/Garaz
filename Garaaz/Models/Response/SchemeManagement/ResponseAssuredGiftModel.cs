using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseAssuredGiftModel
    {
        public int AssuredGiftId { get; set; }
        public int SchemeId { get; set; }
        public string Target { get; set; }
        public string Point { get; set; }
        public string Reward { get; set; }
        public string Categories { get; set; }
        public List<ResponseAssuredGiftCategoryModel> ListAssuredGiftCategory { get; set; }
    }

    public class ResponseWorkshopScheme
    {
        public int SchemeId { get; set; }
        public string SchemeName { get; set; }
        public bool AssuredGift { get; set; }
        public bool LuckyDraw { get; set; }
        public bool CashBack { get; set; }
    }

    public class WorkshopSchemeSelectType
    {
        [Required]
        public int SchemeId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string SelectedOption { get; set; }
    }
    public class GetWorkshopSchemeModel
    {
        [Required(ErrorMessage = "UserId required.")]
        public string UserId { get; set; }
        public int? SchemeId { get; set; }
    }
}