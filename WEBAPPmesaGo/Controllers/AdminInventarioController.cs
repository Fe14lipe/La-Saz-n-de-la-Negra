using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPPmesaGo.Data;
using WEBAPPmesaGo.Models;

namespace WEBAPPmesaGo.Controllers
{
    public class AdminInventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminInventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 10;
            
            var query = _context.Ingredientes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.Nombre.ToLower().Contains(search.ToLower()));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var ingredientes = await query
                .OrderBy(i => i.Nombre) // Ensure consistent ordering
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;

            return View(ingredientes);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarIngrediente(string nombre, decimal cantidad, string unidad, decimal stockMinimo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "El nombre del ingrediente no puede estar vacío.";
                return RedirectToAction(nameof(Index));
            }

            var existe = await _context.Ingredientes
                .AnyAsync(i => i.Nombre.ToLower() == nombre.ToLower());

            if (existe)
            {
                TempData["Error"] = $"¡El ingrediente '{nombre}' ya existe en el inventario!";
                return RedirectToAction(nameof(Index));
            }

            var nuevoIngrediente = new Ingrediente
            {
                Nombre = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombre.ToLower()),
                Cantidad = cantidad,
                Unidad = unidad,
                StockMinimo = stockMinimo
            };

            _context.Ingredientes.Add(nuevoIngrediente);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Ingrediente '{nuevoIngrediente.Nombre}' agregado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarStock(int id, decimal cantidad, string operacion)
        {
            var ingrediente = await _context.Ingredientes.FindAsync(id);
            if (ingrediente == null) return Json(new { success = false, message = "Ingrediente no encontrado" });

            if (operacion == "sumar")
            {
                ingrediente.Cantidad += cantidad;
                
                // Log Suma
                _context.MovimientosInventario.Add(new MovimientoInventario
                {
                    IngredienteId = ingrediente.Id,
                    Cantidad = cantidad,
                    Fecha = DateTime.Now,
                    Tipo = "Ajuste Manual",
                    Detalle = "Reposición / Compra (Manual)"
                });
            }
            else if (operacion == "restar")
            {
                ingrediente.Cantidad -= cantidad;

                // Log Resta
                _context.MovimientosInventario.Add(new MovimientoInventario
                {
                    IngredienteId = ingrediente.Id,
                    Cantidad = -cantidad,
                    Fecha = DateTime.Now,
                    Tipo = "Ajuste Manual",
                    Detalle = "Baja / Desperdicio (Manual)"
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, nuevoTotal = ingrediente.Cantidad, mensaje = "Stock actualizado." });
        }

        [HttpPost]
        public async Task<IActionResult> EditarIngrediente(int id, string nombre, string unidad, decimal stockMinimo)
        {
            var ingrediente = await _context.Ingredientes.FindAsync(id);
            if (ingrediente == null)
            {
                TempData["Error"] = "Ingrediente no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "El nombre no puede estar vacío.";
                return RedirectToAction(nameof(Index));
            }

            // Validar duplicado solo si cambió el nombre
            if (ingrediente.Nombre.ToLower() != nombre.ToLower())
            {
                var existe = await _context.Ingredientes.AnyAsync(i => i.Nombre.ToLower() == nombre.ToLower());
                if (existe)
                {
                    TempData["Error"] = $"¡El nombre '{nombre}' ya existe!";
                    return RedirectToAction(nameof(Index));
                }
            }

            ingrediente.Nombre = nombre;
            ingrediente.Unidad = unidad;
            ingrediente.StockMinimo = stockMinimo;

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Ingrediente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarIngrediente(int id)
        {
            var ingrediente = await _context.Ingredientes.FindAsync(id);
            if (ingrediente != null)
            {
                _context.Ingredientes.Remove(ingrediente);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Ingrediente eliminado del inventario.";
            }
            else
            {
                TempData["Error"] = "No se pudo encontrar el ingrediente.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GestionarReceta(int id)
        {
            var plato = await _context.Platos
                .Include(p => p.Ingredientes).ThenInclude(pi => pi.Ingrediente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plato == null) return NotFound();

            ViewBag.TodosIngredientes = await _context.Ingredientes.OrderBy(i => i.Nombre).ToListAsync();
            return View(plato);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPreparacion(int id, string preparacion)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null) return NotFound();

            plato.Preparacion = preparacion;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GestionarReceta), new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> AgregarIngredientePlato(int platoId, int ingredienteId, decimal cantidad)
        {
            var exists = await _context.PlatoIngredientes
                .AnyAsync(pi => pi.PlatoId == platoId && pi.IngredienteId == ingredienteId);

            if (!exists)
            {
                var pi = new PlatoIngrediente
                {
                    PlatoId = platoId,
                    IngredienteId = ingredienteId,
                    CantidadRequerida = cantidad
                };
                _context.PlatoIngredientes.Add(pi);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(GestionarReceta), new { id = platoId });
        }
        
        [HttpPost]
        public async Task<IActionResult> EliminarIngredientePlato(int id, int platoId)
        {
            var pi = await _context.PlatoIngredientes.FindAsync(id);
            if (pi != null)
            {
                _context.PlatoIngredientes.Remove(pi);
                await _context.SaveChangesAsync();
            }
             return RedirectToAction(nameof(GestionarReceta), new { id = platoId });
        }
            


        [HttpPost]
        public async Task<IActionResult> LimpiarHistorial()
        {
            var todos = await _context.MovimientosInventario.ToListAsync();
            _context.MovimientosInventario.RemoveRange(todos);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Historial));
        }

        public async Task<IActionResult> Historial()
        {
            var historial = await _context.MovimientosInventario
                .Include(m => m.Ingrediente)
                .OrderByDescending(m => m.Fecha)
                .Take(100)
                .ToListAsync();

            return View(historial);
        }
    }
}
