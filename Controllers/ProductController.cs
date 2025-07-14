using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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


            ViewBag.UnitOptions = new List<SelectListItem>
             {
                    new SelectListItem { Text = "Viên", Value = "Viên" },
                    new SelectListItem { Text = "ml", Value = "ml" },
                   
                };


            // Truy vấn danh sách sản phẩm
            var productsQuery = _context.Products
                .Include(p => p.Categorys)
                .Include(p => p.Brands)
                .Include(p => p.Warehouse)
                  .Include(p => p.Discounts)
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
        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Categorys)
                .Include(p => p.Brands)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Gợi ý sản phẩm cùng danh mục
            var relatedProducts = _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .OrderByDescending(p => p.ProductId)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        // Hàm phụ để hiển thị "cách đây bao lâu"
        private string GetTimeAgo(DateTime? dateTime)
        {
            if (dateTime == null) return "";

            var timeSpan = DateTime.Now - dateTime.Value;

            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            else
                return dateTime.Value.ToString("dd/MM/yyyy");
        }




    }
}
