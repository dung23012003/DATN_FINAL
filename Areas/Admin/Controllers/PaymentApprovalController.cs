using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;
using System;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PaymentApprovalController : Controller
    {
        private readonly ShopDongYContext _context;

        public PaymentApprovalController(ShopDongYContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var pendingPayments = _context.Payments
                .Include(p => p.Order)
                .Where(p => !p.IsApproved)
                .ToList();

            return View(pendingPayments);
        }

        // Duyệt thanh toán và cập nhật trạng thái đơn hàng
        [HttpPost]
        public IActionResult Approve(int paymentId)
        {
            var payment = _context.Payments.Include(p => p.Order)
                                           .FirstOrDefault(p => p.PaymentId == paymentId);

            if (payment == null || payment.Order == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin thanh toán hoặc đơn hàng.";
                return RedirectToAction("Index");
            }

            payment.IsApproved = true;
            payment.PaymentDate = DateTime.Now;

            payment.Order.Status = OrderModel.OrderStatus.Completed;

            _context.SaveChanges();

            TempData["Success"] = $"Đã duyệt thanh toán đơn hàng #{payment.Order.OrderId}";
            return RedirectToAction("Index");
        }
    }
}
