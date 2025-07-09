using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopDongY.Models
{
    public class ProductModel
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string? ProductImage { get; set; }

        public DateTime ProductDay { get; set; }

        public int Code { get; set; }

        public int? CategoryId { get; set; }

        public virtual CategoryModel? Categorys { get; set; }

        public int? BrandId { get; set; }

        public virtual BrandModel? Brands { get; set; }

        public virtual WarehouseModel? Warehouse { get; set; }

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - ProductDay;

                if (timeSpan.TotalMinutes < 1)
                    return "Vừa xong";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} phút trước";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} giờ trước";
                return $"{(int)timeSpan.TotalDays} ngày trước";
            }
        }

        // ✅ Đây là phần bạn cần để hiển thị tên danh mục
        [NotMapped]
        public string? CategoryName => Categorys?.CategoryName;
    }
}
