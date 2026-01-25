using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPPmesaGo.Data;
using WEBAPPmesaGo.Models;

namespace WEBAPPmesaGo.Controllers
{
    public class AdminClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Seguridad básica
            // if (HttpContext.Session.GetString("AdminUser") == null) return RedirectToAction("LoginAdmin", "Portal");

            var clientes = await _context.Clientes
                                         .OrderByDescending(c => c.FechaRegistro)
                                         .ToListAsync();
            return View(clientes);
        }

        // GET: AdminClientes/Historial/5
        public async Task<IActionResult> Historial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (cliente == null)
            {
                return NotFound();
            }

            // Buscar reservas asociadas por correo
            var reservas = await _context.Reservas
                                         .Where(r => r.Correo == cliente.Correo)
                                         .OrderByDescending(r => r.FechaHora)
                                         .ToListAsync();

            ViewBag.Reservas = reservas;

            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarHistorial()
        {
            var todos = _context.Clientes.ToList();
            if (todos.Any())
            {
                _context.Clientes.RemoveRange(todos);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
