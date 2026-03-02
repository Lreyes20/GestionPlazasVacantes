using GestionPlazasVacantes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GestionPlazasVacantes.Controllers
{
    [Authorize]
    public class PlazasController : Controller
    {
        private readonly HttpClient _api;

        public PlazasController(IHttpClientFactory factory)
        {
            _api = factory.CreateClient("Api");
        }

        // GET: Plazas
        public async Task<IActionResult> Index()
        {
            var plazas = await _api
                .GetFromJsonAsync<List<PlazaDto>>("api/plazas")
                ?? new List<PlazaDto>();
            var vigentes = plazas.Where(
                p => p.FechaLimite > DateTime.Now);

            //return View(plazas);
            return View(vigentes);
        }

        // GET: Plazas/Crear
        public IActionResult Crear()
        {
            return View(new PlazaDto());
        }

        // POST: Plazas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PlazaDto plaza)
        {
            if (!ModelState.IsValid)
                return View(plaza);

            var response = await _api.PostAsJsonAsync("api/plazas", plaza);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Error al crear la plaza.";
                return View(plaza);
            }

            TempData["SuccessMessage"] = "✅ Plaza vacante creada correctamente.";
            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Plazas/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var plaza = await _api
                .GetFromJsonAsync<PlazaDto>($"api/plazas/{id}");

            if (plaza == null)
                return NotFound();

            return View(plaza);
        }

        // POST: Plazas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, PlazaDto plaza)
        {
            if (id != plaza.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(plaza);

            var response = await _api.PutAsJsonAsync($"api/plazas/{id}", plaza);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Error al actualizar la plaza.";
                return View(plaza);
            }

            TempData["SuccessMessage"] = "✏️ Plaza actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Plazas/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            var plaza = await _api
                .GetFromJsonAsync<PlazaDto>($"api/plazas/{id}");

            if (plaza == null)
                return NotFound();

            return View(plaza);
        }

        // POST: Plazas/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var response = await _api.DeleteAsync($"api/plazas/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "⚠️ No se puede eliminar la plaza porque tiene postulantes o registros asociados.";
            }
            else
            {
                TempData["SuccessMessage"] = "🗑️ Plaza eliminada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Detalle(int id)
        {
            var response = await _api.GetAsync($"api/plazas/{id}/detalle");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var data = await response.Content.ReadFromJsonAsync<PlazaDetalleDto>();

            ViewBag.Postulantes = data!.Postulantes;

            return View(data.Plaza);
        }
    }
}