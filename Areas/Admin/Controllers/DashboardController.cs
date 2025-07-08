using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;

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
            // Tổng số lượng thống kê
            ViewBag.ProductCount = _context.Products.Count();
            ViewBag.UserCount = _context.Users.Count();
            ViewBag.OrderCount = _context.Orders.Count();
            ViewBag.BrandCount = _context.Brands.Count();
            ViewBag.TotalRevenue = _context.Orders.Sum(o => o.TotalAmount);

            // Thống kê doanh thu theo tháng trong năm hiện tại
            var currentYear = DateTime.Now.Year;
            var revenueByMonth = _context.Orders
                .Where(o => o.OrderDate.Year == currentYear)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            // Gán doanh thu vào 12 tháng
            var months = Enumerable.Range(1, 12).Select(m =>
            {
                var data = revenueByMonth.FirstOrDefault(x => x.Month == m);
                return data?.Total ?? 0;
            }).ToList();

            ViewBag.Months = string.Join(",", Enumerable.Range(1, 12).Select(m => $"\"Tháng {m}\""));
            ViewBag.Revenues = string.Join(",", months);

            // 🟦 Thống kê sản phẩm theo thương hiệu
            var productByBrand = _context.Products
                .GroupBy(p => p.Brands.BrandName)
                .Select(g => new
                {
                    Brand = g.Key,
                    Count = g.Count()
                })
                .ToList(); // ❗ cũng dùng ToList ở đây

            ViewBag.BrandLabels = string.Join(",", productByBrand.Select(b => $"\"{b.Brand}\""));
            ViewBag.BrandCounts = string.Join(",", productByBrand.Select(b => b.Count));

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
