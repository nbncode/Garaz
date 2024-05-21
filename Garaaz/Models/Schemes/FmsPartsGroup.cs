using System;

namespace Garaaz.Models.Schemes
{
    public class FmsPartsGroup
    {
        public int SchemeId { get; set; }
        public int DistributorId { get; set; }

        public string PartCategory { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string FValue { get; set; }
        public string MValue { get; set; }
        public string SValue { get; set; }
        public string PartCreation { get; set; }
    }
}