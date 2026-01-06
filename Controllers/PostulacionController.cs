using System;
using System.IO;
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
    public class PostulacionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private static readonly string[] ExtImgs = new[] { ".jpg", ".jpeg", ".png" };
        private static readonly string[] ExtPdf = new[] { ".pdf" };
        private static readonly string[] ExtWord = new[] { ".doc", ".docx" };

        public PostulacionController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 🏢 Vista principal de plazas
        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.Now;
            var plazasExternas = await _context.PlazasVacantes
                .Where(p => p.TipoConcurso == "Externo" && p.FechaLimite >= DateTime.Today && p.Activa)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(plazasExternas);
        }

        // 🧾 Mostrar formulario
        public async Task<IActionResult> Aplicar(int id)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(id);
            if (plaza == null) return NotFound();

            ViewBag.Plaza = plaza;
            return View(new Postulante { PlazaVacanteId = id });
        }

        // 💾 Guardar postulación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aplicar(
    Postulante postulante,
    IFormFile? archivoCV,
    IFormFile? FotoTitulo,
    IFormFile? FotoColegiatura,
    IFormFile? FotoLicencia,
    IFormFile? FotoPermisoArmas,
    List<IFormFile>? ArchivoTitulos  // múltiples
)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(postulante.PlazaVacanteId);
            if (plaza == null)
            {
                TempData["ErrorMessage"] = "❌ La plaza seleccionada no existe o fue eliminada.";
                return RedirectToAction("Index");
            }

            ViewBag.Plaza = plaza;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "⚠️ Algunos campos son inválidos o faltan datos. Revísalos antes de enviar.";
                return View(postulante);
            }

            var existe = await _context.Postulantes
                .AnyAsync(p => p.PlazaVacanteId == postulante.PlazaVacanteId && p.Cedula == postulante.Cedula);

            if (existe)
            {
                TempData["ErrorMessage"] = $"⚠️ Ya existe una postulación con esta cédula para la plaza '{plaza.Titulo}'.";
                return View(postulante);
            }

            try
            {
                // CV → /curriculums (solo PDF)
                postulante.CurriculumPath = await GuardarArchivo(archivoCV, "curriculums", ExtPdf);
                // Fotos/Docs → /uploads_postulantes
                postulante.FotoTituloPath = await GuardarArchivo(FotoTitulo, "uploads_postulantes", ExtImgs.Concat(ExtPdf).ToArray());
                postulante.FotoColegiaturaPath = await GuardarArchivo(FotoColegiatura, "uploads_postulantes", ExtImgs.Concat(ExtPdf).ToArray());
                postulante.FotoLicenciaPath = await GuardarArchivo(FotoLicencia, "uploads_postulantes", ExtImgs.Concat(ExtPdf).ToArray());
                postulante.FotoPermisoArmasPath = await GuardarArchivo(FotoPermisoArmas, "uploads_postulantes", ExtImgs.Concat(ExtPdf).ToArray());

                // Títulos múltiples → guarda y concatena rutas con coma
                if (ArchivoTitulos != null && ArchivoTitulos.Count > 0)
                {
                    var rutas = new List<string>();
                    foreach (var f in ArchivoTitulos)
                    {
                        var r = await GuardarArchivo(f, "uploads_postulantes", ExtImgs.Concat(ExtPdf).ToArray());
                        if (!string.IsNullOrEmpty(r)) rutas.Add(r);
                    }
                    postulante.ArchivoTitulosPath = string.Join(",", rutas);
                }

                postulante.FechaActualizacion = DateTime.Now;
                postulante.EstadoProceso = "En revisión";

                // Siempre crear como nuevo: nunca respetar un Id que venga del formulario
                postulante.Id = 0;

                _context.Postulantes.Add(postulante);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Postulación enviada con éxito.";
                return RedirectToAction("Confirmacion", new { id = postulante.Id });
            }
           
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al guardar postulación: " + ex);

                var inner = ex.InnerException?.Message ?? ex.Message;
                TempData["ErrorMessage"] = $"❌ Error al guardar la postulación: {inner}";

                return View(postulante);
            }

        }


        // ✅ Confirmación de postulación
        public async Task<IActionResult> Confirmacion(int id)
        {
            var postulante = await _context.Postulantes
                .Include(p => p.PlazaVacante)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (postulante == null)
            {
                TempData["ErrorMessage"] = "⚠️ No se encontró la postulación.";
                return RedirectToAction("Index");
            }

            return View(postulante);
        }

        // 🧾 Generar CV PDF antes de guardar (versión sin ID)
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

        // 📄 Descargar comprobante en PDF
        public async Task<IActionResult> DescargarComprobante(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var postulante = await _context.Postulantes
                .Include(p => p.PlazaVacante)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (postulante == null)
                return NotFound();

            var pdfBytes = Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica"));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("COMPROBANTE DE POSTULACIÓN")
                            .FontSize(18).Bold().FontColor(Colors.Orange.Medium);
                        col.Item().AlignCenter().Text("Municipalidad de Curridabat")
                            .FontSize(12).FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Orange.Medium);
                    });

                    // Content
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Fecha: {postulante.FechaActualizacion:dd/MM/yyyy HH:mm}").FontSize(10);
                        
                        col.Item().PaddingTop(10).Text("Datos del Postulante").Bold().FontSize(14).FontColor(Colors.Orange.Medium);
                        col.Item().Text($"Nombre: {postulante.NombreCompleto}");
                        col.Item().Text($"Cédula: {postulante.Cedula}");
                        col.Item().Text($"Correo: {postulante.Correo}");
                        col.Item().Text($"Teléfono: {postulante.Telefono}");

                        col.Item().PaddingTop(10).Text("Datos de la Plaza").Bold().FontSize(14).FontColor(Colors.Orange.Medium);
                        col.Item().Text($"Título: {postulante.PlazaVacante?.Titulo ?? "N/A"}");
                        col.Item().Text($"Departamento: {postulante.PlazaVacante?.Departamento ?? "N/A"}");
                        col.Item().Text($"Número de Concurso: {postulante.PlazaVacante?.NumeroConcurso ?? "N/A"}");

                        col.Item().PaddingTop(10).Text("Estado").Bold().FontSize(14).FontColor(Colors.Orange.Medium);
                        col.Item().Text($"Estado: {postulante.EstadoProceso}").FontColor(Colors.Green.Darken2);

                        col.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10)
                            .Text("Este documento sirve como comprobante de que su solicitud fue ingresada al sistema.")
                            .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Municipalidad de Curridabat · Gestión de Plazas Vacantes © 2025")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Comprobante_{postulante.Cedula}.pdf");
        }
       

        private async Task<string?> GuardarArchivo(
            IFormFile? file,
            string subfolder,
            string[] extensionesPermitidas,
            long maxBytes = 10 * 1024 * 1024)  // 10 MB
        {
            if (file == null || file.Length == 0) return null;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!extensionesPermitidas.Contains(ext) && !ExtPdf.Contains(ext))
                throw new InvalidOperationException($"Extensión no permitida: {ext}");

            if (file.Length > maxBytes)
                throw new InvalidOperationException("El archivo supera el tamaño máximo permitido (10 MB).");

            // Asegura carpeta
            var destinoFisico = Path.Combine(_env.WebRootPath, subfolder.TrimStart('/', '\\'));
            if (!Directory.Exists(destinoFisico))
                Directory.CreateDirectory(destinoFisico);

            var nombre = $"{Guid.NewGuid():N}{ext}";
            var rutaFisica = Path.Combine(destinoFisico, nombre);
            using (var fs = new FileStream(rutaFisica, FileMode.Create))
                await file.CopyToAsync(fs);

            // Ruta pública para el navegador
            return $"/{subfolder.Trim('/', '\\')}/{nombre}";
        }

        private static bool EsImagen(string? ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta)) return false;
            var ext = Path.GetExtension(ruta).ToLowerInvariant();
            return ExtImgs.Contains(ext);
        }

    }
}
