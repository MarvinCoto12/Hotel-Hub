using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;
using Hotel_Hub.Models;

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

        public async Task OnGetAsync()
        {
            if (_contexto.Reservaciones != null)
            {
                ListaReservaciones = await _contexto.Reservaciones.ToListAsync();
            }
        }
    }
}