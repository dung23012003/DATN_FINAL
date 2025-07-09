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

        public IActionResult Index(string? searchString, string? category, string? brand, int page = 1)
        {
            int pageSize = 8;

            // Gọi danh sách danh mục và thương hiệu
            var categories = _context.Categories.ToList();
            var brands = _context.Brands.ToList();


            ViewBag.Categories = categories;
            ViewBag.Brands = brands;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentBrand = brand;
            ViewBag.CurrentSearch = searchString;

            // Truy vấn danh sách sản phẩm
            var productsQuery = _context.Products
                .Include(p => p.Categorys)
                .Include(p => p.Brands)
                .Include(p => p.Warehouse)
                .AsQueryable();

            // Lọc theo danh mục nếu có
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Categorys.CategoryName == category);
            }

            // Lọc theo thương hiệu nếu có
            if (!string.IsNullOrEmpty(brand))
            {
                productsQuery = productsQuery.Where(p => p.Brands.BrandName == brand);
            }

            // Tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.ToLower().Contains(searchString) ||
                    (p.Categorys != null && p.Categorys.CategoryName.ToLower().Contains(searchString)) ||
                    (p.Brands != null && p.Brands.BrandName.ToLower().Contains(searchString))
                );
            }

            // Tính tổng số trang
            int totalProducts = productsQuery.Count();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // Lấy danh sách sản phẩm theo trang
            var products = productsQuery
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }




    }
}
