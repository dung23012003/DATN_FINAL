using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WarehouseController : Controller
    {
        private readonly ShopDongYContext _context;

        public WarehouseController(ShopDongYContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.ProductId == productId);

            ViewBag.ProductName = product.ProductName;

            // Tính hàng tồn kho hiện tại
            int quantityInStock = 0;
            if (warehouse != null)
            {
                quantityInStock = warehouse.TotalImported - warehouse.TotalSold;
            }

            ViewBag.QuantityInStock = quantityInStock;

            // Trả về model với TotalImported = 0 để nhập mới
            return View(new WarehouseModel
            {
                ProductId = productId,
                TotalImported = 0 // mặc định bằng 0 để user nhập
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseModel model)
        {
            if (!ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(model.ProductId);
                ViewBag.ProductName = product?.ProductName;
                ViewBag.ProductId = model.ProductId;
                return View(model);
            }

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.ProductId == model.ProductId);

            if (warehouse == null)
            {
                warehouse = new WarehouseModel
                {
                    ProductId = model.ProductId,
                    TotalImported = model.TotalImported,
                    TotalSold = 0,
                    LastUpdated = DateTime.Now
                };
                _context.Warehouses.Add(warehouse);
            }
            else
            {
                warehouse.TotalImported += model.TotalImported;
                warehouse.LastUpdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Nhập hàng thành công.";
            return RedirectToAction("Index", "Product", new { area = "Admin" });
        }
    }
}
