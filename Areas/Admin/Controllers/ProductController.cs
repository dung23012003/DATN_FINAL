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

        public ProductController(ShopDongYContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var totalProducts = await _context.Products.CountAsync();

            var products = await _context.Products
                .Include(p => p.Categorys)
                .Include(p => p.Brands)
                .Include(p => p.Warehouse) // thêm dòng này
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
            ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "BrandName");
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            // Hàm load lại dropdown
            void LoadSelectLists()
            {
                ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "BrandName");
                ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            }

            // Kiểm tra tên trùng
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName == product.ProductName);

            if (existingProduct != null)
            {
                ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");
            }

            // Nếu dữ liệu không hợp lệ thì load lại View
            if (!ModelState.IsValid)
            {
                LoadSelectLists();
                return View(product);
            }

            // Xử lý ảnh
            var file = HttpContext.Request.Form.Files["ProductImage"];
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "product_image");
                var fullPath = Path.Combine(uploadPath, fileName);

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Kiểm tra trùng file ảnh
                if (System.IO.File.Exists(fullPath))
                {
                    ModelState.AddModelError("ProductImage", "Hình ảnh đã tồn tại trên hệ thống.");
                    LoadSelectLists();
                    return View(product);
                }

                // Lưu file ảnh
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                product.ProductImage = "image/product_image/" + fileName;
            }

            // Ngày tạo
            product.ProductDay = DateTime.Now;

            // Sinh mã Code không trùng
            int newCode;
            do
            {
                newCode = new Random().Next(100000, 999999);
            } while (_context.Products.Any(p => p.Code == newCode));
            product.Code = newCode;

            // Thêm vào DB với try-catch
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sản phẩm đã được thêm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu sản phẩm: " + ex.Message);
                LoadSelectLists();
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductModel product)
        {
            if (id != product.ProductId)
                return NotFound();

            void LoadSelectLists()
            {
                ViewBag.BrandId = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
                ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            }

            // Kiểm tra tên trùng trừ chính nó
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName == product.ProductName && p.ProductId != product.ProductId);

            if (existingProduct != null)
                ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");

            if (!ModelState.IsValid)
            {
                LoadSelectLists();
                return View(product);
            }

            try
            {
                var productInDb = await _context.Products.FindAsync(id);
                if (productInDb == null) return NotFound();

                productInDb.ProductName = product.ProductName;
                productInDb.Description = product.Description;
                productInDb.Price = product.Price;
                productInDb.BrandId = product.BrandId;
                productInDb.CategoryId = product.CategoryId;

                // Xử lý ảnh mới nếu có
                var file = HttpContext.Request.Form.Files["ProductImage"];
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "product_image");
                    var fullPath = Path.Combine(uploadPath, fileName);

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    if (System.IO.File.Exists(fullPath))
                    {
                        ModelState.AddModelError("ProductImage", "Hình ảnh đã tồn tại.");
                        LoadSelectLists();
                        return View(product);
                    }

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    productInDb.ProductImage = "image/product_image/" + fileName;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                LoadSelectLists();
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

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            try
            {
                // 👉 Xóa file ảnh nếu tồn tại
                if (!string.IsNullOrEmpty(product.ProductImage))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ProductImage);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // 👉 Xóa khỏi CSDL
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Xóa sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }




    }
}
