using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionPlazasVacantes.Controllers
{
    public class SeguimientoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public SeguimientoController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // 📋 Vista general de plazas activas con seguimiento
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("Api");

            var username = User.Identity?.Name;

            var response = await client.GetFromJsonAsync<List<PlazaVacante>>(
                $"api/SeguimientoApi/plazas?username={username}",
                _jsonOptions);

            if (response == null)
                return Unauthorized();

            ViewBag.UsuarioActual = username;
            return View(response);
        }

        // 👀 Seguimiento por plaza
        public async Task<IActionResult> PorPlaza(int plazaId)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var data = await client.GetFromJsonAsync<SeguimientoPlazaDTO>(
                $"api/SeguimientoApi/por-plaza/{plazaId}",
                _jsonOptions);

            if (data == null)
                return NotFound();

            ViewBag.Plaza = data.Plaza;
            ViewBag.Seguimientos = data.Seguimientos;

            var postulantesActivos = data.Postulantes
                .Where(p => p.EstadoProceso != "Descartado")
                .ToList();

            return View(postulantesActivos);
            //return View(data.Postulantes);
        }

        // 🧾 Detalle individual de un postulante
        public async Task<IActionResult> Detalle(int id)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var dto = await client.GetFromJsonAsync<DetallePostulanteDTO>(
                $"api/SeguimientoApi/detalle/{id}");

            if (dto == null)
                return NotFound();

            var vm = new DetallePostulanteVM
            {
                Postulante = dto.Postulante,
                Seguimiento = dto.Seguimiento,
                Archivos = dto.Archivos ?? new List<ArchivoDto>()
            };

            ViewBag.ApiBaseUrl = _config["Api:BaseUrl"];

            return View(vm);
        }

        // 💾 Actualizar etapa
        [HttpPost]
        public async Task<IActionResult> Actualizar(int postulanteId, string etapa, bool cumple, decimal? notaTec, decimal? notaPsi, string obs)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var payload = new
            {
                PostulanteId = postulanteId,
                Etapa = etapa,
                Cumple = cumple,
                NotaTec = notaTec,
                NotaPsi = notaPsi,
                Obs = obs
            };

            await client.PostAsJsonAsync("api/SeguimientoApi/actualizar", payload);

            return RedirectToAction("Detalle", new { id = postulanteId });
        }

        // 🔹 DESCARTAR POSTULANTE
        [HttpPost]
        public async Task<IActionResult> Descartar(int postulanteId, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Error"] = "⚠️ Debe especificar un motivo para descartar al postulante.";
                return RedirectToAction("Detalle", new { id = postulanteId });
            }

            var client = _httpClientFactory.CreateClient("Api");

            var payload = new { postulanteId, motivo };

            await client.PostAsJsonAsync("api/SeguimientoApi/descartar", payload);

            TempData["Success"] = "✅ El postulante fue descartado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarPlaza(int plazaId)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var response = await client.PostAsJsonAsync("api/SeguimientoApi/finalizar", new { plazaId });

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "⚠️ No se pudo finalizar la plaza.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "✅ La plaza fue finalizada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPlaza(int plazaId)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var response = await client.PostAsJsonAsync("api/SeguimientoApi/eliminar", new { plazaId });

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "🗑️ Plaza eliminada correctamente.";
            else
                TempData["ErrorMessage"] = "⚠️ No se pudo eliminar la plaza.";

            return RedirectToAction(nameof(Index));
        }
    }
}