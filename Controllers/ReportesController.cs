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

            var data = await client.GetFromJsonAsync<ReporteViewModel>($"api/ReportesApi/detalle/{id}");

            if (data == null) return NotFound();

            return View(data);
        }

        public async Task<IActionResult> ExportarPDF(int plazaId)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var stats = await client.GetFromJsonAsync<ReporteViewModel>($"api/ReportesApi/detalle/{plazaId}");

            if (stats == null) return NotFound();

            var plaza = stats.Plaza;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);

                    page.Header().Text($"Reporte de Plaza: {plaza.NumeroConcurso}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().Column(x =>
                    {
                        x.Item().Text($"Título: {plaza.Titulo}");
                        x.Item().Text($"Departamento: {plaza.Departamento}");

                        x.Item().Text("Total Participantes: " + stats.TotalParticipantes);
                        x.Item().Text("Documentación Completa: " + stats.DocumentacionCompleta);
                        x.Item().Text("Aprobaron Técnica: " + stats.AprobaronTecnica);
                        x.Item().Text("Aprobaron Psicométrica: " + stats.AprobaronPsicometrica);
                        x.Item().Text("Elegibles: " + stats.CandidatosElegibles);
                        x.Item().Text("Seleccionados: " + stats.Seleccionados);
                    });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"Reporte_{plaza.NumeroConcurso}.pdf");
        }

        public async Task<IActionResult> ExportarExcel(int plazaId)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var stats = await client.GetFromJsonAsync<ReporteViewModel>($"api/ReportesApi/detalle/{plazaId}");

            if (stats == null) return NotFound();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte");

            ws.Cell(1, 1).Value = "Total Participantes";
            ws.Cell(1, 2).Value = stats.TotalParticipantes;

            ws.Cell(2, 1).Value = "Seleccionados";
            ws.Cell(2, 2).Value = stats.Seleccionados;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Reporte.xlsx");
        }
    }
}