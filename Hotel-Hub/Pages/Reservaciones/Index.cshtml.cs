using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;
using Hotel_Hub.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Hub.Pages.Reservaciones
{
    public class IndexModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;

        public IndexModel(ContextoBaseDatos contexto)
        {
            _contexto = contexto;
        }

        public IList<Reservacion> ListaReservaciones { get; set; } = new List<Reservacion>();

        // Propiedad que atrapa el texto que el usuario escribe en el buscador
        [BindProperty(SupportsGet = true)]
        public string? CorreoFiltro { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(CorreoFiltro))
            {
                // Solo busca en la BD si el usuario ingresó un correo
                ListaReservaciones = await _contexto.Reservaciones
                    .Include(r => r.Habitacion)
                    .Where(r => r.CorreoUsuario == CorreoFiltro)
                    .OrderByDescending(r => r.FechaEntrada)
                    .ToListAsync();
            }
            else
            {
                // Si no hay correo, la lista se queda vacía
                ListaReservaciones = new List<Reservacion>();
            }
        }
    }
}