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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Hub.Pages.Admin
{
    public class MensajesModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;
        private readonly IConfiguration _config;

        public MensajesModel(ContextoBaseDatos contexto, IConfiguration config)
        {
            _contexto = contexto;
            _config = config;
        }

        public IList<MensajeSoporte> ListaMensajes { get; set; } = default!;

        public async Task OnGetAsync()
        {
            ListaMensajes = await _contexto.MensajesSoporte
                .Include(m => m.Reservacion)
                .OrderByDescending(m => m.FechaEnvio)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostEnviarRespuestaAsync(int id, string respuestaTexto)
        {
            var ticket = await _contexto.MensajesSoporte
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null)
            {
                TempData["Error"] = "No se encontró el mensaje.";
                return RedirectToPage();
            }

            var smtp = _config.GetSection("SmtpSettings");

            try
            {
                var correo = new MimeMessage();
                correo.From.Add(MailboxAddress.Parse(smtp["SenderEmail"]!));
                correo.To.Add(MailboxAddress.Parse(ticket.Correo));
                correo.Subject = $"Respuesta a tu reporte: {ticket.Asunto}";

                correo.Body = new TextPart("html")
                {
                    Text = $@"
                    <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto'>
                        <h2 style='color:#198754'>Respuesta a tu reporte de soporte</h2>
                        <p>Hola <strong>{ticket.Nombre}</strong>,</p>
                        <p>Nuestro equipo ha revisado tu reporte <em>'{ticket.Asunto}'</em> y te compartimos la siguiente solución:</p>
                        <div style='border-left:4px solid #0d6efd; padding:12px 16px; background:#f8f9fa; border-radius:4px; margin:16px 0'>
                            <p style='white-space: pre-wrap;'>{respuestaTexto}</p>
                        </div>
                        <p>¡Gracias por elegir Hotel Hub! Esperamos poder atenderte nuevamente en el futuro.</p>
                        <hr style='border:none;border-top:1px solid #dee2e6'>
                        <small style='color:#6c757d'>Hotel Hub — Soporte al Huésped</small>
                    </div>"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtp["Server"]!, int.Parse(smtp["Port"]!)!, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtp["SenderEmail"]!, smtp["Password"]!);
                await client.SendAsync(correo);
                await client.DisconnectAsync(true);

                ticket.Estado = "Respondido";
                ticket.RespuestaAdmin = respuestaTexto;
                await _contexto.SaveChangesAsync();

                TempData["Exito"] = $"¡Respuesta enviada correctamente a {ticket.Correo}!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al enviar el correo: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCambiarEstadoAsync(int id, string nuevoEstado)
        {
            var ticket = await _contexto.MensajesSoporte.FindAsync(id);
            if (ticket != null)
            {
                ticket.Estado = nuevoEstado;
                await _contexto.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}