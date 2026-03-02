using GestionPlazasVacantes.Models;

public class PlazaDetalleDto
{
    public PlazaVacante Plaza { get; set; } = null!;
    public List<Postulante> Postulantes { get; set; } = new();
}