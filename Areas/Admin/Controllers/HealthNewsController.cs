using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HealthNewsController : Controller
    {
        private readonly ShopDongYContext _context;

        public HealthNewsController(ShopDongYContext context)
        {
            _context = context;
        }

        // GET: Danh sách bài viết
        public IActionResult Index()
        {
            var newsList = _context.HealthNews
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(newsList);
        }

        // GET: Chi tiết bài viết
        public IActionResult Details(int id)
        {
            var news = _context.HealthNews.FirstOrDefault(n => n.Id == id);
            if (news == null) return NotFound();
            return View(news);
        }

        // GET: Tạo bài viết
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tạo bài viết
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HealthNewsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Xử lý ảnh nếu có
            if (model.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "news");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImageFile.CopyTo(stream);
                }

                model.ImageUrl = Path.Combine("image/news", fileName).Replace("\\", "/");
            }

            model.CreatedAt = DateTime.Now;
            _context.HealthNews.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Sửa bài viết
        public IActionResult Edit(int id)
        {
            var news = _context.HealthNews.Find(id);
            if (news == null) return NotFound();
            return View(news);
        }

        // POST: Sửa bài viết
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(HealthNewsModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = _context.HealthNews.AsNoTracking().FirstOrDefault(n => n.Id == model.Id);
            if (existing == null) return NotFound();

            if (model.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "news");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImageFile.CopyTo(stream);
                }

                model.ImageUrl = Path.Combine("image/news", fileName).Replace("\\", "/");
            }
            else
            {
                model.ImageUrl = existing.ImageUrl;
            }

            model.CreatedAt = existing.CreatedAt;
            _context.HealthNews.Update(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Xóa bài viết
        public IActionResult Delete(int id)
        {
            var news = _context.HealthNews.Find(id);
            if (news == null) return NotFound();

            _context.HealthNews.Remove(news);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
