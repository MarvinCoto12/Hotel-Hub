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

        [BindProperty(SupportsGet = true)]
        public string? CorreoFiltro { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(CorreoFiltro))
            {
                ListaReservaciones = await _contexto.Reservaciones
                    .Include(r => r.Habitacion)
                    .Where(r => r.CorreoUsuario == CorreoFiltro)
                    .OrderByDescending(r => r.FechaEntrada)
                    .ToListAsync();
            }
            else
            {
                ListaReservaciones = new List<Reservacion>();
            }
        }
    }
}