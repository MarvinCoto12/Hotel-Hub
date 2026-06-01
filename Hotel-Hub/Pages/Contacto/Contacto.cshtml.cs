using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hotel_Hub.Data;
using Hotel_Hub.Models;
using System;
using System.Threading.Tasks;

// 1. EL NAMESPACE AHORA INCLUYE ".Contacto"
namespace Hotel_Hub.Pages.Contacto
{
    public class ContactoModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;

        public ContactoModel(ContextoBaseDatos contexto)
        {
            _contexto = contexto;
        }

        [BindProperty]
        public MensajeSoporte NuevoMensaje { get; set; } = default!;

        [TempData]
        public string? MensajeExito { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Configurar metadatos antes del insert
            NuevoMensaje.FechaEnvio = DateTime.UtcNow;
            NuevoMensaje.Estado = "Pendiente";

            _contexto.MensajesSoporte.Add(NuevoMensaje);
            await _contexto.SaveChangesAsync();

            MensajeExito = "¡Muchas gracias! Tu mensaje ha sido recibido. Nuestro equipo de soporte te responderá pronto.";

            // 2. REDIRECCIÓN SEGURA A LA MISMA PÁGINA
            return RedirectToPage();
        }
    }
}