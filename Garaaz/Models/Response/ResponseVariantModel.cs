using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseVariantModel
    {
        public int VariantId { get; set; }
        public int VehicleId { get; set; }
        public int ParentId { get; set; }
        public string VariantName { get; set; }
        public string ProductionYear { get; set; }
        public string Description { get; set; }
        public string Region { get; set; }
        public string Engine { get; set; }
        public string ChassisType { get; set; }
        public string VNo { get; set; }
    }
}