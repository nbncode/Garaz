using System.Linq;

namespace Products.Garaaz.Models
{
    public class RepoProducts
    {
        /// <summary>
        /// Get product id of the product with product number.
        /// </summary>
        /// <param name="productNumber">The product number of the product.</param>
        /// <param name="price">The price for passed product.</param>
        /// <returns>Return product Id if exists else zero.</returns>
        public int GetProductId(string productNumber, out decimal price)
        {
            price = 0;
            Product product;
            using (var db = new ProductGaraazEntities())
            {
                product = db.Products.AsNoTracking().FirstOrDefault(p => p.PartNo == productNumber);
            }

            if (product == null) return 0;

            price = product.Price ?? 0;
            return product.ProductId;
        }
    }
}