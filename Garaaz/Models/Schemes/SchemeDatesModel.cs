using System;

namespace Garaaz.Models.Schemes
{
    public class SchemeDatesModel
    {
        public int SchemeId{get;set;}
        public DateTime PrevYearFromDate{get;set;}
        public DateTime PrevYearToDate{get;set;}
        public DateTime PrevMonthFromDate{get;set;}
        public DateTime PrevMonthToDate{get;set;}
    }
}