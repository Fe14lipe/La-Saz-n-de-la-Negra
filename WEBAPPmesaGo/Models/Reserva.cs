using System.ComponentModel.DataAnnotations;

namespace WEBAPPmesaGo.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreCliente { get; set; }

        [Required]
        public string Correo { get; set; }

        public string? Telefono { get; set; }

        [Required]
        public DateTime FechaHora { get; set; } // Guarda fecha y hora

        public int NumeroPersonas { get; set; }

        // Estados: "Pendiente", "Confirmada", "Cancelada"
        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public string? CuponUsado { get; set; } // <--- AGREGAR ESTO

    }

}