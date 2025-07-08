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
                .Take(10)
                .ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categorys)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            // L?y danh sách s?n ph?m cùng danh m?c, lo?i tr? chính nó
            var relatedProducts = _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .Take(4) // L?y t?i ?a 4 s?n ph?m
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }




    }
}
