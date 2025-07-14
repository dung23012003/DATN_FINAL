using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopDongY.Models
{
    public class DiscountModel
    {
        [Key]
        public int DiscountId { get; set; }

        [Required]
        public string DiscountName { get; set; }

        public string? Description { get; set; }

        public decimal DiscountAmount { get; set; } // Số tiền giảm hoặc phần trăm

        public bool IsPercentage { get; set; } // true nếu giảm theo %

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Liên kết với Product
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual ProductModel? Product { get; set; }
    }
}
