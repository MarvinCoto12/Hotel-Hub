using System.ComponentModel.DataAnnotations;

namespace Hotel_Hub.Models
{
    public class Reservacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre no puede estar vacío.")]
        [StringLength(40, ErrorMessage = "El nombre no puede exceder los 40 caracteres.")]
        [RegularExpression(@"^[a-zA-Z\sáéíóúÁÉÍÓÚñÑ]+$", ErrorMessage = "Solo se permiten letras.")]
        [Display(Name = "Nombre del Huésped")]
        public string NombreHuesped { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo no puede estar vacío.")]
        [EmailAddress(ErrorMessage = "La dirección de correo no es válida.")]
        [Display(Name = "Correo Electrónico")]
        public string CorreoUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
        [Display(Name = "Fecha de Entrada")]
        [DataType(DataType.Date)]
        public DateTime? FechaEntrada { get; set; }

        [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
        [Display(Name = "Fecha de Salida")]
        [DataType(DataType.Date)]
        public DateTime? FechaSalida { get; set; }

        [Required(ErrorMessage = "Debe elegir una habitación.")]
        [Display(Name = "Habitación")]
        public int? HabitacionId { get; set; }

        public Habitacion? Habitacion { get; set; }

        public decimal CostoTotal { get; set; }
    }
}