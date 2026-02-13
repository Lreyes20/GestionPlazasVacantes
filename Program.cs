using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Middleware;
using GestionPlazasVacantes.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURACIÓN DE SERILOG =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Iniciando aplicación Gestión de Plazas Vacantes");

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Cookie Auth (no se crean usuarios desde la app)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Account/Login";
        opt.LogoutPath = "/Account/Logout";
        opt.AccessDeniedPath = "/Account/Denied";
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opt.Cookie.SameSite = SameSiteMode.Lax;
        opt.SlidingExpiration = true;
        opt.ExpireTimeSpan = TimeSpan.FromMinutes(30); // 30 minutos de inactividad
    });

builder.Services.AddAuthorization();

// ===== OPTIMIZACIONES DE RENDIMIENTO =====
// Caché en memoria para consultas frecuentes
builder.Services.AddMemoryCache();

// Caché de respuestas HTTP
builder.Services.AddResponseCaching();

// Compresión de respuestas (GZIP)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Configurar Kestrel para manejar más conexiones concurrentes
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentConnections = 1000;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 1000;
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

// Rate limiting de login (mitiga fuerza bruta, sin lockout)
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("login", options =>
    {
        options.PermitLimit = 10; // 10 intentos por minuto por IP
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
    }));

// ===== REGISTRO DE REPOSITORIOS =====
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPlazaVacanteRepository, PlazaVacanteRepository>();
builder.Services.AddScoped<IPostulanteRepository, PostulanteRepository>();

// ===== HEALTH CHECKS =====
builder.Services.AddHealthChecks();

builder.Services.AddControllersWithViews();


QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("http://localhost:5196/");
});


var app = builder.Build();

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("CONNECTION STRING = " + (cs ?? "NULL"));


// Inicializar datos de prueba
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    GestionPlazasVacantes.Services.DbInitializer.Initialize(context);
}

// ===== MIDDLEWARE DE MANEJO DE EXCEPCIONES =====
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseResponseCompression(); // Optimización: Compresión GZIP
app.UseStaticFiles();

// Cabeceras seguras + CSP para Bootstrap CDN
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "font-src 'self' https://cdn.jsdelivr.net data:; " +
        "img-src 'self' data:; " +
        "frame-ancestors 'none'; base-uri 'self';";
    await next();
});


// Sirve archivos desde wwwroot


app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ===== HEALTH CHECKS ENDPOINT =====
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
