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

        public CreateModel(ContextoBaseDatos contexto)
        {
            _contexto = contexto;
        }

        [BindProperty]
        public Reservacion Reservacion { get; set; } = new Reservacion();

        public SelectList OpcionesHabitaciones { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarListaVacia();
            return Page();
        }

        public async Task<JsonResult> OnGetVerificarDisponibilidad(DateTime? entrada, DateTime? salida)
        {
            if (!entrada.HasValue || !salida.HasValue)
            {
                return new JsonResult(new List<object>());
            }

            var habitaciones = await _contexto.Habitaciones.ToListAsync();

            // Buscar choques de fechas
            var reservasConflictivas = await _contexto.Reservaciones
                .Where(r => entrada.Value < r.FechaSalida && salida.Value > r.FechaEntrada)
                .ToListAsync();

            var resultado = habitaciones.Select(h => {
                var reservaChoque = reservasConflictivas.FirstOrDefault(r => r.HabitacionId == h.Id);
                bool estaOcupada = reservaChoque != null;

                string status = estaOcupada
                    ? $"[OCUPADA - Libre el {reservaChoque!.FechaSalida?.ToShortDateString()}]"
                    : "[Disponible]";

                return new
                {
                    id = h.Id,
                    texto = $"Habitación {h.Numero} ({h.Tipo}) - ${h.PrecioPorNoche}/noche {status}",
                    disponible = !estaOcupada
                };
            });

            return new JsonResult(resultado);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarListaVacia();
                return Page();
            }

            bool ocupada = await _contexto.Reservaciones.AnyAsync(r =>
                r.HabitacionId == Reservacion.HabitacionId &&
                (Reservacion.FechaEntrada < r.FechaSalida && Reservacion.FechaSalida > r.FechaEntrada)
            );

            if (ocupada)
            {
                ModelState.AddModelError(string.Empty, "La habitación seleccionada ya no está disponible.");
                await CargarListaVacia();
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

            // ¡Redirigimos al Index filtrando automáticamente por el correo que acaba de usar!
            return RedirectToPage("./Index", new { CorreoFiltro = Reservacion.CorreoUsuario });
        }

        private async Task CargarListaVacia()
        {
            OpcionesHabitaciones = new SelectList(new List<Habitacion>(), "Id", "Numero");
        }
    }
}