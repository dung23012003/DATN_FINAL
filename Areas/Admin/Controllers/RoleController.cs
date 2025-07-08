using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

namespace ShopDongY.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly ShopDongYContext _context;

        public RoleController(ShopDongYContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var totalRoles = await _context.Roles.CountAsync();

            var roles = await _context.Roles
                .OrderByDescending(p => p.RoleId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRoles / pageSize);

            return View(roles);
        }

        [HttpGet]

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(RoleModel role)
        {
            if (ModelState.IsValid)
            {
                _context.Roles.Add(role);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = _context.Roles.Find(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }
        [HttpPost]
        public IActionResult Edit(int id, RoleModel role)
        {
            if(id != role.RoleId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _context.Roles.Update(role);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(role);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var role = _context.Roles.Find(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = _context.Roles.Find(id);
            if (role == null)
            {
                return NotFound();
            }
            _context.Roles.Remove(role);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
