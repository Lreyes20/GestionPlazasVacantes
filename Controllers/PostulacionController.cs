using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Helpers;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GestionPlazasVacantes.Controllers
{
    [AllowAnonymous]
    public class PostulacionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly string[] ExtImgs = new[] { ".jpg", ".jpeg", ".png" };
        private static readonly string[] ExtPdf = new[] { ".pdf" };
        private static readonly string[] ExtWord = new[] { ".doc", ".docx" };

        public PostulacionController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
            _configuration = configuration;
        }

        // 🏢 Vista principal
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                var plazasExternas = await client.GetFromJsonAsync<List<PlazaDto>>(
                    "api/PostulacionPublica/plazas-externas",
                    _jsonOptions);

                return View(plazasExternas ?? new List<PlazaDto>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error al cargar plazas: {ex.Message}";
                return View(new List<PlazaDto>());
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Aplicar(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                var plaza = await client.GetFromJsonAsync<PlazaDto>(
                    $"api/PostulacionPublica/plazas/{id}",
                    _jsonOptions);

                if (plaza == null)
                    return NotFound();

                // 🔥 IMPORTANTE: asegurar lista para la vista
                plaza.Documentos ??= new List<DocumentoDto>();

                ViewBag.Plaza = plaza;

                return View(new Postulante
                {
                    PlazaVacanteId = id
                });
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aplicar(
            Postulante postulante,
            IFormFile? archivoCV,
            List<IFormFile>? DocumentosSubidos,
            List<string>? DocumentosNombres
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

            plaza.Documentos ??= new List<DocumentoDto>();
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

                // 🔹 FUNCIÓN AUXILIAR
                void AddFile(IFormFile? file, string name)
                {
                    if (file != null && file.Length > 0)
                    {
                        var stream = file.OpenReadStream();

                        var fileContent = new StreamContent(stream);

                        fileContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                        content.Add(fileContent, name, file.FileName);
                    }
                }

                // 🔥 CV
                AddFile(archivoCV, "archivoCV");

                // =====================================================
                // 🔥 DOCUMENTOS DINÁMICOS
                // =====================================================
                if (DocumentosSubidos != null && DocumentosNombres != null)
                {
                    for (int i = 0; i < DocumentosSubidos.Count; i++)
                    {
                        var file = DocumentosSubidos[i];
                        var nombre = DocumentosNombres.ElementAtOrDefault(i);

                        if (file != null && file.Length > 0 && !string.IsNullOrWhiteSpace(nombre))
                        {
                            var stream = file.OpenReadStream();

                            var fileContent = new StreamContent(stream);

                            fileContent.Headers.ContentType =
                                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                            content.Add(fileContent, "Documentos", file.FileName);
                            content.Add(new StringContent(nombre.Trim()), "DocumentosNombres");
                        }
                    }
                }

                var response = await client.PostAsync(
                    "api/PostulacionPublica/postular",
                    content);

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

        [AllowAnonymous]
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

        [HttpPost]
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
            QuestPDF.Settings.License = LicenseType.Community;

            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.MarginTop(40);
                    page.MarginBottom(40);
                    page.MarginLeft(30);
                    page.MarginRight(30);

                    // 🔶 HEADER INSTITUCIONAL
                    page.Header().Element(h => h.DocumentHeader(logoPath, "CURRÍCULUM VITAE"));

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        // ═══ NOMBRE DESTACADO ═══
                        col.Item().Background(PdfHelper.BlueLight)
                            .Padding(15)
                            .Column(nameCol =>
                            {
                                nameCol.Item().Text(NombreCompleto)
                                    .FontSize(20).Bold().FontColor(PdfHelper.Blue);
                                nameCol.Item().Text($"Cédula: {Cedula}")
                                    .FontSize(11).FontColor(PdfHelper.DarkGrey);
                            });

                        // ═══ DATOS DE CONTACTO ═══
                        col.Item().DataCard().Column(contact =>
                        {
                            contact.Spacing(6);
                            contact.Item().InfoRow("Correo", Correo);
                            contact.Item().InfoRow("Teléfono", Telefono);
                            contact.Item().InfoRow("Dirección", Direccion);
                        });

                        // ═══ SECCIONES CV ═══
                        void CvSection(string titulo, string? contenido)
                        {
                            if (string.IsNullOrWhiteSpace(contenido))
                                return;

                            col.Item().PaddingTop(8).SectionTitle(titulo);
                            col.Item().PaddingTop(4).Text(contenido)
                                .FontSize(10).FontColor(PdfHelper.DarkGrey);
                        }

                        CvSection("Perfil Profesional", PerfilProfesional);
                        CvSection("Experiencia Laboral", ExperienciaLaboral);
                        CvSection("Formación Académica", FormacionAcademica);

                        // ═══ HABILIDADES E IDIOMAS EN DOS COLUMNAS ═══
                        if (!string.IsNullOrWhiteSpace(Habilidades) ||
                            !string.IsNullOrWhiteSpace(Idiomas))
                        {
                            col.Item().PaddingTop(8).Row(row =>
                            {
                                if (!string.IsNullOrWhiteSpace(Habilidades))
                                {
                                    row.RelativeItem().PaddingRight(5).Column(c =>
                                    {
                                        c.Item().SectionTitle("Habilidades");
                                        c.Item().PaddingTop(4).Text(Habilidades)
                                            .FontSize(10).FontColor(PdfHelper.DarkGrey);
                                    });
                                }

                                if (!string.IsNullOrWhiteSpace(Idiomas))
                                {
                                    row.RelativeItem().PaddingLeft(5).Column(c =>
                                    {
                                        c.Item().SectionTitle("Idiomas");
                                        c.Item().PaddingTop(4).Text(Idiomas)
                                            .FontSize(10).FontColor(PdfHelper.DarkGrey);
                                    });
                                }
                            });
                        }

                        CvSection("Formación Complementaria", FormacionComplementaria);
                        CvSection("Otros Datos", OtrosDatos);

                        // ═══ DISCLAIMER ═══
                        col.Item().LegalText(
                            "Documento generado automáticamente por el sistema institucional. " +
                            "La información contenida es responsabilidad del postulante.");
                    });

                    // 🔻 FOOTER PROFESIONAL
                    page.Footer().PaddingTop(10).Element(f => f.DocumentFooter());
                });
            });

            var stream = new MemoryStream();
            pdf.GeneratePdf(stream);

            return File(stream.ToArray(), "application/pdf");
        }

        [AllowAnonymous]
        public async Task<IActionResult> MisPostulaciones()
        {
            var client = _httpClientFactory.CreateClient("Api");

            //var plazas = await client.GetFromJsonAsync<List<dynamic>>("api/PostulacionPublica/plazas");
            List<PlazaSimpleDto> plazas = new();

            try
            {
                var response = await client.GetAsync("api/PostulacionPublica/plazas");

                if (response.IsSuccessStatusCode)
                {
                    plazas = await response.Content.ReadFromJsonAsync<List<PlazaSimpleDto>>();
                }
            }
            catch
            {
            }

            ViewBag.Plazas = plazas;

            return View(new List<Postulante>());
        }

        public async Task<IActionResult> DescargarComprobantePdf(int id)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var modelo = await client.GetFromJsonAsync<ConfirmacionPostulacionDto>(
                $"api/plazas-internas/confirmacion/{id}");

            if (modelo == null)
                return NotFound();

            QuestPDF.Settings.License = LicenseType.Community;

            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(0);

                    // 🔶 MARCA DE AGUA
                    page.Background().Element(bg => bg.Watermark());

                    page.Content().Column(col =>
                    {
                        col.Spacing(0);

                        // ═══ BANNER NARANJA INSTITUCIONAL ═══
                        col.Item().Element(b => b.OrangeBannerHeader(logoPath));

                        // ═══ CONTENIDO PRINCIPAL ═══
                        col.Item().PaddingVertical(25).PaddingHorizontal(30).Column(body =>
                        {
                            body.Spacing(12);

                            // Título del documento
                            body.Item().AlignCenter().Text("COMPROBANTE OFICIAL DE POSTULACIÓN")
                                .Bold().FontSize(18).FontColor(PdfHelper.Blue);

                            body.Item().AlignCenter().Text($"Código de trámite: #{modelo.Id}")
                                .FontSize(10).FontColor(PdfHelper.MediumGrey);

                            // Línea decorativa
                            body.Item().LineHorizontal(1).LineColor(PdfHelper.Orange);

                            // ═══ DATOS DEL TRÁMITE ═══
                            body.Item().DataCard().Column(card =>
                            {
                                card.Spacing(8);
                                card.Item().InfoRowBold("Fecha de registro", modelo.FechaActualizacion.ToString("dd/MM/yyyy HH:mm"));
                                card.Item().InfoRowBold("Cédula", modelo.Cedula);
                                card.Item().InfoRowBold("Postulante", modelo.NombreCompleto);
                                card.Item().InfoRowBold("Plaza aplicada", modelo.PlazaTitulo);

                                card.Item().PaddingTop(8).LineHorizontal(1).LineColor(PdfHelper.LightGrey);

                                card.Item().PaddingTop(4).Text("Estado del proceso:")
                                    .SemiBold().FontSize(10).FontColor(PdfHelper.DarkGrey);
                                card.Item().Element(e => e.StatusBadge(modelo.EstadoProceso));
                            });

                            // ═══ TEXTO LEGAL ═══
                            body.Item().LegalText(
                                "Este documento certifica que la postulación fue registrada correctamente en el sistema " +
                                "institucional de la Municipalidad de Curridabat. Conserve este comprobante para el " +
                                "seguimiento del proceso de selección.");

                            // ═══ FIRMA ═══
                            body.Item().SignatureArea();
                        });
                    });

                    // 🔻 FOOTER PROFESIONAL
                    page.Footer().PaddingVertical(10).PaddingHorizontal(15).Element(f => f.DocumentFooter());
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);

            return File(
                stream.ToArray(),
                "application/pdf",
                $"Comprobante_{modelo.Cedula}_{modelo.Id}.pdf"
            );
        }

        public async Task<IActionResult> BuscarAjax(string numeroPlaza, string cedula)
        {
            var client = _httpClientFactory.CreateClient("Api");

            List<Postulante> data = new();

            try
            {
                var response = await client.GetAsync(
                    $"api/PostulacionPublica/buscar-por-plaza?numeroPlaza={numeroPlaza}&cedula={cedula}");

                if (response.IsSuccessStatusCode)
                {
                    data = await response.Content.ReadFromJsonAsync<List<Postulante>>(_jsonOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return PartialView("_ListaPostulaciones", data);
        }
    }
}