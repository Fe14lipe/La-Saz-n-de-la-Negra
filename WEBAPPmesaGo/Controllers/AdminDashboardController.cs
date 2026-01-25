using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPPmesaGo.Data;
using WEBAPPmesaGo.Models; // Adjust namespace if needed

namespace WEBAPPmesaGo.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ALMACENAMIENTO EN MEMORIA (SIMPLE / HACK PARA DEMO)
        // En un sistema real, esto leería de una tabla "Ventas".
        // ALMACENAMIENTO EN MEMORIA (SIMPLE / HACK PARA DEMO)
        // Guardamos Fecha, Monto y Nombre del Plato
        public static List<(DateTime Date, decimal Amount, string DishName)> SalesLog = new List<(DateTime, decimal, string)>();

        public static void RegistrarVenta(decimal amount, string dishName)
        {
            SalesLog.Add((DateTime.Now, amount, dishName));
        }

        // Endpoint for Real-time Chart Data (Polled by JS)
        [HttpGet]
        public async Task<IActionResult> GetDashboardData(string? dateStr)
        {
            DateTime referenceDate = DateTime.Now;
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime parsedDate))
            {
                referenceDate = parsedDate;
            }

            // 1. Calcular Lunes de la semana de referencia
            var currentDayOfWeek = (int)referenceDate.DayOfWeek; // 0=Sun, 1=Mon...
            var offsetToMonday = referenceDate.DayOfWeek == DayOfWeek.Sunday ? -6 : 1 - currentDayOfWeek;
            var mondayResult = referenceDate.AddDays(offsetToMonday).Date;
            var endOfWeek = mondayResult.AddDays(7);

            // 2. Reservations by Status
            var reservationsAll = await _context.Reservas.ToListAsync();
            var weeklyReservations = reservationsAll.Where(r => r.FechaHora.Date >= mondayResult && r.FechaHora.Date < endOfWeek).ToList();

            var pendientes = weeklyReservations.Count(r => r.Estado == "Pendiente");
            var confirmadas = weeklyReservations.Count(r => r.Estado == "Confirmada");
            var canceladas = weeklyReservations.Count(r => r.Estado.Contains("Cancelada"));

            // 3. Procesar Ventas y Reservas de la Semana Seleccionada
            int[] weeklySalesCount = new int[6];
            int[] weeklyReservationsCount = new int[6];
            List<List<string>> weeklyDishDetails = new List<List<string>>(); // Lista de listas de platos por día
            List<List<string>> weeklyReservationDetails = new List<List<string>>(); // Lista de listas de reservas por día
            decimal totalIncome = 0;

            // Filtrar ventas de esta semana
            var ventasSemana = SalesLog.Where(d => d.Date.Date >= mondayResult && d.Date.Date < endOfWeek).ToList();

            for (int i = 0; i < 6; i++) // 0=Lun ... 5=Sáb
            {
                var targetDate = mondayResult.AddDays(i);
                
                // Ventas del día
                var salesOfDay = ventasSemana.Where(v => v.Date.Date == targetDate).ToList();
                weeklySalesCount[i] = salesOfDay.Count;
                weeklyDishDetails.Add(salesOfDay.Select(v => v.DishName).ToList()); // Guardar nombres

                // Reservas del día
                var reservationsOfDay = weeklyReservations.Where(r => r.FechaHora.Date == targetDate).ToList();
                weeklyReservationsCount[i] = reservationsOfDay.Count;
                
                // Obtener las últimas 5 reservas del día, ordenadas por hora
                var last5Reservations = reservationsOfDay
                    .OrderByDescending(r => r.FechaHora)
                    .Take(5)
                    .Select(r => $"{r.NombreCliente} ({r.FechaHora:HH:mm})")
                    .ToList();
                weeklyReservationDetails.Add(last5Reservations);
            }

            totalIncome = ventasSemana.Sum(v => v.Amount);

            return Json(new
            {
                reservations = new { pendientes, confirmadas, canceladas },
                reservationsWeekly = weeklyReservationsCount,
                reservationsDetails = weeklyReservationDetails, // Detalles para el Tooltip de Reservas
                sales = weeklySalesCount,
                salesDetails = weeklyDishDetails, // Detalles para el Tooltip de Ventas
                income = totalIncome,
                startDate = mondayResult.ToString("dd MMM"),
                endDate = mondayResult.AddDays(6).ToString("dd MMM yyyy"),
                lastUpdated = DateTime.Now.ToString("HH:mm:ss")
            });
        }
    }
}
