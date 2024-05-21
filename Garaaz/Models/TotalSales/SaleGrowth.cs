namespace Garaaz.Models
{
    /// <summary>
    /// Represents a particular row of frequency, sale and growth.
    /// </summary>
    public class SaleGrowth
    {
        /// <summary>
        /// Gets or sets the frequency column value.
        /// </summary>
        public string FrequencyCol { get; set; }

        /// <summary>
        /// Gets or sets the sum of sales as per frequency.
        /// </summary>
        public string Sale { get; set; }

        /// <summary>
        /// Gets or sets the growth in percentage.
        /// </summary>
        public string Growth { get; set; }

        /// <summary>
        /// Gets or sets the sum of previous sales as per frequency.
        /// </summary>
        public string PrevSale { get; set; }
    }
}