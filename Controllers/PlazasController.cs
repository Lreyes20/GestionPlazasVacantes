using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionPlazasVacantes.Models;
using GestionPlazasVacantes.Data;

namespace GestionPlazasVacantes.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize] // Todos los usuarios autenticados pueden crear plazas
    public class PlazasController : Controller
    {
        private readonly AppDbContext _context;

        public PlazasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Plazas
        public async Task<IActionResult> Index()
        {
            // Usamos DateTime.Today para incluir las plazas que vencen hoy hasta el final del día
            var plazas = await _context.PlazasVacantes
                .Where(p => p.Activa && p.FechaLimite >= DateTime.Today && (p.EstadoFinal == "Abierta" || p.EstadoFinal == null))
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(plazas);
        }

        // GET: Plazas/Crear
        public IActionResult Crear()
        {
            return View();
        }

        //// POST: Plazas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PlazaVacante plaza)
        {
            if (ModelState.IsValid)
            {
                plaza.FechaCreacion = DateTime.Now;
                _context.Add(plaza);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = " Plaza vacante creada correctamente.";

                //  Nueva lógica de flujo:
                // Si la plaza es de concurso interno, redirige a la vista de plazas internas
                if (plaza.TipoConcurso == "Interno")
                {
                    // Esto manda al nuevo controlador PlazasInternas (que crearemos)
                    return RedirectToAction("Index", "PlazasInternas");
                }
                else
                {
                    // Si es externo, sigue el flujo normal al listado público
                    return RedirectToAction("Index", "Plazas");
                }
            }

            TempData["ErrorMessage"] = " Hubo un error al guardar la plaza.";
            return View(plaza);
        }


        // GET: Plazas/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var plaza = await _context.PlazasVacantes.FindAsync(id);
            if (plaza == null)
                return NotFound();

            return View(plaza);
        }

        // POST: Plazas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, PlazaVacante plaza)
        {
            if (id != plaza.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plaza);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "✏️ Plaza actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PlazasVacantes.Any(p => p.Id == plaza.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(plaza);
        }

        // GET: Plazas/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
                return NotFound();

            var plaza = await _context.PlazasVacantes.FindAsync(id);
            if (plaza == null)
                return NotFound();

            return View(plaza);
        }

        // POST: Plazas/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(id);
            if (plaza != null)
            {
                try
                {
                    _context.PlazasVacantes.Remove(plaza);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "🗑️ Plaza eliminada correctamente.";
                }
                catch (DbUpdateException)
                {
                    // Captura errores de integridad referencial (e.g. tiene postulantes)
                    TempData["ErrorMessage"] = "⚠️ No se puede eliminar la plaza porque tiene postulantes o registros asociados.";
                    // Opcional: Podrías redirigir a una página de error o simplemente volver al index con el mensaje
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "❌ Ocurrió un error inesperado al intentar eliminar la plaza.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
