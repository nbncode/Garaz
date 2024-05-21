using System;

namespace Garaaz.Models
{
    public class LevelInfo
    {
        public bool LevelAchieved { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public DateTime  Date { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}