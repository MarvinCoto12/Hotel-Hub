using System.ComponentModel.DataAnnotations;

namespace Hotel_Hub.Models
{
    public class Habitacion
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Número")]
        public string Numero { get; set; } = string.Empty;

        [Required]
        public string Tipo { get; set; } = string.Empty; // Sencilla, Doble, Suite

        [Required]
        [Display(Name = "Precio por Noche")]
        public decimal PrecioPorNoche { get; set; }

        // Propiedad de navegación: Una habitación puede tener muchas reservaciones en el tiempo
        public ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();

        // Propiedad extra para que el ComboBox se vea bonito ("101 - Sencilla ($50.00)")
        public string DescripcionCompleta => $"{Numero} - {Tipo} (${PrecioPorNoche}/noche)";
    }
}