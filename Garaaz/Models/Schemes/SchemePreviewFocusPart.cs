using System.Collections.Generic;

namespace Garaaz.Models.Schemes
{
    public class SchemePreviewFocusPart
    {
        /// <summary>
        /// Get or set fields
        /// </summary>
        public string PartGroup { get; set; }
        public List<SchemePreviewPart> Parts { get; set; }
        public SchemePreviewFocusPart()
        {
            // Default initialized
            Parts = new List<SchemePreviewPart>();
        }
    }
}