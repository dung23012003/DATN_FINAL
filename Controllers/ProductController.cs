using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;

namespace ShopDongY.Controllers
{
    public class ProductController : Controller
    {
        private readonly ShopDongYContext _context;
        private string? searchString;

        public ProductController(ShopDongYContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? searchString, string? category)
        {
            var products = _context.Products
                .Include(p => p.Categorys)
                .Include(p => p.Brands)
                .AsQueryable();

            // Danh sách danh mục có sản phẩm
            var categories = _context.Categories
                .Where(c => c.Products.Any())
                .ToList();
            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = category;

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Categorys.CategoryName == category);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(searchString) ||
                    (p.Categorys != null && p.Categorys.CategoryName.ToLower().Contains(searchString)) ||
                    (p.Brands != null && p.Brands.BrandName.ToLower().Contains(searchString))
                );
            }

            return View(products.ToList());
        }

    }
}
