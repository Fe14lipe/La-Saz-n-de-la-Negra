using System.ComponentModel.DataAnnotations.Schema;

namespace WEBAPPmesaGo.Models
{
    public class PlatoIngrediente
    {
        public int Id { get; set; }

        public int PlatoId { get; set; }
        [ForeignKey("PlatoId")]
        public Plato? Plato { get; set; }

        public int IngredienteId { get; set; }
        [ForeignKey("IngredienteId")]
        public Ingrediente? Ingrediente { get; set; }

        public decimal CantidadRequerida { get; set; } // Cuánto de este ingrediente usa el plato
    }
}
