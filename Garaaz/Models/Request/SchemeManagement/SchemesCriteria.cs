using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class SchemesCriteria
    {
        [Required(ErrorMessage ="Please Fill SchemeId")]
        public int SchemeId { get; set; }
        [Required(ErrorMessage = "Please Enter Target Criteria")]
        public string TargetCriteria { get; set; }      
        public string TargetWorkshopCriteria { get; set; }        
    }
    public class CriteriaOnCashback
    {
        [Required(ErrorMessage = "Please Fill SchemeId")]
        public int SchemeId { get; set; }
        [Required(ErrorMessage = "Please Enter Target Criteria")]
        public string CashbackCriteria { get; set; }
    }
}