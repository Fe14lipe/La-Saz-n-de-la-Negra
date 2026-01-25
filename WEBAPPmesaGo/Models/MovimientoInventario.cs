using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBAPPmesaGo.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int Id { get; set; }

        public int IngredienteId { get; set; }
        
        [ForeignKey("IngredienteId")]
        public Ingrediente? Ingrediente { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Cantidad positiva (Aumentó stock) o negativa (Disminuyó stock)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cantidad { get; set; }

        // Tipo de movimiento: "Venta", "Ajuste Manual", "Compra", "Desperdicio"
        public string Tipo { get; set; } = "General";

        // Detalle extra: "Venta de Lomo Saltado #123" o "Reposición de stock"
        public string? Detalle { get; set; }
    }
}
