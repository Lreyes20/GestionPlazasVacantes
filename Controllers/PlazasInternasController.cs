using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;

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
                .Where(p => p.TipoConcurso == "Interno" && p.FechaLimite > ahora && p.Activa)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(plazasInternas);
        }

        // 🧾 Mostrar formulario de aplicación
        public async Task<IActionResult> Aplicar(int id)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(id);
            if (plaza == null || plaza.TipoConcurso != "Interno") 
                return NotFound("Esta plaza no está disponible para postulación interna.");

            ViewBag.Plaza = plaza;
            return View(new Postulante { PlazaVacanteId = id });
        }

        // 💾 Guardar postulación
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
                model.EstadoProceso = "Recibido";
                model.FechaActualizacion = DateTime.Now;

                _context.Postulantes.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "✅ Su postulación ha sido recibida exitosamente.";
                return RedirectToAction("Confirmacion", new { id = model.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "⚠️ Ocurrió un error al procesar su postulación.");
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

        private bool ValidarExtension(string fileName, string[] extensionesPermitidas)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return extensionesPermitidas.Contains(ext);
        }

        private string SanitizarNombre(string fileName)
        {
            return Path.GetFileName(fileName).Replace(" ", "_");
        }
    }
}
