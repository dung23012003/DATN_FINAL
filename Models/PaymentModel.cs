using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShopDongY.Models
{
    public class PaymentModel
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual OrderModel Order { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền không hợp lệ")]
        public decimal TotalAmount { get; set; }

        public bool IsApproved { get; set; }

        [StringLength(255)]
        public string? TransferNote { get; set; }
    }
}
