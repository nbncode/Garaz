using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class DashboardResponse
    {
        public string UPID { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
        public string SaleHeading { get; set; }
        public string SaleSubHeading { get; set; }
        public string SaleValue { get; set; }
        public string TotalOutstanding { get; set; }
        public string CriticalOutstanding { get; set; }
        public string ClosingDate { get; set; }
        public string CreditLimit { get; set; }
        public List<Banner> Banners { get; set; }
    }
    public class Banner
    {
        public string Heading { get; set; }
        public string ImagePath { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public int SchemeId { get; set; }
    }

    public class DashboardProductTypeDetail
    {
        public decimal MGP { get; set; }
        public decimal MGA { get; set; }
        public decimal MGO { get; set; }
        public decimal TotalSales { get; set; }
        public decimal Total { get; set; }
    }
}