using Microsoft.EntityFrameworkCore;
using Hotel_Hub.Data;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddRazorPages();

// 1. CORRECCIÓN: El nombre coincide exactamente con appsettings.json
builder.Services.AddDbContext<ContextoBaseDatos>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ConexionPorDefecto")));

// 2. CONFIGURAR SERVICIO DE SESIÓN
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ====================================================================
// 3. ¡EL SALVAVIDAS!: Crear la base de datos y tablas automáticamente
// ====================================================================
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ContextoBaseDatos>();
    // Esto crea el archivo .db y todas sus tablas si no existen,
    // ahorrándote tener que usar comandos en la terminal.
    contexto.Database.EnsureCreated();
}
// ====================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. ACTIVAR EL MIDDLEWARE DE SESIÓN
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();