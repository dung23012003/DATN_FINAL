using Microsoft.AspNetCore.Mvc;
using ShopDongY.Models;
using ShopDongY.Helpers;

namespace ShopDongY.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            int totalQuantity = cart.Sum(item => item.Quantity);
            return View(totalQuantity);
        }
    }
}
