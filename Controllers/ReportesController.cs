using ClosedXML.Excel;
using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace GestionPlazasVacantes.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(AppDbContext context, ILogger<ReportesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Reportes
        public async Task<IActionResult> Index(string? numeroConcurso)
        {
            try
            {
                var query = _context.PlazasVacantes
                    .Include(p => p.UsuarioAsignado)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(numeroConcurso))
                {
                    query = query.Where(p => p.NumeroConcurso.Contains(numeroConcurso));
                }

                var plazasDisponibles = await query
                    .OrderByDescending(p => p.FechaCreacion)
                    .ToListAsync();

                // Solo retornamos la lista para el Index
                return View(new ReporteViewModel { PlazasDisponibles = plazasDisponibles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar índice de reportes");
                return View(new ReporteViewModel());
            }
        }

        // GET: Reportes/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var plaza = await _context.PlazasVacantes
                .Include(p => p.Postulantes)
                .Include(p => p.UsuarioAsignado)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plaza == null) return NotFound();

            // Obtener seguimientos detallados
            var postulantesIds = plaza.Postulantes.Select(p => p.Id).ToList();
            var seguimientos = await _context.SeguimientosPostulantes
                .Include(s => s.Postulante) // Incluir datos del postulante
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
                AprobaronEntrevista = seguimientos.Count(s => (s.EtapaActual == "Entrevista presencial" && s.CumpleRequisitos) || s.EtapaActual == "Final"),
                CandidatosElegibles = seguimientos.Count(s => s.CumpleRequisitos && s.Activo && s.EtapaActual != "Descartado"),
                Seleccionados = plaza.Postulantes.Count(p => p.EstadoProceso == "Contratado"),
                Seguimientos = seguimientos // Lista detallada para la tabla
            };

            return View(stats);
        }

        public async Task<IActionResult> ExportarPDF(int plazaId)
        {
            // Configurar licencia de QuestPDF (requerido)
            QuestPDF.Settings.License = LicenseType.Community;
            
            try
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

                var document = QuestPDF.Fluent.Document.Create(container =>
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
                string safeConcurso = plaza.NumeroConcurso.Trim().Replace("/", "-").Replace("\\", "-");
                string fileName = $"Reporte_{safeConcurso}.pdf";
                
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar PDF para plaza {PlazaId}", plazaId);
                return BadRequest("Ocurrió un error al generar el PDF.");
            }
        }

        public async Task<IActionResult> ExportarExcel(int plazaId)
        {
            try
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

                    // Detalle de postulantes en Excel también
                    row += 2;
                    worksheet.Cell(row, 1).Value = "Detalle de Postulantes";
                    worksheet.Range(row, 1, row, 6).Merge().Style.Font.Bold = true;
                    row++;
                    
                    worksheet.Cell(row, 1).Value = "Postulante";
                    worksheet.Cell(row, 2).Value = "Cédula";
                    worksheet.Cell(row, 3).Value = "Nota Técnica";
                    worksheet.Cell(row, 4).Value = "Nota Psicométrica";
                    worksheet.Cell(row, 5).Value = "Etapa Actual";
                    worksheet.Cell(row, 6).Value = "Resultado";
                    worksheet.Range(row, 1, row, 6).Style.Font.Bold = true;
                    row++;

                    if (seguimientos.Any())
                    {
                        var postulantesDetallados = await _context.SeguimientosPostulantes
                             .Include(s => s.Postulante)
                             .Where(s => postulantesIds.Contains(s.PostulanteId))
                             .ToListAsync();

                        foreach (var s in postulantesDetallados)
                        {
                            worksheet.Cell(row, 1).Value = s.Postulante.NombreCompleto;
                            worksheet.Cell(row, 2).Value = s.Postulante.Cedula;
                            worksheet.Cell(row, 3).Value = s.NotaPruebaTecnica;
                            worksheet.Cell(row, 4).Value = s.NotaPsicometrica;
                            worksheet.Cell(row, 5).Value = s.EtapaActual;
                            worksheet.Cell(row, 6).Value = s.Postulante.EstadoProceso == "Descartado" 
                                ? (!string.IsNullOrEmpty(s.MotivoDescarte) ? s.MotivoDescarte : "Descartado") 
                                : s.Postulante.EstadoProceso;
                            row++;
                        }
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream); // Esto no cierra el stream, pero sí lo llena
                        stream.Position = 0; 
                        var content = stream.ToArray();
                        
                        string safeConcurso = plaza.NumeroConcurso.Trim().Replace("/", "-").Replace("\\", "-");
                        string fileName = $"Reporte_{safeConcurso}.xlsx";
                        
                        Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar Excel para plaza {PlazaId}", plazaId);
                return BadRequest("Ocurrió un error al generar el Excel.");
            }
        }

        public async Task<IActionResult> ExportarWord(int plazaId)
        {
            try
            {
                var plaza = await _context.PlazasVacantes
                    .Include(p => p.Postulantes)
                    .FirstOrDefaultAsync(p => p.Id == plazaId);

                if (plaza == null) return NotFound();

                var postulantesIds = plaza.Postulantes.Select(p => p.Id).ToList();
                var seguimientos = await _context.SeguimientosPostulantes
                    .Include(s => s.Postulante)
                    .Where(s => postulantesIds.Contains(s.PostulanteId))
                    .ToListAsync();

                using (MemoryStream ms = new MemoryStream())
                {
                    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
                    {
                        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                        Body body = mainPart.Document.AppendChild(new Body());

                        // Estilos básicos
                        Paragraph title = new Paragraph(new Run(new Text($"Reporte de Plaza: {plaza.NumeroConcurso}")));
                        title.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                        RunProperties titleRunProps = title.GetFirstChild<Run>().PrependChild(new RunProperties());
                        titleRunProps.Bold = new Bold();
                        titleRunProps.FontSize = new FontSize() { Val = "32" }; // 16pt
                        body.Append(title);

                        body.Append(new Paragraph(new Run(new Text($"Título: {plaza.Titulo}"))));
                        body.Append(new Paragraph(new Run(new Text($"Departamento: {plaza.Departamento}"))));
                        body.Append(new Paragraph(new Run(new Text($"Fecha de Cierre: {plaza.FechaLimite:dd/MM/yyyy}"))));
                        body.Append(new Paragraph(new Run(new Text("")))); // Espacio

                        // Tabla Resumen
                        body.Append(new Paragraph(new Run(new Text("Resumen Estadístico"))) { ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Left }) });
                        
                        Table tableStats = new Table();
                        
                        // Propiedades de la tabla
                        TableProperties tblProps = new TableProperties(
                            new TableBorders(
                                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                                new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                                new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                                new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                                new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                            )
                        );
                        tableStats.AppendChild(tblProps);

                        // Filas
                        AddRowToWordTable(tableStats, "Total Participantes", plaza.Postulantes.Count.ToString());
                        AddRowToWordTable(tableStats, "Documentación Completa", seguimientos.Count(s => s.CumpleRequisitos).ToString());
                        AddRowToWordTable(tableStats, "Aprobaron Pruebas Técnicas", seguimientos.Count(s => s.NotaPruebaTecnica >= 70).ToString());
                        AddRowToWordTable(tableStats, "Aprobaron Psicométricas", seguimientos.Count(s => s.NotaPsicometrica >= 70).ToString());
                        AddRowToWordTable(tableStats, "Candidatos Elegibles", seguimientos.Count(s => s.CumpleRequisitos && s.Activo).ToString());
                        AddRowToWordTable(tableStats, "Seleccionados", plaza.Postulantes.Count(p => p.EstadoProceso == "Contratado").ToString());

                        body.Append(tableStats);
                        body.Append(new Paragraph(new Run(new Text(""))));

                        // Tabla Detalles
                        body.Append(new Paragraph(new Run(new Text("Detalle de Postulantes"))) { ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Left }) });
                        
                        Table tableDetails = new Table();
                        tableDetails.AppendChild((TableProperties)tblProps.CloneNode(true));

                        // Header
                        TableRow trHeader = new TableRow();
                        trHeader.Append(new TableCell(new Paragraph(new Run(new Text("Nombre")) { RunProperties = new RunProperties(new Bold()) })));
                        trHeader.Append(new TableCell(new Paragraph(new Run(new Text("Cédula")) { RunProperties = new RunProperties(new Bold()) })));
                        trHeader.Append(new TableCell(new Paragraph(new Run(new Text("Estado")) { RunProperties = new RunProperties(new Bold()) })));
                        tableDetails.Append(trHeader);

                        foreach(var s in seguimientos)
                        {
                            TableRow tr = new TableRow();
                            tr.Append(new TableCell(new Paragraph(new Run(new Text(s.Postulante.NombreCompleto)))));
                            tr.Append(new TableCell(new Paragraph(new Run(new Text(s.Postulante.Cedula)))));
                            
                            string estado = s.Postulante.EstadoProceso;
                            if (estado == "Descartado" && !string.IsNullOrEmpty(s.MotivoDescarte))
                            {
                                estado += $" ({s.MotivoDescarte})";
                            }
                            tr.Append(new TableCell(new Paragraph(new Run(new Text(estado)))));
                            tableDetails.Append(tr);
                        }
                         body.Append(tableDetails);

                        mainPart.Document.Save();
                    }
                    
                    ms.Position = 0;
                    string safeConcurso = plaza.NumeroConcurso.Trim().Replace("/", "-").Replace("\\", "-");
                    string fileName = $"Reporte_{safeConcurso}.docx";
                    
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar Word para plaza {PlazaId}", plazaId);
                return BadRequest("Ocurrió un error al generar el documento Word.");
            }
        }

        private void AddRowToWordTable(Table table, string label, string value)
        {
            TableRow tr = new TableRow();
            tr.Append(new TableCell(new Paragraph(new Run(new Text(label)))));
            tr.Append(new TableCell(new Paragraph(new Run(new Text(value)))));
            table.Append(tr);
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
        
        // 📋 Lista detallada para la tabla de reporte
        public List<SeguimientoPostulante> Seguimientos { get; set; } = new List<SeguimientoPostulante>();
    }
}
