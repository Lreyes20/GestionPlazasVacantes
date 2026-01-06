using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionPlazasVacantes.Controllers
{
    public class PlazasInternasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private static readonly string[] ExtImgs = new[] { ".jpg", ".jpeg", ".png" };
        private static readonly string[] ExtPdf = new[] { ".pdf" };

        public PlazasInternasController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 🏢 Vista principal de plazas INTERNAS disponibles
        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.Now;
            var plazasInternas = await _context.PlazasVacantes
                .Where(p => p.TipoConcurso == "Interno" && p.FechaLimite >= DateTime.Today && p.Activa)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(plazasInternas);
        }

        // 🧾 Mostrar formulario de aplicación (GET) - Con Pre-llenado
        public async Task<IActionResult> Aplicar(int id)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(id);
            if (plaza == null || plaza.TipoConcurso != "Interno") 
                return NotFound("Esta plaza no está disponible para postulación interna.");

            // Pre-llenar datos del usuario logueado
            var username = User.Identity?.Name;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);

            var postulante = new Postulante 
            { 
                PlazaVacanteId = id,
                NombreCompleto = usuario?.FullName ?? "",
                Correo = usuario?.Email ?? "",
                Cedula = "" 
            };

            ViewBag.Plaza = plaza;
            return View(postulante);
        }

        // 💾 Guardar postulación (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aplicar(Postulante model, IFormFile? curriculum, IFormFile? fotoTitulo, IFormFile? fotoColegiatura, IFormFile? fotoLicencia, IFormFile? fotoPermisoArmas)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(model.PlazaVacanteId);
            if (plaza == null || plaza.TipoConcurso != "Interno") 
                return NotFound();

            ViewBag.Plaza = plaza;

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(model.NombreCompleto) ||
                string.IsNullOrWhiteSpace(model.Cedula) ||
                string.IsNullOrWhiteSpace(model.Correo))
            {
                ModelState.AddModelError("", "⚠️ Debe completar todos los campos obligatorios.");
                return View(model);
            }

            // Validar unicidad de la postulación
            var existe = await _context.Postulantes
                .AnyAsync(p => p.PlazaVacanteId == model.PlazaVacanteId && p.Cedula == model.Cedula);

            if (existe)
            {
                ModelState.AddModelError("", $"⚠️ Ya existe una postulación registrada con esta cédula para la plaza '{plaza.Titulo}'.");
                return View(model);
            }

            // Validar curriculum (obligatorio)
            if (curriculum == null || curriculum.Length == 0)
            {
                ModelState.AddModelError("", "⚠️ Debe adjuntar su currículum vitae en formato PDF.");
                return View(model);
            }

            try
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "postulantes");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Guardar curriculum
                if (curriculum != null && curriculum.Length > 0)
                {
                    if (!ValidarExtension(curriculum.FileName, ExtPdf))
                    {
                        ModelState.AddModelError("", "⚠️ El currículum debe ser un archivo PDF.");
                        return View(model);
                    }

                    var curriculumFileName = $"{Guid.NewGuid()}_{SanitizarNombre(curriculum.FileName)}";
                    var curriculumPath = Path.Combine(uploadsPath, curriculumFileName);
                    using (var stream = new FileStream(curriculumPath, FileMode.Create))
                    {
                        await curriculum.CopyToAsync(stream);
                    }
                    model.CurriculumPath = $"/uploads/postulantes/{curriculumFileName}";
                }

                // Guardar documentos opcionales
                if (fotoTitulo != null && fotoTitulo.Length > 0 && plaza.SolicitarTitulos)
                {
                    var fileName = $"{Guid.NewGuid()}_{SanitizarNombre(fotoTitulo.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoTitulo.CopyToAsync(stream);
                    }
                    model.FotoTituloPath = $"/uploads/postulantes/{fileName}";
                }

                if (fotoColegiatura != null && fotoColegiatura.Length > 0 && plaza.SolicitarColegiatura)
                {
                    var fileName = $"{Guid.NewGuid()}_{SanitizarNombre(fotoColegiatura.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoColegiatura.CopyToAsync(stream);
                    }
                    model.FotoColegiaturaPath = $"/uploads/postulantes/{fileName}";
                }

                if (fotoLicencia != null && fotoLicencia.Length > 0 && plaza.SolicitarLicencia)
                {
                    var fileName = $"{Guid.NewGuid()}_{SanitizarNombre(fotoLicencia.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoLicencia.CopyToAsync(stream);
                    }
                    model.FotoLicenciaPath = $"/uploads/postulantes/{fileName}";
                }

                if (fotoPermisoArmas != null && fotoPermisoArmas.Length > 0 && plaza.SolicitarPermisoArmas)
                {
                    var fileName = $"{Guid.NewGuid()}_{SanitizarNombre(fotoPermisoArmas.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoPermisoArmas.CopyToAsync(stream);
                    }
                    model.FotoPermisoArmasPath = $"/uploads/postulantes/{fileName}";
                }

                // Guardar en base de datos
                model.Id = 0; // Asegurar que Entity Framework genere el ID automáticamente
                model.EstadoProceso = "Recibido";
                model.FechaActualizacion = DateTime.Now;

                _context.Postulantes.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Su postulación ha sido recibida exitosamente.";
                return RedirectToAction("Confirmacion", new { id = model.Id });
            }
            catch (Exception ex)
            {
                // Log detallado del error
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += $" | Inner: {ex.InnerException.Message}";
                }
                Console.WriteLine($"❌ Error al guardar postulación: {errorMsg}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                ModelState.AddModelError("", $"⚠️ Ocurrió un error al procesar su postulación: {errorMsg}");
                return View(model);
            }
        }

        // ✅ Confirmación
        public async Task<IActionResult> Confirmacion(int id)
        {
            var postulante = await _context.Postulantes
                .Include(p => p.PlazaVacante)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (postulante == null) return NotFound();
            return View(postulante);
        }

        // 🧾 Generar CV en PDF
        [HttpPost]
        public IActionResult GenerarCVPrevia(Postulante postulante)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            if (string.IsNullOrWhiteSpace(postulante.NombreCompleto) || string.IsNullOrWhiteSpace(postulante.Cedula))
            {
                TempData["ErrorMessage"] = "⚠️ Debes completar al menos tu nombre y cédula antes de generar el CV.";
                return RedirectToAction("Index");
            }

            var pdfBytes = Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));

                    page.Header().Element(h =>
                    {
                        h.PaddingBottom(20)
                         .AlignCenter()
                         .Text("Currículum Vitae")
                         .FontSize(20)
                         .Bold()
                         .FontColor(Colors.Blue.Medium);
                    });

                    page.Content().Column(col =>
                    {
                        void AddSection(string titulo, string? contenido)
                        {
                            if (!string.IsNullOrWhiteSpace(contenido))
                            {
                                col.Item().Text(titulo)
                                    .Bold()
                                    .FontSize(14)
                                    .FontColor(Colors.Orange.Medium);

                                col.Item().Element(e =>
                                {
                                    e.PaddingBottom(10)
                                     .Text(contenido)
                                     .FontSize(12)
                                     .FontColor(Colors.Grey.Darken3);
                                });
                            }
                        }

                        AddSection("🧍 Datos Personales",
                            $"Nombre: {postulante.NombreCompleto}\n" +
                            $"Cédula: {postulante.Cedula}\n" +
                            $"Correo: {postulante.Correo}\n" +
                            $"Teléfono: {postulante.Telefono}\n" +
                            $"Dirección: {postulante.Direccion}");

                        AddSection("💼 Perfil Profesional", postulante.PerfilProfesional);
                        AddSection("🏢 Experiencia Laboral", postulante.ExperienciaLaboral);
                        AddSection("🎓 Formación Académica", postulante.FormacionAcademica);
                        AddSection("⚙️ Habilidades", postulante.Habilidades);
                        AddSection("🌍 Idiomas", postulante.Idiomas);
                        AddSection("📚 Formación Complementaria", postulante.FormacionComplementaria);
                        AddSection("⭐ Otros Datos", postulante.OtrosDatos);
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Municipalidad de Curridabat · Gestión de Plazas Vacantes © 2025")
                         .FontSize(10)
                         .FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"CV_{postulante.NombreCompleto.Replace(" ", "_")}.pdf");
        }

        /* TEMPORALMENTE DESHABILITADO - PROBLEMA DE COMPILACIÓN
        // 📄 Descargar comprobante en PDF
        public async Task<IActionResult> DescargarComprobante(int id)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var postulante = await _context.Postulantes
                .Include(p => p.PlazaVacante)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (postulante == null)
                return NotFound();

            var pdfBytes = QuestPDF.Fluent.Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(QuestPDF.Infrastructure.PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica"));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("COMPROBANTE DE POSTULACIÓN INTERNA")
                            .FontSize(18).Bold().FontColor(QuestPDF.Helpers.Colors.Orange.Medium);
                        col.Item().AlignCenter().Text("Municipalidad de Curridabat")
                            .FontSize(12).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                        col.Item().PaddingTop(5).LineHorizontal(2).LineColor(QuestPDF.Helpers.Colors.Orange.Medium);
                    });

                    // Content
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Fecha: {postulante.FechaActualizacion:dd/MM/yyyy HH:mm}").FontSize(10);
                        
                        col.Item().PaddingTop(10).Text("Datos del Postulante").Bold().FontSize(14).FontColor(QuestPDF.Helpers.Colors.Orange.Medium);
                        col.Item().Text($"Nombre: {postulante.NombreCompleto}");
                        col.Item().Text($"Cédula: {postulante.Cedula}");
                        col.Item().Text($"Correo: {postulante.Correo}");
                        col.Item().Text($"Teléfono: {postulante.Telefono}");

                        col.Item().PaddingTop(10).Text("Datos de la Plaza").Bold().FontSize(14).FontColor(QuestPDF.Helpers.Colors.Orange.Medium);
                        col.Item().Text($"Título: {postulante.PlazaVacante?.Titulo ?? "N/A"}");
                        col.Item().Text($"Departamento: {postulante.PlazaVacante?.Departamento ?? "N/A"}");
                        col.Item().Text($"Número de Concurso: {postulante.PlazaVacante?.NumeroConcurso ?? "N/A"}");

                        col.Item().PaddingTop(10).Text("Estado").Bold().FontSize(14).FontColor(QuestPDF.Helpers.Colors.Orange.Medium);
                        col.Item().Text($"Estado: {postulante.EstadoProceso}").FontColor(QuestPDF.Helpers.Colors.Green.Darken2);

                        col.Item().PaddingTop(20).BorderTop(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingTop(10)
                            .Text("Este documento sirve como comprobante de que su solicitud fue ingresada al sistema.")
                            .FontSize(9).Italic().FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Municipalidad de Curridabat · Gestión de Plazas Vacantes © 2025")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Comprobante_{postulante.Cedula}.pdf");
        }
        */

        private bool ValidarExtension(string fileName, string[] extensionesPermitidas)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return extensionesPermitidas.Contains(ext);
        }

        private string SanitizarNombre(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "archivo";
            return Path.GetFileName(fileName).Replace(" ", "_");
        }
    }
}
