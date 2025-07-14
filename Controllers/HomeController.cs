using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShopDongYContext _context;

        public HomeController(ILogger<HomeController> logger, ShopDongYContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 8;

            // Tất cả sản phẩm có phân trang
            var allProducts = _context.Products
                .Include(p => p.Categorys)
                .Include(p => p.Brands)
                .Include(p => p.Warehouse)
                .Include(p => p.Discounts)
                .OrderByDescending(p => p.ProductId);

            int totalProducts = allProducts.Count();
            var products = allProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // ✅ Sản phẩm đang giảm giá
            var now = DateTime.Now;
            var productsOnSale = allProducts
                .Where(p => p.Discounts.Any(d => d.StartDate <= now && d.EndDate >= now))
                .ToList();
            ViewBag.ProductsOnSale = productsOnSale;

            // ✅ Top 3 sản phẩm bán chạy nhất (dựa trên OrderDetails)
            var topProducts = _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => od.Product)
                .Select(g => new
                {
                    Product = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(3)
                .Select(x => x.Product)
                .ToList();
            ViewBag.TopSellingProducts = topProducts;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            return View(products);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categorys)
                .Include(p => p.Discounts)
                .Include(p => p.Warehouse)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            var relatedProducts = _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
    }
}
