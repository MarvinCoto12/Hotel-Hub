using System.ComponentModel.DataAnnotations;

namespace Hotel_Hub.Models
{
    public class Reservacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del huésped es obligatorio")]
        [Display(Name = "Nombre del Huésped")]
        public string NombreHuesped { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de entrada es obligatoria")]
        [Display(Name = "Fecha de Entrada")]
        [DataType(DataType.Date)]
        public DateTime FechaEntrada { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de salida es obligatoria")]
        [Display(Name = "Fecha de Salida")]
        [DataType(DataType.Date)]
        public DateTime FechaSalida { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Debe seleccionar una habitación")]
        [Display(Name = "Habitación")]
        public string NumeroHabitacion { get; set; } = string.Empty;
    }
}