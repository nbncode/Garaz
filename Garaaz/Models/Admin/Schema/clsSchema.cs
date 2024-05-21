using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class ClsSchema
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DispersalType { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Gets scheme types (Cashback & Lucky draw).
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> SchemeTypes => new List<SelectListItem>{
                new SelectListItem {Text = "--Select--",Value = ""},
                new SelectListItem { Text = "Cashback", Value = "Cashback" },
                new SelectListItem { Text = "Assured gifts/Lucky draw", Value = "Lucky Draw" }
            };

        /// <summary>
        /// Gets sub-scheme types of 'Cashback' scheme type.
        /// </summary>
        public static List<SelectListItem> SubSchemeTypes => new List<SelectListItem>{
                new SelectListItem {Text = "--Select--",Value = ""},
                new SelectListItem { Text = "Slabwise discount scheme", Value = "SlabwiseDiscountScheme" },
                new SelectListItem { Text = "Target based scheme", Value = "TargetBasedScheme" }
        };

        public List<SelectListItem> GetTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem {Text = "--Select--",Value = ""},
                new SelectListItem { Text = "Lucky Draw", Value = "Lucky Draw" },
                new SelectListItem { Text = "Assured Benefits", Value = "Fixed Price" }
            };
        }

        /// <summary>
        /// Gets list of payback periods.
        /// </summary>
        public static List<SelectListItem> PaybackPeriods => new List<SelectListItem>{
                new SelectListItem {Text = "--Select--",Value = ""},
                new SelectListItem { Text = "One Time", Value = "One Time" },
                new SelectListItem { Text = "Monthly", Value = "Monthly" },
                new SelectListItem { Text = "Quarterly", Value = "Quarterly" },
                new SelectListItem { Text = "Half Yearly", Value = "Half Yearly" },
                new SelectListItem { Text = "Yearly", Value = "Yearly" }
            };

        /// <summary>
        /// Gets list of part category.
        /// </summary>
        public static List<SelectListItem> PartCategories => new List<SelectListItem>
            {
                 new SelectListItem { Text = "Maruti Suzuki Genuine Parts", Value = "MGP" },
                new SelectListItem { Text = "Maruti Suzuki Genuine Accessories", Value = "MGA" },
                new SelectListItem { Text = "Maruti Suzuki Genuine Oil", Value = "MGO" }

            };

        /// <summary>
        /// Gets list of part groups.
        /// </summary>
        public static List<SelectListItem> PartGroups => new List<SelectListItem>
            {
                new SelectListItem {Text = "--Select--",Value = ""},
                new SelectListItem { Text = "All Parts", Value = "All Parts" },
                new SelectListItem { Text = "Focus Parts Group", Value = "Focus Parts Group" },
                new SelectListItem { Text = "FMS Parts Group", Value = "FMS Parts Group" }
            };

        /// <summary>
        /// Gets list of part creations.
        /// </summary>
        public static List<SelectListItem> PartCreations => new List<SelectListItem>
        {
            new SelectListItem {Text = "--Select--",Value = ""},
            new SelectListItem{Text="All", Value="All"},
            new SelectListItem{Text="F", Value="F"},
            new SelectListItem{Text="M", Value="M"},
            new SelectListItem{Text="S", Value="S"}
        };

        /// <summary>
        /// Gets list of product types.
        /// </summary>
        public static List<SelectListItem> ProductTypes => new List<SelectListItem>
        {
            new SelectListItem{Text="Parts", Value="Parts"},
            new SelectListItem{Text="Accessories", Value="Accessories"}
        };

        /// <summary>
        /// Gets list of benefit types.
        /// </summary>
        public static List<SelectListItem> BenefitTypes => new List<SelectListItem>
        {
            new SelectListItem {Text = "Coupon", Value = "Coupon"},
            new SelectListItem {Text = "Amount", Value = "Amount"},
            new SelectListItem {Text = "Point", Value = "Point"},
            new SelectListItem {Text = "Cashback", Value = "Percentage"},
            new SelectListItem {Text = "Product", Value = "Product"},
            new SelectListItem {Text = "Gift", Value = "Gift"}
        };

        public List<SelectListItem> GetTargetCriterias()
        {
            return new List<SelectListItem>
            {
                new SelectListItem{Text="--Select--", Value=""},
                new SelectListItem{Text="Daily",Value="Daily"},
                new SelectListItem{Text="Weekly",Value="Weekly"},
                new SelectListItem{Text="Monthly",Value="Monthly"},
                new SelectListItem{Text="One Time",Value="One Time"}
            };
        }

        /// <summary>
        /// Gets list of cashback criterias.
        /// </summary>
        public static List<SelectListItem> CashbackCriterias => new List<SelectListItem>
        {
            new SelectListItem{Text="--Select--", Value=""},
            new SelectListItem{Text="Monthly",Value="Monthly"},
            new SelectListItem{Text="One Time",Value="One Time"}
        };

        public List<SelectListItem> GetSchemeFor()
        {
            return new List<SelectListItem>
            {
                new SelectListItem {Text = "--Select--",Value = ""},
                new SelectListItem { Text = "Sales Executive", Value = "SalesExecutive" },
                new SelectListItem { Text = "Ro Incharge", Value = "RoIncharge" }
            };
        }

        public List<SelectListItem> GetTargetCriteria()
        {
            return new List<SelectListItem>
            {
                        new SelectListItem {Text = "--Select--",Value = ""},
                        new SelectListItem { Text = "Column1", Value = "Min" },
                        new SelectListItem { Text = "Column2", Value = "Max" }
                        //new SelectListItem() { Text = "Avg", Value = "Avg"
            };
        }

        public List<SelectListItem> GetStateList()
        {
            return new List<SelectListItem>
                {
                    new SelectListItem {Text = "--Select--",Value = ""},
                    new SelectListItem { Text = "Rajasthan", Value = "Rajasthan" },
                    new SelectListItem { Text = "Andhra Pradesh", Value = "Andhra Pradesh" },
                    new SelectListItem { Text = "Arunachal Pradesh", Value = "Arunachal Pradesh" },
                    new SelectListItem { Text = "Assam", Value = "Assam" },
                    new SelectListItem { Text = "Bihar", Value = "Bihar" },
                    new SelectListItem { Text = "Chhattisgarh", Value = "Chhattisgarh" },
                    new SelectListItem { Text = "Goa", Value = "Goa" },
                    new SelectListItem { Text = "Gujarat", Value = "Gujarat" },
                    new SelectListItem { Text = "Haryana", Value = "Haryana" },
                    new SelectListItem { Text = "Himachal Pradesh", Value = "Himachal Pradesh" },
                    new SelectListItem { Text = "Jammu and Kashmir", Value = "Jammu and Kashmir" },
                    new SelectListItem { Text = "Jharkhand", Value = "Jharkhand" },
                    new SelectListItem { Text = "Karnataka", Value = "Karnataka" },
                    new SelectListItem { Text = "Kerala", Value = "Kerala" },
                    new SelectListItem { Text = "Madhya Pradesh", Value = "Madhya Pradesh" },
                    new SelectListItem { Text = "Maharashtra", Value = "Maharashtra" },
                    new SelectListItem { Text = "Manipur", Value = "Manipur" },
                    new SelectListItem { Text = "Meghalaya", Value = "Meghalaya" },
                    new SelectListItem { Text = "Mizoram", Value = "Mizoram" },
                    new SelectListItem { Text = "Nagaland", Value = "Nagaland" },
                    new SelectListItem { Text = "Odisha", Value = "Odisha" },
                    new SelectListItem { Text = "Punjab", Value = "Punjab" },
                    new SelectListItem { Text = "Sikkim", Value = "Sikkim" },
                    new SelectListItem { Text = "Tamil Nadu", Value = "Tamil Nadu" },
                    new SelectListItem { Text = "Telangana", Value = "Telangana" },
                    new SelectListItem { Text = "Tripura", Value = "Tripura" },
                    new SelectListItem { Text = "Uttar Pradesh", Value = "Uttar Pradesh" },
                    new SelectListItem { Text = "Uttarakhand", Value = "Uttarakhand" },
                    new SelectListItem { Text = "West Bengal", Value = "West Bengal" }
                };
        }
    }
}