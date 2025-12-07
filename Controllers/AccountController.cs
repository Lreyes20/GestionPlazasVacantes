using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using GestionPlazasVacantes.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestionPlazasVacantes.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            // 🔹 Búsqueda directa por usuario y contraseña
            var user = await _db.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == vm.Username
                                          && u.Password == vm.Password
                                          && u.Activo);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
                return View(vm);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.GivenName, user.FullName),
        new Claim(ClaimTypes.Role, user.Rol.ToString())
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = vm.RememberMe, AllowRefresh = true });

            // 🔹 Registrar último acceso
            var tracked = await _db.Usuarios.FirstAsync(u => u.Id == user.Id);
            tracked.UltimoAccesoUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Denied() => View();
    }
}
