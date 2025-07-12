using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ShopDongYContext _context;

        public UserController(ShopDongYContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var totalUsers = await _context.Users.CountAsync();

            var users = await _context.Users
                .Include(p => p.Role)                
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            // Hàm load lại dropdown
            void LoadSelectLists()
            {
                ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName");
            }

            // Kiểm tra tên trùng
            var existingProduct = await _context.Users
                .FirstOrDefaultAsync(p => p.UserName == user.UserName);

            if (existingProduct != null)
            {
                ModelState.AddModelError("UserName", "Tên người dùng đã tồn tại.");
            }

            // Nếu dữ liệu không hợp lệ thì load lại View
            if (!ModelState.IsValid)
            {
                LoadSelectLists();
                return View(user);
            }
            if (user.RoleId == null || user.RoleId == 0)
            {
                ModelState.AddModelError("RoleId", "Vui lòng chọn vai trò.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RoleId = new SelectList(_context.Roles.ToList(), "RoleId", "RoleName");
                return View(user);
            }

            // Xử lý ảnh
            var file = HttpContext.Request.Form.Files["Avatar"];
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "user_image");
                var fullPath = Path.Combine(uploadPath, fileName);

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Kiểm tra trùng file ảnh
                if (System.IO.File.Exists(fullPath))
                {
                    ModelState.AddModelError("Avatar", "Hình ảnh đã tồn tại trên hệ thống.");
                    LoadSelectLists();
                    return View(user);
                }

                // Lưu file ảnh
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                user.Avatar = "image/user_image/" + fileName;
            }

            // Ngày tạo
            user.CreatedAt = DateTime.Now;

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Người dùng đã được thêm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu sản phẩm: " + ex.Message);
                LoadSelectLists();
                return View(user);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            TempData["Success"] = "Tạo người dùng thành công!";
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserModel user)
        {
            if (id != user.UserId)
                return NotFound();

            void LoadSelectLists()
            {
                ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
                
            }

            // Kiểm tra tên trùng trừ chính nó
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(p => p.UserName == user.UserName && p.UserId != user.UserId);

            if (existingUser != null)
                ModelState.AddModelError("UserName", "Tên người dùng đã tồn tại.");

            if (!ModelState.IsValid)
            {
                LoadSelectLists();
                return View(user);
            }

            try
            {
                var userInDb = await _context.Users.FindAsync(id);
                if (userInDb == null) return NotFound();

                userInDb.UserName = user.UserName;
                userInDb.Email = user.Email;
                userInDb.PhoneNumur = user.PhoneNumur;
                userInDb.RoleId = user.RoleId;
                userInDb.Password = user.Password;
                userInDb.Address = user.Address;


                // Xử lý ảnh mới nếu có
                var file = HttpContext.Request.Form.Files["Avatar"];
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "user_image");
                    var fullPath = Path.Combine(uploadPath, fileName);

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    if (System.IO.File.Exists(fullPath))
                    {
                        ModelState.AddModelError("Avatar", "Hình ảnh đã tồn tại.");
                        LoadSelectLists();
                        return View(user);
                    }

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    userInDb.Avatar = "image/user_image/" + fileName;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                LoadSelectLists();
                return View(user);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .Include(p => p.Role)                
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            try
            {
                // 👉 Xóa file ảnh nếu tồn tại
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Avatar);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // 👉 Xóa khỏi CSDL
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Xóa người dùng thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        // GET: Admin/User/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}
