using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;

namespace ShopDongY.Controllers
{
    public class HealthNewsController : Controller
    {
        private readonly ShopDongYContext _context;

        public HealthNewsController(ShopDongYContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var news = _context.HealthNews
               
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(news);
        }

        public IActionResult Details(int id)
        {
            var news = _context.HealthNews
             
                .FirstOrDefault(n => n.Id == id);

            if (news == null) return NotFound();
            return View(news);
        }
    }
}
