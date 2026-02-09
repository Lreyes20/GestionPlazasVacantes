using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestionPlazasVacantes.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _api;

        public AccountController(IHttpClientFactory factory)
        {
            _api = factory.CreateClient("Api");
        }

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
            if (!ModelState.IsValid)
                return View(vm);

            // LLAMADA AL API
            var response = await _api.PostAsJsonAsync(
                "api/auth/login",
                new { vm.Username, vm.Password });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
                return View(vm);
            }

            var user = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user!.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.FullName),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        public IActionResult Denied() => View();
    }
}