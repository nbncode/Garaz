using System.Collections.Generic;

namespace Garaaz.Models
{
    public class ChartModel
    {
        /// <summary>
        /// Get or set the chart title which defines text to draw at the top of the chart.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Get or set the X-Axis label to tell the viewer what data they are viewing.
        /// </summary>
        public string XAxisLabel { get; set; }
        /// <summary>
        /// Get or set the sales grouped by frequency.
        /// </summary>
        public List<decimal> Sales { get; set; }
        /// <summary>
        /// Get or set the labels values such as days, weeks, months, quarters and years.
        /// </summary>
        public List<string> Labels { get; set; }

        public ChartModel()
        {
            Sales = new List<decimal>();
            Labels = new List<string>();
        }
    }
    
}