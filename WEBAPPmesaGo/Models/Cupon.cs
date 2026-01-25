using System.ComponentModel.DataAnnotations;

namespace WEBAPPmesaGo.Models
{
    public class Cupon
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        public string Codigo { get; set; } // Ej: CARNAVAL

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } // Ej: 50% de descuento

        [Required]
        [Range(1, 100, ErrorMessage = "El porcentaje debe estar entre 1 y 100")]
        public int Porcentaje { get; set; } // Ej: 50

        public bool EsFeriado { get; set; } // True = Color Naranja, False = Blanco

        public DateTime FechaExpiracion { get; set; } = DateTime.Now.AddDays(7);

        public bool Activo { get; set; } = true;
    }
}
