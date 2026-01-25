using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBAPPmesaGo.Models
{
    public class Plato
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public string? Preparacion { get; set; } // Receta textual

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public string Categoria { get; set; }

        public string? ImagenUrl { get; set; }

        public bool Disponible { get; set; } = true;

        public ICollection<PlatoIngrediente> Ingredientes { get; set; } = new List<PlatoIngrediente>();

    }
}