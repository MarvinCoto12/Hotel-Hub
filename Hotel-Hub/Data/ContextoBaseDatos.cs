using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Models;

namespace Hotel_Hub.Data
{
    public class ContextoBaseDatos : DbContext
    {
        public ContextoBaseDatos(DbContextOptions<ContextoBaseDatos> opciones) : base(opciones)
        {
        }

        public DbSet<Reservacion> Reservaciones { get; set; }
    }
}
