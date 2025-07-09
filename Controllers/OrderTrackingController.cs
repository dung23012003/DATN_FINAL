using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Controllers
{
    public class OrderTrackingController : Controller
    {
        private readonly ShopDongYContext _context;

        public OrderTrackingController(ShopDongYContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem đơn hàng.";
                return RedirectToAction("Login", "Account");
            }

            var orders = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem chi tiết đơn hàng.";
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult Cancel(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId.Value);

            if (order == null || order.Status != OrderModel.OrderStatus.Pending)
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
                return RedirectToAction("Index");
            }

            foreach (var detail in order.OrderDetails)
            {
                var warehouse = _context.Warehouses.FirstOrDefault(w => w.ProductId == detail.ProductId);
                if (warehouse != null)
                {
                    warehouse.TotalSold -= detail.Quantity;
                    warehouse.TotalSold = Math.Max(0, warehouse.TotalSold);
                    warehouse.LastUpdated = DateTime.Now;
                }
            }

            order.Status = OrderModel.OrderStatus.Cancelled;
            _context.SaveChanges();

            TempData["Success"] = $"Đơn hàng #{order.OrderId} đã được hủy.";
            return RedirectToAction("Index");
        }

    }
}
