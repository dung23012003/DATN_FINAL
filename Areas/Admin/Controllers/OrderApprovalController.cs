using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;
using System;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderApprovalController : Controller
    {
        private readonly ShopDongYContext _context;

        public OrderApprovalController(ShopDongYContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var totalOrders = await _context.Orders
                .Where(o =>
                    o.Status == OrderModel.OrderStatus.Pending ||
                    o.Status == OrderModel.OrderStatus.Completed ||
                    o.Status == OrderModel.OrderStatus.Cancelled)
                .CountAsync();

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Where(o =>
                    o.Status == OrderModel.OrderStatus.Pending ||
                    o.Status == OrderModel.OrderStatus.Completed ||
                    o.Status == OrderModel.OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            return View(orders);
        }


        public IActionResult Details(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            return View(order);
        }


        [HttpPost]
        public IActionResult Approve(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            // Nếu là thanh toán Online, chỉ duyệt khi đã được chuyển khoản
            if (order.Payment != null && !order.Payment.IsApproved)
            {
                TempData["Error"] = "Chưa xác nhận thanh toán Online!";
                return RedirectToAction("Index");
            }

            order.Status = OrderModel.OrderStatus.Completed;
            _context.SaveChanges();

            TempData["Success"] = $"Đã duyệt đơn hàng #{order.OrderId}.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Cancel(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            if (order.Status == OrderModel.OrderStatus.Cancelled)
            {
                TempData["Info"] = "Đơn hàng này đã được hủy trước đó.";
                return RedirectToAction("Index");
            }

            // ✅ Trừ lại TotalSold trong kho
            foreach (var detail in order.OrderDetails)
            {
                var warehouse = _context.Warehouses.FirstOrDefault(w => w.ProductId == detail.ProductId);
                if (warehouse != null)
                {
                    warehouse.TotalSold -= detail.Quantity;
                    if (warehouse.TotalSold < 0)
                        warehouse.TotalSold = 0; // Tránh âm số
                    warehouse.LastUpdated = DateTime.Now;
                }
            }

            // ✅ Hủy đơn hàng
            order.Status = OrderModel.OrderStatus.Cancelled;
            _context.SaveChanges();

            TempData["Success"] = $"Đã hủy đơn hàng #{order.OrderId} và cập nhật lại kho hàng.";
            return RedirectToAction("Index");
        }

    }
}
