using Microsoft.AspNetCore.Mvc;
using ShopDongY.Data;
using ShopDongY.Helpers;
using ShopDongY.Models;

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
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.Product.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Product = product,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["Success"] = "Đã thêm vào giỏ hàng.";
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int change)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.Product.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += change;
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


    }
}
