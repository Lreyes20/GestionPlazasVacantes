using ClosedXML.Excel;
using GestionPlazasVacantes.Helpers;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionPlazasVacantes.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ReportesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public ReportesController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        // GET: Reportes
        public async Task<IActionResult> Index(string? numeroConcurso)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                //var plazas = await client.GetFromJsonAsync<List<PlazaVacante>>("api/ReportesApi/plazas");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new JsonStringEnumConverter());

                var plazas = await client.GetFromJsonAsync<List<PlazaVacante>>(
                    "api/ReportesApi/plazas",
                    options);

                if (!string.IsNullOrWhiteSpace(numeroConcurso))
                {
                    plazas = plazas
                        .Where(p => p.NumeroConcurso.Contains(numeroConcurso))
                        .ToList();
                }

                var ordenadas = plazas
                    .OrderByDescending(p => p.FechaCreacion)
                    .ToList();

                return View(new ReporteViewModel { PlazasDisponibles = ordenadas });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ReportesController.Index: {ex.Message}");
                return View(new ReporteViewModel());
            }
        }

        // GET: Reportes/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // 🔥 ESTA LÍNEA ARREGLA TODO
            options.Converters.Add(new JsonStringEnumConverter());

            var response = await client.GetAsync($"api/ReportesApi/detalle/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var data = await response.Content.ReadFromJsonAsync<ReporteViewModel>(options);

            if (data == null)
                return NotFound();

            return View(data);
        }

        public async Task<IActionResult> ExportarPDF(int plazaId)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            var response = await client.GetAsync($"api/ReportesApi/detalle/{plazaId}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var stats = await response.Content.ReadFromJsonAsync<ReporteViewModel>(options);
            if (stats == null) return NotFound();

            var plaza = stats.Plaza;

            QuestPDF.Settings.License = LicenseType.Community;

            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(0);

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
                            body.Item().AlignCenter().Text("REPORTE DE PLAZA VACANTE")
                                .Bold().FontSize(20).FontColor(PdfHelper.Blue);

                            body.Item().LineHorizontal(1).LineColor(PdfHelper.Orange);

                            // ═══ DATOS DE LA PLAZA ═══
                            body.Item().DataCard().Column(card =>
                            {
                                card.Spacing(6);
                                card.Item().InfoRowBold("Concurso", plaza.NumeroConcurso);
                                card.Item().InfoRowBold("Título", plaza.Titulo);
                                card.Item().InfoRowBold("Departamento", plaza.Departamento);
                            });

                            // ═══ ESTADÍSTICAS EN GRID ═══
                            body.Item().PaddingTop(8).SectionTitle("Resumen del Proceso");

                            // Fila 1: 3 tarjetas
                            body.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().PaddingRight(4)
                                    .Element(e => e.StatCard("Participantes", stats.TotalParticipantes.ToString(), PdfHelper.Blue));
                                row.RelativeItem().PaddingHorizontal(2)
                                    .Element(e => e.StatCard("Doc. Completa", stats.DocumentacionCompleta.ToString(), PdfHelper.Green));
                                row.RelativeItem().PaddingLeft(4)
                                    .Element(e => e.StatCard("Doc. Incompleta", stats.DocumentacionIncompleta.ToString(), PdfHelper.Red));
                            });

                            // Fila 2: 3 tarjetas
                            body.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().PaddingRight(4)
                                    .Element(e => e.StatCard("Técnica", stats.AprobaronTecnica.ToString(), PdfHelper.Orange));
                                row.RelativeItem().PaddingHorizontal(2)
                                    .Element(e => e.StatCard("Psicométrica", stats.AprobaronPsicometrica.ToString(), "#7B1FA2"));
                                row.RelativeItem().PaddingLeft(4)
                                    .Element(e => e.StatCard("Entrevista", stats.AprobaronEntrevista.ToString(), "#00838F"));
                            });

                            // Fila 3: 2 tarjetas
                            body.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().PaddingRight(4)
                                    .Element(e => e.StatCard("Elegibles", stats.CandidatosElegibles.ToString(), PdfHelper.Green));
                                row.RelativeItem().PaddingLeft(4)
                                    .Element(e => e.StatCard("Seleccionados", stats.Seleccionados.ToString(), PdfHelper.Blue));
                            });

                            // ═══ DISCLAIMER ═══
                            body.Item().LegalText(
                                "Este reporte es generado automáticamente por el sistema institucional y tiene carácter informativo. " +
                                "Los datos reflejan el estado actual del proceso de selección.");
                        });
                    });

                    // 🔻 FOOTER PROFESIONAL
                    page.Footer().PaddingVertical(10).PaddingHorizontal(15).Element(f => f.DocumentFooter());
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);

            return File(stream.ToArray(), "application/pdf", $"Reporte_{plaza.NumeroConcurso}.pdf");
        }

        public async Task<IActionResult> ExportarExcel(int plazaId)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            var response = await client.GetAsync($"api/ReportesApi/detalle/{plazaId}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var stats = await response.Content.ReadFromJsonAsync<ReporteViewModel>(options);
            if (stats == null) return NotFound();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte");

            // HEADER
            ws.Cell("A1").Value = "Reporte de Plaza";
            ws.Range("A1:B1").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16);

            // DATOS
            ws.Cell("A3").Value = "Total Participantes";
            ws.Cell("B3").Value = stats.TotalParticipantes;

            ws.Cell("A4").Value = "Documentación Completa";
            ws.Cell("B4").Value = stats.DocumentacionCompleta;

            ws.Cell("A5").Value = "Aprobaron Técnica";
            ws.Cell("B5").Value = stats.AprobaronTecnica;

            ws.Cell("A6").Value = "Aprobaron Psicométrica";
            ws.Cell("B6").Value = stats.AprobaronPsicometrica;

            ws.Cell("A7").Value = "Seleccionados";
            ws.Cell("B7").Value = stats.Seleccionados;

            // ESTILO
            ws.Range("A3:A7").Style.Font.SetBold();
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Reporte.xlsx");
        }

        public async Task<IActionResult> ExportarWord(int plazaId)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            var response = await client.GetAsync($"api/ReportesApi/detalle/{plazaId}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var stats = await response.Content.ReadFromJsonAsync<ReporteViewModel>(options);
            if (stats == null) return NotFound();

            var plaza = stats.Plaza;

            using var stream = new MemoryStream();

            using (var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(
                stream,
                DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
                true))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                var body = new DocumentFormat.OpenXml.Wordprocessing.Body();

                void AddText(string text, bool bold = false)
                {
                    var run = new DocumentFormat.OpenXml.Wordprocessing.Run();
                    if (bold)
                        run.Append(new DocumentFormat.OpenXml.Wordprocessing.RunProperties(
                            new DocumentFormat.OpenXml.Wordprocessing.Bold()));

                    run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text(text));
                    body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(run));
                }

                AddText("REPORTE DE PLAZA", true);
                AddText("");
                AddText($"Concurso: {plaza.NumeroConcurso}");
                AddText($"Título: {plaza.Titulo}");
                AddText($"Departamento: {plaza.Departamento}");
                AddText("");
                AddText("Resumen:", true);
                AddText($"Total participantes: {stats.TotalParticipantes}");
                AddText($"Documentación completa: {stats.DocumentacionCompleta}");
                AddText($"Seleccionados: {stats.Seleccionados}");

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"Reporte_{plaza.NumeroConcurso}.docx");
        }
    }
}