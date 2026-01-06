using GestionPlazasVacantes.Models;

namespace GestionPlazasVacantes.Repositories
{
    /// <summary>
    /// Repositorio específico para Postulante con operaciones personalizadas
    /// </summary>
    public interface IPostulanteRepository : IRepository<Postulante>
    {
        Task<IEnumerable<Postulante>> GetPostulantesPorPlazaAsync(int plazaId);
        Task<Postulante?> GetPostulanteConDetallesAsync(int id);
        Task<bool> ExistePostulacionAsync(string cedula, int plazaId);
        Task<IEnumerable<Postulante>> GetPostulantesPorEstadoAsync(string estado);
        Task<int> ContarPostulantesPorPlazaAsync(int plazaId);
    }
}
