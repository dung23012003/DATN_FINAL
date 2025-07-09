using System.ComponentModel.DataAnnotations;

namespace ShopDongY.Models
{
    public class OrderModel
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public virtual UserModel User { get; set; }
        public string OrderEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string? Note { get; set; }
        public virtual ICollection<OrderDetailsModel> OrderDetails { get; set; } = new List<OrderDetailsModel>();
        public virtual PaymentModel? Payment { get; set; }

        public enum OrderStatus
        {
            Pending,      // 0: Chờ xử lý
            Completed,    // 1: Đã thanh toán
            Cancelled     // 2: Đã hủy
        }
    }
}
