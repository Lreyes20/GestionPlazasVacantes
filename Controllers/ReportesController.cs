using ClosedXML.Excel;
using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionPlazasVacantes.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Jefe")]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Reportes
        public async Task<IActionResult> Index(string? numeroConcurso)
        {
            try
            {
                // Lista general de plazas (para el seletor/listado)
                var plazasDisponibles = await _context.PlazasVacantes
                    .OrderByDescending(p => p.FechaCreacion)
                    .ToListAsync();

                if (string.IsNullOrWhiteSpace(numeroConcurso))
                {
                    // Si no busca nada, retornamos el modelo con la lista pero sin estadísticas específicas
                    return View(new ReporteViewModel { PlazasDisponibles = plazasDisponibles });
                }

                var plaza = await _context.PlazasVacantes
                    .Include(p => p.Postulantes)
                    .FirstOrDefaultAsync(p => p.NumeroConcurso == numeroConcurso);

                if (plaza == null)
                {
                    ViewBag.Error = "⚠️ No se encontró ninguna plaza con ese número de concurso.";
                    return View(new ReporteViewModel { PlazasDisponibles = plazasDisponibles });
                }

                // Calcular estadísticas
                var postulantesIds = plaza.Postulantes.Select(p => p.Id).ToList();
                var seguimientos = await _context.SeguimientosPostulantes
                    .Where(s => postulantesIds.Contains(s.PostulanteId))
                    .ToListAsync();

                var stats = new ReporteViewModel
                {
                    Plaza = plaza,
                    PlazasDisponibles = plazasDisponibles, // Pasamos la lista también aquí
                    TotalParticipantes = plaza.Postulantes.Count,
                    DocumentacionCompleta = seguimientos.Count(s => s.CumpleRequisitos),
                    DocumentacionIncompleta = seguimientos.Count(s => !s.CumpleRequisitos),
                    AprobaronTecnica = seguimientos.Count(s => s.NotaPruebaTecnica >= 70),
                    AprobaronPsicometrica = seguimientos.Count(s => s.NotaPsicometrica >= 70),
                    AprobaronEntrevista = seguimientos.Count(s => (s.EtapaActual == "Entrevista presencial" && s.CumpleRequisitos) || s.EtapaActual == "Final"),
                    CandidatosElegibles = seguimientos.Count(s => s.CumpleRequisitos && s.Activo && s.EtapaActual != "Descartado"),
                    Seleccionados = plaza.Postulantes.Count(p => p.EstadoProceso == "Contratado")
                };

                return View(stats);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ocurrió un error: {ex.Message} \n {ex.StackTrace}";
                return View(null);
            }
        }

        public async Task<IActionResult> ExportarPDF(int plazaId)
        {
            var plaza = await _context.PlazasVacantes
                .Include(p => p.Postulantes)
                .FirstOrDefaultAsync(p => p.Id == plazaId);

            if (plaza == null) return NotFound();

            var postulantesIds = plaza.Postulantes.Select(p => p.Id).ToList();
            var seguimientos = await _context.SeguimientosPostulantes
                .Where(s => postulantesIds.Contains(s.PostulanteId))
                .ToListAsync();

            var stats = new ReporteViewModel
            {
                Plaza = plaza,
                TotalParticipantes = plaza.Postulantes.Count,
                DocumentacionCompleta = seguimientos.Count(s => s.CumpleRequisitos),
                DocumentacionIncompleta = seguimientos.Count(s => !s.CumpleRequisitos),
                AprobaronTecnica = seguimientos.Count(s => s.NotaPruebaTecnica >= 70),
                AprobaronPsicometrica = seguimientos.Count(s => s.NotaPsicometrica >= 70),
                CandidatosElegibles = seguimientos.Count(s => s.CumpleRequisitos && s.Activo),
                Seleccionados = plaza.Postulantes.Count(p => p.EstadoProceso == "Contratado")
            };

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text($"Reporte de Plaza: {plaza.NumeroConcurso}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Título: {plaza.Titulo}");
                            x.Item().Text($"Departamento: {plaza.Departamento}");
                            x.Item().Text($"Fecha de Cierre: {plaza.FechaLimite:dd/MM/yyyy}");

                            x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            x.Item().Text("Estadísticas del Proceso").Bold().FontSize(16);

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Métrica");
                                    header.Cell().Element(CellStyle).Text("Cantidad");
                                });

                                table.Cell().Element(CellStyle).Text("Total Participantes");
                                table.Cell().Element(CellStyle).Text(stats.TotalParticipantes.ToString());

                                table.Cell().Element(CellStyle).Text("Documentación Completa");
                                table.Cell().Element(CellStyle).Text(stats.DocumentacionCompleta.ToString());

                                table.Cell().Element(CellStyle).Text("Aprobaron Pruebas Técnicas");
                                table.Cell().Element(CellStyle).Text(stats.AprobaronTecnica.ToString());

                                table.Cell().Element(CellStyle).Text("Aprobaron Psicométricas");
                                table.Cell().Element(CellStyle).Text(stats.AprobaronPsicometrica.ToString());

                                table.Cell().Element(CellStyle).Text("Candidatos Elegibles");
                                table.Cell().Element(CellStyle).Text(stats.CandidatosElegibles.ToString());

                                table.Cell().Element(CellStyle).Text("Seleccionados / Contratados");
                                table.Cell().Element(CellStyle).Text(stats.Seleccionados.ToString());

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingVertical(5);
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generado el ");
                            x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
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
            var plaza = await _context.PlazasVacantes
                .Include(p => p.Postulantes)
                .FirstOrDefaultAsync(p => p.Id == plazaId);

            if (plaza == null) return NotFound();

            var postulantesIds = plaza.Postulantes.Select(p => p.Id).ToList();
            var seguimientos = await _context.SeguimientosPostulantes
                .Where(s => postulantesIds.Contains(s.PostulanteId))
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte");

                // Encabezado
                worksheet.Cell(1, 1).Value = "Reporte de Plaza";
                worksheet.Cell(1, 2).Value = plaza.NumeroConcurso;
                worksheet.Cell(2, 1).Value = "Título";
                worksheet.Cell(2, 2).Value = plaza.Titulo;
                worksheet.Cell(3, 1).Value = "Departamento";
                worksheet.Cell(3, 2).Value = plaza.Departamento;

                // Estadísticas
                int row = 5;
                worksheet.Cell(row++, 1).Value = "Métrica";
                worksheet.Cell(row - 1, 2).Value = "Cantidad";

                worksheet.Cell(row, 1).Value = "Total Participantes";
                worksheet.Cell(row++, 2).Value = plaza.Postulantes.Count;

                worksheet.Cell(row, 1).Value = "Documentación Completa";
                worksheet.Cell(row++, 2).Value = seguimientos.Count(s => s.CumpleRequisitos);

                worksheet.Cell(row, 1).Value = "Aprobaron Pruebas Técnicas";
                worksheet.Cell(row++, 2).Value = seguimientos.Count(s => s.NotaPruebaTecnica >= 70);

                worksheet.Cell(row, 1).Value = "Aprobaron Psicométricas";
                worksheet.Cell(row++, 2).Value = seguimientos.Count(s => s.NotaPsicometrica >= 70);

                worksheet.Cell(row, 1).Value = "Candidatos Elegibles";
                worksheet.Cell(row++, 2).Value = seguimientos.Count(s => s.CumpleRequisitos && s.Activo);

                worksheet.Cell(row, 1).Value = "Seleccionados / Contratados";
                worksheet.Cell(row++, 2).Value = plaza.Postulantes.Count(p => p.EstadoProceso == "Contratado");

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_{plaza.NumeroConcurso}.xlsx");
                }
            }
        }
    }

    public class ReporteViewModel
    {
        public List<PlazaVacante> PlazasDisponibles { get; set; } = new List<PlazaVacante>();
        public PlazaVacante Plaza { get; set; }
        public int TotalParticipantes { get; set; }
        public int DocumentacionCompleta { get; set; }
        public int DocumentacionIncompleta { get; set; }
        public int AprobaronTecnica { get; set; }
        public int AprobaronPsicometrica { get; set; }
        public int AprobaronEntrevista { get; set; }
        public int CandidatosElegibles { get; set; }
        public int Seleccionados { get; set; }
    }
}
