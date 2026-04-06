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

        public List<SelectListItem> OpcionesHabitaciones { get; set; } = new()
        {
            new SelectListItem { Value = "101", Text = "Habitación 101 (Sencilla)" },
            new SelectListItem { Value = "102", Text = "Habitación 102 (Sencilla)" },
            new SelectListItem { Value = "201", Text = "Habitación 201 (Doble)" },
            new SelectListItem { Value = "202", Text = "Habitación 202 (Doble)" },
            new SelectListItem { Value = "301", Text = "Suite 301 (Presidencial)" }
        };

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            bool ocupada = await _contexto.Reservaciones.AnyAsync(r =>
                r.NumeroHabitacion == Reservacion.NumeroHabitacion &&
                ((Reservacion.FechaEntrada >= r.FechaEntrada && Reservacion.FechaEntrada < r.FechaSalida) ||
                 (Reservacion.FechaSalida > r.FechaEntrada && Reservacion.FechaSalida <= r.FechaSalida) ||
                 (Reservacion.FechaEntrada <= r.FechaEntrada && Reservacion.FechaSalida >= r.FechaSalida))
            );

            if (ocupada)
            {
                ModelState.AddModelError(string.Empty, $"Lo sentimos, la habitación {Reservacion.NumeroHabitacion} ya está reservada para las fechas seleccionadas.");
                return Page();
            }

            if (Reservacion.FechaSalida <= Reservacion.FechaEntrada)
            {
                ModelState.AddModelError(string.Empty, "La fecha de salida debe ser posterior a la fecha de entrada.");
                return Page();
            }

            _contexto.Reservaciones.Add(Reservacion);
            await _contexto.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}