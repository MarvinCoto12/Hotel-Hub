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

        // CORREGIDO: Cambiado a OpcionesHabitaciones para que coincida perfectamente con tu vista .cshtml
        public SelectList OpcionesHabitaciones { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Cargamos las habitaciones desde el servidor de PostgreSQL
            var habitaciones = await _contexto.Habitaciones.ToListAsync();

            // Usamos 'Id' como valor y 'DescripcionCompleta' para mostrar el número, tipo y precio en el select
            OpcionesHabitaciones = new SelectList(habitaciones, "Id", "DescripcionCompleta");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Si el formulario falla, recargamos la lista con el nombre correcto para evitar errores en la vista
                var habitaciones = await _contexto.Habitaciones.ToListAsync();
                OpcionesHabitaciones = new SelectList(habitaciones, "Id", "DescripcionCompleta");
                return Page();
            }

            // =======================================================================
            // 🔥 SOLUCIÓN POSTGRESQL: Conversión de DateTime a UTC
            // =======================================================================
            if (Reservacion.FechaEntrada.HasValue)
            {
                Reservacion.FechaEntrada = DateTime.SpecifyKind(Reservacion.FechaEntrada.Value, DateTimeKind.Utc);
            }

            if (Reservacion.FechaSalida.HasValue)
            {
                Reservacion.FechaSalida = DateTime.SpecifyKind(Reservacion.FechaSalida.Value, DateTimeKind.Utc);
            }
            // =======================================================================

            // LÓGICA: Validación y cálculo automático del CostoTotal en el Backend
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
                    // Validación por si el usuario pone una fecha de salida menor o igual a la de entrada
                    ModelState.AddModelError("Reservacion.FechaSalida", "La fecha de salida debe ser posterior a la fecha de entrada.");
                    var habitaciones = await _contexto.Habitaciones.ToListAsync();
                    OpcionesHabitaciones = new SelectList(habitaciones, "Id", "DescripcionCompleta");
                    return Page();
                }
            }

            // Guardado final en el servidor remoto de tu profesor
            _contexto.Reservaciones.Add(Reservacion);
            await _contexto.SaveChangesAsync();

            // Redirección al index protegido
            return RedirectToPage("./Index");
        }
    }
}