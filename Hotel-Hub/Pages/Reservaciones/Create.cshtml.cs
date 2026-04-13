using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;
using Hotel_Hub.Models;

namespace Hotel_Hub.Pages.Reservaciones
{
    public class CreateModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;
        public CreateModel(ContextoBaseDatos contexto) => _contexto = contexto;

        [BindProperty]
        public Reservacion Reservacion { get; set; } = new Reservacion();

        public SelectList OpcionesHabitaciones { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            OpcionesHabitaciones = new SelectList(new List<Habitacion>(), "Id", "Numero");
            return Page();
        }

        public async Task<JsonResult> OnGetVerificarDisponibilidad(string entrada, string salida)
        {
            if (!DateTime.TryParse(entrada, out DateTime fEntrada) || !DateTime.TryParse(salida, out DateTime fSalida))
                return new JsonResult(new List<object>());

            var habitaciones = await _contexto.Habitaciones.ToListAsync();

            var ocupadas = await _contexto.Reservaciones
                .Where(r => fEntrada < r.FechaSalida && fSalida > r.FechaEntrada)
                .ToListAsync();

            var salenMismoDia = await _contexto.Reservaciones
                .Where(r => r.FechaSalida == fEntrada)
                .ToListAsync();

            var resultado = habitaciones.Select(h => {
                var rChoque = ocupadas.FirstOrDefault(res => res.HabitacionId == h.Id);
                var rLimpieza = salenMismoDia.FirstOrDefault(res => res.HabitacionId == h.Id);

                bool estaOcupada = rChoque != null;
                string msg = "";

                if (estaOcupada)
                {
                    // Si está ocupada por otro, le decimos cuándo podrá entrar y a qué hora
                    msg = $"[OCUPADA - Se libera el {rChoque?.FechaSalida?.ToShortDateString()} a las 3:00 PM]";
                }
                else if (rLimpieza != null)
                {
                    // Si el huésped anterior sale ese mismo día, avisamos que debe esperar a que limpien
                    msg = "[Libre a partir de las 3:00 PM (Motivos de limpieza)]";
                }
                else
                {
                    // Si la habitación lleva días vacía, puede entrar desde temprano
                    msg = "[Totalmente Disponible]";
                }

                return new
                {
                    id = h.Id,
                    texto = $"{h.Numero} ({h.Tipo}) - ${h.PrecioPorNoche} {msg}",
                    disponible = !estaOcupada
                };
            });

            return new JsonResult(resultado);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Validación de seguridad antes de guardar
            bool ocupada = await _contexto.Reservaciones.AnyAsync(r =>
                r.HabitacionId == Reservacion.HabitacionId &&
                (Reservacion.FechaEntrada < r.FechaSalida && Reservacion.FechaSalida > r.FechaEntrada)
            );

            if (ocupada)
            {
                ModelState.AddModelError(string.Empty, "La habitación se ocupó mientras llenabas el formulario.");
                OpcionesHabitaciones = new SelectList(new List<Habitacion>(), "Id", "Numero");
                return Page();
            }

            var hab = await _contexto.Habitaciones.FindAsync(Reservacion.HabitacionId);
            if (hab != null && Reservacion.FechaEntrada.HasValue && Reservacion.FechaSalida.HasValue)
            {
                int noches = (Reservacion.FechaSalida.Value - Reservacion.FechaEntrada.Value).Days;
                Reservacion.CostoTotal = (noches <= 0 ? 1 : noches) * hab.PrecioPorNoche;
            }

            _contexto.Reservaciones.Add(Reservacion);
            await _contexto.SaveChangesAsync();
            return RedirectToPage("./Index", new { CorreoFiltro = Reservacion.CorreoUsuario });
        }
    }
}