using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionPlazasVacantes.Controllers
{
    public class PostulantesController : Controller
    {
        private readonly AppDbContext _context;

        // Estados válidos del proceso (simple y claro)
        private static readonly string[] EstadosProceso = new[]
        {
            "En revisión", "Preseleccionado", "Entrevista", "Finalista", "Rechazado", "Contratado"
        };

        public PostulantesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Postulantes/PorPlaza/5
        public async Task<IActionResult> PorPlaza(int id)
        {
            var plaza = await _context.PlazasVacantes
                .Include(p => p.Postulantes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plaza == null) return NotFound();

            ViewBag.Estados = EstadosProceso;
            return View(plaza);
        }

        // GET: /Postulantes/Crear?plazaId=5
        public async Task<IActionResult> Crear(int plazaId)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(plazaId);
            if (plaza == null) return NotFound();

            ViewBag.Plaza = plaza;
            ViewBag.Estados = EstadosProceso;
            return View(new Postulante { PlazaVacanteId = plazaId });
        }

        // POST: /Postulantes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Postulante postulante)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Plaza = await _context.PlazasVacantes.FindAsync(postulante.PlazaVacanteId);
                ViewBag.Estados = EstadosProceso;
                return View(postulante);
            }

            postulante.FechaActualizacion = DateTime.Now;
            _context.Postulantes.Add(postulante);

            // Marcar la plaza "En Proceso" si estaba "Abierta"
            var plaza = await _context.PlazasVacantes.FindAsync(postulante.PlazaVacanteId);
            if (plaza != null && plaza.Estado == "Abierta")
                plaza.Estado = "En Proceso";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Postulante agregado.";
            return RedirectToAction(nameof(PorPlaza), new { id = postulante.PlazaVacanteId });
        }

        // POST: /Postulantes/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string estado)
        {
            var postulante = await _context.Postulantes.FindAsync(id);
            if (postulante == null) return NotFound();

            if (!EstadosProceso.Contains(estado))
            {
                TempData["ErrorMessage"] = "Estado inválido.";
                return RedirectToAction(nameof(PorPlaza), new { id = postulante.PlazaVacanteId });
            }

            postulante.EstadoProceso = estado;
            postulante.FechaActualizacion = DateTime.Now;

            // Si alguien quedó contratado, la plaza pasa a "Cerrada"
            if (estado == "Contratado")
            {
                var plaza = await _context.PlazasVacantes.FindAsync(postulante.PlazaVacanteId);
                if (plaza != null)
                {
                    plaza.Estado = "Cerrada";
                    plaza.FechaCierre = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "✏️ Estado actualizado.";
            return RedirectToAction(nameof(PorPlaza), new { id = postulante.PlazaVacanteId });
        }

        // POST: /Postulantes/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var postulante = await _context.Postulantes.FindAsync(id);
            if (postulante == null) return NotFound();

            var plazaId = postulante.PlazaVacanteId;
            _context.Postulantes.Remove(postulante);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🗑️ Postulante eliminado.";
            return RedirectToAction(nameof(PorPlaza), new { id = plazaId });
        }
    }
}
