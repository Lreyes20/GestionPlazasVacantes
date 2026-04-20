using ClosedXML.Excel;
using GestionPlazasVacantes.Models;
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

        public ReportesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(0);

                    page.Content().Column(col =>
                    {
                        // 🟧 FRANJA
                        col.Item().Background("#F58220").Height(15);

                        col.Item().Padding(30).Column(c =>
                        {
                            // HEADER
                            c.Item().Text("Municipalidad de Curridabat")
                                .FontSize(12);

                            c.Item().Text("REPORTE DE PLAZA")
                                .FontSize(18)
                                .Bold()
                                .FontColor("#F58220");

                            c.Item().PaddingTop(10).Text($"Concurso: {plaza.NumeroConcurso}");
                            c.Item().Text($"Título: {plaza.Titulo}");
                            c.Item().Text($"Departamento: {plaza.Departamento}");

                            // ESTADÍSTICAS
                            c.Item().PaddingTop(15).Text("Resumen del proceso:")
                                .Bold();

                            c.Item().Text($"Total participantes: {stats.TotalParticipantes}");
                            c.Item().Text($"Documentación completa: {stats.DocumentacionCompleta}");
                            c.Item().Text($"Aprobaron técnica: {stats.AprobaronTecnica}");
                            c.Item().Text($"Aprobaron psicométrica: {stats.AprobaronPsicometrica}");
                            c.Item().Text($"Elegibles: {stats.CandidatosElegibles}");
                            c.Item().Text($"Seleccionados: {stats.Seleccionados}");
                        });
                    });
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