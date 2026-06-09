using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;
using Hotel_Hub.Models;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
        public IList<Reservacion> HistorialReservaciones { get; set; } = new List<Reservacion>();


        [BindProperty(SupportsGet = true)]
        public string? CorreoFiltro { get; set; }

        // Propiedad para capturar si el admin presionó el botón de filtrar (ej. ver solo Check-In)
        [BindProperty(SupportsGet = true)]
        public bool FiltrarCheckIn { get; set; } = false;

        public async Task OnGetAsync()
        {
            // Base de la consulta con relación a la habitación
            var query = _contexto.Reservaciones
                .Include(r => r.Habitacion)
                .AsQueryable();

            // Si se filtra por correo del usuario, aplicarlo
            if (!string.IsNullOrEmpty(CorreoFiltro))
            {
                query = query.Where(r => r.CorreoUsuario == CorreoFiltro);
            }

            // Separación de datos por estado:
            // - Lista principal: Activa + CheckIn (o sólo CheckIn si FiltrarCheckIn == true)
            // - Historial: CheckOut
            if (FiltrarCheckIn)
            {
                ListaReservaciones = await query
                    .Where(r => r.Estado == "CheckIn")
                    .OrderByDescending(r => r.FechaEntrada)
                    .ToListAsync();

                HistorialReservaciones = await query
                    .Where(r => r.Estado == "CheckOut")
                    .OrderByDescending(r => r.FechaEntrada)
                    .ToListAsync();
            }
            else
            {
                ListaReservaciones = await query
                    .Where(r => r.Estado == "Activa" || r.Estado == "CheckIn")
                    .OrderByDescending(r => r.FechaEntrada)
                    .ToListAsync();

                HistorialReservaciones = await query
                    .Where(r => r.Estado == "CheckOut")
                    .OrderByDescending(r => r.FechaEntrada)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnGetDescargarFacturaAsync(int id)
        {
            var reserva = await _contexto.Reservaciones
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text("HOTEL HUB - FACTURA ELECTRÓNICA")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken3);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text($"Factura de Estadía N°: FE-2026-{reserva.Id}").Bold();
                        column.Item().Text($"Cliente / Huésped: {reserva.NombreHuesped}");
                        column.Item().Text($"Correo Registrado: {reserva.CorreoUsuario}");
                        column.Item().Text($"Período: Desde {reserva.FechaEntrada?.ToString("dd/MM/yyyy")} hasta {reserva.FechaSalida?.ToString("dd/MM/yyyy")}");

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        if (reserva.Habitacion != null)
                        {
                            column.Item().Text($"Habitación Asignada: N° {reserva.Habitacion.Numero}");
                            column.Item().Text($"Tipo de Habitación: {reserva.Habitacion.Tipo}");
                            column.Item().Text($"Precio Unitario por Noche: ${reserva.Habitacion.PrecioPorNoche}");
                        }

                        column.Item().PaddingTop(25).AlignRight().Text($"TOTAL NETO CANCELADO: ${reserva.CostoTotal.ToString("0.00")}")
                            .Bold().FontSize(14).FontColor(Colors.Green.Darken3);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x => {
                            x.Span("Gracias por su preferencia en Hotel Hub. Visítenos pronto. ").Italic();
                            x.CurrentPageNumber();
                        });
                });
            }).GeneratePdf();

            string nombreArchivo = $"Factura_HotelHub_{reserva.NombreHuesped.Replace(" ", "_")}.pdf";
            return File(pdfBytes, "application/pdf", nombreArchivo);
        }

        // Acción POST: marcar Check-In
        public async Task<IActionResult> OnPostMarcarCheckInAsync(int id)
        {
            var reserva = await _contexto.Reservaciones.FindAsync(id);
            if (reserva == null) return NotFound();

            reserva.Estado = "CheckIn";
            _contexto.Reservaciones.Update(reserva);
            await _contexto.SaveChangesAsync();

            return RedirectToPage();
        }

        // Acción POST: marcar Check-Out
        public async Task<IActionResult> OnPostMarcarCheckOutAsync(int id)
        {
            var reserva = await _contexto.Reservaciones.FindAsync(id);
            if (reserva == null) return NotFound();

            reserva.Estado = "CheckOut";
            _contexto.Reservaciones.Update(reserva);
            await _contexto.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}