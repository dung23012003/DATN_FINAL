using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopDongYContext _context;

        public AccountController(ShopDongYContext context)
        {
            _context = context;
        }

        // Trang đăng ký
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(UserModel user)
        {
            if (!ModelState.IsValid)
                return View(user);

            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                return View(user);
            }

            user.CreatedAt = DateTime.Now;
            user.RoleId = _context.Roles.FirstOrDefault(r => r.RoleName == "Customer")?.RoleId ?? 2;

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // Trang đăng nhập
        [HttpGet]
        public IActionResult Login() => View();
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserName == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("Role", user.Role.RoleName);

            TempData["Success"] = $"Xin chào {user.UserName}";

            // Phân quyền
            if (user.Role.RoleName == "Admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" }); // Chuyển sang trang Admin
            else
                return RedirectToAction("Index", "Home"); // Người dùng bình thường về trang chính
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
           

          
            return RedirectToAction("Login", "Account", new { area = "" });
        }
    }
}
