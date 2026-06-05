using Hotel_Hub.Data;
using Hotel_Hub.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Hub.Pages.Contacto
{
    public class ContactoModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;
        private readonly IConfiguration _config;

        public ContactoModel(ContextoBaseDatos contexto, IConfiguration config)
        {
            _contexto = contexto;
            _config = config;
        }

        [BindProperty]
        public MensajeSoporte NuevoMensaje { get; set; } = default!;

        [TempData]
        public string? MensajeExito { get; set; }

        public void OnGet() { }

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
                return Page();

            NuevoMensaje.FechaEnvio = DateTime.UtcNow;
            NuevoMensaje.Estado = "Pendiente";

            if (NuevoMensaje.ReservacionId <= 0)
                NuevoMensaje.ReservacionId = null;

            _contexto.MensajesSoporte.Add(NuevoMensaje);
            await _contexto.SaveChangesAsync();

            try
            {
                var smtp = _config.GetSection("SmtpSettings");
                var adminEmail = _config["AdminNotificationEmail"];

                var correo = new MimeMessage();
                
                correo.From.Add(MailboxAddress.Parse(smtp["SenderEmail"] ?? string.Empty));
                correo.To.Add(MailboxAddress.Parse(adminEmail ?? string.Empty));
                correo.Subject = "Nuevo reporte de soporte recibido";

                correo.Body = new TextPart("html")
                {
                    Text = $@"
                        <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto'>
                            <h2 style='color:#0d6efd'>Nuevo reporte de soporte</h2>
                            <p>Se ha recibido un nuevo mensaje en el sistema de soporte.</p>
                            <table style='width:100%;border-collapse:collapse'>
                                <tr>
                                    <td style='padding:8px;background:#f8f9fa;font-weight:bold;width:30%'>Huésped:</td>
                                    <td style='padding:8px'>{NuevoMensaje.Nombre}</td>
                                </tr>
                                <tr>
                                    <td style='padding:8px;background:#f8f9fa;font-weight:bold'>Correo:</td>
                                    <td style='padding:8px'>{NuevoMensaje.Correo}</td>
                                </tr>
                                <tr>
                                    <td style='padding:8px;background:#f8f9fa;font-weight:bold'>Asunto:</td>
                                    <td style='padding:8px'>{NuevoMensaje.Asunto}</td>
                                </tr>
                                <tr>
                                    <td style='padding:8px;background:#f8f9fa;font-weight:bold'>Mensaje:</td>
                                    <td style='padding:8px'>{NuevoMensaje.Mensaje}</td>
                                </tr>
                            </table>
                            <br>
                            <p style='color:#6c757d;font-size:13px'>
                                Ingresa al panel de administración para responder este ticket.
                            </p>
                        </div>"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    smtp["Server"] ?? string.Empty,       
                    int.Parse(smtp["Port"] ?? "587"),
                    SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    smtp["SenderEmail"] ?? string.Empty,  
                    smtp["Password"] ?? string.Empty);
                await client.SendAsync(correo);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMTP Error Contacto] {ex.Message}");
            }

            MensajeExito = "¡Muchas gracias! Tu mensaje ha sido recibido. Nuestro equipo de soporte te responderá pronto.";
            return RedirectToPage();
        }
    }
}