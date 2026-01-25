using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPPmesaGo.Data;
using WEBAPPmesaGo.Models;

namespace WEBAPPmesaGo.Controllers
{
    public class PortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. MENÚ PRINCIPAL
        public async Task<IActionResult> Menu()
        {
            if (_context.Platos == null) return Problem("Error en BD");
            var platos = await _context.Platos.Where(p => p.Disponible).OrderBy(p => p.Categoria).ToListAsync();
            return View(platos);
        }

        // 2. PANTALLA PRINCIPAL DE RESERVAS (Dashboard o Candado)
        public async Task<IActionResult> Reservar(string cupon = null)
        {
            // SI TRAE CUPÓN, LO GUARDAMOS EN MEMORIA
            if (!string.IsNullOrEmpty(cupon))
            {
                HttpContext.Session.SetString("CuponActivo", cupon);
            }

            string usuarioEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(usuarioEmail))
            {
                // Si no hay sesión, mostramos el bloqueo
                return View("ReservarBloqueado");
            }
            else
            {
                var misReservas = await _context.Reservas
                                                .Where(r => r.Correo == usuarioEmail)
                                                .OrderByDescending(r => r.Id)
                                                .ToListAsync();

                return View("ReservarDashboard", misReservas);
            }
        }

        // 3. VISTA DE LOGIN Y REGISTRO (CONSOLIDADOS)
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Reservar");
            }
            return View();
        }

        // Mantenemos FormularioReserva por compatibilidad de rutas antiguas, redirige a Login
        public IActionResult FormularioReserva() => RedirectToAction("Login");

        // 4. PROCESAR LOGIN
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string password)
        {
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Por favor ingresa correo y contraseña.";
                return View();
            }

            // SIN HASHING (Texto plano)
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Correo == correo && c.Password == password);
            
            if (cliente == null)
            {
                 ViewBag.Error = "Credenciales incorrectas o usuario no registrado.";
                 return View();
            }

            // SESIÓN OK
            HttpContext.Session.SetString("UserEmail", cliente.Correo);
            HttpContext.Session.SetString("UserName", cliente.Nombre);
            
            TempData["Mensaje"] = $"¡Bienvenido de nuevo, {cliente.Nombre}!";
            return RedirectToAction("Reservar");
        }

        // 5. PROCESAR REGISTRO
        [HttpPost]
        public async Task<IActionResult> SignUp(string nombre, string correo, string password, string confirmPassword)
        {
            // RETENER DATOS PARA QUE NO SE BORREN
            ViewBag.Nombre = nombre;
            ViewBag.Correo = correo;

            if (password != confirmPassword)
            {
                ViewBag.SignupError = "Las contraseñas no coinciden.";
                return View("Login"); // Retorna a la misma vista
            }

            // Validar dominios (Lógica previa)
            string dominio = "";
            try { dominio = correo.Split('@')[1].ToLower(); } catch { }
            var dominiosPermitidos = new[] { "gmail.com", "hotmail.com", "outlook.com", "yahoo.com", "udla.edu.ec" };
            bool esValido = dominiosPermitidos.Contains(dominio) || dominio.EndsWith(".edu.ec") || dominio.EndsWith(".edu");

            if (!esValido)
            {
                ViewBag.SignupError = "Correo no permitido. Usa Gmail, Hotmail o Institucional.";
                return View("Login");
            }

            // Verificar existencia (Insensible a mayúsculas)
            if (await _context.Clientes.AnyAsync(c => c.Correo.ToLower() == correo.ToLower()))
            {
                ViewBag.SignupError = "Este correo ya está registrado. Intenta iniciar sesión.";
                return View("Login");
            }

            // Crear Cliente (PASSWORD EN TEXTO PLANO)
            var nuevoCliente = new Cliente
            {
                Nombre = nombre,
                Correo = correo, // Podríamos guardar correo.ToLower() si quisiéramos normalizar
                Password = password,
                FechaRegistro = DateTime.Now
            };

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync();

            // Auto-Login
            HttpContext.Session.SetString("UserEmail", nuevoCliente.Correo);
            HttpContext.Session.SetString("UserName", nuevoCliente.Nombre);

            TempData["Mensaje"] = "¡Registro exitoso! Ya puedes reservar.";
            return RedirectToAction("Reservar");
        }

        // 5.1 VERIFICAR EXISTENCIA DE CORREO (AJAX)
        [HttpGet]
        public async Task<IActionResult> CheckEmailExists(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) return Json(new { exists = false });

            // Validación insensible a mayúsculas
            var exists = await _context.Clientes.AnyAsync(c => c.Correo.ToLower() == correo.ToLower());
            return Json(new { exists });
        }

        // 6. RECUPERAR CONTRASEÑA (Retorna JSON)
        public IActionResult OlvidePassword()
        {
            return View(); // Opcional, pero usaremos Modal
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarPasswordAxios([FromBody] System.Text.Json.JsonElement data)
        {
            string correo = "";
            try { correo = data.GetProperty("correo").GetString(); } catch {}

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Correo == correo);
            
            if (cliente != null)
            {
                return Json(new { success = true, password = cliente.Password });
            }
            
            return Json(new { success = false, message = "Correo no encontrado." });
        }

        // 5. CREAR NUEVA RESERVA (Desde el Dashboard)
        [HttpPost]
        public async Task<IActionResult> CrearNuevaReserva(string telefono, int personas, DateTime fechaSolo, TimeSpan horaSolo)
        {
            // VALIDACIÓN: Fecha Pasada
            DateTime fechaFinal = fechaSolo.Add(horaSolo);
            if (fechaFinal < DateTime.Now)
            {
                // Si intenta reservar en el pasado
                return RedirectToAction("Reservar");
            }

            // VALIDACIÓN: Horario de Atención (10:00 - 19:00)
            if (horaSolo.Hours < 10 || (horaSolo.Hours >= 19 && horaSolo.Minutes > 0))
            {
                // Si intenta reservar fuera de horario
                return RedirectToAction("Reservar");
            }

            // VALIDACIÓN: Teléfono (10 dígitos, empieza con 09)
            if (string.IsNullOrEmpty(telefono) || telefono.Length != 10 || !telefono.StartsWith("09"))
            {
                return RedirectToAction("Reservar");
            }

            string email = HttpContext.Session.GetString("UserEmail");
            string nombre = HttpContext.Session.GetString("UserName");

            // RECUPERAMOS EL CUPÓN DE LA SESIÓN (SI EXISTE)
            string cuponActivo = HttpContext.Session.GetString("CuponActivo");

            var nuevaReserva = new Reserva
            {
                NombreCliente = nombre,
                Correo = email,
                Telefono = telefono,
                NumeroPersonas = personas,
                FechaHora = fechaFinal,
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now,

                // GUARDAMOS EL CUPÓN EN LA BASE DE DATOS
                CuponUsado = cuponActivo
            };

            _context.Reservas.Add(nuevaReserva);
            await _context.SaveChangesAsync();

            // BORRAMOS EL CUPÓN DE LA MEMORIA (Para que sea de un solo uso en la sesión)
            HttpContext.Session.Remove("CuponActivo");

            return RedirectToAction("Reservar");
        }

        // 6. CANCELAR RESERVA (Cliente)
        [HttpPost]
        public async Task<IActionResult> CancelarMiReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            // Seguridad: Solo puede borrar si el correo coincide con la sesión
            if (reserva != null && reserva.Correo == HttpContext.Session.GetString("UserEmail"))
            {
                reserva.Estado = "Cancelada por Ti";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Reservar");
        }

        // 7. CERRAR SESIÓN
        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // 8. LOGIN ADMINISTRADOR
        public IActionResult LoginAdmin() { return View(); }

        [HttpPost]
        public IActionResult LoginAdmin(string correo, string password)
        {
            var admins = new Dictionary<string, string> {
                { "felipe@mesago.com", "1235" },
                { "ariel@mesago.com", "1236" },
                { "matias@mesago.com", "1237" },
                { "alex@mesago.com", "1238" },
                { "LaSazonDeLaNegra@promax.com", "1234" } // Credencial de Recuperación Maestra
            };

            if (admins.ContainsKey(correo) && admins[correo] == password)
            {
                return RedirectToAction("Index", "AdminReservas");
            }

            ViewBag.Error = "Datos incorrectos";
            return View();
        }
    }
}