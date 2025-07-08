using System.ComponentModel.DataAnnotations;

namespace ShopDongY.Models
{
    public class BrandModel
    {
        [Key]
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();

        // Additional properties can be added here as needed
    }
}
