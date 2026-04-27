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
        public DbSet<Habitacion> Habitaciones { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Habitacion>().HasData(
                new Habitacion { Id = 1, Numero = "101", Tipo = "Sencilla", PrecioPorNoche = 45.00m },
                new Habitacion { Id = 2, Numero = "102", Tipo = "Sencilla", PrecioPorNoche = 45.00m },
                new Habitacion { Id = 3, Numero = "201", Tipo = "Doble", PrecioPorNoche = 80.00m },
                new Habitacion { Id = 4, Numero = "202", Tipo = "Doble", PrecioPorNoche = 80.00m },
                new Habitacion { Id = 5, Numero = "301", Tipo = "Suite", PrecioPorNoche = 150.00m }
            );
        }
    }
}