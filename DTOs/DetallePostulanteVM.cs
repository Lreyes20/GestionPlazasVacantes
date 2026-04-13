using GestionPlazasVacantes.Models;

namespace GestionPlazasVacantes.DTOs
{
    public class DetallePostulanteVM
    {
        public Postulante Postulante { get; set; }
        public SeguimientoPostulante Seguimiento { get; set; }
    }
}
