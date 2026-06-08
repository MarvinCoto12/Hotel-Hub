using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Hub.Models
{
    public class MensajeSoporte
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [StringLength(100)]
        public string Asunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cuerpo del mensaje no puede estar vacío.")]
        [StringLength(1000)]
        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

        [Required]
        public string Estado { get; set; } = "Pendiente";

        [Display(Name = "Elije tu reservación vinculada a tu correo (si tienes una)")]
        public int? ReservacionId { get; set; }

        [ForeignKey("ReservacionId")]
        public Reservacion? Reservacion { get; set; }

        public string? RespuestaAdmin { get; set; }
    }
}