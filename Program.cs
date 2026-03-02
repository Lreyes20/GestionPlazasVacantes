using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Handlers;
using GestionPlazasVacantes.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

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
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// Rate limiting de login (mitiga fuerza bruta, sin lockout)
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("login", options =>
    {
        options.PermitLimit = 10; // 10 intentos por minuto por IP
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
    }));

// TipoConcurso como STRING
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

builder.Services.AddControllersWithViews();

// <summary>
// Para que el HttpClient pueda acceder al token JWT almacenado en sesión con Handler automático
// </summary>
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtDelegatingHandler>();


builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:44330/");
})
.AddHttpMessageHandler<JwtDelegatingHandler>();



QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:44330/");
});
//builder.Services.AddHttpClient("Api", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:44328/");
//})
//.AddHttpMessageHandler(() => new JwtHandler());

/// <sumary>
/// Blinda las URL para usuarios logueados
/// </summary>
builder.Services.AddAuthorization(options =>
{
    // TODO requiere login por defecto
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// <summary>
// Para que el HttpClient pueda acceder al token JWT almacenado en sesión
// </summary>
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<GestionPlazasVacantes.Services.JwtDelegatingHandler>();

// Cliente SIN handler (para refresh/logout)
builder.Services.AddHttpClient("ApiNoAuth", client =>
{
    client.BaseAddress = new Uri("https://localhost:44330/"); // tu API
});

// Cliente CON handler (para el resto del sistema)
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:44330/");
})
.AddHttpMessageHandler<GestionPlazasVacantes.Services.JwtDelegatingHandler>();

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("Api", (sp, client) =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var token = httpContext?.Session.GetString("JWToken");

    if (!string.IsNullOrEmpty(token))
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    client.BaseAddress = new Uri("https://localhost:44330/");
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<JwtAuthorizationHandler>();

// <summary>
// Configura el HttpClient para que use el JwtAuthorizationHandler, que adjunta el token JWT a cada request.
// <summary>
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:44330/");
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
