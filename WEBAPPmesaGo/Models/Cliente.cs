using System.ComponentModel.DataAnnotations;

namespace WEBAPPmesaGo.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Correo { get; set; }

        public string Password { get; set; } = ""; // Encriptada
        public string? TokenRecuperacion { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
