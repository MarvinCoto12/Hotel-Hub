using System.ComponentModel.DataAnnotations;

namespace Hotel_Hub.Models
{
    public class Reservacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre del Huésped")]
        public string NombreHuesped { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es necesario para identificar su reserva")]
        [EmailAddress(ErrorMessage = "La dirección de correo electrónico no es válida")]
        [Display(Name = "Su Correo Electrónico")]
        public string CorreoUsuario { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de Entrada")]
        [DataType(DataType.Date)]
        public DateTime? FechaEntrada { get; set; }

        [Required]
        [Display(Name = "Fecha de Salida")]
        [DataType(DataType.Date)]
        public DateTime? FechaSalida { get; set; }

        [Required]
        [Display(Name = "Habitación")]
        public int HabitacionId { get; set; }

        public Habitacion? Habitacion { get; set; }

        public decimal CostoTotal { get; set; }
    }
}