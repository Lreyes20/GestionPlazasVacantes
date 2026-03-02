using GestionPlazasVacantes.DTOs;

namespace GestionPlazasVacantes.Models
{
    public class Postulacion
    {
        public PostulanteDto Postulante { get; set; } = new();
        public PlazaDto Plaza { get; set; } = new();
    }
}
