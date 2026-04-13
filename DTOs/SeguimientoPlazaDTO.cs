using GestionPlazasVacantes.Models;

public class SeguimientoPlazaDTO
{
    public PlazaVacante Plaza { get; set; }
    public List<Postulante> Postulantes { get; set; }
    public List<SeguimientoPostulante> Seguimientos { get; set; }
}