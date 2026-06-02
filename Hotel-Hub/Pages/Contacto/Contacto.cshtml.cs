using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;
using Hotel_Hub.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<JsonResult> OnGetBuscarReservasAsync(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return new JsonResult(new object[] { });

            var reservas = await _contexto.Reservaciones
                .Where(r => r.CorreoUsuario == correo)
                .OrderByDescending(r => r.FechaEntrada)
                .Select(r => new {
                    id = r.Id,
                    texto = $"Reserva FE-2026-{r.Id} (Entrada: {r.FechaEntrada:dd/MM/yyyy})"
                })
                .ToListAsync();

            return new JsonResult(reservas);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            NuevoMensaje.FechaEnvio = DateTime.UtcNow;
            NuevoMensaje.Estado = "Pendiente";

            if (NuevoMensaje.ReservacionId <= 0)
            {
                NuevoMensaje.ReservacionId = null;
            }

            _contexto.MensajesSoporte.Add(NuevoMensaje);
            await _contexto.SaveChangesAsync();

            MensajeExito = "¡Muchas gracias! Tu mensaje ha sido recibido. Nuestro equipo de soporte te responderá pronto.";

            return RedirectToPage();
        }
    }
}