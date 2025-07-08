using System.ComponentModel.DataAnnotations;

namespace ShopDongY.Models
{
    public class CategoryModel
    {
        [Key]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
    }
}
