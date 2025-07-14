using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;
using ShopDongY.ViewModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace ShopDongY.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopDongYContext _context;
        private int userId;

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
            // Kiểm tra model hợp lệ
            if (!ModelState.IsValid)
                return View(user);

            // Kiểm tra trùng tên đăng nhập
            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                ModelState.AddModelError(nameof(user.UserName), "Tên đăng nhập đã tồn tại.");
                return View(user);
            }

            // Kiểm tra trùng email
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError(nameof(user.Email), "Email đã được sử dụng.");
                return View(user);
            }

            // Kiểm tra mật khẩu nhập lại
            var confirmPassword = Request.Form["ConfirmPassword"];
            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Mật khẩu không khớp.");
                return View(user);
            }

            // Thiết lập các giá trị mặc định
            user.CreatedAt = DateTime.Now;
            user.RoleId = _context.Roles.FirstOrDefault(r => r.RoleName == "User")?.RoleId ?? 2;

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }


        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
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

            if (user == null || user.Role == null)
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
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

        // ✅ GET: /Account/Details
        [HttpGet]
        public IActionResult Details()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem thông tin tài khoản.";
                return RedirectToAction("Login");
            }

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }
        // GET: /Account/Edit
        [HttpGet]
        public IActionResult Edit()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            return View(user);
        }


        [HttpPost]
        public IActionResult Edit(UserModel model, IFormFile? AvatarFile)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để cập nhật thông tin.";
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            // Cập nhật thông tin cơ bản
            user.Email = model.Email;
            user.PhoneNumur = model.PhoneNumur;
            user.Address = model.Address;

            // Xử lý ảnh đại diện nếu người dùng upload
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image/user_image");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{Guid.NewGuid()}_{AvatarFile.FileName}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    AvatarFile.CopyTo(stream);
                }

                user.Avatar = $"image/user_image/{fileName}";
                HttpContext.Session.SetString("Avatar", user.Avatar);
            }

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Details");
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đổi mật khẩu.";
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            // Cập nhật mật khẩu
            user.Password = model.NewPassword;
            _context.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Details");
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(); // sẽ hiển thị form nhập email
        }
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Email = email;
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View(); // vẫn giữ lại form nhập email
            }

            // Tạo và lưu OTP
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("ResetOTP", otp);
            HttpContext.Session.SetInt32("ResetUserId", user.UserId);

            // Gửi mail
            SendEmail(email, "Mã xác nhận đặt lại mật khẩu", $"Mã xác nhận của bạn là: {otp}");

            // ✅ Sau khi gửi OTP, điều hướng tới trang đặt lại mật khẩu
            return RedirectToAction("ResetPassword");
        }

        public IActionResult ResetPassword() => View();

        [HttpPost]
        public IActionResult ResetPassword(string otp, string newPassword)
        {
            var savedOtp = HttpContext.Session.GetString("ResetOTP");
            var userId = HttpContext.Session.GetInt32("ResetUserId");

            if (savedOtp == null || userId == null || otp != savedOtp)
            {
                ViewBag.Error = "Mã xác nhận không đúng.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại.";
                return View();
            }

            user.Password = newPassword;
            _context.SaveChanges();

            HttpContext.Session.Remove("ResetOTP");
            HttpContext.Session.Remove("ResetUserId");

            TempData["Success"] = "Mật khẩu đã được cập nhật!";
            return RedirectToAction("Login");
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Shop Đông Y", "your-email@gmail.com"));  // <== thay email thật
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // Thay thế bằng email thật và App Password (không dùng mật khẩu tài khoản Gmail!)
                client.Authenticate("nguyendung23012003@gmail.com", "dyuwousypmsaozre");  // <== App Password 16 ký tự

                client.Send(message);
                client.Disconnect(true);
            }
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
