using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ShopDongYContext _context;

        public DashboardController(ShopDongYContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Thống kê tổng quan
            ViewBag.ProductCount = _context.Products.Count();
            ViewBag.UserCount = _context.Users.Count();
            ViewBag.OrderCount = _context.Orders.Count();
            ViewBag.BrandCount = _context.Brands.Count();

            // Tổng doanh thu của đơn đã hoàn thành
            ViewBag.TotalRevenue = _context.Orders
                .Where(o => o.Status == OrderModel.OrderStatus.Completed)
                .Sum(o => o.TotalAmount);

            // 🟧 Doanh thu theo tháng trong năm hiện tại
            var currentYear = DateTime.Now.Year;
            var revenueByMonth = _context.Orders
                .Where(o => o.OrderDate.Year == currentYear && o.Status == OrderModel.OrderStatus.Completed)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            var months = Enumerable.Range(1, 12).Select(m =>
            {
                var data = revenueByMonth.FirstOrDefault(x => x.Month == m);
                return data?.Total ?? 0;
            }).ToList();

            ViewBag.Months = string.Join(",", Enumerable.Range(1, 12).Select(m => $"\"Tháng {m}\""));
            ViewBag.Revenues = string.Join(",", months);

            // 🟦 Top 5 sản phẩm bán chạy nhất
            var topProducts = _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => od.Product.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            ViewBag.TopProductLabels = string.Join(",", topProducts.Select(p => $"\"{p.ProductName}\""));
            ViewBag.TopProductCounts = string.Join(",", topProducts.Select(p => p.TotalSold));

            return View();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
            }
            base.OnActionExecuting(context);
        }
    }
}
