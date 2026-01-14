using BookingMvcDotNet.Services;
using TravelioDatabaseConnector.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------
// Servicios
// -----------------------------------------

builder.Services.AddControllersWithViews();

// CORS para React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 1. ACTIVAMOS LA CACH� Y LA SESI�N (Necesario para el carrito)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // El carrito dura 30 mins
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 2. BASE DE DATOS TRAVELIO (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("TravelioDb")
    ?? "Server=localhost;Database=TravelioDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

builder.Services.AddDbContext<TravelioDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. SERVICIOS DE INTEGRACI�N CON TRAVELIO
builder.Services.AddScoped<IAutosService, AutosService>();
builder.Services.AddScoped<IHotelesService, HotelesService>();
builder.Services.AddScoped<IVuelosService, VuelosService>();
builder.Services.AddScoped<IRestaurantesService, RestaurantesService>();
builder.Services.AddScoped<IPaquetesService, PaquetesService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

// HttpClient para el servicio SOA/REST (mantener compatibilidad)
builder.Services.AddHttpClient<IBookingService, BookingService>(client =>
{
    var baseUrl = builder.Configuration["BookingApiBaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    else
    {
        // Evitar InvalidOperationException cuando se usan URIs relativas en el servicio.
        client.BaseAddress = new Uri("http://localhost:5000/");
    }
});
// Nota: si tu API de backend est� en otra URL, configura 'BookingApiBaseUrl' en appsettings.json o variables de entorno.

var app = builder.Build();

// -----------------------------------------
// INICIALIZAR BASE DE DATOS
// -----------------------------------------
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TravelioDbContext>();
    try
    {
        // Esto crea la base de datos y aplica las migraciones/seed si no existe
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("? Base de datos TravelioDb inicializada correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"?? Error inicializando base de datos: {ex.Message}");
        Console.WriteLine("Aseg�rate de que SQL Server est� corriendo y accesible.");
    }
}

// -----------------------------------------
// Pipeline HTTP
// -----------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Helper para archivos est�ticos (css, js, imagenes)
app.UseStaticFiles();

app.UseRouting();

// CORS - Debe ir antes de Authorization
app.UseCors("ReactApp");

app.UseAuthorization();

// USAMOS LA SESI�N (IMPORTANTE: Debe ir antes de los controladores)
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();