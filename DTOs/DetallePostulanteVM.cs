using GestionPlazasVacantes.DTOs;

namespace GestionPlazasVacantes.DTOs
{
    public class DetallePostulanteVM
    {
        public PostulanteDto Postulante { get; set; }

        public SeguimientoDto Seguimiento { get; set; }

        public List<ArchivoDto> Archivos { get; set; } = new();
    }
}