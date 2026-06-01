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
        public Reservacion Reservacion { get; set; } = default!;
        public SelectList OpcionesHabitaciones { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var habitaciones = await _contexto.Habitaciones.ToListAsync();

            OpcionesHabitaciones = new SelectList(habitaciones, "Id", "DescripcionCompleta");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var habitaciones = await _contexto.Habitaciones.ToListAsync();
                OpcionesHabitaciones = new SelectList(habitaciones, "Id", "DescripcionCompleta");
                return Page();
            }

            if (Reservacion.FechaEntrada.HasValue)
            {
                Reservacion.FechaEntrada = DateTime.SpecifyKind(Reservacion.FechaEntrada.Value, DateTimeKind.Utc);
            }

            if (Reservacion.FechaSalida.HasValue)
            {
                Reservacion.FechaSalida = DateTime.SpecifyKind(Reservacion.FechaSalida.Value, DateTimeKind.Utc);
            }

            var habitacionAsignada = await _contexto.Habitaciones.FindAsync(Reservacion.HabitacionId);

            if (habitacionAsignada != null && Reservacion.FechaEntrada.HasValue && Reservacion.FechaSalida.HasValue)
            {
                int noches = (Reservacion.FechaSalida.Value - Reservacion.FechaEntrada.Value).Days;

                if (noches > 0)
                {
                    Reservacion.CostoTotal = noches * habitacionAsignada.PrecioPorNoche;
                }
                else
                {
                    ModelState.AddModelError("Reservacion.FechaSalida", "La fecha de salida debe ser posterior a la fecha de entrada.");
                    var habitaciones = await _contexto.Habitaciones.ToListAsync();
                    OpcionesHabitaciones = new SelectList(habitaciones, "Id", "DescripcionCompleta");
                    return Page();
                }
            }

            _contexto.Reservaciones.Add(Reservacion);
            await _contexto.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}