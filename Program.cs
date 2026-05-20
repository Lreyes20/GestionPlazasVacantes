//using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Handlers;
using GestionPlazasVacantes.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
//using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// EF Core
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie Auth
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

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Rate limiting
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("login", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
    }));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

builder.Services.AddControllersWithViews();

// Session + ContextAccessor
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// Si realmente usas handlers JWT en otras pantallas, déjalos:
builder.Services.AddTransient<GestionPlazasVacantes.Services.JwtDelegatingHandler>();
builder.Services.AddTransient<JwtAuthorizationHandler>();

// Cliente sin auth, por si lo ocupas
builder.Services.AddHttpClient("ApiNoAuth", client =>
{
    client.BaseAddress = new Uri("http://localhost:5132/");
});

// Cliente principal API
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("http://localhost:5132/");
})
.AddHttpMessageHandler<GestionPlazasVacantes.Services.JwtDelegatingHandler>();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var app = builder.Build();

//app.Use(async (context, next) =>
//{
//    context.Response.Headers["Content-Security-Policy"] =
//        "default-src 'self'; " +
//        "img-src 'self' https://localhost:44330 data:; " +
//        "script-src 'self' 'unsafe-inline'; " +
//        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
//        "font-src 'self' https://fonts.gstatic.com;";
//    await next();
//});

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "img-src 'self' http://localhost:5132 data:; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "connect-src 'self' http://localhost:5132; " +
        "frame-ancestors 'none'; base-uri 'self';";

    await next();
});

// Inicializar datos de prueba
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<AppDbContext>();
//    GestionPlazasVacantes.Services.DbInitializer.Initialize(context);
//}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();


// Cabeceras seguras + CSP
//app.Use(async (ctx, next) =>
//{
//    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
//    ctx.Response.Headers["X-Frame-Options"] = "DENY";
//    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
//    ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
//    ctx.Response.Headers["Content-Security-Policy"] =
//        "default-src 'self'; " +
//        "script-src 'self' https://cdn.jsdelivr.net; " +
//        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
//        "font-src 'self' https://cdn.jsdelivr.net data:; " +
//        "img-src 'self' data:; " +
//        "frame-ancestors 'none'; base-uri 'self';";
//    await next();
//});

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();