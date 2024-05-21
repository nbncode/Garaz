using System.Collections.Generic;

namespace Garaaz.Models
{
    public class AppLevelDataModel
    {
        public string TopMessage { get; set; }
        public List<LevelInfo> Levels { get; set; }
        /// <summary>
        /// Set value of TargetAchived is total sale of this scheme type. 
        /// </summary>
        public decimal? TargetAchieved { get; set; }
        public string TargetAchievedPercentage { get; set; }
    }
}