using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPPmesaGo.Data;
using WEBAPPmesaGo.Models;

namespace WEBAPPmesaGo.Controllers
{
    public class AdminCuponesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCuponesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cupones = await _context.Cupones.OrderByDescending(c => c.Id).ToListAsync();
            return View(cupones);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Cupon cupon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cupon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Si falla, recargamos la lista y volvemos a la vista (idealmente enviando errores)
            var lista = await _context.Cupones.OrderByDescending(c => c.Id).ToListAsync();
            return View("Index", lista);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cupon = await _context.Cupones.FindAsync(id);
            if (cupon != null)
            {
                _context.Cupones.Remove(cupon);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public async Task<IActionResult> ToggleEstado(int id)
        {
             var cupon = await _context.Cupones.FindAsync(id);
            if (cupon != null)
            {
                cupon.Activo = !cupon.Activo;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
