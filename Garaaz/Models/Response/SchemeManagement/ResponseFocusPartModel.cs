using System.Collections.Generic;

namespace Garaaz.Models
{
    public class ResponseFocusPartModel
    {
        public int FocusPartId { get; set; }
        public int SchemeId { get; set; }
        public int? GroupId { get; set; }
        public int? ProductId { get; set; }
        public int? Qty { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
    }
    public class FocusGroupModel
    {
        public int SchemeId { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public string ProductText { get; set; }
        public string ProductIds { get; set; }
    }
    public class FocusPartsModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
    }
}