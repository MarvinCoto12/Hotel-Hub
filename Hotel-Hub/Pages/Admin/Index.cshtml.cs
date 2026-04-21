using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;
using Hotel_Hub.Models;

namespace Hotel_Hub.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;
        public IndexModel(ContextoBaseDatos contexto) => _contexto = contexto;

        public IList<Reservacion> TodasLasReservaciones { get; set; } = new List<Reservacion>();

        [BindProperty]
        public string PasswordIngresado { get; set; } = string.Empty;
        public bool EsAdmin { get; set; } = false;

        public async Task OnGetAsync()
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
            {
                EsAdmin = true;
                TodasLasReservaciones = await _contexto.Reservaciones
                    .Include(r => r.Habitacion)
                    .OrderByDescending(r => r.FechaEntrada)
                    .ToListAsync();
            }
        }

        public IActionResult OnPostLogin()
        {
            if (PasswordIngresado == "admin123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToPage();
            }
            ModelState.AddModelError("", "Contraseña incorrecta");
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var res = await _contexto.Reservaciones.FindAsync(id);
            if (res != null)
            {
                _contexto.Reservaciones.Remove(res);
                await _contexto.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}