using GestionPlazasVacantes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GestionPlazasVacantes.Controllers
{
    /// <summary>
    /// Controlador MVC para la gestión de asignaciones de plazas vacantes.
    /// 
    /// Seguridad:
    /// - Requiere autenticación
    /// - Acceso exclusivo para usuarios con rol "Jefe"
    /// </summary>
    [Authorize(Roles = "Jefe")]
    public class AsignacionesController : Controller
    {
        private readonly HttpClient _api;

        /// <summary>
        /// Constructor que inicializa el HttpClient para consumo del API.
        /// </summary>
        public AsignacionesController(IHttpClientFactory factory)
        {
            _api = factory.CreateClient("Api");
        }

        /// <summary>
        /// Vista principal para gestionar asignaciones de plazas a colaboradores.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Obtener plazas activas con asignaciones
            var plazasResponse = await _api.GetAsync("api/asignaciones/plazas-activas");

            if (!plazasResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "⚠️ No se pudieron cargar las plazas.";
                return RedirectToAction("Index", "Dashboard");
            }

            //var plazas = await plazasResponse
            //    .Content
            //    .ReadFromJsonAsync<List<PlazaAsignacionDto>>();
            var plazas = await plazasResponse.Content.ReadFromJsonAsync<List<PlazaAsignacionDto>>()
            ?? new List<PlazaAsignacionDto>();

            //// Obtener colaboradores disponibles
            //var colaboradoresResponse = await _api.GetAsync("api/asignaciones/colaboradores");

            //if (!colaboradoresResponse.IsSuccessStatusCode)
            //{
            //    TempData["ErrorMessage"] = "⚠️ No se pudieron cargar los colaboradores.";
            //    return RedirectToAction("Index", "Dashboard");
            //}

            //var colaboradores = await colaboradoresResponse
            //    .Content
            //    .ReadFromJsonAsync<List<UsuarioDto>>();


            //var colaboradores = await _api.GetFromJsonAsync<List<UsuarioDto>>("api/usuarios/colaboradores")
            //      ?? new List<UsuarioDto>();
            var colaboradores = await _api
                .GetFromJsonAsync<List<UsuarioDto>>("api/asignaciones/colaboradores")
                ?? new List<UsuarioDto>();


            //var colaboradores = await _api
            //      .GetFromJsonAsync<List<UsuarioDto>>("api/usuarios/colaboradores");


            ViewBag.Colaboradores = colaboradores;

            return View(plazas);
        }

        /// <summary>
        /// Asigna una plaza vacante a un colaborador.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPlaza(int plazaId, int usuarioId)
        {
            var response = await _api.PostAsJsonAsync(
                "api/asignaciones/asignar",
                new { plazaId, usuarioId });

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "⚠️ No se pudo asignar la plaza.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "✅ Plaza asignada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Remueve la asignación de una plaza vacante.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverAsignacion(int plazaId)
        {
            var response = await _api.PostAsJsonAsync(
                "api/asignaciones/remover",
                new { plazaId });

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "⚠️ No se pudo remover la asignación.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "✅ Asignación removida correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
