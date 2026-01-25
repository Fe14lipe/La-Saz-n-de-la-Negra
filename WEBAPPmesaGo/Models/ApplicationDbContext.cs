using Microsoft.EntityFrameworkCore;
using WEBAPPmesaGo.Models; // <--- AQUÍ ESTABA EL ERROR (Antes decía TuProyecto)

namespace WEBAPPmesaGo.Data // <--- Y AQUÍ TAMBIÉN
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Plato> Platos { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<PlatoIngrediente> PlatoIngredientes { get; set; }
        public DbSet<Cupon> Cupones { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
    }
}