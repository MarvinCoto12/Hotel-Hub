using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;
using Hotel_Hub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Hub.Pages.Admin
{
    public class MensajesModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;

        public MensajesModel(ContextoBaseDatos contexto)
        {
            _contexto = contexto;
        }

        public IList<MensajeSoporte> ListaMensajes { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Listar primero los mensajes más recientes
            ListaMensajes = await _contexto.MensajesSoporte
                .OrderByDescending(m => m.FechaEnvio)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCambiarEstadoAsync(int id, string nuevoEstado)
        {
            var mensaje = await _contexto.MensajesSoporte.FindAsync(id);

            if (mensaje != null)
            {
                mensaje.Estado = nuevoEstado;
                await _contexto.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}