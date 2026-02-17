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
    /// Este controlador NO accede directamente a la base de datos.
    /// Toda la validación de credenciales se delega al API PlazasVacantesAPI.
    /// 
    /// Arquitectura:
    /// MVC → API → Base de Datos
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Cliente HTTP configurado para comunicarse con el API.
        /// Se obtiene desde IHttpClientFactory para una gestión eficiente de conexiones y configuración centralizada.
        /// </summary>
        private readonly HttpClient _api;

        /// <summary>
        /// Constructor del controlador.
        /// Inicializa el HttpClient utilizando el cliente nombrado "Api".
        /// </summary>
        public AccountController(IHttpClientFactory factory)
        {
            _api = factory.CreateClient("Api");
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
        /// Procesa el intento de inicio de sesión del usuario.
        /// 
        /// Flujo:
        /// 1. Valida el modelo recibido desde la vista.
        /// 2. Envía las credenciales al API de autenticación.
        /// 3. Si el API valida correctamente, crea una sesión basada en cookies.
        /// 4. Genera los Claims del usuario autenticado.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            // Validación del modelo enviado desde la vista
            if (!ModelState.IsValid)
                return View(vm);

            // Llamada al API para validar credenciales
            var response = await _api.PostAsJsonAsync(
                "api/auth/login",
                new { vm.Username, vm.Password });

            // Si el API devuelve error, se asume credenciales inválidas
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
                return View(vm);
            }

            // Lectura de la respuesta del API con la información del usuario autenticado
            var user = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            HttpContext.Session.SetString("JWToken", user!.Token);

            // Creación de los claims que representarán la identidad del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user!.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.FullName),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            // Construcción de la identidad basada en cookies
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            // Inicio de sesión en el contexto HTTP usando autenticación por cookies
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = vm.RememberMe,
                    AllowRefresh = true
                });

            // Redirección segura a la URL original si existe
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Redirección por defecto tras login exitoso
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Cierra la sesión del usuario autenticado.
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Vista mostrada cuando el usuario intenta acceder a un recurso no autorizado.
        /// </summary>
        public IActionResult Denied() => View();
    }
}
