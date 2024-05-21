namespace Garaaz.Models
{
    public class CanTakeMoreThanOneModel
    {
        public int SchemeId { get; set; }
        public bool CanTakeMoreThanOne { get; set; }
        public int? MaxGiftsAllowed { get; set; }
    }
}