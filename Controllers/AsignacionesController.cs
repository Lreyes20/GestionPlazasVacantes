using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionPlazasVacantes.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Jefe")]
    public class AsignacionesController : Controller
    {
        private readonly AppDbContext _context;

        public AsignacionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Asignaciones
        // Vista principal para gestionar asignaciones de plazas a colaboradores
        public async Task<IActionResult> Index()
        {
            // Verificar que el usuario actual es Jefe
            var username = User.Identity?.Name;
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username);

            if (usuarioActual == null || usuarioActual.Rol != RolUsuario.Jefe)
            {
                TempData["ErrorMessage"] = "⚠️ No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Obtener todas las plazas activas con sus asignaciones
            var plazasActivas = await _context.PlazasVacantes
                .Include(p => p.UsuarioAsignado)
                .Include(p => p.Postulantes)
                .Where(p => p.Activa == true)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            // Obtener todos los colaboradores activos
            var colaboradores = await _context.Usuarios
                .Where(u => u.Activo == true && u.Rol == RolUsuario.Colaborador)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Colaboradores = colaboradores;
            ViewBag.UsuarioActual = usuarioActual;

            return View(plazasActivas);
        }

        // POST: Asignaciones/AsignarPlaza
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPlaza(int plazaId, int usuarioId)
        {
            // Verificar que el usuario actual es Jefe
            var username = User.Identity?.Name;
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username);

            if (usuarioActual == null || usuarioActual.Rol != RolUsuario.Jefe)
            {
                TempData["ErrorMessage"] = "⚠️ No tienes permisos para realizar esta acción.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Buscar la plaza
            var plaza = await _context.PlazasVacantes.FindAsync(plazaId);
            if (plaza == null)
            {
                TempData["ErrorMessage"] = "⚠️ No se encontró la plaza especificada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que la plaza esté activa
            if (!plaza.Activa)
            {
                TempData["ErrorMessage"] = "⚠️ No se puede asignar una plaza inactiva.";
                return RedirectToAction(nameof(Index));
            }

            // Buscar el colaborador
            var colaborador = await _context.Usuarios.FindAsync(usuarioId);
            if (colaborador == null || colaborador.Rol != RolUsuario.Colaborador)
            {
                TempData["ErrorMessage"] = "⚠️ El usuario seleccionado no es válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!colaborador.Activo)
            {
                TempData["ErrorMessage"] = "⚠️ No se puede asignar a un usuario inactivo.";
                return RedirectToAction(nameof(Index));
            }

            // Asignar la plaza al colaborador
            plaza.UsuarioAsignadoId = usuarioId;
            _context.PlazasVacantes.Update(plaza);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Plaza '{plaza.Titulo}' asignada a {colaborador.FullName} correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Asignaciones/RemoverAsignacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverAsignacion(int plazaId)
        {
            // Verificar que el usuario actual es Jefe
            var username = User.Identity?.Name;
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username);

            if (usuarioActual == null || usuarioActual.Rol != RolUsuario.Jefe)
            {
                TempData["ErrorMessage"] = "⚠️ No tienes permisos para realizar esta acción.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Buscar la plaza
            var plaza = await _context.PlazasVacantes.FindAsync(plazaId);
            if (plaza == null)
            {
                TempData["ErrorMessage"] = "⚠️ No se encontró la plaza especificada.";
                return RedirectToAction(nameof(Index));
            }

            // Remover la asignación
            plaza.UsuarioAsignadoId = null;
            _context.PlazasVacantes.Update(plaza);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Asignación removida de la plaza '{plaza.Titulo}' correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
