using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WEBAPPmesaGo.Controllers
{
    public class HomeController : Controller
    {
        private readonly WEBAPPmesaGo.Data.ApplicationDbContext _context;

        public HomeController(WEBAPPmesaGo.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        // Esta accin carga la vista Views/Home/Index.cshtml
        public async Task<IActionResult> Index()
        {
            var activeCoupons = await _context.Cupones.Where(c => c.Activo).ToListAsync();
            ViewData["Cupones"] = activeCoupons;

            var platos = await _context.Platos.Where(p => p.Disponible).ToListAsync();
            return View(platos);
        }
    }
}