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
    public class PlazasInternasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PlazasInternasController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: PlazasInternas
        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.Now;

            var internasVigentes = await _context.PlazasVacantes
                .Where(p => p.TipoConcurso == "Interno" && p.FechaLimite > ahora)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(internasVigentes);
        }

        // 🧾 Formulario de aplicación
        public async Task<IActionResult> Aplicar(int id)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(id);
            if (plaza == null) return NotFound();

            ViewBag.Plaza = plaza;
            return View(new Postulante { PlazaVacanteId = id });
        }

        // 💾 Guardar postulación completa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aplicar(Postulante postulante, IFormFile? archivoCV)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(postulante.PlazaVacanteId);
            if (plaza == null) return NotFound();

            bool duplicado = await _context.Postulantes.AnyAsync(p =>
                p.PlazaVacanteId == postulante.PlazaVacanteId && p.Cedula == postulante.Cedula);

            if (duplicado)
            {
                TempData["ErrorMessage"] = "Ya te has postulado a esta plaza.";
                return RedirectToAction(nameof(Aplicar), new { id = postulante.PlazaVacanteId });
            }

            if (archivoCV != null && archivoCV.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath ?? "", "curriculums");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(archivoCV.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await archivoCV.CopyToAsync(stream);

                postulante.CurriculumPath = $"/curriculums/{fileName}";
            }

            postulante.FechaActualizacion = DateTime.Now;
            postulante.EstadoProceso = "En revisión";
            postulante.Id = 0;

            _context.Postulantes.Add(postulante);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Postulación enviada con éxito.";
            TempData["CVId"] = postulante.Id;

            return RedirectToAction(nameof(Confirmacion));
        }

        // 🧾 Generar CV en PDF (QuestPDF)
        [HttpPost]
        public IActionResult GenerarCV(Postulante postulante)
        {
            var pdfBytes = Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
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

        // 🎉 Confirmación
        public IActionResult Confirmacion()
        {
            return View();
        }
    }
}
