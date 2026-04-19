using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Models;

public class DetallePostulanteDTO
{
    public PostulanteDto Postulante { get; set; }
    public SeguimientoDto Seguimiento { get; set; }
    public List<ArchivoDto> Archivos { get; set; } = new(); // 🔥 importante
}