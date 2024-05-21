using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseSchemeOnEdit
    {
        public int SchemeId { get; set; }
        public string SchemeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DispersalFrequency { get; set; }
        public string SchemesType { get; set; }
        public bool IsAreBothCbAgApplicable { get; set; }
        public bool IsAreBothAgLdApplicable { get; set; }
        public bool IsAssuredGift { get; set; }
        public bool IsCashBack { get; set; }
        public bool IsCanTakeMoreThanOneGift { get; set; }
        public bool IsFocusPartApplicable { get; set; }
        public string Types { get; set; }
        public string UserId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public AdditionalCouponModel AdditionalCoupon { get; set; }
        public List<CategorySchemeModel> ListCategoryScheme { get; set; }
        public List<GiftManagementModel> ListGiftManagement { get; set; }
        public List<QualifyCriteriaModel> ListQualifyCriteria { get; set; }
        public List<AssuredGiftModel> ListAssuredGift { get; set; }
        public List<CashBackModel> ListCashBack { get; set; }
        public List<TicketOfJoyModel> ListTicketOfJoy { get; set; }
    }
}