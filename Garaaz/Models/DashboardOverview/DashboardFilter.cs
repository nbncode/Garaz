using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models.DashboardOverview
{
    public class DashboardFilter
    {
        [Required(ErrorMessage = "User Id required")]
        public string UserId { get; set; }
        public IEnumerable<string> Roles { get; set; }
        [Required(ErrorMessage = "Date type required")]
        public string DateType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Used as additional filter for web side
        public string Category { get; set; }
        public string BranchCode { get; set; }
        public string CustomerType { get; set; }

        // For dataTable search, pagination and sorting
        public string SearchTxt { get; set; }
        public int PageSize { get; set; }
        public int Skip { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }
}