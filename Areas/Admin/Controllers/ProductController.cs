using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ShopDongYContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(ShopDongYContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult TopSelling()
        {
            var topSelling = _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => od.Product.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToList();

            ViewBag.TopSellingList = topSelling;

            return View();
        }
        private void LoadUnitOptions()
        {
            ViewBag.UnitOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "Viên", Value = "Viên" },
                new SelectListItem { Text = "ml", Value = "ml" },
            };
        }

        private void LoadDropdowns(object? selectedBrand = null, object? selectedCategory = null)
        {
            ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "BrandName", selectedBrand);
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", selectedCategory);
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "image/product");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            if (System.IO.File.Exists(filePath))
                throw new Exception("Ảnh đã tồn tại.");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "image/product/" + fileName;
        }

        private void DeleteImageFile(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fullPath = Path.Combine(_hostEnvironment.WebRootPath, imagePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (System.IO.File.Exists(fullPath))
            {
                try { System.IO.File.Delete(fullPath); }
                catch (Exception ex) { Console.WriteLine($"❌ Không thể xóa ảnh: {imagePath}. Lỗi: {ex.Message}"); }
            }
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var totalProducts = await _context.Products.CountAsync();
            var products = await _context.Products.Include(p => p.Categorys).Include(p => p.Brands).Include(p => p.Warehouse)
                .OrderByDescending(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadUnitOptions();
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            LoadUnitOptions();
            LoadDropdowns();

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductName == product.ProductName);
            if (existingProduct != null)
                ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");

            if (!ModelState.IsValid)
                return View(product);

            try
            {
                product.ProductImage = await SaveImage(HttpContext.Request.Form.Files["ProductImage"]);
                product.ProductImage1 = await SaveImage(HttpContext.Request.Form.Files["ProductImage1"]);
                product.ProductImage2 = await SaveImage(HttpContext.Request.Form.Files["ProductImage2"]);
                product.ProductImage3 = await SaveImage(HttpContext.Request.Form.Files["ProductImage3"]);

                product.ProductDay = DateTime.Now;

                int newCode;
                do { newCode = new Random().Next(100000, 999999); }
                while (_context.Products.Any(p => p.Code == newCode));
                product.Code = newCode;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sản phẩm đã được thêm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu sản phẩm: " + ex.Message);
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            LoadUnitOptions();
            LoadDropdowns(product.BrandId, product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductModel product)
        {
            if (id != product.ProductId) return NotFound();

            LoadUnitOptions();
            LoadDropdowns(product.BrandId, product.CategoryId);

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName == product.ProductName && p.ProductId != product.ProductId);
            if (existingProduct != null)
                ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");

            if (!ModelState.IsValid)
                return View(product);

            try
            {
                var productInDb = await _context.Products.FindAsync(id);
                if (productInDb == null) return NotFound();

                productInDb.ProductName = product.ProductName;
                productInDb.Description = product.Description;
                productInDb.Price = product.Price;
                productInDb.BrandId = product.BrandId;
                productInDb.CategoryId = product.CategoryId;
                productInDb.QuantityPerUnit = product.QuantityPerUnit;
                productInDb.Unit = product.Unit;

                var image = await SaveImage(HttpContext.Request.Form.Files["ProductImage"]);
                if (!string.IsNullOrEmpty(image)) productInDb.ProductImage = image;

                var image1 = await SaveImage(HttpContext.Request.Form.Files["ProductImage1"]);
                if (!string.IsNullOrEmpty(image1)) productInDb.ProductImage1 = image1;

                var image2 = await SaveImage(HttpContext.Request.Form.Files["ProductImage2"]);
                if (!string.IsNullOrEmpty(image2)) productInDb.ProductImage2 = image2;

                var image3 = await SaveImage(HttpContext.Request.Form.Files["ProductImage3"]);
                if (!string.IsNullOrEmpty(image3)) productInDb.ProductImage3 = image3;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brands)
                .Include(p => p.Categorys)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            try
            {
                var imagePaths = new[] {
                    product.ProductImage,
                    product.ProductImage1,
                    product.ProductImage2,
                    product.ProductImage3
                };

                foreach (var imagePath in imagePaths.Where(p => !string.IsNullOrEmpty(p)))
                    DeleteImageFile(imagePath);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "✅ Sản phẩm và các hình ảnh liên quan đã được xóa.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ Lỗi khi xóa sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
