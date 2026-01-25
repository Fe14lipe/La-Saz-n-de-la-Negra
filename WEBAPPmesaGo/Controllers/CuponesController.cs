using Microsoft.AspNetCore.Mvc;
using WEBAPPmesaGo.Services;
using Microsoft.EntityFrameworkCore;

namespace WEBAPPmesaGo.Controllers
{
    public class CuponesController : Controller
    {
        private readonly NagerDateService _nagerDateService;
        private readonly WEBAPPmesaGo.Data.ApplicationDbContext _context;

        public CuponesController(NagerDateService nagerDateService, WEBAPPmesaGo.Data.ApplicationDbContext context)
        {
            _nagerDateService = nagerDateService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Obtenemos el año actual
            int anio = DateTime.Now.Year;

            // 2. Traemos la lista de feriados de la API
            var listaFeriados = await _nagerDateService.ObtenerFeriadosEcuador(anio);

            // 3. Verificamos si HOY es feriado para activar el cupón especial
            bool esFeriadoHoy = listaFeriados.Any(f => f.Date.Date == DateTime.Now.Date);

            // 4. Obtener cupones activos de la BD
            var cuponesBD = await _context.Cupones.Where(c => c.Activo).ToListAsync();

            // Pasamos los datos a la vista usando ViewBag
            ViewBag.Feriados = listaFeriados;
            ViewBag.EsFeriado = esFeriadoHoy;
            ViewBag.CuponesActivos = cuponesBD;

            return View();
        }
    }
}