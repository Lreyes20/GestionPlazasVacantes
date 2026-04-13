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

        // SOLO CAMBIA ESTE MÉTODO, TODO LO DEMÁS QUEDA IGUAL

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
                var content = new MultipartFormDataContent();

                // 🔹 MAPEAR TODO EL MODELO
                foreach (var prop in typeof(Postulante).GetProperties())
                {
                    var value = prop.GetValue(postulante);
                    if (value != null)
                        content.Add(new StringContent(value.ToString()!), prop.Name);
                }

                // 🔹 ARCHIVOS
                void AddFile(IFormFile? file, string name)
                {
                    if (file != null)
                    {
                        var stream = file.OpenReadStream();
                        var fileContent = new StreamContent(stream);
                        fileContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                        content.Add(fileContent, name, file.FileName);
                    }
                }

                AddFile(archivoCV, "archivoCV");
                AddFile(FotoTitulo, "FotoTitulo");
                AddFile(FotoColegiatura, "FotoColegiatura");
                AddFile(FotoLicencia, "FotoLicencia");
                AddFile(FotoPermisoArmas, "FotoPermisoArmas");

                if (ArchivoTitulos != null)
                {
                    foreach (var file in ArchivoTitulos)
                        AddFile(file, "ArchivoTitulos");
                }

                var response = await client.PostAsync(
                    "api/PostulacionPublica/postular", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = error;
                    return View(postulante);
                }

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

            try
            {
                var postulante = await client.GetFromJsonAsync<Postulante>(
                $"api/PostulacionPublica/postulacion/{id}",
                _jsonOptions);

                return View(postulante);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<string?> GuardarArchivo(IFormFile? file, string subfolder, string[] extensionesPermitidas)
        {
            Console.WriteLine("ENTRÓ A GENERAR CV");
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

        [HttpPost("GenerarCVPrevia")]
        public IActionResult GenerarCVPrevia(
            string NombreCompleto,
            string Cedula,
            string? Correo,
            string? Telefono,
            string? Direccion,
            string? PerfilProfesional,
            string? ExperienciaLaboral,
            string? FormacionAcademica,
            string? Habilidades,
            string? Idiomas,
            string? FormacionComplementaria,
            string? OtrosDatos
        )
        {
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Content().Column(col =>
                    {
                        // 🔷 HEADER CON LOGO
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(80).Height(60).Image(logoPath);

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text(NombreCompleto)
                                    .FontSize(20).Bold();

                                c.Item().AlignRight().Text($"Cédula: {Cedula}")
                                    .FontSize(10).FontColor(Colors.Grey.Darken1);
                            });
                        });

                        col.Item().PaddingVertical(5).LineHorizontal(1);

                        // 🔷 CONTACTO
                        col.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Correo: ").SemiBold();
                            text.Span(Correo ?? "");
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("Teléfono: ").SemiBold();
                            text.Span(Telefono ?? "");
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("Dirección: ").SemiBold();
                            text.Span(Direccion ?? "");
                        });

                        // 🔷 SECCIONES
                        void Seccion(string titulo, string? contenido)
                        {
                            col.Item().PaddingTop(10).Text(titulo)
                                .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                            col.Item().LineHorizontal(1);

                            col.Item().Text(contenido ?? "").FontSize(10);
                        }

                        Seccion("Perfil Profesional", PerfilProfesional);
                        Seccion("Experiencia Laboral", ExperienciaLaboral);
                        Seccion("Formación Académica", FormacionAcademica);

                        // 🔷 DOS COLUMNAS
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Habilidades").Bold();
                                c.Item().LineHorizontal(1);
                                c.Item().Text(Habilidades ?? "").FontSize(10);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Idiomas").Bold();
                                c.Item().LineHorizontal(1);
                                c.Item().Text(Idiomas ?? "").FontSize(10);
                            });
                        });

                        Seccion("Formación Complementaria", FormacionComplementaria);
                        Seccion("Otros Datos", OtrosDatos);
                    });
                });
            });

            var stream = new MemoryStream();
            pdf.GeneratePdf(stream);

            return File(stream.ToArray(), "application/pdf");
        }
    }
}