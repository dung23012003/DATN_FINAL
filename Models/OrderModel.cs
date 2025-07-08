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
        public bool Status { get; set; } // e.g., Pending, Completed, Cancelled
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Note { get; set; }
        public virtual ICollection<OrderDetailsModel> OrderDetails { get; set; } = new List<OrderDetailsModel>();
        

    }
}
