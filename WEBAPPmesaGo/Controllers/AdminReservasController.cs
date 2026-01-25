using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPPmesaGo.Data;
using WEBAPPmesaGo.Models;

namespace WEBAPPmesaGo.Controllers
{
    public class AdminReservasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Muestra la lista de reservas filtrada
        public async Task<IActionResult> Index(string estado = "Todas")
        {
            var query = _context.Reservas.AsQueryable();

            // 1. LÓGICA DE FILTROS
            if (estado == "Pendiente")
            {
                query = query.Where(r => r.Estado == "Pendiente");
            }
            else if (estado == "Confirmada")
            {
                query = query.Where(r => r.Estado == "Confirmada");
            }
            else if (estado == "Cancelada")
            {
                // CAMBIO IMPORTANTE: Usamos Contains para que traiga "Cancelada por Ti" Y "Cancelada por Admin"
                query = query.Where(r => r.Estado.Contains("Cancelada"));
            }
            // Si es "Todas", no filtramos nada (Historial Completo)

            // Guardamos el estado actual para saber qué pestaña pintar de negro en la vista
            ViewData["EstadoActual"] = estado;

            // Ordenamos por fecha descendente (lo más nuevo arriba)
            var reservas = await query.OrderByDescending(r => r.FechaHora).ToListAsync();

            return View(reservas);
        }

        // ACCIÓN: Aprobar Reserva (Botón Verde)
        [HttpPost]
        public async Task<IActionResult> Aprobar(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                reserva.Estado = "Confirmada";
                await _context.SaveChangesAsync();
            }
            // Volvemos a la lista de pendientes para seguir trabajando
            return RedirectToAction(nameof(Index), new { estado = "Pendiente" });
        }

        // ACCIÓN: Cancelar Reserva (Botón Rojo de la tabla)
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                // Especificamos que fue el admin quien canceló
                reserva.Estado = "Cancelada por Admin";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { estado = "Pendiente" });
        }

        // ⚠️ ACCIÓN: BORRAR TODO EL HISTORIAL (Botón de Pánico "RESETEAR SISTEMA")
        // ⚠️ ACCIÓN: BORRAR TODO EL HISTORIAL Y PLATOS DE PRUEBA
        [HttpPost]
        public async Task<IActionResult> BorrarTodoHistorial()
        {
            // 1. Borrar TODAS las reservas (Eso sigue igual)
            var todasLasReservas = await _context.Reservas.ToListAsync();
            _context.Reservas.RemoveRange(todasLasReservas);

            // 2. NUEVO: Borrar SOLO los platos creados con IA (los de prueba)
            // Lógica: Si la imagen empieza con 'http', es de internet (creado por ti).
            // Si la imagen es '~/img/...', es original (no se borra).
            var platosDePrueba = await _context.Platos
                                        .Where(p => p.ImagenUrl.StartsWith("http"))
                                        .ToListAsync();

            if (platosDePrueba.Any())
            {
                _context.Platos.RemoveRange(platosDePrueba);
            }

            await _context.SaveChangesAsync();

            // 3. NUEVO: Borrar también el log de ventas del Dashboard (Memoria)
            AdminDashboardController.SalesLog.Clear();

            // Volvemos a la lista limpia
            TempData["Mensaje"] = "Sistema reseteado: Reservas, platos de prueba y dashboard limpios.";
            return RedirectToAction(nameof(Index));
        }
    }       
}