namespace Garaaz.Models
{
    public class GiftManagementModel
    {
        public int GiftId { get; set; }
        public int SchemeId { get; set; }
        public string Gift { get; set; }
        public int? Qty { get; set; }
        public string DrawOrder { get; set; }
        /// <summary>
        /// Gets or sets the image path for gift.
        /// </summary>
        public string ImagePath { get; set; }
        public string Categories { get; set; }
    }
}