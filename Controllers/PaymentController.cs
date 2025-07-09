using Microsoft.AspNetCore.Mvc;
using ShopDongY.Data;
using ShopDongY.Models;
using System;

namespace ShopDongY.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ShopDongYContext _context;

        public PaymentController(ShopDongYContext context)
        {
            _context = context;
        }

        public IActionResult Index(int orderId, decimal amount, string productName, string userName)
        {
            // Format nội dung chuyển khoản theo yêu cầu
            var transferNote = $"Thanh_Toan_Online_{productName}_{userName}";
            var encodedNote = Uri.EscapeDataString(transferNote);

            // Tạo QR từ VietQR
            var bankSlug = "techcombank";
            var accountNo = "7123012003";
            var qrUrl = $"https://img.vietqr.io/image/{bankSlug}-{accountNo}-compact.png"
                        + $"?amount={amount}&addInfo={encodedNote}";

            // Lưu Payment nếu chưa tồn tại
            var existing = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);
            if (existing == null)
            {
                var payment = new PaymentModel
                {
                    OrderId = orderId,
                    TotalAmount = amount,
                    IsApproved = false,
                    TransferNote = transferNote,
                    PaymentDate = DateTime.Now
                };
                _context.Payments.Add(payment);
                _context.SaveChanges();
            }

            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.TransferNote = transferNote;
            ViewBag.QRUrl = qrUrl;
            return View();
        }
    }
}
