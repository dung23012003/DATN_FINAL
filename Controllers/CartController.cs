using Microsoft.AspNetCore.Mvc;
using ShopDongY.Data;
using ShopDongY.Helpers;
using ShopDongY.Models;
using ShopDongY.ViewModels;

namespace ShopDongY.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopDongYContext _context;

        public CartController(ShopDongYContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var product = _context.Products
                .Where(p => p.ProductId == productId)
                .Select(p => new
                {
                    Product = p,
                    Warehouse = p.Warehouse
                })
                .FirstOrDefault();

            if (product == null || product.Product == null)
                return NotFound();

            int stockAvailable = product.Warehouse?.QuantityInStock ?? 0;

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.Product.ProductId == productId);
            int cartCurrentQuantity = cartItem?.Quantity ?? 0;

            // Kiểm tra nếu vượt quá hàng tồn kho
            if (cartCurrentQuantity + quantity > stockAvailable)
            {
                TempData["Error"] = $"Chỉ còn {stockAvailable - cartCurrentQuantity} sản phẩm trong kho.";
                return RedirectToAction("Index", "Cart");
            }

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Product = product.Product,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["Success"] = "Đã thêm vào giỏ hàng.";
            return Redirect(Request.Headers["Referer"].ToString());
        }


        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int change)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.Product.ProductId == productId);
            if (cartItem != null)
            {
                // Lấy thông tin tồn kho từ database
                var warehouse = _context.Warehouses.FirstOrDefault(w => w.ProductId == productId);
                int stockAvailable = warehouse?.QuantityInStock ?? 0;

                // Nếu tăng số lượng, kiểm tra tồn kho
                if (change > 0 && cartItem.Quantity + change > stockAvailable)
                {
                    TempData["Error"] = $"Không thể tăng vượt quá số lượng trong kho. Tồn kho hiện tại: {stockAvailable}.";
                    return RedirectToAction("Index");
                }

                cartItem.Quantity += change;

                // Nếu giảm về 0 hoặc nhỏ hơn thì xoá
                if (cartItem.Quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            cart.RemoveAll(c => c.Product.ProductId == productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            var model = new CheckoutViewModel
            {
                CartItems = cart
            };

            // Nếu đã đăng nhập, tự động lấy thông tin người dùng
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
                if (user != null)
                {
                    model.FullName = user.UserName;
                    model.Email = user.Email;
                    model.PhoneNumber = user.PhoneNumur;
                    model.Address = user.Address;
                }
            }

            return View(model);
        }


        [HttpPost]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            model.CartItems = cart;
            ModelState.Remove(nameof(model.CartItems)); // Không cần validate giỏ hàng từ người dùng

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => $"• {e.ErrorMessage}")
                    .ToList();
                ViewBag.ModelErrors = errors;
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // ✅ Tạo đơn hàng
                var order = new OrderModel
                {
                    UserId = userId.Value,
                    OrderDate = DateTime.Now,
                    OrderEmail = model.Email,
                    TotalAmount = cart.Sum(c => c.Product.Price * c.Quantity),
                    Status = OrderModel.OrderStatus.Pending,
                    ShippingAddress = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    Note = model.Note
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // ✅ Chi tiết đơn hàng
                foreach (var item in cart)
                {
                    var orderDetail = new OrderDetailsModel
                    {
                        OrderId = order.OrderId,
                        ProductId = item.Product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    var warehouse = _context.Warehouses.FirstOrDefault(w => w.ProductId == item.Product.ProductId);
                    if (warehouse != null)
                    {
                        warehouse.TotalSold += item.Quantity;
                        warehouse.LastUpdated = DateTime.Now;
                    }
                }

                _context.SaveChanges();
                HttpContext.Session.Remove("Cart");

                if (model.PaymentMethod == "Online")
                {
                    var productName = cart.Count == 1 ? cart[0].Product.ProductName : "NhieuSanPham";
                    var userName = _context.Users.FirstOrDefault(u => u.UserId == userId.Value)?.UserName ?? "KhachHang";

                    return RedirectToAction("Index", "Payment", new
                    {
                        orderId = order.OrderId,
                        amount = order.TotalAmount,
                        productName = productName,
                        userName = userName
                    });
                }

                TempData["Success"] = "Đặt hàng thành công. Đơn hàng đang chờ xử lý.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi khi xử lý đơn hàng: " + ex.Message;
                return View(model);
            }
        }




    }
}
