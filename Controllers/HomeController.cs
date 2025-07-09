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

        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Warehouse)
                .OrderByDescending(p => p.ProductDay) // S?p x?p m?i nh?t
                .Take(10) // L?y 10 s?n ph?m m?i nh?t
                .ToList();

            return View(products);
        }


        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categorys)
                .Include(p => p.Warehouse) // ? Thêm dòng này
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
