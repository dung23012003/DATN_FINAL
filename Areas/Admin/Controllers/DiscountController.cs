using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DiscountController : Controller
    {
        private readonly ShopDongYContext _context;

        public DiscountController(ShopDongYContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var discounts = await _context.Discounts.Include(d => d.Product).ToListAsync();
            return View(discounts);
        }

        public IActionResult Create()
        {
            ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountModel discount)
        {
            if (discount.StartDate >= discount.EndDate)
            {
                ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName", discount.ProductId);
                return View(discount);
            }

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName", discount.ProductId);
            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiscountModel discount)
        {
            if (id != discount.DiscountId) return NotFound();

            if (discount.StartDate >= discount.EndDate)
            {
                ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName", discount.ProductId);
                return View(discount);
            }

            try
            {
                _context.Update(discount);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật giảm giá thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật giảm giá.");
                return View(discount);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var discount = await _context.Discounts
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.DiscountId == id);

            if (discount == null) return NotFound();

            return View(discount);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa giảm giá thành công.";
            return RedirectToAction(nameof(Index));
        }


    }

}
