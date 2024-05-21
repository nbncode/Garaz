using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class BackOrderModel
    {
        public int CustomerOrderId { get; set; }
        public int? DistributorId { get; set; }
        public string CONo { get; set; }
        public DateTime? CODate { get; set; }
        public string PartyType { get; set; }
        public string PartyCode { get; set; }
        public string PartyName { get; set; }
        public string PartStatus { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string LocCode { get; set; }
        public string Order { get; set; }
    }
    public class BackOrderExelModel
    {
        public int SrNo { get; set; }
        public string Distributor { get; set; }
        public string CONo { get; set; }
        public string OrderDate { get; set; }
        public string PartyCode { get; set; }
        public string PartyName { get; set; }
        public string PartStatus { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string LocCode { get; set; }
        public string Order { get; set; }
    }
}