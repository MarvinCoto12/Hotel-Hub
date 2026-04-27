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
            await CargarHabitaciones();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarHabitaciones();
                return Page();
            }

            DateTime fEntrada = Reservacion.FechaEntrada!.Value;
            DateTime fSalida = Reservacion.FechaSalida!.Value;

            if (fSalida < fEntrada)
            {
                ModelState.AddModelError("Reservacion.FechaSalida", "La salida no puede ser antes de la entrada.");
                await CargarHabitaciones();
                return Page();
            }

            // Validación mejorada de disponibilidad
            bool estaOcupada = await _contexto.Reservaciones.AnyAsync(r =>
                r.HabitacionId == Reservacion.HabitacionId &&
                (
                    (fEntrada >= r.FechaEntrada && fEntrada < r.FechaSalida) || // Nueva entrada choca con una existente
                    (fSalida > r.FechaEntrada && fSalida <= r.FechaSalida) ||  // Nueva salida choca con una existente
                    (fEntrada <= r.FechaEntrada && fSalida >= r.FechaSalida)    // Nueva reserva envuelve a una existente
                )
            );

            if (estaOcupada)
            {
                // Mensaje más claro para el usuario
                ModelState.AddModelError("Reservacion.HabitacionId", "Esta habitación ya tiene una reserva confirmada para las fechas seleccionadas.");
                await CargarHabitaciones();
                return Page();
            }

            var hab = await _contexto.Habitaciones.FindAsync(Reservacion.HabitacionId);
            if (hab != null)
            {
                int noches = (fSalida - fEntrada).Days;
                Reservacion.CostoTotal = (noches <= 0 ? 1 : noches) * hab.PrecioPorNoche;
            }

            _contexto.Reservaciones.Add(Reservacion);
            await _contexto.SaveChangesAsync();

            return RedirectToPage("./Index", new { CorreoFiltro = Reservacion.CorreoUsuario });
        }

        private async Task CargarHabitaciones()
        {
            var lista = await _contexto.Habitaciones
                .Select(h => new { h.Id, Texto = $"{h.Numero} ({h.Tipo}) - ${h.PrecioPorNoche}/noche" })
                .ToListAsync();
            OpcionesHabitaciones = new SelectList(lista, "Id", "Texto");
        }
    }
}