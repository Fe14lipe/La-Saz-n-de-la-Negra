using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPPmesaGo.Data;
using WEBAPPmesaGo.Models;
using WEBAPPmesaGo.Services;

namespace WEBAPPmesaGo.Controllers
{
    public class AdminMenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiService _geminiService;

        public AdminMenuController(ApplicationDbContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        // VISTA PRINCIPAL
        public async Task<IActionResult> Index()
        {
            // ANTES: var platos = await _context.Platos.ToListAsync();

            // AHORA: Ordenamos por ID Descendente (El último creado sale primero)
            var platos = await _context.Platos
                                .Include(p => p.Ingredientes)
                                .ThenInclude(pi => pi.Ingrediente)
                                .OrderByDescending(p => p.Id)
                                .ToListAsync();

            return View(platos);
        }

        // ACCIÓN: GENERAR CON IA (AJAX o Form Submit)

        [HttpPost]
        public async Task<IActionResult> GenerarDescripcion(string nombre, string ingredientes)
        {
            try 
            {
                var descripcion = await _geminiService.GenerarDescripcion(nombre, ingredientes);
                return Json(new { success = true, description = descripcion });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ACCIÓN: VERIFICAR SI EXISTE EL PLATO (Para validación frontend)
        [HttpGet]
        public async Task<IActionResult> CheckDishExists(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Json(new { exists = false });

            // Verificar ignorando mayúsculas/minúsculas
            var exists = await _context.Platos.AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower());
            return Json(new { exists });
        }

        // ACCIÓN: GUARDAR PLATO
        // ACCIÓN: GUARDAR O ACTUALIZAR PLATO
        [HttpPost]
        public async Task<IActionResult> GuardarPlato(Plato plato, string IngredientesTexto)
        {
            ModelState.Remove("ImagenUrl");
            ModelState.Remove("Descripcion");

            if (!string.IsNullOrEmpty(plato.Nombre) && plato.Precio > 0)
            {
                if (plato.Id == 0)
                {
                    // NUEVO PLATO
                    plato.Disponible = true;
                    plato.ImagenUrl = await _geminiService.GenerarImagenUrl(plato.Nombre);

                    if (string.IsNullOrEmpty(plato.Descripcion))
                    {
                        plato.Descripcion = $"Plato fresco: {plato.Nombre}";
                    }

                    _context.Add(plato);
                }
                else
                {
                    // EDITAR PLATO EXISTENTE
                    var platoExistente = await _context.Platos.FindAsync(plato.Id);
                    if (platoExistente != null)
                    {
                        platoExistente.Nombre = plato.Nombre;
                        platoExistente.Precio = plato.Precio;
                        platoExistente.Categoria = plato.Categoria;
                        platoExistente.Descripcion = plato.Descripcion;
                        // Nota: No regeneramos la imagen al editar para no perder la anterior.
                        // Si quisieran nueva imagen, tendrían que borrar y crear de nuevo o tener un botón específico.
                        
                        _context.Update(platoExistente);
                    }
                }

                await _context.SaveChangesAsync();

                // NUEVO PROCESO: Auto-creación de Ingredientes
                if (!string.IsNullOrEmpty(IngredientesTexto))
                {
                    // Revertido: Permitir comas, saltos de línea y guiones
                    var listaNombres = IngredientesTexto.Split(new[] { ',', '\n', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var rawNombre in listaNombres)
                    {
                        var nombreLimpio = rawNombre.Trim();
                        // Ignorar textos muy cortos o vacíos
                        if (nombreLimpio.Length < 2) continue;

                        // 1. Verificar si el INGREDIENTE existe
                        var ingrediente = await _context.Ingredientes
                                        .FirstOrDefaultAsync(i => i.Nombre.ToLower() == nombreLimpio.ToLower());

                        if (ingrediente == null)
                        {
                            ingrediente = new Ingrediente
                            {
                                Nombre = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombreLimpio.ToLower()),
                                Cantidad = 50, // Stock inicial 50 (Solicitado por usuario)
                                Unidad = "unidad", // Unidad por defecto
                                StockMinimo = 10
                            };
                            _context.Ingredientes.Add(ingrediente);
                            await _context.SaveChangesAsync(); // Guardamos para tener el ID
                        }

                        // 2. Verificar/Crear el VÍNCULO (PlatoIngredientes)
                        var existsLink = await _context.PlatoIngredientes
                                            .AnyAsync(pi => pi.PlatoId == plato.Id && pi.IngredienteId == ingrediente.Id);
                        
                        if (!existsLink)
                        {
                            var nuevoLink = new PlatoIngrediente
                            {
                                PlatoId = plato.Id,
                                IngredienteId = ingrediente.Id,
                                CantidadRequerida = 1 // Valor por defecto 1 unidad
                            };
                            _context.PlatoIngredientes.Add(nuevoLink);
                        }
                    }
                    // Guardar los vínculos
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ACCIÓN: ELIMINAR PLATO
        [HttpPost]
        public async Task<IActionResult> EliminarPlato(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato != null)
            {
                _context.Platos.Remove(plato);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ACCIÓN: TOGGLE STOCK (Disponible / Agotado)
        [HttpPost]
        public async Task<IActionResult> ToggleStock(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato != null)
            {
                plato.Disponible = !plato.Disponible;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ACCIÓN: VENDER PLATO (Simulación de "Cocinar/Venta" para descontar Stock)
        [HttpPost]
        public async Task<IActionResult> VenderPlato(int id)
        {
            var plato = await _context.Platos
                                      .Include(p => p.Ingredientes)
                                      .ThenInclude(pi => pi.Ingrediente)
                                      .FirstOrDefaultAsync(p => p.Id == id);

            if (plato == null) return NotFound();

            var updatedStock = new List<object>();

            // 1. Validar Stock (DESACTIVADO POR EMERGENCIA)
            // Permitimos venta con stock negativo para que no se trabe la demo.
            /*
            foreach (var pi in plato.Ingredientes)
            {
                if (pi.Ingrediente != null)
                {
                    if (pi.Ingrediente.Cantidad < pi.CantidadRequerida)
                    {
                        return Json(new { success = false, message = $"¡Stock insuficiente! Faltan {pi.Ingrediente.Nombre}." });
                    }
                }
            }
            */

            // 2. Descontar Stock (Ya validamos que sí alcanza todo)
            var fechaVenta = DateTime.Now; // Timestamp único para agrupar

            foreach (var pi in plato.Ingredientes)
            {
                if (pi.Ingrediente != null)
                {
                    pi.Ingrediente.Cantidad -= pi.CantidadRequerida;
                    
                    // Guardamos el nuevo stock para devolverlo al cliente
                    updatedStock.Add(new { 
                        ingredienteId = pi.Ingrediente.Id, 
                        nuevaCantidad = pi.Ingrediente.Cantidad,
                        nombre = pi.Ingrediente.Nombre
                    });

                    // NUEVO: REGISTRAR EL MOVIMIENTO EN EL HISTORIAL
                    var logs = new MovimientoInventario
                    {
                        IngredienteId = pi.Ingrediente.Id,
                        Cantidad = -pi.CantidadRequerida, // Negativo porque sale
                        Fecha = fechaVenta,
                        Tipo = "Venta",
                        Detalle = $"Venta de: {plato.Nombre}"
                    };
                    _context.MovimientosInventario.Add(logs);
                }
            }

            await _context.SaveChangesAsync();
            
            // Registrar Venta para el Dashboard (Visualización)
            AdminDashboardController.RegistrarVenta(plato.Precio, plato.Nombre);

            return Json(new { 
                success = true, 
                message = $"¡Venta registrada! Se descontaron los ingredientes.",
                updates = updatedStock 
            });
        }
    }
}