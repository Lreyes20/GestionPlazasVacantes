using GestionPlazasVacantes.Models;

namespace GestionPlazasVacantes.Repositories
{
    /// <summary>
    /// Repositorio específico para PlazaVacante con operaciones personalizadas
    /// </summary>
    public interface IPlazaVacanteRepository : IRepository<PlazaVacante>
    {
        Task<IEnumerable<PlazaVacante>> GetPlazasActivasAsync();
        Task<IEnumerable<PlazaVacante>> GetPlazasPorDepartamentoAsync(string departamento);
        Task<PlazaVacante?> GetPlazaConPostulantesAsync(int id);
        Task<IEnumerable<PlazaVacante>> GetPlazasExternasAbiertasAsync();
    }
}
