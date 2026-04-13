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
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Precio por Noche")]
        public decimal PrecioPorNoche { get; set; }

        public ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();

        public string DescripcionCompleta => $"{Numero} - {Tipo} (${PrecioPorNoche}/noche)";
    }
}