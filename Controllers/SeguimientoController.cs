using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionPlazasVacantes.Controllers
{
    public class SeguimientoController : Controller
    {
        private readonly AppDbContext _context;

        public SeguimientoController(AppDbContext context)
        {
            _context = context;
        }

        // 📋 Vista general de plazas activas con seguimiento
        public async Task<IActionResult> Index()
        {
            // Obtener usuario actual
            var username = User.Identity?.Name;
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username);

            if (usuarioActual == null)
                return Unauthorized();

            IQueryable<PlazaVacante> query = _context.PlazasVacantes
                .Include(p => p.Postulantes)
                .Include(p => p.UsuarioAsignado)
                .Where(p => p.Activa == true); // Solo plazas activas

            // Si es Colaborador, solo ver plazas asignadas a él
            if (usuarioActual.Rol == RolUsuario.Colaborador)
            {
                query = query.Where(p => p.UsuarioAsignadoId == usuarioActual.Id);
            }
            // Si es Jefe, ve todas las plazas activas

            var plazasConPostulantes = await query
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            ViewBag.UsuarioActual = usuarioActual;
            return View(plazasConPostulantes);
        }


        // 👀 Seguimiento por plaza
        public async Task<IActionResult> PorPlaza(int plazaId)
        {
            var plaza = await _context.PlazasVacantes.FirstOrDefaultAsync(p => p.Id == plazaId);
            if (plaza == null) return NotFound();

            // 🔹 Solo seguimientos ACTIVOS (no descartados)
            var seguimientos = await _context.SeguimientosPostulantes
                .Where(s => s.PlazaVacanteId == plazaId && s.Activo)
                .ToListAsync();

            // Obtener IDs de postulantes activos
            var postulanteIds = seguimientos.Select(s => s.PostulanteId).ToList();

            // Solo mostrar postulantes que tienen seguimiento activo
            var postulantes = await _context.Postulantes
                .Include(p => p.PlazaVacante)
                .Where(p => p.PlazaVacanteId == plazaId && postulanteIds.Contains(p.Id))
                .ToListAsync();

            ViewBag.Plaza = plaza;
            ViewBag.Seguimientos = seguimientos;

            return View(postulantes);
        }

        // 🧾 Detalle individual de un postulante
        public async Task<IActionResult> Detalle(int id)
        {
            var postulante = await _context.Postulantes
                .Include(p => p.PlazaVacante)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (postulante == null) return NotFound();

            var seguimiento = await _context.SeguimientosPostulantes
                .FirstOrDefaultAsync(s => s.PostulanteId == id);

            // 🔹 Si no existe, se crea automáticamente
            if (seguimiento == null)
            {
                seguimiento = new SeguimientoPostulante
                {
                    PostulanteId = id,
                    PlazaVacanteId = postulante.PlazaVacanteId,
                    EtapaActual = "Revisión documental",
                    CumpleRequisitos = true,
                    Activo = true
                };

                _context.SeguimientosPostulantes.Add(seguimiento);
                await _context.SaveChangesAsync();
            }

            ViewBag.Seguimiento = seguimiento;
            return View(postulante);
        }

        // 💾 Actualizar etapa
        [HttpPost]
        public async Task<IActionResult> Actualizar(int postulanteId, string etapa, bool cumple, decimal? notaTec, decimal? notaPsi, string obs)
        {
            var s = await _context.SeguimientosPostulantes
                .FirstOrDefaultAsync(x => x.PostulanteId == postulanteId);
            if (s == null) return NotFound();

            s.EtapaActual = etapa;
            s.CumpleRequisitos = cumple;
            s.NotaPruebaTecnica = notaTec;
            s.NotaPsicometrica = notaPsi;
            s.Observaciones = obs;
            s.FechaActualizacion = DateTime.Now;

            // Reglas automáticas
            if (etapa == "Pruebas técnicas" && notaTec < 70)
                s.CumpleRequisitos = false;

            if (etapa == "Final")
                s.Aprobado = true;

            await _context.SaveChangesAsync();
            return RedirectToAction("Detalle", new { id = postulanteId });
        }

        // 🔹 DESCARTAR POSTULANTE
        [HttpPost]
        public async Task<IActionResult> Descartar(int postulanteId, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Error"] = "⚠️ Debe especificar un motivo para descartar al postulante.";
                return RedirectToAction("Detalle", new { id = postulanteId });
            }

            var seguimiento = await _context.SeguimientosPostulantes
                .FirstOrDefaultAsync(s => s.PostulanteId == postulanteId);

            var postulante = await _context.Postulantes
                .Include(p => p.PlazaVacante)
                .FirstOrDefaultAsync(p => p.Id == postulanteId);

            if (seguimiento == null || postulante == null)
                return NotFound();

            // 🔸 Marcar seguimiento como descartado
            seguimiento.CumpleRequisitos = false;
            seguimiento.Aprobado = false;
            seguimiento.MotivoDescarte = motivo.Trim();
            seguimiento.Activo = false;
            seguimiento.EtapaActual = "Descartado";
            seguimiento.FechaActualizacion = DateTime.Now;

            // 🔸 Actualizar el postulante
            postulante.EstadoProceso = "Descartado";
            postulante.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            // 🔸 Si ya no quedan postulantes activos, marcamos la plaza como cerrada
            bool hayActivos = await _context.SeguimientosPostulantes
                .AnyAsync(s => s.PlazaVacanteId == postulante.PlazaVacanteId && s.Activo);

            if (!hayActivos)
            {
                var plaza = postulante.PlazaVacante;
                if (plaza != null)
                {
                    plaza.EstadoFinal = "Cerrada";
                    plaza.FechaLimite = DateTime.Now;
                    _context.PlazasVacantes.Update(plaza);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "✅ El postulante fue descartado correctamente.";
            return RedirectToAction("PorPlaza", new { plazaId = postulante.PlazaVacanteId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarPlaza(int plazaId)
        {
            // ✅ Busca la plaza
            var plaza = await _context.PlazasVacantes
                .Include(p => p.Postulantes)
                .FirstOrDefaultAsync(p => p.Id == plazaId);

            if (plaza == null)
            {
                TempData["ErrorMessage"] = "⚠️ No se encontró la plaza especificada.";
                // Redirige al listado de plazas cerradas, no al dashboard
                return RedirectToAction("Index");
            }

            // ✅ Cambia su estado
            plaza.EstadoFinal = "Finalizada";
            plaza.FechaLimite = DateTime.Now;
            plaza.Activa = false; // si existe esta propiedad, marca la plaza como inactiva

            // ✅ Marca los postulantes activos como cerrados también
            var postulantes = await _context.Postulantes
                .Where(p => p.PlazaVacanteId == plazaId)
                .ToListAsync();

            foreach (var post in postulantes)
            {
                if (post.EstadoProceso != "Contratado")
                {
                    post.EstadoProceso = "Cierre de plaza";
                    post.FechaActualizacion = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ La plaza fue finalizada correctamente.";
            return RedirectToAction("Index"); // 👈 Te redirige al listado de plazas cerradas
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPlaza(int plazaId)
        {
            var plaza = await _context.PlazasVacantes.FindAsync(plazaId);
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
                     TempData["ErrorMessage"] = "⚠️ No se puede eliminar la plaza porque tiene postulantes o actividad asociada. Considere solo cerrarla.";
                }
                catch (Exception)
                {
                     TempData["ErrorMessage"] = "❌ Error inesperado al eliminar la plaza.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ La plaza no existe o ya fue eliminada.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
