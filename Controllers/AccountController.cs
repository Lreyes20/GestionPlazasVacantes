using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestionPlazasVacantes.Controllers
{
    /// <summary>
    /// Controlador encargado de la autenticación de usuarios en el sistema MVC.
    ///
    /// Este controlador NO valida credenciales localmente ni consulta la BD.
    /// Toda la autenticación se delega al API (JWT + RefreshToken).
    ///
    /// Flujo:
    /// 1) MVC envía credenciales al API (cliente "ApiNoAuth")
    /// 2) API devuelve { token, refreshToken, datos de usuario }
    /// 3) MVC guarda tokens en Session y crea cookie auth (Claims) para navegación
    /// 4) HttpClient "Api" (con JwtDelegatingHandler) adjunta JWT y refresca si expira
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Cliente HTTP SIN handler (no adjunta JWT).
        /// Se usa para Login/Refresh/Logout hacia el API para evitar loops.
        /// </summary>
        private readonly HttpClient _apiNoAuth;

        public AccountController(IHttpClientFactory factory)
        {
            _apiNoAuth = factory.CreateClient("ApiNoAuth");
        }

        /// <summary>
        /// Muestra la vista de login.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginVM());
        }

        /// <summary>
        /// Procesa el inicio de sesión.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Login SIEMPRE con ApiNoAuth (sin Bearer) para evitar efectos colaterales.
            var response = await _apiNoAuth.PostAsJsonAsync(
                "api/auth/login",
                new { vm.Username, vm.Password });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
                return View(vm);
            }

            var user = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (user == null || string.IsNullOrWhiteSpace(user.Token) || string.IsNullOrWhiteSpace(user.RefreshToken))
            {
                ModelState.AddModelError(string.Empty, "Respuesta inválida del servicio de autenticación.");
                return View(vm);
            }

            // 🔐 Guardar tokens en Session (lo que usan los handlers para adjuntar/refresh)
            HttpContext.Session.SetString("JWToken", user.Token);
            HttpContext.Session.SetString("RefreshToken", user.RefreshToken);

            // Claims para cookie auth (navegación/autorizar vistas/controladores en MVC)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.FullName),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = vm.RememberMe,
                    AllowRefresh = true
                });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Cierra sesión (cookie auth) y elimina tokens de Session.
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            // Limpia tokens usados por el HttpClient handler
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Remove("RefreshToken");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Vista cuando el usuario no tiene permisos para un recurso.
        /// </summary>
        public IActionResult Denied() => View();
    }
}