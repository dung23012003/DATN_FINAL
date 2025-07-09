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

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Account/Register
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

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập, điều hướng phù hợp
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                var role = HttpContext.Session.GetString("Role");
                if (role == "Admin")
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string useremail, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == useremail && u.Password == password);


            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
                return View();
            }

            // Lưu thông tin người dùng vào session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("Role", user.Role.RoleName);
            HttpContext.Session.SetString("Avatar", user.Avatar ?? "/images/default-avatar.png");


            TempData["Success"] = $"Xin chào {user.UserName}!";

            if (user.Role.RoleName == "Admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Đăng xuất thành công.";
            return RedirectToAction("Login");
        }
    }
}
