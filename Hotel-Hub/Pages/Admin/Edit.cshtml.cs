using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;
using Hotel_Hub.Models;

namespace Hotel_Hub.Pages.Admin
{
    public class EditModel : PageModel
    {
        private readonly ContextoBaseDatos _contexto;

        public EditModel(ContextoBaseDatos contexto)
        {
            _contexto = contexto;
        }

        [BindProperty]
        public Reservacion Reservacion { get; set; } = default!;

        public SelectList ListaHabitaciones { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToPage("./Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var reservacion = await _contexto.Reservaciones
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservacion == null)
            {
                return NotFound();
            }

            Reservacion = reservacion;

            CargarListaHabitaciones(Reservacion.HabitacionId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToPage("./Index");
            }

            if (!ModelState.IsValid)
            {
                CargarListaHabitaciones();
                return Page();
            }

            bool ocupada = await _contexto.Reservaciones.AnyAsync(r =>
                r.Id != Reservacion.Id &&
                r.HabitacionId == Reservacion.HabitacionId &&
                ((Reservacion.FechaEntrada >= r.FechaEntrada && Reservacion.FechaEntrada < r.FechaSalida) ||
                 (Reservacion.FechaSalida > r.FechaEntrada && Reservacion.FechaSalida <= r.FechaSalida) ||
                 (r.FechaEntrada >= Reservacion.FechaEntrada && r.FechaEntrada < Reservacion.FechaSalida)));

            if (ocupada)
            {
                ModelState.AddModelError(string.Empty, "La habitación seleccionada ya está ocupada en ese rango de fechas.");
                CargarListaHabitaciones();
                return Page();
            }

            var habitacion = await _contexto.Habitaciones.FindAsync(Reservacion.HabitacionId);
            if (habitacion != null && Reservacion.FechaEntrada.HasValue && Reservacion.FechaSalida.HasValue)
            {
                var diferenciaDias = (Reservacion.FechaSalida.Value - Reservacion.FechaEntrada.Value).Days;

                int diasEstancia = diferenciaDias > 0 ? diferenciaDias : 1;

                Reservacion.CostoTotal = diasEstancia * habitacion.PrecioPorNoche;
            }

            _contexto.Attach(Reservacion).State = EntityState.Modified;

            try
            {
                await _contexto.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservacionExists(Reservacion.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private void CargarListaHabitaciones(int? seleccionado = null)
        {
            var habitacionesQuery = _contexto.Habitaciones
                .Select(h => new
                {
                    Id = h.Id,
                    Detalle = $"{h.Numero} - {h.Tipo} (${h.PrecioPorNoche}/noche)"
                })
                .ToList();

            ListaHabitaciones = new SelectList(habitacionesQuery, "Id", "Detalle", seleccionado);
        }

        private bool ReservacionExists(int id)
        {
            return _contexto.Reservaciones.Any(e => e.Id == id);
        }
    }
}