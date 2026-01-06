using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPlazasVacantes.Repositories
{
    /// <summary>
    /// Implementación del repositorio para Postulante
    /// </summary>
    public class PostulanteRepository : Repository<Postulante>, IPostulanteRepository
    {
        public PostulanteRepository(AppDbContext context, ILogger<Repository<Postulante>> logger) 
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<Postulante>> GetPostulantesPorPlazaAsync(int plazaId)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Where(p => p.PlazaVacanteId == plazaId)
                    .OrderByDescending(p => p.FechaActualizacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener postulantes por plaza: {PlazaId}", plazaId);
                throw;
            }
        }

        public async Task<Postulante?> GetPostulanteConDetallesAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(p => p.PlazaVacante)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener postulante con detalles: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistePostulacionAsync(string cedula, int plazaId)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .AnyAsync(p => p.Cedula == cedula && p.PlazaVacanteId == plazaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de postulación: {Cedula}, {PlazaId}", cedula, plazaId);
                throw;
            }
        }

        public async Task<IEnumerable<Postulante>> GetPostulantesPorEstadoAsync(string estado)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Where(p => p.EstadoProceso == estado)
                    .OrderByDescending(p => p.FechaActualizacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener postulantes por estado: {Estado}", estado);
                throw;
            }
        }

        public async Task<int> ContarPostulantesPorPlazaAsync(int plazaId)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .CountAsync(p => p.PlazaVacanteId == plazaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar postulantes por plaza: {PlazaId}", plazaId);
                throw;
            }
        }
    }
}
