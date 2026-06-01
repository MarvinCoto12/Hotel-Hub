using System;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Hub.Models
{
    public class MensajeSoporte
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "La dirección de correo no es válida.")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El asunto no puede exceder los 100 caracteres.")]
        public string Asunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cuerpo del mensaje no puede estar vacío.")]
        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder los 1000 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

        [Required]
        public string Estado { get; set; } = "Pendiente"; // Estados: Pendiente, Respondido, Archivado
    }
}