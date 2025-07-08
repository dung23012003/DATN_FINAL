using System.ComponentModel.DataAnnotations;

namespace ShopDongY.Models
{
    public class OrderDetailsModel
    {
        [Key]
        public int OrderDetailId { get; set; }       // Mã chi tiết đơn hàng
        public int OrderId { get; set; }             // Liên kết đến đơn hàng
        public int ProductId { get; set; }           // Sản phẩm nào

        public int Quantity { get; set; }            // Số lượng mua
        public decimal UnitPrice { get; set; }       // Giá tại thời điểm mua
        public decimal TotalPrice => Quantity * UnitPrice; // Tổng tiền (có thể là property chỉ đọc)

        public virtual OrderModel Order { get; set; }         // Liên kết ngược đến đơn hàng
        public virtual ProductModel Product { get; set; }
    }
}
