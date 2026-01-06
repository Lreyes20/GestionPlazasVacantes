using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionPlazasVacantes.Models;
using GestionPlazasVacantes.Data;
using System.Linq;

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
            // Mostrar TODAS las plazas activas (no cerradas) para permitir edición
            // Cuando una plaza se cierra (Activa = false), desaparece del listado
            var plazas = await _context.PlazasVacantes
                .AsNoTracking() // Optimización: No trackear cambios para consultas de solo lectura
                .Where(p => p.Activa) // Solo plazas activas (no cerradas manualmente)
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
                try
                {
                    plaza.Id = 0; // Asegurar que Entity Framework genere el ID automáticamente
                    plaza.FechaCreacion = DateTime.Now;
                    plaza.Activa = true;
                    plaza.Estado = "Abierta";
                    plaza.EstadoFinal = "Abierta"; // Para que aparezca en el filtro del Index
                    _context.Add(plaza);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ Plaza vacante creada correctamente.";
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (Exception ex)
                {
                    var errorMsg = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMsg += $" | Inner: {ex.InnerException.Message}";
                    }
                    Console.WriteLine($"❌ Error al crear plaza: {errorMsg}");
                    TempData["ErrorMessage"] = $"❌ Error al guardar la plaza: {errorMsg}";
                    return View(plaza);
                }
            }
            
            // Mostrar errores de validación
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            
            TempData["ErrorMessage"] = $"⚠️ Errores de validación: {errors}";
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
