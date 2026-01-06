using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPlazasVacantes.Repositories
{
    /// <summary>
    /// Implementación del repositorio para PlazaVacante
    /// </summary>
    public class PlazaVacanteRepository : Repository<PlazaVacante>, IPlazaVacanteRepository
    {
        public PlazaVacanteRepository(AppDbContext context, ILogger<Repository<PlazaVacante>> logger) 
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<PlazaVacante>> GetPlazasActivasAsync()
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Where(p => p.Activa && (p.Estado == "Abierta" || p.Estado == null))
                    .OrderByDescending(p => p.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener plazas activas");
                throw;
            }
        }

        public async Task<IEnumerable<PlazaVacante>> GetPlazasPorDepartamentoAsync(string departamento)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Where(p => p.Departamento == departamento && p.Activa)
                    .OrderByDescending(p => p.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener plazas por departamento: {Departamento}", departamento);
                throw;
            }
        }

        public async Task<PlazaVacante?> GetPlazaConPostulantesAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(p => p.Postulantes)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener plaza con postulantes: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PlazaVacante>> GetPlazasExternasAbiertasAsync()
        {
            try
            {
                var ahora = DateTime.Now;
                return await _dbSet
                    .AsNoTracking()
                    .Where(p => p.TipoConcurso == "Externo" 
                             && p.FechaLimite >= DateTime.Today 
                             && p.Activa)
                    .OrderByDescending(p => p.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener plazas externas abiertas");
                throw;
            }
        }
    }
}
