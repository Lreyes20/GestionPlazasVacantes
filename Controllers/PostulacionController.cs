using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GestionPlazasVacantes.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionPlazasVacantes.Controllers
{
    public class PostulacionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly string[] ExtImgs = new[] { ".jpg", ".jpeg", ".png" };
        private static readonly string[] ExtPdf = new[] { ".pdf" };
        private static readonly string[] ExtWord = new[] { ".doc", ".docx" };

        public PostulacionController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        // 🏢 Vista principal
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                var plazasExternas = await client.GetFromJsonAsync<List<PlazaVacante>>(
                    "api/PostulacionPublica/plazas-externas",
                    _jsonOptions);

                return View(plazasExternas ?? new List<PlazaVacante>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error al cargar plazas: {ex.Message}";
                return View(new List<PlazaVacante>());
            }
        }

        public async Task<IActionResult> Aplicar(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                var plaza = await client.GetFromJsonAsync<PlazaVacante>(
                    $"api/PostulacionPublica/plazas/{id}",
                    _jsonOptions);

                if (plaza == null)
                    return NotFound();

                ViewBag.Plaza = plaza;
                return View(new Postulante { PlazaVacanteId = id });
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aplicar(
            Postulante postulante,
            IFormFile? archivoCV,
            IFormFile? FotoTitulo,
            IFormFile? FotoColegiatura,
            IFormFile? FotoLicencia,
            IFormFile? FotoPermisoArmas,
            List<IFormFile>? ArchivoTitulos
        )
        {
            var client = _httpClientFactory.CreateClient("Api");

            var plaza = await client.GetFromJsonAsync<PlazaVacante>(
                $"api/PostulacionPublica/plazas/{postulante.PlazaVacanteId}",
                _jsonOptions);

            if (plaza == null)
            {
                TempData["ErrorMessage"] = "❌ Plaza no existe.";
                return RedirectToAction("Index");
            }

            ViewBag.Plaza = plaza;

            if (!ModelState.IsValid)
                return View(postulante);

            var existe = await client.GetFromJsonAsync<bool>(
                $"api/PostulacionPublica/existe-postulacion?plazaVacanteId={postulante.PlazaVacanteId}&cedula={Uri.EscapeDataString(postulante.Cedula ?? "")}",
                _jsonOptions);

            if (existe)
            {
                TempData["ErrorMessage"] = "⚠️ Ya existe postulación.";
                return View(postulante);
            }

            try
            {
                postulante.CurriculumPath = await GuardarArchivo(archivoCV, "curriculums", ExtPdf);
                postulante.FotoTituloPath = await GuardarArchivo(FotoTitulo, "uploads_postulantes", ExtImgs.Concat(ExtPdf).ToArray());

                postulante.EstadoProceso = "En revisión";
                postulante.FechaActualizacion = DateTime.Now;
                postulante.Id = 0;

                var response = await client.PostAsJsonAsync(
                    "api/PostulacionPublica/postular", postulante);

                var creado = await response.Content.ReadFromJsonAsync<Postulante>(_jsonOptions);

                return RedirectToAction("Confirmacion", new { id = creado!.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(postulante);
            }
        }

        public async Task<IActionResult> Confirmacion(int id)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var postulante = await client.GetFromJsonAsync<Postulante>(
                $"api/PostulacionPublica/postulacion/{id}",
                _jsonOptions);

            return View(postulante);
        }

        private async Task<string?> GuardarArchivo(IFormFile? file, string subfolder, string[] extensionesPermitidas)
        {
            if (file == null) return null;

            var ext = Path.GetExtension(file.FileName).ToLower();

            var ruta = Path.Combine(_env.WebRootPath, subfolder);
            Directory.CreateDirectory(ruta);

            var nombre = $"{Guid.NewGuid()}{ext}";
            var full = Path.Combine(ruta, nombre);

            using var fs = new FileStream(full, FileMode.Create);
            await file.CopyToAsync(fs);

            return $"/{subfolder}/{nombre}";
        }
    }
}