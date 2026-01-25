using System.ComponentModel.DataAnnotations;

namespace WEBAPPmesaGo.Models
{
    public class Ingrediente
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public decimal Cantidad { get; set; } // Stock actual

        public string Unidad { get; set; } = "unidades"; // kg, g, lb, unidades

        public decimal StockMinimo { get; set; } // Alerta cuando baje de esto
    }
}
