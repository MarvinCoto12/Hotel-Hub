using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Hotel_Hub.Data;
using Hotel_Hub.Models;

namespace Hotel_Hub.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;
        private readonly IConfiguration _configuracion;

        public IndexModel(ContextoBaseDatos contexto, IConfiguration configuracion)
        {
            _contexto = contexto;
            _configuracion = configuracion;
        }

        public IList<Reservacion> TodasLasReservaciones { get; set; } = new List<Reservacion>();

        [BindProperty]
        public string PasswordIngresado { get; set; } = string.Empty;
        public bool EsAdmin { get; set; } = false;
        public int TotalReservasActivas { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Vista { get; set; } = "Todas";

        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        private const int TamanoPagina = 10;

        public async Task OnGetAsync()
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
            {
                EsAdmin = true;

                var query = _contexto.Reservaciones
                    .AsNoTracking()
                    .Include(r => r.Habitacion)
                    .AsQueryable();

                TotalReservasActivas = await _contexto.Reservaciones.CountAsync(r => r.Estado == "Activa" || r.Estado == "CheckIn");

                if (Vista == "CheckIn")
                {
                    query = query.Where(r => r.Estado == "CheckIn");
                }
                else if (Vista == "CheckOut")
                {
                    query = query.Where(r => r.Estado == "CheckOut");
                }
                else
                {
                    query = query.Where(r => r.Estado == "Activa" || r.Estado == "CheckIn");
                }

                int totalRegistros = await query.CountAsync();
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanoPagina);

                if (PaginaActual < 1) PaginaActual = 1;
                if (PaginaActual > TotalPaginas && TotalPaginas > 0) PaginaActual = TotalPaginas;

                TodasLasReservaciones = await query
                    .OrderByDescending(r => r.FechaEntrada)
                    .Skip((PaginaActual - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToListAsync();
            }
        }

        public IActionResult OnPostLogin()
        {
            string passwordReal = _configuracion["AdminSettings:Password"] ?? "admin123";

            if (PasswordIngresado == passwordReal)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToPage();
            }
            ModelState.AddModelError("", "Contraseña incorrecta");
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id, string vista, int paginaActual)
        {
            var res = await _contexto.Reservaciones.FindAsync(id);
            if (res != null)
            {
                _contexto.Reservaciones.Remove(res);
                await _contexto.SaveChangesAsync();
            }
            return RedirectToPage(new { Vista = vista, PaginaActual = paginaActual });
        }

        public async Task<IActionResult> OnPostMarcarCheckInAsync(int id, string vista, int paginaActual)
        {
            var reserva = await _contexto.Reservaciones.FindAsync(id);
            if (reserva != null)
            {
                reserva.Estado = "CheckIn";
                await _contexto.SaveChangesAsync();
            }
            return RedirectToPage(new { Vista = vista, PaginaActual = paginaActual });
        }

        public async Task<IActionResult> OnPostMarcarCheckOutAsync(int id, string vista, int paginaActual)
        {
            var reserva = await _contexto.Reservaciones.FindAsync(id);
            if (reserva != null)
            {
                reserva.Estado = "CheckOut";
                await _contexto.SaveChangesAsync();
            }
            return RedirectToPage(new { Vista = vista, PaginaActual = paginaActual });
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}